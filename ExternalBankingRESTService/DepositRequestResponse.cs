using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
	public class DepositRequestResponse
    {
        public Deposit Deposit { get; set; }
        public Result Result { get; set; }

        public DepositRequestResponse()
        {
            Result = new Result();
        }
    }
}