using System;

namespace ExternalBanking
{
    /// <summary>
    /// Վարկի միջնորդավճար և այլ մուծումներ
    /// </summary>
    public class ProductOtherFee
    {
        /// <summary>
        /// Վճարվման ամսաթիվ 
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

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
