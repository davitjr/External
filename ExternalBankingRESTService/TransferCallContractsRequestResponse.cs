using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

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