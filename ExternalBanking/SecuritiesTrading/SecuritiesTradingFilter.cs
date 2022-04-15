using System;

namespace ExternalBanking.SecuritiesTrading
{
    public class SecuritiesTradingFilter
    {
        /// <summary>
        /// Ամսաթվի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ամսաթվի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Հաճ. համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հայտի կարգավիճակ
        /// </summary>
        public OrderQuality Quality { get; set; }

        /// <summary>
        /// Հայտի ունիկալ համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ցուցադրվող էջ
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Հայտի տեսակ
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// Դասակարգում
        /// </summary>
        public SortBy Sort { get; set; }

        /// <summary>
        /// Պատվերի տեսակ
        /// </summary>
        public SecuritiesTradingOrderTypes TradingOrderType { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes SecurityType { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Թողարկման սերիա
        /// </summary>
        public int? IssueSeria { get; set; }


        /// <summary>
        /// Արժեթղթի ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Հանձնարարականի համար
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Հանձնարարականի ուժի մեջ լինելու տեսակ
        /// </summary>
        public SecuritiesTradingOrderExpirationType ExpirationType { get; set; }

        /// <summary>
        /// Բրոքերային կոդ
        /// </summary>
        public string BrokerageCode { get; set; }

        /// <summary>
        /// Բորսայական հապավում
        /// </summary>
        public string Ticker { get; set; }
    }

    public enum SortBy
    {
        NotDefined,
        AmountMinToMax,
        AmountMaxToMin,
        DateMinToMax,
        DateMaxToMin
    }
}
