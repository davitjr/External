using System;

namespace ExternalBanking
{
    public class CardStatus
    {
        /// <summary>
        /// Քարտի կարգավիճակի համարը 1-ՏՐ,2-ԹՏՐ,3-ՉՏՐ
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Քարտի կարգավիճակի նկարագրությունը
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Կարգավիճակի փոփոխման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Թերի տրամադրման պատճառ
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Աշխատակցի համար
        /// </summary>
        public int UserId { get; set; }

    }
}
