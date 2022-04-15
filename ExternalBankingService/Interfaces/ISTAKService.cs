using ExternalBanking.ARUSDataService;
using ExternalBankingService.InfSecServiceReference;
using System.ServiceModel;

namespace ExternalBankingService.Interfaces
{
    [ServiceContract]
    [ServiceKnownType(typeof(R2ARequestOutput))]
    [ServiceKnownType(typeof(R2ARequest))]
    [ServiceKnownType(typeof(AuthorizedUser))]
    [ServiceKnownType(typeof(LoginInfo))]

    public interface ISTAKService
    {

        [OperationContract]
        R2ARequestOutput SaveAndApproveSTAKPaymentOrder(R2ARequest r2ARequest);

        [OperationContract]
        AuthorizedUser AuthorizeUser(LoginInfo loginInfo);

    }

}
