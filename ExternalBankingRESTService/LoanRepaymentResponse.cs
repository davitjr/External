
using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class LoanRepaymentResponse
    {
        public List<LoanRepaymentGrafik> LoanRepaymentGrafik { get; set; }
        public Result Result { get; set; }

        public LoanRepaymentResponse()
        {
            Result = new Result();
        }
    }
}