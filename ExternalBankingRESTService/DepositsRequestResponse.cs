using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class DepositsRequestResponse
    {
        public List<Deposit> Deposits { get; set; }
        public Result Result { get; set; }

        public DepositsRequestResponse()
        {
            Result = new Result();
        }

    }
}