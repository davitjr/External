using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class PaidAccreditive : Loan
    {
        public static List<PaidAccreditive> GetPaidAccreditives(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PaidAccreditive> paidAccreditives = new List<PaidAccreditive>();

            if (filter == ProductQualityFilter.NotSet || filter == ProductQualityFilter.Opened)
            {
                paidAccreditives.AddRange(PaidAccreditiveDB.GetPaidAccreditives(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                paidAccreditives.AddRange(PaidAccreditiveDB.GetClosedPaidAccreditives(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                paidAccreditives.AddRange(PaidAccreditiveDB.GetPaidAccreditives(customerNumber));
                paidAccreditives.AddRange(PaidAccreditiveDB.GetClosedPaidAccreditives(customerNumber));
            }
            return paidAccreditives;

        }
        public static PaidAccreditive GetPaidAccreditive(ulong customerNumber, ulong productId)
        {
            return PaidAccreditiveDB.GetPaidAccreditive(customerNumber, productId);
        }
    }
}
