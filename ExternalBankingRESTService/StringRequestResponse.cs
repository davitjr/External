namespace ExternalBankingRESTService
{
    public class StringRequestResponse
    {
        public string Content { get; set; }
        public Result Result { get; set; }

        public StringRequestResponse()
        {
            Result = new Result();
        }
    }

}