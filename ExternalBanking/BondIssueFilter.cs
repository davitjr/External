using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class BondIssueFilter
    {

        #region Properties
        /// <summary>
        /// Թողարկման պարտատոմսի ID
        /// </summary>
        public int BondIssueId { get; set; }
        /// <summary>
        /// Տվյալ թողարկման պարտատոմս ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Պարտատոմսի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Պարտատոմսի թողարկման ամսաթվի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Պարտատոմսի թողարկման ամսաթվի ավարտ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Պարտատոմսի կարգավիճակ
        /// </summary>
        public BondIssueQuality Quality { get; set; }

        /// <summary>
        /// Պարտատոմս թողարկող կազմակերպություններ
        /// </summary>
        public BondIssuerType IssuerType { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes ShareType { get; set; }

        /// <summary>
        /// Թողարկման սերիա
        /// </summary>
        public int? IssueSeria { get; set; }

        #endregion

        /// <summary>
        /// Պարտատոմսի որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<BondIssue> SearchBondIssues(BondIssueFilter searchParams)
        {
            List<BondIssue> bondIssues = BondIssueFilterDB.SearchBondIssue(searchParams);
            return bondIssues;
        }

    }
}
