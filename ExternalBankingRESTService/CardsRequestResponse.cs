using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

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
