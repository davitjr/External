using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class LoansRequestResponse
    {
        public List<Loan> Loans { get; set; }
        public Result Result { get; set; }

        public LoansRequestResponse()
        {
            Result = new Result();
        }
    }
}