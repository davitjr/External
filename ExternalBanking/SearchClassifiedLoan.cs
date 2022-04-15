namespace ExternalBanking
{
    public class SearchClassifiedLoan : SearchParams
    {
        /// <summary>
        /// Պրոդուկտի ֆիլտր
        /// </summary>
        public ProductQualityFilter QualityFilter { get; set; }
        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Կցված ընթացիկ հաշիվ
        /// </summary>
        public ulong LoanFullNumber { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Դասակարգված վարկերի ցանկի տեսակ
        /// </summary>
        public ClassifiedLoanListType ListType { get; set; }

        /// <summary>
        /// Ամբողջ դատան
        /// </summary>
        public bool AllData { get; set; }

    }
}