using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Պետ. տուրքի հաշվարկ
    /// </summary>
    public class ProblemLoanCalculationsDetail
    {
        /// <summary>
        /// Պետ. տուրքի հաշվարկի համար
        /// </summary>
        public ulong ClaimCalculationNumber { get; set; }

        /// <summary>
        /// Դատական գործընթացի համար
        /// </summary>
        public int ClaimNumber { get; set; }

        /// <summary>
        /// Իրադարձության համար
        /// </summary>
        public int EventNumber { get; set; }

        /// <summary>
        /// Պետ. տուրքի հաշվարկի ա/թ
        /// </summary>
        public DateTime ClaimCalculationDate { get; set; }

        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double CurrentCapital { get; set; }

        /// <summary>
        /// Ժամկետանց գումար
        /// </summary>
        public double OverdueCapital { get; set; }


        /// <summary>
        /// Կուտակված տոկոսագումար
        /// </summary>
        public double CurrentRateValue { get; set; }

        /// <summary>
        /// Որից ժամկետանց
        /// </summary>
        public double InpaiedRestOfRate { get; set; }

        /// <summary>
        /// Տուգանային տոկոսագումար
        /// </summary>
        public double PenaltyRate { get; set; }


        /// <summary>
        /// Որից ժամկետանց
        /// </summary>
        public double PenaltyAdd { get; set; }

        /// <summary>
        /// Սպասարկման վճար
        /// </summary>
        public double CurrentFee { get; set; }

        /// <summary>
        /// Վերջին վճարման օր
        /// </summary>
        public DateTime? LastDateOfRateCalculation { get; set; }

        /// <summary>
        /// Հաշվարկի փոխարժեք
        /// </summary>
        public double Course { get; set; }

        /// <summary>
        /// Ընդհանուր պարտք
        /// </summary>
        public double DebtAmount { get; set; }


    }
}
