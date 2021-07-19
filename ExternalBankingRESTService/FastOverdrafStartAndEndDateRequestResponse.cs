using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalBankingRESTService
{
    /// <summary>
    /// Արագ օվերդրաֆտի սկիզբ և վերջ հարցման պատասխան
    /// </summary>
    public class FastOverdrafStartAndEndDateRequestResponse
    {
        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Գործողության արդյունք
        /// </summary>
        public Result Result { get; set; }

        public FastOverdrafStartAndEndDateRequestResponse()
        {
            Result = new Result();
        }

    }
}