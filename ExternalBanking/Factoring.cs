using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
   public class Factoring
    {
        /// <summary>
        /// Ֆակտորինգի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// ֆակտորինգի մասնաճյուղ
        /// </summary>
        public int FillialCode { get; set; }
        /// <summary>
        /// Ֆակտորինգի սկզբի ա/թ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Ֆակտորինգի վերջի ա/թ
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Ֆակտորինգի սահմանաչափ
        /// </summary>
        public double StartCapital { get; set; }
        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Պրոդուկտի Տեսակ
        /// </summary>
        public short Type { get; set; }
        /// <summary>
        /// Պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Ֆակտորինգի տեսակ ըստ գործունեության
        /// </summary>
        public short FactoringType { get; set; }
        
        /// <summary>
        /// Ֆակտորինգի տեսակի նկարագրություն
        /// </summary>
        public string FactoringTypeDescription { get; set; }
        
        /// <summary>
        /// Ֆակտորինգի տեսակ
        /// </summary>
        public short FactoirngRegresType { get; set; }
        
        /// <summary>
        /// Ֆակտորինգի տեսակի նկարագրություն
        /// </summary>
        public string FactoringRegresTypeDescription { get; set; }

        /// <summary>
        /// Ֆակտորինգի արժույթ
        /// </summary>
        public string FactoringCurrency { get; set; }

        /// <summary>
        /// Միջնորդավճարի տոկոսադրույք
        /// </summary>
        public float CommissionPercent { get; set; }

        
        /// <summary>
        /// Ստանում է հաճախորդի ֆակտորինգները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
       public static List<Factoring> GetFactorings(ulong customerNumber,ProductQualityFilter filter)
        {
            List<Factoring> factorings = new List<Factoring>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                factorings.AddRange(FactoringDB.GetFactorings(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                factorings.AddRange(FactoringDB.GetClosedFactorings(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                factorings.AddRange(FactoringDB.GetFactorings(customerNumber));
                factorings.AddRange(FactoringDB.GetClosedFactorings(customerNumber));
            }
            return factorings;
        }
        /// <summary>
        /// Ստանում է ֆակտորինգը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
       public static Factoring GetFactoring(ulong customerNumber,ulong productId)
       {
           return FactoringDB.GetFactoring( customerNumber,productId);
       }
        /// <summary>
        /// Ստուգում է ռեգրեսային տեսակի ֆակտորինգի համար  հաճախորդի կողմից երաշխավորության առկայությունը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool FactoringValidation1(long productId)
        {
            return FactoringDB.FactoringValidation1(productId);
        }
        /// <summary>
        /// Ստուգում է դրամական միջոցների գրավի արժույթը համապատասխանում է վարկի արժույթին
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckFactoringProvisionCurrency(long productId)
        {
            return FactoringDB.CheckFactoringProvisionCurrency(productId);
        }

        public static bool IsSecondTermination(FactoringTerminationOrder order)
        {
            return FactoringDB.IsSecondTermination(order);
        }
    }
}
