using System;

namespace ExternalBanking
{
    /// <summary>
    /// Նվազեցում
    /// </summary>
    public class GivenGuaranteeReduction
    {

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductAppId { get; set; }

        /// <summary>
        /// Նվազեցման ա/թ
        /// </summary>
        public DateTime ReductionDate { get; set; }

        /// <summary>
        /// Նվազեցվող գումար
        /// </summary>
        public double ReductionAmount { get; set; }

        /// <summary>
        /// Գրության համար
        /// </summary>
        public string ReasonNumber { get; set; }

        /// <summary>
        /// Գրության տրման ա/թ
        /// </summary>
        public DateTime ReasonDate { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public uint SetNumber { get; set; }



    }
}
