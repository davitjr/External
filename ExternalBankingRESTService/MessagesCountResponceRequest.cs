namespace ExternalBankingRESTService
{
    public class MessagesCountResponceRequest
    {
        public int MessagesCount { get; set; }
        public Result Result { get; set; }

        public MessagesCountResponceRequest()
        {
            Result = new Result();
        }
    }
}