using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class OrderFilter
    {
        /// <summary>
        /// Հանձնարարականի ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Սկիզբ (հայտի ա/թ)
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Վերջ (հայտի ա/թ)
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Հայտի կարգավիճակ
        /// </summary>
        public OrderQuality OrderQuality { get; set; }

        /// <summary>
        /// Հայտի տեսակ 
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// Հայտի ենթատեսակ
        /// </summary>
        public byte SubType { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        public SourceType Source { get; set; }

      
       
    }
}
