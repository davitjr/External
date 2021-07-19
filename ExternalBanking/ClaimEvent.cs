using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Դատական իրադարձություն
    /// </summary>
    public class ClaimEvent
    {
        /// <summary>
        /// Դատական գործընթացի համար
        /// </summary>
        public int claimNumber { get; set; }

        /// <summary>
        /// Իրադարձության համար
        /// </summary>
        public int EventNumber { get; set; }

        /// <summary>
        /// Իրադարձության ա/թ
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Իրադարձության տեսակ
        /// </summary>
        public short Type { get; set; }

        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Դատարանի տեսակ
        /// </summary>
        public short CourtType { get; set; }

        /// <summary>
        /// Դատարանի նկարագրություն
        /// </summary>
        public string CourtTypeDescription { get; set; }

        /// <summary>
        /// Իրադարձություն մուտքագրողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Իրադարձության նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Դատական գործընթացի գումար
        /// </summary>
        public double ClaimAmount { get; set; }

        /// <summary>
        /// Իրադարձության նպատակ
        /// </summary>
        public short Purpose { get; set; }

        /// <summary>
        /// Նպատակի նկարագրություն
        /// </summary>
        public string PurposeDescription { get; set; }

        public List<Tax> EventTax { get; set; }



        public static List<ClaimEvent> GetClaimEvents(int claimNumber)
        {
            return ClaimEventDB.GetClaimEvents(claimNumber);
        }
    }
}
