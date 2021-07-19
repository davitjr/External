using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class SearchSwiftCodes
    {   
        /// <summary>
        /// SWIFT կոդ
        /// </summary>
        public string SwiftCode { get; set; }

        /// <summary>
        /// Քաղաք
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Բանկի անվանում
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Երկիր
        /// </summary>
        public string Counry { get; set; }
 
        /// <summary>
        /// Քարտի(երի) որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<SearchSwiftCodes> Search(SearchSwiftCodes searchParams)
        {
            List<SearchSwiftCodes> swiftCodes = SearchSwiftCodesDB.GetSwiftCodes(searchParams);
            return swiftCodes;
        }

    }
}




