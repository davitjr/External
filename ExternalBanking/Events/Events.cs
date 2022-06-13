using ExternalBanking.DBManager.EventsDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.Events
{
    public class Events
    {
        public static List<Event> GetEventSubTypes(EventTypes eventTypes, Languages Language)
        {
            string cacheKey = "EventSubTypes_" + Language.ToString();
            List<Event> eventSubTypes = CacheHelper.Get<List<Event>>(cacheKey);
            if (eventSubTypes == null)
            {
                eventSubTypes = EventsDB.Get(eventTypes, Language);
                CacheHelper.Add(eventSubTypes, cacheKey);
            }
            return eventSubTypes;
        }
    }

    public class Event
    {
        /// <summary>
        /// Միջոցառման տեսակի ունիկալ համար
        /// </summary>
        public EventTypes Id { get; set; }

        /// <summary>
        /// Միջոցառման ենթատեսակի ունիկալ համար
        /// </summary>
        public int SubTypeId { get; set; }

        /// <summary>
        /// Միջոցառման տեսակի անվանում հայերեն
        /// </summary>
        public string EventTypeName { get; set; }

        /// <summary>
        /// Միջոցառման ենթատեսակի անվանում հայերեն
        /// </summary>
        public string EventSubTypeName { get; set; }

        /// <summary>
        /// Գին
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Զեղչի տոկոս
        /// </summary>
        public double DiscountRate { get; set; }

        /// <summary>
        /// Զեղչված գին
        /// </summary>
        public double DiscountedPrice { get; set; }

        /// <summary>
        /// 1 օգտագործողի մասով տվյալ ենթատեսակի միջոցառման տոմսերի առավելագույն քանակ
        /// </summary>
        public int MaxQuantityPerUser { get; set; }

        /// <summary>
        /// Տոմսի վաճառքի գումարի համար նախատեսված մուտքագրվով հաշիվ
        /// </summary>
        public string ReceiverAccount { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Միջոցառման ենթատեսակի հասանելիություն
        /// </summary>
        public bool IsActive { get; set; }

      
    }
}
