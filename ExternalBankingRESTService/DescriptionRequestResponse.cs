
namespace ExternalBankingRESTService
{
    public class DescriptionRequestResponse
    {

        public string Description { get; set; }

        public Result Result { get; set; }

        public DescriptionRequestResponse()
        {
            Result = new Result();
        }
    }
}