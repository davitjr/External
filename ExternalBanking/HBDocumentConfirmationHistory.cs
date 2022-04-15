namespace ExternalBanking
{
    public class HBDocumentConfirmationHistory
    {
        public long TransactionCode { get; set; }
        public string TransactionDate { get; set; }
        public string Description { get; set; }
        public string SetNumber { get; set; }
        public string CustomerUsername { get; set; }

        public int? UniqNumber { get; set; }
        public long? InternalTransactionCode { get; set; }
    }
}
