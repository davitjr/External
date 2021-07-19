using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class EOGetClientResponse
    {
        /// <summary>
        /// Հարցման հերթական համար
        /// </summary>
        public int ParentID { get; set; }

        /// <summary>
        /// Պատասխանի հերթական համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Քարտին կից դրամային հաշվի համարը՝ լրացվում է եթե հարցման մեջ նշված է քարտի համարը և պատասխանի errorCode = 0
        /// </summary>
        public long Account { get; set; }

        /// <summary>
        /// Սխալի կոդ`  0 – ճիշտ հարցում, 1 – սխալ

        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Սխալի նկարագրություն։ Եթե errorCode = 0, ապա այս էլեմենտը պատասխանի մեջ չի ներառվում։
        /// </summary>
        public string ErrorText { get; set; }
    }
}
