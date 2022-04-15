using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class SearchCashBook
    {
        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int? FillialCode { get; set; }

        /// <summary>
        /// Գրանցման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Աշխատակցի համար
        /// </summary>
        public int? RegisteredUserID { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Տեսակ
        /// </summary>
        public int? RowType { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public int? Quality { get; set; }

        /// <summary>
        /// Մուտք/Ելք
        /// </summary>
        public int? OperationType { get; set; }
        /// <summary>
        ///Որոնվող աշխատակցի համար
        /// </summary>
        public int? SearchUserID { get; set; }

        /// <summary>
        /// Ստանում է searchParams կրիտերիաին բավարարող բոլոր գրությունները
        /// </summary>
        /// <param name="cashBookParams"></param>
        /// <returns></returns>
        public static List<CashBook> GetCashBooks(SearchCashBook searchParams, ExternalBanking.ACBAServiceReference.User user)
        {

            if (user.IsChiefAcc || user.AdvancedOptions["isHeadCashBook"] == "1" || user.isOnlineAcc ||
                (user.AdvancedOptions["canApproveCashBookSurplusDeficit"] == "1" && user.AdvancedOptions["isEncashmentDepartment"] == "1"))
            {
                searchParams.RegisteredUserID = null;
            }
            return CashBookDB.GetCashBooks(searchParams);
        }
    }
}
