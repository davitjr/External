using ExternalBanking;
using System.ServiceModel;

namespace ExternalBankingService.Interfaces
{

    [ServiceContract]
    public interface ILoanXBService
    {
        [OperationContract]
        bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        AuthorizedCustomer AuthorizeCustomer(ulong customerNumber);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductActivationOrder(LoanProductActivationOrder order);

        [OperationContract]
        ActionResult ConfirmLoanProductActivationOrder(LoanProductActivationOrder order);

        [OperationContract]
        long? CheckPreviousActivationOrderId(LoanProductActivationOrder order);

        [OperationContract]
        LoanProductActivationOrder GetLoanProductActivationOrder(LoanProductActivationOrder order);
    }
}
