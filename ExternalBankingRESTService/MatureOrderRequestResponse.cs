using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class MatureOrderRequestResponse
    {
        public MatureOrder MatureOrder { get; set; }

        public Result Result { get; set; }

        public MatureOrderRequestResponse()
        {
            Result = new Result();
        }
    }
    
    
}