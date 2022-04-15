namespace ExternalBankingRESTService
{
    public class LoanMatureCapitalPenaltyRequestResponse
    {
        public double LoanMatureCapitalPenalty { get; set; }
        public Result Result { get; set; }

        public LoanMatureCapitalPenaltyRequestResponse()
        {
            Result = new Result();
        }
    }
}