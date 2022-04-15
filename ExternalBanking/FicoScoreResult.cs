using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class FicoScoreResult
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// FICO Սքոր
        /// </summary>
        public double FicoScore { get; set; }

        /// <summary>
        /// Միջին FICO Սքոր
        /// </summary>
        public double MidFicoScore { get; set; }

        /// <summary>
        /// Հարցման ամսաթիվ
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Գնահատականի վրա ազդեցություն ունեցող 4 հիմնական գործոններ
        /// </summary>
        public List<string> Reasons { get; set; }
    }
}
