using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class BondFilter
    {

        #region Properties
        /// <summary>
        /// Պարտատոմսի թողարկման ունիկալ համար
        /// </summary>
        public int BondIssueId { get; set; }

        /// <summary>
        /// Պարտատոմսի թողարկման ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Վաճառված պարտատոմսի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Վաճառված պարտատոմսի կարգավիճակ
        /// </summary>
        public BondQuality Quality { get; set; }

        /// <summary>
        /// Պարտատոմսի  ամսաթվի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Պարտատոմսի ամսաթվի ավարտ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes ShareType { get; set; }

        #endregion

    }

}
