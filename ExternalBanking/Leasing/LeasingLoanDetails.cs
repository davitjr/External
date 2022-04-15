using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.Leasing
{
    public class LeasingLoanDetails : CustomerLeasingLoans
    {
        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public long LoanAccount { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }

        /// <summary>
        /// Փաստացի տոկոսադրույք
        /// </summary>
        public float ActualInterestRate { get; set; }

        ///// <summary>
        ///// Ապագա ժամանակաշրջանի տոկոսագումար
        ///// </summary>
        //public double FuturePercent { get; set; }        

        /// <summary>
        /// Ժամկետանց գումար
        /// </summary>
        public double OverdueCapital { get; set; }

        /// <summary>
        /// Ժամկետանց տոկոսագումար
        /// </summary>
        public double OverduePercent { get; set; }

        /// <summary>
        /// Ժամկետանց դառնալու ամսաթիվ
        /// </summary>
        public DateTime? OverdueLoanDate { get; set; }

        /// <summary>
        /// Ժամկետանց օրեր
        /// </summary>
        public short OverdueDays { get; set; }

        /// <summary>
        /// Սուբսիդավորման տոկոս
        /// </summary>
        public float SubsidRate { get; set; }

        /// <summary>
        /// Անուն
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Անվանում
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Հաճախորդ համարը լիզինգում
        /// </summary>
        public int LeasingCustomerNumber { get; set; }

        /// <summary>
        /// Կուրս
        /// </summary>
        public double Kurs { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public long CustomerNumber { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; }

        /// <summary>
        /// Մասնաճյուղի անվանում
        /// </summary>
        public string FilialName { get; set; }

        /// <summary>
        /// Տոկոսագումարի հաշվարկի ամսաթիվ
        /// </summary>
        public DateTime? DateOfRateCalculation { get; set; }

        /// <summary>
        /// Վերջին վճարման ամսաթիվ
        /// </summary>
        public DateTime? DateOfLastPayment { get; set; }

        /// <summary>
        /// Ընացիկ հաշվի մնացորդ
        /// </summary>
        public double AccountBalance { get; set; }

        /// <summary>
        /// Ընացիկ հաշվի մնացորդ AMD
        /// </summary>
        public double AccountBalanceAMD { get; set; }

        /// <summary>
        /// Կանխավճար
        /// </summary>
        public double AdvanceAmount { get; set; }

        /// <summary>
        /// Կուտակված (չվճարված) տոկոսագումար
        /// </summary>
        public double AccumulatedPercent { get; set; }

        /// <summary>
        /// Կուտակված (վճարված) տոկոսագումար
        /// </summary>
        public double AccumulatedPercentPaied { get; set; }

        /// <summary>
        /// Կուտակված (չվճարված) տուժանք
        /// </summary>
        public double PenaltyRate { get; set; }

        /// <summary>
        /// Կուտակված (վճարված) տուժանք
        /// </summary>
        public double PenaltyRatePaied { get; set; }

        /// <summary>
        /// Ապահովագրավճար
        /// </summary>
        public double InsuranceAmount { get; set; }

        /// <summary>
        /// Ապահովագրավճարի ձևակերպման ամսաթիվ
        /// </summary>
        public DateTime? InsuranceCalculationDate { get; set; }

        /// <summary>
        /// Ապահովագրավճարի վճարման ամսաթիվ
        /// </summary>
        public DateTime? InsurancePaymentDate { get; set; }

        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Միջնորդավճարի վճարման ամսաթիվ
        /// </summary>
        public DateTime? FeePaymentDate { get; set; }

        /// <summary>
        /// Այլ վճար
        /// </summary>
        public double OtherPaymentAmount { get; set; }

        /// <summary>
        /// Այլ վճարի վճարման ամսաթիվ
        /// </summary>
        public DateTime? OtherPaymentDate { get; set; }

        /// <summary>
        /// նախավարձ
        /// </summary>
        public double PrepaymentAmount { get; set; }

        /// <summary>
        /// նախավարձի վճարման ամսաթիվ
        /// </summary>
        public DateTime? PrepaymentPaymentDate { get; set; }

        /// <summary>
        /// Վերջնավճար
        /// </summary>
        public double FinalFee { get; set; }

        /// <summary>
        /// Գրաֆիկով առաջիկա վճարումների քանակ
        /// </summary>
        public short PaymentsCount { get; set; }

    }
}
