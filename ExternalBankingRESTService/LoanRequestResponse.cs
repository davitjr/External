using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class LoanRequestResponse
    {
        public Loan Loan { get; set; }
        public Result Result { get; set; }

        public LoanRequestResponse()
        {
            Result = new Result();
        }
    }
}