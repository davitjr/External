using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class MatureOrderRequestResponse
    {
        public MatureOrder MatureOrder { get; set; }

        public Result Result { get; set; }

        public MatureOrderRequestResponse()
        {
            Result = new Result();
        }
    }


}