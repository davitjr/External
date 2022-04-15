namespace ExternalBanking
{
    /// <summary>
    /// ՋՕԸ - ների տվյալներ
    /// </summary>
    public class WaterCoDetail
    {
        /// <summary>
        /// Համար
        /// </summary>
        public ushort Number { get; set; }

        /// <summary>
        /// Կոդ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public double Percent { get; set; }
    }
}
