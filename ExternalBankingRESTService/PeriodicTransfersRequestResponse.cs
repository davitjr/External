using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class PeriodicTransfersRequestResponse
    {
        public List<PeriodicTransfer> PeriodicTransfers { get; set; }
        public Result Result { get; set; }

        public PeriodicTransfersRequestResponse()
        {
            Result = new Result();
        }
    }
}