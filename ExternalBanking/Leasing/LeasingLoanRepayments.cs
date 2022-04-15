using System;

namespace ExternalBanking.Leasing
{
    public class LeasingLoanRepayments
    {
        /// <summary>
        /// Մայր գումարի վճարում
        /// </summary>
        public double CapitalRepayment { get; set; }

        /// <summary>
        /// Տոկոսագումարի վճարում
        /// </summary>
        public double RateRepayment { get; set; }

        /// <summary>
        /// Ամսական վճարում
        /// </summary>
        public double PayableAmount { get; set; }

        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double CurrentCapital { get; set; }

        /// <summary>
        /// Վճարման ամսաթիվ
        /// </summary>
        public DateTime DateOfRepayment { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }        

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

    }
}
