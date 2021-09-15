using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Միջնորդավճար
    /// </summary>
    public class OrderFee
    {
        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Տեսակ
        /// </summary>
        public short Type { get; set; }
        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Հայտի համար
        /// </summary>
        public string OrderNumber { get; set; }
        
        /// <summary>
        /// Կրեդիտագրվող հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Գործարքի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Չգանձման պատճառ
        /// </summary>
        public string DescriptionForRejectFeeType { get; set; }

        /// <summary>
        /// Չգանձման պատճառի տեսակ
        /// </summary>
        public int? RejectFeeType { get; set; }

        /// <summary>
        /// Չգանձման պատճառի տեսակի նկարագրություն
        /// </summary>
        public string RejectFeeTypeDescription { get; set; }
    }
}
