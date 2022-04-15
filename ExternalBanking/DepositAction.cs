using System;

namespace ExternalBanking
{
    /// <summary>
    /// Ավանդի ակցիաներ
    /// </summary>
    public class DepositAction
    {
        /// <summary>
        /// Ակցիայի ունիկալ համար
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Ակցիայի տեսակ
        /// </summary>
        public ushort ActionNumber { get; set; }

        /// <summary>
        /// Ակցիայի տեսակի նկարագրություն
        /// </summary>
        public string ActionTypeDescription { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Ավանդի տեսակը
        /// </summary>
        public DepositType DepositType { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public double Percent { get; set; }
    }
}
