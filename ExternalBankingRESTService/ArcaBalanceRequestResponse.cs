using System;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class ArcaBalanceRequestResponse
    {
        public KeyValuePair<String, double> ArcaBalance { get; set; }
        public Result Result { get; set; }

        public ArcaBalanceRequestResponse()
        {
            Result = new Result();
        }
    }
}