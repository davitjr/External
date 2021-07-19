using System;
using System.Collections.Generic;
using ExternalBanking.DBManager;
using System.Data;


namespace ExternalBanking
{
    /// <summary>
    /// Վարկի գլխավոր պայմանագիր
    /// </summary>
    public class LoanMainContract
    {
        /// <summary>
        /// Վարկի գլխավոր պայմանագրի համար
        /// </summary>
        public string GeneralNumber { get; set; }

        /// <summary>
        /// Պայմանագրի սկիզբ
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Պայմանգրի վերջ
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        

    }
}
