using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

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