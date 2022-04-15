using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class PeriodicTransferRequestResponse
    {
        public PeriodicTransfer PeriodicTransfer { get; set; }
        public Result Result { get; set; }

        public PeriodicTransferRequestResponse()
        {
            Result = new Result();
        }
    }
}