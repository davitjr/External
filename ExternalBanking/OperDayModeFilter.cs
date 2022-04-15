using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class OperDayModeFilter
    {
        /// <summary>
        /// Որոնման ամսաթվի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Որոնման ամսաթվի ավարտ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        ///Օգտագործողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(տեսակները նկարագրված են ACCOPERBASE ի tbl_type_of_24_7_mode -ում)
        /// </summary>
        public OperDayModeType Option { get; set; }

        /// <summary>
        /// 24/7 Ռեժիմի պատմության որոնում
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<OperDayMode> SearchOperDayMode(OperDayModeFilter searchParams)
        {
            List<OperDayMode> operDayCheckings = OperDayModeFilterDB.GetOperDayModeHistory(searchParams);
            return operDayCheckings;
        }
    }
}
