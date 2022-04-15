namespace ExternalBankingRESTService
{
    public class AccountDescriptionRequestResponse
    {
        public string AccountDescription { get; set; }


        public Result Result { get; set; }

        public AccountDescriptionRequestResponse()
        {
            Result = new Result();
        }
    }
}