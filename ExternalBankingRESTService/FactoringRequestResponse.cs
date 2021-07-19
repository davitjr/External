using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class FactoringRequestResponse
    {
        public Factoring Factoring { get; set; }
        public Result Result { get; set; }

        public FactoringRequestResponse()
        {
            Result = new Result();
        }

    }
}