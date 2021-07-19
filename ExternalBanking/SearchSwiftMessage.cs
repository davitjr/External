using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;


namespace ExternalBanking
{
    /// <summary>
    /// Swift հաղորդագրության որոնում
    /// </summary>
    public class SearchSwiftMessage
    {

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Գրանցման ա/թ
        /// </summary>
        public DateTime? RegistartionDate { get; set; }

        /// <summary>
        /// Ֆիլտրում ա/թ ի սկիզբ
        /// </summary>
        public DateTime? DateFrom { get; set; }


        /// <summary>
        /// Ֆիլտրում ա/թ ի վերջ
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Հաստատման ա/թ
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>
        /// Swift կոդ
        /// </summary>
        public string SwiftCode { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության ուղղություն ` ելքային կամ մուտքային
        /// </summary>
        public int? InputOutput { get; set; }


        /// <summary>
        /// SWIFT հաղորդագրության MT (SWIFT կոդավորման) տեսակ 
        /// </summary>
        public int MtCode { get; set; }

        /// <summary>
        /// Հեռացված է թե ոչ
        /// </summary>
        public bool Deleted { get; set; }


        /// <summary>
        /// Գործարքի համարը
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// Հղման համար
        /// </summary>
        public string ReferanceNumber { get; set; }

        /// <summary>
        /// Ֆայլի առկայություն
        /// </summary>
        public int? MessageAttachment { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության ներքին հաշվառման տեսակ
        /// </summary>
        public int? MessageType { get; set; }


        /// <summary>
        /// Վերադարձնում է  Swift հաղորդագրությունները կախված ֆիլտրի պարամետրերից
        /// </summary>
        /// <returns></returns>
        public List<SwiftMessage> GetSearchedSwiftMessages()
        {
            return SearchSwiftMessageDB.GetSearchedSwiftMessages(this);
        }

    }
}
