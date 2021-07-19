using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;


namespace ExternalBanking
{
    public class CustomerDebts
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public string ObjectNumber { get; set; }
        /// <summary>
        /// Պարտավորության չափը
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// Գումարի նկարագրություն
        /// </summary>
        public string AmountDescription { get; set; }
        /// <summary>
        /// Պարտավորության Արժույթը
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Պարտավորության տեսակը
        /// </summary>
        public string DebtDescription { get; set; }
        /// <summary>
        /// Հնարավոր է ձևակերպել թե ոչ
        /// </summary>
        public short AlowTransaction { get; set; }


        public DebtTypes DebtType { get; set; }

    
        public static List<CustomerDebts> GetCustomerDebts(ulong customerNumber)
        {
            return CustomerDebtsDB.GetCustomerDebts(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի սպասարկման գծով պարտքը
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="DebtType">Պարտքի տեսակ</param>
        /// <returns></returns>
        public static double GetCustomerServiceFeeDebt(ulong customerNumber,DebtTypes DebtType)
        {
            return CustomerDebtsDB.GetCustomerServiceFeeDebt(customerNumber, DebtType);
        }
    }
}
