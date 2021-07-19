using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LoanStatement
    {
        //Ընդամենը մուտքեր
        public double TotalCreditAmount { get; set; }

        //Ընդամենը ելքեր
        public double TotalDebitAmount { get; set; }

        //Դրամային գործարքների ընդամենը մուտքեր
        public double TotalCreditAmountAMD { get; set; }

        //Դրամային գործարքների ընդամենը ելքեր
        public double TotalDebitAmountAMD { get; set; }

        //Արտարժութային գործարքների ընդամենը մուտքեր
        public double TotalCreditAmountInCurrency { get; set; }

        //Արտարժութային գործարքների ընդամենը ելքեր
        public double TotalDebitAmountInCurrency { get; set; }

        //Կատարված գործարքներ
        public List<LoanStatementDetail> Transactions { get; set; }

        /// <summary>
        /// Էջերի քանակ
        /// </summary>
        public int PagesCount { get; set; }

        public LoanStatement()
        {
            Transactions = new List<LoanStatementDetail>();            
        }
    }
}
