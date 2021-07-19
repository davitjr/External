using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Ի պահ ընդունված արժեքների հայտ
    /// </summary>
    public class SafekeepingItemOrder:Order
    {

        /// <summary>
        /// Ի պահ ընդունված արժեքներ
        /// </summary>
        public SafekeepingItem SafekeepingItem { get; set; }



    }
}
