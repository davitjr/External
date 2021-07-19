using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class PosCashbackRate
    {
        public ushort CardType { get; set; }
        public string CardTypeDescription { get; set; }
        public float Rate { get; set; }
        public float RateFromACBA { get; set; }
        public DateTime StartDate { get; set; }
    }
}
