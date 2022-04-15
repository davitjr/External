using ExternalBanking;
using ExternalBanking.ACBAServiceReference;
using System.Collections.Generic;
using System.ServiceModel;

namespace ExternalBankingService.Interfaces
{
    [ServiceContract]
    public interface IAcbamatService
    {
        [OperationContract]
        void Init(AuthorizedCustomer authorizedCustomer, string clientIp, User user, byte language, SourceType source);

        [OperationContract]
        List<ExchangeRate> GetExchangeRates();

        [OperationContract]
        List<Communal> CheckMobileNumber(SearchCommunal communal);

        [OperationContract]
        void RegisterExchange(AcbamatExchangeOrder acbamatExchangeOrder);

        [OperationContract]
        void RegisterThirdPartyWithdrawal(AcbamatThirdPartyWithdrawalOrder acbamatThirdPartyWithdrawalOrder);
    }
}