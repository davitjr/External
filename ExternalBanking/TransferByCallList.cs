using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class TransferByCallList
    {
        /// <summary>
        /// Փոխանցումներ քանակ
        /// </summary>
        public long TransferCount { get; set; }

        /// <summary>
        /// Փոխանցումների ցուցակ
        /// </summary>
        public List<TransferByCall> TransferList { get; set; }

    }
}
