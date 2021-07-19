using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class IBankingHomePage
    {
        public ContentResult<List<Account>> Accounts { get; set; }
        public ContentResult<List<Card>> Cards { get; set; }
        public ContentResult<List<Deposit>> Deposits { get; set; }
        public ContentResult<List<Loan>> Loans { get; set; }
    }
}
