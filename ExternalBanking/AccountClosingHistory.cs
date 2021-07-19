using System;

namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի փակման պատմություն
    /// </summary>
    public class AccountClosingHistory
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Փակմնան ամսաթիվ
        /// </summary>
        public DateTime? ClosingDate { get; set; }
        /// <summary>
        /// Մուտքագրողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Փակման նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// HB-ի գործարքի համար
        /// </summary>
        public ulong HBDocId { get; set; }

        /// <summary>
        /// Փակման տեսակ
        /// </summary>
        public short ReasonType { get; set; }

    }
}
