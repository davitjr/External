using ExternalBanking.DBManager;
using System.Collections.Generic;


namespace ExternalBanking
{
    public class SearchBudgetAccount
    {
        /// <summary>
        /// Փոխանցման ամսաթիվ
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Փոխանցողի տվյալներ
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Փոխանցողի հաշվեհամար
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Ստացողի տվյալներ
        /// </summary>
        public string CustomerType { get; set; }

        /// <summary>
        /// Ստացող բանկ
        /// </summary>
        public string IsLegal { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string IsEntrepreneur { get; set; }

        /// <summary>
        /// Ստացողի հաշվեհամար
        /// </summary>
        public string IsPhysical { get; set; }

        /// <summary>
        /// Ստացող բանկ Swift
        /// </summary>
        public string Soc { get; set; }

        /// <summary>
        /// Առաջին տողի համար
        /// </summary>
        public int BeginRow { get; set; }

        /// <summary>
        /// Վերջին տողի համար
        /// </summary>
        public int EndRow { get; set; }

        /// <summary>
        /// Տողերի քանակ
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        ///  Փոխանցումների որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<SearchBudgetAccount> Search(SearchBudgetAccount searchParams)
        {
            List<SearchBudgetAccount> budgetAccounts = SearchBudgetAccountDB.GetBudgetAccountsDB(searchParams);
            return budgetAccounts;
        }
    }
}
