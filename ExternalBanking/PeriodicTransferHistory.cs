using System;

namespace ExternalBanking
{
    public class PeriodicTransferHistory
    {
        /// <summary>
        ///Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        ///Գործարքի արդյունքը
        /// </summary>
        public int OperationResult { get; set; }

        /// <summary>
        ///Գործարքի արդյունքի նկարագրություն
        /// </summary>

        public string OperationResultDescription { get; set; }

        /// <summary>
        ///Գործարքի արժույթը
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        ///Գործարքի գումարը
        /// </summary>
        public double Amount { get; set; }
    }
}
