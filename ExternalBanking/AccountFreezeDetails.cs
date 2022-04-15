using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի սառեցման գրություններ
    /// </summary>
    public class AccountFreezeDetails
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Մուտքագրման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Սառեցման պատճառ
        /// </summary>
        public ushort ReasonId { get; set; }

        /// <summary>
        /// Սառեցման պատճառի նկարագրություն
        /// </summary>
        public string ReasonDescription { get; set; }

        /// <summary>
        /// Հաշվի սառեցման ամսաթիվ
        /// </summary>
        public DateTime? FreezeDate { get; set; }

        /// <summary>
        /// Գումարի սառեցման ամսաթիվ
        /// </summary>
        public DateTime? AmountFreezeDate { get; set; }

        /// <summary>
        /// Սառեցված գումար
        /// </summary>
        public double FreezeAmount { get; set; }

        /// <summary>
        /// Սառեցնողի համար
        /// </summary>
        public long FreezeUserId { get; set; }

        /// <summary>
        /// Սառեցման նկարագրություն
        /// </summary>
        public string FreezeDescription { get; set; }

        /// <summary>
        /// Փակման ամսաթիվ
        /// </summary>
        public DateTime? UnfreezeDate { get; set; }

        /// <summary>
        /// Փակողի համար
        /// </summary>
        public long UnfreezeUserId { get; set; }

        /// <summary>
        /// Փակման պատճառ
        /// </summary>
        public ushort UnfreezeReasonId { get; set; }

        /// <summary>
        /// Փակման պատճառի նկարագրություն
        /// </summary>
        public string UnfreezeReasonDescription { get; set; }

        /// <summary>
        /// Փակման նկարագրություն
        /// </summary>
        public string UnfreezeDescription { get; set; }

        /// <summary>
        /// Սառեցման օրը ըստ գրության
        /// </summary>
        public DateTime? FreezeDateByDocumnet { get; set; }


        /// <summary>
        /// Հաշվի սառեցման մանրամասներ
        /// </summary>
        /// <param name="freezeId"></param>
        /// <returns></returns>
        public static AccountFreezeDetails GetAccountFreezeDetails(string freezeId)
        {
            AccountFreezeDetails freezeDetails = AccountFreezeDB.GetAccountFreezeDetails(freezeId);
            Localization.SetCulture(freezeDetails, new Culture((Languages)1));
            return freezeDetails;
        }


        /// <summary>
        /// Հաշվի սառեցումներ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static List<AccountFreezeDetails> GetAccountFreezeHistory(string accountNumber, ushort freezeStatus, ushort reasonId)
        {
            List<AccountFreezeDetails> freezeHistory = AccountFreezeDB.GetAccountFreezeHistory(accountNumber, freezeStatus, reasonId);
            Localization.SetCulture(freezeHistory, new Culture((Languages)1));
            return freezeHistory;
        }


    }
}
