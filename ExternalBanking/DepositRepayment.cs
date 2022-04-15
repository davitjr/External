using System;

namespace ExternalBanking
{
    public class DepositRepayment
    {
        /// <summary>
        /// Տոկոսի վճարման օր
        /// </summary>
        public DateTime DateOfRepayment { get; set; }
        /// <summary>
        /// Վերջնական գումար
        /// </summary>
        public double CapitalRepayment { get; set; }
        /// <summary>
        /// Տոկոսագումար
        /// </summary>
        public double RateRepayment { get; set; }
        /// <summary>
        /// Եկամտահարկ
        /// </summary>
        public double ProfitTax { get; set; }
    }
}
