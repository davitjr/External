namespace ExternalBanking
{
    /// <summary>
    /// Պահատուփերի քարտեզ
    /// </summary>
    public class DepositCaseMap
    {
        public ulong CaseId { get; set; }

        /// <summary>
        /// Պահատուփի համար
        /// </summary>
        public ulong CaseNumber { get; set; }

        /// <summary>
        /// Տեսակը
        /// </summary>
        public short CaseType { get; set; }

        /// <summary>
        /// Պահատուփի պատը
        /// </summary>
        public short CaseSide { get; set; }

        /// <summary>
        /// Օգտագործված է թե ոչ
        /// </summary>
        public bool InUse { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

    }
}
