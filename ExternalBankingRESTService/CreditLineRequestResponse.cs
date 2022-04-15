
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class CreditLineRequestResponse
    {
        public CreditLine CreditLine { get; set; }
        public Result Result { get; set; }

        public CreditLineRequestResponse()
        {
            Result = new Result();
        }
    }
}