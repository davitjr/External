using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class ContactRequestResponse
    {
        public Contact Contact { get; set; }
        public Result Result { get; set; }

        public ContactRequestResponse()
        {
            Result = new Result();
        }
    }
}