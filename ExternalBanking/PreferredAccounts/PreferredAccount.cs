using ExternalBanking.DBManager.PreferredAccounts;

namespace ExternalBanking.PreferredAccounts
{
    public class PreferredAccount
    {
        public string AccountNumber { get; set; }

        public bool IsActive { get; set; }


        public PreferredAccount GetSelectedOrDefaultPreferredAccountNumber(PreferredAccountServiceTypes serviceType, ulong customerNumber)
        {
            return PreferredAccountDB.GetSelectedOrDefaultPreferredAccountNumber(serviceType, customerNumber);
        }

        public bool IsDisabledPreferredAccountService(ulong customerNumber, PreferredAccountServiceTypes preferredAccountServiceType)
        {
            return PreferredAccountDB.IsDisabledPreferredAccountService(customerNumber, preferredAccountServiceType);
        }
    }
}
