using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի բացման/փակման պատմություն
    /// </summary>
    public class AccountOpeningClosingDetail
    {
        /// <summary>
        ///  Գործողություն կատարած ՊԿ
        /// </summary>
        public int ActionSetNumber { get; set; }


        /// <summary>
        /// Փակման ամսաթիվ
        /// </summary>
        public DateTime? ActionDate { get; set; }

        /// <summary>
        /// Գործողության պատճառ
        /// </summary>
        public string ActionDescription { get; set; }


        /// <summary>
        ///Վերադարձնում է հաշվեհամարով հաշվի փակման բացման գործողությունները 
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static List<AccountOpeningClosingDetail> GetAccountOpeningClosingDetails(string accountNumber)
        {
            return AccountDB.GetAccountOpeningClosingDetails(accountNumber);
        }

        public static AccountOpeningClosingDetail GetAccountOpeningDetail(string accountNumber)
        {
            return AccountDB.GetAccountOpeningDetail(accountNumber);
        }


    }
}
