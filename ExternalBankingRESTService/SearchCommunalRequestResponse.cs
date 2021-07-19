using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

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