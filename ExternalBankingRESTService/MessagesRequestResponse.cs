using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
    public class MessagesRequestResponse
    {
        public List<Message> Messages { get; set; }
        public Result Result { get; set; }

        public MessagesRequestResponse()
        {
            Result = new Result();
        }

    }
}