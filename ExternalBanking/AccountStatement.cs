using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Անալիտիկ հաշվի քաղվածք
    /// </summary>
	public class AccountStatement
    {
        /// <summary>
        /// Հաշվի վերջնական մնացորդ հաշվի արժույթով
        /// </summary>
        public double FinalBalance { get; set; }

        /// <summary>
        /// Հաշվի սկզբնական մնացորդ հաշվի արժույթով
        /// </summary>
        public double InitialBalance { get; set; }

        /// <summary>
        /// Հաշվի կրեդիտային շրջանառություն հաշվի արժույթով
        /// </summary>
        public double TotalCreditAmount { get; set; }

        /// <summary>
        /// Հաշվի դեբետաային շրջանառություն հաշվի արժույթով
        /// </summary>
        public double TotalDebitAmount { get; set; }

        /// <summary>
        /// Հաշվի գործառնությունների ցուցակ
        /// </summary>
        public List<AccountStatementDetail> Transactions { get; set; }

        /// <summary>
        /// Հաշվի ամփոփ գումարների ցուցակ ըստ օրերի
        /// </summary>
        public List<AccountStatementTotalsByDays> TotalsByDays { get; set; }

        /// <summary>
        /// Էջերի քանակ
        /// </summary>
        public int PagesCount { get; set; }

        /// <summary>
        /// Հաշվի սինթետիկ համար
        /// </summary>
        public string SyntheticTypeOfAccount { get; set; }
        public AccountStatement()
        {
            Transactions = new List<AccountStatementDetail>();
            TotalsByDays = new List<AccountStatementTotalsByDays>();
        }
        
    }
}
