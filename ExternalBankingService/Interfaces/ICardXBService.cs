using ExternalBanking;
using System.ServiceModel;

namespace ExternalBankingService.Interfaces
{

    [ServiceContract]
    public interface ICardXBService
    {
        [OperationContract]
        bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source, ServiceType serviceType = ServiceType.CustomerService);

        [OperationContract]
        void SetUser(AuthorizedCustomer authorizedCustomer, byte language, string ClientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        AuthorizedCustomer AuthorizeCustomer(ulong customerNumber);

        [OperationContract]
        ActionResult SaveAndApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCardRenew(CardRenewOrder order);

        [OperationContract]
        Card GetCard(ulong productId, ulong customerNumber);
    }
}
