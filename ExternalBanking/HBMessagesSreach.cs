using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class HBMessagesSreach
    {
        public ulong CustomerNumber { get; set; }
        public int? ReadOrUnReadMsg { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int? ReceivedOrSentMsg { get; set; }

        public int SetNumber { get; set; }

        public int firstRow { get; set; }
        public int LastGetRowCount { get; set; }

    }
}
