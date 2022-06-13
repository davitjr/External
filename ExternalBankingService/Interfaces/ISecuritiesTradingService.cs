using ExternalBanking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ExternalBankingService.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISecuritiesTradingService" in both code and config file together.
    [ServiceContract]
    public interface ISecuritiesTradingService
    {

        [OperationContract]
        void SetUser(AuthorizedCustomer authorizedCustomer, byte language, string ClientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        SecuritiesTradingOrder GetSecuritiesTradingOrder(long ID);

        [OperationContract]
        double GetSecuritiesTradingOrderDepositedAmount(SecuritiesTradingOrder order);

        [OperationContract]
        (double, double) GetSecuritiesTradingOrderFee(long OrderId, string ISIN, double DepositedAmount, SharesTypes types, string ListingType, string Currency, int Quantity, short CalculatingType);

        [OperationContract]
        bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source, ServiceType serviceType);

    }
}
