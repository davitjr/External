using System;

namespace ExternalBanking
{
    public class CardStatementDetail
    {
        //Գործարքի ամսաթիվ	
        public DateTime OperationDate { get; set; }

        //Գործարքի գումար
        public double OperationAmount { get; set; }

        //Արժույթ
        public string OperationCurrency { get; set; }

        //Գումար
        public double Amount { get; set; }

        //Գործարքի տեսակ մուտք/ելք
        public byte DebitCredit { get; set; }

        //Միջնորդավճար	
        public double CommissionFee { get; set; }

        //Միջնորդավճար	
        public string CommissionFeeString { get; set; }

        //Գործարքի նկարագրություն
        public string Description { get; set; }

        //Ձևակերպման ամսաթիվ
        public DateTime TransactionDate { get; set; }

        //Գործարքը կատարած քարտի համարը
        public string OperationCardNumber { get; set; }

        /// <summary>
        /// Ձևակերպման ա/թ և ժամ
        /// </summary>
        public DateTime DateOfVale { get; set; }

        /// <summary>
        /// Գործարքի վայրը
        /// </summary>
        public short Filial { get; set; }

        /// <summary>
        /// Հաշվի վերջնական մնացորդ
        /// </summary>
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// Կիրառվող փոխարժեք
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// CashBack
        /// </summary>
        public decimal CashBack { get; set; }

        /// <summary>
        /// MR Միավոր
        /// </summary>
        public decimal MR { get; set; }

        /// <summary>
        /// Գործարքի գումար հաշվի արժույթով
        /// </summary>
        public decimal? OperationAmountCardCurrency { get; set; }


        /// <summary>
        /// Ցուցադրել MR միավորը թե Cashback-ը
        /// </summary>
        public bool HasMR { get; set; }

        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int ItemNumber { get; set; }
    }
}
