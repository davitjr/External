using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class PosRate
    {
        public ushort CardSystem { get; set; }
        public string CardSystemDescription { get; set; }
        //public ushort RateType { get; set; }
        //public ushort RateTypeDescription { get; set; }
        public float Rate { get; set; }
        public float LocalRate { get; set; }
        public ushort FixedRate { get; set; }
    }
}
