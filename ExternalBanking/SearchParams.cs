using System;

namespace ExternalBanking
{
    public class SearchParams
    {
        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime DateTo { get; set; }
        /// <summary>
        /// Առաջին տողի համար
        /// </summary>
        public int StartRow { get; set; }
        /// <summary>
        /// Վերջին տողի համար
        /// </summary>
        public int EndRow { get; set; }
    }
}
