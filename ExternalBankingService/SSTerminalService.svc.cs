using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ExternalBankingService.Interfaces;
using ExternalBanking;
using System.Web.Configuration;
using NLog;
using NLog.Targets;
using ACBALibrary;
using System.Data;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CTPaymentService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select CTPaymentService.svc or CTPaymentService.svc.cs at the Solution Explorer and start debugging.
    public class SSTerminalService : ISSTerminalService
    {
        /// <summary>
        ///Ավտորիզացված տերմինալ
        /// </summary>
        SSTerminal AuthorizedSSTerminal { get; set; }
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

        Logger _logger = LogManager.GetCurrentClassLogger();
        public SSTerminalService() { }
        public SSTerminalService(string clientIp, byte language, AuthorizedCustomer authorizedCustomer, SSTerminal authorizedSSTerminal, ExternalBanking.ACBAServiceReference.User user, SourceType source)
        {
            ClientIp = clientIp;
            Language = language;
            AuthorizedCustomer = authorizedCustomer;
            AuthorizedSSTerminal = authorizedSSTerminal;
            User = user;
            Source = source;
        }
        public void Init(AuthorizedCustomer authorizedCustomer, SSTerminal authorizedSSTerminal, string clientIp, ExternalBanking.ACBAServiceReference.User user, byte language, SourceType source)
        {
            AuthorizedCustomer = authorizedCustomer;
            AuthorizedSSTerminal = authorizedSSTerminal;
            //if (AuthorizedSSTerminal != null)
            //    AuthorizedSSTerminal.TerminalID = "10000001";//kjnenq
            ClientIp = clientIp;
            User = user;
            Language = language;
            Source = source;
        }
        public void WriteLog(Exception ex)
        {
            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
            {
                GlobalDiagnosticsContext.Set("Logger", "SSTerminalService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "SSTerminalService-Test");
            }

            string stackTrace = (ex.StackTrace != null ? ex.StackTrace : " ") + Environment.NewLine + " InnerException StackTrace:" + (ex.InnerException != null ? ex.InnerException.StackTrace : "");
            GlobalDiagnosticsContext.Set("StackTrace", stackTrace);
            GlobalDiagnosticsContext.Set("ExceptionType", ex.GetType().ToString());
            if (AuthorizedSSTerminal != null && !string.IsNullOrEmpty(AuthorizedSSTerminal.TerminalID))
            {
                GlobalDiagnosticsContext.Set("UserName", AuthorizedSSTerminal.TerminalID);
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
        public List<Card> GetCards(ProductQualityFilter filter)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCards(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Account> GetAccounts()
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                List<Account> accounts = service.GetCurrentAccounts(ProductQualityFilter.Opened);
                accounts.RemoveAll(acc => acc.AccountType == 115);
                accounts.RemoveAll(a => (a.AccountType == 18 && a.TypeOfAccount == 282) || (a.AccountType == 18 && a.TypeOfAccount == 283));
                return accounts;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Deposit> GetDeposits(ProductQualityFilter filter)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetDeposits(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Loan> GetLoans(ProductQualityFilter filter)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetLoans(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Account GetAccount(string accountNumber)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetAccount(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GenerateNewOrderNumber(orderNumberType, User.filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetLastRates(string currency, RateType rateType, ExchangeDirection direction)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetLastExchangeRate(currency, rateType, direction, AuthorizedSSTerminal.FilialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCurrencyExchangeOrder(CurrencyExchangeOrder order)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(order);
                return service.SaveAndApproveCurrencyExchangeOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApprovePaymentOrder(PaymentOrder order)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(order);
                return service.SaveAndApprovePaymentOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ushort GetCrossConvertationVariant(string debitCurrency, string creditCurrency)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCrossConvertationVariant(debitCurrency, creditCurrency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ulong GetAccountCustomerNumber(string accountNumber)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                Account account = new Account();
                account.AccountNumber = accountNumber;
                return service.GetAccountCustomerNumber(account);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLastCrossExchangeRate(string dCur, string cCur, ushort filialCode = 22000)
        {
            try
            {
                return Utility.GetLastCrossExchangeRate(dCur, cCur, filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<ExchangeRate> GetExchangeRates(int filialCode)
        {

            try
            {
                return ExchangeRate.GetExchangeRates(filialCode);
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
        private void initCTOrder(CTOrder order)
        {
            order.Source = Source;
            order.user = User;
            order.OperationDate = Utility.GetCurrentOperDay();
            order.RegistrationDate = DateTime.Now.Date;
            order.TerminalID = AuthorizedSSTerminal.TerminalID.ToString();
            order.TerminalDescription = "";
            order.TerminalAddress = "";
            order.FilialCode = AuthorizedSSTerminal.FilialCode;
        }
        

        public Loan GetLoanByLoanFullNumber(string loanFullNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoan(loanFullNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CreditLine GetCreditLineByLoanFullNumber(string loanFullNumber)
        {
            try
            {
                return CreditLine.GetCreditLine(loanFullNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public ActionResult SaveAndApproveUtilityPaymentOrder(UtilityPaymentOrder order)
        {

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(order);

                if (order.CommunalType == CommunalTypes.Gas)
                {
                    List<CommunalDetails> communalDetails = new List<CommunalDetails>();
                    communalDetails = CommunalDetails.GetCommunalDetails((CommunalTypes)order.CommunalType, order.Code, 2, 1, "", (AbonentTypes)Enum.Parse(typeof(AbonentTypes), order.AbonentType.ToString()), order.Source);

                    double gasUsageDebt = 0;
                    double serviceFeeDebt = 0;

                    foreach (CommunalDetails detail in communalDetails.FindAll(m => m.Id == 37 || m.Id == 41))
                    {
                        if (detail.Id == 37) //Գազի սպառման դիմաց պարտք
                            gasUsageDebt = Double.Parse(detail.Value.ToString());
                        else //Սպասարկման դիմաց պարտք
                            serviceFeeDebt = Double.Parse(detail.Value.ToString());
                    }

                    Communal.CalculateGasPromPaymentAmounts(order, gasUsageDebt, serviceFeeDebt);
                }

                return service.SaveAndApproveUtilityPaymentOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարումների նկարագրությունը
        /// </summary>
        public string GetCommunalPaymentDescription(short utilityType, short abonentType, string searchData, string branch)
        {
            SearchCommunal communal = new SearchCommunal();
            communal.AbonentType = 1;
            communal.CommunalType = (CommunalTypes)utilityType;
            communal.AbonentNumber = searchData;
            communal.PhoneNumber = "";
            communal.Branch = branch;
            string description = "";

            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                description = service.GetCommunalPaymentDescription(communal);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

            return description;
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

        public string GetAccountCurrency(string accountNumber)
        {

            try
            {
                return Account.GetAccountCurrency(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ConfirmOrder(Order order)
        {

            try
            {
                return order.Confirm(User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveMatureOrder(MatureOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(order);
                return service.SaveAndApproveMatureOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public Double GetLoanAmountForFullRepayment(Loan loan)
        {
            try
            {
                return loan.CurrentCapital + loan.OutCapital +
               FormatRound(loan.CurrentRateValue, 0) + FormatRound(loan.PenaltyRate, 0)
               + FormatRound(loan.JudgmentRate, 0) + FormatRound(loan.CurrentFee, 0);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        private double FormatRound(double decimals, short value)
        {
            return Double.Parse(Math.Round(decimals, value).ToString());
        }

        public Order GetOrderDetails(long orderId)
        {
            try
            {
                return Order.GetOrderDetails(orderId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public double GetCardFeeForCurrencyExchangeOrder(CurrencyExchangeOrder order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                return customer.GetCardFee(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        private void InitOrder(Order order)
        {
            if (Source == SourceType.SSTerminal || Source == SourceType.CashInTerminal )
            {
                order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                AuthorizedSSTerminal.FilialCode = SSTerminal.GetTerminalFilial(AuthorizedSSTerminal.TerminalID);
                if (AuthorizedCustomer.UserName != "0" || order.Type == OrderType.CommunalPayment || order.Type == OrderType.CashCommunalPayment)
                {
                    order.OPPerson = new OPPerson();
                    order.OPPerson.PersonName = AuthorizedCustomer.FullName;
                    order.OPPerson.PersonResidence = 1;
                    order.OPPerson.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                }
                order.FilialCode = AuthorizedSSTerminal.FilialCode;
                order.TerminalID = AuthorizedSSTerminal.TerminalID.ToString();
                order.RegistrationDate = order.RegistrationDate.Date;
                order.OperationDate = GetCurrentOperDay();
            }
        }
        public List<Account> GetAccountsForUtility()
        {
            List<Account> accounts;
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                accounts = service.GetAccountsForOrder(15, 1, 1);
                accounts.RemoveAll(m => m.AccountType == 54 || m.AccountType == 58);

                //Քարտի բլոկավորված լինելու դեպքում հեռացնում է քարտային հաշիվը հաշվիների ցուցակից
                List<Card> cards = GetCards(ProductQualityFilter.Opened);
                List<Account> cardAccounts = new List<Account>();

                foreach (Card card in cards.FindAll(m => m.Currency == "AMD"))
                {
                    ArcaBalanceResponseData arcaData = card.GetArCaBalanceResponseData(User.userID);

                    if (arcaData.ResponseCode == "00" && !cardAccounts.Contains(card.CardAccount))
                    {
                        cardAccounts.Add(card.CardAccount);
                    }
                }

                foreach (Account account in accounts.FindAll(m => m.AccountType == 11 && m.TypeOfAccount == 24))
                {
                    if (cardAccounts.Find(a => a.AccountNumber == account.AccountNumber) == null)
                    {
                        accounts.Remove(account);
                    }
                }

                return accounts;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Account> GetAccountsForDebet()
        {
            List<Account> accounts;
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                accounts = service.GetAccountsForOrder(1, 3, 1);
                accounts.RemoveAll(m => m.AccountType == 54 || m.AccountType == 58);

                //Քարտի բլոկավորված լինելու դեպքում հեռացնում է քարտային հաշիվը հաշվիների ցուցակից
                List<Card> cards = GetCards(ProductQualityFilter.Opened);
                List<Account> cardAccounts = new List<Account>();

                foreach (Card card in cards)
                {
                    ArcaBalanceResponseData arcaData = card.GetArCaBalanceResponseData(User.userID);

                    if (arcaData.ResponseCode == "00" && !cardAccounts.Contains(card.CardAccount))
                    {
                        cardAccounts.Add(card.CardAccount);
                    }
                }

                foreach (Account account in accounts.FindAll(m => m.AccountType == 11 && m.TypeOfAccount == 24))
                {
                    if (cardAccounts.Find(a => a.AccountNumber == account.AccountNumber) == null)
                    {
                        accounts.Remove(account);
                    }
                }

                return accounts;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Account> GetAccountsForCredit()
        {
            List<Account> accounts;
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                accounts = service.GetAccountsForOrder(1, 3, 2);
                if (accounts.Exists(m => m.AccountType == 54 || m.AccountType == 58))
                {
                    List<Loan> aparikTexumLoans = service.GetAparikTexumLoans();
                    foreach (Account account in accounts.FindAll(m => m.AccountType == 54 || m.AccountType == 58))
                    {
                        if (!aparikTexumLoans.Exists(m => m.ConnectAccount.AccountNumber == account.AccountNumber))
                        {
                            accounts.RemoveAll(m => m.AccountNumber == account.AccountNumber);
                        }
                    }
                }
                return accounts;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Account GetProductAccountFromCreditCode(string creditCode, ushort productType, ushort accountType)
        {
            try
            {
                return Account.GetProductAccountFromCreditCode(creditCode, productType, accountType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Account GetCreditCodeAccountForMature(string creditCode, string loanCurrency, string amountCurrency)
        {
            try
            {
                Account account = new Account();
                if (loanCurrency == "AMD")
                {
                    account = GetProductAccountFromCreditCode(creditCode, 18, 224);
                }
                else if (loanCurrency != "AMD")
                {
                    if (amountCurrency == "AMD")
                    {
                        account = GetProductAccountFromCreditCode(creditCode, 18, 279);
                    }
                    else
                    {
                        account = GetProductAccountFromCreditCode(creditCode, 18, 224);
                    }
                }
                return account;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public short CheckTerminalAuthorization(string terminalID, string ipAddress, string password)
        {
            try
            {
                return SSTerminal.CheckTerminalAuthorization(terminalID, ipAddress, password);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetPaymentOrderFee(PaymentOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCardFee(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SendSMS(string phoneNumber, string messageText, int messageTypeID, SourceType sourceType)
        {

            try
            {
                return Utility.SendSMS(phoneNumber, messageText, messageTypeID, User.userID, sourceType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ushort GetTerminalFilial(string TerminalID)
        {

            try
            {
                return SSTerminal.GetTerminalFilial(TerminalID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման մանրամասները
        /// </parameters>
        ///    checkType=3 Հիմնական տվյալներ,checkType=2 մանրամասն տվյալներ
        ///  </parameters>
        /// </summary>
        public List<CommunalDetails> GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType)
        {
            try
            {
                List<CommunalDetails> details = CommunalDetails.GetCommunalDetails((CommunalTypes)communalType, abonentNumber, checkType, Language, branchCode, abonentType, Source);
                details.RemoveAll(m => m.Description == "Operator Name");
                return details;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ArcaBalanceResponseData GetArCaBalanceResponseData(string cardNumber)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetArCaBalanceResponseData(cardNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public OperDayClosingStatus GetOperDayClosingStatus(ushort filialCode)
        {

            try
            {
                return Validation.GetOperDayClosingStatus(filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public double GetAcccountAvailableBalance(string accountNumber)
        {
            try
            {
                return Account.GetAcccountAvailableBalance(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        } 

        public Account GetOperationSystemAccount(string currency)
        {
            try
            {
                return SSTerminal.GetOperationSystemAccount(AuthorizedSSTerminal.TerminalID, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        } 

        public Account GetOperationSystemTransitAccount(string currency)
        {
            try
            {
                return SSTerminal.GetOperationSystemTransitAccount(AuthorizedSSTerminal.TerminalID, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool IsCardAccount(string AccountNumber)
        {
            return Utility.IsCardAccount(AccountNumber);
        }
        public double GetCardFee(PaymentToARCAOrder paymentOrder)
        {
            double fee = 0;
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                return service.GetCardFee(paymentOrder);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApprovePaymentToARCAOrder(PaymentToARCAOrder order)
        {
            try
            {
                XBService service = new XBService(ClientIp, Language, AuthorizedCustomer, User, Source);
                InitOrder(order);
                return service.SaveAndApprovePaymentToARCAOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool HasActiveCreditLineForCardAccount(string cardAccount)
        {
            return CreditLine.HasActiveCreditLineForCardAccount(cardAccount);

        }
        public string GetAccountCustomerFullNameEng(string accountNumber)
        {
            return Account.GetAccountCustomerFullNameEng(accountNumber);
        }

        public string GetLoanTypeDescriptionEng(string loanFullNumber)
        {
            return Loan.GetLoanTypeDescriptionEng(loanFullNumber);
        }

        public string GetJointTypeDescription(ushort jointType, Languages language)
        {
            return Info.GetJointTypeDescription(jointType, language);
        }

        public Dictionary<int, string> GetCommunalDetailsTypes()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            DataTable dt = Info.GetCommunalDetailsTypes();

            foreach (DataRow dr in dt.Rows)
            {
                int key = int.Parse(dr["id"].ToString());
                string value = dr["description_eng"].ToString().Trim();
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, value);
                }

            }

            return dictionary;
        }
        public Loan GetLoan(string loanFullNumber)
        {
            Loan loan = Loan.GetLoan(loanFullNumber);

            if (loan != null)
            {
                loan.NextRepayment = loan.GetLoanNextRepayment();
            }

            return loan;
        }
    }
}







