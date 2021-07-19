using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class OperDayOptionsFilter
    {
        /// <summary>
        /// Id
        /// </summary>
        public int CheckingId { get; set; }
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
        public int SearchUserID { get; set; }

        /// <summary>
        /// Գործառնական օր
        /// </summary>
        public DateTime OperDay { get; set; }

        /// <summary>
        /// Ստուգման տեսակ
        /// </summary>
        public OperDayOptionsType Option { get; set; }


        public static List<OperDayOptions> SearchOperDayOptions(OperDayOptionsFilter searchParams)
        {
            List<OperDayOptions> operDayCheckings = OperDayOptionsFilterDB.SearchOperDayOptions(searchParams);
            return operDayCheckings;

        }

    }
}
