using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class Shop
    {
        /// <summary>
        /// Ապառիկ Տեղում խանութի համար
        /// </summary>
        public int ShopID { get; set; }

        /// <summary>
        /// Խանութի իրավաբանական անձի համար
        /// </summary>
        public long IdentityID { get; set; }

        /// <summary>
        /// Խանութի անվանում
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// Խանութի իրավաբանական անձի անվանում
        /// </summary>
        public string ShopLegalName { get; set; }
    }
}
