using ACBALibrary;
using ExternalBanking;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Configuration;
using infsec = ExternalBankingService.InfSecServiceReference;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "XBSForAPS" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select XBSForAPS.svc or XBSForAPS.svc.cs at the Solution Explorer and start debugging.
    public class XBAutomatedProcessesService : IXBAutomatedProcessesService
    {
        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ClientIp { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }
        Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Ավտորիզացված հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }
        /// <summary>
        ///  Լեզու
        /// </summary>
        byte Language { get; set; }

        public ActionResult GenerateAndMakeSwiftMessagesByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                if (User != null)
                {
                    return SwiftMessage.GenerateAndMakeSwiftMessagesByPeriodicTransfer(statementDate, dateFrom, dateTo, User.userID);
                }
                else
                {
                    ActionResult result = new ActionResult();
                    result.ResultCode = ResultCode.NotAutorized;
                    return result;
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public infsec.AuthorizedUser AuthorizeUserBySessionToken(string sessionToken)
        {
            try
            {
                return AuthorizationService.AuthorizeUserBySessionToken(sessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ACBALibrary.User InitUser(infsec.AuthorizedUser authUser)
        {
            try
            {
                return AuthorizationService.InitUser(authUser);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public void WriteLog(Exception ex)
        {
            GlobalDiagnosticsContext.Set("ClientIp", ClientIp);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingService-Test");
            }

            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());

            if (Source == SourceType.Bank || Source == SourceType.PhoneBanking)
                GlobalDiagnosticsContext.Set("UserName", User.userName);
            else
            {
                GlobalDiagnosticsContext.Set("UserName", "");
            }

            if (ClientIp != null)
                GlobalDiagnosticsContext.Set("ClientIp", ClientIp);
            else
                GlobalDiagnosticsContext.Set("ClientIp", "");

            string message = (ex.Message != null ? ex.Message : " ") +
                Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "")
               ;

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }

        public infsec.UserAccessForCustomer GetUserAccessForCustomer(string userSessionToken, string customerSessionToken)
        {
            try
            {
                return AuthorizationService.GetUserAccessForCustomer(userSessionToken, customerSessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public void Init(string clientIp, ExternalBanking.ACBAServiceReference.User user)
        {
            try
            {

                User = user;
                ClientIp = clientIp;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<ActionResult> SaveAndApproveClassifiedLoanActionOrders(SearchClassifiedLoan searchParameters)
        {
            ActionResult result = new ActionResult();


            int RowCount;
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, SourceType.Bank);
                List<ActionResult> approveResult = new List<ActionResult>();

                // search loans 
                List<ClassifiedLoan> list = service.GetClassifiedLoans(searchParameters, out RowCount);

                if (RowCount > 0)
                {
                    ClassifiedLoanActionOrders orders = new ClassifiedLoanActionOrders();

                    orders.ClassifiedLoanActionDetails = new List<PreOrderDetails>();

                    orders.RegistrationDate = DateTime.Now.Date;
                    orders.OperationFilialCode = User.filialCode;

                    for (int i = 0; i < list.Count; i++)
                    {
                        PreOrderDetails loan = new PreOrderDetails();
                        loan.CustomerNumber = list[i].CustomerNumber;
                        loan.AppID = ulong.Parse(list[i].ProductId.ToString());
                        loan.QualityDescription = list[i].QualityDescription;
                        loan.ProductType = list[i].ProductType;

                        orders.ClassifiedLoanActionDetails.Add(loan);
                    }

                    if (searchParameters.ListType == ClassifiedLoanListType.NotOutLoansList)
                    {
                        orders.PreOrderType = PreOrderType.ClassifiedLoanMakeOutOrdersCreation;
                        orders.ActionType = ClassifiedLoanActionType.MakeOut;
                    }
                    else if (searchParameters.ListType == ClassifiedLoanListType.WrongClassifiedLoansList)
                    {
                        orders.PreOrderType = PreOrderType.ClassifiedLoanRemoveClassificationOrdersCreation;
                        orders.ActionType = ClassifiedLoanActionType.RemoveClassification;
                    }

                    //Save Automatic HB Documents PreOrders
                    result = service.SaveAndApproveClassifiedLoanActionOrders(orders);

                    if (result.Errors.Count == 0)
                    {
                        // Generate Orders 
                        switch (orders.PreOrderType)
                        {
                            case PreOrderType.ClassifiedLoanRemoveClassificationOrdersCreation:
                                approveResult = service.SaveAndApproveAutomaticGenaratedPreOrdersClassificationRemove(result.Id).Result;
                                break;
                            case PreOrderType.ClassifiedLoanMakeOutOrdersCreation:
                                approveResult = service.SaveAndApproveAutomaticGenaratedPreOrdersMakeLoanOut(result.Id).Result;
                                break;
                        }
                    }
                    else
                    {
                        string errMessage = "";
                        for (int i = 0; i < result.Errors.Count; i++)
                        {
                            errMessage += result.Errors[i].Description + "\n";
                        }
                        throw new Exception(errMessage);
                    }



                }

                return approveResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
    }
}
