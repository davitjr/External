using System;

namespace ExternalBanking
{
    /// <summary>
    /// Վարկի/Վարկային գծերի երկարացման դիմումներ
    /// </summary>
    public class LoanProductProlongation
    {
        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Փաստացի տոկոսադրույք
        /// </summary>
        public decimal InterestRateEffective { get; set; }

        /// <summary>
        /// Փաստացի տոկոսադրույք ծախսերով
        /// </summary>
        public decimal InterestRateFull { get; set; }

        /// <summary>
        /// Տուժանքի տոկոսադրույք
        /// </summary>
        public decimal? PenaltyAddPercent { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Գրանցման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public int RegistrationSetNumber { get; set; }

        /// <summary>
        /// Հաստատման ա/թ
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>
        /// Հաստատողի ՊԿ
        /// </summary>
        public int ConfirmationSetNumber { get; set; }

        /// <summary>
        /// Ակտիվացման ա/թ
        /// </summary>
        public DateTime? ActivationDate { get; set; }

        /// <summary>
        /// Ակտիվացնողի ՊԿ
        /// </summary>
        public int ActivationSetNumber { get; set; }
    }
}
