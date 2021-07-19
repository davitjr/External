using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// քարտի սպասարկման վարձի գրաֆիկ
    /// </summary>
    public class CardServiceFeeGrafik
    {
        /// <summary>
        /// Քարտի արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Սպասարկան միջնորդավճար
        /// </summary>
        public double ServiceFee { get; set;}
        /// <summary>
        /// Սպասարկան սկիզբ
        /// </summary>
        public DateTime PeriodStart { get; set; }
        /// <summary>
        /// Սպասարկան վերջ
        /// </summary>
        public DateTime PeriodEnd { get; set; }

    }
}
