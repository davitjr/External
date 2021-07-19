using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class SearchCards
    {   
        /// <summary>
        /// Որոնվող քարի մասնաճյուղ
        /// </summary>
        public int filialCode { get; set; }

        /// <summary>
        /// Որոնվող քարի հաճախորդի համար
        /// </summary>
        public string customerNumber { get; set; }

        /// <summary>
        /// Որոնվող քարի համար
        /// </summary>
        public string cardNumber { get; set; }

        /// <summary>
        /// Որոնվող քարի համար
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Որոնել նաև փակված քարտերի մեջ
        /// </summary>
        public bool includeCloseCards { get; set; }
                
        /// <summary>
        /// Քարտի(երի) որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<SearchCardResult> Search(SearchCards searchParams)
        {
            List<SearchCardResult> cards = SearchCardsDB.SearchCards(searchParams);
            Localization.SetCulture(cards,new Culture(Languages.hy));
            return cards;
        }

    }
}
