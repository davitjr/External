using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking.XBManagement
{
    public class AssigneeCustomer
    {
        public long CustomerNumber { get; set; }
        public string DefaultDocument { get; set; }
        public string FullName { get; set; }

        public static List<AssigneeCustomer> GetHBAssigneeCustomers(ulong customerNumber)
        {
            return HBApplicationDB.GetHBAssigneeCustomers(customerNumber);
        }

    }
}
