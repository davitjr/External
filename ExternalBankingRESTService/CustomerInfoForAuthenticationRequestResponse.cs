using ExternalBankingRESTService.ACBAServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static ExternalBankingRESTService.Enumerations;

namespace ExternalBankingRESTService
{
    public class CustomerInfoForAuthenticationRequestResponse
    {
        public CustomerInfoForAuthentication Info { get; set; }
        public Result Result { get; set; }
        public CustomerInfoForAuthenticationRequestResponse()
        {
            Result = new Result();
            Info = new CustomerInfoForAuthentication();
        }
    }

    public class CustomerInfoForAuthentication
    {
        public CustomerInfoForAuthentication()
        {
            Result = new CustomerAuthenticationResult();
            Data = new Dictionary<byte[], string>();
        }

        public ulong CustomerNumber { get; set; }

        public CustomerAuthenticationInfoType TypeOfDocument { get; set; }

        public Dictionary<byte[], string> Data { get; set; }

        public CustomerAuthenticationResult Result { get; set; }

        public string ResultDescription { get; set; }
    }
}