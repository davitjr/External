using ExternalBankingRESTService.XBS;
using System.Collections.Generic;


namespace ExternalBankingRESTService
{
    public class DepositRepaymentResponse
    {
        public List<DepositRepayment> DepositRepaymentGrafik { get; set; }
        public Result Result { get; set; }

        public DepositRepaymentResponse()
        {
            Result = new Result();
        }
    }
}