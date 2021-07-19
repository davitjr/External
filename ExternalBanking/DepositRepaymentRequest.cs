using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class DepositRepaymentRequest
    {
        /// <summary>
        /// Ավանդի տեսակ
        /// </summary>
        public DepositType DepositType { get; set; }

        /// <summary>
        /// Տվյալների աղբյուր
        /// </summary>
        public SourceType Source { get; set; }

        /// <summary>
        /// Ավանդի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ավանդի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Ավանդի սկզբնական գումար
        /// </summary>
        public double StartCapital { get; set; }

        /// <summary>
        /// Ավանդի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Ավանդի տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Ավանդի օպցիաներ
        /// </summary>
        public List<DepositOption> DepositOption { get; set; }

        /// <summary>
        /// Որոշում է ավանդը երրորդ անձի համար է թե ոչ
        /// </summary>
        public ushort AccountType { get; set; }

        /// <summary>
        /// Երրորդ անձանանց հաճախորդի համարներ
        /// </summary>
        public List<KeyValuePair<ulong, string>> ThirdPersonCustomerNumbers { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

    }
}
