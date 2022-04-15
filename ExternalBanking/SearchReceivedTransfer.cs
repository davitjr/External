using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class SearchReceivedTransfer
    {
        /// <summary>
        /// Փոխանցման ամսաթիվ
        /// </summary>
        public DateTime DateTransfer { get; set; }

        /// <summary>
        /// Փոխանցողի տվյալներ
        /// </summary>
        public string SenderName { get; set; }


        /// <summary>
        /// Ստացողի տվյալներ
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// Ստացող բանկ
        /// </summary>
        public string ReceiverPassport { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }


        /// <summary>
        /// Փոխանցման կոդ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Երկիր
        /// </summary>
        public string Country { get; set; }



        public double ChargeW { get; set; }
        public double RateW { get; set; }
        public double TotalAmount { get; set; }
        public double AcbaFee { get; set; }
        public double ProfitPercent { get; set; }
        /// <summary>
        /// Ուղարկողի հսացե
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Ուղարկողի հեռախոս
        /// </summary>
        public string SenderPhone { get; set; }

        /// <summary>
        /// Ստացողի հասցե
        /// </summary>
        public string ReceiverAddress { get; set; }

        /// <summary>
        /// Ստացողի հեռախոս
        /// </summary>
        public string ReceiverPhone { get; set; }

        /// <summary>
        /// Ֆայլի անուն
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Փոխանցման տեսակ
        /// </summary>
        public short TransferType { get; set; }


        /// <summary>
        /// Տողերի քանակ
        /// </summary>
        public int RowCount { get; set; }


        /// <summary>
        /// Երկրի կոդ
        /// </summary>
        public string CountryCode { get; set; }




        /// <summary>
        ///  Փոխանցումների որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<SearchReceivedTransfer> Search(SearchReceivedTransfer searchParams, ushort FilialCode)
        {
            List<SearchReceivedTransfer> ReceivedTransfers = SearchReceivedTransferDB.GetReceivedTransfersDB(searchParams, FilialCode);
            return ReceivedTransfers;
        }
    }
}
