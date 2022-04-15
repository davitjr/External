using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class AccreditiveRequestResponse
    {
        public Accreditive Accreditive { get; set; }
        public Result Result { get; set; }

        public AccreditiveRequestResponse()
        {
            Result = new Result();
        }
    }
}