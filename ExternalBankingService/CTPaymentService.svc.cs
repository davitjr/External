using ExternalBanking;
using ExternalBankingService.Interfaces;
using NLog;
using NLog.Targets;
using System;
using System.ServiceModel;
using System.Web.Configuration;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CTPaymentService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select CTPaymentService.svc or CTPaymentService.svc.cs at the Solution Explorer and start debugging.
    public class CTPaymentService : ICTPaymentService
    {

        CashTerminal CashTerminal { get; set; }

        Logger _logger = LogManager.GetCurrentClassLogger();

        public PaymentRegistrationResult SaveCTPaymentOrder(CTPaymentOrder order)
        {
            try
            {
                CashTerminal terminal = new CashTerminal(CashTerminal.CustomerNumber, CashTerminal.UserName);
                return terminal.SaveCTPaymentOrder(order);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public void Init(CashTerminal cashTerminal)
        {
            CashTerminal = new CashTerminal();
            CashTerminal.CustomerNumber = cashTerminal.CustomerNumber;
            CashTerminal.UserName = cashTerminal.UserName;
            CashTerminal.ID = cashTerminal.ID;
        }


        public void WriteLog(Exception ex)
        {
            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "CTPaymentService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "CTPaymentService-Test");
            }

            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());
            if (CashTerminal != null && !string.IsNullOrEmpty(CashTerminal.UserName))
            {
                GlobalDiagnosticsContext.Set("UserName", CashTerminal.UserName);
            }
            else
            {
                GlobalDiagnosticsContext.Set("UserName", "");
            }
            GlobalDiagnosticsContext.Set("ClientIp", "");

            string message = (ex.Message != null ? ex.Message : " ") + Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }

        public PaymentRegistrationResult SaveCTLoanMatureOrder(CTLoanMatureOrder order)
        {
            try
            {
                CashTerminal terminal = new CashTerminal(CashTerminal.CustomerNumber, CashTerminal.UserName);
                return terminal.SaveCTLoanMatureOrder(order);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public PaymentStatus GetPaymentStatus(long paymentID)
        {

            try
            {
                CashTerminal terminal = new CashTerminal();
                return terminal.GetPaymentStatus(paymentID);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public PaymentStatus GetPaymentStatusByOrderID(long orderID)
        {

            try
            {
                CashTerminal terminal = new CashTerminal();
                return terminal.GetPaymentStatusByOrderID(orderID);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public CashTerminal CheckTerminalPassword(string userName, string password)
        {
            try
            {
                return CashTerminal.CheckTerminalPassword(userName, password);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Deposit GetActiveDeposit(string depositFullNumber)
        {

            try
            {
                Deposit deposit = new Deposit();
                deposit = Deposit.GetActiveDeposit(depositFullNumber);
                return deposit;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public C2CTransferResult SaveC2CTransferOrder(C2CTransferOrder request)
        {

            try
            {
                CashTerminal terminal = new CashTerminal(CashTerminal.CustomerNumber, CashTerminal.UserName);
                C2CTransferResult response = new C2CTransferResult();
                response = terminal.C2CTransfer(request);
                return response;

            }
            catch (C2CTransferException ex)
            {
                WriteLog(ex);
                return ex.TransferResponse;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public CashTerminal GetTerminal(int terminalID)
        {
            CashTerminal terminal = new CashTerminal();
            terminal = CashTerminal.GetTerminal(Convert.ToUInt16(terminalID));
            return terminal;
        }


        public int GetC2CTransferCurrencyCode(string currency)
        {
            try
            {
                int code = 0;
                string currencyCodeN = Utility.GetCurrencyCode(currency);
                code = Convert.ToInt32(currencyCodeN);
                return code;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public C2CTransferStatusResponse GetC2CTransferStatus(long transferID)
        {

            try
            {
                CashTerminal terminal = new CashTerminal();
                return terminal.GetC2CTransferStatus(transferID);

            }
            catch (C2CTransferStatusException ex)
            {
                WriteLog(ex);
                return ex.TransferStatusResponse;
            }

            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public C2CTransferStatusResponse GetC2CTransferStatusByOrderID(long orderID)
        {

            try
            {
                CashTerminal terminal = CashTerminal;
                return terminal.GetC2CTransferStatusByOrderID(orderID);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public EOGetClientResponse GetClient(EOGetClientRequest request)
        {

            try
            {
                EOGetClientResponse response = new EOGetClientResponse();
                response = request.GetClient();
                return response;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        /// <summary>
        /// Արտաքին կազմակերպության համար (GoodCredit) քարտից քարտ փոխանցում 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public EOTransferResponse MakeTransfer(EOTransferRequest request)
        {

            try
            {
                EOTransferResponse response = new EOTransferResponse();
                response = request.MakeTransfer();
                return response;

            }
            catch (C2CTransferException ex)
            {
                WriteLog(ex);
                EOTransferResponse response = new EOTransferResponse();
                response.ErrorCode = 1;
                response.ErrorText = ex.TransferResponse.ResultCodeDescription;
                return response;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public int GetLastKeyNumber(int keyID)
        {

            try
            {
                int result = Utility.GetLastKeyNumber(keyID);

                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public string GetCreditAccountNumberFromLoanFullNumber(string loanFullNumber)
        {
            SSTerminalService terminalService = new SSTerminalService();
            Loan loan = CTLoanMatureOrder.GetLoanByLoanFullNumber(loanFullNumber);
            return terminalService.GetCreditCodeAccountForMature(loan.CreditCode, loan.Currency, "AMD").AccountNumber;

        }

        public Account GetAccount(string accountNumber)
        {

            return Account.GetAccount(accountNumber);


        }


    }
}
