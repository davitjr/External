using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.Leasing
{
    public class LeasingOverdueDetail
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long Productid { get; set; }

        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public byte ProductType { get; set; }

        /// <summary>
        /// Պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }
        
        /// <summary>
        /// Պրոդուկտի սկիզբ
        /// </summary>
        public DateTime ProductStartDate { get; set; }

        /// <summary>
        /// Պրոդուկտի վերջ
        /// </summary>
        public DateTime ProductEndDate { get; set; }

        /// <summary>
        /// Մայր գումար
        /// </summary>
        public double StartCapital { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// ժամկետանց օրերի քանակ
        /// </summary>
        public ushort OverdueDaysCount { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Տոկոսագումար
        /// </summary>
        public double RateAmount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string OverdueCurrency { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public byte Quality { get; set; }        
    }
}
