namespace ExternalBankingRESTService
{
    public class ExchangeRateRequestResponse
    {
        public double ExchangeRate { get; set; }
        public Result Result { get; set; }

        public ExchangeRateRequestResponse()
        {
            Result = new Result();
        }


    }
}