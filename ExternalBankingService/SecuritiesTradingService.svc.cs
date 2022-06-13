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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SecuritiesTradingService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SecuritiesTradingService.svc or SecuritiesTradingService.svc.cs at the Solution Explorer and start debugging.

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SecuritiesTradingService : ISecuritiesTradingService
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

        /// <summary>
        /// Սպասարկման տեսակ (հաճախորդի կամ ոչ հաճախորդի սպասարկում)
        /// </summary>
        ServiceType CustomerServiceType { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Պայմանական SESSIONID ոչ հաճախորդի սպասարկման դեպքում
        /// </summary>
        const string SESSIONID_NonCustomerService = "4D17A445F7504D059A6279936B1847AD";


        public SecuritiesTradingService()
        {

        }
        public SecuritiesTradingService(string clientIp, byte language, AuthorizedCustomer authorizedCustomer, ExternalBanking.ACBAServiceReference.User user, SourceType source)
        {
            ClientIp = clientIp;
            Language = language;
            AuthorizedCustomer = authorizedCustomer;
            User = user;
            Source = source;
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
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void WriteLog(Exception ex, string json = "")
        {
            GlobalDiagnosticsContext.Set("ClientIp", ClientIp);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingService-TestRelease");
            }

            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "") + " customer_number = " + AuthorizedCustomer?.CustomerNumber.ToString() + " //  json = " + json;
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
                Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }

        public SecuritiesTradingOrder GetSecuritiesTradingOrder(long ID)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetSecuritiesTradingOrder(ID, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetSecuritiesTradingOrderDepositedAmount(SecuritiesTradingOrder order)
        {
            try
            {
                return SecuritiesTradingOrder.GetSecuritiesTradingOrderDepositedAmount(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public (double, double) GetSecuritiesTradingOrderFee(long OrderId, string ISIN, double DepositedAmount, SharesTypes types, string ListingType, string Currency, int OperationQuantity, short CalculatingType)
        {
            try
            {
                return SecuritiesTradingOrder.GetSecuritiesTradingOrderFee(OrderId, ISIN, DepositedAmount, types, ListingType, Currency, OperationQuantity, CalculatingType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
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
    }
}
