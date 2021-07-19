using NLog;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using ExternalBanking;
using ExternalBankingService.Interfaces;
using xbs = ExternalBanking.ACBAServiceReference;
using infsec = ExternalBankingService.InfSecServiceReference;
using ExternalBankingService.AOService;
using ExternalBanking.XBManagement;
using System.Web.Configuration;
using NLog.Targets;
using System.Data;
using ExternalBanking.ServiceClient;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ACBAOnlineService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ACBAOnlineService.svc or ACBAOnlineService.svc.cs at the Solution Explorer and start debugging.
    public class ACBAOnlineService : IACBAOnlineService
    {


        string ClientIp { get; set; }
        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// Հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }
        byte Language { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Սերվիսի ինիցիալիզացում
        /// </summary>
        /// <param name="authorizedCustomerSessionID">սեսսիայի համար</param>
        /// <param name="language">լեզու</param>
        /// <param name="clientIp">IP որից եկել է հարցումը</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="source">Սկզբնաղբյուր</param>
        public bool InitAOCustomer(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source)
        {
            try
            {
                bool isOK = false;
                if (authorizedCustomerSessionID != "")
                {
                        AcbaOnlineUserData customerData = null;

                        Use(client =>
                        {
                            customerData = client.CheckAuthorization(authorizedCustomerSessionID, language);
                        });

                        if (customerData != null && customerData.AuthorizationResult != null && customerData.AuthorizationResult.IsAuthorized)
                        {
                            AuthorizedCustomer = new AuthorizedCustomer();
                            AuthorizedCustomer.ApprovementScheme = Convert.ToInt16(customerData.ApprovementScheme);
                            AuthorizedCustomer.BranchCode = customerData.BranchCode;
                            AuthorizedCustomer.CustomerNumber = Convert.ToUInt64(customerData.CustomerNumber);
                            AuthorizedCustomer.DailyTransactionsLimit = customerData.DailyTransactionsLimit;
                            AuthorizedCustomer.FullName = customerData.FullName;
                            AuthorizedCustomer.OneTransactionLimit = customerData.OneTransactionLimit;
                            AuthorizedCustomer.Permission = customerData.Permission;
                            AuthorizedCustomer.SecondConfirm = customerData.SecondConfirm;
                            AuthorizedCustomer.SessionID = customerData.SessionID;
                            AuthorizedCustomer.TypeOfClient = customerData.TypeOfClient;
                            AuthorizedCustomer.UserName = customerData.UserName;

                            isOK = true;
                        }
                }
               

                if (source == SourceType.Bank)
                {
                    if (AuthorizedCustomer != null)
                    {
                        AuthorizedCustomer.OneTransactionLimit = 1000000000;
                        AuthorizedCustomer.DailyTransactionsLimit = 1000000000;
                    }
                }

                User = user;
                Source = source;
                Language = language;
                ClientIp = clientIp;

                return isOK;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AOService.AcbaOnlineUserData AuthorizeAcbaOnlineCustomerByPassword(AOService.LoginInfo loginInfo, byte langId)
        {
            AcbaOnlineUserData customerData = new AcbaOnlineUserData();
            Customer customer = new Customer();

            try
            {
                Use(client =>
                {
                    customerData = client.AuthorizeUserByUserPassword(loginInfo, langId);
                });

                customer.SaveExternalBankingLogHistory(1, customerData.UserID, SourceType.AcbaOnline, customerData.SessionID, loginInfo.IpAddress, loginInfo.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return customerData;
        }


        public AOService.AcbaOnlineUserData AuthorizeAcbaOnlineCustomerByToken(AOService.LoginInfo loginInfo, LoginResult lr, byte langId)
        {

            AcbaOnlineUserData customerData = new AcbaOnlineUserData();
            Customer customer = new Customer();

            try
            {
                Use(client =>
                {
                    customerData = client.AuthorizeUserByToken(loginInfo, lr, langId);
                });

                customer.SaveExternalBankingLogHistory(1, customerData.UserID, SourceType.AcbaOnline, customerData.SessionID, loginInfo.IpAddress, loginInfo.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return customerData;
        }


        public bool ResetUserPassword(AOService.LoginInfo loginInfo)
        {

            bool isReset = false;

            try
            {
                Use(client =>
                {
                    isReset = client.ResetUserPassword(loginInfo);
                });

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return isReset;
        }

        public AOService.AcbaOnlineUserData ChangeAcbaOnlineCustomerPassword(AOService.ChangePasswordInfo passwordInfo, string SessionId, byte langId)
        {

            AcbaOnlineUserData customerData = new AcbaOnlineUserData();
            Customer customer = new Customer();

            try
            {

                Use(client =>
                {
                    customerData = client.ChangeUserPassword(passwordInfo, SessionId, langId);
                });

                customer.SaveExternalBankingLogHistory(1, customerData.UserID, SourceType.AcbaOnline, customerData.SessionID, passwordInfo.IpAddress, passwordInfo.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return customerData;
        }

        public bool IsAbleToChangeQuality(string userName, string userGroups, int id)
        {
            try
            {
                return Order.IsAbleToChangeQuality(userName, userGroups, id);
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
                GlobalDiagnosticsContext.Set("Logger", "XBACBAOnlineService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "XBACBAOnlineService-Test");
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

            string message = (ex.Message != null ? ex.Message : " ") + Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }


        public ActionResult ApproveOrder(Order order)
        {
            try
            {
                ActionResult result = new ActionResult();
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                result = service.ApproveOrder(order);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        
        public string GetTerm(short id, string[] param, Languages language)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetTerm(id, param, language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLoanProductInterestRate(LoanProductOrder order, string cardNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetLoanProductInterestRate(order, cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveLoanProductOrder(LoanProductOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.SaveLoanProductOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveCredentialOrder(CredentialOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.SaveCredentialOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public List<ActionError> FastOverdraftValidations(string cardNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.FastOverdraftValidations(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetOrderServiceFee(OrderType type, int urgent)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetOrderServiceFee(type, urgent);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PaymentOrder GetPaymentOrder(long id)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetPaymentOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApprovePaymentOrder(PaymentOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.ApprovePaymentOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<int> GetConfirmRequiredOrders(string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                List<int> list = new List<int>();
                List<Order> orders= service.GetConfirmRequiredOrders(userName, subTypeId, startDate, endDate, langId, receiverName, account, period, groups, quality);
                orders.ForEach(m =>
                {
                    list.Add((int)m.Id);
                });
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public KeyValuePair<String, double> GetArCaBalance(string cardNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetArCaBalance(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool VerifyToken(string SessionId, string OTP, string ipAddress, byte langId)
        {
            bool isVerified = false;

            try
            {
                Use(client =>
                {
                    isVerified = client.VerifyToken(SessionId, OTP, ipAddress, langId);
                });
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return isVerified;
        }


        public string GetSwiftMessageStatement(DateTime dateFrom, DateTime dateTo, string accountNumber)
        {
            string statement = "";

            try
            {
                statement = SwiftMessage.GetSwiftMessageStatement(dateFrom, dateTo, accountNumber, SourceType.AcbaOnline);
                return statement;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<AccountFreezeDetails> GetAccountFreezeHistory(string accountNumber, ushort freezeStatus, ushort reasonId)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetAccountFreezeHistory(accountNumber, freezeStatus, reasonId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է լիազորագրի տվյալ գործողության տեսակին համապատասխանող հաճախորդի հաշիվները 
        /// </summary>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public List<Account> GetAccountsForCredential(int operationType)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetAccountsForCredential(operationType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetServiceProvidedOrderFee(OrderType orderType, ushort serviceType)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetServiceProvidedOrderFee(orderType, serviceType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CredentialOrder GetCredentialOrder(long ID)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCredentialOrder(ID);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetStatement(string cardAccount, DateTime dateFrom, DateTime dateTo, byte option)
        {
            try
            {
                PosTerminal p = new PosTerminal();
                return p.GetStatement(cardAccount, dateFrom, dateTo, option);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }
        }

        public double GetDepositLoanCreditLineAndProfisionCoefficent(string loanCurrency, string provisionCurrency, bool mandatoryPayment, int creditLineType)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetDepositLoanCreditLineAndProfisionCoefficent(loanCurrency, provisionCurrency, mandatoryPayment, creditLineType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public List<TransferCallContract> GetContractsForTransfersCall(string customerNumber, string accountNumber, string cardNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetContractsForTransfersCall(customerNumber, accountNumber, cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.SaveReceivedFastTransferPaymentOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ReceivedFastTransferPaymentOrder GetReceivedFastTransferPaymentOrder(long id)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetReceivedFastTransferPaymentOrder(id, "");

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

  

        public List<OrderHistory> GetOrderHistory(long orderId)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetOnlineOrderHistory(orderId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        /// <summary>
        /// Վերադարձնում է ստացված արագ փոխանցման հայտի մերժման պատճառը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string GetReceivedFastTransferOrderRejectReason(int orderId)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetReceivedFastTransferOrderRejectReason(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        /// <summary>
        /// Վերադարձնում է բիզնես ավանդի օպցիաները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public DepositOrderCondition GetDepositCondition(DepositOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetDepositCondition(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Վերադարձնում է բիզնես ավանդի օպցիաների պարամետրերի ստուգում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public ActionResult CheckDepositOrderCondition(DepositOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.CheckDepositOrderCondition(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է նվազեցման հնարավորություն ունեցող բիզնես ավանդի հաշիվները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public List<Account> GetDecreasingDepositAccountList(ulong customerNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetDecreasingDepositAccountList(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetSwiftMessage940Statement(DateTime dateFrom, DateTime dateTo, string accountNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetSwiftMessage940Statement(dateFrom, dateTo, accountNumber, SourceType.AcbaOnline);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Deposit GetDeposit(ulong productId)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetDeposit(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public string GetInternationalTransferSentTime (int docId)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetInternationalTransferSentTime(docId);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }
        }


        public bool HasOrHadAccount(ulong customerNumber)
        {
            try
            {

                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.HasOrHadAccount(customerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<Communal> GetCommunals(SearchCommunal searchCommunal)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetCommunals(searchCommunal);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }
        }

        public decimal? GetBeelineAbonentBalance(string abonentNumber)
        {
            try
            {

                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetBeelineAbonentBalance(abonentNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CommunalDetails> GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType)
        {
            try
            {
                return CommunalDetails.GetCommunalDetails((CommunalTypes)communalType, abonentNumber, checkType, Language, branchCode, abonentType, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string SearchFullCommunalGasOnline(string abonentNumber, string branchCode, int num = 0)
        {
            try
            {
                return CommunalDetails.SearchFullCommunalGasOnline(abonentNumber,branchCode, num);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        private ActionResult ConfirmOrderOnline(long id)
        {
            return Order.ConfirmOrderOnline(id, User);
        }

        public OrderQuality GetOrderQualityByDocID(long docID)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetOrderQualityByDocID(docID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        private static void Use(Action<IAcbaOnlineLogin> action)
        {
            IAcbaOnlineLogin client = ProxyManager<IAcbaOnlineLogin>.GetProxy(nameof(IAcbaOnlineLogin));

            bool success = false;

            try
            {
                action(client);
                ((IClientChannel)client).Close();

                success = true;
            }

            catch (FaultException ex)

            {
                ((IClientChannel)client).Close();

                throw;
            }
            catch (TimeoutException e)
            {

            }
            catch (Exception e)
            {
                ((IClientChannel)client).Abort();
                throw;
            }
            finally
            {
                if (!success)
                {
                    ((IClientChannel)client).Abort();

                }
                ((IClientChannel)client).Dispose();
            }
        }

        public double GetAcccountAvailableBalance(string accountNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetAcccountAvailableBalance(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public KeyValuePair<string, string> GetCurrentOperDay24_7_Mode()
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCurrentOperDay24_7_Mode();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsLoan_24_7(ulong productId)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.IsLoan_24_7(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public decimal? GetUcomFixAbonentBalance(string abonentNumber)
        {
            try
            {

                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);

                return service.GetUcomFixAbonentBalance(abonentNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
    }
}
