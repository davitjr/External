namespace ExternalBankingRESTService
{
    public class ByteArrayRequestResponse
    {
        public byte[] Content { get; set; }
        public Result Result { get; set; }

        public ByteArrayRequestResponse()
        {
            Result = new Result();
        }
    }
}

