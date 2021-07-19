using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class PaymentOrderFutureBalanceRequestResponse
    {
        public PaymentOrderFutureBalance FutureBalance { get; set; }

        public Result Result { get; set; }

        public PaymentOrderFutureBalanceRequestResponse()
        {
            Result = new Result();
        }
    }
}