using ExternalBanking;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using xbs = ExternalBanking.ACBAServiceReference;
using infsec = ExternalBankingService.InfSecServiceReference;
using ExternalBanking.XBManagement;
using System.Threading.Tasks;
using System.Data;
using ExternalBanking.UtilityPaymentsManagment;
using ExternalBanking.ARUSDataService;
using ActionResult = ExternalBanking.ActionResult;
using ActionError = ExternalBanking.ActionError;
using ExternalBanking.PreferredAccounts;
using ExternalBanking.QrTransfers;
using static ExternalBanking.ReceivedBillSplitRequest;

using ExternalBanking.Leasing;
namespace ExternalBankingService.Interfaces
{
    [ServiceContract]
    [ServiceKnownType(typeof(DepositOrder))]
    [ServiceKnownType(typeof(UtilityPaymentOrder))]
    [ServiceKnownType(typeof(PaymentOrder))]
    [ServiceKnownType(typeof(ReferenceOrder))]
    [ServiceKnownType(typeof(ChequeBookOrder))]
    [ServiceKnownType(typeof(CashOrder))]
    [ServiceKnownType(typeof(StatmentByEmailOrder))]
    [ServiceKnownType(typeof(SwiftCopyOrder))]
    [ServiceKnownType(typeof(CustomerDataOrder))]
    [ServiceKnownType(typeof(RateType))]
    [ServiceKnownType(typeof(CreditLineTerminationOrder))]
    [ServiceKnownType(typeof(DepositRepayment))]
    [ServiceKnownType(typeof(LoanRepaymentGrafik))]
    [ServiceKnownType(typeof(CreditLineGrafik))]
    [ServiceKnownType(typeof(DepositCase))]
    [ServiceKnownType(typeof(OrderHistory))]
    [ServiceKnownType(typeof(OverdueDetail))]
    [ServiceKnownType(typeof(AccountOrder))]
    [ServiceKnownType(typeof(PeriodicOrder))]
    [ServiceKnownType(typeof(PeriodicUtilityPaymentOrder))]

    [ServiceKnownType(typeof(PeriodicTransferDataChangeOrder))]

    [ServiceKnownType(typeof(PeriodicPaymentOrder))]
    [ServiceKnownType(typeof(BudgetPaymentOrder))]
    [ServiceKnownType(typeof(AccountClosingOrder))]
    [ServiceKnownType(typeof(SearchCards))]
    [ServiceKnownType(typeof(SearchSwiftCodes))]
    [ServiceKnownType(typeof(SearchAccountResult))]
    [ServiceKnownType(typeof(SearchAccountResult))]
    [ServiceKnownType(typeof(OrderNumberTypes))]
    [ServiceKnownType(typeof(Languages))]
    [ServiceKnownType(typeof(TransitPaymentOrder))]
    [ServiceKnownType(typeof(CashPosPaymentOrder))]
    [ServiceKnownType(typeof(ServicePaymentOrder))]
    [ServiceKnownType(typeof(SearchTransferBankMail))]
    [ServiceKnownType(typeof(SearchInternationalTransfer))]
    [ServiceKnownType(typeof(SearchReceivedTransfer))]
    [ServiceKnownType(typeof(SearchBudgetAccount))]
    [ServiceKnownType(typeof(ChequeBookReceiveOrder))]
    [ServiceKnownType(typeof(AccountReOpenOrder))]
    [ServiceKnownType(typeof(InternationalPaymentOrder))]
    [ServiceKnownType(typeof(infsec.UserAccessForCustomer))]
    [ServiceKnownType(typeof(ActionPermission))]
    [ServiceKnownType(typeof(CardReReleaseOrder))]
    [ServiceKnownType(typeof(PlasticCard))]
    [ServiceKnownType(typeof(CardRegistrationOrder))]
    [ServiceKnownType(typeof(DepositCasePenaltyMatureOrder))]
    [ServiceKnownType(typeof(CardRenewOrder))]
    [ServiceKnownType(typeof(DepositOption))]
    [ServiceKnownType(typeof(ReestrUtilityPaymentOrder))]
    [ServiceKnownType(typeof(CashBookOrder))]
    [ServiceKnownType(typeof(AssigneeIdentificationOrder))]
    [ServiceKnownType(typeof(CardUSSDServiceOrder))]
    [ServiceKnownType(typeof(CardUSSDServiceHistory))]
    [ServiceKnownType(typeof(ProductNote))]
    [ServiceKnownType(typeof(HBToken))]
    [ServiceKnownType(typeof(HBUser))]
    [ServiceKnownType(typeof(CardToCardOrderTemplate))]
    [ServiceKnownType(typeof(CardToCardOrder))]
    [ServiceKnownType(typeof(CustomerTypes))]
    [ServiceKnownType(typeof(DigitalAccountRestConfigurations))]
    [ServiceKnownType(typeof(LeasingDetailedInformation))]
    [ServiceKnownType(typeof(LeasingInsuranceDetails))]
    [ServiceKnownType(typeof(TransitCurrencyExchangeOrder))]
    [ServiceKnownType(typeof(CurrencyExchangeOrder))]
    [ServiceKnownType(typeof(RenewedCardAccountRegOrder))]
    [ServiceKnownType(typeof(VisaAliasOrder))]

    public interface IXBService
    {

        [OperationContract]
        Account GetAccount(string accountNumber);
        [OperationContract]
        Dictionary<string, string> GetAccountsForBlockingAvailableAmount(ulong customerNumber);

        [OperationContract]
        double GetTransitAccountNumberFromCardAccount(double cardAccountNumber);

        [OperationContract]
        List<Account> GetAccounts();

        [OperationContract]
        List<Account> GetCurrentAccounts(ProductQualityFilter filter);

        [OperationContract]
        List<Account> GetCardAccounts();

        [OperationContract]
        AccountStatement AccountStatement(string accountNumber, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0);

        [OperationContract]
        Card GetCard(ulong productId);

        [OperationContract]
        List<Card> GetCards(ProductQualityFilter filter, bool includingAttachedCards);

        [OperationContract]
        CardStatement GetCardStatement(string cardNumber, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0);

        [OperationContract]
        Loan GetLoan(ulong productId);

        [OperationContract]
        List<Loan> GetLoans(ProductQualityFilter filter);

        [OperationContract]
        Deposit GetDeposit(ulong productId);

        [OperationContract]
        List<Deposit> GetDeposits(ProductQualityFilter filter);

        [OperationContract]
        PeriodicTransfer GetPeriodicTransfer(ulong productId);

        [OperationContract]
        List<PeriodicTransfer> GetPeriodicTransfers(ProductQualityFilter filter);

        [OperationContract]
        KeyValuePair<String, double> GetArCaBalance(string cardNumber);

        [OperationContract]
        List<Order> GetDraftOrders(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        List<Message> GetMessages(DateTime dateFrom, DateTime dateTo, short type);

        [OperationContract]
        List<Message> GetNumberOfMessages(short messagesCount, MessageType type);

        [OperationContract]
        List<Order> GetSentOrders(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        void AddMessage(Message message);

        [OperationContract]
        void DeleteMessage(int messageId);

        [OperationContract]
        void MarkMessageReaded(int messageId);

        [OperationContract]
        Contact GetContact(ulong contactId);

        [OperationContract]
        List<Contact> GetContacts();

        [OperationContract]
        int AddContact(Contact contact);
        [OperationContract]
        int UpdateContact(Contact contact);

        [OperationContract]
        int DeleteContact(ulong contactId);

        [OperationContract]
        List<Communal> GetCommunals(SearchCommunal searchCommunal, bool isSearch = true);

        [OperationContract]
        List<CommunalDetails> GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType);

        [OperationContract]
        ActionResult SavePaymentOrder(PaymentOrder order);

        [OperationContract]
        ActionResult SaveBudgetPaymentOrder(BudgetPaymentOrder order);

        [OperationContract]
        PaymentOrder GetPaymentOrder(long id);

        [OperationContract]
        BudgetPaymentOrder GetBudgetPaymentOrder(long id);

        [OperationContract]
        InternationalPaymentOrder GetInternationalPaymentOrder(long id);

        [OperationContract]
        FastTransferPaymentOrder GetFastTransferPaymentOrder(long id, string authorizedUserSessionToken);

        [OperationContract]
        ReceivedFastTransferPaymentOrder GetReceivedFastTransferPaymentOrder(long id, string authorizedUserSessionToken);

        [OperationContract]
        Transfer GetTransfer(ulong id);

        [OperationContract]
        Transfer GetApprovedTransfer(ulong id);

        [OperationContract]
        ReceivedBankMailTransfer GetReceivedBankMailTransfer(ulong id);

        [OperationContract]
        ActionResult ConfirmTransfer(ulong transferID, short allowTransferConfirm, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult DeleteTransfer(ulong transferID, string description);

        [OperationContract]
        ActionResult ApproveTransfer(TransferApproveOrder transferApproveOrder);

        [OperationContract]
        double GetLastExchangeRate(string currency, RateType rateType, ExchangeDirection direction, ushort filalCode);

        [OperationContract]
        double GetFastTransferFeeAcbaPercent(byte transferType);

        [OperationContract]
        double GetReceivedFastTransferFeePercent(byte transferType, string code, string countryCode, double amount, string currency, DateTime date);

        [OperationContract]
        byte GetFastTransferAcbaCommisionType(byte transferType, string code);

        [OperationContract]
        double GetPaymentOrderFee(PaymentOrder order, int feeType);

        [OperationContract]
        double GetInternationalPaymentOrderFee(InternationalPaymentOrder order);

        [OperationContract]
        double GetCardFee(PaymentOrder order);

        [OperationContract]
        PaymentOrderFutureBalance GetPaymentOrderFutureBalance(PaymentOrder order);

        [OperationContract]
        PaymentOrderFutureBalance GetPaymentOrderFutureBalanceById(long id);

        [OperationContract]
        ActionResult SaveUtiliyPaymentOrder(UtilityPaymentOrder utilityPaymentOrder);

        [OperationContract]
        ActionResult ApprovePaymentOrder(PaymentOrder order);

        [OperationContract]
        UtilityPaymentOrder GetUtilityPaymentOrder(long id);

        [OperationContract]
        List<Account> GetAccountsForOrder(short orderType, byte orderSubType, byte accountTyp, bool includingAttachedCards = true);

        [OperationContract]
        List<Account> GetCustomerAccountsForOrder(ulong CustomerNumber, short orderType, byte orderSubType, byte accountType);

        [OperationContract]
        ActionResult DeleteOrder(Order order);


        [OperationContract]
        int GetUnreadedMessagesCount();

        [OperationContract]
        void SetUser(AuthorizedCustomer authorizedCustomer, byte language, string ClientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        ActionResult ApproveUtilityPaymentOrder(UtilityPaymentOrder order);

        [OperationContract]
        string GetCurrencyCode(string currency);

        [OperationContract]
        int GetUnreadMessagesCountByType(MessageType type);

        [OperationContract]
        ActionResult SaveDepositOrder(DepositOrder order);

        [OperationContract]
        ActionResult SaveDepositTermination(DepositTerminationOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCreditLineTerminationOrder(CreditLineTerminationOrder order);

        [OperationContract]
        DepositOrder GetDepositorder(long ID);

        [OperationContract]
        ActionResult ApproveDepositOrder(DepositOrder order);

        [OperationContract]
        ActionResult SaveReferenceOrder(ReferenceOrder order);

        [OperationContract]
        ActionResult ApproveReferenceOrder(ReferenceOrder order);

        [OperationContract]
        ReferenceOrder GetReferenceOrder(long ID);

        [OperationContract]
        ActionResult SaveChequeBookOrder(ChequeBookOrder order);

        [OperationContract]
        ActionResult ApproveChequeBookOrder(ChequeBookOrder order);

        [OperationContract]
        ChequeBookOrder GetChequeBookOrder(long ID);

        [OperationContract]
        ActionResult SaveCashOrder(CashOrder order);

        [OperationContract]
        ActionResult ApproveCashOrder(CashOrder order);

        [OperationContract]
        CashOrder GetCashOrder(long ID);

        [OperationContract]
        ActionResult SaveStatmentByEmailOrder(StatmentByEmailOrder order);

        [OperationContract]
        ActionResult ApproveStatmentByEmailOrder(StatmentByEmailOrder order);

        [OperationContract]
        StatmentByEmailOrder GetStatmentByEmailOrder(long ID);

        [OperationContract]
        ActionResult SaveSwiftCopyOrder(SwiftCopyOrder order);

        [OperationContract]
        ActionResult ApproveSwiftCopyOrder(SwiftCopyOrder order);

        [OperationContract]
        SwiftCopyOrder GetSwiftCopyOrder(long ID);

        [OperationContract]
        ActionResult SaveCustomerDataOrder(CustomerDataOrder order);

        [OperationContract]
        ActionResult ApproveCustomerDataOrder(CustomerDataOrder order);

        [OperationContract]
        CustomerDataOrder GetCustomerDataOrder(long ID);

        [OperationContract]
        Dictionary<ulong, string> GetThirdPersons();


        [OperationContract]
        ActionResult CheckDepositOrderCondition(DepositOrder order);

        [OperationContract]
        DepositOrderCondition GetDepositCondition(DepositOrder order);

        [OperationContract]
        ActionResult ApproveDepositTermination(DepositTerminationOrder order);

        [OperationContract]
        TransferByCallList GetTransfersbyCall(TransferByCallFilter filter);

        [OperationContract]
        TransferByCallList GetCustomerTransfersbyCall(TransferByCallFilter filter);

        [OperationContract]
        List<Transfer> GetTransfers(TransferFilter filter);

        [OperationContract]
        List<Transfer> GetTransfersForHB(TransferFilter filter);

        [OperationContract]
        List<ReceivedBankMailTransfer> GetReceivedBankMailTransfers(TransferFilter filter);

        [OperationContract]
        ActionResult SaveTransferbyCall(TransferByCall transfer);

        [OperationContract]
        ActionResult SendTransfeerCallForPay(ulong transferID);

        [OperationContract]
        TransferByCall GetTransferbyCall(long Id);

        [OperationContract]
        List<DepositRepayment> GetDepositRepayments(ulong productId);

        [OperationContract]
        Boolean ManuallyRateChangingAccess(Double amount, string currency, string convertationCurrency, SourceType sourceType);

        [OperationContract]
        List<LoanRepaymentGrafik> GetLoanGrafik(Loan loan);

        [OperationContract]
        List<LoanRepaymentGrafik> GetLoanInceptiveGrafik(Loan loan, ulong customerNumber);

        [OperationContract]
        List<ulong> GetDepositJointCustomers(ulong productId);

        [OperationContract]
        List<PeriodicTransferHistory> GetPeriodicTransferHistory(long ProductId, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        CreditLine GetCreditLine(ulong productId);

        [OperationContract]
        HasHB HasACBAOnline();

        [OperationContract]
        List<CreditLine> GetCreditLines(ProductQualityFilter filter);

        [OperationContract]
        List<CreditLine> GetCardClosedCreditLines(string cardNumber);

        [OperationContract]
        List<CreditLineGrafik> GetCreditLineGrafik(ulong productId);

        [OperationContract]
        List<DepositCase> GetDepositCases(ProductQualityFilter filter);

        [OperationContract]
        DepositCase GetDepositCase(ulong productId);

        [OperationContract]
        ActionResult MakeSwiftStatement(ulong messageUnicNumber, DateTime dateStatement, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        SwiftMessage GenerateNewSwiftMessageByPeriodicTransfer(DateTime registrationDate, ulong periodicTransferId);

        [OperationContract]
        List<OrderHistory> GetOrderHistory(long orderId);

        [OperationContract]
        List<CustomerDebts> GetCustomerDebts(ulong customerNumber);

        [OperationContract]
        Account GetCurrentAccount(string accountNumber);

        [OperationContract]
        ActionResult SaveAndApproveDepositOrder(DepositOrder order);

        [OperationContract]
        ActionResult SaveAndApproveDepositTermination(DepositTerminationOrder order);

        [OperationContract]
        ActionResult SaveAndApprovePaymentOrder(PaymentOrder order);

        [OperationContract]
        ActionResult SaveAndApproveReferenceOrder(ReferenceOrder order);

        [OperationContract]
        ActionResult SaveAndApproveChequeBookOrder(ChequeBookOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCashOrder(CashOrder order);

        [OperationContract]
        ActionResult SaveAndApproveSwiftCopyOrder(SwiftCopyOrder order);

        [OperationContract]
        ActionResult SaveAndApproveStatmentByEmailOrder(StatmentByEmailOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCustomerDataOrder(CustomerDataOrder order);

        [OperationContract]
        ActionResult SaveAndApproveUtilityPaymentOrder(UtilityPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndApproveBudgetPaymentOrder(BudgetPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndApproveInternationalPaymentOrder(InternationalPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndApproveFastTransferPaymentOrder(FastTransferPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndApproveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult SaveAndApproveCallTransferChangeOrder(TransferByCallChangeOrder order);

        [OperationContract]
        List<KeyValuePair<ulong, double>> GetAccountJointCustomers(string accountNumber);

        [OperationContract]
        ActionResult GenerateAndMakeSwitMessageByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo, ulong periodicTransferId);

        [OperationContract]
        List<TransferCallContract> GetContractsForTransfersCall(string customerNumber, string accountNumber, string cardNumber);

        //[OperationContract]
        //List<Tuple<string, string>> GetCustomerAuthorizationData(ulong customerNumber);

        [OperationContract]
        AuthorizedCustomer AuthorizeCustomer(ulong customerNumber, string authorizedUserSessionToken);

        [OperationContract]
        List<OverdueDetail> GetOverdueDetails();

        [OperationContract]
        List<OverdueDetail> GetCurrentProductOverdueDetails(long productId);

        [OperationContract]
        void GenerateLoanOverdueUpdate(long productId, DateTime startDate, DateTime? endDate, string updateReason, short setNumber);

        [OperationContract]
        bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source, ServiceType serviceType);

        [OperationContract]
        ulong GetAuthorizedCustomerNumber();

        [OperationContract]
        TransferCallContract GetContractDetails(long contractId);

        [OperationContract]
        void SaveExternalBankingLogOutHistory(string authorizedUserSessionToken);

        //[OperationContract]
        //ActionResult SendAutorizationSMS();

        //[OperationContract]
        //ActionResult VerifyAuthorizationSMS(string smsCode);

        [OperationContract]
        List<ExchangeRate> GetExchangeRates();

        [OperationContract]
        List<xbs.AttachmentDocument> GetHBAttachmentsInfo(ulong documentId);

        [OperationContract]
        List<ProductDocument> GetProductDocuments(ulong productId);

        [OperationContract]
        byte[] GetOneHBAttachment(ulong id);

        [OperationContract]
        double GetCBKursForDate(DateTime date, string currency);

        [OperationContract]
        double GetMRFeeAMD(string cardNumber);

        [OperationContract]
        double GetCardTotalDebt(string cardNumber);

        [OperationContract]
        double GetPetTurk(long productId);

        [OperationContract]
        DateTime GetNextOperDay();

        [OperationContract]
        ActionResult SaveAndApproveMatureOrder(MatureOrder order);

        [OperationContract]
        MatureOrder GetMatureOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApprovePlasticCardOrder(PlasticCardOrder cardOrder);

        [OperationContract]
        PlasticCardOrder GetPlasticCardOrder(long orderID);

        [OperationContract]
        ActionResult SaveAccountOrder(AccountOrder order);

        [OperationContract]
        AccountOrder GetAccountOrder(long ID);

        [OperationContract]
        ActionResult ApproveAccountOrder(AccountOrder order);

        [OperationContract]
        ActionResult SaveAndApproveAccountOrder(AccountOrder order);

        [OperationContract]
        ActionResult SavePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order);

        [OperationContract]
        ActionResult SavePeriodicPaymentOrder(PeriodicPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndAprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndAprovePeriodicPaymentOrder(PeriodicPaymentOrder order);

        [OperationContract]
        ActionResult SaveMatureOrder(MatureOrder order);

        [OperationContract]
        ActionResult ApproveMatureOrder(MatureOrder order);
        [OperationContract]
        bool IsPoliceAccount(string accountNumber);

        [OperationContract]
        bool CheckAccountForPSN(string accountNumber);

        [OperationContract]
        MembershipRewards GetCardMembershipRewards(string cardNumber);

        [OperationContract]
        Guarantee GetGuarantee(ulong productId);

        [OperationContract]
        List<Guarantee> GetGuarantees(ProductQualityFilter filter);

        [OperationContract]
        Accreditive GetAccreditive(ulong productId);

        [OperationContract]
        List<Accreditive> GetAccreditives(ProductQualityFilter filter);

        [OperationContract]
        PaidGuarantee GetPaidGuarantee(ulong productId);

        [OperationContract]
        List<PaidGuarantee> GetPaidGuarantees(ProductQualityFilter filter);

        [OperationContract]
        PaidAccreditive GetPaidAccreditive(ulong productId);

        [OperationContract]
        List<PaidAccreditive> GetPaidAccreditives(ProductQualityFilter filter);

        [OperationContract]
        Factoring GetFactoring(ulong productId);

        [OperationContract]
        List<Factoring> GetFactorings(ProductQualityFilter filter);

        [OperationContract]
        PaidFactoring GetPaidFactoring(ulong productId);

        [OperationContract]
        List<PaidFactoring> GetPaidFactorings(ProductQualityFilter filter);

        [OperationContract]
        List<SearchAccountResult> GetSearchedAccounts(SearchAccounts searchParams);

        [OperationContract]
        ActionResult CloseAccountOrder(AccountClosingOrder order);

        [OperationContract]
        ActionResult ApproveAccountClosing(AccountClosingOrder order);

        [OperationContract]
        ActionResult SaveAndApproveAccountClosing(AccountClosingOrder order);

        [OperationContract]
        AccountClosingOrder GetAccountClosingOrder(long ID);

        [OperationContract]
        ActionResult ApprovePeriodicPaymentOrder(Order order);

        [OperationContract]
        ActionResult ApprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order);

        [OperationContract]
        ActionResult SaveAndAprovePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order);

        [OperationContract]
        ActionResult ApprovePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order);

        [OperationContract]
        ActionResult SavePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order);

        [OperationContract]
        List<SearchCardResult> GetSearchedCards(SearchCards searchParams);

        [OperationContract]
        List<SearchSwiftCodes> GetSearchedSwiftCodes(SearchSwiftCodes searchParams);

        [OperationContract]
        ActionResult SaveCardClosingOrder(CardClosingOrder order);

        [OperationContract]
        CardClosingOrder GetCardClosingOrder(long ID);

        [OperationContract]
        ActionResult ApproveCardClosingOrder(CardClosingOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCardClosingOrder(CardClosingOrder order);

        [OperationContract]
        List<string> GetCardClosingWarnings(ulong productId);

        [OperationContract]
        List<string> GetCredentialClosingWarnings(ulong assignId);

        [OperationContract]
        PeriodicBudgetPaymentOrder GetPeriodicBudgetPaymentOrder(long ID);

        [OperationContract]
        PeriodicUtilityPaymentOrder GetPeriodicUtilityPaymentOrder(long ID);

        [OperationContract]
        PeriodicPaymentOrder GetPeriodicPaymentOrder(long ID);

        [OperationContract]
        List<string> GetReceiverAccountWarnings(string accountNumber);

        [OperationContract]
        PeriodicTerminationOrder GetPeriodicTerminationOrder(long ID);

        [OperationContract]
        ActionResult ApprovePeriodicTerminationOrder(PeriodicTerminationOrder order);

        [OperationContract]
        ActionResult SaveAndApprovePeriodicTerminationOrder(PeriodicTerminationOrder order);

        [OperationContract]
        ActionResult SavePeriodicTerminationOrder(PeriodicTerminationOrder order);

        [OperationContract]
        ActionResult SaveAndApproveAccountReOpenOrder(AccountReOpenOrder order);

        [OperationContract]
        AccountReOpenOrder GetAccountReOpenOrder(long ID);

        [OperationContract]
        List<Account> GetAccountsForNewDeposit(DepositOrder order);

        [OperationContract]
        List<AdditionalDetails> GetAccountAdditionalDetails(string accountNumber);

        [OperationContract]
        Dictionary<string, string> GetAccountAdditionsTypes();

        [OperationContract]
        string CreateSerialNumber(int currencyCode, byte operationType);

        [OperationContract]
        ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType, ushort filialCode);

        [OperationContract]
        List<KeyValuePair<string, string>> GetCommunalReportParameters(SearchCommunal searchCommunal);

        [OperationContract]
        List<string> GetAccountOpenWarnings();

        [OperationContract]
        ActionResult SaveAndApproveAccountDataChangeOrder(AccountDataChangeOrder order);

        [OperationContract]
        string GetCommunalPaymentDescription(SearchCommunal searchCommunal);

        [OperationContract]
        List<OPPerson> GetOrderOPPersons(string accountNumber, OrderType orderType);

        [OperationContract]
        int GetAccountStatementDeliveryType(string accountNumber);

        [OperationContract]
        bool IsPrepaidArmenTell(SearchCommunal searchCommunal);

        [OperationContract]
        List<string> GetCustomerDocumentWarnings(ulong customerNumber);

        [OperationContract]
        double GetThreeMonthLoanRate(ulong productId);

        [OperationContract]
        double GetLoanMatureCapitalPenalty(MatureOrder order, ExternalBanking.ACBAServiceReference.User user);

        [OperationContract]
        List<OrderAttachment> GetOrderAttachments(long orderId);

        [OperationContract]
        OrderAttachment GetOrderAttachment(string attachmentId);

        [OperationContract]
        OrderAttachment GetTransferAttachmentInfo(long Id);

        [OperationContract]
        OrderAttachment GetTransferAttachment(ulong attachmentId);

        [OperationContract]
        AccountDataChangeOrder GetAccountDataChangeOrder(long ID);

        [OperationContract]
        Tuple<bool, string> IsBigAmountForPaymentOrder(PaymentOrder order);

        [OperationContract]
        Tuple<bool, string> IsBigAmountForCurrencyExchangeOrder(CurrencyExchangeOrder order);

        [OperationContract]
        double GetOrderServiceFee(OrderType type, int urgent);

        [OperationContract]
        ActionResult CheckForTransactionLimit(Order order);

        [OperationContract]
        ActionResult SaveAndApproveTransitPaymentOrder(TransitPaymentOrder order);

        [OperationContract]
        TransitPaymentOrder GetTransitPaymentOrder(long ID);

        [OperationContract]
        double GetLoanCalculatedRest(Loan loan, ulong customerNumber, short matureType);

        [OperationContract]
        string GetPaymentOrderDescription(PaymentOrder order, ulong customerNumber);

        [OperationContract]
        ActionResult SaveAndApproveCurrencyExchangeOrder(CurrencyExchangeOrder order);

        [OperationContract]
        ActionResult SaveCurrencyExchangeOrder(CurrencyExchangeOrder order);

        [OperationContract]
        ActionResult SaveAndApproveServicePaymentOrder(ServicePaymentOrder order);

        [OperationContract]
        ActionResult ConfirmOrder(long orderID);

        [OperationContract]
        double GetCustomerCashOuts(string currency);

        [OperationContract]
        List<Order> GetOrders(SearchOrders searchParams);

        [OperationContract]
        CardServiceFee GetCardServiceFee(ulong productId);

        [OperationContract]
        List<AccountFreezeDetails> GetAccountFreezeHistory(string accountNumber, ushort freezeStatus, ushort reasonId);

        [OperationContract]
        AccountFreezeDetails GetAccountFreezeDetails(string freezeId);

        [OperationContract]
        Account GetOperationSystemAccount(Order order, OrderAccountType accountType, string operationCurrency, ushort filialCode = 0, string utilityBranch = "", ulong customerNumber = 0, ushort customerType = 0);


        [OperationContract]
        CurrencyExchangeOrder GetShortChangeAmount(CurrencyExchangeOrder order);

        [OperationContract]
        Account GetRAFoundAccount();

        [OperationContract]
        infsec.AuthorizedUser AuthorizeUserBySessionToken(string authorizedUserSessionToken);

        [OperationContract]
        ACBALibrary.User InitUser(infsec.AuthorizedUser authUser);

        [OperationContract]
        ushort GetCrossConvertationVariant(string debitCurrency, string creditCurrency);

        [OperationContract]
        int GetCardType(string cardNumber);

        [OperationContract]
        ActionResult SaveAndApproveCashPosPaymentOrder(CashPosPaymentOrder order);

        [OperationContract]
        double GetCashPosPaymentOrderFee(CashPosPaymentOrder order, int feeType);

        [OperationContract]
        ActionResult SaveLoanProductOrder(LoanProductOrder order);

        [OperationContract]
        LoanProductOrder GetLoanOrder(long ID);

        [OperationContract]
        LoanProductOrder GetCreditLineOrder(long ID);

        [OperationContract]
        ActionResult ApproveLoanProductOrder(LoanProductOrder order);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductOrder(LoanProductOrder order);

        [OperationContract]
        double GetCustomerAvailableAmount(string currency);

        [OperationContract]
        double GetLoanProductInterestRate(LoanProductOrder order, string cardNumber);

        [OperationContract]
        ActionResult SaveLoanProductActivationOrder(LoanProductActivationOrder order);

        [OperationContract]
        LoanProductActivationOrder GetLoanProductActivationOrder(long ID);

        [OperationContract]
        ActionResult ApproveLoanProductActivationOrder(LoanProductActivationOrder order);

        [OperationContract]
        ActionResult SaveAndApproveAccountFreezeOrder(AccountFreezeOrder order);

        [OperationContract]
        AccountFreezeOrder GetAccountFreezeOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductActivationOrder(LoanProductActivationOrder order);

        [OperationContract]
        ActionResult SaveAndApproveAccountUnfreezeOrder(AccountUnfreezeOrder order);

        [OperationContract]
        AccountUnfreezeOrder GetAccountUnfreezeOrder(long ID);

        [OperationContract]
        double GetServiceProvidedOrderFee(OrderType orderType, ushort serviceType);

        [OperationContract]
        ActionResult SaveAndApproveFeeForServiceProvidedOrder(FeeForServiceProvidedOrder order);

        [OperationContract]
        FeeForServiceProvidedOrder GetFeeForServiceProvidedOrder(long id);

        [OperationContract]
        CashPosPaymentOrder GetCashPosPaymentOrder(long id);

        [OperationContract]
        HasHB HasPhoneBanking();

        [OperationContract]
        CardUnpaidPercentPaymentOrder GetCardUnpaidPercentPaymentOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveCardUnpaidPercentPaymentOrder(CardUnpaidPercentPaymentOrder order);

        [OperationContract]
        List<Provision> GetProductProvisions(ulong productId);

        [OperationContract]
        ActionResult SaveAndApproveRemovalOrder(RemovalOrder order);

        [OperationContract]
        RemovalOrder GetRemovalOrder(long id);

        [OperationContract]
        List<ulong> GetProvisionOwners(ulong productId);

        [OperationContract]
        LoanMainContract GetLoanMainContract(ulong productId);

        [OperationContract]
        bool IsOurCard(string cardNumber);

        [OperationContract]
        SourceType GetDepositSource(ulong productId);

        [OperationContract]
        SourceType GetAccountSource(string accountNumber);

        [OperationContract]
        double GetAcccountAvailableBalance(string accountNumber);

        [OperationContract]
        List<LoanMainContract> GetCreditLineMainContract();

        [OperationContract]
        List<LoanProductProlongation> GetLoanProductProlongations(ulong productId);

        [OperationContract]
        List<ProductOtherFee> GetProductOtherFees(ulong productId);

        [OperationContract]
        List<Claim> GetProductClaims(ulong productId, short productType);

        [OperationContract]
        List<ClaimEvent> GetClaimEvents(int claimNumber);

        [OperationContract]
        Tax GetTax(int claimNumber, int eventNumber);

        [OperationContract]
        List<MembershipRewardsStatusHistory> GetCardMembershipRewardsStatusHistory(string cardNumber);

        [OperationContract]
        List<MembershipRewardsBonusHistory> GetCardMembershipRewardsBonusHistory(string cardNumber, DateTime startDate, DateTime endDate);

        [OperationContract]
        ActionResult SaveAndApproveChequeBookReceiveOrder(ChequeBookReceiveOrder order);

        [OperationContract]
        ChequeBookReceiveOrder GetChequeBookReceiveOrder(long ID);

        [OperationContract]
        bool HasAccountPensionApplication(string accountNumber);

        [OperationContract]
        List<CardServiceFeeGrafik> GetCardServiceFeeGrafik(ulong productId);

        [OperationContract]
        CardTariffContract GetCardTariffContract(long tariffID);

        [OperationContract]
        Account GetFeeForServiceProvidedOrderCreditAccount(FeeForServiceProvidedOrder order);

        [OperationContract]
        CardTariff GetCardTariff(ulong productId);

        [OperationContract]
        List<SearchInternationalTransfer> GetSearchedInternationalTransfers(SearchInternationalTransfer searchParams);

        [OperationContract]
        List<SearchReceivedTransfer> GetSearchedReceivedTransfers(SearchReceivedTransfer searchParams);

        [OperationContract]
        List<SearchTransferBankMail> GetSearchedTransfersBankMail(SearchTransferBankMail searchParams);

        [OperationContract]
        List<SearchBudgetAccount> GetSearchedBudgetAccount(SearchBudgetAccount searchParams);

        [OperationContract]
        CardStatus GetCardStatus(ulong productId);

        [OperationContract]
        Account GetOperationSystemAccountForFee(Order order, short feeType);

        [OperationContract]
        DateTime GetCurrentOperDay();

        [OperationContract]
        List<Order> GetConfirmRequiredOrders(string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1);

        [OperationContract]
        double GetLoanProductActivationFee(ulong productId, short withTax);

        [OperationContract]
        List<Credential> GetCredentials(ProductQualityFilter filter);

        [OperationContract]
        bool IsTransferFromBusinessmanToOwnerAccount(string debitAccountNumber, string creditAccountNumber);

        [OperationContract]
        infsec.UserAccessForCustomer GetUserAccessForCustomer(string userSessiobToken, string customerSessionToken);
        [OperationContract]
        List<Account> GetAccountsForCredential(int operationType);

        [OperationContract]
        CurrencyExchangeOrder GetCurrencyExchangeOrder(long id);

        [OperationContract]
        string GenerateNextOrderNumber(ulong customerNumber);
        [OperationContract]
        ActionResult SaveAndApproveCredentialOrder(CredentialOrder order);

        [OperationContract]
        ActionResult SaveCredentialOrder(CredentialOrder order);

        [OperationContract]
        ActionResult ApproveCredentialOrder(CredentialOrder order);


        [OperationContract]
        ActionResult SaveAndApproveCredentialTerminationOrder(CredentialTerminationOrder order);

        [OperationContract]
        List<string> GetChequeBookReceiveOrderWarnings(ulong customerNumber, string accountNumber);

        [OperationContract]
        CredentialOrder GetCredentialOrder(long ID);

        [OperationContract]
        List<AssigneeOperation> GetAllOperations();

        [OperationContract]
        double GetTransitPaymentOrderFee(TransitPaymentOrder order, int feeType);

        [OperationContract]
        ActionResult ValidateRenewedOtherTypeCardApplicationForPrint(string cardNumber);

        [OperationContract]
        bool IsNormCardStatus(string cardNumber);

        [OperationContract]
        bool IsCardRegistered(string cardNumber);

        [OperationContract]
        ulong GetAccountCustomerNumber(Account account);

        [OperationContract]
        string GetSpesialPriceMessage(string accountNumber, short additionID);

        [OperationContract]
        double GetCardFeeForCurrencyExchangeOrder(CurrencyExchangeOrder order);

        [OperationContract]
        double GetAccountReopenFee(short customerType);

        [OperationContract]
        short GetCustomerSyntheticStatus(ulong customerNumber);

        [OperationContract]
        CreditLineTerminationOrder GetCreditLineTerminationOrder(long ID);

        [OperationContract]
        DepositTerminationOrder GetDepositTerminationOrder(long ID);

        [OperationContract]
        CreditLine GetClosedCreditLine(ulong productId);

        [OperationContract]
        List<string> GetLoanActivationWarnings(long productId, short productType);

        [OperationContract]
        List<LoanRepaymentGrafik> GetDecreaseLoanGrafik(CreditLine creditLine);

        [OperationContract]
        List<CashBook> GetCashBooks(SearchCashBook searchParams);

        [OperationContract]
        ActionResult SaveAndApproveCashBookOrder(CashBookOrder order);

        [OperationContract]
        int GetCorrespondentSetNumber();

        [OperationContract]
        ActionResult RemoveCashBook(int cashBookID);

        [OperationContract]
        xbs.CustomerMainData GetCustomerMainData(ulong customerNumber);

        [OperationContract]
        List<KeyValuePair<int, double>> GetRest(SearchCashBook searchParams);

        [OperationContract]
        List<SearchLeasingCustomer> GetSearchedLeasingCustomers(SearchLeasingCustomer searchParams);

        [OperationContract]
        List<LeasingLoan> GetSearchedLeasingLoans(SearchLeasingLoan searchParams);

        [OperationContract]
        ActionResult SaveAndApproveTransitCurrencyExchangeOrder(TransitCurrencyExchangeOrder order);


        [OperationContract]
        ActionResult ApproveOrder(Order order);

        [OperationContract]
        ActionResult ApproveAccountReOpenOrder(AccountReOpenOrder order);

        [OperationContract]
        CardReReleaseOrder GetCardReReleaseOrder(long ID);


        [OperationContract]
        ActionResult ApproveCardReReleaseOrder(CardReReleaseOrder order);

        [OperationContract]
        ActionResult ApproveCreditLineTerminationOrder(CreditLineTerminationOrder order);

        [OperationContract]
        ActionResult ApproveInternationalPaymentOrder(InternationalPaymentOrder order);

        [OperationContract]
        ActionResult ChangeCashBookStatus(int cashBookID, int newStatus);

        [OperationContract]
        Account GetOperationSystemAccountForLeasing(string operationCurrency, ushort filialCode);

        [OperationContract]
        Account GetTransitCurrencyExchangeOrderSystemAccount(TransitCurrencyExchangeOrder order, OrderAccountType accountType, string operationCurrency);

        [OperationContract]
        List<Account> GetReferenceOrderAccounts();

        [OperationContract]
        LoanRepaymentGrafik GetLoanNextRepayment(Loan loan);

        [OperationContract]
        List<AccountClosingHistory> GetAccountClosinghistory();

        [OperationContract]
        List<VehicleViolationResponse> GetVehicleViolationById(string violationId);

        [OperationContract]
        List<VehicleViolationResponse> GetVehicleViolationByPsnVehNum(string psn, string vehNum);

        [OperationContract]
        List<DahkDetails> GetDahkBlockages(ulong customerNumber);

        [OperationContract]
        List<DahkDetails> GetDahkCollections(ulong customerNumber);

        [OperationContract]
        ProblemLoanCalculationsDetail GetProblemLoanCalculationsDetail(int claimNumber, int eventNumber);

        [OperationContract]
        List<DahkEmployer> GetDahkEmployers(ulong customerNumber, ProductQualityFilter quality, string inquestId);

        [OperationContract]
        List<ulong> GetDAHKproductAccounts(ulong accountNumber);

        [OperationContract]
        List<LoanInterestRateChangeHistory> GetLoanInterestRateChangeHistoryDetails(ulong productID);

        [OperationContract]
        List<DahkAmountTotals> GetDahkAmountTotals(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetFreezedAccounts(ulong customerNumber);

        [OperationContract]
        List<AccountDAHKfreezeDetails> GetAccountDAHKFreezeDetails(ulong customerNumber, string inquestId, ulong accountNumber);

        [OperationContract]
        List<AccountDAHKfreezeDetails> GetCurrentInquestDetails(ulong customerNumber);

        [OperationContract]
        ActionResult MakeAvailable(List<long> freezeIdList, float availableAmount, ushort filialCode, short userId);

        [OperationContract]
        ActionResult BlockingAmountFromAvailableAccount(double accountNumber, float blockingAmount, List<DahkDetails> inquestDetailsList, int userID);


        [OperationContract]
        string GetTerm(short id, string[] param, Languages language);

        [OperationContract]
        List<Loan> GetAparikTexumLoans();

        [OperationContract]
        List<GoodsDetails> GetGoodsDetails(ulong productId);

        [OperationContract]
        AccountFlowDetails GetAccountFlowDetails(string accountNumber, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<ServicePaymentNote> GetServicePaymentNoteList();

        [OperationContract]
        ActionResult SaveAndApproveServicePaymentNoteOrder(ServicePaymentNoteOrder order);

        [OperationContract]
        ServicePaymentNoteOrder GetServicePaymentNoteOrder(long ID);

        [OperationContract]
        ServicePaymentNoteOrder GetDelatedServicePaymentNoteOrder(long ID);

        [OperationContract]
        double GetDepositLoanAndProvisionCoefficent(string loanCurrency, string provisionCurrency);

        [OperationContract]
        ActionResult SaveAndApprovePensionApplicationOrder(PensionApplicationOrder order);

        [OperationContract]
        List<PensionApplication> GetPensionApplicationHistory(ProductQualityFilter filter);

        [OperationContract]
        ActionResult SaveAndApprovePensionApplicationTerminationOrder(PensionApplicationTerminationOrder order);

        [OperationContract]
        PensionApplicationTerminationOrder GetPensionApplicationTerminationOrder(long ID);

        [OperationContract]
        PensionApplicationOrder GetPensionApplicationOrder(long ID);

        [OperationContract]
        List<Account> GetClosedDepositAccountList(DepositOrder order);

        [OperationContract]
        ActionResult SaveAndApproveTransferCallContractOrder(TransferCallContractOrder order);

        [OperationContract]
        TransferCallContractOrder GetTransferCallContractOrder(long ID);

        [OperationContract]
        List<TransferCallContractDetails> GetTransferCallContractsDetails();


        [OperationContract]
        List<int> GetTransferCriminalLogId(ulong id);

        [OperationContract]
        ActionResult UpdateTransferVerifiedStatus(ulong transferId, int verified);

        [OperationContract]
        ActionResult SaveAndApproveReestrTransferOrder(ReestrTransferOrder order);

        [OperationContract]
        ReestrTransferOrder GetReestrTransferOrder(long id);

        [OperationContract]
        List<Order> GetNotConfirmedOrders(int start, int end);

        [OperationContract]
        TransferCallContractDetails GetTransferCallContractDetails(long contractId);

        [OperationContract]
        ActionResult SaveAndApproveTransferCallContractTerminationOrder(TransferCallContractTerminationOrder order);

        [OperationContract]
        TransferCallContractTerminationOrder GetTransferCallContractTerminationOrder(long ID);

        [OperationContract]
        Order GetOrder(long ID);

        [OperationContract]
        bool AccountAccessible(string accountNumber, long accountGroup);

        [OperationContract]
        List<RejectedOrderMessage> GetRejectedMessages(int filter, int start, int end);

        [OperationContract]
        void CloseRejectedMessage(int messageId);

        [OperationContract]
        ActionResult SaveAndApproveDepositCaseOrder(DepositCaseOrder order);

        [OperationContract]
        ulong GetDepositCaseOrderContractNumber();

        [OperationContract]
        int GetTotalRejectedUserMessages();

        [OperationContract]
        int GetTotalNotConfirmedOrder();

        [OperationContract]
        List<DepositCaseMap> GetDepositCaseMap(short caseSide);

        [OperationContract]
        double GetDepositCasePrice(string caseNumber, int filialCode, short contractDuration);

        [OperationContract]
        DepositCaseOrder GetDepositCaseOrder(long id);

        [OperationContract]
        List<CardTariffContract> GetCustomerCardTariffContracts(ProductQualityFilter filter);

        [OperationContract]
        bool HasCardTariffContract();

        [OperationContract]
        int GetCardTariffContractActiveCardsCount(int contractId);

        [OperationContract]
        bool HasPosTerminal();

        [OperationContract]
        List<PosLocation> GetCustomerPosLocations(ProductQualityFilter filter);

        [OperationContract]
        PosLocation GetPosLocation(int posLocationId);

        [OperationContract]
        List<PosRate> GetPosRates(int terminalId);

        [OperationContract]
        List<Order> GetApproveReqOrder(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        List<PosCashbackRate> GetPosCashbackRates(int terminalId);

        [OperationContract]
        short IsCustomerSwiftTransferVerified(ulong customerNummber, string swiftCode = "", string receiverAaccount = "");

        [OperationContract]
        bool IsExistingTransferByCall(short transferSystem, string code, long transferId);

        [OperationContract]
        List<PlasticCard> GetCardsForRegistration();

        [OperationContract]
        ActionResult SaveAndApproveDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order);

        [OperationContract]
        DepositCasePenaltyMatureOrder GetDepositCasePenaltyMatureOrder(long id);

        [OperationContract]
        ActionResult SavePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order);

        [OperationContract]
        ActionResult SaveAndAprovePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCardRegistrationOrder(CardRegistrationOrder order);

        [OperationContract]
        List<Account> GetAccountListForCardRegistration(string cardCurrency, int cardFililal);

        [OperationContract]
        List<Account> GetOverdraftAccountsForCardRegistration(string cardCurrency, int cardFililal);

        [OperationContract]
        CardRegistrationOrder GetCardRegistrationOrder(long ID);
        [OperationContract]
        ActionResult ApprovePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order);

        [OperationContract]
        double GetPeriodicSwiftStatementOrderFee();

        [OperationContract]
        PeriodicSwiftStatementOrder GetPeriodicSwiftStatementOrder(long ID);

        [OperationContract]
        bool CheckTransferToShopPayment(ulong productId);

        [OperationContract]
        ActionResult SaveAndApproveTransferToShopOrder(TransferToShopOrder order);

        [OperationContract]
        Account GetShopAccount(ulong productId);

        [OperationContract]
        TransferToShopOrder GetTransferToShopOrder(long ID);

        [OperationContract]
        double GetCOWaterOrderAmount(string abonentNumber, string branchCode, ushort paymentType);

        [OperationContract]
        List<string> GetCardRegistrationWarnings(PlasticCard plasticCard);

        [OperationContract]
        List<Provision> GetCustomerProvisions(string currency, ushort type, ushort quality);

        [OperationContract]
        List<ProvisionLoan> GetProvisionLoans(ulong provisionId);

        [OperationContract]
        Insurance GetInsurance(ulong productId);

        [OperationContract]
        List<Insurance> GetInsurances(ProductQualityFilter filter);

        [OperationContract]
        List<Insurance> GetPaidInsurance(ulong loanProductId);

        [OperationContract]
        InsuranceOrder GetInsuranceOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveInsuranceOrder(InsuranceOrder order);

        [OperationContract]
        Account GetInsuraceCompanySystemAccount(ushort companyID, ushort insuranceType);

        [OperationContract]
        uint GetInsuranceCompanySystemAccountNumber(ushort companyID, ushort insuranceType);

        [OperationContract]
        bool CheckAccountForDAHK(string accountNumber);
        [OperationContract]
        ActionResult SaveAndApproveCardDataChangeOrder(CardDataChangeOrder order);

        [OperationContract]
        CardDataChangeOrder GetCardDataChangeOrder(long ID);

        [OperationContract]
        bool CheckCardDataChangeOrderFieldTypeIsRequaried(short fieldType);

        [OperationContract]
        ActionResult SaveAndApproveCardServiceFeeGrafikDataChangeOrder(CardServiceFeeGrafikDataChangeOrder order);

        [OperationContract]
        CardServiceFeeGrafikDataChangeOrder GetCardServiceFeeGrafikDataChangeOrder(long ID);

        [OperationContract]
        List<CardServiceFeeGrafik> SetNewCardServiceFeeGrafik(ulong productId);

        [OperationContract]
        List<GivenGuaranteeReduction> GetGivenGuaranteeReductions(ulong productId);

        [OperationContract]
        ActionResult SaveAndApproveReestrUtilityPaymentOrder(ReestrUtilityPaymentOrder order);

        [OperationContract]
        ReestrUtilityPaymentOrder GetReestrUtilityPaymentOrder(long id);

        [OperationContract]
        ActionResult SaveAndApproveAccountAdditionalDataRemovableOrder(AccountAdditionalDataRemovableOrder order);

        [OperationContract]
        DAHKDetail GetCardDAHKDetails(string cardNumber);

        [OperationContract]
        bool IsDAHKAvailability(ulong customerNumber);

        [OperationContract]
        PlasticCard GetPlasticCard(ulong productId, bool productIdType);

        [OperationContract]
        int GetPoliceResponseDetailsIDWithoutRequest(string violationID, DateTime violationDate);

        [OperationContract]
        ulong GetOrderTransactionsGroupNumber(long orderId);

        [OperationContract]
        List<Account> GetATSSystemAccounts(string currency);

        [OperationContract]
        ArcaBalanceResponseData GetArCaBalanceResponseData(string cardNumber);

        [OperationContract]
        bool HasATSSystemAccountInFilial();

        [OperationContract]
        List<LoanProductClassification> GetLoanProductClassifications(ulong productId, DateTime dateFrom);

        [OperationContract]
        List<SafekeepingItem> GetSafekeepingItems(ProductQualityFilter filter);

        [OperationContract]
        SafekeepingItem GetSafekeepingItem(ulong productId);

        [OperationContract]
        List<ExchangeRate> GetExchangeRatesHistory(int filialCode, string currency, DateTime startDate);

        [OperationContract]
        List<CrossExchangeRate> GetCrossExchangeRatesHistory(int filialCode, DateTime startDate);

        [OperationContract]
        List<Account> GetTransitAccountsForDebitTransactions();

        [OperationContract]
        List<TransitAccountForDebitTransactions> GetAllTransitAccountsForDebitTransactions(ProductQualityFilter quality);

        [OperationContract]
        Account GetCardCashbackAccount(ulong productId);

        [OperationContract]
        string GetCardMotherName(ulong productId);

        [OperationContract]
        List<ExchangeRate> GetCBExchangeRatesHistory(string currency, DateTime startDate);

        [OperationContract]
        List<CorrespondentBankAccount> GetCorrespondentBankAccounts(CorrespondentBankAccount filter);

        [OperationContract]
        ActionResult SaveAndApproveCardStatusChangeOrder(CardStatusChangeOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCardMembershipRewardsOrder(MembershipRewardsOrder order);

        [OperationContract]
        CardStatusChangeOrder GetCardStatusChangeOrder(long orderId);

        [OperationContract]
        ActionResult SaveTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account);

        [OperationContract]
        ActionResult UpdateTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account);

        [OperationContract]
        ActionResult CloseTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account);

        [OperationContract]
        ActionResult ReopenTransitAccountForDebitTransactions(TransitAccountForDebitTransactions account);

        [OperationContract]
        double GetBusinesDepositOptionRate(ushort depositOption, string currency);

        [OperationContract]
        List<CardActivationInArCa> GetCardActivationInArCa(string cardNumber, DateTime startDate, DateTime endDate);

        [OperationContract]
        DateTime? GetLastSendedPaymentFileDate();

        [OperationContract]
        List<CardActivationInArCaApigateDetails> GetCardActivationInArCaApigateDetail(ulong Id);

        [OperationContract]
        ActionResult SaveCustomerCommunalCard(CustomerCommunalCard customerCommunalCard);

        [OperationContract]
        ActionResult ChangeCustomerCommunalCardQuality(CustomerCommunalCard customerCommunalCard);

        [OperationContract]
        List<CustomerCommunalCard> GetCustomerCommunalCards();
        [OperationContract]
        List<double> GetComunalAmountPaidThisMonth(string code, short comunalType, short abonentType, DateTime DebtListDate, string texCode, int waterCoPaymentType);

        [OperationContract]
        ActionResult SaveFactoringTerminationOrder(FactoringTerminationOrder order);

        [OperationContract]
        FactoringTerminationOrder GetFactoringTerminationOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveFactoringTerminationOrder(FactoringTerminationOrder order);

        [OperationContract]
        ActionResult ApproveFactoringTerminationOrder(FactoringTerminationOrder order);

        [OperationContract]
        ActionResult SaveLoanProductTerminationOrder(LoanProductTerminationOrder order);

        [OperationContract]
        LoanProductTerminationOrder GetLoanProductTerminationOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductTerminationOrder(LoanProductTerminationOrder order);

        [OperationContract]
        ActionResult ApproveLoanProductTerminationOrder(LoanProductTerminationOrder order);

        [OperationContract]
        double GetShopTransferAmount(ulong productId);

        [OperationContract]
        ActionResult SaveDepositDataChangeOrder(DepositDataChangeOrder order);

        [OperationContract]
        DepositDataChangeOrder GetDepositDataChangeOrder(long ID);

        [OperationContract]
        ActionResult ApproveDepositDataChangeOrder(DepositDataChangeOrder order);

        [OperationContract]
        ActionResult SaveAndApproveDepositDataChangeOrder(DepositDataChangeOrder order);

        [OperationContract]
        List<DepositAction> GetDepositActions(DepositOrder order);

        [OperationContract]
        List<ENAPayments> GetENAPayments(string abonentNumber, string branch);

        [OperationContract]
        List<DateTime> GetENAPaymentDates(string abonentNumber, string branch);

        [OperationContract]
        double GetLoanTotalInsuranceAmount(ulong productId);

        [OperationContract]
        Account GetProductAccount(ulong productId, ushort productType, ushort accountType);

        [OperationContract]
        Account GetProductAccountFromCreditCode(string creditCode, ushort productType, ushort accountType);

        [OperationContract]
        CashBookOrder GetCashBookOrder(long ID);

        [OperationContract]
        Account GetCashBookOperationSystemAccount(CashBookOrder order, OrderAccountType accountType, ushort filialCode);

        [OperationContract]
        LoanApplication GetLoanApplication(ulong productId);

        [OperationContract]
        List<LoanApplication> GetLoanApplications();

        [OperationContract]
        List<FicoScoreResult> GetLoanApplicationFicoScoreResults(ulong productId, DateTime requestDate);

        [OperationContract]
        List<ActionError> FastOverdraftValidations(string cardNumber);

        [OperationContract]
        LoanApplication GetLoanApplicationByDocId(long docId);

        [OperationContract]
        List<CardTariffContract> GetCardTariffContracts(ProductQualityFilter filter, ulong customerNumber);

        [OperationContract]
        ActionResult SaveAndApproveDepositCaseStoppingPenaltyCalculationOrder(DepositCaseStoppingPenaltyCalculationOrder order);

        [OperationContract]
        DepositCaseStoppingPenaltyCalculationOrder GetDepositCaseStoppingPenaltyCalculationOrder(long ID);

        [OperationContract]
        CTPaymentOrder GetCTPaymentOrder(long ID);


        [OperationContract]
        CTLoanMatureOrder GetCTLoanMatureOrder(long ID);

        [OperationContract]
        ActionResult SaveLoanMonitoringConclusion(LoanMonitoringConclusion monitoring);

        [OperationContract]
        ActionResult ApproveLoanMonitoringConclusion(long monitoringId);

        [OperationContract]
        ActionResult DeleteLoanMonitoringConclusion(long monitoringId);

        [OperationContract]
        List<LoanMonitoringConclusion> GetLoanMonitoringConclusions(long productId);

        [OperationContract]
        LoanMonitoringConclusion GetLoanMonitoringConclusion(long monitoringId, long productId);

        [OperationContract]
        List<MonitoringConclusionLinkedLoan> GetLinkedLoans(long productId);

        [OperationContract]
        float GetProvisionCoverCoefficient(long productId);

        [OperationContract]
        short GetLoanMonitoringType();

        [OperationContract]
        Dictionary<string, string> InitUserPagePermissions(string userSessionToken);

        [OperationContract]
        string GetAccountDescription(string accountNumber);

        [OperationContract]
        ActionResult SaveAndApproveCredentialActivationOrder(CredentialActivationOrder order);

        [OperationContract]
        CredentialActivationOrder GetCredentialActivationOrder(long ID);

        [OperationContract]
        List<CreditHereAndNow> GetCreditsHereAndNow(SearchCreditHereAndNow serachParameters, out int RowCount);

        [OperationContract]
        ActionResult SaveAndApproveCreditHereAndNowActivationOrders(CreditHereAndNowActivationOrders creditHereAndNowActivationOrders);

        [OperationContract]
        List<PreOrderDetails> GetSearchedPreOrderDetails(SearchPreOrderDetails serachParameters, out int RowCount);

        [OperationContract]
        Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrdersLoanActivation(long preOrderId);

        [OperationContract]
        ulong AuthorizeCustomerByLoanApp(ulong productId);

        [OperationContract]
        List<ClassifiedLoan> GetClassifiedLoans(SearchClassifiedLoan searchParameters, out int RowCount);

        [OperationContract]
        ActionResult SaveAndApproveClassifiedLoanActionOrders(ClassifiedLoanActionOrders classifiedLoanActionOrders);

        [OperationContract]
        Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrdersClassificationRemove(long preOrderId);

        [OperationContract]
        Task<List<ActionResult>> SaveAndApproveAutomaticGenaratedPreOrdersMakeLoanOut(long preOrderId);

        [OperationContract]
        bool HasPropertyProvision(ulong productId);
        [OperationContract]
        ActionResult SaveAndApproveAssigneeIdentificationOrder(AssigneeIdentificationOrder order);

        [OperationContract]
        int GetCredentialDocId(ulong credentialId);

        [OperationContract]
        AssigneeIdentificationOrder GetAssigneeIdentificationOrder(long ID);

        [OperationContract]
        Dictionary<string, string> ProvisionContract(long docId);

        [OperationContract]
        ulong GetNextCredentialDocumentNumber();

        [OperationContract]
        ActionResult SaveAndApproveDemandDepositRateChangeOrder(DemandDepositRateChangeOrder order);

        [OperationContract]
        DemandDepositRateChangeOrder GetDemandDepositRateChangeOrder(long ID);

        [OperationContract]
        MembershipRewardsOrder GetCardMembershipRewardsOrder(long ID);


        [OperationContract]
        DemandDepositRate GetDemandDepositRate(string accountNumber);


        [OperationContract]
        List<AccountOpeningClosingDetail> GetAccountOpeningClosingDetails(string accountNumber);

        [OperationContract]
        AccountOpeningClosingDetail GetAccountOpeningDetail(string accountNumber);


        [OperationContract]
        CreditLinePrecontractData GetCreditLinePrecontractData(DateTime startDate, DateTime endDate, double interestRate, double repaymentPercent, string cardNumber, string currency, double amount, int loanType);

        [OperationContract]
        xbs.Customer GetCustomerData(ulong customerNumber);

        [OperationContract]
        List<DemandDepositRate> GetDemandDepositRateTariffs();

        [OperationContract]
        short GetCustomerFilial();

        [OperationContract]
        ulong GetBankruptcyManager(string accountNumber);

        [OperationContract]
        double GetDepositLoanCreditLineAndProfisionCoefficent(string loanCurrency, string provisionCurrency, bool mandatoryPayment, int creditLineType);

        [OperationContract]
        ActionResult SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order);

        [OperationContract]
        string GetReceivedFastTransferOrderRejectReason(int orderId);

        [OperationContract]
        ActionResult ApproveFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order);

        [OperationContract]
        int GetStatementFixedReceivingType(string cardnumber);

        [OperationContract]
        List<OrderHistory> GetOnlineOrderHistory(long orderId);


        [OperationContract]
        List<CashierCashLimit> GetCashierLimits(int setNumber);

        [OperationContract]
        ActionResult GenerateCashierCashDefaultLimits(int setNumber, int changeSetNumber);


        [OperationContract]
        ActionResult SaveCashierCashLimits(List<CashierCashLimit> limit);

        [OperationContract]
        int GetCashierFilialCode(int setNumber);


        [OperationContract]
        ActionResult SaveAndApproveProductNotificationConfigurationsOrder(ProductNotificationConfigurationsOrder order);

        [OperationContract]
        List<ProductNotificationConfigurations> GetProductNotificationConfigurations(ulong productId);

        [OperationContract]
        ActionResult SaveAndApproveClassificationRemoveOrder(LoanProductClassificationRemoveOrder order, bool includingSurcharge);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductMakeOutOrder(LoanProductMakeOutOrder order, bool includingSurcharge);

        [OperationContract]
        ActionResult SaveBranchDocumentSignatureSetting(DocumentSignatureSetting setting);


        [OperationContract]
        DocumentSignatureSetting GetBranchDocumentSignatureSetting();

        [OperationContract]
        List<Account> GetDecreasingDepositAccountList(ulong customerNumber);

        [OperationContract]
        List<SwiftMessage> GetSearchedSwiftMessages(SearchSwiftMessage searchSwiftMessage);

        [OperationContract]
        SwiftMessage GetSwiftMessage(ulong messageUnicNumber);

        [OperationContract]
        CardServiceQualities GetCardUSSDService(ulong productID);

        [OperationContract]
        string GetCardMobilePhone(ulong productID);

        [OperationContract]
        ActionResult SaveAndApproveCardUSSDServiceOrder(CardUSSDServiceOrder order);


        [OperationContract]
        CardUSSDServiceOrder GetCardUSSDServiceOrder(long orderId);

        [OperationContract]
        List<float> GetCardUSSDServiceTariff(ulong productID);


        [OperationContract]
        CardServiceQualities GetPlasticCardSMSService(string cardNumber);

        [OperationContract]
        ActionResult SaveAndApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order);

        [OperationContract]
        ActionResult ApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order);

        [OperationContract]
        ActionResult SavePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order);

        [OperationContract]
        PlasticCardSMSServiceHistory GetPlasticCardSMSServiceHistory(string cardNumber);

        [OperationContract]
        PlasticCardSMSServiceOrder GetPlasticCardSMSServiceOrder(long orderId);

        [OperationContract]
        ActionResult CustomersClassification();

        [OperationContract]
        ActionResult SaveAndApproveTransactionSwiftConfirmOrder(TransactionSwiftConfirmOrder order);
        [OperationContract]
        ActionResult SaveAndApproveSwiftMessageRejectOrder(SwiftMessageRejectOrder order);

        [OperationContract]
        List<HBProductPermission> GetHBUserProductsPermissions(string hbUserName);

        [OperationContract]
        AuthorizedCustomer GetTestMobileBankingUser();

        [OperationContract]
        Account GetAccountInfo(string accountNumber);


        [OperationContract]
        List<LoanEquipment> GetSearchedLoanEquipment(SearchLoanEquipment searchLoanEquipment);

        [OperationContract]
        LoanEquipment GetSumsOfEquipmentPrices(SearchLoanEquipment searchLoanEquipment);

        [OperationContract]
        TransactionSwiftConfirmOrder GetTransactionSwiftConfirmOrder(long orderId);
        [OperationContract]
        ActionResult SaveAndApproveCard3DSecureServiceOrder(Card3DSecureServiceOrder order);

        [OperationContract]
        Card3DSecureService GetCard3DSecureService(ulong productID);

        [OperationContract]
        List<Card3DSecureService> GetCard3DSecureServiceHistory(ulong productID);

        [OperationContract]
        List<CardUSSDServiceHistory> GetCardUSSDServiceHistory(ulong productID);

        [OperationContract]
        Card GetCardWithOutBallance(string accountNumber);

        [OperationContract]
        LoanEquipment GetEquipmentDetails(int equipmentID);

        [OperationContract]
        string GetEquipmentClosingReason(int equipmentID);

        [OperationContract]
        ActionResult LoanEquipmentClosing(int equipmentID, int setNumber, string closingReason);

        [OperationContract]
        ActionResult ChangeCreditProductMatureRestriction(double appID, int setNumber, int allowMature);
        [OperationContract]
        ProductNotificationConfigurationsOrder GetProductNotificationConfigurationOrder(long ID);


        [OperationContract]
        BondIssue GetBondIssue(int id);

        [OperationContract]
        ActionResult SaveBondIssue(BondIssue bondissue);

        [OperationContract]
        ActionResult DeleteBondIssue(int id);


        [OperationContract]
        ActionResult ApproveBondIssue(int id);


        [OperationContract]
        List<BondIssue> SearchBondIssues(BondIssueFilter searchParams);

        [OperationContract]
        List<DateTime> CalculateCouponRepaymentSchedule(BondIssue bondissue);

        [OperationContract]
        List<DateTime> GetCouponRepaymentSchedule(BondIssue bondissue);

        [OperationContract]
        List<Bond> GetBonds(BondFilter filter);

        [OperationContract]
        Bond GetBondByID(int ID);

        [OperationContract]
        ActionResult SaveAndApproveBondOrder(BondOrder order);

        [OperationContract]
        BondOrder GetBondOrder(long ID);

        [OperationContract]
        int GetNonDistributedBondsCount(int bondIssueId);

        [OperationContract]
        List<Account> GetAccountsForCouponRepayment();

        [OperationContract]
        List<Account> GetAccountsForBondRepayment(string currency);

        [OperationContract]
        double GetBondPrice(int bondIssueId);

        [OperationContract]
        BondQualityUpdateOrder GetBondQualityUpdateOrder(long ID);

        [OperationContract]
        bool HasCustomerDepositaryAccountInBankDB(ulong customerNumber);

        [OperationContract]
        DepositaryAccount GetCustomerDepositaryAccount(ulong customerNumber);

        [OperationContract]
        List<Bond> GetBondsForDealing(BondFilter searchParams, string bondFilterType);

        [OperationContract]
        ActionResult SaveAndApproveBondAmountChargeOrder(BondAmountChargeOrder order);

        [OperationContract]
        BondAmountChargeOrder GetBondAmountChargeOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveDepositaryAccountOrder(DepositaryAccountOrder order);

        [OperationContract]
        ActionResult SaveAndApproveBondQualityUpdateOrder(BondQualityUpdateOrder order);

        [OperationContract]
        DepositaryAccountOrder GetDepositaryAccountOrder(int id);

        [OperationContract]
        DepositaryAccount GetDepositaryAccountById(int id);

        [OperationContract]
        double GetLastCrossExchangeRate(string dCur, string cCur, ushort filialCode = 22000);

        [OperationContract]
        List<Account> GetCustomerTransitAccounts(ProductQualityFilter filter);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductMakeOutBalanceOrder(LoanProductMakeOutBalanceOrder order, bool includingSurcharge);

        [OperationContract]
        double GetCashBookAmount(int cashBookID);

        [OperationContract]
        bool HasUnconfirmedOrder(int cashBookID);

        [OperationContract]
        ActionResult SaveAndApproveCreditLineProlongationOrder(CreditLineProlongationOrder order);

        [OperationContract]
        ActionResult SaveAndApproveFondOrderr(FondOrder order);

        [OperationContract]
        List<Fond> GetFonds(ProductQualityFilter filter);

        [OperationContract]
        Fond GetFondByID(int ID);

        [OperationContract]
        FondOrder GetFondOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order);

        [OperationContract]
        PeriodicTransferDataChangeOrder GetPeriodicDataChangeOrder(long ID);

        [OperationContract]
        CreditLineProlongationOrder GetCreditLineProlongationOrder(int id);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductDataChangeOrder(LoanProductDataChangeOrder order);

        [OperationContract]
        LoanProductDataChangeOrder GetLoanProductDataChangeOrder(long ID);

        [OperationContract]
        bool ExistsLoanProductDataChange(ulong appId);

        [OperationContract]
        ActionResult ConfirmOrRejectDepositProductPrices(string listOfId, int confirmationSetNumber, byte status, string rejectionDescription);

        [OperationContract]
        ActionResult DeleteDepositProductPrices(int id, int registrationSetNumber);

        [OperationContract]
        ActionResult UpdateDepositPrices(DepositProductPrices product);

        [OperationContract]
        List<DepositProductPrices> GetDepositProductPrices(SearchDepositProductPrices searchParameters);

        [OperationContract]
        ActionResult AddDepositProductPrices(DepositProductPrices searchProduct);

        [OperationContract]
        List<Account> GetCreditCodesTransitAccounts(ProductQualityFilter filter);

        [OperationContract]
        string GetInternationalTransferSentTime(int docID);

        [OperationContract]
        bool HasOrHadAccount(ulong customerNumber);

        [OperationContract]
        FTPRate GetFTPRateDetails(FTPRateType rateType);

        [OperationContract]
        ActionResult SaveAndApproveFTPRateOrder(FTPRateOrder order);

        [OperationContract]
        List<OperDayOptions> SearchOperDayOptions(OperDayOptionsFilter searchParams);

        [OperationContract]
        ActionResult SaveOperDayOptions(List<OperDayOptions> list);

        [OperationContract]
        FTPRateOrder GetFTPRateOrder(long ID);

        [OperationContract]
        ActionResult GenerateAndMakeSwiftMessagesByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        ActionResult SaveOperDayMode(OperDayMode operDayMode);

        [OperationContract]
        List<OperDayMode> GetOperDayModeHistory(OperDayModeFilter operDayMode);

        [OperationContract]
        KeyValuePair<string, string> GetCurrentOperDay24_7_Mode();

        [OperationContract]
        Dictionary<int, List<ProblemLoanTax>> SearchProblemLoanTax(ProblemLoanTaxFilter problemLoanTaxFilter, bool Cache);

        [OperationContract]
        ProblemLoanTax GetProblemLoanTaxDetails(long AppId);

        [OperationContract]
        List<Account> GetFactoringCustomerCardAndCurrencyAccounts(ulong productId, string currency);
        [OperationContract]
        List<Account> GetFactoringCustomerFeeCardAndCurrencyAccounts(ulong productId);
        [OperationContract]
        void SetTransferByCallType(short type, long id);

        [OperationContract]
        List<UtilityOptions> SearchUtilityOptions(UtilityOptionsFilter searchParams);

        [OperationContract]
        List<UtilityOptions> GetUtiltyForChange();

        [OperationContract]
        List<Order> GetOrdersByFilter(OrderFilter orderFilter);

        [OperationContract]
        ActionResult SaveUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions);

        [OperationContract]
        ActionResult SaveAllUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions, int a);

        [OperationContract]
        List<string> GetExistsNotSentAndSettledRows(Dictionary<int, bool> keyValues);


        [OperationContract]
        decimal? GetBeelineAbonentBalance(string abonentNumber);

        [OperationContract]
        Dictionary<string, string> GetAccountFreezeReasonsTypesForOrder(bool isHB = false);

        [OperationContract]
        ActionResult SaveAndApproveCardLimitChangeOrder(CardLimitChangeOrder cardLimitChangeOrder);

        [OperationContract]
        CardLimitChangeOrder GetCardLimitChangeOrder(long orderId);

        [OperationContract]
        ActionResult SaveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder);

        [OperationContract]
        ActionResult SaveAndApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder);

        [OperationContract]
        short GetBlockingReasonForBlockedCard(string cardNumber);

        [OperationContract]
        ActionResult ApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder);

        [OperationContract]
        ArcaCardsTransactionOrder GetArcaCardsTransactionOrder(long orderId);

        [OperationContract]
        Dictionary<string, string> GetCardLimits(long productId);

        [OperationContract]
        string GetEmbossingName(string cardNumber, ulong productId);

        [OperationContract]
        string GetAttachedCardEmbossingName(string cardNumber);

        [OperationContract]
        ActionResult SaveCardToCardOrder(CardToCardOrder order);

        [OperationContract]
        ActionResult ToCardWithECommerce(CardToCardOrder order);

        [OperationContract]
        double GetCardToCardTransferFee(string debitCardNumber, string creditCardNumber, double amount, string currency);

        [OperationContract]
        ActionResult SaveAndApproveCardToCardOrder(CardToCardOrder order);

        [OperationContract]
        CardToCardOrder GetCardToCardOrder(long orderId);

        [OperationContract]
        AttachedCardTransactionReceipt GetAttachedCardTransactionDetails(long orderId);

        [OperationContract]
        CreditCommitmentForgivenessOrder GetForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness);

        [OperationContract]
        ActionResult SaveForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness);

        [OperationContract]
        ActionResult SaveUtiliyPaymentOrderTemplate(UtilityPaymentOrderTemplate template);

        [OperationContract]
        double GetTaxForForgiveness(ulong customerNumber, double? capital, string RebetType, string currency);


        [OperationContract]
        CreditCommitmentForgivenessOrder GetCreditCommitmentForgiveness(long ID);
        [OperationContract]
        List<InfSecServiceReference.ApplicationClientPermissions> GetPermittedReports(InfSecServiceReference.ApplicationClientPermissionsInfo userReportPermissionInfo);
        [OperationContract]
        infsec.AuthorizedUser AuthorizeUserBySAPTicket(string ticket, string softName);

        [OperationContract]
        bool HasCustomerOnlineBanking(ulong customerNumber);

        [OperationContract]
        List<OrderGroup> GetOrderGroups(OrderGroupStatus status, OrderGroupType groupType);

        [OperationContract]
        int GetTransferTypeByAppId(ulong appId);

        [OperationContract]
        PaymentOrderTemplate GetPaymentOrderTemplate(int id);
        [OperationContract]
        byte[] GetOpenedAccountContract(string acccountNumber);

        [OperationContract]
        BudgetPaymentOrderTemplate GetBudgetPaymentOrderTemplate(int id);

        [OperationContract]
        LoanMatureOrderTemplate GetLoanMatureOrderTemplate(int id);

        [OperationContract]
        string SaveUploadedFile(UploadedFile uploadedFile);

        [OperationContract]
        ActionResult ApproveHBServletRequestOrder(HBServletRequestOrder order);

        [OperationContract]
        ActionResult SaveHBServletRequestOrder(HBServletRequestOrder order);

        [OperationContract]
        HBServletRequestOrder GetHBServletRequestOrder(long ID);

        [OperationContract]
        ReadXmlFileAndLog ReadXmlFile(string fileId, short filial);

        [OperationContract]

        ActionResult SaveAndApproveHBApplicationReplacmentOrder(HBApplicationOrder order);


        [OperationContract]
        List<HBActivationRequest> GetHBRequests();

        [OperationContract]
        ActionResult SaveAndApproveHBActivationOrder(HBActivationOrder order);

        [OperationContract]
        List<string> GetHBTokenNumbers(HBTokenTypes tokenType, short filialCode);

        [OperationContract]
        HBToken GetHBToken(int tokenId);

        [OperationContract]
        HBToken GetHBTokenWithSerialNumber(string TokenSerial);

        [OperationContract]
        List<HBToken> GetHBTokens(int HBUserID, ProductQualityFilter filter);

        [OperationContract]
        List<HBToken> GetFilteredHBTokens(int HBUserID, HBTokenQuality filter);

        [OperationContract]
        ActionResult SaveAndApproveHBServletRequestOrder(HBServletRequestOrder order);

        [OperationContract]
        HBApplication GetHBApplication();

        [OperationContract]
        HBUser GetHBUser(int hbUserID);

        [OperationContract]
        HBUser GetHBUserByUserName(string hbUserName);

        [OperationContract]
        HBApplicationOrder GetHBApplicationOrder(long ID);



        [OperationContract]
        HBActivationOrder GetHBActivationOrder(long ID);


        [OperationContract]
        UtilityPaymentOrderTemplate GetUtilityPaymentOrderTemplate(int id);

        [OperationContract]
        OrderType GetDocumentType(int docId);


        [OperationContract]
        InternationalPaymentOrder GetCustomerDateForInternationalPayment();


        [OperationContract]
        ActionResult SaveProductNote(ProductNote productNote);

        [OperationContract]
        ProductNote GetProductNote(double uniqueId);

        [OperationContract]
        ActionResult SaveAccountClosingOrder(AccountClosingOrder order);

        [OperationContract]
        ActionResult ApproveAccountClosingOrder(AccountClosingOrder order);

        [OperationContract]
        ActionResult SaveInternationalPaymentOrder(InternationalPaymentOrder order);

        [OperationContract]
        ActionResult SaveCreditLineTerminationOrder(CreditLineTerminationOrder order);

        [OperationContract]
        List<DAHKFreezing> GetDahkFreezings();

        [OperationContract]
        InterestMargin GetInterestMarginDetails(InterestMarginType marginType);

        [OperationContract]
        InterestMargin GetInterestMarginDetailsByDate(InterestMarginType marginType, DateTime marginDate);

        [OperationContract]
        ActionResult SaveAndApproveInterestMarginOrder(InterestMarginOrder order);

        [OperationContract]
        InterestMarginOrder GetInterestMarginOrder(long ID);
        [OperationContract]
        string GetSwiftMessage950Statement(DateTime dateFrom, DateTime dateTo, string accountNumber, SourceType source);

        [OperationContract]
        List<DepositRepayment> GetDepositRepaymentsPrior(DepositRepaymentRequest request);

        [OperationContract]
        Card GetCardByCardNumber(string cardNumber);

        [OperationContract]
        List<KeyValuePair<string, string>> GetCommunalReportParametersIBanking(long orderId, CommunalTypes communalType);

        [OperationContract]
        ActionResult SaveOrderGroup(OrderGroup group);

        [OperationContract]
        ActionResult ApproveReestrTransferOrder(ReestrTransferOrder order);

        [OperationContract]
        string GetPasswordForCustomerDataOrder();

        [OperationContract]
        string GetEmailForCustomerDataOrder();

        [OperationContract]
        DepositRateTariff GetDepositRateTariff(DepositType depositType);

        [OperationContract]
        ActionResult ApproveCardToCardOrder(CardToCardOrder cardToCardOrder);

        [OperationContract]
        ActionResult SaveCardLimitChangeOrder(CardLimitChangeOrder order);

        [OperationContract]
        ActionResult ApproveCardLimitChangeOrder(CardLimitChangeOrder order);

        [OperationContract]
        ActionResult SavePaymentOrderTemplate(PaymentOrderTemplate template);

        [OperationContract]
        ActionResult SaveLoanMatureOrderTemplate(LoanMatureOrderTemplate template);

        [OperationContract]
        byte[] LoansDramContract(string accountNumber);

        [OperationContract]
        ActionResult SaveAndApprovePlasticCardRemovalOrder(PlasticCardRemovalOrder cardOrder);
        [OperationContract]
        void SendReminderNote(ulong customerNumber);

        [OperationContract]
        ActionResult DeleteOrderGroup(int groupId);

        [OperationContract]
        ActionResult ChangeTemplateStatus(int id, TemplateStatus status);
        [OperationContract]
        List<PlasticCard> GetCustomerMainCards();
        [OperationContract]
        CreditLine GetCreditLineByAccountNumber(string loanFullNumber);

        [OperationContract]
        PlasticCardRemovalOrder GetPlasticCardRemovalOrder(long orderID);
        [OperationContract]
        ActionResult SavePlasticCardOrder(PlasticCardOrder cardOrder);

        [OperationContract]
        ActionResult ApprovePlasticCardOrder(PlasticCardOrder cardOrder);
        [OperationContract]
        ActionResult SaveAndApproveCardAccountRemovalOrder(CardAccountRemovalOrder order);

        [OperationContract]
        ActionResult SaveCardToCardOrderTemplate(CardToCardOrderTemplate template);

        [OperationContract]
        List<PlasticCard> GetCustomerPlasticCards();
        [OperationContract]
        List<Order> GetOrdersList(OrderListFilter orderListFilter);

        [OperationContract]
        bool IsCurrentAccount(string accountNumber);

        [OperationContract]
        Card GetCardByAccountNumber(string accountNumber);

        [OperationContract]
        ActionResult SaveInternationalOrderTemplate(InternationalOrderTemplate template);


        [OperationContract]
        bool IsAbleToChangeQuality(string userName, int id);

        [OperationContract]
        InternationalOrderTemplate GetInternationalOrderTemplate(int id);

        [OperationContract]
        List<Template> GetCustomerTemplates(TemplateStatus status);


        [OperationContract]
        CardToCardOrderTemplate GetCardToCardOrderTemplate(int templateId);

        [OperationContract]
        ActionResult SaveReestrTransferOrder(ReestrTransferOrder order, string fileId);

        [OperationContract]
        ActionResult CheckExcelRows(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails, string debetAccount, long orderId);


        [OperationContract]
        CreditLine GetCardOverDraft(string cardNumber);

        [OperationContract]
        ulong GetCardCustomerNumber(string cardNumber);

        [OperationContract]
        Dictionary<string, string> GetPlasticCardOrderCardTypes();


        [OperationContract]
        List<string> GetDepositAndCurrentAccountCurrencies(OrderType orderType, byte orderSubType, OrderAccountType accountType);

        [OperationContract]
        double GetRedemptionAmountForDepositLoan(double startCapital, double interestRate, DateTime dateOfBeginning, DateTime dateOfNormalEnd, DateTime firstRepaymentDate);

        [OperationContract]
        double GetCommisionAmountForDepositLoan(double startCapital, DateTime dateOfBeginning, DateTime dateofNormalEnd, string currency);

        [OperationContract]
        double GetCreditLineDecreasingAmount(double startCapital, string currency, DateTime startDate, DateTime endDate);

        [OperationContract]
        Dictionary<string, string> GetDepositCreditLineContractInfo(int docId);

        [OperationContract]
        Dictionary<string, string> GetDepositLoanContractInfo(int docId);


        [OperationContract]
        string GetConnectAccountFullNumber(string currency);


        [OperationContract]
        string GetCardTypeName(string cardNumber);


        [OperationContract]
        byte[] GetDepositLoanOrDepositCreditLineContract(string loanNumber, byte type);

        [OperationContract]
        ActionResult SavePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order);

        [OperationContract]
        ActionResult ApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order);

        [OperationContract]
        List<Card> GetLinkedCards();

        [OperationContract]
        List<Card> GetLinkedAndAttachedCards(ulong productId, ProductQualityFilter productFilter = ProductQualityFilter.Opened);

        [OperationContract]
        List<Account> GetClosedAccounts();

        [OperationContract]
        CardAdditionalInfo GetCardAdditionalInfo(ulong productId);

        [OperationContract]
        double GetCashBackAmount(ulong productId);

        [OperationContract]
        LoanStatement GetLoanStatement(string account, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0);

        [OperationContract]
        byte[] GetAttachedFile(long docID, int type);

        [OperationContract]
        byte[] GetExistingDepositContract(long docID, int type);

        [OperationContract]
        PensionSystem GetPensionSystemBalance();

        [OperationContract]
        ActionResult SaveCVVNote(ulong productId, string CVVNote);

        [OperationContract]
        bool ValidateProductId(ulong productId, ProductType productType);

        [OperationContract]
        bool ValidateDocId(long docId);

        [OperationContract]
        VirtualCardDetails GetVirtualCardDetails(string cardNumber, ulong customerNumber);

        [OperationContract]
        bool ValidateAccountNumber(string accountnumber);

        [OperationContract]
        string GetCVVNote(ulong productId);

        [OperationContract]
        bool ValidateCardNumber(string cardNumber);

        [OperationContract]
        ActionResult ActivateAndOpenProductAccounts(ulong productId, ulong customerNumber);

        [OperationContract]
        ActionResult SaveAccountReOpenOrder(AccountReOpenOrder order);

        [OperationContract]
        IBankingHomePage GetIBankingHomePage();

        [OperationContract]
        List<EmployeeSalary> GetEmployeeSalaryList(DateTime startDate, DateTime endDate);

        [OperationContract]
        EmployeeSalaryDetails GetEmployeeSalaryDetails(int ID);

        [OperationContract]
        bool IsPOSAccount(string accountNumber);

        [OperationContract]
        EmployeePersonalDetails GetEmployeePersonalDetails();

        [OperationContract]
        byte[] GetLoansDramContract(long docId, int productType, bool fromApprove, ulong customerNumber);

        [OperationContract]
        byte[] PrintDepositLoanContract(long docId, ulong customerNumber, bool fromApprove = false);

        [OperationContract]
        ActionResult SaveBudgetPaymentOrderTemplate(BudgetPaymentOrderTemplate template);

        [OperationContract]
        List<Card> GetCardsForNewCreditLine(OrderType orderType);

        [OperationContract]
        bool IsEmployee(ulong customerNumber);

        [OperationContract]
        ActionResult SaveCurrencyExchangeOrderTemplate(CurrencyExchangeOrderTemplate template);

        [OperationContract]
        bool HasUploadedContract(string accountNumber, byte type);

        [OperationContract]
        OrderAttachment GetMessageAttachmentById(int Id);

        [OperationContract]
        DigitalAccountRestConfigurations GetCustomerAccountRestConfig(int DigitalUserId);

        [OperationContract]
        ActionResult UpdateCustomerAccountRestConfig(List<DigitalAccountRestConfigurationItem> ConfigurationItems);

        [OperationContract]
        ActionResult ResetCustomerAccountRestConfig(int DigitalUserId);

        [OperationContract]
        ActionResult RejectOrder(OrderRejection rejection);

        [OperationContract]
        string GetOrderAttachmentInBase64(string attachememntId);

        [OperationContract]
        long GetCardProductId(string cardNumber, ulong customerNumber);

        [OperationContract]
        ActionResult MigrateOldUserToCas(int hbUserId);

        [OperationContract]
        List<string> GetTempTokenList(int tokenCount);

        [OperationContract]
        ActionResult ResetIncompletePreOrderDetailQuality();

        [OperationContract]
        int GetIncompletePreOrdersCount();

        [OperationContract]
        bool HasPermissionForDelete(short setNumber);

        [OperationContract]
        void DeleteInsurance(long insuranceId);

        [OperationContract]
        CardAccountRemovalOrder GetCardAccountRemovalOrder(long ID);

        [OperationContract]
        VivaCellBTFCheckDetails CheckVivaCellTransferPossibility(string transferNote, double amount);

        [OperationContract]
        string GetArrestTypesList();

        [OperationContract]
        string GetArrestsReasonTypesList();

        [OperationContract]
        string PostNewAddedCustomerArrestInfo(CustomerArrestInfo obj);

        [OperationContract]
        string RemoveCustomerArrestInfo(CustomerArrestInfo obj);

        [OperationContract]
        string GetCustomerArrestsInfo(ulong customerNumber);

        [OperationContract]
        ulong GetCustomerNumberForArrests();

        [OperationContract]
        string GetSetNumberInfo(UserInfoForArrests obj);

        [OperationContract]
        CheckCustomerArrests GetCustomerHasArrests(ulong customerNumber);

        [OperationContract]
        ActionResult ConfirmOrderOnline(long id);

        [OperationContract]
        Dictionary<string, string> SerchGasPromForReport(string abonentNumber, string branchCode);

        [OperationContract]
        ActionResult ChangeProblemLoanTaxQuality(ulong taxAppId);
        //HB
        [OperationContract]
        string ConfirmReestrTransaction(long docId, int bankCode, short setNumber);

        [OperationContract]
        List<string> GetTreansactionConfirmationDetails(long docId, long debitAccount);

        [OperationContract]
        List<ReestrTransferAdditionalDetails> CheckHBReestrTransferAdditionalDetails(long orderId, List<ReestrTransferAdditionalDetails> details);

        [OperationContract]
        List<HBDocuments> GetSearchedHBDocuments(HBDocumentFilters obj);

        [OperationContract]
        List<HBDocuments> GetHBDocumentsList(HBDocumentFilters obj);
        [OperationContract]
        HBDocumentTransactionError GetTransactionErrorDetails(long transctionCode);

        [OperationContract]
        List<HBDocumentConfirmationHistory> GetConfirmationHistoryDetails(long transctionCode);

        [OperationContract]
        string GetCheckingProductAccordance(long transctionCode);

        [OperationContract]
        HBDocumentConfirmationHistory GetProductAccordanceDetails(long transctionCode);

        [OperationContract]
        bool SetHBDocumentAutomatConfirmationSign(HBDocumentFilters obj);

        [OperationContract]
        bool ExcludeCardAccountTransactions(HBDocumentFilters obj);

        [OperationContract]
        bool SelectOrRemoveFromAutomaticExecution(HBDocumentFilters obj);

        [OperationContract]
        string GetHBArCaBalancePermission(long transctionCode, long accountGroup);

        [OperationContract]
        string GetHBAccountNumber(string cardNumber);

        [OperationContract]
        string ConfirmTransactionReject(HBDocuments documents);

        [OperationContract]
        bool FormulateAllHBDocuments(HBDocumentFilters obj);

        [OperationContract]
        string ChangeTransactionQuality(long transctionCode);

        //[OperationContract]
        //string CheckTransactionQualityChangability(int transctionCode);

        [OperationContract]
        string ChangeAutomatedConfirmTime(List<string> info);

        [OperationContract]
        string GetAutomatedConfirmTime();

        [OperationContract]
        bool SaveInternationalPaymentAddresses(InternationalPaymentOrder order);

        [OperationContract]
        List<HBMessages> GetHBMessages();

        [OperationContract]
        List<HBMessages> GetSearchedHBMessages(HBMessagesSreach obj);

        [OperationContract]
        string PostMessageAsRead(long msgId, int set_number);

        [OperationContract]
        string PostSentMessageToCustomer(HBMessages obj);

        [OperationContract]
        List<HBMessageFiles> GetMessageUploadedFilesList(long msgId);

        [OperationContract]
        int GetCancelTransactionDetails(long docId);

        [OperationContract]
        List<ReestrTransferAdditionalDetails> GetTransactionIsChecked(long orderId, List<ReestrTransferAdditionalDetails> details);

        [OperationContract]
        bool GetReestrFromHB(HBDocuments obj);

        [OperationContract]
        ActionResult SaveAndApprovePaymentToARCAOrder(PaymentToARCAOrder order);

        [OperationContract]
        ActionResult DownloadOrderXMLs(DateTime DateTo, DateTime DateFrom);

        [OperationContract]
        OrderQuality GetOrderQualityByDocID(long docID);

        //HB
        [OperationContract]
        string PostBypassHistory(HBDocumentBypassTransaction obj);

        [OperationContract]
        string PostApproveUnconfirmedOrder(long docId, int setNumber);

        [OperationContract]
        HBDocuments GetCustomerAccountAndInfoDetails(HBDocuments obj);

        [OperationContract]
        HBMessageFiles GetMsgSelectedFile(int fileId);

        [OperationContract]
        string GetcheckedReestrTransferDetails(long docId);

        [OperationContract]
        bool CheckReestrTransactionIsChecked(long docId);

        [OperationContract]
        void PostReestrPaymentDetails(ReestrTransferOrder order);

        [OperationContract]
        string GetSwiftMessage940Statement(DateTime dateFrom, DateTime dateTo, string accountNumber, SourceType source);
        [OperationContract]
        ActionResult SaveAndApproveVirtualCardStatusChangeOrder(VirtualCardStatusChangeOrder order);
        [OperationContract]
        VirtualCardStatusChangeOrder GetVirtualCardStatusChangeOrder(long orderId);

        [OperationContract]
        string CreateLogonTicket(string userSessionToken);

        [OperationContract]
        void SaveDAHKPaymentType(long orderId, int paymentType, int setNumber);

        [OperationContract]
        string GetcheckedArmTransferDetails(long docId);

        [OperationContract]

        Tuple<string, string> GetSintAccountForHB(string accountNumber);

        [OperationContract]
        ulong GetCardProductIdByAccountNumber(string cardAccountNumber, ulong customerNumber);

        [OperationContract]
        List<Card> GetClosedCardsForDigitalBanking(ulong customerNumber);

        [OperationContract]
        int GetCardSystem(string cardNumber);

        [OperationContract]
        List<Card> GetNotActivatedVirtualCards(ulong customerNumber);

        [OperationContract]
        string GetCardNumber(long productId);

        [OperationContract]
        string GetLiabilitiesAccountNumberByAppId(ulong appId);

        [OperationContract]
        string GetStatement(string cardAccount, DateTime dateFrom, DateTime dateTo, byte option);

        [OperationContract]
        InternationalOrderPrefilledData GetInternationalOrderPrefilledData(ulong customerNumber);

        [OperationContract]
        string GetOrderRejectReason(long orderId, OrderType type);


        [OperationContract]
        ActionResult ValidateAttachCard(string cardNumber, ulong customerNumber, string cardHolderName);

        [OperationContract]
        bool IsAbleToApplyForLoan(LoanProductType type);

        [OperationContract]
        List<Account> GetAccountsDigitalBanking();

        [OperationContract]
        double GetMaxAvailableAmountForNewCreditLine(double productId, int creditLineType, string provisionCurrency, bool existRequiredEntries, ulong customerNumber);

        [OperationContract]
        void PostResetEarlyRepaymentFee(ulong productId, string description, bool recovery);

        [OperationContract]
        bool GetResetEarlyRepaymentFeePermission(ulong productId);

        [OperationContract]
        bool IsLoan_24_7(ulong productId);

        [OperationContract]
        List<OrderForCashRegister> GetOrdersForCashRegister(SearchOrders searchOrders);

        [OperationContract]
        ActionResult ReSendVirtualCardRequest(int requestId);

        [OperationContract]
        List<float> GetPlasticCardSMSServiceTariff(ulong productID);

        [OperationContract]
        double GetMaxAvailableAmountForNewLoan(string provisionCurrency, ulong customerNumber);

        [OperationContract]
        ActionResult SaveAndApproveLoanDelayOrder(LoanDelayOrder order);

        [OperationContract]
        LoanDelayOrder GetLoanDelayOrder(long ID);

        [OperationContract]
        LoanRepaymentDelayDetails GetLoanRepaymentDelayDetails(ulong productId);

        [OperationContract]
        double GetUserTotalAvailableBalance(int digitalUserId, ulong customerNumber, int digitalUserID);

        [OperationContract]
        byte[] PrintDepositContract(long orderId, bool attachedFile);

        [OperationContract]
        xbs.Customer GetCustomer(ulong customerNumber);

        [OperationContract]
        Dictionary<string, string> GetOrderDetailsForReport(long orderId);

        [OperationContract]
        RemittanceDetailsRequestResponse GetRemittanceDetailsByURN(string URN, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult SaveAndApproveRemittanceCancellationOrder(RemittanceCancellationOrder order, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult SaveRemittanceCancellationOrder(RemittanceCancellationOrder order, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult ApproveRemittanceCancellationOrder(long orderId, string authorizedUserSessionToken);

        [OperationContract]
        RemittanceCancellationOrder GetRemittanceCancellationOrder(long id, string authorizedUserSessionToken);

        [OperationContract]
        RemittanceFeeDataRequestResponse GetRemittanceFeeData(RemittanceFeeInput feeInput, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult SaveFastTransferOrder(FastTransferPaymentOrder order);

        [OperationContract]
        ActionResult ApproveFastTransferOrder(long orderId, string authorizedUserSessionToken);

        [OperationContract]
        RemittanceAmendmentOrder GetRemittanceAmendmentOrder(long id);

        [OperationContract]
        ActionResult SaveRemittanceAmendmentOrder(RemittanceAmendmentOrder order, string authorizedUserSessionToken);

        [OperationContract]
        ActionResult ApproveRemittanceAmendmentOrder(long orderId, string authorizedUserSessionToken);

        //[OperationContract]
        //R2ARequestOutput SaveAndApproveSTAKPaymentOrder(R2ARequest r2ARequest, SourceType source);

        [OperationContract]
        bool IsDebetExportAndImportCreditLine(string debAccountNumber);

        [OperationContract]
        double GetOrderServiceFeeByIndex(int indexID);

        [OperationContract]
        int GetCustomerTemplatesCounts(ulong customerNumber);

        [OperationContract]
        int GetPeriodicTransfersCount(ulong customerNumber, PeriodicTransferTypes transferType);

        [OperationContract]
        ActionResult SaveAndApproveCancelLoanDelayOrder(CancelDelayOrder order);

        [OperationContract]
        CancelDelayOrder GetCancelLoanDelayOrder(long ID);
        [OperationContract]
        ActionResult SaveCardToOtherCardsOrder(CardToOtherCardsOrder order);

        [OperationContract]
        ActionResult ApproveCardToOtherCardsOrder(CardToOtherCardsOrder cardToCardOrder);

        [OperationContract]
        CardToOtherCardsOrder GetCardToOtherCardsOrder(long ID);

        [OperationContract]
        bool CheckAccountIsClosed(string accountNumber);

        [OperationContract]
        int GetContactsCount();

        [OperationContract]
        string GetLoanAccountNumber(ulong productId);

        [OperationContract]
        bool CheckCardIsClosed(string cardNumber);

        [OperationContract]
        ActionResult CheckForCurrencyExchangeOrderTransactionLimit(CurrencyExchangeOrder order);

        [OperationContract]
        double GetCardToOtherCardTransferFee(double amount, string currency);

        [OperationContract]
        NonCreditLineCardReplaceOrder GetNonCreditLineCardReplaceOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveNonCreditLineCardReplaceOrder(NonCreditLineCardReplaceOrder order);

        [OperationContract]
        CreditLineCardReplaceOrder GetCreditLineCardReplaceOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveCreditLineCardReplaceOrder(CreditLineCardReplaceOrder order);

        [OperationContract]
        ReplacedCardAccountRegOrder GetReplacedCardAccountRegOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveReplacedCardAccountRegOrder(ReplacedCardAccountRegOrder order);

        [OperationContract]
        int GetCardArCaStatus(ulong productId);

        [OperationContract]
        List<PlasticCard> GetCustomerPlasticCardsForAdditionalData(bool IsClosed);

        [OperationContract]
        List<CardAdditionalData> GetCardAdditionalDatas(string cardnumber, string expirydate);

        [OperationContract]
        ActionResult SaveCardAdditionalDataOrder(CardAdditionalDataOrder AdditionalDataOrder);

        [OperationContract]
        CardAdditionalDataOrder GetCardAdditionalDataOrder(long orderID);

        [OperationContract]
        PINRegenerationOrder GetPINRegenerationOrder(long ID);
        [OperationContract]
        ActionResult SaveAndApprovePINRegOrder(PINRegenerationOrder order);

        [OperationContract]
        CardStatementAddInf GetFullCardStatement(CardStatement statement, string cardnumber, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        string GetCurrentAccountContractBefore(long docId, int attacheDocType = 0);

        [OperationContract]
        bool IsCreditLineActivateOnApiGate(long orderId);

        [OperationContract]
        string GetPreviousBlockUnblockOrderComment(string cardNumber);

        [OperationContract]
        string GetCardTechnology(ulong productId);
        [OperationContract]
        ChangeBranchOrder GetChangeBranchOrder(long ID);
        [OperationContract]
        ChangeBranchOrder GetFilialCode(long curdNumber);
        [OperationContract]
        ActionResult SaveAndApproveChangeBranchOrder(ChangeBranchOrder order);

        [OperationContract]
        int GetConfirmRequiredOrdersCount(string userName, string groups = "");

        [OperationContract]
        DateTime GetTransferSentDateTime(int docID);

        [OperationContract]
        byte[] PrintSwiftCopyOrderFile(long docID);

        [OperationContract]
        string GetCustomerHVHH(ulong customerNumber);

        [OperationContract]
        CardNotRenewOrder GetCardNotRenewOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveCardNotRenewOrder(CardNotRenewOrder order);

        [OperationContract]
        decimal? GetUcomFixAbonentBalance(string abonentNumber);

        [OperationContract]
        string GetCardHolderFullName(ulong customerNumber);


        [OperationContract]
        ActionResult SaveCardAccountClosingOrder(CardAccountClosingOrder CardAccountClosingOrder);

        [OperationContract]
        CardAccountClosingOrder GetCardAccountClosingOrder(long orderID);

        //------9316
        [OperationContract]
        long GetLeasingCustomerNumber(int leasingCustomerNumber);

        [OperationContract]
        LeasingCustomerClassification GetLeasingCustomerInfo(long customerNumber);

        [OperationContract]
        List<LeasingCustomerClassification> GetLeasingCustomerSubjectiveClassificationGrounds(long customerNumber, bool isActive);

        [OperationContract]
        Dictionary<string, string> GetLeasingReasonTypes(short classificationType);

        [OperationContract]
        Tuple<int, string> GetLeasingRiskDaysCountAndName(byte riskClassCode);

        [OperationContract]
        ActionResult AddLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj);

        [OperationContract]
        LeasingCustomerClassification GetLeasingCustomerSubjectiveClassificationGroundsByID(int id);

        [OperationContract]
        ActionResult CloseLeasingCustomerSubjectiveClassificationGrounds(long Id);

        [OperationContract]
        List<LeasingCustomerClassification> GetLeasingConnectionGroundsForNotClassifyingWithCustomer(long customerNumber, byte isActive);

        [OperationContract]
        List<KeyValuePair<string, string>> GetLeasingInterconnectedPersonNumber(long customerNumber);

        [OperationContract]
        ActionResult AddLeasingConnectionGroundsForNotClassifyingWithCustomer(LeasingCustomerClassification obj);

        [OperationContract]
        LeasingCustomerClassification GetLeasingConnectionGroundsForNotClassifyingWithCustomerByID(int id);

        [OperationContract]
        ActionResult CloseLeasingConnectionGroundsForNotClassifyingWithCustomer(string docNumber, DateTime docDate, int id);

        [OperationContract]
        List<LeasingCustomerClassification> GetLeasingConnectionGroundsForClassifyingWithCustomer(long customerNumber, byte isActive);

        [OperationContract]
        ActionResult AddOrCloseLeasingConnectionGroundsForClassifyingWithCustomer(LeasingCustomerClassification obj, byte addORClose);
        [OperationContract]
        LeasingCustomerClassification GetLeasingConnectionGroundsForClassifyingWithCustomerByID(int id, long customerNumber);

        [OperationContract]
        List<LeasingCustomerClassification> GetLeasingCustomerClassificationHistory(int leasingCustomerNumber, DateTime date);

        [OperationContract]
        LeasingCustomerClassification GetLeasingCustomerClassificationHistoryByID(int id, long loanFullNumber, int lpNumber);

        [OperationContract]
        bool LeasingCustomerConnectionResult(int customerNumberN1, int customerNumberN2);

        [OperationContract]
        List<LeasingCustomerClassification> GetLeasingGroundsForNotClassifyingCustomerLoan(int leasingCustomerNumber, byte isActive);

        [OperationContract]
        Dictionary<string, string> GetLeasingLoanInfo(int leasingCustNamber);

        [OperationContract]
        ActionResult AddLeasingGroundsForNotClassifyingCustomerLoan(LeasingCustomerClassification obj);

        [OperationContract]
        LeasingCustomerClassification GetLeasingGroundsForNotClassifyingCustomerLoanByID(int id);

        [OperationContract]
        ActionResult CloseLeasingGroundsForNotClassifyingCustomerLoan(long appId, int id, string docNumber, DateTime docDate);
        [OperationContract]
        ActionResult EditLeasingCustomerSubjectiveClassificationGrounds(LeasingCustomerClassification obj);
        [OperationContract]
        LeasingCustomerClassification GetLeasingSubjectiveClassificationGroundsByIDForEdit(int id);

        [OperationContract]
        ActionResult SaveAttachedCardOrderInHB(CardToCardOrder cardToCardOrder);

        [OperationContract]
        List<CardDataChangeOrder> GetCardDataChangesByProduct(long ProductAppId, short FieldType);

        [OperationContract]
        ActionResult DeclineAttachedCardToCardOrderQuality(long docId);

        [OperationContract]
        ActionResult ApproveAttachedCardToCardOrderQuality(CardToCardOrder cardToCardOrder);
    
        [OperationContract]
        bool IsCustomerConnectedToOurBank(ulong customerNumber);
        

        [OperationContract]
        ActionResult SaveAndApproveLoanInterestRateConcessionOrder(LoanInterestRateConcessionOrder order);

        [OperationContract]
        LoanInterestRateConcessionOrder GetLoanInterestRateConcessionDetails(ulong productId);

        [OperationContract]
        double GetLoanOrderAcknowledgement(long docId);

        [OperationContract]
        PensionPaymentOrder GetPensionPaymentOrderDetails(uint id);

        [OperationContract]
        ActionResult SavePensionPaymentOrder(PensionPaymentOrder order);

        [OperationContract]
        List<PensionPaymentOrder> GetAllPensionPayment(string socialCardNumber);

        [OperationContract]
        ActionResult SaveCardLessCashOutOrder(CardlessCashoutOrder order);


        [OperationContract]
        ContentResult<string> ApproveCardLessCashOutOrder(CardlessCashoutOrder order);

        [OperationContract]
        CardlessCashoutOrder GetCardLessCashOutOrder(long id);

        [OperationContract]
        string GetNotFreezedCurrentAccount(ulong customerNumber);

        [OperationContract]
        List<PlasticCard> GetCustomerMainCardsForAttachedCardOrder();

        [OperationContract]
        List<DahkDetails> GetDahkDetailsForDigital(ulong customerNumber);


        [OperationContract]
        LoanInterestRateConcessionOrder GetLoanInterestRateConcessionOrder(long OrderId);

        [OperationContract]
        (bool, CardlessCashoutOrder) GetCardlessCashoutOrderWithVerificationForNCR(string otp);

        [OperationContract]
        ActionResult CardlessCashOutOrderConfirm(ulong docID, string TransactionId, string AtmId);

        [OperationContract]
        List<MTOListAndBestChoiceOutput> GetSTAKMTOListAndBestChoice(MTOListAndBestChoiceInput bestChoice, string authorizedUserSessionToken);

        [OperationContract]
        int GetRemittanceAmendmentCount(ulong id);

        [OperationContract]
        Dictionary<string, string> GetRemittanceContractDetails(ulong docId, string authorizedUserSessionToken);
        [OperationContract]
        LeasingDetailedInformation GetLeasingDetailedInformation(long loanFullName, DateTime dateOfBeginning);

        [OperationContract]
        List<LeasingInsuranceDetails> GetLeasingInsuranceInformation(long loanFullName, DateTime dateOfBeginning);

        [OperationContract]
        double GetPartlyMatureAmount(string contractNumber);
        [OperationContract]
        List<PlasticCardSMSServiceHistory> GetPlasticCardAllSMSServiceHistory(ulong cardNumber);

        [OperationContract]
        double GetCurrencyExchangeOrderFee(CurrencyExchangeOrder order, int feeType);

        [OperationContract]
        DateTime GetLeasingOperDayForStatements();

        [OperationContract]
        List<long> GetAttachedCardOrdersByDocId(List<int> docIds);
        [OperationContract]
        List<Template> GetGroupTemplates(int groupId, TemplateStatus status);
        [OperationContract]
        byte CardBlockingActionAvailability(string cardNumber);

        [OperationContract]
        ShowNewDahk ShowDAHKMessage();

        [OperationContract]
        void MakeDAHKMessageRead(List<string> inquestCodes);

        [OperationContract]
        ActionResult ResponseConfirmForSTAK(STAKResponseConfirm responseConfirm, string authorizedUserSessionToken);
        
        [OperationContract]
        DateTime GetLeasingOperDay();

        [OperationContract]
        ActionResult SaveBillSplitOrder(BillSplitOrder order);

        [OperationContract]
        ActionResult SaveLinkPaymentOrder(LinkPaymentOrder order);

        [OperationContract]
        LinkPaymentOrder GetLinkPaymentOrder(long docId);

        [OperationContract]
        ContentResult<string> ApproveLinkPaymentOrder(LinkPaymentOrder order);

        [OperationContract]
        LinkPaymentOrder GetLinkPaymentOrderWithShortId(string ShortId);

        [OperationContract]
        ActionResult SaveLinkPaymentPayer(LinkPaymentPayer linkPayment);

        [OperationContract]
        BillSplitOrder GetBillSplitOrder(long id);
        [OperationContract]
        ContentResult<List<BillSplitLinkResult>> ApproveBillSplitOrder(BillSplitOrder order);

        [OperationContract]
        ActionResult SaveAndApproveBillSplitSenderRejectionOrder(BillSplitSenderRejectionOrder order);

        [OperationContract]
        BillSplitSenderRejectionOrder GetBillSplitSenderRejectionOrder(long ID);

        [OperationContract]
        List<SentBillSplitRequest> GetSentBillSplitRequests();

        [OperationContract]
        List<ReceivedBillSplitRequest> GetReceivedBillSplitRequests();

        [OperationContract]
        SentBillSplitRequest GetSentBillSplitRequest(long orderId);

        [OperationContract]
        ActionResult SaveAndApproveBillSplitReminderOrder(BillSplitReminderOrder order);

        [OperationContract]
        BillSplitReminderOrder GetBillSplitReminderOrder(long ID);

        [OperationContract]
        ReceivedBillSplitRequest GetReceivedBillSplitRequest(int billSplitSenderId);

        [OperationContract]
        void WriteCardlessCashoutLog(ulong docID, bool isOk, string msgArm, string msgEng, string AtmId, byte step);

        [OperationContract]
        CardRenewOrder GetCardRenewOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveCardRenewOrder(CardRenewOrder order);

        [OperationContract]
        List<string> CheckCardRenewOrder(CardRenewOrder order);

        [OperationContract]
        ActionResult SaveAndApproveRenewedCardAccountRegOrder(RenewedCardAccountRegOrder order);

        [OperationContract]
        RenewedCardAccountRegOrder GetRenewedCardAccountRegOrder(long ID);

        [OperationContract]
        string GetCardHolderData(ulong productId, string dataType);

        [OperationContract]
        string GetPhoneForCardRenew(long productId);

        [OperationContract]
        List<CardRetainHistory> GetCardRetainHistory(string cardNumber);

        [OperationContract]
        ActionResult SaveAndApproveLoanDeleteOrder(DeleteLoanOrder deleteLoanOrder);

        [OperationContract]
        DeleteLoanOrderDetails GetLoanDeleteOrderDetails(uint orderId);
        [OperationContract]
        ActionResult RemoveAccountOrder(AccountRemovingOrder order);
        [OperationContract]
        ActionResult SaveAndApproveAccountRemoving(AccountRemovingOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCardReOpenOrder(CardReOpenOrder order);

        [OperationContract]
        Dictionary<string, string> GetCardReOpenReason();

        [OperationContract]
        CardReOpenOrder GetCardReOpenOrder(long ID);

        [OperationContract]
        CardlessCashoutOrder GetCardlessCashoutOrderWithVerification(string cardlessCashOutCode);

        [OperationContract]
        ActionResult SavePreferredAccountOrder(PreferredAccountOrder order);

        [OperationContract]
        ActionResult ApprovePreferredAccountOrder(long id);

        [OperationContract]
        PreferredAccount GetSelectedOrDefaultPreferredAccountNumber(PreferredAccountServiceTypes serviceType, ulong customerNumber);

        [OperationContract]
        PreferredAccountOrder GetPreferredAccountOrder(long id);

        [OperationContract]
        bool IsDisabledPreferredAccountService(ulong customerNumber, PreferredAccountServiceTypes preferredAccountServiceType);

        [OperationContract]
        ActionResult SaveAccountQrCode(string accountNumber, string guid, ulong customerNumber);

        [OperationContract]
        QrTransfer SearchAccountByQrCode(string guid);

        [OperationContract]
        string GetAccountQrCodeGuid(string accountNumber);

        [OperationContract]
        List<Account> GetQrAccounts();

        [OperationContract]
        bool IsCardlessCashCodeCorrect(string cardlessCashoutCode);

        [OperationContract]
        ActionResult SaveAndApproveVisaAliasOrder(VisaAliasOrder order);

        [OperationContract]
        VisaAliasOrder VisaAliasOrderDetails(long orderId);

        [OperationContract]
        ActionResult SaveVisaAliasOrder(VisaAliasOrder order);

        [OperationContract]
        ActionResult ApproveVisaAliasOrder(VisaAliasOrder order);

        [OperationContract]
        VisaAliasOrder GetVisaAliasOrder(long ID);

        [OperationContract]
        string GetVisaAliasGuidByCutomerNumber(ulong customerNumber);

        [OperationContract]
        CardHolderAndCardType GetCardTypeAndCardHolder(string cardNumber);
        [OperationContract]
        ulong CheckCustomerFreeFunds(string accountNumber);

        [OperationContract]
        ActionResult SaveAndApproveThirdPersonAccountRightsTransfer(ThirdPersonAccountRightsTransferOrder order);

        [OperationContract]
        bool GetRightsTransferAvailability(string accountNumber);

        [OperationContract]
        bool GetRightsTransferVisibility(string accountNumber);
        [OperationContract]
        bool GetCheckCustomerIsThirdPerson(string accountNumber, ulong customerNumber);

        [OperationContract]
        List<string> GetRenewedCardAccountRegWarnings(Card oldCard);

        [OperationContract]
        bool GetMRDataChangeAvailability(int mrID);

        [OperationContract]
        ActionResult SaveAndApproveMRDataChangeOrder(MRDataChangeOrder order);
        
        [OperationContract]
        List<CustomerLeasingLoans> GetHBLeasingLoans(ulong customerNumber);

        [OperationContract]
        LeasingLoanDetails GetHBLeasingLoanDetails(ulong productId);

        [OperationContract]
        List<LeasingLoanRepayments> GetHBLeasingLoanRepayments(ulong productId);

        [OperationContract]
        List<LeasingLoanStatements> GetHBLeasingLoanStatements(ulong productId, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, int pageNumber = 1, int pageRowCount = 15, short orderByAscDesc = 0);

        [OperationContract]
        List<AdditionalDetails> GetHBLeasingDetailsByAppID(ulong productId, int leasingInsuranceId = 0);

        [OperationContract]
        List<LeasingPaymentsType> GetHBLeasingPaymentsType();

        [OperationContract]
        Account SetHBLeasingReceiver();

        [OperationContract]
        string GetHBLeasingPaymentDescription(short paymentType, short paymentSubType);

        [OperationContract]
        LeasingLoanRepayments GetHBLeasingPaymentDetails(ulong productId);

    }
}
