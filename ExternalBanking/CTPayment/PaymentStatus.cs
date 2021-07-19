using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Վճարման կարգավիճակ
    /// </summary>
    public class PaymentStatus
    {

        /// <summary>
        /// Վճարման կարգավիճակի կոդ
        /// </summary>
        public byte StatusCode { get; set; }

        /// <summary>
        /// Վճարման կարգավիճակի նկարագրություն
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Վճարման վերաբերյալ լրացուցիչ ինֆորմացիա
        /// </summary>
        public string AdditionalInformation { get; set; }

        /// <summary>
        /// Կարգավիճակի անցնելու ամսաթիվ, ժամ
        /// </summary>
        public DateTime StatusDateTime { get; set; }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում հայտի համարով
        /// </summary>
        /// <param name="paymentID">Գործարքի նույնականացուցիչը համակարգում </param>
        /// <returns></returns>
        public static PaymentStatus GetPaymentStatus(long paymentID)
        {
            return CTPaymentDB.GetPaymentStatus(paymentID);
        }

        /// <summary>
        /// Գործարքի կարգավիճակի հարցում տերմինալի կողմից գեներացված գործարքի համարով
        /// </summary>
        /// <param name="orderID">Վճարային տերմինալի կողմից գեներացված գործարքի նույնականացուցիչ</param>
        /// <returns></returns>
        public static PaymentStatus GetPaymentStatusByOrderID(long orderID)
        {
            return CTPaymentDB.GetPaymentStatusByOrderID(orderID);
        }
    }
}
