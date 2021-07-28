using System;

namespace ExternalBanking.Leasing
{
    public class LeasingLoanStatements
    {
        /// <summary>
        /// Վճարման ամսաթիվ
        /// </summary>
        public DateTime DateOfStatement { get; set; }

        /// <summary>
        /// Վճարված գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Վճարման նկարագրություն
        /// </summary>
        public string Wording { get; set; }
    }
}
