using ExternalBanking;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using xbs = ExternalBanking.ACBAServiceReference;
using infsec = ExternalBankingService.InfSecServiceReference;

namespace ExternalBankingService.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IACBAOnlineService" in both code and config file together.
    [ServiceContract]
    [ServiceKnownType(typeof(PaymentOrder))]
    [ServiceKnownType(typeof(CredentialOrder))]
    [ServiceKnownType(typeof(OrderAttachment))]
    [ServiceKnownType(typeof(ReceivedFastTransferPaymentOrder))]
    [ServiceKnownType(typeof(OrderHistory))]
    [ServiceKnownType(typeof(DepositOrder))]

    public interface IACBAOnlineService
    {
        [OperationContract]
        string GetStatement(string cardAccount, DateTime dateFrom, DateTime dateTo, byte option);

        [OperationContract]
        CredentialOrder GetCredentialOrder(long ID);



        [OperationContract]

        ActionResult SaveCredentialOrder(CredentialOrder order);


        [OperationContract]
        bool InitAOCustomer(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        AOService.AcbaOnlineUserData AuthorizeAcbaOnlineCustomerByPassword(AOService.LoginInfo loginInfo, byte langId);

        [OperationContract]
        AOService.AcbaOnlineUserData AuthorizeAcbaOnlineCustomerByToken(AOService.LoginInfo loginInfo, AOService.LoginResult lr, byte langId);

        [OperationContract]
        AOService.AcbaOnlineUserData ChangeAcbaOnlineCustomerPassword(AOService.ChangePasswordInfo passwordInfo, string SessionId, byte langId);

        [OperationContract]
        bool IsAbleToChangeQuality(string userName, string userGroups, int id);

        [OperationContract]
        ActionResult ApproveOrder(Order order);

        [OperationContract]
        string GetTerm(short id, string[] param, Languages language);

        [OperationContract]
        double GetLoanProductInterestRate(LoanProductOrder order, string cardNumber);

        [OperationContract]
        ActionResult SaveLoanProductOrder(LoanProductOrder order);

        [OperationContract]
        List<ActionError> FastOverdraftValidations(string cardNumber);

        [OperationContract]
        double GetOrderServiceFee(OrderType type, int urgent);

        [OperationContract]
        PaymentOrder GetPaymentOrder(long id);

        [OperationContract]
        ActionResult ApprovePaymentOrder(PaymentOrder order);

        [OperationContract]
        List<int> GetConfirmRequiredOrders(string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1);

        [OperationContract]
        KeyValuePair<String, double> GetArCaBalance(string cardNumber);

        [OperationContract]
        bool VerifyToken(string SessionId, string OTP, string ipAddress, byte langId);

        [OperationContract]
        bool ResetUserPassword(AOService.LoginInfo loginInfo);

        [OperationContract]
        string GetSwiftMessageStatement(DateTime dateFrom, DateTime dateTo, string accountNumber);

        [OperationContract]
        List<AccountFreezeDetails> GetAccountFreezeHistory(string accountNumber, ushort freezeStatus, ushort reasonId);

        [OperationContract]
        List<Account> GetAccountsForCredential(int operationType);

        [OperationContract]
        double GetServiceProvidedOrderFee(OrderType orderType, ushort serviceType);

        [OperationContract]
        double GetDepositLoanCreditLineAndProfisionCoefficent(string loanCurrency, string provisionCurrency, bool mandatoryPayment, int creditLineType);



        [OperationContract]
        ActionResult SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order);

        [OperationContract]
        ReceivedFastTransferPaymentOrder GetReceivedFastTransferPaymentOrder(long id);
        [OperationContract]
        List<TransferCallContract> GetContractsForTransfersCall(string customerNumber, string accountNumber, string cardNumber);


        [OperationContract]
        List<OrderHistory> GetOrderHistory(long orderId);

        [OperationContract]
        string GetReceivedFastTransferOrderRejectReason(int orderId);

        [OperationContract]
        DepositOrderCondition GetDepositCondition(DepositOrder order);

        [OperationContract]
        ActionResult CheckDepositOrderCondition(DepositOrder order);

        [OperationContract]
        List<Account> GetDecreasingDepositAccountList(ulong customerNumber);

        [OperationContract]
        string GetSwiftMessage940Statement(DateTime dateFrom, DateTime dateTo, string accountNumber);

        [OperationContract]
        Deposit GetDeposit(ulong productId);


        [OperationContract]

        string GetInternationalTransferSentTime(int docId);

        [OperationContract]
        bool HasOrHadAccount(ulong customerNumber);

        [OperationContract]
        List<Communal> GetCommunals(SearchCommunal searchCommunal);
        [OperationContract]
        decimal? GetBeelineAbonentBalance(string abonentNumber);

        [OperationContract]
        List<CommunalDetails> GetCommunalDetails(short communalType, string abonentNumber, short checkType, string branchCode, AbonentTypes abonentType);

        [OperationContract]
        string SearchFullCommunalGasOnline(string abonentNumber, string branchCode, int num = 0);

        [OperationContract]
        OrderQuality GetOrderQualityByDocID(long docID);

        [OperationContract]
        double GetAcccountAvailableBalance(string accountNumber);

        [OperationContract]
        KeyValuePair<string, string> GetCurrentOperDay24_7_Mode();

        [OperationContract]
        bool IsLoan_24_7(ulong productId);

        [OperationContract]
        decimal? GetUcomFixAbonentBalance(string abonentNumber);

    }
}
