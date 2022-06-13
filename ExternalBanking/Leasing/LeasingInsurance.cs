using System;

namespace ExternalBanking.Leasing
{
    public class LeasingInsurance
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int InsuranceId { get; set; }

        public string InsuranceDescription { get; set; }

        /// <summary>
        /// Վճարման գւմար
        /// </summary>
        public double? Amount { get; set; }

        /// <summary>
        /// Վճարման ամսաթիվ
        /// </summary>
        public DateTime? PayDate { get; set; }
    }
}
