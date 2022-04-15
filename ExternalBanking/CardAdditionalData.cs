using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class CardAdditionalData
    {
        public int CardAdditionalDataID { get; set; }
        public int AdditionID { get; set; }
        public string AdditionDescription { get; set; }
        public string AdditionValue { get; set; }
        public int SetNumber { get; set; }

        public static List<CardAdditionalData> GetCardAdditionalDatas(string cardnumber, string expirydate)
        {
            return CardAdditionalDataDB.GetCardAdditionalDatas(cardnumber, expirydate);
        }

    }
}
