using System.ServiceModel;

namespace ExternalBankingService.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ILeasingXBService" in both code and config file together.
    [ServiceContract]
    public interface ILeasingXBService
    {
        [OperationContract]
        string GetAccountsForLeasing(ulong CustomerNumber);
    }
}