using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class GuaranteeRequestResponse
    {
        public Guarantee Guarantee { get; set; }
        public Result Result { get; set; }

        public GuaranteeRequestResponse()
        {
            Result = new Result();
        }

    }
}