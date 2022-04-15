using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class AccountStatementRequestResponse
    {
        public AccountStatement AccountStatement { get; set; }
        public Result Result { get; set; }

        public AccountStatementRequestResponse()
        {
            Result = new Result();
        }
    }
}