using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class LoanProductOrderRequestResponse
    {
        public LoanProductOrder LoanProductOrder { get; set; }

        public Result Result { get; set; }

        public LoanProductOrderRequestResponse()
        {
            Result = new Result();
        }


    }
}