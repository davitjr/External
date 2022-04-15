using System;

namespace ExternalBanking
{
    public class LoanRepaymentGrafik
    {
        /// <summary>
        /// Մարման ա/թ
        /// </summary>
        public DateTime RepaymentDate { get; set; }
        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }
        /// <summary>
        /// Մարման ենթակա գումար
        /// </summary>
        public double CapitalRepayment { get; set; }
        /// <summary>
        /// Տոկոսի մարում
        /// </summary>
        public double RateRepayment { get; set; }
        /// <summary>
        /// Ընդամենը մարում
        /// </summary>
        public double TotalRepayment { get; set; }
        /// <summary>
        /// Սպասարկման վճար
        /// </summary>
        public double FeeRepayment { get; set; }
        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double RestCapital { get; set; }

        /// <summary>
        /// Սուբսիդավորվող տոկոսագումար
        /// </summary>
        public double SubsidiaRateRepayment { get; set; }

        /// <summary>
        /// Չսուբսիդավորվող տոկոսագումար
        /// </summary>
        public double NonSubsidiaRateRepayment { get; set; }

        /// <summary>
        /// COVID-19 հետաձգված գումար
        /// </summary>
        public double RescheduledAmount { get; set; }

    }
}
