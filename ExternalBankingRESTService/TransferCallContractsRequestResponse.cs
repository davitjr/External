using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class TransferCallContractsRequestResponse
    {

        public List<TransferCallContract> TransferCallContracts { get; set; }

        public Result Result { get; set; }

        public TransferCallContractsRequestResponse()
        {
            Result = new Result();
        }
    }
}