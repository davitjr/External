using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class UtilityOptionsFilter
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Ամսաթվի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ամսաթվի ավարտ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        ///Որոնվող աշխատակցի համար
        /// </summary>
        public int NumberOfSet { get; set; }

        /// <summary>
        /// Ստուգման տեսակ
        /// </summary>
        public CommunalTypes CommunalOptions { get; set; }


        public static List<UtilityOptions> SearchOperDayOptions(UtilityOptionsFilter searchParams)
        {
            List<UtilityOptions> utilityOptions = UtilityOptionsFilterDB.SearchUtilityOptions(searchParams);
            return utilityOptions;

        }

    }

}

