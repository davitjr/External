using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Data;

namespace ExternalBanking
{
    public class SearchTransferBankMail
    {
        /// <summary>
        /// Փոխանցման ամսաթիվ
        /// </summary>
        public DateTime DateOfTransfer { get; set; }

        /// <summary>
        /// Փոխանցողի հաշվեհամար
        /// </summary>
        public string SenderAccount { get; set; }

        /// <summary>
        /// Ստացողի հաշվեհամար
        /// </summary>
        public string ReceiverAccount { get; set; }

        /// <summary>
        /// Ստացողի տվյալներ
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// Ստացողի բանկ
        /// </summary>
        public string ReceiverBank { get; set; }

        /// <summary>
        /// ՏՀՏ
        /// </summary>
        public string SenderRegCode { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPayment { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Ընթացիկ/Արխիվ
        /// </summary>
        public int IsArchive { get; set; }

        /// <summary>
        /// Բյուջետային հաշիվներ
        /// </summary>
        public int IsBudget { get; set; }

        /// <summary>
        /// Առաջին տողի համար
        /// </summary>
        public int BeginRow { get; set; }

        /// <summary>
        /// Վերջին տողի համար
        /// </summary>
        public int EndRow { get; set; }

        /// <summary>
        /// Տողերի քանակ
        /// </summary>
        public int RowCount { get; set; }
        /// <summary>
        /// Փոխանցումների խումբը
        /// </summary>
        public ushort TransferGroup { get; set; }
        /// <summary>
        ///  Փոխանցումների որոնում տրված պարամետրերով (4-միջմասնաճուղային փոխանցման խումբ)
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<SearchTransferBankMail> Search(SearchTransferBankMail searchParams)
        {
            List<SearchTransferBankMail> transfersBankMail = SearchTransferBankMailDB.GetTransfersBankMailDB(searchParams);
            return transfersBankMail;
        }
    }
}
