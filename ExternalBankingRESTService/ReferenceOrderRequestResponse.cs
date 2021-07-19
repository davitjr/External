using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class ReferenceOrderRequestResponse
    {
        public ReferenceOrder ReferenceOrder { get; set; }
        public Result Result { get; set; }

        public ReferenceOrderRequestResponse()
        {
            Result = new Result();
        }
    }
}