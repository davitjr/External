namespace ExternalBankingRESTService
{
    public enum ResultCodes : ushort
    {
        normal = 1,
        failed = 2,
        notAutorized = 3,
        validationError = 4
    }


    public class Result
    {
        public ResultCodes ResultCode { get; set; }
        public string Description { get; set; }
    }
}