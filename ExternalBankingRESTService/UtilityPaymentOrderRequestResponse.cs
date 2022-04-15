using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class UtilityPaymentOrderRequestResponse
    {
        public UtilityPaymentOrder PaymentOrder { get; set; }
        public Result Result { get; set; }

        public UtilityPaymentOrderRequestResponse()
        {
            Result = new Result();
        }

    }
}