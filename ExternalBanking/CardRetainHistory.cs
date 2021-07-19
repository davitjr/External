using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class CardRetainHistory
    {
        public int TerminalId { get; set; }
        public string ATMDescription { get; set; }
        public string FillialCode { get; set; }
        public string HistoryDate { get; set; }
        public string HistoryTime { get; set; }
        public double Amount { get; set; }
        public string CaptStatus { get; set; }
    }
}
