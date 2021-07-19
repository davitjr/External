using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class InternationalPaymentOrderRequestResponse
    {

        public InternationalPaymentOrder InternationalPaymentOrder { get; set; }
        public Result Result { get; set; }

        public InternationalPaymentOrderRequestResponse()
        {
            Result = new Result();
        }

    }
}