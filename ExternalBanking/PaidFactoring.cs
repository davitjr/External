using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class PaidFactoring : Loan
    {
        public long FactoringCustomerNumber { get; set; }
        public static List<PaidFactoring> GetPaidFactorings(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PaidFactoring> paidFactorings = new List<PaidFactoring>();

            if (filter == ProductQualityFilter.NotSet || filter == ProductQualityFilter.Opened)
            {
                paidFactorings.AddRange(PaidFactoringDB.GetPaidFactorings(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                paidFactorings.AddRange(PaidFactoringDB.GetClosedPaidFactorings(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                paidFactorings.AddRange(PaidFactoringDB.GetPaidFactorings(customerNumber));
                paidFactorings.AddRange(PaidFactoringDB.GetClosedPaidFactorings(customerNumber));
            }
            return paidFactorings;

        }
        public static PaidFactoring GetPaidFactoring(ulong customerNumber, ulong productId)
        {
            PaidFactoring paidFactoring = null;
            paidFactoring = PaidFactoringDB.GetPaidFactoring(customerNumber, productId);
            if (paidFactoring == null)
            {
                paidFactoring = PaidFactoringDB.GetClosedPaidFactoring(customerNumber, productId);
            }
            return paidFactoring;
        }
    }
}
