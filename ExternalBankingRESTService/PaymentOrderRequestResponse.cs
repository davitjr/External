using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class PaymentOrderRequestResponse
    {
        public PaymentOrder PaymentOrder { get; set; }
        public Result Result { get; set; }

        public PaymentOrderRequestResponse()
        {
            Result = new Result();
        }

    }
}