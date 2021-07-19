using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
	public class CardRequestResponse
    {
        public Card Card { get; set; }
        public Result Result { get; set; }

        public CardRequestResponse()
        {
            Result = new Result();
        }
    }
}
