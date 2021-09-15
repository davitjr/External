using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class LoanRepaymentFromCardDataChange
    {
        public ulong AppId { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime StartDate { get; set; }

        public int SetNumber { get; set; }

        public string Description { get; set; }

        public int Action { get; set; }
    }
}
