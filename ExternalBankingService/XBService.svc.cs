using ExternalBanking;
using ExternalBanking.ARUSDataService;
using ExternalBanking.PreferredAccounts;
using ExternalBanking.QrTransfers;
using ExternalBanking.ServiceClient;
using ExternalBanking.UtilityPaymentsManagment;
using ExternalBanking.XBManagement;
using ExternalBankingService.InfSecServiceReference;
using ExternalBankingService.Interfaces;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using ActionError = ExternalBanking.ActionError;
using ActionResult = ExternalBanking.ActionResult;
using infsec = ExternalBankingService.InfSecServiceReference;
using ResultCode = ExternalBanking.ResultCode;
using xbs = ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using static ExternalBanking.ReceivedBillSplitRequest;
//using ExternalBankingService.Filters;



namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ExternalBankingService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client1 for testing this service, please select ExternalBankingService.svc or ExternalBankingService.svc.cs at the Solution Explorer and start debugging.
    //[AspNetCompatibilityRequirements(RequirementsMode =AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    //[RequestHeaderOutputBehaviorInfSecServiceOperation]
    public class XBService : IXBService
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

        /// <summary>
        /// Պայմանական SESSIONID ոչ հաճախորդի սպասարկման դեպքում
        /// </summary>
        const string SESSIONID_NonCustomerService = "4D17A445F7504D059A6279936B1847AD";

        Logger _logger = LogManager.GetCurrentClassLogger();


        public XBService()
        {

        }
        public XBService(string clientIp, byte language, AuthorizedCustomer authorizedCustomer, ExternalBanking.ACBAServiceReference.User user, SourceType source)
        {
            ClientIp = clientIp;
            Language = language;
            AuthorizedCustomer = authorizedCustomer;
            User = user;
            Source = source;
        }

        public AccountStatement AccountStatement(string accountNumber, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            try
            {
                AccountStatement result = null;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                Account account = customer.GetAccount(accountNumber);
                account.AccountNumber = accountNumber;
                if (account != null)
                    result = account.GetStatement(dateFrom, dateTo, Language, 0, Source, minAmount, maxAmount, debCred, transactionsCount, orderByAscDesc);

                return result;
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccount(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetTransitAccountNumberFromCardAccount(double cardAccountNumber)
        {
            try
            {
                return DahkDetails.GetTransitAccountNumberFromCardAccount(cardAccountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAccountsForBlockingAvailableAmount(ulong customerNumber)
        {
            try
            {
                return DahkDetails.GetAccountsForBlockingAvailableAmount(customerNumber);
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccounts();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetCurrentAccounts(ProductQualityFilter filter)
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCurrentAccounts(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Account> GetCardAccounts()
        {
            try
            {
                return Account.GetCardAccounts(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Card GetCard(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCard(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Card> GetCards(ProductQualityFilter filter, bool includingAttachedCards = true)
        {
            try
            {
                List<Card> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCards(filter, includingAttachedCards);
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardStatement GetCardStatement(string cardNumber, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            try
            {
                CardStatement result = null;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                bool check = customer.CheckCardOwner(cardNumber);
                if (check)
                    result = Card.GetStatement(cardNumber, dateFrom, dateTo, Language, minAmount, maxAmount, debCred, transactionsCount, orderByAscDesc);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Loan GetLoan(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoan(productId);
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoans(filter, Source);
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDeposit(productId);
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

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDeposits(filter, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PeriodicTransfer GetPeriodicTransfer(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPeriodicTransfer(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PeriodicTransfer> GetPeriodicTransfers(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPeriodicTransfers(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Order> GetDraftOrders(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDraftOrders(dateFrom, dateTo);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Order> GetSentOrders(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetSentOrders(dateFrom, dateTo);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Order> GetOrders(SearchOrders searchParams)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOrders(searchParams, User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Order> GetNotConfirmedOrders(int start, int end)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetNotConfirmedOrders(User, start, end);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաղորդագրությունների ցանկ, կախված տեսակից (type=1 Ուղարկված,type=2 Ստացված)
        /// </summary>
        public List<Message> GetMessages(DateTime dateFrom, DateTime dateTo, short type)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                return customer.GetMessages(dateFrom, dateTo, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Message> GetNumberOfMessages(short messagesCount, MessageType type)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                return customer.GetMessages(messagesCount, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void AddMessage(Message message)
        {
            try
            {
                message.Add(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void DeleteMessage(int messageId)
        {
            try
            {
                Message message = new Message();
                message.Id = messageId;
                message.Delete(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void MarkMessageReaded(int messageId)
        {
            try
            {
                Message message = new Message();
                message.Id = messageId;
                message.MarkAsReaded(AuthorizedCustomer.CustomerNumber);
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

                KeyValuePair<String, double> result;

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);

                Card card = customer.GetCard(cardNumber);

                if (card != null)
                    result = card.GetArCaBalance(User.userID, ClientIp);
                else
                {
                    result = new KeyValuePair<string, double>(key: "997", value: 0);
                }

                return result;
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
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingService");
            }
            else
            {
                GlobalDiagnosticsContext.Set("Logger", "ExternalBankingService-TestRelease");
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
                Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "");

            var databaseTarget = (DatabaseTarget)LogManager.Configuration.FindTargetByName("database");
            databaseTarget.ConnectionString = WebConfigurationManager.ConnectionStrings["NLogDb"].ToString();
            LogManager.ReconfigExistingLoggers();

            _logger.Error(message);

        }

        public int AddContact(Contact contact)
        {
            try
            {
                contact.Add(AuthorizedCustomer.CustomerNumber);
                return 0;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public int UpdateContact(Contact contact)
        {
            try
            {
                contact.Update();
                return 0;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Contact GetContact(ulong contactId)
        {
            Contact contact = Contact.GetContact(contactId);
            return contact;

        }

        public List<Contact> GetContacts()
        {
            return Contact.GetContacts(AuthorizedCustomer.CustomerNumber);
        }

        public int DeleteContact(ulong contactId)
        {
            try
            {
                Contact contact = new Contact();
                contact.Id = contactId;
                contact.Delete();
                return 0;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարումների ցանկ
        /// </summary>
        public List<Communal> GetCommunals(SearchCommunal searchCommunal, bool isSearch = true)
        {
            try
            {

                return searchCommunal.SearchCommunalByType(Source);
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
                return CommunalDetails.GetCommunalDetails((CommunalTypes)communalType, abonentNumber, checkType, Language, branchCode, abonentType, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Վերադարձնում է կոմունալ վճարումների հանձնարարականի համար անհրաժեշտ պարամետրերը
        /// </summary>
        public List<KeyValuePair<string, string>> GetCommunalReportParameters(SearchCommunal searchCommunal)
        {
            try
            {


                return searchCommunal.GetCommunalReportParameters();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SavePaymentOrder(PaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveBudgetPaymentOrder(BudgetPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveBudgetPaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveBudgetPaymentOrder(BudgetPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveBudgetPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveInternationalPaymentOrder(InternationalPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                if (order.SwiftMessageID == 0)
                    InitOrder(order);
                else
                    InitOrderForSwiftMessage(order);
                return customer.SaveAndApproveInternationalPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveFastTransferPaymentOrder(FastTransferPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveFastTransferPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order, string authorizedUserSessionToken)
        {
            try
            {
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.SaveAndApproveReceivedFastTransferPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCallTransferChangeOrder(TransferByCallChangeOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCallTransferChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);

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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaymentOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public BudgetPaymentOrder GetBudgetPaymentOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetBudgetPaymentOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public InternationalPaymentOrder GetInternationalPaymentOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetInternationalPaymentOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte[] LoansDramContract(string accountNumber)
        {
            try
            {
                return Loan.LoansDramContract(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public FastTransferPaymentOrder GetFastTransferPaymentOrder(long id, string authorizedUserSessionToken)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFastTransferPaymentOrder(id, authorizedUserSessionToken, User.userName, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ReceivedFastTransferPaymentOrder GetReceivedFastTransferPaymentOrder(long id, string authorizedUserSessionToken)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetReceivedFastTransferPaymentOrder(id, User.userName, authorizedUserSessionToken, ClientIp, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Transfer GetTransfer(ulong id)
        {
            try
            {

                Transfer transfer = new Transfer();
                transfer.Id = id;
                transfer.Get(User);
                return transfer;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Transfer GetApprovedTransfer(ulong id)
        {
            try
            {

                Transfer transfer = new Transfer();
                transfer.Id = id;
                transfer.GetApprovedTransfer();
                return transfer;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ReceivedBankMailTransfer GetReceivedBankMailTransfer(ulong id)
        {
            try
            {

                ReceivedBankMailTransfer transfer = new ReceivedBankMailTransfer();
                transfer.ID = id;
                transfer.Get();
                return transfer;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ConfirmTransfer(ulong transferID, short allowTransferConfirm, string authorizedUserSessionToken)
        {
            try
            {
                Transfer transfer = new Transfer();
                TransferConfirmOrder transferConfirmOrder = new TransferConfirmOrder();
                transfer.Id = transferID;
                transferConfirmOrder.Transfer = transfer;
                InitOrder(transferConfirmOrder);
                return transferConfirmOrder.Confirm(User.filialCode.ToString(), allowTransferConfirm, Utility.GetCurrentOperDay(), User.userID, AuthorizedCustomer.UserName, Source, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult DeleteTransfer(ulong transferID, string description)
        {
            try
            {
                Transfer transfer = new Transfer();
                TransferDeleteOrder transferDeleteOrder = new TransferDeleteOrder();
                transfer.Id = transferID;
                transferDeleteOrder.Transfer = transfer;
                transferDeleteOrder.Description = description;
                InitOrderForTransfer(transferDeleteOrder);
                return transferDeleteOrder.Delete(User.filialCode.ToString(), Utility.GetCurrentOperDay(), Source, AuthorizedCustomer?.ApprovementScheme ?? 1);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveTransfer(TransferApproveOrder transferApproveOrder)
        {
            try
            {

                InitOrder(transferApproveOrder);
                return transferApproveOrder.Approve(User.filialCode.ToString(), Utility.GetCurrentOperDay(), Source, AuthorizedCustomer.ApprovementScheme);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<int> GetTransferCriminalLogId(ulong id)
        {
            try
            {
                Transfer transfer = new Transfer();
                transfer.Id = id;
                transfer.Get();
                List<int> criminalLogId = transfer.GetTransferCriminalLogId();
                return criminalLogId;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLastExchangeRate(string currency, RateType rateType, ExchangeDirection direction, ushort filalCode)
        {
            try
            {
                return Utility.GetLastExchangeRate(currency, rateType, direction, filalCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetFastTransferFeeAcbaPercent(byte transferType)
        {
            try
            {
                return FastTransferPaymentOrder.GetFastTransferFeeAcbaPercent(transferType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetReceivedFastTransferFeePercent(byte transferType, string code = "", string countryCode = "", double amount = 0, string currency = "", DateTime date = new DateTime())
        {
            try
            {
                return ReceivedFastTransferPaymentOrder.GetReceivedFastTransferFeePercent(transferType, code, countryCode, amount, currency, date);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte GetFastTransferAcbaCommisionType(byte transferType, string code)
        {
            try
            {
                return ReceivedFastTransferPaymentOrder.GetFastTransferAcbaCommisionType(transferType, code);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetPaymentOrderFee(PaymentOrder order, int feeType)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.GetPaymentOrderFee(order, feeType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetInternationalPaymentOrderFee(InternationalPaymentOrder order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                InitOrder(order);
                return customer.GetInternationalPaymentOrderFee(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetCardFee(PaymentOrder order)
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


        public PaymentOrderFutureBalance GetPaymentOrderFutureBalance(PaymentOrder order)
        {
            try
            {
                return order.GetFutureBalance();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PaymentOrderFutureBalance GetPaymentOrderFutureBalanceById(long id)
        {
            try
            {
                PaymentOrder order = new PaymentOrder();
                order.Id = id;
                order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                order.Get();
                return order.GetFutureBalance();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveUtiliyPaymentOrder(UtilityPaymentOrder utilityPaymentOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(utilityPaymentOrder);
                utilityPaymentOrder.Code = utilityPaymentOrder.Code.Trim();
                return customer.SaveUtiliyPaymentOrder(utilityPaymentOrder, AuthorizedCustomer.UserName);
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
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                //Գործարքի կատարում
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult DeleteOrder(Order order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                ActionResult result = customer.DeleteOrder(order, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public UtilityPaymentOrder GetUtilityPaymentOrder(long id)
        {
            UtilityPaymentOrder utilityPaymentOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                utilityPaymentOrder = customer.GetUtilityPaymentOrder(id);
                return utilityPaymentOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetAccountsForOrder(short orderType, byte orderSubType, byte accountType, bool includingAttachedCards = true)
        {
            try
            {


                Customer customer = new Customer(User, AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountsForOrder((OrderType)orderType, orderSubType, (OrderAccountType)accountType, Source, includingAttachedCards);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetCustomerAccountsForOrder(ulong Customer_number, short orderType, byte orderSubType, byte accountType)
        {
            try
            {

                Customer customer = new Customer(User, Customer_number, (Languages)Language);
                return customer.GetAccountsForOrder((OrderType)orderType, orderSubType, (OrderAccountType)accountType, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetUnreadedMessagesCount()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                return customer.GetUnreadedMessagesCount();
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
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveUtilityPaymentOrder(UtilityPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveUtilityPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public string GetCurrencyCode(string currency)
        {
            try
            {
                return Utility.GetCurrencyCode(currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetUnreadMessagesCountByType(MessageType type)
        {
            try
            {
                Customer customer = new Customer();
                customer.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                return customer.GetUnreadMessagesCount(type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveDepositOrder(DepositOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveDepositOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveDepositTermination(DepositTerminationOrder order)
        {
            try
            {
                ActionResult result = new ActionResult();
                Customer customer = CreateCustomer();
                InitOrder(order);
                result = customer.SaveDepositTermination(order, AuthorizedCustomer.UserName);
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public DepositOrder GetDepositorder(long ID)
        {
            DepositOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDepositorder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveDepositOrder(DepositOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveDepositOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved && quality != OrderQuality.Draft)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

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

            if (Source == SourceType.PhoneBanking)
            {
                customer.OneTransactionLimitToAnothersAccount = AuthorizedCustomer.OneTransactionLimitToAnothersAccount;
                customer.OneTransactionLimitToOwnAccount = AuthorizedCustomer.OneTransactionLimitToOwnAccount;
                customer.DayLimitToAnothersAccount = AuthorizedCustomer.DayLimitToAnothersAccount;
                customer.DayLimitToOwnAccount = AuthorizedCustomer.DayLimitToOwnAccount;
            }

            customer.User = User;
            customer.Source = Source;

            return customer;
        }

        public ActionResult SaveReferenceOrder(ReferenceOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveReferenceOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult ApproveReferenceOrder(ReferenceOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveReferenceOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved && quality != OrderQuality.Draft)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ReferenceOrder GetReferenceOrder(long ID)
        {
            ReferenceOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetReferenceOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveChequeBookOrder(ChequeBookOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveChequeBookOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ApproveChequeBookOrder(ChequeBookOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveChequeBookOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ChequeBookOrder GetChequeBookOrder(long ID)
        {
            ChequeBookOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetChequeBookOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveCashOrder(CashOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCashOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ApproveCashOrder(CashOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveCashOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public CashOrder GetCashOrder(long ID)
        {
            CashOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCashOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveStatmentByEmailOrder(StatmentByEmailOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveStatmentByEmailOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ApproveStatmentByEmailOrder(StatmentByEmailOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveStatmentByEmailOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public StatmentByEmailOrder GetStatmentByEmailOrder(long ID)
        {
            StatmentByEmailOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetStatmentByEmailOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveSwiftCopyOrder(SwiftCopyOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveSwiftCopyOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ApproveSwiftCopyOrder(SwiftCopyOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveSwiftCopyOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public SwiftCopyOrder GetSwiftCopyOrder(long ID)
        {
            SwiftCopyOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetSwiftCopyOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveCustomerDataOrder(CustomerDataOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCustomerDataOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ApproveCustomerDataOrder(CustomerDataOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveCustomerDataOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public CustomerDataOrder GetCustomerDataOrder(long ID)
        {
            CustomerDataOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCustomerDataOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Dictionary<ulong, string> GetThirdPersons()
        {
            try
            {
                Dictionary<ulong, string> AllThirdPerson = new Dictionary<ulong, string>();
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                customer.Source = Source;
                AllThirdPerson = customer.GetThirdPersons();
                return AllThirdPerson;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult CheckDepositOrderCondition(DepositOrder order)
        {
            try
            {
                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                InitOrder(order);
                result = order.CheckDepositOrderCondition();

                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    Localization.SetCulture(result, customer.Culture);
                    return result;
                }
                result.ResultCode = ResultCode.Normal;
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public DepositOrderCondition GetDepositCondition(DepositOrder order)
        {
            try
            {
                DepositOrderCondition condition = new DepositOrderCondition();
                order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                bool isEmployeeDeposit = false;

                if (Deposit.CheckCustomerForEmployeeDeposit(AuthorizedCustomer.CustomerNumber))
                {
                    isEmployeeDeposit = true;
                }

                if (order.AccountType == 3 && isEmployeeDeposit == false && order.ThirdPersonCustomerNumbers != null && order.ThirdPersonCustomerNumbers.Count > 0)
                {
                    order.ThirdPersonCustomerNumbers.ForEach(m =>
                    {
                        if (m.Key.ToString().Length == 12)
                        {
                            if (Deposit.CheckCustomerForEmployeeDeposit(m.Key))
                            {
                                isEmployeeDeposit = true;
                            }

                        }
                    });

                }
                else if (order.AccountType == 2)
                {
                    isEmployeeDeposit = false;
                }

                condition = Deposit.GetDepositOrderCondition(order, Source, isEmployeeDeposit);
                return condition;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult ApproveDepositTermination(DepositTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveDepositTermination(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public Boolean ManuallyRateChangingAccess(Double amount, string currency, string convertationCurrency, SourceType sourceType)
        {
            try
            {
                Boolean changeAccess = false;
                changeAccess = Utility.ManuallyRateChangingAccess(amount, currency, convertationCurrency, sourceType);
                return changeAccess;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public TransferByCallList GetTransfersbyCall(TransferByCallFilter filter)
        {
            try
            {
                TransferByCallList list = new TransferByCallList();
                list = filter.GetList();
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public TransferByCallList GetCustomerTransfersbyCall(TransferByCallFilter filter)
        {
            try
            {
                TransferByCallList list = new TransferByCallList();
                string isCallCenter = null;
                User.AdvancedOptions.TryGetValue("isCallCenter", out isCallCenter);
                if (isCallCenter != "1")
                    filter.Filial = User.filialCode;
                list = filter.GetList();
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Transfer> GetTransfers(TransferFilter filter)
        {
            try
            {
                List<Transfer> list = new List<Transfer>();
                ulong CustomerNumber = GetAuthorizedCustomerNumber();
                list = filter.GetList(User, CustomerNumber);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Transfer> GetTransfersForHB(TransferFilter filter)
        {
            try
            {
                List<Transfer> list = new List<Transfer>();
                list = filter.GetList(User, CustomerNumber: 0);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ReceivedBankMailTransfer> GetReceivedBankMailTransfers(TransferFilter filter)
        {
            try
            {
                List<ReceivedBankMailTransfer> list = new List<ReceivedBankMailTransfer>();
                ulong CustomerNumber = GetAuthorizedCustomerNumber();
                list = filter.GetReceivedBankMailTransfersList(User, CustomerNumber);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ActionResult SaveTransferbyCall(TransferByCall transfer)
        {
            try
            {
                return transfer.Save(User);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SendTransfeerCallForPay(ulong transferID)
        {
            try
            {
                DateTime OperationDate = Utility.GetCurrentOperDay();
                return TransferByCall.SendTransfeerCallForPay(transferID, User, OperationDate);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public TransferByCall GetTransferbyCall(long Id)
        {
            try
            {
                return TransferByCall.Get(Id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<DepositRepayment> GetDepositRepayments(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositRepayments(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<LoanRepaymentGrafik> GetLoanGrafik(Loan loan)
        {
            try
            {
                List<LoanRepaymentGrafik> loanGrafik = loan.GetLoanGrafik();
                return loanGrafik;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public LoanRepaymentGrafik GetLoanNextRepayment(Loan loan)
        {
            try
            {
                LoanRepaymentGrafik loanNextRepayment = loan.GetLoanNextRepayment();
                return loanNextRepayment;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public List<LoanRepaymentGrafik> GetLoanInceptiveGrafik(Loan loan, ulong customerNumber)
        {
            try
            {
                List<LoanRepaymentGrafik> loanGrafik = new List<LoanRepaymentGrafik>();
                loan = Loan.GetLoan((ulong)loan.ProductId, AuthorizedCustomer.CustomerNumber);
                if (loan == null)
                {
                    loanGrafik = null;
                }
                else
                    loanGrafik = loan.GetLoanInceptiveGrafik(customerNumber);
                return loanGrafik;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<ulong> GetDepositJointCustomers(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositJointCustomers(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<PeriodicTransferHistory> GetPeriodicTransferHistory(long ProductId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                PeriodicTransfer transfer = new PeriodicTransfer();
                return transfer.GetHistory(ProductId, dateFrom, dateTo);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public HasHB HasACBAOnline()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.HasACBAOnline();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public CreditLine GetCreditLine(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCreditLine(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<CreditLine> GetCreditLines(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCreditLines(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CreditLine> GetCardClosedCreditLines(string cardNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardClosedCreditLines(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CreditLineGrafik> GetCreditLineGrafik(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCreditLineGrafik(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<DepositCase> GetDepositCases(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositCases(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DepositCase GetDepositCase(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositCase(productId);
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOrderHistory(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CustomerDebts> GetCustomerDebts(ulong customerNumber)
        {
            try
            {
                Customer customer = new Customer(customerNumber, (Languages)Language);
                return customer.GetCustomerDebts();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Account GetCurrentAccount(string accountNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCurrentAccount(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveDepositOrder(DepositOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDepositOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveAndApproveDepositTermination(DepositTerminationOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDepositTermination(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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

                Customer customer = CreateCustomer();
                if (order.TransferID == 0 && order.SwiftMessageID == 0)
                    InitOrder(order);
                else if (order.SwiftMessageID == 0)
                    InitOrderForTransfer(order);
                else
                    InitOrderForSwiftMessage(order);
                return customer.SaveAndApprovePaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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

                Customer customer = CreateCustomer();
                InitOrder(order);
                order.Code = order.Code.Trim();
                return customer.SaveAndApproveUtilityPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveReferenceOrder(ReferenceOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveReferenceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveChequeBookOrder(ChequeBookOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveChequeBookOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveCashOrder(CashOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCashOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveSwiftCopyOrder(SwiftCopyOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveSwiftCopyOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveStatmentByEmailOrder(StatmentByEmailOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveStatmentByEmailOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveCustomerDataOrder(CustomerDataOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCustomerDataOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<KeyValuePair<ulong, double>> GetAccountJointCustomers(string accountNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountJointCustomers(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult MakeSwiftStatement(ulong messageUniqNumber, DateTime dateStatement, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                SwiftMessage swiftMessage = SwiftMessage.GetSwiftMessage(messageUniqNumber);
                ActionResult result = swiftMessage.MakeSwiftStatement(dateStatement, dateFrom, dateTo);
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public SwiftMessage GenerateNewSwiftMessageByPeriodicTransfer(DateTime registrationDate, ulong periodicTransferId)
        {
            try
            {
                return SwiftMessage.GenerateNewSwiftMessageByPeriodicTransfer(registrationDate, User.userID, periodicTransferId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult GenerateAndMakeSwitMessageByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo, ulong periodicTransferId)
        {
            try
            {
                if (User != null)
                {
                    return SwiftMessage.GenerateAndMakeSwitMessageByPeriodicTransfer(statementDate, dateFrom, dateTo, User.userID, periodicTransferId);
                }
                else
                {
                    ActionResult result = new ActionResult();
                    result.ResultCode = ResultCode.NotAutorized;
                    return result;
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult GenerateAndMakeSwiftMessagesByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                if (User != null)
                {
                    return SwiftMessage.GenerateAndMakeSwiftMessagesByPeriodicTransfer(statementDate, dateFrom, dateTo, User.userID);
                }
                else
                {
                    ActionResult result = new ActionResult();
                    result.ResultCode = ResultCode.NotAutorized;
                    return result;
                }

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
                List<TransferCallContract> list = TransferCallContract.GetTransferCallContracts(customerNumber, accountNumber, cardNumber);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        //public List<Tuple<string, string>> GetCustomerAuthorizationData(ulong customerNumber)
        //{
        //    Dictionary<string, string> authorizationData = new Dictionary<string, string>();
        //    List<Tuple<string, string>> result = new List<Tuple<string, string>>();
        //    try
        //    {
        //        using (PhoneBankingSecurityServiceClient proxy = new PhoneBankingSecurityServiceClient())
        //        {
        //            authorizationData = proxy.GetCustomerAuthorizationData(customerNumber);
        //        }
        //        foreach (var item in authorizationData)
        //        {
        //            Tuple<string, string> listItem = new Tuple<string, string>(Utility.ConvertAnsiToUnicode(item.Key), Utility.ConvertAnsiToUnicode(item.Value));
        //            result.Add(listItem);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        throw new FaultException(Resourse.InternalError);
        //    }
        //    return result;
        //}

        public List<OverdueDetail> GetOverdueDetails()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOverdueDetails();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<OverdueDetail> GetCurrentProductOverdueDetails(long productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCurrentProductOverdueDetails(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public void GenerateLoanOverdueUpdate(long productId, DateTime startDate, DateTime? endDate, string updateReason, short setNumber)
        {
            try
            {
                OverdueDetail.GenerateLoanOverdueUpdate(productId, startDate, endDate, updateReason, setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AuthorizedCustomer AuthorizeCustomer(ulong customerNumber, string authorizedUserSessionToken)
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
                customer.SaveExternalBankingLogHistory(1, User.userID, Source, customerData.SessionID, ClientIp, " ", authorizedUserSessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(ex.Message);
            }


            return result;
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
        //public bool InitNew(string authorizedCustomerSessionID, byte language, string clientIp, string authorizeduserSessionID, SourceType source, ServiceType customerServiceType = ServiceType.CustomerService)
        //{
        //    try
        //    {

        //        bool checkCustomerSession = true;


        //        AuthorizedUser authorizedUser = AuthorizeUserBySessionToken(authorizeduserSessionID);

        //        xbs.User authorizedXBSUser = AuthorizationService.InitXBSUser(authorizedUser);

        //        if (customerServiceType == ServiceType.NonCustomerService)
        //            authorizedCustomerSessionID = SESSIONID_NonCustomerService;

        //        if (authorizedCustomerSessionID != "")
        //        {
        //            CheckCustomerAuthorization(authorizedCustomerSessionID);
        //            if (AuthorizedCustomer.SessionID == Guid.Empty.ToString())
        //            {
        //                checkCustomerSession = false;
        //            }
        //        }

        //        if (source == SourceType.Bank)
        //        {
        //            if (AuthorizedCustomer != null)
        //            {
        //                AuthorizedCustomer.OneTransactionLimit = 10000000000;
        //                AuthorizedCustomer.DailyTransactionsLimit = 10000000000;
        //            }
        //        }

        //        CustomerServiceType = customerServiceType;
        //        User = authorizedXBSUser;
        //        Source = source;
        //        Language = language;
        //        ClientIp = clientIp;
        //        return checkCustomerSession;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        throw new FaultException(Resourse.InternalError);
        //    }

        //}

        public ulong GetAuthorizedCustomerNumber()
        {
            ulong result = 0;

            if (AuthorizedCustomer != null)
                result = AuthorizedCustomer.CustomerNumber;

            return result;

        }

        public TransferCallContract GetContractDetails(long contractId)
        {
            try
            {
                TransferCallContract contract = TransferCallContract.GetContractDetails(contractId);
                return contract;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public void SaveExternalBankingLogInHistory()
        {
            try
            {
                Customer customer = new Customer();
                if (AuthorizedCustomer != null)
                {
                    customer.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                    customer.SaveExternalBankingLogHistory(1, User.userID, Source, AuthorizedCustomer.SessionID, ClientIp);
                }
                else
                {
                    customer.SaveExternalBankingLogHistory(1, 0, Source, "EXPIRED SESSION", ClientIp);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void SaveExternalBankingLogOutHistory(string authorizedUserSessionToken)
        {
            try
            {
                Customer customer = new Customer();
                if (AuthorizedCustomer != null)
                {
                    customer.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                    customer.SaveExternalBankingLogHistory(2, User.userID, Source, AuthorizedCustomer.SessionID, ClientIp, " ", authorizedUserSessionToken);
                }
                else
                {
                    customer.SaveExternalBankingLogHistory(2, 0, Source, "EXPIRED SESSION", ClientIp, " ", authorizedUserSessionToken);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        //pba delete
        //public ActionResult SendAutorizationSMS()
        //{
        //    try
        //    {
        //        ActionResult result = new ActionResult();

        //public ActionResult SendAutorizationSMS()
        //{
        //    try
        //    {
        //        ActionResult result = new ActionResult();

        //        string smsCode = "";
        //        bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());
        //        if (isTestVersion)
        //        {
        //            result.ResultCode = ResultCode.Normal;
        //            result.Id = 1;
        //            return result;
        //        }

        //        using (PhoneBankingSecurityServiceClient proxy = new PhoneBankingSecurityServiceClient())
        //        {
        //            smsCode = proxy.GetCode(AuthorizedCustomer.SessionID);
        //        }

        //        using (PhoneBankingSecurityServiceClient proxy = new PhoneBankingSecurityServiceClient())
        //        {
        //            smsCode = proxy.GetCode(AuthorizedCustomer.SessionID);
        //        }

        //        if (smsCode == null || smsCode == "")
        //        {
        //            result.ResultCode = ResultCode.Failed;
        //            result.Errors = new List<ActionError>();
        //            ActionError error = new ActionError();
        //            error.Description = "SMS կոդը հասանելի չէ";
        //            result.Errors.Add(error);
        //            return result;
        //        }

        //        if (smsCode == null || smsCode == "")
        //        {
        //            result.ResultCode = ResultCode.Failed;
        //            result.Errors = new List<ActionError>();
        //            ActionError error = new ActionError();
        //            error.Description = "SMS կոդը հասանելի չէ";
        //            result.Errors.Add(error);
        //            return result;
        //        }

        //        result = SMSMessage.SendPhoneBankingAuthorizationSMSMessage(AuthorizedCustomer.CustomerNumber, smsCode, User);

        //        result = SMSMessage.SendPhoneBankingAuthorizationSMSMessage(AuthorizedCustomer.CustomerNumber, smsCode, User);

        //        if (result.ResultCode != ResultCode.Normal)
        //        {
        //            result.Errors = new List<ActionError>();
        //            ActionError error = new ActionError();
        //            error.Description = "Հնարավոր չէ ուղարկել:";
        //            result.Errors.Add(error);
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        throw new FaultException(Resourse.InternalError);
        //    }
        //}

        //public ActionResult VerifyAuthorizationSMS(string smsCode)
        //{
        //    try
        //    {
        //        ActionResult result = new ActionResult();
        //        bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());
        //        if (isTestVersion)
        //        {
        //            result.ResultCode = ResultCode.Normal;
        //            return result;
        //        }
        //        bool isValid = false;
        //        using (PhoneBankingSecurityServiceClient proxy = new PhoneBankingSecurityServiceClient())
        //        {
        //            isValid = proxy.VerifyCode(smsCode, AuthorizedCustomer.SessionID);
        //        }

        //        if (isValid)
        //            result.ResultCode = ResultCode.Normal;
        //        else
        //        {
        //            result.ResultCode = ResultCode.Failed;
        //            result.Errors = new List<ActionError>();
        //            ActionError error = new ActionError();
        //            error.Description = "Մուտքագրված կոդը սխալ է:";
        //            result.Errors.Add(error);
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        throw new FaultException(Resourse.InternalError);
        //    }
        //}

        public List<ExchangeRate> GetExchangeRates()
        {
            try
            {
                return ExchangeRate.GetExchangeRates();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetCBKursForDate(DateTime date, string currency)
        {
            try
            {
                return Utility.GetCBKursForDate(date, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetMRFeeAMD(string cardNumber)
        {
            try
            {
                return Card.GetMRFee(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetCardTotalDebt(string cardNumber)
        {
            try
            {
                return Card.GetCardTotalDebt(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardServiceFee GetCardServiceFee(ulong productId)
        {
            CardServiceFee cardServiceFee;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cardServiceFee = Card.GetCardServiceFee(productId);
                return cardServiceFee;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetPetTurk(long productId)
        {
            try
            {
                return Card.GetPetTurk(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<ProductDocument> GetProductDocuments(ulong productId)
        {
            try
            {
                return ProductDocument.GetProductDocuments(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }
        public List<xbs.AttachmentDocument> GetHBAttachmentsInfo(ulong documentId)
        {
            try
            {
                return ProductDocument.GetHBAttachmentsInfo(documentId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public byte[] GetOneHBAttachment(ulong id)
        {
            try
            {
                return ProductDocument.GetOneHBAttachment(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public DateTime GetNextOperDay()
        {
            try
            {
                return Utility.GetNextOperDay();
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

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveMatureOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public MatureOrder GetMatureOrder(long ID)
        {
            MatureOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetMatureOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }





        public ActionResult SaveAndApprovePlasticCardOrder(PlasticCardOrder cardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardOrder);
                return customer.SaveAndApprovePlasticCardOrder(cardOrder, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ActionResult SaveAccountOrder(AccountOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAccountOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public AccountOrder GetAccountOrder(long ID)
        {
            AccountOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAccountOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveAccountOrder(AccountOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                ActionResult result = new ActionResult();
                if (order.Type == OrderType.CurrentAccountReOpen)
                {
                    AccountReOpenOrder reOpenOrder = GetAccountReOpenOrder(order.Id);
                    result = ApproveAccountReOpenOrder(reOpenOrder);
                }
                else
                {
                    InitOrder(order);
                    result = customer.ApproveAccountOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                    if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                    {
                        try
                        {
                            OrderQuality quality = GetOrderQualityByDocID(order.Id);
                            if (quality != OrderQuality.Approved)
                            {
                                ConfirmOrderOnline(order.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex);
                            //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveAccountOrder(AccountOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SavePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePeriodicUtilityPaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }
        public ActionResult SavePeriodicPaymentOrder(PeriodicPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePeriodicPaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }
        public ActionResult SaveAndAprovePeriodicPaymentOrder(PeriodicPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndAprovePeriodicPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }
        public ActionResult SaveAndAprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndAprovePeriodicUtilityPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }




        public ActionResult SaveMatureOrder(MatureOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveMatureOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveMatureOrder(MatureOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveMatureOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsPoliceAccount(string accountNumber)
        {
            try
            {
                return Account.IsPoliceAccount(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool CheckAccountForPSN(string accountNumber)
        {
            try
            {
                return Account.CheckAccountForPSN(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public MembershipRewards GetCardMembershipRewards(string cardNumber)
        {
            MembershipRewards mr;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                Card card = customer.GetCard(cardNumber);
                if (card != null)
                {
                    mr = card.GetCardMembershipRewards();
                }
                else
                {
                    mr = null;
                }


                return mr;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public MembershipRewardsOrder GetCardMembershipRewardsOrder(long ID)
        {
            MembershipRewardsOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardMembershipRewardsOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<MembershipRewardsStatusHistory> GetCardMembershipRewardsStatusHistory(string cardNumber)
        {
            List<MembershipRewardsStatusHistory> mr = new List<MembershipRewardsStatusHistory>();
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                Card card = customer.GetCard(cardNumber);
                if (card != null)
                {
                    mr = card.GetCardMembershipRewardsStatusHistory();
                }
                else
                {
                    mr = null;
                }


                return mr;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<MembershipRewardsBonusHistory> GetCardMembershipRewardsBonusHistory(string cardNumber, DateTime startDate, DateTime endDate)
        {
            List<MembershipRewardsBonusHistory> mr = new List<MembershipRewardsBonusHistory>();
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                Card card = customer.GetCard(cardNumber);
                if (card != null)
                {
                    mr = card.GetCardMembershipRewardsBonusHistory(startDate, endDate);
                }
                else
                {
                    mr = null;
                }


                return mr;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Guarantee GetGuarantee(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetGuarantee(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Guarantee> GetGuarantees(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetGuarantees(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Accreditive GetAccreditive(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccreditive(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Accreditive> GetAccreditives(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccreditives(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PaidGuarantee GetPaidGuarantee(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidGuarantee(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PaidGuarantee> GetPaidGuarantees(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidGuarantees(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PaidAccreditive GetPaidAccreditive(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidAccreditive(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PaidAccreditive> GetPaidAccreditives(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidAccreditives(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Factoring GetFactoring(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFactoring(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Factoring> GetFactorings(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFactorings(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<PaidFactoring> GetPaidFactorings(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidFactorings(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PaidFactoring GetPaidFactoring(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidFactoring(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApprovePeriodicPaymentOrder(Order order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicUtilityPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApprovePeriodicPaymentOrder(PeriodicPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndAprovePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndAprovePeriodicBudgetPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }
        }

        public ActionResult ApprovePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicBudgetPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SavePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePeriodicBudgetPaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public ActionResult CloseAccountOrder(AccountClosingOrder order)
        {
            try
            {
                ActionResult result = new ActionResult();
                Customer customer = CreateCustomer();
                result = customer.CloseAccountOrder(order, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<SearchAccountResult> GetSearchedAccounts(SearchAccounts searchParams)
        {
            try
            {
                List<SearchAccountResult> GetSearchedAccountsList = SearchAccounts.GetSearchedAccounts(searchParams, User);
                return GetSearchedAccountsList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveAccountClosing(AccountClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveAccountClosing(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveAccountClosing(AccountClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountClosing(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountClosingOrder GetAccountClosingOrder(long ID)
        {
            AccountClosingOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAccountClosingOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<SearchCardResult> GetSearchedCards(SearchCards searchParams)
        {
            try
            {
                List<SearchCardResult> searchResult = SearchCards.Search(searchParams);
                return searchResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<SearchSwiftCodes> GetSearchedSwiftCodes(SearchSwiftCodes searchParams)
        {
            try
            {
                List<SearchSwiftCodes> searchResult = SearchSwiftCodes.Search(searchParams);
                return searchResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveCardClosingOrder(CardClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCardClosingOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }
        public CardClosingOrder GetCardClosingOrder(long ID)
        {
            CardClosingOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardClosingOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveCardClosingOrder(CardClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveCardClosingOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName, ClientIp);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCardClosingOrder(CardClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardClosingOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetCardClosingWarnings(ulong productId)
        {
            try
            {
                Customer customer = CreateCustomer();

                return customer.GetCardClosingWarnings(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetCredentialClosingWarnings(ulong assignId)
        {
            try
            {
                Customer customer = CreateCustomer();

                return customer.GetCredentialClosingWarnings(assignId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PeriodicPaymentOrder GetPeriodicPaymentOrder(long ID)
        {
            PeriodicPaymentOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPeriodicPaymentOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public PeriodicBudgetPaymentOrder GetPeriodicBudgetPaymentOrder(long ID)
        {
            PeriodicBudgetPaymentOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPeriodicBudgetPaymentOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public PeriodicUtilityPaymentOrder GetPeriodicUtilityPaymentOrder(long ID)
        {
            PeriodicUtilityPaymentOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPeriodicUtilityPaymentOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
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


        /// <summary>
        /// Լրացնում հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrderForTransfer(Order order)
        {
            order.FilialCode = User.filialCode;
            if (order.OPPerson != null)
            {
                order.CustomerNumber = order.OPPerson.CustomerNumber;
            }
            order.Source = Source;
            order.user = User;
            order.OperationDate = Utility.GetCurrentOperDay();
        }

        /// <summary>
        /// Լրացնում հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrderForSwiftMessage(Order order)
        {
            order.FilialCode = User.filialCode;
            order.Source = Source;
            order.user = User;
            order.OperationDate = Utility.GetCurrentOperDay();
        }
        /// <summary>
        /// Լրացնում հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrderForLoanEquipment(Order order)
        {
            order.FilialCode = User.filialCode;
            order.Source = Source;
            order.user = User;
            order.OperationDate = Utility.GetCurrentOperDay();
        }
        public List<string> GetReceiverAccountWarnings(string accountNumber)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetReceiverAccountWarnings(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ActionResult SavePeriodicTerminationOrder(PeriodicTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePeriodicTerminationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }
        public PeriodicTerminationOrder GetPeriodicTerminationOrder(long ID)
        {
            PeriodicTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPeriodicTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApprovePeriodicTerminationOrder(PeriodicTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicTerminationOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApprovePeriodicTerminationOrder(PeriodicTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePeriodicTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveAccountReOpenOrder(AccountReOpenOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountReOpenOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public AccountReOpenOrder GetAccountReOpenOrder(long ID)
        {
            AccountReOpenOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAccountReOpenOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<AdditionalDetails> GetAccountAdditionalDetails(string accountNumber)
        {
            List<AdditionalDetails> additionalDetails;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                additionalDetails = customer.GetAccountAdditionalDetails(accountNumber);
                return additionalDetails;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAccountAdditionsTypes()
        {
            Dictionary<string, string> additionsTypes;
            try
            {

                additionsTypes = AccountDataChangeOrder.GetAccountAdditionsTypes(User);
                return additionsTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<Account> GetAccountsForNewDeposit(DepositOrder order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                return customer.GetAccountsForNewDeposit(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public string CreateSerialNumber(int currencyCode, byte operationType)
        {
            try
            {
                PaymentOrder paymentOrder = new PaymentOrder();
                return paymentOrder.CreateSerialNumber(currencyCode, operationType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            try
            {
                ActionResult result = new ActionResult();
                Customer customer = CreateCustomer();
                InitOrder(order);
                result = customer.SaveAndApproveCreditLineTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType, ushort filialCode)
        {
            try
            {
                return Order.GenerateNewOrderNumber(orderNumberType, filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetAccountOpenWarnings()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetAccountOpenWarnings();
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
        public string GetCommunalPaymentDescription(SearchCommunal searchCommunal)
        {
            try
            {

                return searchCommunal.GetCommunalPaymentDescription();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<OPPerson> GetOrderOPPersons(string accountNumber, OrderType orderType)
        {
            try
            {
                Customer customer;
                if (orderType == OrderType.CashBookSurPlusDeficit)
                {
                    ulong customerNumber;
                    Account account = new Account(accountNumber);
                    customerNumber = account.GetAccountCustomerNumber();
                    customer = new Customer(customerNumber, (Languages)Language);
                }
                else
                {
                    customer = CreateCustomer();
                }
                return customer.GetOrderOPPersons(accountNumber, orderType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetAccountStatementDeliveryType(string accountNumber)
        {
            int AccountStatementDeliveryType;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                AccountStatementDeliveryType = customer.GetAccountStatementDeliveryType(accountNumber);
                return AccountStatementDeliveryType;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveAccountDataChangeOrder(AccountDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetCustomerDocumentWarnings(ulong customerNumber)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetCustomerDocumentWarnings(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetThreeMonthLoanRate(ulong productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetThreeMonthLoanRate(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool IsPrepaidArmenTell(SearchCommunal searchCommunal)
        {
            try
            {
                return SearchCommunal.IsPrepaidArmenTel(searchCommunal);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<OrderAttachment> GetOrderAttachments(long orderId)
        {
            try
            {

                return Order.GetOrderAttachments(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public OrderAttachment GetOrderAttachment(string attachmentId)
        {
            try
            {

                return Order.GetOrderAttachment(attachmentId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public OrderAttachment GetTransferAttachmentInfo(long Id)
        {
            try
            {

                return Transfer.GetTransferAttachmentInfo(Id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public OrderAttachment GetTransferAttachment(ulong attachmentId)
        {
            try
            {

                return Transfer.GetTransferAttachment(attachmentId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountDataChangeOrder GetAccountDataChangeOrder(long ID)
        {
            AccountDataChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAccountDataChageOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetLoanMatureCapitalPenalty(MatureOrder order, ExternalBanking.ACBAServiceReference.User user)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetLoanMatureCapitalPenalty(order, user);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Tuple<bool, string> IsBigAmountForPaymentOrder(PaymentOrder order)
        {
            try
            {
                InitOrder(order);
                return PaymentOrder.IsBigAmount(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Tuple<bool, string> IsBigAmountForCurrencyExchangeOrder(CurrencyExchangeOrder order)
        {
            try
            {
                InitOrder(order);
                return PaymentOrder.IsBigAmount(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ulong GetAccountCustomerNumber(Account account)
        {
            try
            {
                return account.GetAccountCustomerNumber();
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
                Customer customer = CreateCustomer();
                return customer.GetOrderServiceFee(type, urgent);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult CheckForTransactionLimit(Order order)
        {
            try
            {
                return User.CheckForTransactionLimit(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveTransitPaymentOrder(TransitPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveTransitPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public TransitPaymentOrder GetTransitPaymentOrder(long ID)
        {
            TransitPaymentOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetTransitPaymentOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetLoanCalculatedRest(Loan loan, ulong customerNumber, short matureType)
        {
            try
            {

                return Loan.GetLoanCalculatedRest(loan, customerNumber, (MatureType)matureType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetPaymentOrderDescription(PaymentOrder order, ulong customerNumber)
        {
            try
            {
                return PaymentOrder.GetPaymentOrderDescription(order, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveServicePaymentOrder(ServicePaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveServicePaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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
                Customer customer = CreateCustomer();
                if (order.TransferID == 0 && order.SwiftMessageID == 0)
                    InitOrder(order);
                else
                    InitOrderForTransfer(order);
                return customer.SaveAndApproveCurrencyExchangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveCurrencyExchangeOrder(CurrencyExchangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCurrencyExchangeOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public CurrencyExchangeOrder GetShortChangeAmount(CurrencyExchangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.GetShortChangeAmount(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetCustomerCashOuts(string currency)
        {
            double cashOut;
            try
            {
                Customer customer = CreateCustomer();
                cashOut = customer.GetCustomerCashOuts(currency);
                return cashOut;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ConfirmOrder(long orderID)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.ConfirmOrder(orderID);
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
                return AccountFreezeDetails.GetAccountFreezeHistory(accountNumber, freezeStatus, reasonId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountFreezeDetails GetAccountFreezeDetails(string freezeId)
        {
            try
            {
                return AccountFreezeDetails.GetAccountFreezeDetails(freezeId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public Account GetOperationSystemAccount(Order order, OrderAccountType accountType, string operationCurrency, ushort filialCode = 0, string utilityBranch = "", ulong customerNumber = 0, ushort customerType = 0)
        {
            try
            {
                return Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, accountType), operationCurrency, filialCode, customerType, "0", utilityBranch, customerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Account GetTransitCurrencyExchangeOrderSystemAccount(TransitCurrencyExchangeOrder order, OrderAccountType accountType, string operationCurrency)
        {
            try
            {
                InitOrder(order);
                return TransitCurrencyExchangeOrder.GetTransitCurrencyExchangeOrderSystemAccount(order, accountType, operationCurrency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        //[AllowAnonymous]
        public infsec.AuthorizedUser AuthorizeUserBySessionToken(string sessionToken)
        {
            try
            {
                return AuthorizationService.AuthorizeUserBySessionToken(sessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ACBALibrary.User InitUser(infsec.AuthorizedUser authUser)
        {
            try
            {
                return AuthorizationService.InitUser(authUser);
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
                return CurrencyExchangeOrder.GetCrossConvertationVariant(debitCurrency, creditCurrency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Account GetRAFoundAccount()
        {
            try
            {
                return Account.GetRAFoundAccount();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public int GetCardType(string cardNumber)
        {
            try
            {
                Customer customer = CreateCustomer();

                return customer.GetCardType(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveAndApproveCashPosPaymentOrder(CashPosPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCashPosPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public ActionResult SaveAndApproveAccountFreezeOrder(AccountFreezeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountFreezeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountFreezeOrder GetAccountFreezeOrder(long ID)
        {
            AccountFreezeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAccountFreezeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetCashPosPaymentOrderFee(CashPosPaymentOrder order, int feeType)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.GetPaymentOrderFee(order, feeType);
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
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveLoanProductOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public LoanProductOrder GetLoanOrder(long ID)
        {
            LoanProductOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetLoanOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanProductOrder GetCreditLineOrder(long ID)
        {
            LoanProductOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCreditLineOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveLoanProductOrder(LoanProductOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveLoanProductOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveLoanProductOrder(LoanProductOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveLoanProductOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetCustomerAvailableAmount(string currency)
        {
            double availableAmount;
            try
            {
                Customer customer = CreateCustomer();
                availableAmount = Customer.GetCustomerAvailableAmount(customer.CustomerNumber, currency);
                return availableAmount;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLoanProductInterestRate(LoanProductOrder order, string cardNumber)
        {
            double interestRate;
            try
            {
                interestRate = LoanProductOrder.GetInterestRate(order, cardNumber);
                return interestRate;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveAccountUnfreezeOrder(AccountUnfreezeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountUnfreezeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountUnfreezeOrder GetAccountUnfreezeOrder(long ID)
        {
            AccountUnfreezeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAccountUnfreezeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveLoanProductActivationOrder(LoanProductActivationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveLoanProductActivationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public LoanProductActivationOrder GetLoanProductActivationOrder(long ID)
        {
            LoanProductActivationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetLoanProductActivationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveLoanProductActivationOrder(LoanProductActivationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveLoanProductActivationOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                return result;
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
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveLoanProductActivationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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
                Customer customer = CreateCustomer();
                return FeeForServiceProvidedOrder.GetServiceFee(customer.CustomerNumber, orderType, serviceType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveFeeForServiceProvidedOrder(FeeForServiceProvidedOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveFeeForServiceProvidedOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public HasHB HasPhoneBanking()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.HasPhoneBanking();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public FeeForServiceProvidedOrder GetFeeForServiceProvidedOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFeeForServiceProvidedOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CashPosPaymentOrder GetCashPosPaymentOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCashPosPaymentOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardUnpaidPercentPaymentOrder GetCardUnpaidPercentPaymentOrder(long ID)
        {
            CardUnpaidPercentPaymentOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardUnpaidPercentPaymentOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardUnpaidPercentPaymentOrder(CardUnpaidPercentPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardUnpaidPercentPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Order> GetConfirmRequiredOrders(string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1)
        {
            try
            {
                groups = "";
                List<XBUserGroup> XBUserGroups = XBUserGroup.GetXBUserGroups(userName);
                XBUserGroups.ForEach(m =>
                    groups += m.Id + ","
                );
                if (!string.IsNullOrEmpty(groups))
                {
                    groups = "(" + groups.Substring(0, groups.Length - 1) + ")";
                }
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetConfirmRequiredOrders(AuthorizedCustomer.CustomerNumber, userName, subTypeId, startDate, endDate, langId, receiverName, account, period, groups, quality);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public List<Provision> GetProductProvisions(ulong productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetProductProvisions(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveRemovalOrder(RemovalOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveRemovalOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public RemovalOrder GetRemovalOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetRemovalOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<SearchInternationalTransfer> GetSearchedInternationalTransfers(SearchInternationalTransfer searchParams)
        {
            try
            {
                List<SearchInternationalTransfer> searchResult = SearchInternationalTransfer.Search(searchParams);
                return searchResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<SearchReceivedTransfer> GetSearchedReceivedTransfers(SearchReceivedTransfer searchParams)
        {
            try
            {
                List<SearchReceivedTransfer> searchResult = SearchReceivedTransfer.Search(searchParams, User.filialCode);
                return searchResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<SearchTransferBankMail> GetSearchedTransfersBankMail(SearchTransferBankMail searchParams)
        {
            try
            {
                List<SearchTransferBankMail> searchResult = SearchTransferBankMail.Search(searchParams);
                return searchResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<SearchBudgetAccount> GetSearchedBudgetAccount(SearchBudgetAccount searchParams)
        {
            try
            {
                List<SearchBudgetAccount> searchResult = SearchBudgetAccount.Search(searchParams);
                return searchResult;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ulong> GetProvisionOwners(ulong productId)
        {
            try
            {
                return Provision.GetProvisionOwners(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanMainContract GetLoanMainContract(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanMainContract(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsOurCard(string cardNumber)
        {
            try
            {
                return Card.IsOurCard(cardNumber);
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

        public SourceType GetDepositSource(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositSource(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public SourceType GetAccountSource(string accountNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountSource(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<LoanMainContract> GetCreditLineMainContract()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCreditLineMainContract();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<LoanProductProlongation> GetLoanProductProlongations(ulong productId)
        {
            try
            {
                Customer customer = new Customer();
                return customer.GetLoanProductProlongations(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public infsec.UserAccessForCustomer GetUserAccessForCustomer(string userSessionToken, string customerSessionToken)
        {
            try
            {
                return AuthorizationService.GetUserAccessForCustomer(userSessionToken, customerSessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveChequeBookReceiveOrder(ChequeBookReceiveOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveChequeBookReceiveOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public List<ProductOtherFee> GetProductOtherFees(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetProductOtherFees(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ChequeBookReceiveOrder GetChequeBookReceiveOrder(long ID)
        {
            ChequeBookReceiveOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetChequeBookReceiveOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<Claim> GetProductClaims(ulong productId, short productType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetProductClaims(productId, productType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ClaimEvent> GetClaimEvents(int claimNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetClaimEvents(claimNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Tax GetTax(int claimNumber, int eventNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetTax(claimNumber, eventNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Credential> GetCredentials(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCustomerCredentialsList(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetAccountsForCredential(int operationType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountsForCredential(operationType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCredentialOrder(CredentialOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCredentialOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCredentialOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveCredentialOrder(CredentialOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveCredentialOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCredentialTerminationOrder(CredentialTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCredentialTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CardServiceFeeGrafik> GetCardServiceFeeGrafik(ulong productId)
        {

            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardServiceFeeGrafik(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool HasAccountPensionApplication(string accountNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.HasAccountPensionApplication(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardTariffContract GetCardTariffContract(long tariffID)
        {
            Customer customer = CreateCustomer();
            return customer.GetCardTariffContract(tariffID);
        }

        public Account GetFeeForServiceProvidedOrderCreditAccount(FeeForServiceProvidedOrder order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                InitOrder(order);
                return customer.GetFeeForServiceProvidedOrderCreditAccount(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CardTariff GetCardTariff(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardTariff(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardStatus GetCardStatus(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardStatus(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Account GetOperationSystemAccountForFee(Order order, short feeType)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return OperationAccountHelper.GetOperationSystemAccountForFee(order, feeType);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        //[AllowAnonymous]
        public DateTime GetCurrentOperDay()
        {
            try
            {
                return Utility.GetCurrentOperDay();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLoanProductActivationFee(ulong productId, short withTax)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanProductActivationFee(productId, withTax);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public bool IsTransferFromBusinessmanToOwnerAccount(string debitAccountNumber, string creditAccountNumber)
        {
            try
            {
                return PaymentOrder.IsTransferFromBusinessmanToOwnerAccount(debitAccountNumber, creditAccountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CurrencyExchangeOrder GetCurrencyExchangeOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCurrencyExchangeOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GenerateNextOrderNumber(ulong customerNumber)
        {
            try
            {
                return Order.GenerateNextOrderNumber(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Չեկային գրքույքի վերաբերյալ զգուշացումներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public List<string> GetChequeBookReceiveOrderWarnings(ulong customerNumber, string accountNumber)
        {
            try
            {
                Customer customer = CreateCustomer();

                return customer.GetChequeBookReceiveOrderWarnings(customerNumber, accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CredentialOrder GetCredentialOrder(long ID)
        {
            CredentialOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCredentialOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<AssigneeOperation> GetAllOperations()
        {
            List<AssigneeOperation> operationList;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                operationList = customer.GetAllOperations();
                return operationList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ValidateRenewedOtherTypeCardApplicationForPrint(string cardNumber)
        {
            try
            {
                return Card.ValidateRenewedOtherTypeCardApplicationForPrint(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public bool IsNormCardStatus(string cardNumber)
        {
            try
            {
                return Card.IsNormCardStatus(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsCardRegistered(string cardNumber)
        {
            try
            {
                return Card.IsCardRegistered(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetSpesialPriceMessage(string accountNumber, short additionID)
        {
            try
            {
                return Account.GetSpesialPriceMessage(accountNumber, additionID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetTransitPaymentOrderFee(TransitPaymentOrder order, int feeType)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.GetPaymentOrderFee(order, feeType);
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

        public short GetCustomerSyntheticStatus(ulong customerNumber)
        {
            try
            {

                return Customer.GetCustomerSyntheticStatus(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public double GetAccountReopenFee(short customerType)
        {
            try
            {
                if (customerType == 6)
                    return Utility.GetPriceInfoByIndex(833, "price");
                else
                    return Utility.GetPriceInfoByIndex(834, "price");
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Վերադարձնում է վարկային գծի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CreditLineTerminationOrder GetCreditLineTerminationOrder(long ID)
        {
            CreditLineTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCreditLineTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է ավանդի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public DepositTerminationOrder GetDepositTerminationOrder(long ID)
        {
            DepositTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDepositTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        /// Դադարեցված վարկային գծի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public CreditLine GetClosedCreditLine(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetClosedCreditLine(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetLoanActivationWarnings(long productId, short productType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                return customer.GetLoanActivationWarnings(productId, productType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LoanRepaymentGrafik> GetDecreaseLoanGrafik(CreditLine creditLine)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, Languages.hy);
                creditLine = CreditLine.GetCreditLine((ulong)creditLine.ProductId, AuthorizedCustomer.CustomerNumber);
                return customer.GetDecreaseLoanGrafik(creditLine);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CashBook> GetCashBooks(SearchCashBook searchParams)
        {
            try
            {
                return SearchCashBook.GetCashBooks(searchParams, User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCashBookOrder(CashBookOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCashBookOrder(order, User, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCorrespondentSetNumber()
        {
            try
            {
                if (User.AdvancedOptions["isEncashmentDepartment"] == "1")
                {
                    return CashBook.GetCorrespondentSetNumber(User.filialCode, true);
                }
                else
                {
                    return CashBook.GetCorrespondentSetNumber(User.filialCode);
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult RemoveCashBook(int cashBookID)
        {
            try
            {
                return CashBook.RemoveCashBook(cashBookID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public xbs.CustomerMainData GetCustomerMainData(ulong customerNumber)
        {
            try
            {
                var customer = ACBAOperationService.GetCustomerMainData(customerNumber);
                return customer;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveAccountReOpenOrder(AccountReOpenOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveAccountReOpenOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveOrder(Order order)
        {
            try
            {
                ActionResult result = new ActionResult();

                if (AuthorizedCustomer == null)
                {
                    result.ResultCode = ResultCode.NotAutorized;
                    result.Errors = new List<ActionError>();
                    result.Errors.Add(new ActionError(1111));
                    return result;
                }


                switch (order.Type)
                {
                    case OrderType.RATransfer:
                        {
                            PaymentOrder paymentOrder = GetPaymentOrder(order.Id);
                            result = ApprovePaymentOrder(paymentOrder);
                            break;
                        }
                    case OrderType.Convertation:
                        {
                            PaymentOrder paymentOrder = GetPaymentOrder(order.Id);
                            result = ApprovePaymentOrder(paymentOrder);
                            break;
                        }

                    case OrderType.InternationalTransfer:
                        {
                            InternationalPaymentOrder internationalPaymentOrder = GetInternationalPaymentOrder(order.Id);
                            result = ApproveInternationalPaymentOrder(internationalPaymentOrder);
                            break;
                        }

                    case OrderType.CommunalPayment:
                        {
                            UtilityPaymentOrder utilityPaymentOrder = GetUtilityPaymentOrder(order.Id);
                            result = ApproveUtilityPaymentOrder(utilityPaymentOrder);
                            break;
                        }

                    case OrderType.ReferenceOrder:
                        {
                            ReferenceOrder referenceOrder = GetReferenceOrder(order.Id);
                            result = ApproveReferenceOrder(referenceOrder);
                            break;
                        }

                    case OrderType.ChequeBookOrder:
                        {
                            ChequeBookOrder chequeBookOrder = GetChequeBookOrder(order.Id);
                            result = ApproveChequeBookOrder(chequeBookOrder);
                            break;
                        }
                    case OrderType.CashOrder:
                        {
                            CashOrder cashOrder = GetCashOrder(order.Id);
                            result = ApproveCashOrder(cashOrder);
                            break;
                        }
                    case OrderType.StatmentByEmailOrder:
                        {
                            StatmentByEmailOrder statementByEmailOrder = GetStatmentByEmailOrder(order.Id);
                            result = ApproveStatmentByEmailOrder(statementByEmailOrder);
                            break;
                        }
                    case OrderType.SwiftCopyOrder:
                        {
                            SwiftCopyOrder swiftCopyOrder = GetSwiftCopyOrder(order.Id);
                            result = ApproveSwiftCopyOrder(swiftCopyOrder);
                            break;
                        }
                    case OrderType.CustomerDataOrder:
                        {
                            CustomerDataOrder customerDataOrder = GetCustomerDataOrder(order.Id);
                            result = ApproveCustomerDataOrder(customerDataOrder);
                            break;
                        }
                    case OrderType.Deposit:
                        {
                            DepositOrder depositOrder = GetDepositorder(order.Id);
                            result = ApproveDepositOrder(depositOrder);
                            break;
                        }
                    case OrderType.DepositTermination:
                        {
                            DepositTerminationOrder depositTerminationOrder = GetDepositTerminationOrder(order.Id);
                            result = ApproveDepositTermination(depositTerminationOrder);
                            break;
                        }
                    case OrderType.RosterTransfer:
                        {
                            ReestrTransferOrder paymentOrder = GetReestrTransferOrder(order.Id);
                            result = ApprovePaymentOrder(paymentOrder);
                            break;
                        }
                    case OrderType.CurrentAccountOpen:
                        {
                            AccountOrder accountOrder = GetAccountOrder(order.Id);
                            result = ApproveAccountOrder(accountOrder);

                            break;
                        }
                    case OrderType.ThirdPersonDeposit:
                        {
                            AccountOrder accountOrder = GetAccountOrder(order.Id);
                            result = ApproveAccountOrder(accountOrder);

                            break;
                        }
                    case OrderType.PeriodicTransfer:
                        {
                            Order periodicOrder = Order.GetOrder(order.Id);
                            if (periodicOrder.SubType == 1 || periodicOrder.SubType == 3)
                            {
                                PeriodicPaymentOrder periodicPaymentOrder = GetPeriodicPaymentOrder(order.Id);

                                result = ApprovePeriodicPaymentOrder(periodicPaymentOrder);
                                break;
                            }
                            else if (periodicOrder.SubType == 2)
                            {
                                PeriodicUtilityPaymentOrder periodicPaymentOrder = GetPeriodicUtilityPaymentOrder(order.Id);

                                result = ApprovePeriodicUtilityPaymentOrder(periodicPaymentOrder);
                                break;
                            }
                            else if (periodicOrder.SubType == 4)
                            {
                                PeriodicBudgetPaymentOrder periodicPaymentOrder = GetPeriodicBudgetPaymentOrder(order.Id);

                                result = ApprovePeriodicBudgetPaymentOrder(periodicPaymentOrder);
                                break;
                            }
                            else if (periodicOrder.SubType == 5)
                            {
                                PeriodicSwiftStatementOrder periodicPaymentOrder = GetPeriodicSwiftStatementOrder(order.Id);

                                result = ApprovePeriodicSwiftStatementOrder(periodicPaymentOrder);
                                break;
                            }
                            else
                            {

                                result = ApprovePeriodicPaymentOrder(periodicOrder);
                                break;
                            }
                        }
                    case OrderType.CreditSecureDeposit:
                        {
                            LoanProductOrder loanProductOrder = GetLoanOrder(order.Id);
                            result = ApproveLoanProductOrder(loanProductOrder);
                            break;
                        }
                    case OrderType.CreditLineSecureDeposit:
                        {
                            LoanProductOrder loanProductOrder = GetCreditLineOrder(order.Id);
                            result = ApproveLoanProductOrder(loanProductOrder);
                            break;
                        }
                    case OrderType.PeriodicTransferStop:
                        {
                            PeriodicTerminationOrder periodicTerminationOrder = GetPeriodicTerminationOrder(order.Id);
                            result = ApprovePeriodicTerminationOrder(periodicTerminationOrder);
                            break;
                        }
                    case OrderType.CurrentAccountReOpen:
                        {
                            AccountReOpenOrder accountReopenOrder = GetAccountReOpenOrder(order.Id);
                            result = ApproveAccountOrder(accountReopenOrder);
                            break;
                        }
                    case OrderType.LoanMature:
                        {
                            MatureOrder matureOrder = GetMatureOrder(order.Id);
                            result = ApproveMatureOrder(matureOrder);
                            break;
                        }
                    case OrderType.CreditLineMature:
                        {
                            CreditLineTerminationOrder creditLineTerminationOrder = GetCreditLineTerminationOrder(order.Id);
                            result = ApproveCreditLineTerminationOrder(creditLineTerminationOrder);
                            break;
                        }
                    case OrderType.OverdraftRepayment:
                        {
                            MatureOrder matureOrder = GetMatureOrder(order.Id);
                            result = ApproveMatureOrder(matureOrder);
                            break;
                        }
                    case OrderType.CardReRelease:
                        {
                            CardReReleaseOrder cardReReleaseOrder = GetCardReReleaseOrder(order.Id);
                            result = ApproveCardReReleaseOrder(cardReReleaseOrder);
                            break;
                        }
                    case OrderType.FastOverdraftApplication:
                        {
                            LoanProductOrder loanProductOrder = GetCreditLineOrder(order.Id);
                            result = ApproveLoanProductOrder(loanProductOrder);
                            break;
                        }
                    case OrderType.CredentialOrder:
                        {
                            CredentialOrder crorder = GetCredentialOrder(order.Id);
                            result = ApproveCredentialOrder(crorder);
                            break;
                        }
                    case OrderType.ReceivedFastTransferPaymentOrder:
                        {
                            ReceivedFastTransferPaymentOrder receivedFastTransferPaymenOrder = GetReceivedFastTransferPaymentOrder(order.Id, "");
                            result = ApproveFastTransferPaymentOrder(receivedFastTransferPaymenOrder);
                            break;
                        }
                    case OrderType.CardLimitChangeOrder:
                        {
                            CardLimitChangeOrder cardLimitChangeOrder = GetCardLimitChangeOrder(order.Id);
                            result = ApproveCardLimitChangeOrder(cardLimitChangeOrder);
                            break;
                        }
                    case OrderType.CardToCardOrder:
                        {
                            CardToCardOrder cardToCardOrder = GetCardToCardOrder(order.Id);
                            result = ApproveCardToCardOrder(cardToCardOrder);
                            break;
                        }
                    case OrderType.PlasticCardOrder:
                    case OrderType.AttachedPlasticCardOrder:
                    case OrderType.LinkedPlasticCardOrder:
                        {
                            PlasticCardOrder cardOrder = GetPlasticCardOrder(order.Id);
                            cardOrder.Attachments = GetOrderAttachments(order.Id);
                            foreach (var item in cardOrder.Attachments)
                            {
                                item.AttachmentInBase64 = GetOrderAttachmentInBase64(item.Id);
                            }
                            result = ApprovePlasticCardOrder(cardOrder);
                            break;
                        }
                    case OrderType.PeriodicTransferDataChangeOrder:
                        {
                            PeriodicTransferDataChangeOrder periodicOrder = GetPeriodicDataChangeOrder(order.Id);
                            result = ApprovePeriodicDataChangeOrder(periodicOrder);
                            break;
                        }
                    case OrderType.CurrentAccountClose:
                        {
                            AccountClosingOrder accountClosing = GetAccountClosingOrder(order.Id);
                            result = ApproveAccountClosingOrder(accountClosing);
                            break;
                        }
                    case OrderType.ArcaCardsTransactionOrder:
                        {
                            ArcaCardsTransactionOrder transactionOrder = GetArcaCardsTransactionOrder(order.Id);
                            result = ApproveArcaCardsTransactionOrder(transactionOrder);
                            break;
                        }
                    case OrderType.CardToOtherCardsOrder:
                        {
                            CardToOtherCardsOrder cardToOtherCardsOrder = GetCardToOtherCardsOrder(order.Id);
                            result = ApproveCardToOtherCardsOrder(cardToOtherCardsOrder);
                            break;
                        }
                    case OrderType.VisaAlias:
                        {
                            VisaAliasOrder visaAlias = GetVisaAliasOrder(order.Id);
                            result = ApproveVisaAliasOrder(visaAlias);
                            break;
                        }
                    default:
                        break;

                }


                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ActionResult SaveAndApproveTransitCurrencyExchangeOrder(TransitCurrencyExchangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveTransitCurrencyExchangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<SearchLeasingCustomer> GetSearchedLeasingCustomers(SearchLeasingCustomer searchParams)
        {
            try
            {
                List<SearchLeasingCustomer> getSearchedLeasingCustomers = SearchLeasingCustomer.Search(searchParams);
                return getSearchedLeasingCustomers;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LeasingLoan> GetSearchedLeasingLoans(SearchLeasingLoan searchParams)
        {
            try
            {
                List<LeasingLoan> getSearchedLeasingCustomers = SearchLeasingLoan.Search(searchParams);
                return getSearchedLeasingCustomers;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<KeyValuePair<int, double>> GetRest(SearchCashBook searchParams)
        {
            try
            {
                return CashBook.GetRest(searchParams, User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardReReleaseOrder GetCardReReleaseOrder(long ID)
        {
            CardReReleaseOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardReReleaseOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveCardReReleaseOrder(CardReReleaseOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveCardReReleaseOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveCreditLineTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveInternationalPaymentOrder(InternationalPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveInternationalPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult ChangeCashBookStatus(int cashBookID, int newStatus)
        {
            try
            {
                return CashBook.ChangeCashBookStatus(cashBookID, User.userID, newStatus, User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Account GetOperationSystemAccountForLeasing(string operationCurrency, ushort filialCode)
        {
            try
            {
                return OperationAccountHelper.GetOperationSystemAccountForLeasing(operationCurrency, filialCode);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<Account> GetReferenceOrderAccounts()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccounts();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<AccountClosingHistory> GetAccountClosinghistory()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountClosinghistory();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ProblemLoanCalculationsDetail GetProblemLoanCalculationsDetail(int claimNumber, int eventNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetProblemLoanCalculationsDetail(claimNumber, eventNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<VehicleViolationResponse> GetVehicleViolationById(string violationId)
        {
            try
            {
                List<VehicleViolationResponse> responses = VehicleViolationResponse.GetVehicleViolationById(violationId, User);
                return responses;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<VehicleViolationResponse> GetVehicleViolationByPsnVehNum(string psn, string vehNum)
        {
            try
            {
                List<VehicleViolationResponse> responses = VehicleViolationResponse.GetVehicleViolationByPsnVehNum(psn, vehNum, User);
                return responses;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<DahkDetails> GetDahkBlockages(ulong customerNumber)
        {
            try
            {
                List<DahkDetails> blockages = DahkDetails.GetDahkBlockages(customerNumber);
                return blockages;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DahkDetails> GetDahkCollections(ulong customerNumber)
        {
            try
            {
                List<DahkDetails> collections = DahkDetails.GetDahkCollections(customerNumber);
                return collections;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DahkEmployer> GetDahkEmployers(ulong customerNumber, ProductQualityFilter quality, string inquestId)
        {
            try
            {
                List<DahkEmployer> employers = DahkDetails.GetDahkEmployers(customerNumber, quality, inquestId);
                return employers;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<ulong> GetDAHKproductAccounts(ulong accountNumber)
        {
            try
            {
                List<ulong> employers = DahkDetails.GetDAHKproductAccounts(accountNumber);
                return employers;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult BlockingAmountFromAvailableAccount(double accountNumber, float blockingAmount, List<DahkDetails> inquestDetailsList, int userID)
        {
            try
            {
                DateTime operationDate = Utility.GetCurrentOperDay();
                ActionResult result = new ActionResult();
                result = DahkDetails.BlockingAmountFromAvailableAccount(accountNumber, blockingAmount, inquestDetailsList, userID, operationDate);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult MakeAvailable(List<long> freezeIdList, float availableAmount, ushort filialCode, short userId)
        {
            try
            {
                DateTime operationDate = Utility.GetCurrentOperDay();
                ActionResult result = new ActionResult();
                result = DahkDetails.MakeAvailable(freezeIdList, availableAmount, filialCode, userId, operationDate);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public Dictionary<string, string> GetFreezedAccounts(ulong customerNumber)
        {
            try
            {
                return DahkDetails.GetFreezedAccounts(customerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<AccountDAHKfreezeDetails> GetAccountDAHKFreezeDetails(ulong customerNumber, string inquestId, ulong accountNumber)
        {
            try
            {
                List<AccountDAHKfreezeDetails> freezeDetails = DahkDetails.GetAccountDAHKFreezeDetails(customerNumber, inquestId, accountNumber);
                return freezeDetails;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<AccountDAHKfreezeDetails> GetCurrentInquestDetails(ulong customerNumber)
        {
            try
            {
                List<AccountDAHKfreezeDetails> details = DahkDetails.GetCurrentInquestDetails(customerNumber);
                return details;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<DahkAmountTotals> GetDahkAmountTotals(ulong customerNumber)
        {
            try
            {
                List<DahkAmountTotals> amountTotals = DahkDetails.GetDahkAmountTotals(customerNumber);
                return amountTotals;


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
                return Info.GetTerm(id, param, language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Loan> GetAparikTexumLoans()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAparikTexumLoans();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<GoodsDetails> GetGoodsDetails(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetGoodsDetails(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountFlowDetails GetAccountFlowDetails(string accountNumber, DateTime startDate, DateTime endDate)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountFlowDetails(accountNumber, startDate, endDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<ServicePaymentNote> GetServicePaymentNoteList()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetServicePaymentNoteList();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveServicePaymentNoteOrder(ServicePaymentNoteOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveServicePaymentNoteOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ServicePaymentNoteOrder GetServicePaymentNoteOrder(long ID)
        {
            ServicePaymentNoteOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetServicePaymentNoteOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public double GetDepositLoanAndProvisionCoefficent(string loanCurrency, string provisionCurrency)
        {
            try
            {
                return LoanProductOrder.GetDepositLoanAndProvisionCoefficent(loanCurrency, provisionCurrency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ServicePaymentNoteOrder GetDelatedServicePaymentNoteOrder(long ID)
        {
            ServicePaymentNoteOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDelatedServicePaymentNoteOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApprovePensionApplicationOrder(PensionApplicationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePensionApplicationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PensionApplication> GetPensionApplicationHistory(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetPensionApplicationHistory(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApprovePensionApplicationTerminationOrder(PensionApplicationTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePensionApplicationTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public PensionApplicationTerminationOrder GetPensionApplicationTerminationOrder(long ID)
        {
            PensionApplicationTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPensionApplicationTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PensionApplicationOrder GetPensionApplicationOrder(long ID)
        {
            PensionApplicationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPensionApplicationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveTransferCallContractOrder(TransferCallContractOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveTransferCallContractOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public TransferCallContractOrder GetTransferCallContractOrder(long ID)
        {
            TransferCallContractOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetTransferCallContractOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Account> GetClosedDepositAccountList(DepositOrder order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                InitOrder(order);
                return customer.GetClosedDepositAccountList(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult UpdateTransferVerifiedStatus(ulong transferId, int verified)
        {
            try
            {
                Transfer transfer = new Transfer();
                transfer.Id = transferId;
                transfer.Get();
                ActionResult result = transfer.UpdateTransferVerifiedStatus(User.userID, verified);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public ActionResult SaveAndApproveReestrTransferOrder(ReestrTransferOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveReestrTransferOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ReestrTransferOrder GetReestrTransferOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetReestrTransferOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<TransferCallContractDetails> GetTransferCallContractsDetails()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetTransferCallContractsDetails();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public TransferCallContractDetails GetTransferCallContractDetails(long contractId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetTransferCallContractDetails(contractId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult SaveAndApproveTransferCallContractTerminationOrder(TransferCallContractTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveTransferCallContractTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public TransferCallContractTerminationOrder GetTransferCallContractTerminationOrder(long ID)
        {
            TransferCallContractTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetTransferCallContractTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Order GetOrder(long ID)
        {
            Order order;
            try
            {
                Customer customer = new Customer();
                order = customer.GetOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool AccountAccessible(string accountNumber, long accountGroup)
        {
            try
            {
                return Account.AccountAccessible(accountNumber, User.AccountGroup);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<RejectedOrderMessage> GetRejectedMessages(int filter, int start, int end)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetRejectedMessages(User, filter, start, end);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void CloseRejectedMessage(int messageId)
        {
            try
            {
                RejectedOrderMessage.CloseRejectedMessage(messageId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public int GetTotalRejectedUserMessages()
        {

            try
            {
                return RejectedOrderMessage.GetTotalRejectedUserMessages(User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetTotalNotConfirmedOrder()
        {

            try
            {
                return Order.GetTotalNotConfirmedOrder(User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveDepositCaseOrder(DepositCaseOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDepositCaseOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong GetDepositCaseOrderContractNumber()
        {
            try
            {
                return DepositCaseOrder.GetContractNumber();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DepositCaseMap> GetDepositCaseMap(short caseSide)
        {
            try
            {
                return DepositCase.GetDepositCaseMap(User.filialCode, caseSide);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetDepositCasePrice(string caseNumber, int filialCode, short contractDuration)
        {
            try
            {
                return DepositCase.GetDepositCasePrice(caseNumber, filialCode, contractDuration);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public DepositCaseOrder GetDepositCaseOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositCaseOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CardTariffContract> GetCustomerCardTariffContracts(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCustomerCardTariffContracts(customer.CustomerNumber, filter);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool HasCardTariffContract()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.HasCardTariffContract(customer.CustomerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool HasPosTerminal()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.HasPosTerminal(customer.CustomerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCardTariffContractActiveCardsCount(int contractId)
        {
            try
            {

                return CardTariffContract.GetActiveCardsCount(contractId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PosLocation> GetCustomerPosLocations(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCustomerPosLocations(customer.CustomerNumber, filter);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public PosLocation GetPosLocation(int posLocationId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPosLocation(posLocationId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<PosRate> GetPosRates(int terminalId)
        {
            try
            {
                return PosTerminal.GetPosRates(terminalId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<PosCashbackRate> GetPosCashbackRates(int terminalId)
        {
            try
            {
                return PosTerminal.GetPosCashbackRates(terminalId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Order> GetApproveReqOrder(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetApproveReqOrder(dateFrom, dateTo);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public short IsCustomerSwiftTransferVerified(ulong customerNummber, string swiftCode = "", string receiverAaccount = "")
        {
            try
            {

                return InternationalPaymentOrder.IsCustomerSwiftTransferVerified(customerNummber, Source, swiftCode, receiverAaccount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public bool IsExistingTransferByCall(short transferSystem, string code, long transferId)
        {
            try
            {

                return ReceivedFastTransferPaymentOrder.IsExistingTransferByCall(transferSystem, code, transferId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public void SetTransferByCallType(short type, long id)
        {
            try
            {

                ReceivedFastTransferPaymentOrder.SetTransferByCallType(type, id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDepositCasePenaltyMatureOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public DepositCasePenaltyMatureOrder GetDepositCasePenaltyMatureOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositCasePenaltyMatureOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PlasticCard> GetCardsForRegistration()
        {
            try
            {

                List<PlasticCard> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCardsForRegistration(User.filialCode);
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardRegistrationOrder(CardRegistrationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardRegistrationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Account> GetAccountListForCardRegistration(string cardCurrency, int cardFililal)
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountListForCardRegistration(cardCurrency, cardFililal);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetOverdraftAccountsForCardRegistration(string cardCurrency, int cardFililal)
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOverdraftAccountsForCardRegistration(cardCurrency, cardFililal);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardRegistrationOrder GetCardRegistrationOrder(long ID)
        {
            CardRegistrationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardRegistrationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public ActionResult SavePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePeriodicSwiftStatementOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public ActionResult SaveAndAprovePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndAprovePeriodicSwiftStatementOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }

        public ActionResult ApprovePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicSwiftStatementOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public double GetPeriodicSwiftStatementOrderFee()
        {
            try
            {
                return Utility.GetPriceInfoByIndex(836, "price");
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PeriodicSwiftStatementOrder GetPeriodicSwiftStatementOrder(long ID)
        {
            PeriodicSwiftStatementOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPeriodicSwiftStatementOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<string> GetCardRegistrationWarnings(PlasticCard plasticCard)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetCardRegistrationWarnings(plasticCard);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Provision> GetCustomerProvisions(string currency, ushort type, ushort quality)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetCustomerProvisions(currency, type, quality);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveTransferToShopOrder(TransferToShopOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveTransferToShopOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public bool CheckTransferToShopPayment(ulong productId)
        {
            try
            {
                return TransferToShopOrder.CheckTransferToShopPayment(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public Account GetShopAccount(ulong productId)
        {
            try
            {
                Account account = null;
                account = TransferToShopOrder.GetShopAccount(productId);
                return account;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<ProvisionLoan> GetProvisionLoans(ulong provisionId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetProvisionLoans(provisionId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public TransferToShopOrder GetTransferToShopOrder(long ID)
        {
            TransferToShopOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetTransferToShopOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }



        public double GetCOWaterOrderAmount(string abonentNumber, string branchCode, ushort paymentType)
        {
            try
            {
                return UtilityPaymentOrder.GetCOWaterOrderAmount(abonentNumber, branchCode, paymentType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public Insurance GetInsurance(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetInsurance(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Insurance> GetInsurances(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetInsurances(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Insurance> GetPaidInsurance(ulong loanProductId)
        {
            try
            {


                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaidInsurance(loanProductId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public InsuranceOrder GetInsuranceOrder(long ID)
        {
            InsuranceOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetInsuranceOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public ActionResult SaveAndApproveInsuranceOrder(InsuranceOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveInsuranceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Account GetInsuraceCompanySystemAccount(ushort companyID, ushort insuranceType)
        {
            try
            {
                return Insurance.GetInsuraceCompanySystemAccount(companyID, insuranceType);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public uint GetInsuranceCompanySystemAccountNumber(ushort companyID, ushort insuranceType)
        {
            try
            {
                return Insurance.GetInsuranceCompanySystemAccountNumber(companyID, insuranceType);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveAndApproveCardDataChangeOrder(CardDataChangeOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardServiceFeeDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardDataChangeOrder GetCardDataChangeOrder(long ID)
        {
            CardDataChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardDataChangeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public bool CheckCardDataChangeOrderFieldTypeIsRequaried(short fieldType)
        {
            try
            {

                return CardDataChangeOrder.CheckFieldTypeIsRequaried(fieldType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveCardServiceFeeGrafikDataChangeOrder(CardServiceFeeGrafikDataChangeOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardServiceFeeGrafikDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardServiceFeeGrafikDataChangeOrder GetCardServiceFeeGrafikDataChangeOrder(long ID)
        {
            CardServiceFeeGrafikDataChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardServiceFeeGrafikDataChangeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public List<CardServiceFeeGrafik> SetNewCardServiceFeeGrafik(ulong productId)
        {

            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.SetNewCardServiceFeeGrafik(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        /// <summary>
        /// Ստուգում ենք հաշվի  արգելանքի կամ բռնագանձման ենթակա լինելը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public bool CheckAccountForDAHK(string accountNumber)
        {
            return Account.CheckAccountForDAHK(accountNumber);
        }

        public List<GivenGuaranteeReduction> GetGivenGuaranteeReductions(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetGivenGuaranteeReductions(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveReestrUtilityPaymentOrder(ReestrUtilityPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveReestrUtilityPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ReestrUtilityPaymentOrder GetReestrUtilityPaymentOrder(long id)
        {
            ReestrUtilityPaymentOrder utilityPaymentOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                utilityPaymentOrder = customer.GetReestrUtilityPaymentOrder(id);
                return utilityPaymentOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveAccountAdditionalDataRemovableOrder(AccountAdditionalDataRemovableOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountAdditionalDataRemovableOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DAHKDetail GetCardDAHKDetails(string cardNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardDAHKDetails(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsDAHKAvailability(ulong customerNumber)
        {
            try
            {
                return Validation.IsDAHKAvailability(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PlasticCard GetPlasticCard(ulong productId, bool productIdType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPlasticCard(productId, User.filialCode, productIdType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetPoliceResponseDetailsIDWithoutRequest(string violationID, DateTime violationDate)
        {
            try
            {

                return BudgetPaymentOrder.GetPoliceResponseDetailsIDWithoutRequest(violationID, violationDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong GetOrderTransactionsGroupNumber(long orderId)
        {
            try
            {
                return Order.GetOrderTransactionsGroupNumber(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetATSSystemAccounts(string currency)
        {
            try
            {
                if (User != null)
                {
                    return Account.GetATSSystemAccounts(currency, User.filialCode);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool HasATSSystemAccountInFilial()
        {
            try
            {
                if (User != null)
                {
                    return Account.HasATSSystemAccountInFilial(User.filialCode);
                }
                else
                {
                    return false;
                }

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
                ArcaBalanceResponseData result;

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);

                Card card = customer.GetCard(cardNumber);

                if (card != null)
                    result = card.GetArCaBalanceResponseData(User.userID, ClientIp);
                else
                {
                    result = new ArcaBalanceResponseData();
                    result.ResponseCode = "997";
                }

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LoanProductClassification> GetLoanProductClassifications(ulong productId, DateTime dateFrom)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetLoanProductClassifications(productId, dateFrom);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<SafekeepingItem> GetSafekeepingItems(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetSafekeepingItems(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public SafekeepingItem GetSafekeepingItem(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetSafekeepingItem(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<ExchangeRate> GetExchangeRatesHistory(int filialCode, string currency, DateTime startDate)
        {
            try
            {
                return ExchangeRate.GetExchangeRatesHistory(filialCode, currency, startDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ExchangeRate> GetCBExchangeRatesHistory(string currency, DateTime startDate)
        {
            try
            {
                return ExchangeRate.GetCBExchangeRatesHistory(currency, startDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CrossExchangeRate> GetCrossExchangeRatesHistory(int filialCode, DateTime startDate)
        {
            try
            {
                return ExchangeRate.GetCrossExchangeRatesHistory(filialCode, startDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetTransitAccountsForDebitTransactions()
        {
            try
            {

                return Account.GetTransitAccountsForDebitTransactions(User.filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Account GetCardCashbackAccount(ulong productId)
        {
            try
            {
                return Card.GetCashBackAccount(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCardMotherName(ulong productId)
        {
            try
            {
                return Card.GetCardMotherName(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CorrespondentBankAccount> GetCorrespondentBankAccounts(CorrespondentBankAccount filter)
        {
            try
            {

                return CorrespondentBankAccount.GetCorrespondentBankAccounts(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardStatusChangeOrder(CardStatusChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardStatusChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardStatusChangeOrder GetCardStatusChangeOrder(long orderId)
        {
            CardStatusChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardStatusChangeOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveAndApproveCardMembershipRewardsOrder(MembershipRewardsOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardMembershipRewardsOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {
            try
            {
                ActionResult result = new ActionResult();
                if (account.IsCustomerTransitAccount)
                {
                    account.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                }

                result = account.SaveTransitAccountForDebitTransactions(User);
                Localization.SetCulture(result, new Culture(Languages.hy));
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult UpdateTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {
            try
            {
                ActionResult result = new ActionResult();
                result = account.UpdateTransitAccountForDebitTransactions();
                Localization.SetCulture(result, new Culture(Languages.hy));
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult CloseTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {
            try
            {
                ActionResult result = new ActionResult();
                result = account.CloseTransitAccountForDebitTransactions();
                Localization.SetCulture(result, new Culture(Languages.hy));
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ReopenTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account)
        {
            try
            {
                ActionResult result = new ActionResult();
                result = account.ReopenTransitAccountForDebitTransactions();
                Localization.SetCulture(result, new Culture(Languages.hy));
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<TransitAccountForDebitTransactions> GetAllTransitAccountsForDebitTransactions(ProductQualityFilter quality)
        {
            try
            {

                return TransitAccountForDebitTransactions.GetTransitAccountsForDebitTransactions(quality);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetBusinesDepositOptionRate(ushort depositOption, string currency)
        {
            try
            {
                return Deposit.GetBusinesDepositOptionRate(depositOption, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<CardActivationInArCa> GetCardActivationInArCa(string cardNumber, DateTime startDate, DateTime endDate)
        {
            try
            {
                return CardActivationInArCa.GetCardActivationInArCa(cardNumber, startDate, endDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public DateTime? GetLastSendedPaymentFileDate()
        {
            try
            {
                return CardActivationInArCa.GetLastSendedPaymentFileDate();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CardActivationInArCaApigateDetails> GetCardActivationInArCaApigateDetail(ulong Id)
        {
            try
            {
                return CardActivationInArCa.GetCardActivationInArCaApigateDetail(Id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveCustomerCommunalCard(CustomerCommunalCard customerCommunalCard)
        {
            try
            {
                ActionResult result = new ActionResult();

                Customer customer = CreateCustomer();
                result = customer.SaveCustomerCommunalCard(customerCommunalCard);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ChangeCustomerCommunalCardQuality(CustomerCommunalCard customerCommunalCard)
        {
            try
            {
                ActionResult result = new ActionResult();

                Customer customer = CreateCustomer();
                result = customer.ChangeCustomerCommunalCardQuality(customerCommunalCard);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CustomerCommunalCard> GetCustomerCommunalCards()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetCustomerCommunalCards();

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ամսում կոմունալ ծառայության համար վճարված գումարը
        /// </summary>
        public List<double> GetComunalAmountPaidThisMonth(string code, short comunalType, short abonentType, DateTime DebtListDate, string texCode, int waterCoPaymentType)
        {
            try
            {

                return SearchCommunal.GetComunalAmountPaidThisMonth(code, comunalType, abonentType, DebtListDate, texCode, waterCoPaymentType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveFactoringTerminationOrder(FactoringTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveFactoringTerminationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public FactoringTerminationOrder GetFactoringTerminationOrder(long ID)
        {
            FactoringTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetFactoringTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveFactoringTerminationOrder(FactoringTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveFactoringTerminationOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveFactoringTerminationOrder(FactoringTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveFactoringTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveLoanProductTerminationOrder(LoanProductTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveLoanProductTerminationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public LoanProductTerminationOrder GetLoanProductTerminationOrder(long ID)
        {
            LoanProductTerminationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetLoanProductTerminationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveLoanProductTerminationOrder(LoanProductTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveLoanProductTerminationOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveLoanProductTerminationOrder(LoanProductTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveLoanProductTerminationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetShopTransferAmount(ulong productId)
        {
            try
            {
                return TransferToShopOrder.GetShopTransferAmount(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveDepositDataChangeOrder(DepositDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveDepositDataChangeOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public DepositDataChangeOrder GetDepositDataChangeOrder(long ID)
        {
            DepositDataChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDepositDataChangeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveDepositDataChangeOrder(DepositDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveDepositDataChangeOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveDepositDataChangeOrder(DepositDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDepositDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DepositAction> GetDepositActions(DepositOrder order)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                InitOrder(order);
                return customer.GetDepositActions(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLoanTotalInsuranceAmount(ulong productId)
        {
            try
            {

                return LoanProductActivationOrder.GetLoanTotalInsuranceAmount(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<ENAPayments> GetENAPayments(string abonentNumber, string branch)
        {
            try
            {
                return ENAPayments.GetENAPayments(abonentNumber, branch);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DateTime> GetENAPaymentDates(string abonentNumber, string branch)
        {
            try
            {
                return ENAPayments.GetENAPaymentDates(abonentNumber, branch);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Account GetProductAccount(ulong productId, ushort productType, ushort accountType)
        {
            try
            {

                return Account.GetProductAccount(productId, productType, accountType);
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


        public CashBookOrder GetCashBookOrder(long ID)
        {
            CashBookOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCashBookOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Account GetCashBookOperationSystemAccount(CashBookOrder order, OrderAccountType accountType, ushort filialCode)
        {
            try
            {
                return CashBookOrder.GetCashBookOperationSystemAccount(order, accountType, filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public LoanApplication GetLoanApplication(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanApplication(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LoanApplication> GetLoanApplications()
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanApplications();
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.FastOverdraftValidations(cardNumber, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<FicoScoreResult> GetLoanApplicationFicoScoreResults(ulong productId, DateTime requestDate)
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanApplicationFicoScoreResults(productId, requestDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanApplication GetLoanApplicationByDocId(long docId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanApplicationByDocId(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<CardTariffContract> GetCardTariffContracts(ProductQualityFilter filter, ulong customerNumber)
        {
            try
            {
                List<CardTariffContract> cardTariffContracts = CardTariffContract.GetCustomerCardTariffContracts(customerNumber, filter);
                return cardTariffContracts;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveLoanMonitoringConclusion(LoanMonitoringConclusion monitoring)
        {
            try
            {
                Customer customer = CreateCustomer();
                monitoring.RegistrationDate = Utility.GetCurrentOperDay();
                return customer.SaveLoanMonitoringConclusion(monitoring);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveLoanMonitoringConclusion(long monitoringId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.ApproveLoanMonitoringConclusion(monitoringId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult DeleteLoanMonitoringConclusion(long monitoringId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.DeleteLoanMonitoringConclusion(monitoringId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LoanMonitoringConclusion> GetLoanMonitoringConclusions(long productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetLoanMonitoringConclusions(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public LoanMonitoringConclusion GetLoanMonitoringConclusion(long monitoringId, long productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetLoanMonitoringConclusion(monitoringId, productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<MonitoringConclusionLinkedLoan> GetLinkedLoans(long productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetLinkedLoans(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public float GetProvisionCoverCoefficient(long productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetProvisionCoverCoefficient(productId);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public short GetLoanMonitoringType()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetLoanMonitoringType();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveDepositCaseStoppingPenaltyCalculationOrder(DepositCaseStoppingPenaltyCalculationOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDepositCaseStoppingPenaltyCalculationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DepositCaseStoppingPenaltyCalculationOrder GetDepositCaseStoppingPenaltyCalculationOrder(long ID)
        {
            DepositCaseStoppingPenaltyCalculationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDepositCaseStoppingPenaltyCalculationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public CTPaymentOrder GetCTPaymentOrder(long ID)
        {
            CTPaymentOrder order;
            try
            {
                order = CashTerminal.GetCTPaymentOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public CTLoanMatureOrder GetCTLoanMatureOrder(long ID)
        {
            CTLoanMatureOrder order;
            try
            {
                order = CashTerminal.GetCTLoanMatureOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        //async version of SaveAndApproveAutomaticGenaratedPreOrders
        //public async System.Threading.Tasks.Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrders(long preOrderId)
        //{
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    List<ActionResult> results = new List<ActionResult>();
        //    try
        //    {
        //        List<LoanProductActivationOrder> orders = LoanProductActivationOrder.GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(preOrderId).FindAll(o => (short)o.Quality == 10);
        //        Customer customer = CreateCustomer();
        //        System.Threading.Tasks.Task taskInit;
        //        List<System.Threading.Tasks.Task> taskInits = new List<System.Threading.Tasks.Task>();
        //        foreach (LoanProductActivationOrder order in orders)
        //        {
        //            // sync
        //            //InitOrder(order);    
        //            ///////////////////////// Async/////////////////////////////////////
        //            taskInit = new System.Threading.Tasks.Task(() => { InitOrder(order); });
        //            taskInits.Add(taskInit);
        //            taskInit.Start();    
        //        }
        //        System.Threading.Tasks.Task.WaitAll(taskInits.ToArray());

        //        System.Threading.Tasks.Task<ActionResult> taskSaveAndApprove;
        //        List<System.Threading.Tasks.Task<ActionResult>> taskSaveAndApproves = new List<System.Threading.Tasks.Task<ActionResult>>();
        //        foreach (LoanProductActivationOrder order in orders)
        //        {
        //            //sync
        //            //customer.SaveAndApproveLoanProductActivationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme); 

        //            ////////////////async   /////////////////////////////////////////////////////////
        //            taskSaveAndApprove = new System.Threading.Tasks.Task<ActionResult>(() => customer.SaveAndApproveLoanProductActivationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme));
        //            taskSaveAndApproves.Add(taskSaveAndApprove);
        //            taskSaveAndApprove.Start();
        //        }
        //        foreach (var task in taskSaveAndApproves)
        //        {
        //            await task;
        //            results.Add(task.Result);
        //        }
        //        watch.Stop();
        //        var elapsedMs = watch.ElapsedMilliseconds;
        //        return results;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog(ex);
        //        throw new FaultException(Resourse.InternalError);
        //    }
        //}

        public List<CreditHereAndNow> GetCreditsHereAndNow(SearchCreditHereAndNow searchParameters, out int RowCount)
        {
            try
            {
                return CreditHereAndNow.GetCreditsHereAndNow(searchParameters, out RowCount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCreditHereAndNowActivationOrders(CreditHereAndNowActivationOrders creditHNActivationOrders)
        {
            try
            {
                CustomerServiceType = ServiceType.NonCustomerService;
                creditHNActivationOrders.OperationFilialCode = User.filialCode;
                Customer customer = CreateCustomer();
                return customer.SaveAndApproveCreditHereAndNowActivationOrders(creditHNActivationOrders, User.userName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PreOrderDetails> GetSearchedPreOrderDetails(SearchPreOrderDetails searchParams, out int RowCount)
        {
            try
            {
                return PreOrder.GetSearchedPreOrderDetails(searchParams, out RowCount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public async Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrdersLoanActivation(long preOrderId)
        {
            List<ActionResult> results = new List<ActionResult>();
            try
            {
                List<LoanProductActivationOrder> orders = LoanProductActivationOrder.GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(preOrderId).FindAll(o => (short)o.Quality == 10);

                System.Threading.Tasks.Task<ActionResult> task;
                List<System.Threading.Tasks.Task<ActionResult>> tasks = new List<System.Threading.Tasks.Task<ActionResult>>();
                foreach (LoanProductActivationOrder order in orders)
                {
                    task = new System.Threading.Tasks.Task<ActionResult>(() => AutoSaveAndApproveLoanProductActivationOrder(order));
                    tasks.Add(task);
                    task.Start();
                    await task;
                    results.Add(task.Result);
                }

                return results;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        private ActionResult AutoSaveAndApproveLoanProductActivationOrder(LoanProductActivationOrder order)
        {
            ActionResult result = SaveAndApproveLoanProductActivationOrderAuto(order);

            if (result.ResultCode != ResultCode.Normal)
            {
                foreach (ActionError error in result.Errors)
                {
                    error.Description = "  Հաճախորդի համար`" + order.CustomerNumber + "   Վարկի համար`" + order.ProductId + " \n" + error.Description;
                }
            }
            switch (result.ResultCode)
            {
                case ResultCode.Normal:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.Done);
                    break;
                case ResultCode.Failed:
                case ResultCode.ValidationError:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.NotCreated);
                    order.UpdateQuality(OrderQuality.Removed);
                    break;
                case ResultCode.SavedNotConfirmed:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.CreatedNotDone);
                    order.UpdateQuality(OrderQuality.Removed);
                    break;

            }
            return result;

        }
        public ActionResult SaveAndApproveLoanProductActivationOrderAuto(LoanProductActivationOrder order)
        {
            try
            {
                Customer customer = new Customer(User, order.CustomerNumber, Languages.hy);
                InitOrderData(order);
                customer.Source = order.Source;
                return customer.SaveAndApproveLoanProductActivationOrder(order, order.CustomerNumber.ToString(), 1);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                ActionResult result = new ActionResult();
                ActionError error = new ActionError();
                result.ResultCode = ResultCode.Failed;
                error.Description = "</br>Տեղի ունեցավ սխալ:Վարկի ակտիվացումը չհաջողվեց:";
                result.Errors.Add(error);
                return result;
            }
        }
        /// <summary>
        /// Լրացնում հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        private void InitOrderData(Order order)
        {
            order.FilialCode = User.filialCode;
            order.Source = Source;
            order.user = User;
            order.OperationDate = Utility.GetCurrentOperDay();
            order.DailyTransactionsLimit = 100000000000;
        }
        public ulong AuthorizeCustomerByLoanApp(ulong productId)
        {
            try
            {
                return LoanProduct.GetCustomerNumberByLoanApp(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Dictionary<string, string> InitUserPagePermissions(string userSessionToken)
        {
            try
            {
                return AuthorizationService.InitUserPagePermissions(userSessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetAccountDescription(string accountNumber)
        {
            try
            {
                return Account.GetAccountDescription(accountNumber);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCredentialActivationOrder(CredentialActivationOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCredentialActivationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CredentialActivationOrder GetCredentialActivationOrder(long ID)
        {
            CredentialActivationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCredentialActivationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<ClassifiedLoan> GetClassifiedLoans(SearchClassifiedLoan searchParameters, out int RowCount)
        {
            try
            {
                return ClassifiedLoan.GetClassifiedLoans(searchParameters, out RowCount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveClassifiedLoanActionOrders(ClassifiedLoanActionOrders classifiedLoanActionOrders)
        {
            try
            {
                CustomerServiceType = ServiceType.NonCustomerService;
                classifiedLoanActionOrders.OperationFilialCode = User.filialCode;
                Customer customer = CreateCustomer();
                return customer.SaveAndApproveClassifiedLoanActionOrders(classifiedLoanActionOrders, User.userName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        //Վարկի հետ դասակարգման ավտոմատ ձևակերպումներ
        #region SaveAndApproveClassificationRemoveAuto
        public async Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrdersClassificationRemove(long preOrderId)
        {
            List<ActionResult> results = new List<ActionResult>();
            try
            {
                List<LoanProductClassificationRemoveOrder> orders = LoanProductClassificationRemoveOrder.GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(preOrderId).FindAll(o => (short)o.Quality == 10);

                System.Threading.Tasks.Task<ActionResult> task;
                List<System.Threading.Tasks.Task<ActionResult>> tasks = new List<System.Threading.Tasks.Task<ActionResult>>();
                foreach (LoanProductClassificationRemoveOrder order in orders)
                {
                    task = new System.Threading.Tasks.Task<ActionResult>(() => AutoSaveAndApproveClassificationRemoveOrder(order));
                    tasks.Add(task);
                    task.Start();
                    await task;
                    results.Add(task.Result);
                }

                return results;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        private ActionResult AutoSaveAndApproveClassificationRemoveOrder(LoanProductClassificationRemoveOrder order)
        {
            ActionResult result = SaveAndApproveClassificationRemoveOrderAuto(order);

            if (result.ResultCode != ResultCode.Normal)
            {
                foreach (ActionError error in result.Errors)
                {
                    error.Description = "  Հաճախորդի համար`" + order.CustomerNumber + "   Վարկի համար`" + order.ProductId + " \n" + error.Description;
                }
            }
            switch (result.ResultCode)
            {
                case ResultCode.Normal:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.Done);
                    break;
                case ResultCode.Failed:
                case ResultCode.ValidationError:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.NotCreated);
                    order.UpdateQuality(OrderQuality.Removed);
                    break;
                case ResultCode.SavedNotConfirmed:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.CreatedNotDone);
                    order.UpdateQuality(OrderQuality.Removed);
                    break;

            }
            return result;

        }
        private ActionResult SaveAndApproveClassificationRemoveOrderAuto(LoanProductClassificationRemoveOrder order)
        {
            try
            {
                Customer customer = new Customer(User, order.CustomerNumber, Languages.hy);
                InitOrderData(order);
                customer.Source = order.Source;
                return customer.SaveAndApproveLoanProductClassificationRemoveOrder(order, order.CustomerNumber.ToString(), 1);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                ActionResult result = new ActionResult();
                ActionError error = new ActionError();
                result.ResultCode = ResultCode.Failed;
                error.Description = "</br>Տեղի ունեցավ սխալ:Վարկի հետ բերումը չհաջողվեց:";
                result.Errors.Add(error);
                return result;
            }
        }
        public ActionResult SaveAndApproveClassificationRemoveOrder(LoanProductClassificationRemoveOrder order, bool includingSurcharge = false)
        {
            ulong SurchargeAppId = 0;
            ActionResult result = new ActionResult();
            ActionResult res = new ActionResult();
            try
            {
                order.Type = OrderType.LoanProductMakeOutOrder;
                Customer customer = CreateCustomer();
                InitOrder(order);
                res = customer.SaveAndApproveLoanProductClassificationRemoveOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                result.Errors.AddRange(res.Errors);
                result.ResultCode = res.ResultCode;

                if (includingSurcharge)
                {
                    SurchargeAppId = Loan.GetSurchargeAppId(order.ProductId);
                    if (SurchargeAppId != 0)
                    {
                        order.ProductId = SurchargeAppId;
                        order.ProductType = 4;
                        res = customer.SaveAndApproveLoanProductClassificationRemoveOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                        result.Errors.AddRange(res.Errors);
                        if (res.ResultCode != ResultCode.Normal)
                        {
                            result.ResultCode = res.ResultCode;
                        }
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        #endregion

        //Վարկի դուրսգրման ավտոմատ ձևակերպումներ
        #region SaveAndApprovePreOrdersMakeLoanOutAuto
        public async Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrdersMakeLoanOut(long preOrderId)
        {
            List<ActionResult> results = new List<ActionResult>();
            try
            {
                List<LoanProductMakeOutOrder> orders = LoanProductMakeOutOrder.GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(preOrderId).FindAll(o => (short)o.Quality == 10);

                System.Threading.Tasks.Task<ActionResult> task;
                List<System.Threading.Tasks.Task<ActionResult>> tasks = new List<System.Threading.Tasks.Task<ActionResult>>();
                foreach (LoanProductMakeOutOrder order in orders)
                {
                    task = new System.Threading.Tasks.Task<ActionResult>(() => AutoSaveAndApproveLoanProductMakeOutOrder(order));
                    tasks.Add(task);
                    task.Start();
                    await task;
                    results.Add(task.Result);
                }

                return results;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        private ActionResult AutoSaveAndApproveLoanProductMakeOutOrder(LoanProductMakeOutOrder order)
        {
            ActionResult result = SaveAndApproveLoanProductMakeOutOrderAuto(order);

            if (result.ResultCode != ResultCode.Normal)
            {
                foreach (ActionError error in result.Errors)
                {
                    error.Description = "  Հաճախորդի համար`" + order.CustomerNumber + "   Վարկի համար`" + order.ProductId + " \n" + error.Description;
                }
            }
            switch (result.ResultCode)
            {
                case ResultCode.Normal:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.Done);
                    break;
                case ResultCode.Failed:
                case ResultCode.ValidationError:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.NotCreated);
                    order.UpdateQuality(OrderQuality.Removed);
                    break;
                case ResultCode.SavedNotConfirmed:
                    PreOrder.UpdatePreOrderDetailQuality(order.CustomerNumber, order.ProductId, PreOrderQuality.CreatedNotDone);
                    order.UpdateQuality(OrderQuality.Removed);
                    break;

            }
            return result;

        }
        private ActionResult SaveAndApproveLoanProductMakeOutOrderAuto(LoanProductMakeOutOrder order)
        {
            try
            {
                Customer customer = new Customer(User, order.CustomerNumber, Languages.hy);
                InitOrderData(order);
                customer.Source = order.Source;
                return customer.SaveAndApproveLoanProductMakeOutOrder(order, order.CustomerNumber.ToString(), 1);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                ActionResult result = new ActionResult();
                ActionError error = new ActionError();
                result.ResultCode = ResultCode.Failed;
                error.Description = "</br>Տեղի ունեցավ սխալ:Վարկի դուրսգրումը չհաջողվեց:";
                result.Errors.Add(error);
                return result;
            }
        }
        public ActionResult SaveAndApproveLoanProductMakeOutOrder(LoanProductMakeOutOrder order, bool includingSurcharge = false)
        {
            ulong SurchargeAppId = 0;
            ActionResult result = new ActionResult();
            ActionResult res = new ActionResult();

            try
            {
                order.Type = OrderType.LoanProductMakeOutOrder;
                Customer customer = CreateCustomer();
                InitOrder(order);
                res = customer.SaveAndApproveLoanProductMakeOutOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                result.Errors.AddRange(res.Errors);
                result.ResultCode = res.ResultCode;


                if (includingSurcharge)
                {
                    SurchargeAppId = Loan.GetSurchargeAppId(order.ProductId);
                    if (SurchargeAppId != 0)
                    {
                        order.ProductId = SurchargeAppId;
                        order.ProductType = 4;
                        res = customer.SaveAndApproveLoanProductMakeOutOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                        result.Errors.AddRange(res.Errors);
                        if (res.ResultCode != ResultCode.Normal)
                        {
                            result.ResultCode = res.ResultCode;
                        }
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        #endregion 


        /// <summary>
        /// Լիազորված անձի նույնականացման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAssigneeIdentificationOrder(AssigneeIdentificationOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAssigneeIdentificationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool HasPropertyProvision(ulong productId)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.HasPropertyProvision(productId);
            }
            catch
            {

                throw;
            }
        }

        public Dictionary<string, string> ProvisionContract(long docId)
        {

            try
            {
                return Provision.ProvisionContract(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong GetNextCredentialDocumentNumber()
        {
            try
            {
                return CredentialOrder.GetNextCredentialDocumentNumber(User.filialCode);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveDemandDepositRateChangeOrder(DemandDepositRateChangeOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveDemandDepositRateChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DemandDepositRateChangeOrder GetDemandDepositRateChangeOrder(long ID)
        {
            DemandDepositRateChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDemandDepositRateChangeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public int GetCredentialDocId(ulong credentialId)
        {
            try
            {

                return Credential.GetCredentialDocId(credentialId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AssigneeIdentificationOrder GetAssigneeIdentificationOrder(long ID)
        {
            AssigneeIdentificationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetAssigneeIdentificationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CreditLinePrecontractData GetCreditLinePrecontractData(DateTime startDate, DateTime endDate, double interestRate, double repaymentPercent, string cardNumber, string currency, double amount, int loanType)
        {
            try
            {
                CreditLinePrecontractData result = CreditLinePrecontractData.GetCreditLinePrecontractData(startDate, endDate, interestRate, repaymentPercent, cardNumber, currency, amount, loanType);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<AccountOpeningClosingDetail> GetAccountOpeningClosingDetails(string accountNumber)
        {
            try
            {
                List<AccountOpeningClosingDetail> list = AccountOpeningClosingDetail.GetAccountOpeningClosingDetails(accountNumber);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public xbs.Customer GetCustomerData(ulong customerNumber)
        {
            try
            {
                Customer customer = new Customer();
                customer.CustomerNumber = customerNumber;
                return customer.GetCustomerData();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public AccountOpeningClosingDetail GetAccountOpeningDetail(string accountNumber)
        {
            try
            {
                return AccountOpeningClosingDetail.GetAccountOpeningDetail(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public DemandDepositRate GetDemandDepositRate(string accountNumber)
        {
            try
            {
                return DemandDepositRate.GetDemandDepositRate(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<DemandDepositRate> GetDemandDepositRateTariffs()
        {
            try
            {
                return DemandDepositRate.GetDemandDepositRateTariffs();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public short GetCustomerFilial()
        {
            try
            {
                return Customer.GetCustomerFilial(AuthorizedCustomer.CustomerNumber).key;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ulong GetBankruptcyManager(string accountNumber)
        {
            try
            {
                return Account.GetBankruptcyManager(accountNumber);
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
                return LoanProductOrder.GetDepositLoanCreditLineAndProfisionCoefficent(loanCurrency, provisionCurrency, mandatoryPayment, creditLineType);
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
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);

                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;



                return customer.SaveReceivedFastTransferPaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order)
        {
            try
            {
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                ActionResult result = customer.ApproveReceivedFastTransferPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
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
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetReceivedFastTransferOrderRejectReason(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetStatementFixedReceivingType(string cardnumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetStatementFixedReceivingType(cardnumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<OrderHistory> GetOnlineOrderHistory(long orderId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOnlineOrderHistory(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        /// Պրոդուկտի ներկայացվող տեղեկատվությունների կարգավորումների հայտի պահպանում
        /// order.OperationType=1 -մուտքագրում (OrderType.ProductNotificationConfigurationsOrder),
        /// order.OperationType=2 -Խմբագրում   (OrderType.ProductNotificationConfigurationsUpdateOrder), 
        /// order.OperationType=3 -Հեռացում    (OrderType.ProductNotificationConfigurationsDeleteOrder)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveProductNotificationConfigurationsOrder(ProductNotificationConfigurationsOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveProductNotificationConfigurationsOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        ///Վերադարձնում է տվյալ պրոդուկտի բոլոր կարգավորումները 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<ProductNotificationConfigurations> GetProductNotificationConfigurations(ulong productId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetProductNotificationConfigurations(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CashierCashLimit> GetCashierLimits(int setNumber)
        {
            try
            {
                return CashierCashLimit.GetCashierLimits(setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult GenerateCashierCashDefaultLimits(int setNumber, int changeSetNumber)
        {
            try
            {
                return CashierCashLimit.GenerateCashierCashDefaultLimits(setNumber, changeSetNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveCashierCashLimits(List<CashierCashLimit> limit)
        {
            try
            {
                return CashierCashLimit.SaveCashierCashLimits(limit);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCashierFilialCode(int setNumber)
        {
            try
            {
                return CashierCashLimit.GetCashierFilialCode(setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveBranchDocumentSignatureSetting(DocumentSignatureSetting setting)
        {
            try
            {
                setting.User = User;
                return setting.SaveBranchDocumentSignatureSetting();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DocumentSignatureSetting GetBranchDocumentSignatureSetting()
        {
            try
            {

                return DocumentSignatureSetting.GetBranchDocumentSignatureSetting(User.filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<Account> GetDecreasingDepositAccountList(ulong customerNumber)
        {
            try
            {
                return Account.GetDecreasingDepositAccountList(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<SwiftMessage> GetSearchedSwiftMessages(SearchSwiftMessage searchSwiftMessage)
        {
            try
            {

                return searchSwiftMessage.GetSearchedSwiftMessages();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public SwiftMessage GetSwiftMessage(ulong messageUnicNumber)
        {
            try
            {
                return SwiftMessage.GetSwiftMessage(messageUnicNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public CardServiceQualities GetCardUSSDService(ulong productID)
        {
            try
            {
                return Card.GetCardUSSDService(productID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ActionResult SaveAndApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePlasticCardSMSServiceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApprovePlasticCardSMSServiceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SavePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePlasticCardSMSServiceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public string GetCardMobilePhone(ulong productID)
        {
            try
            {
                return CardUSSDServiceOrder.GetCardMobilePhone(productID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardUSSDServiceOrder(CardUSSDServiceOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardUSSDServiceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardUSSDServiceOrder GetCardUSSDServiceOrder(long orderId)
        {
            CardUSSDServiceOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardUSSDServiceOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<float> GetCardUSSDServiceTariff(ulong productID)
        {
            List<float> list = new List<float>();
            try
            {
                list = CardTariffContract.GetCardUSSDServiceTariff(productID);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<float> GetPlasticCardSMSServiceTariff(ulong productID)
        {
            List<float> list = new List<float>();
            try
            {
                list = CardTariffContract.GetPlasticCardSMSServiceTariff(productID);
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public ActionResult CustomersClassification()
        {
            ActionResult result = new ActionResult();
            try
            {
                ClassifiedLoan.CustomersClassification(User, Source);
                result.ResultCode = ResultCode.Normal;
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                ActionError er = new ActionError();
                er.Description = Utility.ConvertAnsiToUnicode(ex.Message);
                result.Errors.Add(er);
                WriteLog(ex);
            }
            return result;
        }

        public List<HBProductPermission> GetHBUserProductsPermissions(string hbUserName)
        {
            try
            {
                return Customer.GetHBUserProductsPermissions(hbUserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        private ulong GetTestMobileCustomerNumber()
        {
            ulong customerNumber;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string txt = @"SELECT TOP 1 customer_number FROM tbl_api_test_customer_number";

                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    conn.Open();

                    customerNumber = Convert.ToUInt64(cmd.ExecuteScalar());
                }
            }


            return customerNumber;
        }

        public AuthorizedCustomer GetTestMobileBankingUser()
        {
            AuthorizedCustomer AutorizedCustomer = new AuthorizedCustomer();

            AutorizedCustomer.CustomerNumber = 102100033996; //103200030603;//this.GetTestMobileCustomerNumber(); 100000071433;  //103600004797; //101200003781; //100000004089;//100900047419;//100900047419
            AutorizedCustomer.UserName = "AIMDAN";//TestMobileBankingUser
            AutorizedCustomer.DailyTransactionsLimit = 15000000;
            AutorizedCustomer.Permission = 5;
            AutorizedCustomer.OneTransactionLimit = 5000000;
            AutorizedCustomer.ApprovementScheme = 1;
            AutorizedCustomer.LimitedAccess = 0;
            return AutorizedCustomer;
        }



        public ActionResult SaveAndApproveTransactionSwiftConfirmOrder(TransactionSwiftConfirmOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveTransactionSwiftConfirmOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveSwiftMessageRejectOrder(SwiftMessageRejectOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrderForSwiftMessage(order);
                return customer.SaveAndApproveSwiftMessageRejectOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }





        public Account GetAccountInfo(string accountNumber)
        {
            try
            {
                return Account.GetAccount(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public List<LoanEquipment> GetSearchedLoanEquipment(SearchLoanEquipment searchLoanEquipment)
        {
            try
            {
                return searchLoanEquipment.GetSearchedLoanEquipments();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public LoanEquipment GetSumsOfEquipmentPrices(SearchLoanEquipment searchLoanEquipment)
        {
            try
            {
                return searchLoanEquipment.GetSumsOfEquipmentPrices();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public TransactionSwiftConfirmOrder GetTransactionSwiftConfirmOrder(long orderId)
        {
            TransactionSwiftConfirmOrder order;
            try
            {
                Customer customer = CreateCustomer();
                order = customer.GetTransactionSwiftConfirmOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public string GetSwiftMessage940Statement(DateTime dateFrom, DateTime dateTo, string accountNumber, SourceType source)
        {
            string statement = "";
            try
            {
                statement = SwiftMessage.GetSwiftMessage940Statement(dateFrom, dateTo, accountNumber, source);
                return statement;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public Card GetCardWithOutBallance(string accountNumber)
        {
            try
            {
                return Card.GetCardWithOutBallance(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult SaveAndApproveCard3DSecureServiceOrder(Card3DSecureServiceOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCard3DSecureServiceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Card3DSecureService GetCard3DSecureService(ulong productID)
        {
            try
            {
                return Card.GetCard3DSecureService(productID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Card3DSecureService> GetCard3DSecureServiceHistory(ulong productID)
        {
            try
            {
                List<Card3DSecureService> result = Card3DSecureServiceOrder.GetCard3DSecureServiceHistory(productID);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CardUSSDServiceHistory> GetCardUSSDServiceHistory(ulong productID)
        {
            try
            {
                List<CardUSSDServiceHistory> result = CardUSSDServiceHistory.GetCardUSSDServiceHistory(productID);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PlasticCardSMSServiceHistory GetPlasticCardSMSServiceHistory(string cardNumber)
        {
            try
            {
                PlasticCardSMSServiceHistory result = PlasticCardSMSServiceHistory.GetPlasticCardSMSServiceHistory(cardNumber);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanEquipment GetEquipmentDetails(int equipmentID)
        {
            try
            {
                return LoanEquipment.GetLoanEquipmentDetails(equipmentID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetEquipmentClosingReason(int equipmentID)
        {
            try
            {
                return LoanEquipment.GetEquipmentClosingReason(equipmentID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult LoanEquipmentClosing(int equipmentID, int setNumber, string closingReason)
        {
            try
            {
                return LoanEquipment.LoanEquipmentClosing(equipmentID, setNumber, closingReason);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ChangeCreditProductMatureRestriction(double appID, int setNumber, int allowMature)
        {
            try
            {
                return LoanEquipment.ChangeCreditProductMatureRestriction(appID, setNumber, allowMature);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Account> GetCustomerTransitAccounts(ProductQualityFilter filter)
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCustomerTransitAccounts(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ProductNotificationConfigurationsOrder GetProductNotificationConfigurationOrder(long ID)
        {
            ProductNotificationConfigurationsOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetProductNotificationConfigurationOrder(ID);
                return order;
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

        public BondIssue GetBondIssue(int id)
        {
            try
            {
                BondIssue bondIssue = new BondIssue();
                bondIssue.ID = id;
                return bondIssue.GetBondIssue();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveBondIssue(BondIssue bondissue)
        {
            try
            {
                return bondissue.SaveBondIssue(User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult DeleteBondIssue(int id)
        {
            try
            {
                BondIssue bondIssue = new BondIssue();
                bondIssue.ID = id;
                bondIssue.GetBondIssue();
                return bondIssue.DeleteBondIssue(User);
            }

            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveBondIssue(int id)
        {
            try
            {
                BondIssue bondIssue = new BondIssue();
                bondIssue.ID = id;
                bondIssue.GetBondIssue();
                return bondIssue.ApproveBondIssue(User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<BondIssue> SearchBondIssues(BondIssueFilter searchParams)
        {
            try
            {
                List<BondIssue> result = BondIssueFilter.SearchBondIssues(searchParams);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DateTime> CalculateCouponRepaymentSchedule(BondIssue bondissue)
        {
            try
            {
                List<DateTime> result = bondissue.CalculateCouponRepaymentSchedule();
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Bond GetBondByID(int ID)
        {
            try
            {
                return Bond.GetBondByID(ID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Bond> GetBonds(BondFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetBonds(filter);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveBondOrder(BondOrder order)
        {
            try
            {
                if (order.Bond.ShareType == SharesTypes.Bonds)
                    order.SubType = 1;
                else
                    order.SubType = 2;

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveBondOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public BondOrder GetBondOrder(long ID)
        {
            BondOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetBondOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public int GetNonDistributedBondsCount(int bondIssueId)
        {
            try
            {
                return BondIssue.GetNonDistributedBondsCount(bondIssueId);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetAccountsForCouponRepayment()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountsForCouponRepayment();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetAccountsForBondRepayment(string currency)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountsForBondRepayment(currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetBondPrice(int bondIssueId)
        {
            try
            {
                double price = Bond.GetBondPrice(bondIssueId);
                return price;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveBondQualityUpdateOrder(BondQualityUpdateOrder order)
        {
            try
            {
                Customer customer = new Customer();
                short ApprovementScheme = 1;
                if (AuthorizedCustomer != null)
                {
                    ApprovementScheme = AuthorizedCustomer.ApprovementScheme;
                    customer = CreateCustomer();
                    InitOrder(order);

                }
                else
                {

                    Bond bond = Bond.GetBondByID(order.BondId);
                    if (bond != null && bond.ID != 0)
                    {
                        order.CustomerNumber = Bond.GetBondByID(order.BondId).CustomerNumber;
                    }

                    AuthorizedCustomer = new AuthorizedCustomer();
                    AuthorizedCustomer.CustomerNumber = order.CustomerNumber;


                    InitOrder(order);
                    customer.User = User;
                    customer.Source = Source;

                }

                return customer.SaveAndApproveBondQualityUpdateOrder(order, User.userName, ApprovementScheme);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public BondQualityUpdateOrder GetBondQualityUpdateOrder(long ID)
        {
            BondQualityUpdateOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetBondQualityUpdateOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public bool HasCustomerDepositaryAccountInBankDB(ulong customerNumber)
        {
            try
            {
                return DepositaryAccount.HasCustomerDepositaryAccount(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DepositaryAccount GetCustomerDepositaryAccount(ulong customerNumber)
        {
            try
            {
                bool hasAccount = false;
                return DepositaryAccount.GetCustomerDepositaryAccount(customerNumber, ref hasAccount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DepositaryAccount> GetCustomerDepositaryAccounts(ulong customerNumber)
        {
            try
            {
                return DepositaryAccount.GetCustomerDepositaryAccounts(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Bond> GetBondsForDealing(BondFilter searchParams, string bondFilterType)
        {
            try
            {
                List<Bond> bondDealingList = Bond.GetBondsForDealing(searchParams, bondFilterType);
                return bondDealingList;
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }
        }

        public ActionResult SaveAndApproveBondAmountChargeOrder(BondAmountChargeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveBondAmountChargeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public BondAmountChargeOrder GetBondAmountChargeOrder(long ID)
        {
            BondAmountChargeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetBondAmountChargeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public DepositaryAccount GetDepositaryAccountById(int id)
        {
            try
            {
                return DepositaryAccount.GetDepositaryAccountById(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveDepositaryAccountOrder(DepositaryAccountOrder order)
        {
            try
            {
                Customer customer = new Customer();
                if (AuthorizedCustomer == null)
                {
                    AuthorizedCustomer = new AuthorizedCustomer();
                    AuthorizedCustomer.CustomerNumber = order.CustomerNumber;

                    customer = CreateCustomer();
                    InitOrder(order);
                    return customer.SaveAndApproveDepostitaryAccountOrder(order, User.userName, 1);

                }
                else
                {
                    customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                    InitOrder(order);
                    customer.User = User;
                    customer.Source = Source;
                    return customer.SaveAndApproveDepostitaryAccountOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                }

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DepositaryAccountOrder GetDepositaryAccountOrder(long id)
        {
            DepositaryAccountOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetDepositaryAccountOrder(id);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DateTime> GetCouponRepaymentSchedule(BondIssue bondissue)
        {
            try
            {
                List<DateTime> result = bondissue.GetCouponRepaymentSchedule();
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveLoanProductMakeOutBalanceOrder(LoanProductMakeOutBalanceOrder order, bool includingSurcharge = false)
        {
            ulong SurchargeAppId = 0;
            ActionResult result = new ActionResult();
            ActionResult res = new ActionResult();
            try
            {
                order.Type = OrderType.LoanProductMakeOutBalanceOrder;
                Customer customer = CreateCustomer();
                InitOrder(order);
                res = customer.SaveAndApproveLoanProductMakeOutBalanceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                result.Errors.AddRange(res.Errors);
                result.ResultCode = res.ResultCode;

                if (includingSurcharge)
                {
                    SurchargeAppId = Loan.GetSurchargeAppId(order.ProductId);
                    if (SurchargeAppId != 0)
                    {
                        order.ProductId = SurchargeAppId;
                        order.ProductType = 4;
                        res = customer.SaveAndApproveLoanProductMakeOutBalanceOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                        result.Errors.AddRange(res.Errors);
                        if (res.ResultCode != ResultCode.Normal)
                        {
                            result.ResultCode = res.ResultCode;
                        }

                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<Fond> GetFonds(ProductQualityFilter filter)
        {
            try
            {
                //Customer customer = CreateCustomer();
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFonds(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }


        }


        public Fond GetFondByID(int ID)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFondByID(ID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public FondOrder GetFondOrder(long ID)
        {
            FondOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetFondOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public ActionResult SaveAndApproveFondOrderr(FondOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveFondOrderr(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }



        public double GetCashBookAmount(int cashBookID)
        {
            try
            {
                return CashBook.GetCashBookAmount(cashBookID);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool HasUnconfirmedOrder(int cashBookID)
        {
            try
            {
                return CashBook.HasUnconfirmedOrder(cashBookID);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCreditLineProlongationOrder(CreditLineProlongationOrder order)
        {
            ActionResult result = new ActionResult();
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                result = customer.SaveAndApproveCreditLineProlongationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                return result;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CreditLineProlongationOrder GetCreditLineProlongationOrder(int id)
        {
            CreditLineProlongationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCreditLineProlongationOrder(id);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public ActionResult SaveAndApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePeriodicDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public PeriodicTransferDataChangeOrder GetPeriodicDataChangeOrder(long ID)
        {

            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPeriodicDataChangeOrder(ID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveLoanProductDataChangeOrder(LoanProductDataChangeOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveLoanProductDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public LoanProductDataChangeOrder GetLoanProductDataChangeOrder(long ID)
        {
            LoanProductDataChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetLoanProductDataChangeOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public bool ExistsLoanProductDataChange(ulong appId)
        {
            try
            {
                return LoanProductDataChangeOrder.ExistsLoanProductDataChange(appId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ConfirmOrRejectDepositProductPrices(string listOfId, int confirmationSetNumber, byte status, string rejectionDescription)
        {
            try
            {
                return DepositProductPrices.ConfirmOrRejectDepositProductPrices(listOfId, confirmationSetNumber, status, rejectionDescription);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult DeleteDepositProductPrices(int id, int registrationSetNumber)
        {
            try
            {
                return DepositProductPrices.DeleteDepositProductPrices(id, registrationSetNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult AddDepositProductPrices(DepositProductPrices product)
        {
            try
            {
                return DepositProductPrices.AddDepositProductPrices(product);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DepositProductPrices> GetDepositProductPrices(SearchDepositProductPrices searchProduct)
        {
            try
            {
                return searchProduct.GetSearchedLoanEquipments();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult UpdateDepositPrices(DepositProductPrices product)
        {
            try
            {
                return DepositProductPrices.UpdateDepositPrices(product);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public List<Account> GetCreditCodesTransitAccounts(ProductQualityFilter filter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCreditCodesTransitAccounts(filter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public string GetInternationalTransferSentTime(int docID)
        {
            try
            {
                return InternationalPaymentOrder.GetInternationalTransferSentTime(docID);
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
                return Account.HasOrHadAccount(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }



        public FTPRate GetFTPRateDetails(FTPRateType rateType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetFTPRateDetails(rateType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveFTPRateOrder(FTPRateOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveFTPRateOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<OperDayOptions> SearchOperDayOptions(OperDayOptionsFilter searchParams)
        {
            try
            {
                List<OperDayOptions> operDayOptions = OperDayOptionsFilter.SearchOperDayOptions(searchParams);
                return operDayOptions;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveOperDayOptions(List<OperDayOptions> list)
        {
            try
            {
                return OperDayOptions.SaveOperDayOptions(list);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public FTPRateOrder GetFTPRateOrder(long ID)
        {
            FTPRateOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetFTPRateOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<Account> GetFactoringCustomerCardAndCurrencyAccounts(ulong productId, string currency)
        {
            try
            {
                return LoanProductActivationOrder.GetFactoringCustomerCardAndCurrencyAccounts(productId, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<Account> GetFactoringCustomerFeeCardAndCurrencyAccounts(ulong productId)
        {
            try
            {
                return LoanProductActivationOrder.GetFactoringCustomerFeeCardAndCurrencyAccounts(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveOperDayMode(OperDayMode operDayMode)
        {
            try
            {
                operDayMode.SetNumber = User.userID;
                return operDayMode.SaveOperDayMode(operDayMode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<OperDayMode> GetOperDayModeHistory(OperDayModeFilter searchParams)
        {
            try
            {
                List<OperDayMode> operDayMode = OperDayModeFilter.SearchOperDayMode(searchParams);
                return operDayMode;
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
                return OperDayMode.GetCurrentOperDay24_7_Mode();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ProblemLoanTax GetProblemLoanTaxDetails(long ClaimNumber)
        {
            try
            {
                return ProblemLoanTax.GetProblemLoanTaxDetails(ClaimNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<int, List<ProblemLoanTax>> SearchProblemLoanTax(ProblemLoanTaxFilter problemLoanTaxFilter, bool Cache)
        {
            try
            {
                return ProblemLoanTaxFilter.SearchProblemLoanTax(problemLoanTaxFilter, Cache);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<UtilityOptions> SearchUtilityOptions(UtilityOptionsFilter searchParams)
        {
            try
            {
                List<UtilityOptions> utilityOptions = UtilityOptionsFilter.SearchOperDayOptions(searchParams);
                return utilityOptions;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<UtilityOptions> GetUtiltyForChange()
        {
            try
            {
                List<UtilityOptions> list = UtilityOptions.GetUtiltyForChange();
                return list;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions)
        {
            try
            {
                return UtilityOptions.SaveUtilityConfigurationsAndHistory(utilityOptions);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAllUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions, int a)
        {
            try
            {
                return UtilityOptions.SaveAllUtilityConfigurationsAndHistory(utilityOptions, a);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }
        }

        public List<string> GetExistsNotSentAndSettledRows(Dictionary<int, bool> keyValues)
        {
            try
            {
                List<string> list = UtilityOptions.GetExistsNotSentAndSettledRows(keyValues);
                return list;
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
                BeelineAbonentSearch beelineAbonentSearch = new BeelineAbonentSearch();

                decimal? debt = null;
                string str = beelineAbonentSearch.GetBeelineAbonentBalance(abonentNumber).Balance;
                if (str != null)
                    return debt = Convert.ToDecimal(str);
                else
                    return null;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                arcaCardsTransactionOrder.IPAddress = ClientIp;
                InitOrder(arcaCardsTransactionOrder);
                return customer.SaveAndApproveArcaCardsTransactionOrder(arcaCardsTransactionOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveCardLimitChangeOrder(CardLimitChangeOrder cardLimitChangeOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                cardLimitChangeOrder.IPAddress = ClientIp;
                InitOrder(cardLimitChangeOrder);
                return customer.SaveAndApproveCardLimitChangeOrder(cardLimitChangeOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public short GetBlockingReasonForBlockedCard(string cardNumber)
        {
            short reasonId;
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                reasonId = customer.GetBlockingReasonForBlockedCard(cardNumber);
                return reasonId;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetAccountFreezeReasonsTypesForOrder(bool isHb = false)
        {
            Dictionary<string, string> reasonTypes;
            try
            {
                reasonTypes = Info.GetAccountFreezeReasonsTypesForOrder(User, isHb);
                return reasonTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public double GetTaxForForgiveness(ulong customerNumber, double? capital, string RebetType, string currency)
        {
            return CreditCommitmentForgivenessOrder.GetTax(customerNumber, capital, RebetType, currency);
        }

        public ActionResult SaveForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(creditCommitmentForgiveness);
                return customer.SaveForgivableLoanCommitment(creditCommitmentForgiveness, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CreditCommitmentForgivenessOrder GetForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetForgivableLoanCommitment(creditCommitmentForgiveness);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CreditCommitmentForgivenessOrder GetCreditCommitmentForgiveness(long ID)
        {

            CreditCommitmentForgivenessOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCreditCommitmentForgiveness(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Թույլատրված հաշվետվությունների ցանկի ստացում
        /// </summary>
        /// <param name="userReportPermissionInfo"></param>
        /// <returns></returns>
        public List<InfSecServiceReference.ApplicationClientPermissions> GetPermittedReports(InfSecServiceReference.ApplicationClientPermissionsInfo userReportPermissionInfo)
        {
            try
            {
                userReportPermissionInfo.userID = User.userID;
                userReportPermissionInfo.userGroupID = User.userPermissionId;
                return AuthorizationService.GetPermittedReports(userReportPermissionInfo);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public InterestMargin GetInterestMarginDetails(InterestMarginType marginType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetInterestMarginDetails(marginType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public InterestMargin GetInterestMarginDetailsByDate(InterestMarginType marginType, DateTime marginDate)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetInterestMarginDetailsByDate(marginType, marginDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveInterestMarginOrder(InterestMarginOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveInterestMarginOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public InterestMarginOrder GetInterestMarginOrder(long ID)
        {
            InterestMarginOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetInterestMarginOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApprovePlasticCardRemovalOrder(PlasticCardRemovalOrder cardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardOrder);
                return customer.SaveAndApprovePlasticCardRemovalOrder(cardOrder, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PlasticCardOrder GetPlasticCardOrder(long orderID)
        {
            PlasticCardOrder plasticCardOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                plasticCardOrder = customer.GetPlasticCardOrder(orderID);
                return plasticCardOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<PlasticCard> GetCustomerMainCards()
        {
            try
            {

                List<PlasticCard> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCustomerMainCards();
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PlasticCardRemovalOrder GetPlasticCardRemovalOrder(long orderID)
        {
            PlasticCardRemovalOrder plasticCardRemovalOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                plasticCardRemovalOrder = customer.GetPlasticCardRemovalOrder(orderID);
                return plasticCardRemovalOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        /// Քարտային հաշիվների հեռացում
        /// </summary>
        /// <param name="cardOrder"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardAccountRemovalOrder(CardAccountRemovalOrder cardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardOrder);
                return customer.SaveAndApproveCardAccountRemovalOrder(cardOrder, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        /// Վերադարձնում է հաճախորդի բոլոր քարտերը, անկախ հաշվի առկայությունից 
        /// </summary>
        /// <returns></returns>
        public List<PlasticCard> GetCustomerPlasticCards()
        {
            try
            {
                List<PlasticCard> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCustomerPlasticCards();
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        /// Հեռացնում է նախորդ խմբաքանակի չձևավորված բոլոր հայտերը
        /// </summary>
        public ActionResult ResetIncompletePreOrderDetailQuality()
        {
            try
            {
                return PreOrder.ResetIncompletePreOrderDetailQuality();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        /// <summary>
        /// Վերադարձնում է չձևավորված ապառիկ հայտերի քանակը
        /// </summary>
        public int GetIncompletePreOrdersCount()
        {
            try
            {
                return PreOrder.GetIncompletePreOrdersCount();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void DeleteInsurance(long insuranceId)
        {
            try
            {
                InsuranceOrder.DeleteInsurance(insuranceId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool HasPermissionForDelete(short setNumber)
        {
            try
            {
                return InsuranceOrder.HasPermissionForDelete(setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardAccountRemovalOrder GetCardAccountRemovalOrder(long orderID)
        {
            CardAccountRemovalOrder cardAccountRemovalOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cardAccountRemovalOrder = customer.GetCardAccountRemovalOrder(orderID);
                return cardAccountRemovalOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public VivaCellBTFCheckDetails CheckVivaCellTransferPossibility(string transferNote, double amount)
        {
            VivaCellBTFCheckDetails vivaCellBTFCheck = new VivaCellBTFCheckDetails();
            vivaCellBTFCheck.CheckTransferPossibility(transferNote, amount);
            return vivaCellBTFCheck;
        }

        public string GetArrestTypesList()
        {
            try
            {
                return ArrestTypes.GetArrestTypesList();

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetArrestsReasonTypesList()
        {
            try
            {
                return ArrestsReasonTypes.GetArrestsReasonTypesList();

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string PostNewAddedCustomerArrestInfo(CustomerArrestInfo obj)
        {
            try
            {
                return CustomerArrestInfo.PostNewAddedCustomerArrestInfo(obj);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string RemoveCustomerArrestInfo(CustomerArrestInfo obj)
        {
            try
            {
                return CustomerArrestInfo.RemoveCustomerArrestInfo(obj);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCustomerArrestsInfo(ulong customerNumber)
        {
            try
            {

                return CustomerArrestInfo.GetCustomerArrestsInfo(customerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ChangeProblemLoanTaxQuality(ulong taxAppId)
        {
            try
            {
                return Claim.ChangeProblemLoanTaxQuality(taxAppId, User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong GetCustomerNumberForArrests()
        {
            try
            {
                Customer customer = CreateCustomer();
                ulong customerNumber = customer.CustomerNumber;

                return customerNumber;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetSetNumberInfo(UserInfoForArrests obj)
        {
            try
            {
                short groupId = CustomerArrestInfo.GetSetNumberInfo(obj);

                ApplicationClientPermissionsInfo permission = new ApplicationClientPermissionsInfo();
                permission.progName = "Vark_New";
                permission.formName = "Frm_Info";
                permission.userGroupID = groupId;

                List<ApplicationClientPermissions> list = new List<ApplicationClientPermissions>();

                AuthorizationService.Use(client =>
                {
                    list = client.GetPermissionsForForm(permission);
                });

                if (list.Count != 0)
                {
                    if (list[0].nameOfControl == "Cmd_CenterEmploye")
                    {
                        obj.CenterEmploye = true;
                        obj.IsChief = false;
                    }
                    else if (list[0].nameOfControl == "Cmd_Chief")
                    {
                        obj.IsChief = true;
                        obj.CenterEmploye = false;
                    }
                }

                return new JavaScriptSerializer().Serialize(obj);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CheckCustomerArrests GetCustomerHasArrests(ulong customerNumber)
        {
            try
            {
                return CustomerArrestInfo.GetCustomerHasArrests(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);



            }
        }

        public ActionResult ConfirmOrderOnline(long id)
        {
            return Order.ConfirmOrderOnline(id, User);
        }

        /// <summary>
        /// Վերադարձնում է գազ-ի Report-ի անհրաժեշտ դաշտերը
        /// </summary>
        /// <param name="abonentNumber"></param>
        /// <param name="branchCode"></param>
        /// <returns></returns>
        public Dictionary<string, string> SerchGasPromForReport(string abonentNumber, string branchCode)
        {
            try
            {
                return CommunalDetails.SerchGasPromForReport(abonentNumber, branchCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveVirtualCardStatusChangeOrder(VirtualCardStatusChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveVirtualCardStatusChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public VirtualCardStatusChangeOrder GetVirtualCardStatusChangeOrder(long orderId)
        {
            VirtualCardStatusChangeOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetVirtualCardStatusChangeOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult ReSendVirtualCardRequest(int requestId)
        {
            ActionResult result = new ActionResult();
            try
            {
                result = Card.ReSendVirtualCardrequest(requestId, User.userID);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        //HB 

        public List<string> GetTreansactionConfirmationDetails(long docId, long debitAccount)
        {
            try
            {
                return HBDocuments.GetTreansactionConfirmationDetails(docId, debitAccount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string ConfirmReestrTransaction(long docId, int bankCode, short setNumber)
        {
            try
            {
                return HBDocuments.ConfirmReestrTransaction(docId, bankCode, setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ReestrTransferAdditionalDetails> CheckHBReestrTransferAdditionalDetails(long orderId, List<ReestrTransferAdditionalDetails> details)
        {
            try
            {
                return HBDocuments.CheckHBReestrTransferAdditionalDetails(orderId, details);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<HBDocuments> GetHBDocumentsList(HBDocumentFilters obj)
        {
            try
            {
                return HBDocuments.GetHBDocumentsList(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<HBDocuments> GetSearchedHBDocuments(HBDocumentFilters obj)
        {
            try
            {
                return HBDocuments.GetSearchedHBDocuments(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public HBDocumentTransactionError GetTransactionErrorDetails(long transctionCode)
        {
            try
            {
                return HBDocuments.GetTransactionErrorDetails(transctionCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<HBDocumentConfirmationHistory> GetConfirmationHistoryDetails(long transctionCode)
        {
            try
            {
                return HBDocuments.GetConfirmationHistoryDetails(transctionCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCheckingProductAccordance(long transctionCode)
        {
            try
            {
                return HBDocuments.GetCheckingProductAccordance(transctionCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public HBDocumentConfirmationHistory GetProductAccordanceDetails(long transctionCode)
        {
            try
            {
                return HBDocuments.GetProductAccordanceDetails(transctionCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool SetHBDocumentAutomatConfirmationSign(HBDocumentFilters obj)
        {
            try
            {
                return HBDocuments.SetHBDocumentAutomatConfirmationSign(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool ExcludeCardAccountTransactions(HBDocumentFilters obj)
        {
            try
            {
                return HBDocuments.ExcludeCardAccountTransactions(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool SelectOrRemoveFromAutomaticExecution(HBDocumentFilters obj)
        {
            try
            {
                return HBDocuments.SelectOrRemoveFromAutomaticExecution(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetHBArCaBalancePermission(long transctionCode, long accountGroup)
        {
            try
            {
                return HBDocuments.GetHBArCaBalancePermission(transctionCode, accountGroup);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string ConfirmTransactionReject(HBDocuments documents)
        {
            try
            {
                return HBDocuments.ConfirmTransactionReject(documents);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string ChangeTransactionQuality(long transctionCode)
        {
            try
            {
                return HBDocuments.ChangeTransactionQuality(transctionCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string ChangeAutomatedConfirmTime(List<string> info)
        {
            try
            {
                return HBDocuments.ChangeAutomatedConfirmTime(info);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetAutomatedConfirmTime()
        {
            try
            {
                return HBDocuments.GetAutomatedConfirmTime();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool FormulateAllHBDocuments(HBDocumentFilters obj)
        {
            try
            {
                return HBDocuments.FormulateAllHBDocuments(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool GetReestrFromHB(HBDocuments obj)
        {
            try
            {
                return HBDocuments.GetReestrFromHB(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool SaveInternationalPaymentAddresses(InternationalPaymentOrder order)
        {
            try
            {
                return HBDocuments.SaveInternationalPaymentAddresses(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public List<HBMessages> GetHBMessages()
        {
            try
            {
                return HBDocuments.GetHBMessages(User.filialCode, User.AdvancedOptions["WatchAllMessages"]);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<HBMessages> GetSearchedHBMessages(HBMessagesSreach obj)
        {
            try
            {
                return HBDocuments.GetSearchedHBMessages(obj, User.filialCode, User.AdvancedOptions["WatchAllMessages"]);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string PostMessageAsRead(long msgId, int setNumber)
        {
            try
            {
                return HBDocuments.PostMessageAsRead(msgId, setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string PostSentMessageToCustomer(HBMessages obj)
        {
            try
            {
                return HBDocuments.PostSentMessageToCustomer(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<HBMessageFiles> GetMessageUploadedFilesList(long msgId, bool showUploadFilesContent)
        {
            try
            {
                return HBDocuments.GetMessageUploadedFilesList(msgId, showUploadFilesContent);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public int GetCancelTransactionDetails(long docId)
        {
            try
            {
                return HBDocuments.GetCancelTransactionDetails(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ReestrTransferAdditionalDetails> GetTransactionIsChecked(long orderId, List<ReestrTransferAdditionalDetails> details)
        {
            try
            {
                return HBDocuments.GetTransactionIsChecked(orderId, details);
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
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePaymentToARCAOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult DownloadOrderXMLs(DateTime DateTo, DateTime DateFrom)
        {
            try
            {
                return ExternalBanking.CardDeliveryOrder.DownloadOrderXMLs(DateTo, DateFrom, this.User);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public OrderQuality GetOrderQualityByDocID(long docID)
        {
            try
            {
                return Order.GetOrderQualityByDocID(docID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        //HB
        public string GetHBAccountNumber(string cardNumber)
        {
            try
            {
                return HBDocuments.GetHBAccountNumber(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public HBDocuments GetCustomerAccountAndInfoDetails(HBDocuments obj)
        {
            try
            {
                return HBDocuments.GetCustomerAccountAndInfoDetails(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void PostReestrPaymentDetails(ReestrTransferOrder order)
        {
            try
            {
                HBDocuments.PostReestrPaymentDetails(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetcheckedReestrTransferDetails(long docId)
        {
            try
            {
                return HBDocuments.GetcheckedReestrTransferDetails(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool CheckReestrTransactionIsChecked(long docId)
        {
            try
            {
                return HBDocuments.CheckReestrTransactionIsChecked(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public HBMessageFiles GetMsgSelectedFile(int fileId)
        {
            try
            {
                return HBDocuments.GetMsgSelectedFile(fileId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string PostBypassHistory(HBDocumentBypassTransaction obj)
        {
            try
            {
                return HBDocuments.PostBypassHistory(obj);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string PostApproveUnconfirmedOrder(long docId, int setNumber)
        {
            try
            {
                return HBDocuments.PostApproveUnconfirmedOrder(docId, setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void SaveDAHKPaymentType(long orderId, int paymentType, int setNumber)
        {
            try
            {
                PaymentOrder.SaveDAHKPaymentType(orderId, paymentType, setNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetcheckedArmTransferDetails(long docId)
        {
            try
            {
                return HBDocuments.GetcheckedArmTransferDetails(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Tuple<string, string> GetSintAccountForHB(string accountNumber)
        {
            try
            {
                return PaymentOrder.GetSintAccountForHB(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveProductNote(ProductNote productNote)
        {
            try
            {
                return productNote.Save();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ProductNote GetProductNote(double uniqueId)
        {
            try
            {
                return ProductNote.GetProductNote(uniqueId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAccountClosingOrder(AccountClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAccountClosingOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveAccountClosingOrder(AccountClosingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveAccountClosingOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);

                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (order.ClosingReasonType == 7 || order.ClosingReasonType == 11))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveInternationalPaymentOrder(InternationalPaymentOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveInternationalPaymentOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCreditLineTerminationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է որոնման տվյալներին համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="orderFilter"></param>
        /// <returns></returns>
        public List<Order> GetOrdersByFilter(OrderFilter orderFilter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOrders(orderFilter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<DAHKFreezing> GetDahkFreezings()
        {
            try
            {
                List<DAHKFreezing> freezings = DAHKFreezing.GetDahkFreezings(AuthorizedCustomer.CustomerNumber);
                return freezings;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public string GetSwiftMessage950Statement(DateTime dateFrom, DateTime dateTo, string accountNumber, SourceType source)
        {
            string statement = "";

            try
            {
                statement = SwiftMessage.GetSwiftMessageStatement(dateFrom, dateTo, accountNumber, source);
                return statement;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<DepositRepayment> GetDepositRepaymentsPrior(DepositRepaymentRequest request)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositRepayments(request);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Card GetCardByCardNumber(string cardNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCard(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Վերադարձնում է կոմունալ վճարումների հանձնարարականի համար անհրաժեշտ պարամետրերը Online/Mobile համակարգերի համար
        /// </summary>
        public List<KeyValuePair<string, string>> GetCommunalReportParametersIBanking(long orderId, CommunalTypes communalType)
        {
            try
            {
                SearchCommunal searchCommunal = new SearchCommunal();
                return searchCommunal.GetCommunalReportParametersIBanking(orderId, communalType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Պահպանում է գործարքների խումբը
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public ActionResult SaveOrderGroup(OrderGroup group)
        {
            try
            {
                group.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                return group.Save();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveReestrTransferOrder(ReestrTransferOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApproveReestrTransferOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                arcaCardsTransactionOrder.IPAddress = ClientIp;
                InitOrder(arcaCardsTransactionOrder);
                return customer.SaveArcaCardsTransactionOrder(arcaCardsTransactionOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ArcaCardsTransactionOrder GetArcaCardsTransactionOrder(long orderId)
        {
            ArcaCardsTransactionOrder order;
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetArcaCardsTransactionOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                arcaCardsTransactionOrder.IPAddress = ClientIp;
                InitOrder(arcaCardsTransactionOrder);
                return customer.ApproveArcaCardsTransactionOrder(arcaCardsTransactionOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DepositRateTariff GetDepositRateTariff(DepositType depositType)
        {
            try
            {
                return DepositRateTariff.GetDepositRateTariff(depositType, Language);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public string GetPasswordForCustomerDataOrder()
        {
            try
            {
                Customer customer = CreateCustomer();
                string result = customer.GetPasswordForCustomerDataOrder();
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetEmailForCustomerDataOrder()
        {
            try
            {
                Customer customer = CreateCustomer();
                string result = customer.GetEmailForCustomerDataOrder();
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetCardToCardTransferFee(string debitCardNumber, string creditCardNumber, double amount, string currency)
        {
            try
            {
                return Card.GetCardToCardTransferFee(debitCardNumber, creditCardNumber, amount, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardToCardOrder(CardToCardOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardToCardOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardToCardOrder GetCardToCardOrder(long orderId)
        {
            CardToCardOrder order;
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardToCardOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public AttachedCardTransactionReceipt GetAttachedCardTransactionDetails(long orderId)
        {
            AttachedCardTransactionReceipt details;
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                details = customer.GetAttachedCardTransactionDetails(orderId);
                return details;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public string GetEmbossingName(string cardNumber, ulong productId)
        {
            try
            {
                return Card.GetEmbossingName(cardNumber, productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetAttachedCardEmbossingName(string cardNumber)
        {
            try
            {
                return Card.GetAttachedCardEmbossingName(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveCardToCardOrder(CardToCardOrder cardToCardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardToCardOrder);
                return customer.SaveCardToCardOrder(cardToCardOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ToCardWithECommerce(CardToCardOrder cardToCardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardToCardOrder);
                return customer.ToCardWithECommerce(cardToCardOrder);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult ApproveCardToCardOrder(CardToCardOrder cardToCardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardToCardOrder);
                return customer.ApproveCardToCardOrder(cardToCardOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardLimits(long productId)
        {
            Dictionary<string, string> cardLimits = new Dictionary<string, string>();
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cardLimits = customer.GetCardLimits(productId);
                return cardLimits;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveCardLimitChangeOrder(CardLimitChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                order.IPAddress = ClientIp;
                InitOrder(order);
                return customer.SaveCardLimitChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardLimitChangeOrder GetCardLimitChangeOrder(long orderId)
        {
            CardLimitChangeOrder order;
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardLimitChangeOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult ApproveCardLimitChangeOrder(CardLimitChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveCardLimitChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// ՀՀ տարածքում/հաշիվների միջև փոխանցման ձևանմուշի/խմբային ծառայության պահպանում/խմբագրում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult SavePaymentOrderTemplate(PaymentOrderTemplate template)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(template.PaymentOrder);
                template.TemplateCustomerNumber = template.PaymentOrder.CustomerNumber;
                return customer.SavePaymentOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public byte[] GetOpenedAccountContract(string accountNumber)
        {
            try
            {
                return AccountOrder.GetOpenedAccountContract(accountNumber);
            }
            catch (Exception ex)
            {


                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public string SaveUploadedFile(UploadedFile uploadedFile)
        {
            try
            {
                return UploadedFile.SaveUploadedFile(uploadedFile);
            }
            catch (Exception ex)
            {


                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveHBServletRequestOrder(HBServletRequestOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveHBServletRequestOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveHBServletRequestOrder(HBServletRequestOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveHBServletRequestOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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


        public ReadXmlFileAndLog ReadXmlFile(string fileId, short filial)
        {

            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                ReadXmlFileAndLog readXmlFileAndLog = customer.ReadXmlFile(fileId, filial, AuthorizedCustomer.CustomerNumber, AuthorizedCustomer.UserName);
                return readXmlFileAndLog;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveHBApplicationReplacmentOrder(HBApplicationOrder order)
        {
            try
            {
                Customer customer = new Customer(User, AuthorizedCustomer.CustomerNumber, (Languages)Language);
                customer.Source = Source;
                InitOrder(order);
                return customer.SaveAndApproveHBApplicationOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
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
                Customer customer = new Customer(User, AuthorizedCustomer.CustomerNumber, (Languages)Language);
                customer.Source = Source;
                list = customer.GetHBRequests();
                return list;
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


        public List<string> GetHBTokenNumbers(HBTokenTypes tokenType, short filialCode)
        {
            try
            {
                return HBToken.GetHBTokenNumbers(tokenType, filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public HBToken GetHBToken(int tokenId)
        {
            try
            {
                return HBToken.GetHBToken(tokenId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public HBToken GetHBTokenWithSerialNumber(string TokenSerial)
        {
            try
            {
                return HBToken.GetHBToken(TokenSerial);
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
        public List<HBToken> GetFilteredHBTokens(int HBUserID, HBTokenQuality filter)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetHBTokens(HBUserID, filter);
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
                return customer.SaveAndApproveHBServletRequestOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme, ClientIp, Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

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


        public HBUser GetHBUser(int hbUserID)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetHBUser(hbUserID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public HBUser GetHBUserByUserName(string hbUserName)
        {
            try
            {
                Customer customer = new Customer();
                return customer.GetHBUserByUserName(hbUserName);
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

        /// <summary>
        /// Վարկի մարման՝ որպես խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult SaveLoanMatureOrderTemplate(LoanMatureOrderTemplate template)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(template.MatureOrder);
                template.TemplateCustomerNumber = template.MatureOrder.CustomerNumber;
                return customer.SaveLoanMatureOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Կոմունալ վճարման՝ որպես խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult SaveUtiliyPaymentOrderTemplate(UtilityPaymentOrderTemplate template)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(template.UtilityPaymentOrder);
                template.UtilityPaymentOrder.Code = template.UtilityPaymentOrder.Code.Trim();
                template.TemplateCustomerNumber = template.UtilityPaymentOrder.CustomerNumber;
                return customer.SaveUtilityPaymentOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public void SendReminderNote(ulong customerNumber)
        {
            try
            {
                Customer customer = new Customer();
                customer.SendReminderNote(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        /// <summary>
        /// Գործարքների խմբի հեռացում
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public ActionResult DeleteOrderGroup(int groupId)
        {
            try
            {
                OrderGroup group = new OrderGroup();
                group.ID = groupId;
                group.CustomerNumber = AuthorizedCustomer.CustomerNumber;
                return group.Delete();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Ձևանմուշի կարգավիճակի փոփոխում
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult ChangeTemplateStatus(int id, TemplateStatus status)
        {
            try
            {
                return Template.ChangeTemplateStatus(id, status);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CreditLine GetCreditLineByAccountNumber(string loanFullNumber)
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
        public int GetTransferTypeByAppId(ulong appId)
        {
            try
            {
                return PeriodicTransfer.GetTransferTypeByAppId(appId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ծառայությունների խումբը
        /// </summary>
        /// <param name="status">Խմբի կարգավիճակ</param>
        /// <returns></returns>
        public List<OrderGroup> GetOrderGroups(OrderGroupStatus status, OrderGroupType groupType)
        {
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOrderGroups(status, groupType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PaymentOrderTemplate GetPaymentOrderTemplate(int id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPaymentOrderTemplate(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public BudgetPaymentOrderTemplate GetBudgetPaymentOrderTemplate(int id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetBudgetPaymentOrderTemplate(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanMatureOrderTemplate GetLoanMatureOrderTemplate(int id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetLoanMatureOrderTemplate(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public UtilityPaymentOrderTemplate GetUtilityPaymentOrderTemplate(int id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetUtilityPaymentOrderTemplate(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public OrderType GetDocumentType(int docId)
        {
            try
            {
                return Utility.GetDocumentType(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public InternationalPaymentOrder GetCustomerDateForInternationalPayment()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCustomerDateForInternationalPayment();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public bool HasCustomerOnlineBanking(ulong customerNumber)
        {
            try
            {
                return HBToken.HasCustomerOnlineBanking(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        //[AllowAnonymous]
        public infsec.AuthorizedUser AuthorizeUserBySAPTicket(string ticket, string softName)
        {
            try
            {
                return AuthorizationService.AuthorizeUserBySAPTicket(ticket, softName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SavePlasticCardOrder(PlasticCardOrder cardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardOrder);
                return customer.SavePlasticCardOrder(cardOrder, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApprovePlasticCardOrder(PlasticCardOrder cardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardOrder);
                ActionResult result = customer.ApprovePlasticCardOrder(cardOrder, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Քարտից քարտ փոխանցման ձևանմուշի պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult SaveCardToCardOrderTemplate(CardToCardOrderTemplate template)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(template.CardToCardOrder);
                return customer.SaveCardToCardOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Order> GetOrdersList(OrderListFilter orderListFilter)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOrdersList(orderListFilter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsCurrentAccount(string accountNumber)
        {
            Account acc = new Account();
            acc.AccountNumber = accountNumber;
            return acc.IsCurrentAccount();
        }


        public Card GetCardByAccountNumber(string accountNumber)
        {
            Account acc = new Account();
            acc.AccountNumber = accountNumber;
            return Card.GetCard(acc);
        }




        public ActionResult SaveInternationalOrderTemplate(InternationalOrderTemplate template)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(template.InternationalPaymentOrder);
                return customer.SaveInternationalOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsAbleToChangeQuality(string userName, int id)
        {
            try
            {
                string groups = "";
                List<XBUserGroup> XBUserGroups = XBUserGroup.GetXBUserGroups(userName);
                XBUserGroups.ForEach(m =>
                    groups += m.Id + ","
                );
                if (!string.IsNullOrEmpty(groups))
                {
                    groups = "(" + groups.Substring(0, groups.Length - 1) + ")";
                }
                return Order.IsAbleToChangeQuality(userName, groups, id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public InternationalOrderTemplate GetInternationalOrderTemplate(int id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetInternationalOrderTemplate(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CreditLine GetCardOverDraft(string cardNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardOverDraft(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Template> GetCustomerTemplates(TemplateStatus status)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCustomerTemplates(status);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardToCardOrderTemplate GetCardToCardOrderTemplate(int templateId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardToCardOrderTemplate(templateId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveReestrTransferOrder(ReestrTransferOrder order, string fileId)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveReestrTransferOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme, fileId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult CheckExcelRows(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails, string debetAccount, long orderId)
        {
            try
            {
                ActionResult result = new ActionResult();
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                result = customer.CheckExcelRows(reestrTransferAdditionalDetails, debetAccount, (Languages)Language, orderId);
                if (result.ResultCode == ResultCode.ValidationError)
                {
                    Localization.SetCulture(result, new Culture(Languages.hy));
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ulong GetCardCustomerNumber(string cardNumber)
        {
            try
            {
                return Card.GetCardCustomerNumber(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetPlasticCardOrderCardTypes()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPlasticCardOrderCardTypes();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<string> GetDepositAndCurrentAccountCurrencies(OrderType orderType, byte orderSubType, OrderAccountType accountType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetDepositAndCurrentAccountCurrencies((OrderType)orderType, orderSubType, (OrderAccountType)accountType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public double GetRedemptionAmountForDepositLoan(double startCapital, double interestRate, DateTime dateOfBeginning, DateTime dateOfNormalEnd, DateTime firstRepaymentDate)
        {
            try
            {
                return LoanProductOrder.GetRedemptionAmountForDepositLoan(startCapital, interestRate, dateOfBeginning, dateOfNormalEnd, firstRepaymentDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetCommisionAmountForDepositLoan(double startCapital, DateTime dateOfBeginning, DateTime dateofNormalEnd, string currency)
        {
            try
            {
                return LoanProductOrder.GetCommisionAmountForDepositLoan(startCapital, dateOfBeginning, dateofNormalEnd, currency, AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public double GetCreditLineDecreasingAmount(double startCapital, string currency, DateTime startDate, DateTime endDate)
        {
            try
            {
                return LoanProductOrder.GetCreditLineDecreasingAmount(startCapital, currency, startDate, endDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public Dictionary<string, string> GetDepositCreditLineContractInfo(int docId)
        {

            try
            {
                return LoanProductOrder.GetDepositCreditLineContractInfo(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }



        public Dictionary<string, string> GetDepositLoanContractInfo(int docId)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            DataTable dt = new DataTable();
            try
            {

                dt = LoanProductOrder.GetDepositLoanContractInfo(docId);
                if (dt.Rows.Count > 0)
                {
                    dictionary.Add("security_code_2", dt.Rows[0][0].ToString());
                    dictionary.Add("interest_rate_effective", dt.Rows[0][1].ToString());
                    dictionary.Add("credit_code", dt.Rows[0][2].ToString());
                    dictionary.Add("interest_rate_effective_without_account_service_fee", dt.Rows[0][3].ToString());
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
            return dictionary;
        }

        public string GetConnectAccountFullNumber(string currency)
        {
            try
            {
                return LoanProductOrder.GetConnectAccountFullNumber(AuthorizedCustomer.CustomerNumber, currency);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }


        public string GetCardTypeName(string cardNumber)
        {
            try
            {
                return CreditLine.GetCardTypeName(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public byte[] GetDepositLoanOrDepositCreditLineContract(string loanNumber, byte type)
        {
            try
            {
                return LoanProductOrder.GetDepositLoanOrDepositCreditLineContract(loanNumber, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SavePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePeriodicDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                ActionResult result = customer.ApprovePeriodicDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Card> GetLinkedCards()
        {
            try
            {
                return Card.GetLinkedCards(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Card> GetLinkedAndAttachedCards(ulong productId, ProductQualityFilter productFilter = ProductQualityFilter.Opened)
        {
            try
            {
                return Card.GetLinkedAndAttachedCards(productId, productFilter);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetClosedAccounts()
        {
            try
            {
                return Account.GetClosedAccounts(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CardAdditionalInfo GetCardAdditionalInfo(ulong productId)
        {
            try
            {
                return Card.GetCardAdditionalInfo(productId, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetCashBackAmount(ulong productId)
        {
            try
            {
                return Card.GetCashBackAmount(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte[] GetAttachedFile(long docID, int type)
        {
            try
            {
                return UploadedFile.GetAttachedFile(docID, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanStatement GetLoanStatement(string account, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0)
        {
            try
            {
                LoanStatement result = null;
                result = Loan.GetStatement(account, dateFrom, dateTo, minAmount, maxAmount, debCred, transactionsCount, orderByAscDesc, Language);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte[] GetExistingDepositContract(long docId, int type)
        {

            try
            {
                byte[] depositContent = null;

                depositContent = Deposit.GetExistingDepositContract(docId, type);

                return depositContent;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        /// <summary>
        /// Վերադարձնում է կենսաթոշակային ֆոնդի մնացորդը
        /// </summary>
        /// <returns></returns>
        public PensionSystem GetPensionSystemBalance()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetPensionBalance();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Պահպանում է հաճախորդի կողմից մուտքագրված CVV-ի վերաբերյալ նշումը
        /// </summary>
        /// <param name="productId">Քարտի ունիկալ համար</param>
        /// <param name="CVVNote">Հաճախորդի կողմից մուտքագրված CVV</param>
        /// <returns></returns>
        public ActionResult SaveCVVNote(ulong productId, string CVVNote)
        {
            try
            {
                return Card.SaveCVVNote(productId, CVVNote);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool ValidateProductId(ulong productId, ProductType productType)
        {
            try
            {
                return Utility.ValidateProductId(AuthorizedCustomer.CustomerNumber, productId, productType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public bool ValidateDocId(long docId)
        {
            try
            {
                return Utility.ValidateDocId(AuthorizedCustomer.CustomerNumber, docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public bool ValidateAccountNumber(string accountNumber)
        {
            try
            {
                return Utility.ValidateAccountNumber(AuthorizedCustomer.CustomerNumber, accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public VirtualCardDetails GetVirtualCardDetails(string cardNumber, ulong customerNumber)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetVirtualCardDetails(cardNumber, customerNumber, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ActivateAndOpenProductAccounts(ulong productId, ulong customerNumber)
        {
            try
            {
                Customer customer = new Customer(User, customerNumber, Languages.hy);
                customer.Source = Source;
                return customer.ActivateAndOpenProductAccounts(productId, customerNumber, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCVVNote(ulong productId)
        {
            try
            {
                return Card.GetCVVNote(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool ValidateCardNumber(string cardNumber)
        {
            try
            {
                return Utility.ValidateCardNumber(AuthorizedCustomer.CustomerNumber, cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveAccountReOpenOrder(AccountReOpenOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAccountReOpenOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public IBankingHomePage GetIBankingHomePage()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                var result = customer.GetIBankingHomePage();
                return result.Result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<EmployeeSalary> GetEmployeeSalaryList(DateTime startDate, DateTime endDate)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetEmployeeSalaryList(startDate, endDate, AuthorizedCustomer.CustomerNumber, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public EmployeeSalaryDetails GetEmployeeSalaryDetails(int ID)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetEmployeeSalaryDetails(ID, AuthorizedCustomer.CustomerNumber, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public EmployeePersonalDetails GetEmployeePersonalDetails()
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetEmployeePersonalDetails(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Ցույց է տալիս՝ արդյոք հաշիվը POS հաշիվ է, թե ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public bool IsPOSAccount(string accountNumber)
        {
            Account acc = new Account();
            acc.AccountNumber = accountNumber;
            return acc.IsPOSAccount();
        }

        /// <summary>
        /// Բյուջե փոխանցման ձևանմուշի/խմբային ծառայության պահպանում/խմբագրում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult SaveBudgetPaymentOrderTemplate(BudgetPaymentOrderTemplate template)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(template.BudgetPaymentOrder);
                template.TemplateCustomerNumber = template.BudgetPaymentOrder.CustomerNumber;
                return customer.SaveBudgetPaymentOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի հայտի համար հասանելի քարտերի ցանկը
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public List<Card> GetCardsForNewCreditLine(OrderType orderType)
        {
            try
            {
                List<Card> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCardsForNewCreditLine(orderType);
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsEmployee(ulong customerNumber)
        {
            try
            {
                Customer customer = new Customer();
                return customer.IsEmployee(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Փոխարկման ձևանմուշի/խմբային ծառայության պահպանում/խմբագրում
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public ActionResult SaveCurrencyExchangeOrderTemplate(CurrencyExchangeOrderTemplate template)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(template.CurrencyExchangeOrder);
                template.TemplateCustomerNumber = template.CurrencyExchangeOrder.CustomerNumber;
                return customer.SaveCurrencyExchangeOrderTemplate(template, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        /// <summary>
        /// Պայմանագրի առկայության ստուգում
        /// </summary>
        /// <param name="loanAccountNumber"></param>
        /// <param name="type"> 0 վարկի դեպքում, 1 վարկային գծի դեպքում, 2` ընթացիկ հաշիվ</param>
        /// <returns></returns>
        public bool HasUploadedContract(string accountNumber, byte type)
        {
            try
            {
                //վարկի պայմանագրի առկայության ստուգում
                if (type == 0)
                    return Loan.HasUploadedLoanContract(accountNumber);
                //վարկային գծի պայմանագրի առկայության ստուգում
                else if (type == 1)
                    return CreditLine.HasUploadedCreditLineContract(accountNumber);
                else if (type == 3)
                    return Account.HasUploadedAccountContract(accountNumber);
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        /// <summary>
        /// Հաղորդագրության կից ֆայլի ստացում ունիկալ համարով
        /// </summary>
        /// <param name="Id">Ֆայլի ունիկալ համար</param>
        /// <returns></returns>
        public OrderAttachment GetMessageAttachmentById(int Id)
        {
            try
            {
                return Message.GetMessageAttachmentById(Id);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public DigitalAccountRestConfigurations GetCustomerAccountRestConfig(int DigitalUserId)
        {
            try
            {
                DigitalAccountRestConfigurations restConfigurations = new DigitalAccountRestConfigurations();
                return restConfigurations.GetCustomerAccountRestConfig(DigitalUserId, AuthorizedCustomer.CustomerNumber, Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public ActionResult UpdateCustomerAccountRestConfig(List<DigitalAccountRestConfigurationItem> ConfigurationItems)
        {
            try
            {
                DigitalAccountRestConfigurations restConfigurations = new DigitalAccountRestConfigurations();
                return restConfigurations.UpdateCustomerAccountRestConfig(ConfigurationItems);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public ActionResult ResetCustomerAccountRestConfig(int DigitalUserId)
        {
            try
            {
                DigitalAccountRestConfigurations restConfigurations = new DigitalAccountRestConfigurations();
                return restConfigurations.ResetCustomerAccountRestConfig(DigitalUserId, AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public byte[] GetLoansDramContract(long docId, int productType, bool fromApprove, ulong customerNumber)
        {
            try
            {
                return CreditLine.GetLoansDramContract(docId, productType, fromApprove, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte[] PrintDepositLoanContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            try
            {
                Customer customer = CreateCustomer();
                return LoanProductOrder.PrintDepositLoanContract(docId, AuthorizedCustomer.CustomerNumber, fromApprove);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        /// <summary>
        /// Գործարքի մերժում իրավաբանական անձանց օգտագործողների կողմից
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public ActionResult RejectOrder(OrderRejection rejection)
        {
            try
            {
                Customer customer = CreateCustomer();
                rejection.UserName = AuthorizedCustomer.UserName;
                return customer.RejectOrder(rejection);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public string GetOrderAttachmentInBase64(string attachememntId)
        {
            try
            {
                return Order.GetOrderAttachmentInBase64(attachememntId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public long GetCardProductId(string cardNumber, ulong customerNumber)
        {
            try
            {
                return Card.GetCardProductId(cardNumber, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }


        public ActionResult MigrateOldUserToCas(int hbUserId)
        {
            try
            {
                return HBApplicationOrder.MigrateOldUserToCas(hbUserId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public List<string> GetTempTokenList(int tokenCount)
        {
            try
            {
                HBApplicationOrder hBApplicationOrder = new HBApplicationOrder();
                return hBApplicationOrder.GetTempTokenList(tokenCount);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public string CreateLogonTicket(string userSessionToken)
        {
            try
            {
                return AuthorizationService.CreateLogonTicket(userSessionToken);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong GetCardProductIdByAccountNumber(string cardAccountNumber, ulong customerNumber)
        {
            try
            {
                return Card.GetCardProductIdByAccountNumber(cardAccountNumber, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Card> GetClosedCardsForDigitalBanking(ulong customerNumber)
        {
            try
            {
                return Card.GetClosedCardsForDigitalBanking(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCardSystem(string cardNumber)
        {
            try
            {
                return Card.GetCardSystem(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Card> GetNotActivatedVirtualCards(ulong customerNumber)
        {
            try
            {
                return Card.GetNotActivatedVirtualCards(customerNumber);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetCardNumber(long productId)
        {
            try
            {
                return Card.GetCardNumber(productId);
            }
            catch (Exception ex)
            {

                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetLiabilitiesAccountNumberByAppId(ulong appId)
        {
            try
            {
                return Loan.GetLiabilitiesAccountNumberByAppId(appId);
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

        public InternationalOrderPrefilledData GetInternationalOrderPrefilledData(ulong customerNumber)
        {
            try
            {
                InternationalOrderPrefilledData prefilledData = new InternationalOrderPrefilledData(customerNumber);
                return prefilledData;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);

            }

        }

        public string GetOrderRejectReason(long orderId, OrderType type)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetOrderRejectReason(orderId, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ValidateAttachCard(string cardNumber, ulong customerNumber, string cardHolderName)
        {
            try
            {
                return Validation.ValidateAttachCard(cardNumber, customerNumber, cardHolderName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsAbleToApplyForLoan(LoanProductType type)
        {
            try
            {
                return Loan.IsAbleToApplyForLoan(AuthorizedCustomer.CustomerNumber, type);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetAccountsDigitalBanking()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountsDigitalBanking();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetMaxAvailableAmountForNewCreditLine(double productId, int creditLineType, string provisionCurrency, bool existRequiredEntries, ulong customerNumber)
        {
            try
            {

                return Info.GetMaxAvailableAmountForNewCreditLine(productId, creditLineType, provisionCurrency, existRequiredEntries, customerNumber);

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void PostResetEarlyRepaymentFee(ulong productId, string description, bool recovery)
        {
            try
            {
                Loan.PostResetEarlyRepaymentFee(productId, description, recovery, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool GetResetEarlyRepaymentFeePermission(ulong productId)
        {
            try
            {
                return Loan.GetResetEarlyRepaymentFeePermission(productId);
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
                return Loan.IsLoan_24_7(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<OrderForCashRegister> GetOrdersForCashRegister(SearchOrders searchOrders)
        {
            try
            {
                return OrderForCashRegister.GetOrdersForCashRegister(searchOrders);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CardServiceQualities GetPlasticCardSMSService(string cardNumber)
        {
            try
            {
                return Card.GetPlasticCardSMSService(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PlasticCardSMSServiceOrder GetPlasticCardSMSServiceOrder(long orderId)
        {
            PlasticCardSMSServiceOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPlasticCardSMSServiceOrder(orderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public double GetMaxAvailableAmountForNewLoan(string provisionCurrency, ulong customerNumber)
        {
            try
            {
                return Loan.GetMaxAvailableAmountForNewLoan(provisionCurrency, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveLoanDelayOrder(LoanDelayOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveLoanDelayOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public LoanDelayOrder GetLoanDelayOrder(long ID)
        {
            LoanDelayOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetLoanDelayOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public LoanRepaymentDelayDetails GetLoanRepaymentDelayDetails(ulong productId)
        {
            try
            {
                return Customer.GetLoanRepaymentDelayDetails(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetUserTotalAvailableBalance(int digitalUserId, ulong customerNumber, int digitalUserID)
        {
            try
            {
                return Account.GetUserTotalAvailableBalance(digitalUserId, customerNumber, digitalUserID, Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte[] PrintDepositContract(long orderId, bool attachedFile)
        {
            try
            {
                return Deposit.PrintDepositContract(orderId, attachedFile, AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public xbs.Customer GetCustomer(ulong customerNumber)
        {
            var customer = ACBAOperationService.GetCustomer(customerNumber);
            return customer;
        }

        public Dictionary<string, string> GetOrderDetailsForReport(long orderId)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            try
            {

                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                details = customer.GetOrderDetailsForReport(orderId, AuthorizedCustomer.CustomerNumber);
                return details;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public RemittanceDetailsRequestResponse GetRemittanceDetailsByURN(string URN, string authorizedUserSessionToken)
        {
            try
            {
                RemittanceDetailsRequestResponse result = RemittanceDetails.GetRemittanceDetailsByURN(URN, authorizedUserSessionToken, User.userName, ClientIp);
                Culture Culture = new Culture((Languages)Language);
                Localization.SetCulture(result.ActionResult, Culture);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveRemittanceCancellationOrder(RemittanceCancellationOrder order, string authorizedUserSessionToken)
        {
            try
            {
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.SaveAndApproveRemittanceCancellationOrder(order, User.userName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveRemittanceCancellationOrder(RemittanceCancellationOrder order, string authorizedUserSessionToken)
        {
            try
            {
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.SaveRemittanceCancellationOrder(order, User.userName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveRemittanceCancellationOrder(long orderId, string authorizedUserSessionToken)
        {
            try
            {
                RemittanceCancellationOrder order = GetRemittanceCancellationOrder(orderId, authorizedUserSessionToken);

                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.ApproveRemittanceCancellationOrder(order, User.userName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public RemittanceCancellationOrder GetRemittanceCancellationOrder(long id, string authorizedUserSessionToken)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetRemittanceCancellationOrder(id, authorizedUserSessionToken, User.userName, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public RemittanceFeeDataRequestResponse GetRemittanceFeeData(RemittanceFeeInput feeInput, string authorizedUserSessionToken)
        {
            try
            {
                RemittanceFeeDataRequestResponse result = RemittanceFeeData.GetRemittanceFeeData(feeInput, authorizedUserSessionToken, User.userName, ClientIp);
                Culture Culture = new Culture((Languages)Language);
                Localization.SetCulture(result.ActionResult, Culture);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveFastTransferOrder(FastTransferPaymentOrder order)
        {
            try
            {
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.SaveFastTransferOrder(order, User.userName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveFastTransferOrder(long orderId, string authorizedUserSessionToken)
        {
            try
            {
                FastTransferPaymentOrder order = GetFastTransferPaymentOrder(orderId, authorizedUserSessionToken);

                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.ApproveFastTransferOrder(order, User.userName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public RemittanceAmendmentOrder GetRemittanceAmendmentOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetRemittanceAmendmentOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }




        public ActionResult SaveRemittanceAmendmentOrder(RemittanceAmendmentOrder order, string authorizedUserSessionToken)
        {
            try
            {
                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();

                InitOrder(order);
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.SaveRemittanceAmendmentOrder(order, User.userName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveRemittanceAmendmentOrder(long orderId, string authorizedUserSessionToken)
        {
            try
            {
                RemittanceAmendmentOrder order = GetRemittanceAmendmentOrder(orderId);

                ulong customerNumber = order.CustomerNumber;
                Customer customer = CreateCustomer();
                if (order.CustomerNumber == 0)
                    order.CustomerNumber = customerNumber;
                return customer.ApproveRemittanceAmendmentOrder(order, User.userName, AuthorizedCustomer.ApprovementScheme, authorizedUserSessionToken, ClientIp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public R2ARequestOutput SaveAndApproveSTAKPaymentOrder(R2ARequest r2ARequest)
        {
            try
            {
                //Source = source;

                //xbs.User user = new xbs.User();

                ////ToDo STAK  userID
                //user.userID = 2840; // 1733344  // Գործող միջավայրի համար ՊԿ-ն ուրիշ է, Պռոդ գնալուց ՊԵՏՔ է ՓՈԽԵԼ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //this.User = user;

                //AuthorizedCustomer = new AuthorizedCustomer();
                //AuthorizedCustomer.UserName = "";
                //AuthorizedCustomer.ApprovementScheme = 1;

                STAKPaymentOrder order = new STAKPaymentOrder(r2ARequest);
                order.SetCountry(r2ARequest.SenderCountryCode);

                Customer customer = CreateCustomer();

                InitOrder(order);

                return customer.SaveAndApproveSTAKPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);

                //R2ARequestOutput r2ARequestOutput;
                //r2ARequestOutput = order.SaveAndApprove(r2ARequest);

                //return new R2ARequestOutput();  //  r2ARequestOutput;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool IsDebetExportAndImportCreditLine(string debAccountNumber)
        {
            try
            {
                return PaymentOrder.IsDebetExportAndImportCreditLine(debAccountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetOrderServiceFeeByIndex(int indexID)
        {
            try
            {
                xbs.VipData vip = ACBAOperationService.GetCustomerVipData(AuthorizedCustomer.CustomerNumber);
                int vipType = vip.vipType.key;

                if ((vipType < 7 || vipType > 9) || indexID == 925)
                    return Utility.GetPriceInfoByIndex(indexID, "price");

                return 0;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCustomerTemplatesCounts(ulong customerNumber)
        {
            try
            {
                return Template.GetCustomerTemplatesCounts(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetPeriodicTransfersCount(ulong customerNumber, PeriodicTransferTypes transferType)
        {
            try
            {
                return PeriodicTransfer.GetPeriodicTransfersCount(customerNumber, transferType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveCancelLoanDelayOrder(CancelDelayOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCancelLoanDelayOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CancelDelayOrder GetCancelLoanDelayOrder(long ID)
        {
            CancelDelayOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCancelLoanDelayOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SaveCardToOtherCardsOrder(CardToOtherCardsOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCardToOtherCardsOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult ApproveCardToOtherCardsOrder(CardToOtherCardsOrder cardToCardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardToCardOrder);
                return customer.ApproveCardToOtherCardsOrder(cardToCardOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardToOtherCardsOrder GetCardToOtherCardsOrder(long ID)
        {
            CardToOtherCardsOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardToOtherCardsOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool CheckAccountIsClosed(string accountNumber)
        {
            try
            {
                return Account.CheckAccountIsClosed(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetContactsCount()
        {
            try
            {
                return Contact.GetContactsCount(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetLoanAccountNumber(ulong productId)
        {
            try
            {
                return Loan.GetLoanAccountNumber(productId, AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetCardToOtherCardTransferFee(double amount, string currency)
        {
            try
            {
                return CardToOtherCardsOrder.GetCardToOtherCardTransferFee(amount, currency, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool CheckCardIsClosed(string cardNumber)
        {
            try
            {
                return Card.CheckCardIsClosed(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult CheckForCurrencyExchangeOrderTransactionLimit(CurrencyExchangeOrder order)
        {
            try
            {
                return User.CheckForTransactionLimit(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardStatementAddInf GetFullCardStatement(CardStatement statement, string cardnumber, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                return Card.GetFullCardStatement(statement, cardnumber, dateFrom, dateTo, Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
                throw;
            }
        }

        public ActionResult SaveAndApproveNonCreditLineCardReplaceOrder(NonCreditLineCardReplaceOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveNonCreditLineCardReplaceOrder(order, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public NonCreditLineCardReplaceOrder GetNonCreditLineCardReplaceOrder(long ID)
        {
            NonCreditLineCardReplaceOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetNonCreditLineCardReplaceOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCreditLineCardReplaceOrder(CreditLineCardReplaceOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCreditLineCardReplaceOrder(order, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CreditLineCardReplaceOrder GetCreditLineCardReplaceOrder(long ID)
        {
            CreditLineCardReplaceOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCreditLineCardReplaceOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ReplacedCardAccountRegOrder GetReplacedCardAccountRegOrder(long ID)
        {
            ReplacedCardAccountRegOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetReplacedCardAccountRegOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveReplacedCardAccountRegOrder(ReplacedCardAccountRegOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveReplacedCardAccountRegOrder(order, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetCardArCaStatus(ulong productId)
        {
            try
            {
                return Card.GetCardArCaStatus(productId, AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PlasticCard> GetCustomerPlasticCardsForAdditionalData(bool IsClosed)
        {
            try
            {
                List<PlasticCard> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCustomerPlasticCardsForAdditionalData(IsClosed);
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<CardAdditionalData> GetCardAdditionalDatas(string cardnumber, string expirydate)
        {
            try
            {
                List<CardAdditionalData> cardAdditionalDatas;

                cardAdditionalDatas = CardAdditionalData.GetCardAdditionalDatas(cardnumber, expirydate);
                return cardAdditionalDatas;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveCardAdditionalDataOrder(CardAdditionalDataOrder AdditionalDataOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(AdditionalDataOrder);
                return customer.SaveCardAdditionalDataOrder(AdditionalDataOrder, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardAdditionalDataOrder GetCardAdditionalDataOrder(long orderID)
        {
            CardAdditionalDataOrder cardAdditionalDataOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cardAdditionalDataOrder = customer.GetCardAdditionalDataOrder(orderID);
                return cardAdditionalDataOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApprovePINRegOrder(PINRegenerationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApprovePINRegOrder(order, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public PINRegenerationOrder GetPINRegenerationOrder(long ID)
        {
            PINRegenerationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetPINRegenerationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCurrentAccountContractBefore(long docId, int attacheDocType = 0)
        {
            try
            {
                return Account.GetCurrentAccountContractBefore(docId, AuthorizedCustomer.CustomerNumber, attacheDocType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsCreditLineActivateOnApiGate(long orderId)
        {
            try
            {
                return CreditLine.IsCreditLineActivateOnApiGate(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetPreviousBlockUnblockOrderComment(string cardNumber)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                string orderComment = customer.GetPreviousBlockUnblockOrderComment(cardNumber);
                return orderComment;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LoanInterestRateChangeHistory> GetLoanInterestRateChangeHistoryDetails(ulong productID)
        {
            try
            {
                List<LoanInterestRateChangeHistory> loanInterestRateChangeHistoryDetails = LoanInterestRateChangeHistory.GetLoanInterestRateChangeHistoryDetails(productID);
                return loanInterestRateChangeHistoryDetails;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCardTechnology(ulong productId)
        {
            try
            {
                return Card.GetCardTechnology(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ChangeBranchOrder GetChangeBranchOrder(long ID)
        {
            ChangeBranchOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetChangeBranchOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ChangeBranchOrder GetFilialCode(long cardNumber)
        {
            ChangeBranchOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetFilialCode(cardNumber);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveChangeBranchOrder(ChangeBranchOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveChangeBranchOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetConfirmRequiredOrdersCount(string userName, string groups = "")
        {
            try
            {
                groups = "";
                List<XBUserGroup> XBUserGroups = XBUserGroup.GetXBUserGroups(userName);
                XBUserGroups.ForEach(m =>
                    groups += m.Id + ","
                );
                if (!string.IsNullOrEmpty(groups))
                {
                    groups = "(" + groups.Substring(0, groups.Length - 1) + ")";
                }
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetConfirmRequiredOrdersCount(AuthorizedCustomer.CustomerNumber, userName, groups);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DateTime GetTransferSentDateTime(int docID)
        {
            try
            {
                DateTime sentDateTime = Order.GetTransferSentDateTime(docID);
                return sentDateTime;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte[] PrintSwiftCopyOrderFile(long docID)
        {

            try
            {
                return SwiftCopyOrder.PrintSwiftCopyOrderFile(docID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public string GetCustomerHVHH(ulong customerNumber)
        {
            try
            {
                return Customer.GetCustomerHVHH(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardNotRenewOrder(CardNotRenewOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardNotRenewOrder(order, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CardNotRenewOrder GetCardNotRenewOrder(long ID)
        {
            CardNotRenewOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardNotRenewOrder(ID);
                return order;
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
                UcomFixAbonentSearch ucomFixAbonentSearch = new UcomFixAbonentSearch();

                decimal? debt = null;
                string str = ucomFixAbonentSearch.GetUcomFixAbonentSearch(abonentNumber).TotalBalance;
                if (str != null)
                    return debt = Convert.ToDecimal(str);
                else
                    return null;

            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetCardHolderFullName(ulong productId)
        {
            try
            {
                return Card.GetCardHolderFullName(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveCardAccountClosingOrder(CardAccountClosingOrder CardAccountClosingOrder)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                InitOrder(CardAccountClosingOrder);
                return customer.SaveCardAccountClosingOrder(CardAccountClosingOrder, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveAndApproveLoanInterestRateConcessionOrder(LoanInterestRateConcessionOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveLoanInterestRateConcessionOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanInterestRateConcessionOrder GetLoanInterestRateConcessionDetails(ulong productId)
        {
            try
            {
                return LoanInterestRateConcessionOrder.GetLoanInterestRateConcessionDetails(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public CardAccountClosingOrder GetCardAccountClosingOrder(long orderID)
        {
            CardAccountClosingOrder cardAccountClosingOrder;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cardAccountClosingOrder = customer.GetCardAccountClosingOrder(orderID);
                return cardAccountClosingOrder;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public long GetLeasingCustomerNumber(int leasingCustomerNumber)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingCustomerNumber(leasingCustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LeasingCustomerClassification GetLeasingCustomerInfo(long customerNumber)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingCustomerInfo(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<LeasingCustomerClassification> GetLeasingCustomerSubjectiveClassificationGrounds(long customerNumber, bool isActive)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingCustomerSubjectiveClassificationGrounds(customerNumber, isActive);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLeasingReasonTypes(short classificationType)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingReasonTypes(classificationType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Tuple<int, string> GetLeasingRiskDaysCountAndName(byte riskClassCode)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingRiskDaysCountAndName(riskClassCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult AddLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj)
        {
            DateTime registrationDate = GetCurrentOperDay();
            int overdueDays = 0;
            if (Convert.ToInt32(obj.ClassificationType) == 2)
            {
                overdueDays = ((new DateTime(registrationDate.Date.Year, registrationDate.Date.Month, 1).AddMonths(1).AddDays(-1)).Date - obj.ClassificationDate.Date).Days + 1;

                Tuple<int, string> tuple = LeasingCustomerClassification.GetLeasingRiskDaysCountAndName(Convert.ToInt32(obj.RiskClassName));

                int daysCountMin = tuple.Item1;
                string RiskClassName = tuple.Item2;

                if (obj.CalcByDays == false)
                {
                    obj.ClassificationDate = registrationDate.AddDays(-daysCountMin);
                }

                if ((registrationDate - obj.ClassificationDate).Days < 0)
                {
                    ActionError error = new ActionError();
                    error.Description = "Դասակարգման ամսաթիվը չի կարող մեծ լինել գրանցման ամսաթվից:";

                    ActionResult result = new ActionResult();
                    result.Errors = new List<ActionError>();
                    result.ResultCode = ResultCode.ValidationError;
                    result.Errors.Add(error);

                    return result;
                }
            }

            try
            {
                return LeasingCustomerClassification.AddLeasingCustomerSubjectiveClassificationGrounds(obj, overdueDays, registrationDate, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LeasingCustomerClassification GetLeasingCustomerSubjectiveClassificationGroundsByID(int id)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingCustomerSubjectiveClassificationGroundsByID(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult CloseLeasingCustomerSubjectiveClassificationGrounds(long Id)
        {
            try
            {
                DateTime setDate = GetCurrentOperDay();
                return LeasingCustomerClassification.CloseLeasingCustomerSubjectiveClassificationGrounds(Id, User.userID, setDate);
            }

            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LeasingCustomerClassification> GetLeasingConnectionGroundsForNotClassifyingWithCustomer(long customerNumber, byte isActive)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingConnectionGroundsForNotClassifyingWithCustomer(customerNumber, isActive);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<KeyValuePair<string, string>> GetLeasingInterconnectedPersonNumber(long customerNumber)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingInterconnectedPersonNumber(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult AddLeasingConnectionGroundsForNotClassifyingWithCustomer(LeasingCustomerClassification obj)
        {
            try
            {
                DateTime setDate = GetCurrentOperDay();
                return LeasingCustomerClassification.AddLeasingConnectionGroundsForNotClassifyingWithCustomer(obj, setDate, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LeasingCustomerClassification GetLeasingConnectionGroundsForNotClassifyingWithCustomerByID(int id)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingConnectionGroundsForNotClassifyingWithCustomerByID(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult CloseLeasingConnectionGroundsForNotClassifyingWithCustomer(string docNumber, DateTime docDate, int id)
        {
            try
            {
                DateTime setDate = GetCurrentOperDay();
                return LeasingCustomerClassification.CloseLeasingConnectionGroundsForNotClassifyingWithCustomer(User.userID, setDate, docNumber, docDate, id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LeasingCustomerClassification> GetLeasingConnectionGroundsForClassifyingWithCustomer(long customerNumber, byte isActive)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingConnectionGroundsForClassifyingWithCustomer(customerNumber, isActive);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult AddOrCloseLeasingConnectionGroundsForClassifyingWithCustomer(LeasingCustomerClassification obj, byte addORClose)
        {
            try
            {
                DateTime setDate = GetCurrentOperDay();
                return LeasingCustomerClassification.AddOrCloseLeasingConnectionGroundsForClassifyingWithCustomer(obj, addORClose, setDate, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LeasingCustomerClassification GetLeasingConnectionGroundsForClassifyingWithCustomerByID(int id, long customerNumber)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingConnectionGroundsForClassifyingWithCustomerByID(id, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LeasingCustomerClassification> GetLeasingCustomerClassificationHistory(int leasingCustomerNumber, DateTime date)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingCustomerClassificationHistory(leasingCustomerNumber, date);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LeasingCustomerClassification GetLeasingCustomerClassificationHistoryByID(int id, long loanFullNumber, int lpNumber)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingCustomerClassificationHistoryByID(id, loanFullNumber, lpNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool LeasingCustomerConnectionResult(int customerNumberN1, int customerNumberN2)
        {
            try
            {
                return LeasingCustomerClassification.LeasingCustomerConnectionResult(customerNumberN1, customerNumberN2);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<LeasingCustomerClassification> GetLeasingGroundsForNotClassifyingCustomerLoan(int leasingCustomerNumber, byte isActive)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingGroundsForNotClassifyingCustomerLoan(leasingCustomerNumber, isActive);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetLeasingLoanInfo(int leasingCustNamber)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingLoanInfo(leasingCustNamber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult AddLeasingGroundsForNotClassifyingCustomerLoan(LeasingCustomerClassification obj)
        {
            DateTime regDate = GetCurrentOperDay();
            int quality = LeasingCustomerClassification.GetLeasingLoanQuality(obj.AppId);
            if (quality == 11)
            {
                ActionError error = new ActionError();
                error.Description = "Տվյալ պրոդուկտը դուրսգրված է: Հնարավոր չէ զեկուցագրի մուտքագրում:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);

                return result;
            }
            else if (quality == 10)
            {
                ActionError error = new ActionError();
                error.Description = "Տվյալ պրոդուկտը գտնվում է պայմանագրային վիճակում: Հնարավոր չէ զեկուցագրի մուտքագրում:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);

                return result;
            }
            else if (LeasingCustomerClassification.IsLeasingLoanActive(obj.AppId))
            {
                ActionError error = new ActionError();
                error.Description = "Տվյալ պրոդուկտի համար առկա է գործող զեկուցագիր:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);

                return result;
            }

            try
            {
                return LeasingCustomerClassification.AddLeasingGroundsForNotClassifyingCustomerLoan(obj, regDate, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public LeasingCustomerClassification GetLeasingGroundsForNotClassifyingCustomerLoanByID(int id)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingGroundsForNotClassifyingCustomerLoanByID(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult CloseLeasingGroundsForNotClassifyingCustomerLoan(long appId, int id, string docNumber, DateTime docDate)
        {
            DateTime closeDate = GetCurrentOperDay();
            long appResult = LeasingCustomerClassification.GetReportAppId(id);
            if (LeasingCustomerClassification.IsReportActive(id))
            {
                ActionError error = new ActionError();
                error.Description = "Ընտրված զեկուցագիրը փակված է:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);

                return result;
            }
            else if (appResult != 0 && appResult != appId)
            {
                ActionError error = new ActionError();
                error.Description = "Այլ պրոդուկտի զեկուցագրի փակման իրավունք չկա:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);

                return result;
            }
            try
            {
                return LeasingCustomerClassification.CloseLeasingGroundsForNotClassifyingCustomerLoan(appId, id, User.userID, closeDate, docNumber, docDate);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult EditLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj)
        {
            DateTime registrationDate = GetCurrentOperDay();
            int overdueDays = ((new DateTime(registrationDate.Date.Year, registrationDate.Date.Month, 1).AddMonths(1).AddDays(-1)).Date - obj.ClassificationDate.Date).Days + 1;

            Tuple<int, string> tuple = LeasingCustomerClassification.GetLeasingRiskDaysCountAndName(Convert.ToInt32(obj.RiskClassName));

            int daysCountMin = tuple.Item1;
            string RiskClassName = tuple.Item2;

            if (obj.CalcByDays == false)
            {
                obj.ClassificationDate = registrationDate.AddDays(-daysCountMin);
            }

            if ((registrationDate - obj.ClassificationDate).Days < 0)
            {
                ActionError error = new ActionError();
                error.Description = "Դասակարգման ամսաթիվը չի կարող մեծ լինել գրանցման ամսաթվից:";

                ActionResult result = new ActionResult();
                result.Errors = new List<ActionError>();
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(error);

                return result;
            }

            try
            {
                return LeasingCustomerClassification.EditLeasingCustomerSubjectiveClassificationGrounds(obj, overdueDays, registrationDate, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LeasingCustomerClassification GetLeasingSubjectiveClassificationGroundsByIDForEdit(int id)
        {
            try
            {
                return LeasingCustomerClassification.GetLeasingSubjectiveClassificationGroundsByIDForEdit(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAttachedCardOrderInHB(CardToCardOrder cardToCardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardToCardOrder);
                return customer.SaveAttachedCardOrderInHB(cardToCardOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public List<CardDataChangeOrder> GetCardDataChangesByProduct(long ProductAppId, short FieldType)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                List<CardDataChangeOrder> cardDataChangeOrders = customer.GetCardDataChangesByProduct(ProductAppId, FieldType);
                return cardDataChangeOrders;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult DeclineAttachedCardToCardOrderQuality(long docId)
        {
            try
            {
                Customer customer = new Customer(User, AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.DeclineAttachedCardToCardOrderQuality(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveAttachedCardToCardOrderQuality(CardToCardOrder cardToCardOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(cardToCardOrder);
                return customer.ApproveAttachedCardToCardOrderQuality(cardToCardOrder);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsCustomerConnectedToOurBank(ulong customerNumber)
        {
            try
            {
                return Validation.IsCustomerConnectedToOurBank(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetLoanOrderAcknowledgement(long docId)
        {
            try
            {
                return Loan.GetLoanOrderAcknowledgement(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<PensionPaymentOrder> GetAllPensionPayment(string socialCardNumber)
        {
            try
            {
                return PensionPaymentOrder.GetAllPensionPayment(socialCardNumber); ;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PensionPaymentOrder GetPensionPaymentOrderDetails(uint id)
        {
            try
            {
                return PensionPaymentOrder.GetPensionPaymentOrderDetails(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SavePensionPaymentOrder(PensionPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePensionPaymentOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveCardLessCashOutOrder(CardlessCashoutOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveCardLessCashOutOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ContentResult<string> ApproveCardLessCashOutOrder(CardlessCashoutOrder order)
        {
            try
            {
                //TODO Remove next 3 lines after going to production
                (bool isTestVersion, ContentResult<string> testResult) = TestCardlessCashoutOrderApproveForTestEnvironment(order.Id);
                if (isTestVersion && (order.Id == 333333 || order.Id == 444444))
                    return testResult;

                Customer customer = CreateCustomer();
                InitOrder(order);
                ContentResult<string> result = customer.ApproveCardLessCashOutOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                //Գործարքի կատարում
                if ((order.Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft) && (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.DoneAndReturnedValues || result.ResultCode == ResultCode.PartiallyCompleted || result.ResultCode == ResultCode.SaveAndSendToConfirm))
                {
                    try
                    {
                        OrderQuality quality = GetOrderQualityByDocID(order.Id);
                        if (quality != OrderQuality.Approved)
                        {
                            ConfirmOrderOnline(order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                        //եթե հայտի օնլայն կատարման ժամանակ խնդիր առաջանա, ապա հայտը կկատարվի 24/7 job-ով 
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public CardlessCashoutOrder GetCardLessCashOutOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetCardLessCashOutOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetNotFreezedCurrentAccount(ulong customerNumber)
        {
            try
            {
                return Account.GetNotFreezedCurrentAccount(customerNumber, "AMD");
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public CardlessCashoutOrder GetCardlessCashoutOrderWithVerification(string cardlessCashOutCode)
        {
            try
            {
                return CardlessCashoutOrder.GetCardlessCashoutOrderWithVerification(cardlessCashOutCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        private static (bool isTestVersion, ContentResult<string> result) TestCardlessCashoutOrderApproveForTestEnvironment(long docId)
        {
            ContentResult<string> result = new ContentResult<string>();

            bool isTestVersion = bool.Parse(WebConfigurationManager.AppSettings["TestVersion"].ToString());

            if (!isTestVersion)
                return (isTestVersion: false, result);

            if (docId == 333333)
                result.ResultCode = ResultCode.Normal;
            else if (docId == 444444)
                result.ResultCode = ResultCode.Failed;

            return (isTestVersion: true, result);
        }

        public List<DahkDetails> GetDahkDetailsForDigital(ulong customerNumber)
        {
            try
            {
                return DahkDetails.GetDahkDetailsForDigital(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }





        public LoanInterestRateConcessionOrder GetLoanInterestRateConcessionOrder(long OrderId)
        {
            LoanInterestRateConcessionOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetLoanInterestRateConcessionOrder(OrderId);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հիմնական քարտերը, լրացուցիչ քարտի հայտի համար 
        /// </summary>
        /// <returns></returns>
        public List<PlasticCard> GetCustomerMainCardsForAttachedCardOrder()
        {
            try
            {
                List<PlasticCard> cards;
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                cards = customer.GetCustomerMainCardsForAttachedCardOrder();
                return cards;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public (bool, CardlessCashoutOrder) GetCardlessCashoutOrderWithVerificationForNCR(string otp)
        {
            try
            {
                return CardlessCashoutOrder.GetCardlessCashoutOrderWithVerificationForNCR(otp);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult CardlessCashOutOrderConfirm(ulong docID, string TransactionId, string AtmId)
        {
            try
            {
                return CardlessCashoutOrder.Confirm(docID, TransactionId, AtmId, Source);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<MTOListAndBestChoiceOutput> GetSTAKMTOListAndBestChoice(MTOListAndBestChoiceInput bestChoice, string authorizedUserSessionToken)
        {
            try
            {
                List<MTOListAndBestChoiceOutput> result = RemittanceFeeData.GetSTAKMTOListAndBestChoice(bestChoice, authorizedUserSessionToken, User.userName, ClientIp);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public int GetRemittanceAmendmentCount(ulong id)
        {
            try
            {
                return RemittanceAmendmentOrder.GetRemittanceAmendmentCount(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetRemittanceContractDetails(ulong docId, string authorizedUserSessionToken)
        {
            try
            {
                return FastTransferPaymentOrder.GetRemittanceContractDetails(authorizedUserSessionToken, User.userName, ClientIp, docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetCurrencyExchangeOrderFee(CurrencyExchangeOrder order, int feeType)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.GetPaymentOrderFee(order, feeType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public LeasingDetailedInformation GetLeasingDetailedInformation(long loanFullName, DateTime dateOfBeginning)
        {
            try
            {

                return SearchLeasingLoan.GetLeasingDetailedInformation(loanFullName, dateOfBeginning);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<LeasingInsuranceDetails> GetLeasingInsuranceInformation(long loanFullName, DateTime dateOfBeginning)
        {
            try
            {

                return SearchLeasingLoan.GetLeasingInsuranceInformation(loanFullName, dateOfBeginning);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetPartlyMatureAmount(string contractNumber)
        {
            try
            {

                return SearchLeasingLoan.GetPartlyMatureAmount(contractNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<PlasticCardSMSServiceHistory> GetPlasticCardAllSMSServiceHistory(ulong cardNumber)
        {
            try
            {
                return PlasticCardSMSServiceHistory.GetPlasticCardAllSMSServiceHistory(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public DateTime GetLeasingOperDayForStatements()
        {
            try
            {
                return Utility.GetLeasingOperDayForStatements();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<long> GetAttachedCardOrdersByDocId(List<int> docIds)
        {
            try
            {
                return Order.GetAttachedCardOrdersByDocId(docIds);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<GroupTemplateResponse> GetGroupTemplates(int groupId, TemplateStatus status)
        {
            try
            {
                return Template.GetGroupTemplates(groupId, status, (Languages)Language);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public byte CardBlockingActionAvailability(string cardNumber)
        {
            try
            {
                return ArcaCardsTransactionOrder.CardBlockingActionAvailability(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ShowNewDahk ShowDAHKMessage()
        {
            try
            {
                return DahkDetails.ShowDAHKMessage(AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void MakeDAHKMessageRead(List<string> inquestCodes)
        {
            try
            {
                DahkDetails.MakeDAHKMessageRead(inquestCodes, AuthorizedCustomer.CustomerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ResponseConfirmForSTAK(STAKResponseConfirm responseConfirm, string authorizedUserSessionToken)
        {
            try
            {
                ActionResult result = responseConfirm.ResponseConfirm(authorizedUserSessionToken, User.userName, ClientIp);
                //Culture Culture = new Culture((Languages)Language);

                //Localization.SetCulture(result, Culture);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DateTime GetLeasingOperDay()
        {
            try
            {
                return Utility.GetLeasingOperDay();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveLinkPaymentOrder(LinkPaymentOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                order.user.userName = AuthorizedCustomer.UserName;
                return customer.SaveLinkPaymentOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public LinkPaymentOrder GetLinkPaymentOrder(long docId)
        {
            try
            {
                return LinkPaymentOrder.Get(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public ContentResult<string> ApproveLinkPaymentOrder(LinkPaymentOrder order)
        {
            try
            {
                ContentResult<string> result = new ContentResult<string>();

                Customer customer = CreateCustomer();
                InitOrder(order);
                result = customer.ApproveLinkPaymentOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                OrderQuality quality = GetOrderQualityByDocID(order.Id);
                if (quality != OrderQuality.Approved)
                {
                    ConfirmOrderOnline(order.Id);
                }
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public LinkPaymentOrder GetLinkPaymentOrderWithShortId(string ShortId)
        {
            try
            {
                return LinkPaymentOrder.GetLinkPaymentOrderWithShortId(ShortId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public ActionResult ConfirmPayerLinkPayment(PayerLinkPaymentOrder order)
        {
            try
            {
                Customer customer = new Customer();
                ActionResult result = customer.SaveAndApprovePayerLinkPaymentOrder(order);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        public ActionResult SaveBillSplitOrder(BillSplitOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveBillSplitOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public BillSplitOrder GetBillSplitOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetBillSplitOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ContentResult<List<BillSplitLinkResult>> ApproveBillSplitOrder(BillSplitOrder order)
        {
            try
            {
                ContentResult<List<BillSplitLinkResult>> result = new ContentResult<List<BillSplitLinkResult>>();
                Customer customer = CreateCustomer();
                InitOrder(order);
                result = customer.ApproveBillSplitOrder(order, AuthorizedCustomer.ApprovementScheme, AuthorizedCustomer.UserName);
                OrderQuality quality = GetOrderQualityByDocID(order.Id);

                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveBillSplitSenderRejectionOrder(BillSplitSenderRejectionOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveBillSplitSenderRejectionOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public BillSplitSenderRejectionOrder GetBillSplitSenderRejectionOrder(long ID)
        {
            BillSplitSenderRejectionOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetBillSplitSenderRejectionOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<SentBillSplitRequest> GetSentBillSplitRequests()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetSentBillSplitRequests();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<ReceivedBillSplitRequest> GetReceivedBillSplitRequests()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetReceivedBillSplitRequests();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public SentBillSplitRequest GetSentBillSplitRequest(long orderId)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetSentBillSplitRequest(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveBillSplitReminderOrder(BillSplitReminderOrder order)
        {
            try
            {

                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveBillSplitReminderOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public BillSplitReminderOrder GetBillSplitReminderOrder(long ID)
        {
            BillSplitReminderOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetBillSplitReminderOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ReceivedBillSplitRequest GetReceivedBillSplitRequest(int billSplitSenderId)
        {
            try
            {
                return ReceivedBillSplitRequest.GetReceivedBillSplitRequest(billSplitSenderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }





        public ActionResult SaveAndApproveCardReOpenOrder(CardReOpenOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardReOpenOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }



        public CardReOpenOrder GetCardReOpenOrder(long ID)
        {
            CardReOpenOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardReOpenOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public Dictionary<string, string> GetCardReOpenReason()
        {
            try
            {
                Dictionary<string, string> cashTypes = new Dictionary<string, string>();
                DataTable dt = CardReOpenOrder.GetCardReOpenReason();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cashTypes.Add(dt.Rows[i]["id"].ToString(), (Languages)Language == Languages.hy ? Utility.ConvertAnsiToUnicode(dt.Rows[i].ItemArray[1].ToString()) : Utility.ConvertAnsiToUnicode(dt.Rows[i].ItemArray[2].ToString()));

                }
                return cashTypes;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public void WriteCardlessCashoutLog(ulong docID, bool isOk, string msgArm, string msgEng, string AtmId, byte step)
        {
            try
            {
                CardlessCashoutOrder.WriteCardlessCashoutLog(docID, isOk, msgArm, msgEng, AtmId, step);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveCardRenewOrder(CardRenewOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveCardRenewOrder(order, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CardRenewOrder GetCardRenewOrder(long ID)
        {
            CardRenewOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetCardRenewOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> CheckCardRenewOrder(CardRenewOrder order)
        {
            List<string> messages = new List<string>();
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                messages = customer.CheckCardRenewOrder(order);
                return messages;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveRenewedCardAccountRegOrder(RenewedCardAccountRegOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveRenewedCardAccountRegOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<string> GetRenewedCardAccountRegWarnings(Card oldCard)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetRenewedCardAccountRegWarnings(oldCard);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public RenewedCardAccountRegOrder GetRenewedCardAccountRegOrder(long ID)
        {
            try
            {
                Customer customer = new Customer();
                return customer.GetRenewedCardAccountRegOrder(ID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetCardHolderData(ulong productId, string dataType)
        {
            try
            {
                return Card.GetCardHolderData(productId, dataType);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetPhoneForCardRenew(long productId)
        {
            try
            {
                return CardRenewOrder.GetPhoneForCardRenew(productId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public List<CardRetainHistory> GetCardRetainHistory(string cardNumber)
        {
            List<CardRetainHistory> historyList = new List<CardRetainHistory>();
            try
            {
                historyList = Card.GetCardRetainHistory(cardNumber);
                return historyList;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveLoanDeleteOrder(DeleteLoanOrder deleteLoanOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(deleteLoanOrder);

                ActionResult result = customer.SaveAndApproveDeleteLoan(deleteLoanOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public DeleteLoanOrderDetails GetLoanDeleteOrderDetails(uint orderId)
        {
            try
            {
                return DeleteLoanOrderDetails.GetLoanDeleteOrderDetails(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveAutomateArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder)
        {
            try
            {
                Customer customer = CreateCustomer();
                arcaCardsTransactionOrder.IPAddress = ClientIp;
                InitOrder(arcaCardsTransactionOrder);
                return customer.SaveAndApproveAutomateArcaCardsTransactionOrder(arcaCardsTransactionOrder, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult RemoveAccountOrder(AccountRemovingOrder order)
        {
            try
            {
                ActionResult result = new ActionResult();
                Customer customer = CreateCustomer();
                result = customer.RemoveAccountOrder(order, AuthorizedCustomer.UserName);
                return result;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveAndApproveAccountRemoving(AccountRemovingOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveAccountRemoving(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool GetRightsTransferAvailability(string accountNumber)
        {
            try
            {
                return Account.GetRightsTransferAvailability(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong CheckCustomerFreeFunds(string accountNumber)
        {
            try
            {
                return Account.CheckCustomerFreeFunds(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveThirdPersonAccountRightsTransfer(ThirdPersonAccountRightsTransferOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveThirdPersonAccountRightsTransfer(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public bool GetRightsTransferVisibility(string accountNumber)
        {
            try
            {
                return Account.GetRightsTransferVisibility(accountNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsCardlessCashCodeCorrect(string cardlessCashoutCode)
        {
            try
            {
                return CardlessCashoutOrder.IsCardlessCashCodeCorrect(cardlessCashoutCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public bool GetCheckCustomerIsThirdPerson(string accountNumber, ulong customerNumber)
        {
            try
            {
                return Account.GetCheckCustomerIsThirdPerson(accountNumber, customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public void SaveCancelNotificationMessage(string request)
        {
            try
            {
                CardlessCashoutOrder.SaveCancelNotificationMessage(request);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool GetMRDataChangeAvailability(int mrID)
        {
            try
            {
                return MRDataChangeOrder.GetMRDataChangeAvailability(mrID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveMRDataChangeOrder(MRDataChangeOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveAndApproveMRDataChangeOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ulong GetAmexGoldProductId(string account)
        {
            try
            {
                return LoanProductOrder.GetAmexGoldProductId(account);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanRepaymentFromCardDataChange GetLoanRepaymentFromCardDataChangeHistory(ulong appId)
        {
            try
            {
                return Loan.GetLoanRepaymentFromCardDataChangeHistory(appId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public LoanRepaymentFromCardDataChange SaveLoanRepaymentFromCardDataChange(LoanRepaymentFromCardDataChange loanRepaymentFromCardDataChange)
        {
            try
            {
                return Loan.SaveLoanRepaymentFromCardDataChange(loanRepaymentFromCardDataChange);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }

        public ActionResult SavePreferredAccountOrder(PreferredAccountOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SavePreferredAccountOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApprovePreferredAccountOrder(long id)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.ApprovePreferredAccountOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public PreferredAccount GetSelectedOrDefaultPreferredAccountNumber(PreferredAccountServiceTypes serviceType, ulong customerNumber)
        {
            try
            {
                PreferredAccount preferredAccount = new PreferredAccount();
                preferredAccount = preferredAccount.GetSelectedOrDefaultPreferredAccountNumber(serviceType, customerNumber);
                return preferredAccount;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public PreferredAccountOrder GetPreferredAccountOrder(long id)
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetPreferredAccountOrder(id);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public bool IsDisabledPreferredAccountService(ulong customerNumber, PreferredAccountServiceTypes preferredAccountServiceType)
        {
            try
            {
                PreferredAccount preferredAccount = new PreferredAccount();
                bool isDisabled = preferredAccount.IsDisabledPreferredAccountService(customerNumber, preferredAccountServiceType);
                return isDisabled;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAccountQrCode(string accountNumber, string guid, ulong customerNumber)
        {
            try
            {
                QrTransfer qrTransfers = new QrTransfer(accountNumber, guid, customerNumber);
                return qrTransfers.SaveAccountQrCode();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public QrTransfer SearchAccountByQrCode(string guid)
        {
            try
            {
                QrTransfer qrTransfers = new QrTransfer
                {
                    Guid = guid
                };
                return qrTransfers.SearchAccountByQrCode();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public string GetAccountQrCodeGuid(string accountNumber)
        {
            try
            {
                QrTransfer qrTransfers = new QrTransfer
                {
                    AccountNumber = accountNumber
                };
                return qrTransfers.GetAccountQrCodeGuid();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetQrAccounts()
        {
            try
            {
                Customer customer = new Customer(User, AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetQrAccounts();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveAndApproveVisaAliasOrder(VisaAliasOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();

                return customer.SaveAndApproveVisaAliasOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult SaveVisaAliasOrder(VisaAliasOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveVisaAliasOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public ActionResult ApproveVisaAliasOrder(VisaAliasOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveVisaAliasOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public VisaAliasOrder GetVisaAliasOrder(long ID)
        {
            VisaAliasOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetVisaAliasOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public VisaAliasOrder VisaAliasOrderDetails(long orderId)
        {
            try
            {
                Customer customer = CreateCustomer();

                return customer.VisaAliasOrderDetails(orderId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public string GetVisaAliasGuidByCutomerNumber(ulong customerNumber)
        {
            try
            {
                return VisaAliasDB.GetVisaAliasGuidByCutomerNumber(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public CardHolderAndCardType GetCardTypeAndCardHolder(string cardNumber)
        {
            try
            {
                Customer customer = CreateCustomer();
                return customer.GetCardTypeAndCardHolder(cardNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public int GetBondOrderIssueSeria(int bondIssueId)
        {
            try
            {
                return BondOrder.GetBondOrderIssueSeria(bondIssueId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public double GetUnitPrice(int bondIssueId)
        {
            try
            {
                return BondIssue.GetUnitPrice(bondIssueId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public  void DeleteDepoAccounts(ulong customerNumber)
        {
            try
            {
                DepositaryAccount.DeleteDepoAccounts(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
        public  double GetDepositaryAccount(ulong customerNumber)
        {
            try
            {
              return   DepositaryAccount.GetDepositaryAccount(customerNumber);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveDepositaryAccountOrder(DepositaryAccountOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveDepositaryAccountOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveDepositaryAccountOrder(DepositaryAccountOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveDepositaryAccountOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult SaveBondOrder(BondOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveBondOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ApproveBondOrder(BondOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.ApproveBondOrder(order, AuthorizedCustomer.UserName, AuthorizedCustomer.ApprovementScheme);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public List<Account> GetAccountsForStock()
        {
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                return customer.GetAccountsForStock();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ActionResult ConfirmStockOrder(int bondId)
        {
            try
            {
                return BondOrder.ConfirmStockOrder(bondId, User.userID);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public double GetBuyKursForDate(string currency)
        {
            try
            {
                return Utility.GetBuyKursForDate(currency, User.filialCode);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }


        public ActionResult SaveConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order)
        {
            try
            {
                Customer customer = CreateCustomer();
                InitOrder(order);
                return customer.SaveConsumeLoanApplicationOrder(order, AuthorizedCustomer.UserName);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }

        }
        public ActionResult UpdateDepositoryAccountOrder(DepositaryAccountOrder order)
        {
            try
            {
                return DepositaryAccountOrder.UpdateDepositoryAccountOrder(order);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public ConsumeLoanApplicationOrder GetConsumeLoanApplicationOrder(long ID)
        {
            ConsumeLoanApplicationOrder order;
            try
            {
                Customer customer = new Customer(AuthorizedCustomer.CustomerNumber, (Languages)Language);
                order = customer.GetConsumeLoanApplicationOrder(ID);
                return order;
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public BondCertificateDetails  GetBondCertificateDetailsByDocId(ulong docId)
        {
            try
            {
                return Bond.GetBondCertificateDetailsByDocId(docId);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }

        public (long, DateTime) ExistsConsumeLoanApplicationOrder(List<OrderQuality> qualities)
        {
            try
            {
                return ConsumeLoanApplicationOrder.ExistsConsumeLoanApplicationOrder(AuthorizedCustomer.CustomerNumber,  qualities);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
    }
}
