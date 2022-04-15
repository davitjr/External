using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Ավանդի տվյալներ
    /// </summary>
    public class DepositOrderCondition
    {
        /// <summary>
        /// Մինիմալ վերջնաժամկետ
        /// </summary>
        public DateTime MinDate { get; set; }
        /// <summary>
        /// Մաքսիամալ վերջնաժամկետ
        /// </summary>
        public DateTime MaxDate { get; set; }
        /// <summary>
        /// Մինիմալ գումար
        /// </summary>
        public double MinAmount { get; set; }
        /// <summary>
        /// Ավանդի տոկոսադրույք
        /// </summary>
        public double Percent { get; set; }

        /// <summary>
        /// Ավանդի օպցիայի տոկոսադրույքը
        /// </summary>
        public double InterestRateVariationFromOption { get; set; }

        /// <summary>
        /// Ավանդի նոմինալ տոկոսադրույք
        /// </summary>
        public double NominalRate { get; set; }

        /// <summary>
        /// Ավանդի օպցիաներ
        /// </summary>
        public List<DepositOption> DepositOption { get; set; }


    }
}
