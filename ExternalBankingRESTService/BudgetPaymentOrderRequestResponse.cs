using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class BudgetPaymentOrderRequestResponse
    {

        public BudgetPaymentOrder PaymentOrder { get; set; }
        public Result Result { get; set; }

        public BudgetPaymentOrderRequestResponse()
        {
            Result = new Result();
        }
    }
}