using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;


namespace ExternalBanking
{
    public class LoanInterestRateChangeHistory
    {
        /// <summary>
        /// Տոկոսադրույքի փոփոխման ամսաթիվ
        /// </summary>
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// Հին տոկոսադրույք
        /// </summary>
        public float InterestRateOld { get; set; }

        /// <summary>
        /// Նոր տոկոսադրույք
        /// </summary>
        public float InterestRateNew { get; set; }

        public static List<LoanInterestRateChangeHistory> GetLoanInterestRateChangeHistoryDetails(ulong productID)
        {
            return LoanInterestRateChangeHistoryDB.GetLoanInterestRateChangeHistoryDetails(productID);
        }

    }
}
