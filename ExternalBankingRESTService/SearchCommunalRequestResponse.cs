using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class SearchCommunalRequestResponse
    {
        public List<Communal> Communals { get; set; }
        public Result Result { get; set; }

        public SearchCommunalRequestResponse()
        {
            Result = new Result();
        }
    }
}