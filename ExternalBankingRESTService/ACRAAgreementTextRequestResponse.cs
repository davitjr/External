namespace ExternalBankingRESTService
{
    public class ACRAAgreementTextRequestResponse
    {
        public string AcraText { get; set; }

        public Result Result { get; set; }

        public ACRAAgreementTextRequestResponse()
        {
            Result = new Result();
        }

    }
}