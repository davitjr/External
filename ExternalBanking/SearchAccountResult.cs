using System;

namespace ExternalBanking
{
    public class SearchAccountResult
    {
        /// <summary>
        /// Որոնվող հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի hաշվի տեսակ
        /// </summary>
        public ushort AccountType { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի hաշվի տեսակի նկարագրություն
        /// </summary>
        public string AccountTypeDescription { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի փակման ամսաթիվ
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// Որոնվող հաշվեհամարի պրոդուկտի (քարտի/ավանդի) համար
        /// </summary>
        public string ProductNumber { get; set; }


    }
}
