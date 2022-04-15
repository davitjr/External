using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class TransferCallContractRequestResponse
    {
        public TransferCallContract TransferCallContract { get; set; }

        public Result Result { get; set; }

        public TransferCallContractRequestResponse()
        {
            Result = new Result();
        }

    }
}