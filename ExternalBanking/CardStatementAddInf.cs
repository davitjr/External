using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExternalBanking.CardStatement;

namespace ExternalBanking
{
    public class CardStatementAddInf
    {
        /// <summary>
        /// Տուժանք Summery
        /// </summary>
        public decimal WholePenalty { get; set; }

        /// <summary>
        /// Տուժանք Summery
        /// </summary>
        public decimal WholeInterest { get; set; }

        /// <summary>
        /// Միջնորդավճարներ (գործարքներից և այլ)
        /// </summary>
        public decimal WholeFee { get; set; }


        /// <summary>
        /// Միջնորդավճարներ (գործարքներից և այլ)
        /// </summary>
        public decimal AvailableAmountTotal { get; set; }

        /// <summary>
        /// Ադդ քարդ Summery
        /// </summary>
        public string AddCard { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public DateTime OperDay { get; set; }

        /// <summary>
        /// Card-ի համար
        /// </summary>
        public int CardSystemId { get; set; }

        /// <summary>
        /// Անուն Ազգանուն հայրանուն 
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Քարտի տեսակ
        /// </summary>
        public int CardType { get; set; }

        /// <summary>
        /// Հաշվի արժույթ
        /// </summary>
        public string AccountCurrency { get; set; }


        /// <summary>
        /// Հաշվի համար
        /// </summary>
        public string CardAccount { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Վերջնական մնացորդ 
        /// </summary>
        public decimal ArcaLimitEnd { get; set; }

        /// <summary>
        /// Վերջնական մնացորդ 
        /// </summary>
        public decimal ArcaLimitStart { get; set; }


        /// <summary>
        /// Վերջնական մնացորդ 
        /// </summary>
        public decimal StartCapital { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public int CardGroupHead { get; set; }

        public List<MRCardStatement> MRCardStatementTransactions { get; set; }


        public List<SummaryCreditLineCardStatement> SummaryCreditLineCardStatementTransactions { get; set; }


        public List<LinkCardStatement> LinkCardStatementTransactions { get; set; }


        public List<SummaryBonusCardStatement> SummaryBonusCardStatementTransactions { get; set; }


        public List<AddInfoCardStatement> AddInfoCardStatementTransactions { get; set; }


        public List<UnsettledTransactions> UnsettledTransactionsTransactions { get; set; }

        public CardStatement MainCardStatement { get; set; }
    }
}
