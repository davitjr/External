using System.Collections.Generic;

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
