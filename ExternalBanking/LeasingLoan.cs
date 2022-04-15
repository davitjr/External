using System;

namespace ExternalBanking
{
    public class LeasingLoan : LoanProduct
    {
        /// <summary>
        /// Լիզինգի վճարում
        /// </summary>
        public double LeasingPayment { get; set; }

        /// <summary>
        /// Կանխավճար և միջնորդավճար
        /// </summary>
        public double AdvanceAndFee { get; set; }

        /// <summary>
        /// Կանխավճար 
        /// </summary>
        public double AdvanceAmount { get; set; }
        /// <summary>
        /// միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Ապահովագրության պարտավորություն
        /// </summary>
        public double InsurancePayments { get; set; }

        /// <summary>
        /// Այլ պարտավորություններ
        /// </summary>
        public double OtherPayments { get; set; }


        /// <summary>
        /// Հերթական մարման օր
        /// </summary>
        public DateTime? NextRepaymentDate { get; set; }

        public string GeneralNumber { get; set; }

        public double PrepaymentAmount { get; set; }

    }
}
