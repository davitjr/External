using ExternalBankingRESTService.XBS;
using System.Collections.Generic;


namespace ExternalBankingRESTService
{
    public class DepositCasesRequestResponse
    {
        public List<DepositCase> DepositCases { get; set; }
        public Result Result { get; set; }

        public DepositCasesRequestResponse()
        {
            Result = new Result();
        }
    }
}