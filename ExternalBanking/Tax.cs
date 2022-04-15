using ExternalBanking.DBManager;
using System;

namespace ExternalBanking
{
    /// <summary>
    /// Պետ տուրք
    /// </summary>
    public class Tax
    {
        /// <summary>
        /// Դատական գործընթացի համար
        /// </summary>
        public int ClaimNumber { get; set; }

        /// <summary>
        /// Իրադարձության համար
        /// </summary>
        public int EventNumber { get; set; }

        /// <summary>
        /// Գրանցման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Հարկի տեսակ
        /// </summary>
        public short Type { get; set; }

        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Փոխանցման ա/թ
        /// </summary>
        public DateTime TransferRegistrationDate { get; set; }

        /// <summary>
        /// Վճարված գումար
        /// </summary>
        public double MaturedAmount { get; set; }

        /// <summary>
        /// Հարկի նպատակ
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Հարկի տոկոսադրույք
        /// </summary>
        public double Percent { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Հարկի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Զիջված գումար
        /// </summary>
        public double ConcedeAmount { get; set; }

        /// <summary>
        /// Փոխանցող հանձնողի համար
        /// </summary>
        public int? TransferUnicnumber { get; set; }

        /// <summary>
        /// Զիջման զեկուցագիր
        /// </summary>
        public string ConcedeAddInf { get; set; }

        /// <summary>
        /// Դատարանի վճիռ
        /// </summary>
        public short? CourtDecision { get; set; }

        /// <summary>
        /// Դատարանի վճիռի ա/թ
        /// </summary>
        public DateTime? CourtDecisionDate { get; set; }

        /// <summary>
        /// Դուրսգրման ա/թ
        /// </summary>
        public DateTime? OutLoanDate { get; set; }



        public static Tax GetTax(int claimNumber, int eventNumber)
        {
            return TaxDB.GetTax(claimNumber, eventNumber);
        }

        /// <summary>
        /// Պետ. տուրքի հաշվարկ
        /// </summary>
        /// <param name="claimNumber"></param>
        /// <param name="eventNumber"></param>
        /// <returns></returns>
        internal static ProblemLoanCalculationsDetail GetProblemLoanCalculationsDetail(int claimNumber, int eventNumber)
        {
            return TaxDB.GetProblemLoanCalculationsDetail(claimNumber, eventNumber);
        }


        /// <summary>
        /// Պետ տուրք
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static double GetPetTurk(long productId)
        {
            return TaxDB.GetPetTurk(productId);
        }

    }
}
