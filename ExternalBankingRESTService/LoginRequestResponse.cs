namespace ExternalBankingRESTService
{
    public class LoginRequestResponse
    {
        public string SessionId { get; set; }

        public int PasswordChangeRequirement { get; set; }

        public int UserPermission { get; set; }

        public Result Result { get; set; }

        public string FullName { get; set; }

        public string FullNameEnglish { get; set; }

        public bool IsLegal { get; set; }

        public LoginRequestResponse()
        {
            Result = new Result();
        }
    }
}