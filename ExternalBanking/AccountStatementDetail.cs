using System;

namespace ExternalBanking
{
    /// <summary>
    /// Քաղվածքի մեկ գործարքի տվյալներ
    /// </summary>
	public class AccountStatementDetail
    {

        /// <summary>
        /// Գործարքի գումար գործարքի արժույթով
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Գործարքի գումար ազգային արժույթով (AMD)
        /// </summary>
        public double AmountBase { get; set; }

        /// <summary>
        /// Գործարքի թղթակից հաշիվ 
        /// </summary>
        public string CorrespondentAccount { get; set; }

        /// <summary>
        /// Գործարքի տեսակ 'd' -դեբետային, 'c' -կրեդիտային 
        /// </summary>
        public char DebitCredit { get; set; }

        /// <summary>
        /// Գործարքի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Գործարքի կատարման ամսաթիվ
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Գործարքի փասթաթղթի համար
        /// </summary>
        public int CashOperationNumber { get; set; }

        /// <summary>
        /// Գործարքի փասթաթղթի տեսակ
        /// </summary>
        public short CurrentAccountNumber { get; set; }
        /// <summary>
        /// Հիմնական Դեբետ հաշիվ / փոխանցող
        /// </summary>
        public string MainDebAccount { get; set; }
        /// <summary>
        /// հիմնական կրեդիտ հաշիվ / ստացող
        /// </summary>
        public string MainCredAccount { get; set; }

        /// <summary>
        /// Գործարքի տեսակի ներքին կոդավորում
        /// </summary>
        public int OperationType { get; set; }

        /// <summary>
        /// Ստացողի\Ուղարկողի անվանում
        /// </summary>
        public string Correspondent { get; set; }

        /// <summary>
        /// Գործարք կատարող ՊԿ
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Գործարքի համար
        /// </summary>
        public double TransactionsGroupNumber { get; set; }

        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int ItemNumber { get; set; }
    }

    /// <summary>
    /// Քաղվածքի ամփոփ տվյալներ ըստ օրերի
    /// </summary>
    public class AccountStatementTotalsByDays
    {
        /// <summary>
        /// Ամփոփ տվյալի օրը
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Քաղվածքի մեկ օրվա դեբետային շրջանառության գումար արտարժույթով 
        /// </summary>
        public double DayTotalDebetAmount { get; set; }

        /// <summary>
        /// Քաղվածքի մեկ օրվա դեբետային շրջանառության գումար ազգային արժույթով (AMD) 
        /// </summary>
        public double DayTotalDebetAmountBase { get; set; }


        /// <summary>
        /// Քաղվածքի մեկ օրվա կրեդիտային շրջանառության գումար արտարժույթով
        /// </summary>
        public double DayTotalCreditAmount { get; set; }

        /// <summary>
        /// Քաղվածքի մեկ օրվա կրեդիտային շրջանառության գումար ազգային արժույթով (AMD) 
        /// </summary>
        public double DayTotalCreditAmountBase { get; set; }
        
    }
}
