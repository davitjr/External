using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

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