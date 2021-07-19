using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class PlasticCardSMSServiceHistory
    {
        /// <summary>
        /// Բջջային հեռախոսահամար
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        ///   SMS-ի տեսակ
        /// </summary>
        public short SMSType { get; set; }

        /// <summary>
        /// SMS-ի նվազագույն գումար
        /// </summary>
        public string SMSFilter{ get; set; }


        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        public ulong ProductID { get; set; }

        /// <summary>
        /// SMS ծառայության գրանցման գործողության տեսակի նկարագրություն(1-գրանցել, 2-դադարեցնել, 3-փոփոխել)
        /// </summary>
        public string OperationTypeDescription { get; set; }
        /// <summary>
        /// Կատարման ա/թ
        /// </summary>
        public DateTime RegDate { get; set; }
        /// <summary>
        /// SMS-ի տեսակ և նվազագույն գումար
        /// </summary>
        public string SmsTypeAndSum { get; set; }

        /// <summary>
        /// Ուղարկված ֆայլ
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Ուղարկված ֆայլ
        /// </summary>
        public string ArcaAnswer { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        public static List<PlasticCardSMSServiceHistory> GetPlasticCardAllSMSServiceHistory(ulong curdNumber)
        {
            return PlasticCardSMSServiceOrderDB.GetPlasticCardAllSMSServiceHistory(curdNumber);
        }


        public static PlasticCardSMSServiceHistory GetPlasticCardSMSServiceHistory(string CardNumber)
        {
            PlasticCardSMSServiceHistory plasticCardSMSServiceHistory = new PlasticCardSMSServiceHistory();
            string smsTypeAndValue = PlasticCardSMSServiceOrderDB.SMSTypeAndValue(CardNumber);
            if (smsTypeAndValue != string.Empty)
            {
                string type = smsTypeAndValue.Substring(0, 1);
                switch (type)
                {
                    case "M":
                        plasticCardSMSServiceHistory.SMSType = 1;
                        break;
                    case "B":
                        plasticCardSMSServiceHistory.SMSType = 2;
                        break;
                    case "A":
                        plasticCardSMSServiceHistory.SMSType = 3;
                        break;
                    case "E":
                        plasticCardSMSServiceHistory.SMSType = 4;
                        break;
                    case "W":
                        plasticCardSMSServiceHistory.SMSType = 5;
                        break;
                    case "H":
                        plasticCardSMSServiceHistory.SMSType = 6;
                        break;
                    case "C":
                        plasticCardSMSServiceHistory.SMSType = 7;
                        break;
                    case "D":
                        plasticCardSMSServiceHistory.SMSType = 8;
                        break;
                    case "F":
                        plasticCardSMSServiceHistory.SMSType = 9;
                        break;
                    default:
                        plasticCardSMSServiceHistory.SMSType = -1;
                        break;
                }
                plasticCardSMSServiceHistory.SMSFilter = smsTypeAndValue.Substring(1, smsTypeAndValue.Length - 1);
            }
            else
            {
                plasticCardSMSServiceHistory.SMSType = -1;
                plasticCardSMSServiceHistory.SMSFilter = "0";
            }
            plasticCardSMSServiceHistory.MobilePhone = PlasticCardSMSServiceOrderDB.GetCardMobilePhone(CardNumber);
            plasticCardSMSServiceHistory.ProductID = (ulong)CardDB.GetCard(CardNumber).ProductId;

            return plasticCardSMSServiceHistory;
        }
    }
}

