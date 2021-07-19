using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBankingRESTService
{
    /// <summary>
    /// Գործողության հարցման պատասխան
    /// </summary>
    public class ActionRequestResponse
    {
        /// <summary>
        /// Օբեկտի ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Գործողության արդյունք
        /// </summary>
        public Result Result { get; set; }

        public ActionRequestResponse()
        {
            Result = new Result();
        }
    }
}
