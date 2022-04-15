using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.Leasing
{
    public class LeasingInsurance
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int InsuranceId { get; set; }

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
