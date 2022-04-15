namespace ExternalBanking
{
    /// <summary>
    /// Կանխիկ տերմինալում վճարման գրանցման արդյունք
    /// </summary>
    public class PaymentRegistrationResult
    {
        /// <summary>
        /// Վճարման գրանցման արդյունքի կարգավիճակ
        /// </summary>
        public byte ResultCode { get; set; }

        /// <summary>
        /// Վճարման գրանցման արդյունքի նկարագրություն
        /// </summary>
        public string ResultDescription { get; set; }

        /// <summary>
        /// Վճարման նույնականացուցիչ համակարգում
        /// </summary>
        public long PaymentID { get; set; }

        /// <summary>
        /// Վճարման հարցման նույնականացուցիչ։
        /// </summary>
        public long OrderID { get; set; }

    }
}
