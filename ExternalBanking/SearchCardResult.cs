using System;

namespace ExternalBanking
{
    public class SearchCardResult
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Քարտի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Քարտի տեսակ (նկարագրություն)
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// Փակման ամսաթիվ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Քարտապանի նկարագրություն
        /// </summary>
        public string CardHolderDescription { get; set; }

        /// <summary>
        /// Քարտի մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Քարտի հաշվեհամար
        /// </summary>
        public Account CardAccount { get; set; }


    }
}
