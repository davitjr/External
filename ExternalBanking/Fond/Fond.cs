using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Ֆոնդեր
    /// </summary>
    public class Fond
    {
        /// <summary>
        /// Ֆոնդի համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Ֆոնդի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ակտիվ ֆոնդ (1` այո, 0` ոչ)
        /// </summary>
        public byte IsActive { get; set; }

        /// <summary>
        /// Սուբսիդավորվող ֆոնդ (1` այո, 0` ոչ)
        /// </summary>
        public byte IsSubsidia { get; set; }
        /// <summary>
        /// Ֆոնդի միջոցների տրամադրման պայմաններ
        /// </summary>
        public List<FondProvidingDetail> ProvidingDetails { get; set; }

        /// <summary>
        /// Վերադարձնում է բոլոր ֆոնդերը
        /// </summary>
        /// <returns></returns>
        public static List<Fond> GetFonds(ProductQualityFilter filter)
        {
            List<Fond> fonds = new List<Fond>();

            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                fonds.AddRange(FondDB.GetFonds());
            }
            if (filter == ProductQualityFilter.Closed)
            {
                fonds.AddRange(FondDB.GetClosedFonds());
            }


            return fonds;
        }
        /// <summary>
        /// Վերադարձնում է ֆոնդի տվյալները
        /// </summary>
        /// <returns></returns>
        public static Fond GetFondByID(int ID)
        {
            Fond fond = FondDB.GetFond(ID);

            return fond;
        }
    }
}
