using ExternalBanking;
using ExternalBankingService.Interfaces;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Web.Configuration;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AcbamatService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AcbamatService.svc or AcbamatService.svc.cs at the Solution Explorer and start debugging.
    public class AcbamatService : IAcbamatService
    {
        string ClientIp { get; set; }
        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }

        /// <summary>
        /// Ավտորիզացված հաճախորդ
        /// </summary>
        AuthorizedCustomer AuthorizedCustomer { get; set; }

        /// <summary>
        ///  Լեզու
        /// </summary>
        byte Language { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        SourceType Source { get; set; }

        public void Init(AuthorizedCustomer authorizedCustomer, string clientIp, ExternalBanking.ACBAServiceReference.User user, byte language, SourceType source)
        {
            AuthorizedCustomer = authorizedCustomer;
            ClientIp = clientIp;
            User = user;
            Language = language;
            Source = source;
        }

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void WriteLog(Exception ex)
        {
            GlobalDiagnosticsContext.Set("Logger", "AcbamatService");

            string stackTrace = (ex.StackTrace ?? " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");

            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());
            GlobalDiagnosticsContext.Set("UserName", "");
            GlobalDiagnosticsContext.Set("ClientIp", "");

            string message = (ex.Message ?? " ") + Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            DatabaseTarget databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);
        }

        private void InitOrder(Order order)
        {
            if (Source != SourceType.SSTerminal && Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking && Source != SourceType.CashInTerminal && Source != SourceType.STAK)
            {
                order.FilialCode = User.filialCode;
            }

            order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
            order.Source = Source;
            order.user = User;
            if (AuthorizedCustomer.UserName != "0" || order.Type == OrderType.CommunalPayment || order.Type == OrderType.CashCommunalPayment)
            {
                order.OPPerson = new OPPerson
                {
                    PersonName = AuthorizedCustomer.FullName,
                    PersonResidence = 1,
                    CustomerNumber = AuthorizedCustomer.CustomerNumber
                };
            }

            order.RegistrationDate = order.RegistrationDate.Date;
            if (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline || Source == SourceType.SSTerminal || Source == SourceType.STAK || Source == SourceType.CashInTerminal)
            {
                order.OperationDate = Utility.GetNextOperDay();
            }
            else
            {
                order.OperationDate = Utility.GetCurrentOperDay();
            }

            order.DailyTransactionsLimit = AuthorizedCustomer.DailyTransactionsLimit;
        }

        public DateTime GetCurrentOperDay()
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCurrentOperDay();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ExchangeRate> GetExchangeRates()
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetExchangeRates();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Communal> CheckMobileNumber(SearchCommunal communal)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCommunals(communal, true);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void RegisterExchange(AcbamatExchangeOrder acbamatExchangeOrder)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(acbamatExchangeOrder);
                service.RegisterExchange(acbamatExchangeOrder);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void RegisterThirdPartyWithdrawal(AcbamatThirdPartyWithdrawalOrder acbamatThirdPartyWithdrawalOrder)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(acbamatThirdPartyWithdrawalOrder);
                service.RegisterThirdPartyWithdrawal(acbamatThirdPartyWithdrawalOrder);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
    }
}
