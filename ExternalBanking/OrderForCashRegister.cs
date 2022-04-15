using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class OrderForCashRegister
    {
        public Order Order { get; set; }
        /// <summary>
        /// Գործարքի տեսակ
        /// </summary>
        public string TypeDescription { get; set; }
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Հաշվեհամարի նկարագրությունը
        /// </summary>
        public string AccountNumberDescription { get; set; }
        /// <summary>
        /// Ազգանուն Անուն Հայրանուն
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Փաստաթուղթ
        /// </summary>
        public string DocumentNumber { get; set; }
        /// <summary>
        /// Ստեղծողի ՊԿ
        /// </summary>
        public int UserID { get; set; }
        public OrderForCashRegister()
        {
            Order = new Order();
        }

        /// <summary>
        /// Վերադարցնում է դրամարկղի գործարքներ բաժնի որոնման արդյունքները
        /// </summary>
        /// <param name="searchOrders"></param>
        /// <returns></returns>
        public static List<OrderForCashRegister> GetOrdersForCashRegister(SearchOrders searchOrders)
        {
            return OrderForCashRegisterDB.GetOrdersForCashRegister(searchOrders);
        }

    }
}
