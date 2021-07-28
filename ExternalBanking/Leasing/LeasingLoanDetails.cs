using System;

namespace ExternalBanking.Leasing
{

    public class LeasingLoanDetails : CustomerLeasingLoans
    {
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public long LoanAccount { get; set; }

        /// <summary>
        /// Վարկային կոդ
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }             

        /// <summary>
        /// Տույժ
        /// </summary>
        public double PenaltyRate { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public string GeneralNumber { get; set; }

        /// <summary>
        /// Ժամկետանց գումար
        /// </summary>
        public double OverdueCapital { get; set; }

        /// <summary>
        /// Ժամկետանց դառնալու ամսաթիվ
        /// </summary>
        public DateTime? OverdueLoanDate { get; set; }

        /// <summary>
        /// Ապահովագրավճար
        /// </summary>
        public double InsuranceAmount { get; set; }

        
    }
}
