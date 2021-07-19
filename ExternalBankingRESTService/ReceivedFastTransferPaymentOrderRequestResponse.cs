﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class ReceivedFastTransferPaymentOrderRequestResponse
    {
        public ReceivedFastTransferPaymentOrder ReceivedFastTransferPaymentOrder { get; set; }

        public Result Result { get; set; }

        public ReceivedFastTransferPaymentOrderRequestResponse()
        {
            Result = new Result();
        }
    }

}