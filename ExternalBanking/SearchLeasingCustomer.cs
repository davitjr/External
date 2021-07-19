using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class SearchLeasingCustomer
    {
        public string CustomerNumber { get; set; }

        public short LeasingCustomerNumber { get; set; }
        /// <summary>
        /// Անուն
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Անվանում
        /// </summary>
        public string OrganizationName { get; set;}
        /// <summary>
        /// ՀՀՎՀ
        /// </summary>
        public string TaxCode { get; set; }
        /// <summary>
        /// Անձնագրի համար
        /// </summary>
        public string PassportNumber { get; set; }

        public static List<SearchLeasingCustomer> Search(SearchLeasingCustomer searchParams)
        {       
            List<SearchLeasingCustomer> leasingCustomers = LeasingDB.GetLeasingCustomers(searchParams);
            return leasingCustomers;
        }

    }

}
