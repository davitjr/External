using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class CreditLineGrafikRequestResponse
    {
        public List<CreditLineGrafik> CreditLineGrafik { get; set; }
        public Result Result { get; set; }

        public CreditLineGrafikRequestResponse()
        {
            Result = new Result();
        }
    }
}