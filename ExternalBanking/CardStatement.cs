using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class CardStatement
    {
        //Վերջնական մնացորդ
        public double FinalBalance { get; set; }

        //Սկզբնական մնացորդ
        public double InitialBalance { get; set; }

        //Ընդամենը մուտքեր
        public double TotalCreditAmount { get; set; }

        //Ընդամենը ելքեր
        public double TotalDebitAmount { get; set; }

        /// <summary>
        /// Հաշվետու ժամանակաշրջան
        /// </summary>
        public string ReportingPeriod { get; set; }

        ///Կատարված գործարքներ
        public List<CardStatementDetail> Transactions { get; set; }

        /// <summary>
        /// Էջերի քանակ
        /// </summary>
        public int PagesCount { get; set; }


        /// <summary>
        /// MR
        /// </summary>
        public class MRCardStatement
        {
            #region MR
            public decimal InitialPoints { get; set; }
            public decimal EarnedPoints { get; set; }
            public decimal RedeemedPoints { get; set; }
            public decimal FinalPoints { get; set; }
            #endregion
        }

        /// <summary>
        /// ՎԱՐԿԱՅԻՆ ԳԾԻ ՎԵՐԱԲԵՐՅԱԼ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
        /// </summary>
        public class SummaryCreditLineCardStatement
        {
            #region SummaryCreditLine

            /// <summary>
            /// Նկարագրություն հայերան
            /// </summary>
            public string DescrArm { get; set; }

            /// <summary>
            /// Նկարագրություն անգլերեն
            /// </summary>
            public string DescrEng { get; set; }

            /// <summary>
            /// Index descr
            /// </summary>
            public short LinkIndexArm { get; set; }

            /// <summary>
            /// Index descr
            /// </summary>
            public short LinkIndexEng { get; set; }

            /// <summary>
            ///  
            /// </summary>
            public string UnitArm { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string UnitEng { get; set; }

            /// <summary>
            /// Արժեք
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Կարմիրով նշելու համար դաշտ
            /// </summary>
            public bool IsRed { get; set; }

            #endregion
        }

        /// <summary>
        /// ՎԱՐԿԱՅԻՆ ԳԾԻ ՎԵՐԱԲԵՐՅԱԼ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
        /// </summary>
        public class LinkCardStatement
        {
            #region Link

            /// <summary>
            /// Նկարագրության համար
            /// </summary>
            public int IndexArm { get; set; }

            /// <summary>
            /// Նկարագրության համար
            /// </summary>
            public int IndexEng { get; set; }

            /// <summary>
            /// Նկարագրություն հայերան
            /// </summary>
            public string ValueArm { get; set; }

            /// <summary>
            /// Նկարագրություն անգլերեն
            /// </summary>
            public string ValueEng { get; set; }

            /// <summary>
            /// տեսակ
            /// </summary>
            public int LinkType { get; set; }

            #endregion
        }

        /// <summary>
        /// ՎԱՍՏԱԿԱԾ ԵԿԱՄՈՒՏՆԵՐ ԵՎ ԲՈՆՈՒՍՆԵՐ
        /// </summary>
        public class AddInfoCardStatement
        {
            #region AddInfo

            /// <summary>
            /// Նկարագրություն հայերան
            /// </summary>
            public string DescrArm { get; set; }

            /// <summary>
            /// Նկարագրություն անգլերեն
            /// </summary>
            public string DescrEng { get; set; }

            /// <summary>
            /// Index descr
            /// </summary>
            public int LinkIndexArm { get; set; }

            /// <summary>
            /// Index descr
            /// </summary>
            public int LinkIndexEng { get; set; }

            /// <summary>
            /// Ժամանակաշրջան հայերեն
            /// </summary>
            public string PeriodArm { get; set; }

            /// <summary>
            /// Ժամանակաշրջան Անգլերեն
            /// </summary>
            public string PeriodEng { get; set; }

            /// <summary>
            /// Արժեք
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Կարմիրով նշելու համար դաշտ
            /// </summary>
            public bool IsRed { get; set; }
            #endregion
        }

        /// <summary>
        /// Լրացուցիչ տվյալներ
        /// </summary>
        public class SummaryBonusCardStatement
        {
            #region SummaryBonus

            /// <summary>
            /// Նկարագրություն հայերան
            /// </summary>
            public string DescrArm { get; set; }

            /// <summary>
            /// Նկարագրություն անգլերեն
            /// </summary>
            public string DescrEng { get; set; }

            /// <summary>
            /// Index descr
            /// </summary>
            public short LinkIndexArm { get; set; }

            /// <summary>
            /// Index descr
            /// </summary>
            public short LinkIndexEng { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string UnitArm { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string UnitEng { get; set; }

            /// <summary>
            /// Արժեք
            /// </summary>
            public string Value { get; set; }


            #endregion
        }

        /// <summary>
        /// ՉՁԵՎԱԿԵՐՊՎԱԾ ԵՎ ԱՐԳԵԼԱԴՐՎԱԾ  ԳՈՐԾԱՐՔՆԵՐԻ/ԳՈՐԾԱՌՆՈՒԹՅՈՒՆՆԵՐԻ ՎԵՐԱԲԵՐՅԱԼ ՄԱՆՐԱՄԱՍՆ ՏԵՂԵԿԱՏՎՈՒԹՅՈՒՆ
        /// </summary>
        public class UnsettledTransactions
        {
            #region UnsettledTransactionsPropertis

            /// <summary>
            /// Գործարքի ամսաթիվ
            /// </summary>
            public DateTime DateOfTransaction { get; set; }

            /// <summary>
            /// Գործարքի գումար
            /// </summary>
            public decimal TransactionAmount { get; set; }

            /// <summary>
            /// Գործարքի Արժույթ
            /// </summary>
            public string Currency { get; set; }


            /// <summary>
            /// Գործարքի գումարը քարտի արժույթով Մուտք
            /// </summary>
            public decimal TransactionAmountSignIn { get; set; }


            /// <summary>
            /// Գործարքի գումարը քարտի արժույթով Ելք
            /// </summary>
            public decimal TransactionAmountOut { get; set; }

            /// <summary>
            /// Գործարքի վայրը/նկարագրություն
            /// </summary>
            public string OperationPlace { get; set; }

            /// <summary>
            /// Գործարքի տեսակը
            /// </summary>
            public string TransactionType { get; set; }

            #endregion
        }

        public CardStatement()
        {
            Transactions = new List<CardStatementDetail>();

        }

        public void GetFullCardStatement(string cardNumber, DateTime dateFrom, DateTime dateTo, byte language)
        {
            CardDB.GetFullCardStatement(this, cardNumber, dateFrom, dateTo, language);
        }
    }
}
