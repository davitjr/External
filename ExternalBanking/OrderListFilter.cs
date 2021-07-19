using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class OrderListFilter
    {
        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Վերջ 
        /// </summary>
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Հայտի տեսակ
        /// </summary>
        public List<short> OrderTypes { get; set; }

        /// <summary>
        /// Հայտի կաչգավիճակ
        /// </summary>
        public List<short> OrderQualities { get; set; }

        /// <summary>
        /// Ստացողի հաշիվ
        /// </summary>
        public string ReceiverAccount { get; set; }

        /// <summary>
        /// Ստացողի քարտի համար
        /// </summary>
        public string ReceiverCardNumber { get; set; }

        /// <summary>
        /// Ստացողի անուն
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// Բաժանորդի համար ( նախատեսված կոմունալի համար)
        /// </summary>
        public string AbonentNumber { get; set; }

        /// <summary>
        /// Մուտքագրման աղբյուր
        /// </summary>
        public SourceType Source { get; set; }

        /// <summary>
        /// Խմբի համար
        /// </summary>
        public int GroupId { get; set; } = 0;

    }
}
