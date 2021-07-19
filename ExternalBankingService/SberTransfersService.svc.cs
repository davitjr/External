using ExternalBanking;
using ExternalBanking.SberTransfers;
using ExternalBanking.SberTransfers.Models;
using ExternalBanking.SberTransfers.Order;
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

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SberTransfersService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SberTransfersService.svc or SberTransfersService.svc.cs at the Solution Explorer and start debugging.
    public class SberTransfersService : ISberTransfersService
    {
        Logger _logger = LogManager.GetCurrentClassLogger();

        AuthorizedCustomer AuthorizedCustomer { get; set; }

        string ClientIp { get; set; }

        byte Language { get; set; }

        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        ExternalBanking.ACBAServiceReference.User User { get; set; }
        const string SESSIONID_NonCustomerService = "4D17A445F7504D059A6279936B1847AD";

        public void WriteLog(Exception ex)
        {

            GlobalDiagnosticsContext.Set("Logger", "SberTransfersService");
            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());

            GlobalDiagnosticsContext.Set("UserName", "");
            GlobalDiagnosticsContext.Set("ClientIp", "");

            string message = (ex.Message != null ? ex.Message : " ") +
                Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }

        public SberPreTransferRequisites GetDataForSberTransfer(ulong customerNumber)
        {
            return SberTransfers.GetDataForSberTransfer(customerNumber);
        }

        private void InitOrder(Order order)
        {
            order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
            order.Source = SourceType.SberBankTransfer;
            order.user = User;
            order.OperationDate = Utility.GetNextOperDay();
            order.DailyTransactionsLimit = AuthorizedCustomer.DailyTransactionsLimit;
        }

        private Customer CreateCustomer()
        {
            Customer customer;
            customer = new Customer();
            customer.OneOperationAmountLimit = 100000000000;
            customer.DailyOperationAmountLimit = 100000000000;
            customer.User = User;
            customer.User.filialCode = 22000;
            customer.Source = SourceType.SberBankTransfer;

            return customer;
        }

        public (ActionResult, DateTime?) SaveAndApproveSberIncomingTransferOrder(SberIncomingTransferOrder order)
        {
            try
            {
                Init(order.CustomerNumber);
                Customer customer = CreateCustomer();
                InitOrder(order);
                var (result, registrationDate) = customer.SaveAndApproveSberIncomingTransferOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                return (result, registrationDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                return (new ActionResult { ResultCode = ResultCode.Failed }, null);
            }
        }

        /// <summary>
        /// Սերվիսի ինիցիալիզացում
        /// </summary>
        /// <param name="clientIp">IP որից եկել է հարցումը</param>
        internal void Init(ulong customerNumber)
        {
            try
            {
                ClientIp = "SberTransfersService";
                Language = 1;
                User = new ExternalBanking.ACBAServiceReference.User() { userID = 88 };
                AuthorizedCustomer = new AuthorizedCustomer();

                AuthorizedCustomer.CustomerNumber = customerNumber;
                AuthorizedCustomer.ApprovementScheme = 1;
                AuthorizedCustomer.DailyTransactionsLimit = 10000000000;
                AuthorizedCustomer.OneTransactionLimit = 10000000000;
                AuthorizedCustomer.Permission = 0;
                AuthorizedCustomer.SecondConfirm = 0;
                AuthorizedCustomer.TypeOfClient = 0;
                AuthorizedCustomer.UserName = "";
                AuthorizedCustomer.BranchCode = 22000;
                AuthorizedCustomer.FullName = "";
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

    }
}
