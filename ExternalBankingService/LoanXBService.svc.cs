using ExternalBanking;
using ExternalBankingService.Interfaces;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Configuration;
using infsec = ExternalBankingService.InfSecServiceReference;


namespace ExternalBankingService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LoanXBService : ILoanXBService
    {

        #region PrivateRegion
        Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// Օգտագործողի IP
        /// </summary>
        string ClientIp { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }

        byte Language { get; set; }


        /// <summary>
        /// Հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }


        private void WriteLog(Exception ex)
        {
            GlobalDiagnosticsContext.Set("ClientIp", ClientIp);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "LoanXBService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "LoanXBService-Test");
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
        #endregion


        private void CheckCustomerAuthorization(string sessionID)
        {

            infsec.CustomertData customerData = null;

            AuthorizationService.Use(client =>
            {
                customerData = client.CheckCustomerAuthorization(sessionID);
            });

            //customerData.IsIdentified
            if (customerData != null)
            {
                AuthorizedCustomer = new AuthorizedCustomer();
                AuthorizedCustomer.ApprovementScheme = 1;
                AuthorizedCustomer.BranchCode = customerData.BranchCode;
                AuthorizedCustomer.CustomerNumber = customerData.CustomerNumber;
                AuthorizedCustomer.DailyTransactionsLimit = customerData.DailyTransactionsLimit;
                AuthorizedCustomer.FullName = customerData.FullName;
                AuthorizedCustomer.OneTransactionLimit = customerData.OneTransactionLimit;
                AuthorizedCustomer.Permission = 0;
                AuthorizedCustomer.SecondConfirm = 0;
                AuthorizedCustomer.SessionID = customerData.IsIdentified ? customerData.SessionID : Guid.Empty.ToString();
                AuthorizedCustomer.TypeOfClient = 0;
                AuthorizedCustomer.UserName = customerData.CustomerNumber.ToString();
                AuthorizedCustomer.OneTransactionLimitToOwnAccount = customerData.LimitOfOneTransactionOwnAccount;
                AuthorizedCustomer.OneTransactionLimitToAnothersAccount = customerData.LimitOfOneTransactionOtherAccount;
                AuthorizedCustomer.DayLimitToOwnAccount = customerData.LimitOfDayOwnAccount;
                AuthorizedCustomer.DayLimitToAnothersAccount = customerData.LimitOfDayOtherAccount;
            }
        }

        public AuthorizedCustomer AuthorizeCustomer(ulong customerNumber)
        {
            AuthorizedCustomer result = new AuthorizedCustomer();
            Customer customer = new Customer();
            try
            {
                infsec.CustomertData customerData = null;

                AuthorizationService.Use(client =>
                {
                    customerData = client.AuthorizeCustomer(customerNumber);
                });

                result.CustomerNumber = (ulong)customerNumber;
                result.SessionID = customerData.SessionID;
                result.DailyTransactionsLimit = customerData.DailyTransactionsLimit;
                customer.CustomerNumber = customerNumber;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(ex.Message);
            }


            return result;
        }

        public bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source)
        {
            try
            {
                bool checkCustomerSession = true;



                if (authorizedCustomerSessionID != "")
                {
                    CheckCustomerAuthorization(authorizedCustomerSessionID);
                    if (AuthorizedCustomer.SessionID == Guid.Empty.ToString())
                    {
                        checkCustomerSession = false;
                        return checkCustomerSession;
                    }
                }

                if (source == SourceType.Bank)
                {
                    if (AuthorizedCustomer != null)
                    {
                        AuthorizedCustomer.OneTransactionLimit = 10000000000;
                        AuthorizedCustomer.DailyTransactionsLimit = 10000000000;
                    }
                }

                User = user;
                Source = source;
                Language = language;
                ClientIp = clientIp;
                return checkCustomerSession;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public ActionResult SaveAndApproveLoanProductActivationOrder(LoanProductActivationOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.SaveAndApproveLoanProductActivationOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


    }
}
