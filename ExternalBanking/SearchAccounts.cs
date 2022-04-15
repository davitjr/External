using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class SearchAccounts
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string accountNumber { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public string customerNumber { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Հին Բալանսային հաշիվ
        /// </summary>
        public string sintAcc { get; set; }

        /// <summary>
        /// Բալանսային հաշիվ
        /// </summary>
        public string sintAccNew { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int filialCode { get; set; }

        /// <summary>
        /// Թղթակցային հաշիվ 
        /// </summary>
        public bool isCorAcc { get; set; }

        /// <summary>
        /// Փակված հաշիվներ
        /// </summary>
        public bool includeClosedAccounts { get; set; }


        /// <summary>
        /// Որոնվող հաշվեհամարների ցուցակ
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public static List<SearchAccountResult> GetSearchedAccounts(SearchAccounts serachParams, ACBAServiceReference.User currentUser)
        {
            List<SearchAccountResult> result = SearchAccountsDB.SearchAccounts(serachParams, currentUser);

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
            //return result.Distinct().ToList();
        }

    }


}

