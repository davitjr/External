using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Սպասարկան կետ
    /// </summary>
    public class PosLocation
    {
        /// <summary>
        ///Սպասարկան կետի ունիկալ համար 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Անվանում
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Բացման ա/թ
        /// </summary>
        public DateTime OpenDate { get; set; }
        /// <summary>
        /// Փակման ամսաթիվ
        /// </summary>
        public DateTime? ClosedDate { get; set; }
        /// <summary>
        /// Հասցե
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Հեռախոսահամարներ
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Սպասարկան կետի տեսակ
        /// </summary>
        public ushort LocationType { get; set; }

        /// <summary>
        /// Սպասարկան կետի տեսակի նկարագրություն
        /// </summary>
        public string LocationTypeDescription { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public ushort Quality { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Լրացուցիչ նկարագրություն
        /// </summary>
        public string AdditionalDescription { get; set; }

        /// <summary>
        /// Սպասարկման կետի POS տերմինալներ
        /// </summary>
        public List<PosTerminal> Posterminals { get; set; }

        /// <summary>
        /// Ստանում է հաճախորդի սպասարկան կետերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// 
        public static List<PosLocation> GetCustomerPosLocations(ulong customerNumber, ProductQualityFilter filter)
        {
            return PosLocationDB.GetCustomerPosLocations(customerNumber, filter);
        }
        /// <summary>
        /// Ստանում է սպասարկաման կետը
        /// </summary>
        public void Get()
        {
            PosLocationDB.GetPosLocation(this);
            PosLocationDB.GetPosTerminals(this);
        }
    }
}
