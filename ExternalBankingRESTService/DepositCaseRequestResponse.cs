using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class DepositCaseRequestResponse
    {
        public DepositCase DepositCase { get; set; }
        public Result Result { get; set; }

        public DepositCaseRequestResponse()
        {
            Result = new Result();
        }

    }
}