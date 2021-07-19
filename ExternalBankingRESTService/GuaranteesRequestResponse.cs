using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class GuaranteesRequestResponse
    {
       public List<Guarantee> Guarantees { get; set; }
       public Result Result { get; set; }

       public GuaranteesRequestResponse()
        {
            Result = new Result();
        }

    }
}