namespace ExternalBanking
{
    /// <summary>
    /// Ցպահանջ ավանդի սակագնի տվյալներ
    /// </summary>
    public class DemandDepositRateTariffDetail
    {

        /// <summary>
        /// Սակագնի տեսակ
        /// </summary>
        public int TariffGroup { get; set; }

        /// <summary>
        /// Սակագնի ենթատեսակ
        /// </summary>
        public int TariffTypeId { get; set; }

        /// <summary>
        /// Գումար որտեղից
        /// </summary>
        public double AmountFrom { get; set; }

        /// <summary>
        /// Գումար մինչև
        /// </summary>
        public double AmountTo { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string ProductCurrency { get; set; }

    }
}
