using System.ServiceModel;
using System.ServiceModel.Web;
using System;
using ExternalBankingRESTService.XBS;
using System.Collections.Generic;
using static ExternalBankingRESTService.Enumerations;

namespace ExternalBankingRESTService
{
    [ServiceContract]
    public interface IXBRESTService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "MobileAuthorization", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoginRequestResponse MobileAuthorization();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccount", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AccountRequestResponse GetAccount(string accountNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccounts")]
        AccountsRequestResponse GetAccounts();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCurrentAccounts")]
        AccountsRequestResponse GetCurrentAccounts(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCards")]
        CardsRequestResponse GetCards(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCard", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CardRequestResponse GetCard(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetDeposits")]
        DepositsRequestResponse GetDeposits(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetDeposit", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        DepositRequestResponse GetDeposit(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetLoans")]
        LoansRequestResponse GetLoans(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetLoan", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoanRequestResponse GetLoan(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetPeriodicTransfers")]
        PeriodicTransfersRequestResponse GetPeriodicTransfers(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetPeriodicTransfer", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PeriodicTransferRequestResponse GetPeriodicTransfer(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCardStatement", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CardStatementRequestResponse GetCardStatement(string cardNumber, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccountStatement", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AccountStatementRequestResponse GetAccountStatement(string accountNumber, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetArcaBalance", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ArcaBalanceRequestResponse GetArcaBalance(string cardNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetDraftOrders", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        OrdersRequestResponse GetDraftOrders(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetSentOrders", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        OrdersRequestResponse GetSentOrders(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetApproveReqOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        OrdersRequestResponse GetApproveReqOrder(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetMessages", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        MessagesRequestResponse GetMessages(DateTime dateFrom, DateTime dateTo, short type);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetNumberOfMessages", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        MessagesRequestResponse GetNumberOfMessages(short messagesCount, MessageType type);
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "AddMessage", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result AddMessage(Message message);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "DeleteMessage", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result DeleteMessage(int messageId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "MarkMessageReaded", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result MarkMessageReaded(int messageId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetContact", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ContactRequestResponse GetContact(ulong contactId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetContacts", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ContactsRequestResponse GetContacts();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "AddContact", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result AddContact(Contact contact);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "UpdateContact", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result UpdateContact(Contact contact);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "DeleteContact", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result DeleteContact(ulong contactId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCommunals", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SearchCommunalRequestResponse GetCommunals(SearchCommunal searchCommunal);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCommunalDetails", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CommunalDetailsRequestResponse GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType = AbonentTypes.physical);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SavePaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SavePaymentOrder(PaymentOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveBudgetPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveBudgetPaymentOrder(BudgetPaymentOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PaymentOrderRequestResponse GetPaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetLastExhangeRate", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ExchangeRateRequestResponse GetLastExhangeRate(string currency, byte rateType, byte direction);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetPaymentOrderFutureBalance", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PaymentOrderFutureBalanceRequestResponse GetPaymentOrderFutureBalance(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveUtilityPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveUtilityPaymentOrder(UtilityPaymentOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetUtilityPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        UtilityPaymentOrderRequestResponse GetUtilityPaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetPaymentOrderFee", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PaymentOrderFeeRequestResponse GetPaymentOrderFee(PaymentOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccountsForOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AccountsRequestResponse GetAccountsForOrder(short orderType, byte orderSubType, byte accountType);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ApprovePaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse ApprovePaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "DeletePaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse DeletePaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetUnreadedMessagesCount", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        MessagesCountResponceRequest GetUnreadedMessagesCount();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ApproveUtilityPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse ApproveUtilityPaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetUnreadMessagesCountByType", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        MessagesCountResponceRequest GetUnreadMessagesCountByType(MessageType type);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ValidateOTP", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result ValidateOTP();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ValidateRegistrationCode", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ValidateRegistrationCodeRequestResponse ValidateRegistrationCode();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ChangeMobileUserPassword", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result ChangeMobileUserPassword(string password, string newPassword, string retypeNewPassword);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetLoanRepaymentGrafik", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoanRepaymentResponse GetLoanRepaymentGrafik(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetDepositRepaymentGrafik", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        DepositRepaymentResponse GetDepositRepaymentGrafik(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveMatureOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveMatureOrder(MatureOrder order);
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ApproveMatureOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse ApproveMatureOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetMatureOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        MatureOrderRequestResponse GetMatureOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveReferenceOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveReferenceOrder(ReferenceOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetReferenceOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ReferenceOrderRequestResponse GetReferenceOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ApproveReferenceOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse ApproveReferenceOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetEmbassyList", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetEmbassyList();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetFilialList", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetFilialList();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetReferenceLanguages", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetReferenceLanguages();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetReferenceTypes", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetReferenceTypes();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetReferenceOrderFee", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PaymentOrderFeeRequestResponse GetReferenceOrderFee(bool UrgentSign);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCardMembershipRewards", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        MembershipRewardsRequestResponse GetCardMembershipRewards(string cardNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetGuarantees")]
        GuaranteesRequestResponse GetGuarantees(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetGuarantee", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        GuaranteeRequestResponse GetGuarantee(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccreditives")]
        AccreditivesRequestResponse GetAccreditives(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccreditive", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AccreditiveRequestResponse GetAccreditive(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetDepositCases")]
        DepositCasesRequestResponse GetDepositCases(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetDepositCase", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        DepositCaseRequestResponse GetDepositCase(ulong productId);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetPeriodicTransferHistory", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PeriodicTransferHistoryRequestResponse GetPeriodicTransferHistory(long productId, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetFactorings")]
        FactoringsRequestResponse GetFactorings(ProductQualityFilter filter = ProductQualityFilter.Opened);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetFactoring", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        FactoringRequestResponse GetFactoring(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCBKursForDate", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ExchangeRateRequestResponse GetCBKursForDate(string currency, DateTime date);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "CheckAuthorization", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        Result CheckAuthorization();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetLoanMatureCapitalPenalty", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoanMatureCapitalPenaltyRequestResponse GetLoanMatureCapitalPenalty(MatureOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveAndApproveRemovalOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveAndApproveRemovalOrder(RemovalOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetRemovalOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        RemovalOrderRequestResponse GetRemovalOrder(long id);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetVehicleViolationById", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        VehicleViolationRequestRespone GetVehicleViolationById(string violationId);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetVehicleViolationByPsnVehNum", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        VehicleViolationRequestRespone GetVehicleViolationByPsnVehNum(string psn, string vehNum);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetBudgetPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        BudgetPaymentOrderRequestResponse GetBudgetPaymentOrder(long id);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCreditLine", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CreditLineRequestResponse GetCreditLine(ulong productId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCreditLineGrafik", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CreditLineGrafikRequestResponse GetCreditLineGrafik(ulong productId);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveLoanProductOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveLoanProductOrder(LoanProductOrder order);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ApproveLoanProductOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse ApproveLoanProductOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCreditLineOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoanProductOrderRequestResponse GetCreditLineOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetLoanProductInterestRate", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        LoanProductInterestRateRequestResponse GetLoanProductInterestRate(LoanProductOrder order, string cardNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetListOfLoanApplicationAmounts", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetListOfLoanApplicationAmounts();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetFastOverdraftFeeAmount", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        FastOverdraftFeeAmountRequestResponse GetFastOverdraftFeeAmount(double amount);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCommunicationTypes", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetCommunicationTypes();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCountries", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetCountries();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetRegions", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetRegions(int country);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetArmenianPlaces", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetArmenianPlaces(int region);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetAccountDescription", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AccountDescriptionRequestResponse GetAccountDescription(string accountNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "FastOverdraftValidations", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse FastOverdraftValidations(string cardNumber);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetFastOverdrafStartAndEndDate", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        FastOverdrafStartAndEndDateRequestResponse GetFastOverdrafStartAndEndDate();


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetFastOverdraftContract", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        FastOverdraftContractRequestResponse GetFastOverdraftContract(DateTime startDate, DateTime endDate, string cardNumber, string creditLineAccount, string currency, double amount, double interestRate);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetACRAAgreementText", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ACRAAgreementTextRequestResponse GetACRAAgreementText();


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetReceivedFastTransferPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ReceivedFastTransferPaymentOrderRequestResponse GetReceivedFastTransferPaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetTransferSystemCurrency", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetTransferSystemCurrency(int transfersystem);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetTransferTypes", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InfoListRequestResponse GetFastTransferSystemTypes();


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SaveReceivedFastTransferPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ApproveReceivedFastTransferPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ActionRequestResponse ApproveReceivedFastTransferPaymentOrder(long id);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetReceivedFastTransferOrderRejectReason", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        DescriptionRequestResponse GetReceivedFastTransferOrderRejectReason(int orderId);



        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetOnlineOrderHistory", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        OrderHistoryRequestResponse GetOnlineOrderHistory(int orderId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetConfirmRequiredOrders", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        OrdersRequestResponse GetConfirmRequiredOrders(DateTime startDate, DateTime endDate);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetInternationalPaymentOrder", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        InternationalPaymentOrderRequestResponse GetInternationalPaymentOrder(long id);

        [OperationContract]
        [WebGet(UriTemplate = "ChangeMailVerificationStatus/?emailGuid={emailGuid}&sap-language={lang}")]
        void ChangeMailVerificationStatus(string emailGuid, string lang);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "GetCustomerInfoForAuthentication", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        CustomerInfoForAuthenticationRequestResponse GetCustomerInfoForAuthentication(DocumentType documentType, string documentValue);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "PrintLoanTermsSheet", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ByteArrayRequestResponse PrintLoanTermsSheet(byte loanType, long orderid);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "PrintLoanTermsSheetBase64", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        StringRequestResponse PrintLoanTermsSheetBase64(byte loanType, long orderid);


    }

}
