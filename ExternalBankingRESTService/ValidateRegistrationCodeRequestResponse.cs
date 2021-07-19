using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalBankingRESTService
{
    public class ValidateRegistrationCodeRequestResponse
    {
        public string UserName { get; set; }


        public Result Result { get; set; }

        public ValidateRegistrationCodeRequestResponse()
        {
            Result = new Result();
        }
    }
}