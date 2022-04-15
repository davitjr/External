namespace ExternalBanking
{
    public class ProvisionLoan
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }
        /// <summary> 0
        /// Վարկի հաճախորդի համար
        /// </summary>
        public ulong LoanCustomerNumber { get; set; }
        /// <summary>
        /// Վարի հաճախորդի նկարագրություն
        /// </summary>
        public string LoanCustomerDescription { get; set; }
        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public string LoanAccount { get; set; }
        /// <summary>
        /// Վարկի կարգավիճակ
        /// </summary>
        public short LoanQuality { get; set; }
        /// <summary>
        /// Վարկի կարգավիճակի նկարագրություն
        /// </summary>
        public string LoanQualityDescription { get; set; }
        /// <summary>
        /// վարկի տեսակ
        /// </summary>
        public short Loantype { get; set; }
        /// <summary>
        /// վարկի տեսակի նկարագրություն
        /// </summary>
        public string LoanTypeDescription { get; set; }
        /// <summary>
        /// արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// գումար
        /// </summary>
        public double StartCapital { get; set; }
    }
}
