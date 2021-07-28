namespace ExternalBanking.Leasing
{
    public class LeasingPaymentsType
    {
        /// <summary>
        /// Վճարման տեսակ
        /// </summary>
        public byte PaymentType { get; set; }

        /// <summary>
        /// Վճարման անվանում
        /// </summary>
        public string PaymentName { get; set; }
    }
}
