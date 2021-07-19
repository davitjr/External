using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.Interfaces;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class PreOrder 
    {
        /// <summary>
        /// Նախնական հայտի համար
        /// </summary>
        public int PreOrderID { get; set; }
        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        /// Նախնական հայտի տեսակ
        /// </summary>
        public PreOrderType PreOrderType { get; set; }
        /// <summary>
        /// Նախնական հայտի կարգավիճակ
        /// </summary>
        public PreOrderQuality PreOrderQuality { get; set; }
        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int OperationFilialCode { get; set; }

        /// <summary>
        /// Որոնում է նախնական հայտի մանրամասները
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="RowCount"></param>
        /// <returns></returns>
        public static List<PreOrderDetails> GetSearchedPreOrderDetails(SearchPreOrderDetails searchParams, out int RowCount)
        {
            List<PreOrderDetails> list = PreOrderDB.GetSearchedPreOrderDetails(searchParams, out RowCount);
            return list;
        }
        /// <summary>
        /// Կատարում է նախնական հայտի մանրամասն տողի կարգավիճակի թարմացում 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="appID"></param>
        /// <param name="quality"></param>
        public static void UpdatePreOrderDetailQuality(ulong customerNumber,ulong appID, PreOrderQuality quality)
        {
             PreOrderDB.UpdatePreOrderDetailQuality(customerNumber, appID, quality);
        }
        /// <summary>
        /// Հեռացնում է նախորդ խմբաքանակի չձևավորված բոլոր հայտերը
        /// </summary>
        public static ActionResult ResetIncompletePreOrderDetailQuality()
        {
            ActionResult result = PreOrderDB.ResetIncompletePreOrderDetailQuality();

            if (result.ResultCode != ResultCode.Normal)
            {
                result.ResultCode = ResultCode.Failed;
            }

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }
        /// <summary>
        /// Վերադարձնում է չձևավորված ապառիկ հայտերի քանակը
        /// </summary>
        public static int GetIncompletePreOrdersCount()
        {
            return PreOrderDB.GetIncompletePreOrdersCount();
        }
    }
}
