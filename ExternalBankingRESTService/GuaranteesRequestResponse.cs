using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

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