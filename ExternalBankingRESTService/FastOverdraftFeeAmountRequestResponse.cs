namespace ExternalBankingRESTService
{
    public class FastOverdraftFeeAmountRequestResponse
    {
        public double FastOverdraftFeeAmount { get; set; }
        public Result Result { get; set; }

        public FastOverdraftFeeAmountRequestResponse()
        {
            Result = new Result();
        }
    }
}