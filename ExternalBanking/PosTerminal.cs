using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExternalBanking
{
    public class PosTerminal
    {
        /// <summary>
        ///POS տերմինալի ունիկալ համար
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Անվանում
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// POS տերմինալի համար
        /// </summary>
        public string TerminalID { get; set; }
        /// <summary>
        /// Amex Merchant ID
        /// </summary>
        public string AmexMID { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public ushort Type { get; set; }
        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Սեփականատեր բանկ
        /// </summary>
        public int OwnerBankCode { get; set; }

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Ստուգում է տվյալ հաճախորդը ունի POS տերմինալներ 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static bool HasPosTerminal(ulong customerNumber)
        {
            return PosTerminalDB.HasPosTerminal(customerNumber);
        }


        public static List<PosRate> GetPosRates(int terminalId)
        {

            return PosTerminalDB.GetPosRates(terminalId); 
        }

        public static List<PosCashbackRate> GetPosCashbackRates(int terminalId)
        {

            return PosTerminalDB.GetPosCashbackRates(terminalId); 
        }

        public string GetStatement(string cardAccount, DateTime dateFrom, DateTime dateTo, byte option)
        {
            MerchantStatement merchantSt = PosTerminalDB.GetStatement(cardAccount, dateFrom, dateTo, option);


            string XMLString = "";

            StringWriter stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(MerchantStatement));
            serializer.Serialize(stringWriter, merchantSt);
            XMLString = stringWriter.ToString();

            return XMLString;

        }

    }
}
