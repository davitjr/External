using System.Collections.Generic;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
   public class Accreditive:Loan
    {
        /// <summary>
        /// Բենեֆիցիար
        /// </summary>
        public string Benefeciar { get; set; }

        /// <summary>
        /// Կոմիսիոն վճարի գանձման եղանակ
        /// </summary>
        public ushort PercentCummulation { get; set; }

        /// <summary>
        /// Ստանում է հաճախորդի ակրեդիտիվները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>

        public static List<Accreditive> GetAccreditives(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Accreditive> accreditives = new List<Accreditive>();

            if (filter == ProductQualityFilter.NotSet || filter == ProductQualityFilter.Opened)
            {
                accreditives.AddRange(AccreditiveDB.GetAccreditives(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                accreditives.AddRange(AccreditiveDB.GetClosedAccreditives(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                accreditives.AddRange(AccreditiveDB.GetAccreditives(customerNumber));
                accreditives.AddRange(AccreditiveDB.GetClosedAccreditives(customerNumber));
            }
            return accreditives;

        }
        /// <summary>
        /// Ստանում է ակրեդիտիվը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Accreditive GetAccreditive(ulong customerNumber, ulong productId)
        {
            return AccreditiveDB.GetAccreditive(customerNumber, productId);
        }

        /// <summary>
        /// Ստուգում է դրամական միջոցների գրավի արժույթը համապատասխանում է վարկի արժույթին
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static bool CheckAccreditiveProvisionCurrency(long productId, string currency)
        {
            return AccreditiveDB.CheckAccreditiveProvisionCurrency(productId, currency);
        }

        /// <summary>
        /// Ստուգում է տրանսպորտային միջոցի գրավ առկա է թե ոչ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool HasTransportProvison(long productId)
        {
            return AccreditiveDB.HasTransportProvison(productId);
        }
    }
}
