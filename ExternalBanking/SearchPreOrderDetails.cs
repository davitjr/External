using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class SearchPreOrderDetails : SearchParams
    {
        /// <summary>
        /// Նախնական հայտի կարգավիճակ
        /// </summary>
        public PreOrderQuality Quality{ get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Պրոդուկտի հերթական համար
        /// </summary>
        public ulong AppID { get; set; }
        /// <summary>
        /// Նախնական հայտի տեսակ
        /// </summary>
        public PreOrderType Type{ get; set; }

    }
}
