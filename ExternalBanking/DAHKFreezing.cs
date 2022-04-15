using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class DAHKFreezing
    {
        /// <summary>
        /// Վարույթի համար
        /// </summary>
        public string InquestNumber { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double AmountAMD { get; set; }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ԴԱՀԿ սառեցումները և վարույթի համարները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<DAHKFreezing> GetDahkFreezings(ulong customerNumber)
        {
            return DahkDetailsDB.GetDahkFreezings(customerNumber);
        }
    }


}
