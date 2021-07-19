﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalBankingRESTService
{
    public class FastOverdraftContractRequestResponse
    {

        /// <summary>
        /// Պայմանագրի բովանդակություն
        /// </summary>
        public string ContractContent { get; set; }

        /// <summary>
        /// Գործողության արդյունք
        /// </summary>
        public Result Result { get; set; }

        public FastOverdraftContractRequestResponse()
        {
            Result = new Result();
        }
    }
}