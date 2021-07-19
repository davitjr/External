using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class EOTransferResponse
    {
        /// <summary>
        /// Հարցման համար
        /// </summary>
        public int ParentID { get; set; }

        /// <summary>
        /// Փոխանցման գումար
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Փոխանցման համար
        /// </summary>
        public ulong TransferID { get; set; }

        /// <summary>
        /// Սխալի կոդ` 0 – ճիշտ հարցում, 1 – սխալ
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Սխալի նկարագրություն։ Եթե errorCode = 0, ապա այս էլեմենտը պատասխանի մեջ չի ներառվում։
        /// </summary>
        public string ErrorText { get; set; }
    }
}
