using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class ENAPayments
    {
        /// <summary>
        /// Վճարման ամսաթիվ
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Վճարված գումար
        /// </summary>
        public double PaidAmount { get; set; }

        /// <summary>
        /// Վճարման թերեիկի համար
        /// </summary>
        public int PaymentOrderNumber { get; set; }


        /// <summary>
        /// Բանկի մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// ՀԵՑ մասնաճյուղ
        /// </summary>
        public string Branch { get; set; }


        public static List<ENAPayments> GetENAPayments(string abonentNumber, string branch)
        {
            return ENAPaymentsDB.GetENAPayments(abonentNumber, branch);
        }

        public static List<DateTime> GetENAPaymentDates(string abonentNumber, string branch)
        {
            return ENAPaymentsDB.GetENAPaymentDates(abonentNumber, branch);
        }

    }
}
