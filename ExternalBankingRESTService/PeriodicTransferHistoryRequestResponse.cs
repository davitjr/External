using ExternalBankingRESTService.XBS;
using System.Collections.Generic;


namespace ExternalBankingRESTService
{
    public class PeriodicTransferHistoryRequestResponse
    {
        public List<PeriodicTransferHistory> PeriodicTransferHistory { get; set; }
        public Result Result { get; set; }

        public PeriodicTransferHistoryRequestResponse()
        {
            Result = new Result();
        }
    }
}