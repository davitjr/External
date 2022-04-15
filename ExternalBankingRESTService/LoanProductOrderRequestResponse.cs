using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class LoanProductOrderRequestResponse
    {
        public LoanProductOrder LoanProductOrder { get; set; }

        public Result Result { get; set; }

        public LoanProductOrderRequestResponse()
        {
            Result = new Result();
        }


    }
}