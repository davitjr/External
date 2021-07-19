using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

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