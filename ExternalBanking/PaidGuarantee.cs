using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
   public class PaidGuarantee:Loan
    {
        /// <summary>
        /// Վճարված երաշխիքի կարգավիճակ
        /// </summary>
        public int RequestStatus { get; set; }

        public static List<PaidGuarantee> GetPaidGarantees(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PaidGuarantee> paidGuarantees = new List<PaidGuarantee>();

            if (filter == ProductQualityFilter.NotSet || filter == ProductQualityFilter.Opened)
            {
                paidGuarantees.AddRange(PaidGuaranteeDB.GetPaidGuarantees(customerNumber));
            }

            if (filter == ProductQualityFilter.Closed)
            {
                paidGuarantees.AddRange(PaidGuaranteeDB.GetClosedPaidGuarantees(customerNumber));
            }

            if (filter == ProductQualityFilter.All)
            {
                paidGuarantees.AddRange(PaidGuaranteeDB.GetPaidGuarantees(customerNumber));
                paidGuarantees.AddRange(PaidGuaranteeDB.GetClosedPaidGuarantees(customerNumber));
            }
            return paidGuarantees;
        }

        public static PaidGuarantee GetPaidGuarantee(ulong customerNumber, ulong productId)
        {
            return PaidGuaranteeDB.GetPaidGuarantee(customerNumber, productId);
        }
    }
}
