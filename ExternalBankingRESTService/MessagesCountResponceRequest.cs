using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class MessagesCountResponceRequest
    {
        public int MessagesCount { get; set; }
        public Result Result { get; set; }

        public MessagesCountResponceRequest()
        {
            Result = new Result();
        }
    }
}