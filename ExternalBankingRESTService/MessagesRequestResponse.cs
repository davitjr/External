using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

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