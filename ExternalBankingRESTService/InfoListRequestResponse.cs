using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class InfoListRequestResponse
    {
        public Dictionary<string, string> InfoList { get; set; }
        public Result Result { get; set; }

        public InfoListRequestResponse()
        {
            Result = new Result();
        }
    }
}