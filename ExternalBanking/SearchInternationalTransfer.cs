using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class SearchInternationalTransfer
    {
        /// <summary>
        /// Փոխանցման ամսաթիվ
        /// </summary>
        public DateTime DateOfTransfer { get; set; }

        /// <summary>
        /// Փոխանցողի տվյալներ
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Փոխանցողի հաշվեհամար
        /// </summary>
        public string SenderAccNumber { get; set; }

        /// <summary>
        /// Ստացողի տվյալներ
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// Ստացող բանկ
        /// </summary>
        public string ReceiverBank { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPayment { get; set; }

        /// <summary>
        /// Ստացողի հաշվեհամար
        /// </summary>
        public string ReceiverAccount { get; set; }

        /// <summary>
        /// Ստացող բանկ Swift
        /// </summary>
        public string ReceiverBankSwift { get; set; }

        /// <summary>
        /// Միջնորդ բանկ Swift
        /// </summary>
        public string IntermidateBankSwift { get; set; }

        /// <summary>
        /// Միջնորդ բանկ
        /// </summary>
        public string IntermidateBank { get; set; }

        /// <summary>
        /// Ստացող բանկ լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverBankAddInf { get; set; }

        /// <summary>
        /// Ստացողի Swift
        /// </summary>
        public string ReceiverSwift { get; set; }

        /// <summary>
        /// Ուղարկողի հասցե
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Ուղարկողի հեռախոս
        /// </summary>
        public string SenderPhone { get; set; }

        /// <summary>
        /// Այլ բանկում հաշվեհամար
        /// </summary>
        public string SenderOtherBankAccount { get; set; }

        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverAddInf { get; set; }

        /// <summary>
        /// Ստացողի տեսակ
        /// </summary>
        public string ReceiverType { get; set; }

        /// <summary>
        /// ԲԻԿ
        /// </summary>
        public string BIK { get; set; }

        /// <summary>
        /// ԿՊՊ
        /// </summary>
        public string KPP { get; set; }

        /// <summary>
        /// ԻՆՆ
        /// </summary>
        public string INN { get; set; }

        /// <summary>
        /// Թղթակցային հաշիվ
        /// </summary>
        public string CorrAccount { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPaymentRUR1 { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPaymentRUR2 { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPaymentRUR3 { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPaymentRUR4 { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPaymentRUR5 { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string DescriptionForPaymentRUR6 { get; set; }

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
        /// Երկիր
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        ///  Փոխանցումների որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<SearchInternationalTransfer> Search(SearchInternationalTransfer searchParams)
        {
            List<SearchInternationalTransfer> internationalTransfers = SearchInternationalTransferDB.GetInternationalTransfersDB(searchParams);
            return internationalTransfers;
        }
    }
}
