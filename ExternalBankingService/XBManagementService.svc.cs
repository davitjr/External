using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ExternalBanking;
using System.Data;
using NLog;
using System.Collections;
using ExternalBankingService.Interfaces;
using infsec = ExternalBankingService.InfSecServiceReference;
using ExternalBanking.XBManagement;
using System.Web.Configuration;
using NLog.Targets;
using acba = ExternalBanking.ACBAServiceReference;
using ExternalBankingService.InfSecServiceReference;


namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "XBManagementService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select XBManagementService.svc or XBManagementService.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class XBManagementService : IXBManagementService
    {
        string ClientIp { get; set; }

        byte Language { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }

        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }

        /// <summary>
        /// Սպասարկման տեսակ (հաճախորդի կամ ոչ հաճախորդի սպասարկում)
        /// </summary>
        ServiceType CustomerServiceType { get; set; }

        /// <summary>
        /// Պայմանական SESSIONID ոչ հաճախորդի սպասարկման դեպքում
        /// </summary>
        const string SESSIONID_NonCustomerService = "4D17A445F7504D059A6279936B1847AD";


        internal void CheckCustomerAuthorization(string sessionID)
        {
            if (sessionID == SESSIONID_NonCustomerService)
            {
                AuthorizedCustomer = new AuthorizedCustomer();
                AuthorizedCustomer.ApprovementScheme = 1;
                AuthorizedCustomer.DailyTransactionsLimit = 10000000000;
                AuthorizedCustomer.OneTransactionLimit = 10000000000;
                AuthorizedCustomer.Permission = 0;
                AuthorizedCustomer.SecondConfirm = 0;
                AuthorizedCustomer.SessionID = sessionID;
                AuthorizedCustomer.TypeOfClient = 0;
                AuthorizedCustomer.UserName = "";
                AuthorizedCustomer.BranchCode = 22000;
                AuthorizedCustomer.CustomerNumber = 0;
                AuthorizedCustomer.FullName = "";


                return;
            }

            infsec.CustomertData customerData = null;

            AuthorizationService.Use(client =>
            {
                customerData = client.CheckCustomerAuthorization(sessionID);
            });

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
                AuthorizedCustomer.SessionID = customerData.SessionID;
                AuthorizedCustomer.TypeOfClient = 0;
                AuthorizedCustomer.UserName = customerData.CustomerNumber.ToString();
            }
        }

        /// <summary>
        /// Սերվիսի ինիցիալիզացում
        /// </summary>
        /// <param name="authorizedCustomerSessionID">սեսսիայի համար</param>
        /// <param name="language">լեզու</param>
        /// <param name="clientIp">IP որից եկել է հարցումը</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="source">Սկզբնաղբյուր</param>
        public bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source, ServiceType customerServiceType = ServiceType.CustomerService)
        {
            try
            {
                bool checkCustomerSession = true;

                if (customerServiceType == ServiceType.NonCustomerService)
                    authorizedCustomerSessionID = SESSIONID_NonCustomerService;

                if (authorizedCustomerSessionID != "")
                {
                    CheckCustomerAuthorization(authorizedCustomerSessionID);
                    if (AuthorizedCustomer.SessionID == Guid.Empty.ToString())
                    {
                        checkCustomerSession = false;
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

                CustomerServiceType = customerServiceType;
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


        public void WriteLog(Exception ex)
        {


            GlobalDiagnosticsContext.Set("ClientIp", ClientIp);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingManagementService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingManagementService-Test");
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

        private Customer CreateCustomer()
        {
            Customer customer;
            if (CustomerServiceType == ServiceType.CustomerService || Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
            {
                customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                customer.OneOperationAmountLimit = AuthorizedCustomer.OneTransactionLimit;
                customer.DailyOperationAmountLimit = AuthorizedCustomer.DailyTransactionsLimit;
            }
            else
            {
                customer = new Customer();
                customer.OneOperationAmountLimit = 100000000000;
                customer.DailyOperationAmountLimit = 100000000000;
            }

            customer.User = User;
            customer.Source = Source;

            return customer;
        }



        public List<XBUserGroup> GetXBUserGroups()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetXBUserGroups();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<HBUser> GetHBUsersByGroup(int id)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetHBUsersByGroup(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ApprovementSchema GetApprovementSchema()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetApprovementSchema();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ApprovementSchemaDetails> GetApprovementSchemaDetails(int schemaId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetApprovementSchemaDetails(schemaId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public string GenerateNextGroupName(ulong customerNumber)
        {
            try
            {
                return XBUserGroup.GenerateNextGroupName(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Լրացնում հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrder(Order order)
        {
            order.FilialCode = User.filialCode;
            order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
            order.Source = Source;
            order.user = User;
            order.OperationDate = Utility.GetCurrentOperDay();
            order.DailyTransactionsLimit = AuthorizedCustomer.DailyTransactionsLimit;
        }
        public HBApplication GetHBApplication()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetHBApplication();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public HBApplication GetHBApplicationShablon()
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetHBApplicationShablon();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public HBApplicationOrder GetHBApplicationOrder(long ID)
        {
            HBApplicationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetHBApplicationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveHBApplicationOrder(HBApplicationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveHBApplicationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<HBUser> GetHBUsers(int hbAppID, ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetHBUsers(hbAppID, filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<HBToken> GetHBTokens(int HBUserID, ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetHBTokens(HBUserID, filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetHBTokenNumbers(HBTokenTypes tokenType)
        {
            try
            {
                //to do  Jnjel
                acba.User user = new acba.User();
                user.filialCode = 22000;
                return HBToken.GetHBTokenNumbers(tokenType, User.filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool CheckHBUserNameAvailability(HBUser hbUser)
        {
            bool result;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                result = hbUser.CheckHBUserNameAvailability(customer.CustomerNumber);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public PhoneBankingContract GetPhoneBankingContract()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPhoneBankingContract();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApprovePhoneBankingContractOrder(PhoneBankingContractOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePhoneBankingContractOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public ActionResult SaveAndApproveHBApplicationQualityChangeOrder(HBApplicationQualityChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveHBApplicationQualityChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApprovePhoneBankingContractClosingOrder(PhoneBankingContractClosingOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePhoneBankingContractClosingOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult CancelTokenNumberReservation(HBToken token)
        {
            try
            {
                return HBToken.CancelTokenNumberReservation(token);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public PhoneBankingContractClosingOrder GetPhoneBankingContractClosingOrder(long ID)
        {
            PhoneBankingContractClosingOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPhoneBankingContractClosingOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveHBServletRequestOrder(HBServletRequestOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveHBServletRequestOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public PhoneBankingContractOrder GetPhoneBankingContractOrder(long ID)
        {
            PhoneBankingContractOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPhoneBankingContractOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<AssigneeCustomer> GetHBAssigneeCustomers(ulong customerNumber)
        {
            List<AssigneeCustomer> assigneeList;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                assigneeList = customer.GetHBAssigneeCustomers(customerNumber);
                return assigneeList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public HBApplicationQualityChangeOrder GetHBApplicationQualityChangeOrder(long ID)
        {
            HBApplicationQualityChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetHBApplicationQualityChangeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public HBServletRequestOrder GetHBServletRequestOrder(long ID)
        {
            HBServletRequestOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetHBServletRequestOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public HBActivationOrder GetHBActivationOrder(long ID)
        {
            HBActivationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetHBActivationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveHBActivationOrder(HBActivationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveHBActivationOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveHBActivationOrder(HBActivationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveHBActivationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveHBActivationOrder(HBActivationOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveHBActivationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<HBActivationRequest> GetHBRequests()
        {
            try
            {
                List<HBActivationRequest> list = new List<HBActivationRequest>();
                Customer customer = CreateCustomer();
                list = customer.GetHBRequests();
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public Account GetOperationSystemAccount(Order order, OrderAccountType accountType, string operationCurrency, ushort filialCode = 0, string utilityBranch = "", ulong customerNumber = 0)
        {
            try
            {
                return Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, accountType), operationCurrency, filialCode, 0, "0", utilityBranch, customerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetHBServiceFee(DateTime setDate, HBServiceFeeRequestTypes requestType, HBTokenTypes tokenType, HBTokenSubType tokenSubType)
        {
            double serviceFee;
            try
            {
                Customer customer = CreateCustomer();
                serviceFee = customer.GetHBServiceFee(setDate, requestType, tokenType, tokenSubType);
                return serviceFee;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public String GetHBTokenGID(int hbuserID, HBTokenTypes tokenType)
        {
            String GID;
            try
            {
                GID = HBToken.GetHBTokenGID(hbuserID, tokenType);
                return GID;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveHBRegistrationCodeResendOrder(HBRegistrationCodeResendOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveHBRegistrationCodeResendOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveHBActivationRejectionOrder(HBActivationRejectionOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveHBActivationRejectionOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<HBUserLog> GetHBUserLog(String userName)
        {
            List<HBUserLog> hbUserLog = new List<HBUserLog>();
            try
            {
                Customer customer = CreateCustomer();
                hbUserLog = customer.GetHBUserLog(userName);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return hbUserLog;
        }

        public PhoneBankingContractActivationRequest GetPhoneBankingRequests()
        {
            try
            {
                PhoneBankingContractActivationRequest request = new PhoneBankingContractActivationRequest();
                Customer customer = CreateCustomer();
                request = customer.GetPhoneBankingRequests();
                return request;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApprovePhoneBankingContractActivationOrder(PhoneBankingContractActivationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePhoneBankingContractActivationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetUnusedTokensByFilialAndRange(string from, string to, int filial)
        {
            try
            {
                return TokensDistribution.GetTokenNumbersByRangeAndFilial(from, to, filial);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void MoveUnusedTokens(int filialToMove, List<string> unusedTokens)
        {
            try
            {
                TokensDistribution.MoveUnusedTokens(filialToMove, unusedTokens);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void SetUser(AuthorizedCustomer authorizedCustomer, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source)
        {
            try
            {
                AuthorizedCustomer = authorizedCustomer;
                if (source == SourceType.MobileBanking || source == SourceType.AcbaOnline)
                {
                    user.filialCode = 22000;
                }
                User = user;
                Source = source;
                Language = language;
                ClientIp = clientIp;
                CustomerServiceType = ServiceType.CustomerService;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<acba.CustomerEmail> GetCustomerEmails(ulong customerNumber)
        {
            List<acba.CustomerEmail> emails = new List<acba.CustomerEmail>();
            XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
            try
            {
                emails = service.GetCustomerMainData(customerNumber).Emails;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return emails;
        }



        public ActionResult GenerateAcbaOnline(string userName, string password, ulong customerNumber, string phoneNumber, int customerQuality, string email)
        {
            try
            {

                HBApplicationOrder hBApplicationOrder = new HBApplicationOrder();
                return hBApplicationOrder.GenerateAcbaOnline(userName, password, customerNumber, (Languages)Language, User, phoneNumber, ClientIp, customerQuality, email);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ulong GetHBUserCustomerNumber(string userName)
        {
            try
            {
                return HBUser.GetHBUserCustomerNumber(userName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult AddEmailForCustomer(string emailAddress, ulong customerNumber)
        {
            try
            {
                HBApplicationOrder hBApplicationOrder = new HBApplicationOrder();
                return hBApplicationOrder.AddEmailForCustomer(emailAddress, customerNumber, User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public AuthorizedCustomer GetXBMTestMobileBankingUser()
        {
            AuthorizedCustomer AutorizedCustomer = new AuthorizedCustomer();

            //AutorizedCustomer.CustomerNumber = this.GetTestMobileCustomerNumber(); 100000071433;  //103600004797; //101200003781; //100000004089;//100900047419;//100900047419
            AutorizedCustomer.UserName = "AIMDAN";//TestMobileBankingUser
            AutorizedCustomer.DailyTransactionsLimit = 15000000;

            AutorizedCustomer.OneTransactionLimit = 5000000;
            AutorizedCustomer.ApprovementScheme = 1;
            AutorizedCustomer.LimitedAccess = 0;
            return AutorizedCustomer;
        }
        public ActionResult SaveAndApproveHBApplicationFullPermissionsGrantingOrder(HBApplicationFullPermissionsGrantingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveHBApplicationFullPermissionsGrantingOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public HBApplicationFullPermissionsGrantingOrder GetHBApplicationFullPermissionsGrantingOrder(long ID)
        {
            HBApplicationFullPermissionsGrantingOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetHBApplicationFullPermissionsGrantingOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


    }
}
