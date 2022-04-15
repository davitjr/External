using ExternalBanking;
using ExternalBanking.ARUSDataService;
using ExternalBankingService.Filters;
using ExternalBankingService.InfSecServiceReference;
using ExternalBankingService.Interfaces;
using NLog;
using NLog.Targets;
using System;
using System.ServiceModel;
using System.Web.Configuration;

namespace ExternalBankingService
{
    [RequestHeaderOutputBehaviorInfSecServiceOperation]
    public class STAKService : ISTAKService
    {
        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }

        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// Ավտորիզացված հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }

        Languages Language { get; set; }

        string ClientIP { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();


        public R2ARequestOutput SaveAndApproveSTAKPaymentOrder(R2ARequest r2ARequest)
        {
            try
            {
                // NEW
                STAKPaymentOrder order = new STAKPaymentOrder(r2ARequest);
                order.SetCountry(r2ARequest.SenderCountryCode);

                Customer customer = CreateCustomer();

                InitOrder(order);

                return customer.SaveAndApproveSTAKPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);


                /*  // OLD
                 //ClientIP = "";
                 //Language = Languages.eng;   //   hy = 1,  eng = 2

                 //AuthorizedCustomer = new AuthorizedCustomer();
                 //AuthorizedCustomer.UserName = "";
                 //AuthorizedCustomer.ApprovementScheme = 1;

                 //Source = SourceType.STAK;

                 ////ToDo STAK  userID
                 //User = new ExternalBanking.ACBAServiceReference.User();
                 //User.userID = 2840; // 1733344, 2944  // Գործող միջավայրի համար ՊԿ-ն ուրիշ է, Պռոդ գնալուց ՊԵՏՔ է ՓՈԽԵԼ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                 */

                /*
                XBService service = new XBService(ClientIP, (byte)Language, AuthorizedCustomer, User, Source);

                //XBService service = new XBService();

                return service.SaveAndApproveSTAKPaymentOrder(r2ARequest);
                */
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public void WriteLog(Exception ex)
        {
            ClientIP = "";

            GlobalDiagnosticsContext.Set("ClientIp", ClientIP);

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "STAKService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "STAKService-TestRelease");
            }

            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());

            GlobalDiagnosticsContext.Set("UserName", "");

            if (ClientIP != null)
                GlobalDiagnosticsContext.Set("ClientIp", ClientIP);
            else
                GlobalDiagnosticsContext.Set("ClientIp", "");

            string message = (ex.Message != null ? ex.Message : " ") +
                Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }

        public bool Initialization(string authorizedUserSessionToken, Languages language, string clientIP, SourceType source)
        {
            try
            {
                bool checkUserSession = true;

                //authorizedUserSessionToken = "asdasd";

                AuthorizedUser authorizedUser = AuthorizationService.AuthorizeUserBySessionToken(authorizedUserSessionToken);

                if (!authorizedUser.isAutorized)
                {
                    checkUserSession = false;
                    return checkUserSession;
                }


                ClientIP = clientIP; // "";
                Language = language; //  Languages.eng;   //   hy = 1,  eng = 2

                Source = source;  //  SourceType.STAK;

                AuthorizedCustomer = new AuthorizedCustomer();
                AuthorizedCustomer.UserName = "";
                AuthorizedCustomer.ApprovementScheme = 1;


                ACBALibrary.User ACBALibraryUser = AuthorizationService.InitUser(authorizedUser);

                User = new ExternalBanking.ACBAServiceReference.User();
                User.userID = ACBALibraryUser.userID;   // 2840; // 1733344, 2944  // Գործող միջավայրի համար ՊԿ-ն ուրիշ է, Պռոդ գնալուց ՊԵՏՔ է ՓՈԽԵԼ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                return checkUserSession;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        private Customer CreateCustomer()
        {
            Customer customer = new Customer();
            customer.OneOperationAmountLimit = 100000000000;
            customer.DailyOperationAmountLimit = 100000000000;

            customer.User = User;
            customer.Source = Source;
            customer.Culture = new Culture(Language);

            return customer;
        }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrder(Order order)
        {
            order.Source = Source;
            order.user = User;

            order.OperationDate = Utility.GetNextOperDay();

            //order.DailyTransactionsLimit = AuthorizedCustomer.DailyTransactionsLimit;
        }


        public AuthorizedUser AuthorizeUser(LoginInfo loginInfo)
        {
            AuthorizedUser authorizedUser = null;

            try
            {
                authorizedUser = AuthorizationService.AuthorizeUser(loginInfo);
            }
            catch (Exception)
            {
                //WriteLog(ex);
                //throw new FaultException(SaveErrorMessage);
                throw new FaultException();
            }

            return authorizedUser;
        }


    }
}
