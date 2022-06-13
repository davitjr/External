using ExternalBanking;
using ExternalBanking.Events;
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
    public class EventService : IEventService
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


        public EventService()
        {

        }
        public EventService(string clientIp, byte language, AuthorizedCustomer authorizedCustomer, ExternalBanking.ACBAServiceReference.User user, SourceType source)
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

        private Customer CreateCustomer()
        {
            Customer customer;
            if (CustomerServiceType == ServiceType.CustomerService || Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline || Source == SourceType.SSTerminal)
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


        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrder(Order order)
        {
            if (Source != SourceType.SSTerminal && Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking && Source != SourceType.CashInTerminal && Source != SourceType.STAK)
            {
                order.FilialCode = User.filialCode;
            }

            order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
            order.Source = Source;
            order.user = User;
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
        public ActionResult SaveEventTicketOrder(EventTicketOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveEventTicketOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<Event> GetEventSubTypes(EventTypes eventTypes)
        {
            try
            {
                return Events.GetEventSubTypes(eventTypes, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public EventTicketOrder GetEventTicketOrder(long ID)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetEventTicketOrder(ID, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveEventTicketOrder(EventTicketOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveEventTicketOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                //Գործարքի կատարում
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = Order.GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            Order.ConfirmOrderOnline(order.Id, User);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                result.Id = order.Id;
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
    }
}
