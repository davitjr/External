using ExternalBanking;
using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using acba = ExternalBanking.ACBAServiceReference;

namespace ExternalBankingService.Interfaces
{
    [ServiceContract]
    [ServiceKnownType(typeof(HBUser))]
    [ServiceKnownType(typeof(HBToken))]
    [ServiceKnownType(typeof(PhoneBankingContract))]
    [ServiceKnownType(typeof(PhoneBankingContractQuestionAnswer))]
    [ServiceKnownType(typeof(List<PhoneBankingContractQuestionAnswer>))]
    [ServiceKnownType(typeof(HBActivationOrder))]
    [ServiceKnownType(typeof(OrderAccountType))]

    public interface IXBManagementService
    {


        [OperationContract]
        bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source, ServiceType customerServiceType);

        [OperationContract]
        List<XBUserGroup> GetXBUserGroups();

        [OperationContract]
        List<HBUser> GetHBUsersByGroup(int id);

        [OperationContract]
        ApprovementSchema GetApprovementSchema();

        [OperationContract]
        List<ApprovementSchemaDetails> GetApprovementSchemaDetails(int schemaId);


        [OperationContract]
        string GenerateNextGroupName(ulong customerNumber);

        [OperationContract]
        HBApplication GetHBApplication();
        [OperationContract]
        HBApplication GetHBApplicationShablon();
        [OperationContract]
        HBApplicationOrder GetHBApplicationOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveHBApplicationOrder(HBApplicationOrder order);

        [OperationContract]
        List<HBUser> GetHBUsers(int hbAppID, ProductQualityFilter filter);

        [OperationContract]
        List<HBToken> GetHBTokens(int HBUserID, ProductQualityFilter filter);

        [OperationContract]
        List<string> GetHBTokenNumbers(HBTokenTypes tokenType);

        [OperationContract]
        bool CheckHBUserNameAvailability(HBUser hbUser);

        [OperationContract]
        PhoneBankingContract GetPhoneBankingContract();

        [OperationContract]
        ActionResult SaveAndApprovePhoneBankingContractOrder(PhoneBankingContractOrder order);

        [OperationContract]
        ActionResult SaveAndApproveHBApplicationQualityChangeOrder(HBApplicationQualityChangeOrder order);

        [OperationContract]
        ActionResult SaveAndApprovePhoneBankingContractClosingOrder(PhoneBankingContractClosingOrder order);

        [OperationContract]
        PhoneBankingContractClosingOrder GetPhoneBankingContractClosingOrder(long ID);

        [OperationContract]
        ActionResult CancelTokenNumberReservation(HBToken token);

        [OperationContract]
        PhoneBankingContractOrder GetPhoneBankingContractOrder(long ID);

        [OperationContract]
        ActionResult SaveAndApproveHBServletRequestOrder(HBServletRequestOrder order);

        [OperationContract]
        List<AssigneeCustomer> GetHBAssigneeCustomers(ulong customerNumber);

        [OperationContract]
        HBApplicationQualityChangeOrder GetHBApplicationQualityChangeOrder(long ID);

        [OperationContract]
        HBServletRequestOrder GetHBServletRequestOrder(long ID);

        [OperationContract]
        ActionResult ApproveHBActivationOrder(HBActivationOrder order);

        [OperationContract]
        ActionResult SaveAndApproveHBActivationOrder(HBActivationOrder order);

        [OperationContract]
        ActionResult SaveHBActivationOrder(HBActivationOrder order);

        [OperationContract]
        List<HBActivationRequest> GetHBRequests();

        [OperationContract]
        HBActivationOrder GetHBActivationOrder(long ID);

        [OperationContract]
        Account GetOperationSystemAccount(Order order, OrderAccountType accountType, string operationCurrency, ushort filialCode = 0, string utilityBranch = "", ulong customerNumber = 0);

        [OperationContract]
        Double GetHBServiceFee(DateTime date, HBServiceFeeRequestTypes requestType, HBTokenTypes tokenType, HBTokenSubType tokenSubType);

        [OperationContract]
        String GetHBTokenGID(int hbuserID, HBTokenTypes tokenType);

        [OperationContract]
        ActionResult SaveHBRegistrationCodeResendOrder(HBRegistrationCodeResendOrder order);

        [OperationContract]
        ActionResult SaveAndApproveHBActivationRejectionOrder(HBActivationRejectionOrder order);

        [OperationContract]
        List<HBUserLog> GetHBUserLog(String userName);

        [OperationContract]
        PhoneBankingContractActivationRequest GetPhoneBankingRequests();

        [OperationContract]
        ActionResult SaveAndApprovePhoneBankingContractActivationOrder(PhoneBankingContractActivationOrder order);

        [OperationContract]
        List<string> GetUnusedTokensByFilialAndRange(string from, string to, int filial);

        [OperationContract]
        void MoveUnusedTokens(int filialToMove, List<String> unusedTokens);

        [OperationContract]
        void SetUser(AuthorizedCustomer authorizedCustomer, byte language, string ClientIp, acba.User user, SourceType source);
        [OperationContract]
        List<acba.CustomerEmail> GetCustomerEmails(ulong customerNumber);

        [OperationContract]
        ActionResult GenerateAcbaOnline(string userName, string password, ulong customerNumber, string phoneNumber, int customerQuality, string email);

        [OperationContract]
        ActionResult AddEmailForCustomer(string emailAddress, ulong customerNumber);

        [OperationContract]
        ulong GetHBUserCustomerNumber(string userName);

        [OperationContract]
        AuthorizedCustomer GetXBMTestMobileBankingUser();

        [OperationContract]
        ActionResult SaveAndApproveHBApplicationFullPermissionsGrantingOrder(HBApplicationFullPermissionsGrantingOrder order);

        [OperationContract]
        HBApplicationFullPermissionsGrantingOrder GetHBApplicationFullPermissionsGrantingOrder(long ID);

    }
}


