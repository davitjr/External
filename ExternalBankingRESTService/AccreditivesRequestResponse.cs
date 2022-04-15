using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class AccreditivesRequestResponse
    {
        public List<Accreditive> Accreditives { get; set; }
        public Result Result { get; set; }

        public AccreditivesRequestResponse()
        {
            Result = new Result();
        }
    }
}