using System;

namespace ExternalBanking
{
    public class HBDocumentFilters
    {
        public int? TransactionCode { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public Int16? OperationType { get; set; }
        public int? SourceType { get; set; }
        public int? QualityType { get; set; }
        public long? CustomerNumber { get; set; }
        public long? DebitAccount { get; set; }
        public int? FilialCode { get; set; }
        public int? DocumentType { get; set; }
        public int? DocumentSubType { get; set; }
        public float? Amount { get; set; }
        public string CurrencyType { get; set; }

        public string Description { get; set; }

        public bool OnlySelectedCustomer { get; set; }

        public int? OnlyACBA { get; set; }

        public int BankCode { get; set; }

        //add /remove checkbox
        public int DocumentTransactionCode { get; set; }
        public bool TransactionChecked { get; set; }

        public int SetNumber { get; set; }

        public int FileIndex { get; set; }

        public int firstRow { get; set; }
        public int LastGetRowCount { get; set; }
        public string DebitAccountCurrencyType { get; set; }
    }
}
