
namespace ExternalBankingService
{
    public class AuthorizedCustomer
    {
        public short ApprovementScheme { get; set; }

        public int BranchCode { get; set; }

        public ulong CustomerNumber { get; set; }

        public double DailyTransactionsLimit { get; set; }

        public string FullName { get; set; }

        public double OneTransactionLimit { get; set; }

        public int Permission { get; set; }

        public int SecondConfirm { get; set; }

        public string SessionID { get; set; }

        public int TypeOfClient { get; set; }

        public string UserName { get; set; }

        public double OneTransactionLimitToOwnAccount { get; set; }

        public double OneTransactionLimitToAnothersAccount { get; set; }

        public double DayLimitToOwnAccount { get; set; }

        public double DayLimitToAnothersAccount { get; set; }

        public int LimitedAccess { get; set; }


    }
}
