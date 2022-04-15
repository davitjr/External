using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class CardUSSDServiceHistory
    {


        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        public ulong ProductID { get; set; }

        /// <summary>
        /// Բջջային հեռախոսահամար
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// 1-Գրանցել, 2- Հանել
        /// </summary>
        public short ActionType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// գործողության տեսակի նկարագրություն
        /// </summary>
        public string ActionTypeDescription { get; set; }

        /// <summary>
        /// Մուտքագրման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        public short? ArcaResponse { get; set; }
        /// <summary>
        /// ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// ARCA-ի պատասխան ֆայլի անունը
        /// </summary>
        public string RequestFileName { get; set; }

        public static List<CardUSSDServiceHistory> GetCardUSSDServiceHistory(ulong productID)
        {
            return CardUSSDServiceOrderDB.GetCardUSSDServiceHistory(productID);
        }
    }
}
