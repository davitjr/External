using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class HBDocumentArCaBalance
    {
        public long CardNumber { get; set; }

        public string Currency { get; set; }
        public int ResponseCode { get; set; }
        public string Responsedescription { get; set; }
        public int ArcaBalanceAmount { get; set; }
        public int AvailableExceedLimit { get; set; }
        public int OwnFunds { get; set; }
    }
}
