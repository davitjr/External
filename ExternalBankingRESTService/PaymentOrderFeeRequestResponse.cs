using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class PaymentOrderFeeRequestResponse
    {
        public double PaymentOrderFee { get; set; }
        public Result Result { get; set; }

        public PaymentOrderFeeRequestResponse()
        {
            Result = new Result();
        }
    }
}