using System;

namespace ExternalBanking.Leasing
{
    public class CustomerLeasingLoans
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Լիզինգի տեսակ
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// Սկզբնական գումար	
        /// </summary>
        public double StartCapital { get; set; }

        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double CurrentCapital { get; set; }

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
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Հերթական վճարում
        /// </summary>
        public double LeasingPayment { get; set; }

        /// <summary>
        /// Հերթական վճարման ամսաթիվ
        /// </summary>
        public DateTime? NextRepaymentDate { get; set; }
    }
}
