using System;

namespace ExternalBanking.Leasing
{
    public class CustomerLeasingLoans
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

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
        /// Մնացորդ ՀՀ դրամ
        /// </summary>
        public double CurrentCapitalAMD { get; set; }

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
        /// Հերթական վճարում
        /// </summary>
        public double LeasingPayment { get; set; }

        /// <summary>
        /// Հերթական վճարման ամսաթիվ
        /// </summary>
        public DateTime? NextRepaymentDate { get; set; }

        /// <summary>
        /// Վարկային կոդ
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        /// Լիզինգի ընթացիկ հաշիվ
        /// </summary>
        public ulong LoanFullNumber { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public byte Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public string GeneralNumber { get; set; }
    }
}
