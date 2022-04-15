using System;

namespace ExternalBanking
{
    public class LoanStatementDetail
    {
        //Գործարքի ամսաթիվ	
        public DateTime OperationDate { get; set; }

        //Գործարքի գումար
        public double OperationAmount { get; set; }

        //Արժույթ
        public string OperationCurrency { get; set; }

        //Գործարքի նկարագրություն
        public string Description { get; set; }

        //Գործարքի նկարագրություն
        public string DebetCredit { get; set; }

        /// <summary>
        /// Գործարքի համար
        /// </summary>
        public ulong TransactionsGroupNumber { get; set; }

        //Ելքագրվող (դեբետային) հաշիվը
        //public string OperationAccountNumber { get; set; }

    }
}
