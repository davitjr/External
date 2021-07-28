using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Ի պահ ընդունված արժեքներ
    /// </summary>
    public class SafekeepingItem
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Արժեքի տեսակ
        /// </summary>
        public ushort ItemType { get; set; }

        /// <summary>
        /// Արժեքի տեսակի նկարագրություն
        /// </summary>
        public string ItemTypeDescription { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Պարկի համար
        /// </summary>
        public string PacketNumber { get; set; }

        /// <summary>
        /// Սակագին
        /// </summary>
        public double Tariff { get; set; }

        /// <summary>
        /// Գանձման հաճախականություն
        /// </summary>
        public int Periodicity { get; set; }

        /// <summary>
        /// Պարտավորության գումար
        /// </summary>
        public double DebtAmount { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }
        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Վերդարձնում է հաճախորդի Ի պահ ընդունված արժեքները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<SafekeepingItem> GetSafekeepingItems(ulong customerNumber, ProductQualityFilter filter)
        {
            List<SafekeepingItem> items = new List<SafekeepingItem>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                items.AddRange(SafekeepingItemDB.GetSafekeepingItems(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                items.AddRange(SafekeepingItemDB.GetClosedSafekeepingItems(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                items.AddRange(SafekeepingItemDB.GetSafekeepingItems(customerNumber));
                items.AddRange(SafekeepingItemDB.GetClosedSafekeepingItems(customerNumber));
            }
            return items;

        }
        /// <summary>
        /// Վերդարձնում է հաճախորդի Ի պահ ընդունված արժեքը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static SafekeepingItem GetSafekeepingItem(ulong customerNumber, ulong productId)
        {
            return SafekeepingItemDB.GetSafekeepingItem( customerNumber,productId);
        }
    }
}
