namespace ExternalBankingRESTService
{
    public class LoanProductInterestRateRequestResponse
    {

        public double LoanProductInterestRate { get; set; }
        public Result Result { get; set; }


        public LoanProductInterestRateRequestResponse()
        {
            Result = new Result();
        }

    }
}