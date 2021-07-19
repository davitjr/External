using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
	public class CardStatementRequestResponse
    {
        public CardStatement CardStatement { get; set; }
        public Result Result { get; set; }

        public CardStatementRequestResponse()
        {
            Result = new Result();
        }
    }
}