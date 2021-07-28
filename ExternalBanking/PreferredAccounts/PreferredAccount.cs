using ExternalBanking.DBManager.PreferredAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.PreferredAccounts
{
    public class PreferredAccount 
    {
        public string AccountNumber { get; set; }

        public bool IsActive { get; set; }


        public PreferredAccount GetSelectedOrDefaultPreferredAccountNumber(PreferredAccountServiceTypes serviceType,ulong customerNumber)
        {
            return PreferredAccountDB.GetSelectedOrDefaultPreferredAccountNumber(serviceType, customerNumber);
        }

        public bool IsDisabledPreferredAccountService(ulong customerNumber, PreferredAccountServiceTypes preferredAccountServiceType)
        {
            return PreferredAccountDB.IsDisabledPreferredAccountService(customerNumber, preferredAccountServiceType);
        }
    }
}
