using ExternalBanking;
using System.Collections.Generic;
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
        ActionResult SaveAndApproveAutomateArcaCardsTransactionOrder(ArcaCardsTransactionOrder order);

        [OperationContract]
        ActionResult SaveAndApproveCardRenew(CardRenewOrder order);

        [OperationContract]
        Card GetCard(ulong productId, ulong customerNumber);

        [OperationContract]
        PlasticCard GetPlasticCard(ulong productId);

        [OperationContract]
        Card GetCardWithCardNumber(string cardNumber, ulong customerNumber);

        [OperationContract]
        List<Card> GetCards(ulong customerNumber, ProductQualityFilter filter, bool includingAttachedCards);

        [OperationContract]
        List<PlasticCard> GetCustomerPlasticCards(ulong customerNumber);

        [OperationContract]
        List<AccountFreezeDetails> GetAccountFreezeHistory(string accountNumber, ushort freezeStatus, ushort reasonId);

        [OperationContract]
        int[] GetFreezingReasonsForBlocking();

        [OperationContract]
        long? GetPreviousBlockingOrderId(string cardNumber);

        [OperationContract]
        long? GetPreviousUnBlockingOrderId(string cardNumber);
    }
}
