using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking.SecuritiesTrading
{
    public class SecuritiesTrading 
    {
        /// <summary>
        /// վերադարձնում է նշված ֆիլտեր-ով սահմանված հայտերի քանակը և մաքսիմում 100 հայտը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Dictionary<int, List<SentSecuritiesTradingOrder>> GetSentSecuritiesTradingOrders(SecuritiesTradingFilter filter) 
            => SecuritiesTradingDB.GetSentSecuritiesTradingOrders(filter);

    }

    public class SentSecuritiesTradingOrder
    {
        /// <summary>
        /// Հայտի գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Հայտի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Հայտի գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Հայտի տեսակ
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// Հայտի տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Հայտի կարգավիճակ
        /// </summary>
        public OrderQuality Quality { get; set; }

        /// <summary>
        /// Հայտի կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Հայտի 
        /// </summary>
        public SourceType Source { get; set; }

        /// <summary>
        /// Հայտի ունիկալ համար
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի ԱԱՀ
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Կատարվող մասնաճուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Հերթական համար
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Արժեթղթի ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Արժեթղթերի քանակ
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Պատվերի տեսակ
        /// </summary>
        public SecuritiesTradingOrderTypes TradingOrderType { get; set; }

        /// <summary>
        /// Պատվերի տեսակ
        /// </summary>
        public string TradingOrderTypeDescription { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes SecurityType { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public string SecurityTypeDescription { get; set; }

        /// <summary>
        /// Թողարկման սերիա
        /// </summary>
        public int IssueSeria { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշիվ
        /// </summary>
        public DepositaryAccount DepositoryAccount { get; set; }

        /// <summary>
        /// Հանձնարարականի համար
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Հանձնարարականի ուժի մեջ լինելու տեսակ
        /// </summary>
        public SecuritiesTradingOrderExpirationType ExpirationType { get; set; }

        /// <summary>
        /// Գործարքի ժամկետի նկարագրություն
        /// </summary>
        public string ExpirationTypeDescription { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված արժեթղթի գին լիմիտային / ստոպ լիմիտային գործարքի դեպքում
        /// </summary>
        public double LimitPrice { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված ստոպ գին ՍՏՈՊ / ստոպ լիմիտային գործարքի դեպքում
        /// </summary>
        public double StopPrice { get; set; }

        /// <summary>
        /// SecuritiesMarketTradingOrder Տեսակի հայտերի քանակ (quantity) դաշտի համագումարը
        /// </summary>
        public int MarketTradedQuantity { get; set; }

        /// <summary>
        /// Բրոքերային կոդ
        /// </summary>
        public string BrokerageCode { get; set; }

        /// <summary>
        /// Բորսայական հապավում
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Հայտի փոփոխման ամսաթիվ
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// Հայտի փոփոխման ամսաթիվ
        /// </summary>
        public bool IsDeposited { get; set; }

        /// <summary>
        /// Շուկայական արժեք
        /// </summary>
        public double MarketPrice { get; set; }

    }
}
