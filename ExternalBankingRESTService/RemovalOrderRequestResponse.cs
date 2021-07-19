using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class RemovalOrderRequestResponse
    {
        public RemovalOrder RemovalOrder { get; set; }

        public Result Result { get; set; }

        public RemovalOrderRequestResponse()
        {
            Result = new Result();
        }
    }
}