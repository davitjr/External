using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class FactoringsRequestResponse
    {
       public List<Factoring> Factorings { get; set; }
       public Result Result { get; set; }

       public FactoringsRequestResponse()
        {
            Result = new Result();
        }
    }
}