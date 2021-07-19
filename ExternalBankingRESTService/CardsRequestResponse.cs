using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
	public class CardsRequestResponse
    {
        public List<Card> Cards { get; set; }
        public Result Result { get; set; }

       public CardsRequestResponse()
        {
            Result = new Result();
        }

    }
}
