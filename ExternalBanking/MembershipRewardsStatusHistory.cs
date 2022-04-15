using System;

namespace ExternalBanking
{
    public class MembershipRewardsStatusHistory
    {
        /// <summary>
        /// MR-ի համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// MR կարգավիճակ
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        /// MR կարգավիճակի նկարագրություն
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Կարգավիճակի փոփոխման ա/թ
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// Փոփոխողի ՊԿ
        /// </summary>
        public int ChangeSetNumber { get; set; }

    }
}
