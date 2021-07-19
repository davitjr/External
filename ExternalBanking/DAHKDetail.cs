using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// ԴԱՀԿ ի տվյալներ
    /// </summary>
    public class DAHKDetail
    {
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// ԴԱՀԿ գումար
        /// </summary>
        public double Amount { get; set; }
    }
}
