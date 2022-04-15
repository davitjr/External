using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class CommunalDetailsRequestResponse
    {
        public List<CommunalDetails> CommunalDetails { get; set; }
        public Result Result { get; set; }

        public CommunalDetailsRequestResponse()
        {
            Result = new Result();
        }

    }
}