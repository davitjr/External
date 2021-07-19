using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Գրավ
    /// </summary>
   public class Provision
    {
       /// <summary>
       /// Գրավի համար
       /// </summary>
       public string ProvisionNumber { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public int ContractId { get; set;}
       
       /// <summary>
       /// Բալանսային հաշիվ
       /// </summary>
       public string OutBalance { get; set; }

       /// <summary>
       /// Արժույթ
       /// </summary>
       public string Currency { get; set; }

       /// <summary>
       /// Գումար
       /// </summary>
       public double Amount { get; set; }

       /// <summary>
       /// Տեսակ
       /// </summary>
       public short Type { get; set; }

       /// <summary>
       /// Տեսակի նկարագրություն
       /// </summary>
       public string TypeDescription { get; set; }

       /// <summary>
       /// Ծրագիր
       /// </summary>
       public short LoanProgram { get; set; }

       /// <summary>
       /// Պարկի համար
       /// </summary>
       public string PacketNumber { get; set; }

       /// <summary>
       /// Գրավի վկայականի համար
       /// </summary>
       public string LicenceNumber { get; set; }

       /// <summary>
       /// Անշարժ գույքի. Սեփ. վկայականի համար
       /// </summary>
       public string PropertyLicenceNumber { get; set; }

       /// <summary>
       /// Ակտիվացման ա/թ
       /// </summary>
       public DateTime ActivateDate { get; set; }

       /// <summary>
       /// Փակման ա/թ
       /// </summary>
       public DateTime? ClosingDate { get; set; }

       /// <summary>
       /// Գրավի ներքին համար
       /// </summary>
       public long Id { get; set; }

       /// <summary>
       /// Փակման հիմք
       /// </summary>
       public string ClosingReason { get; set; }

       /// <summary>
       /// Գրավը շարժական է տե անշարժ 1 ի դեպքում անշարժ
       /// </summary>
       public short? MoveableIndex { get; set; }

       /// <summary>
       /// Գրավի նկարագրություն
       /// </summary>
       public string Description { get; set; }

       /// <summary>
       /// Գրավի կցման ՊԿ
       /// </summary>
       public int ActivatedSetNumber { get; set; }

       /// <summary>
       /// Գրավի փակման ՊԿ
       /// </summary>
       public int MaturedSetNumber { get; set; }

        /// <summary>
        ///Կարգավիճակ 
        /// </summary>
       public ushort Quality { get; set; }
        /// <summary>
        /// կարգավիճակի նկարագրություն
        /// </summary>
       public string QualityDescription { get; set; }

       public static List<Provision> GetProductProvisions(ulong productId,ulong customerNumber)
       {
           return ProvisionDB.GetProductProvisions(productId,customerNumber);
       }

        public static List<Provision> GetCustomerProvisions(ulong customerNumber, string currency, ushort type, ushort quality)
        {
            return ProvisionDB.GetCustomerProvisions(customerNumber,currency,type,quality);
        }

        public static List<ProvisionLoan> GetProvisionLoans(ulong provisionId)
        {
            return ProvisionDB.GetProvisionLoans(provisionId);

        }

       /// <summary>
       /// Վերադարձնում է գրավի երաշխավորների հաճախորդի համարները
       /// </summary>
       /// <param name="productId"></param>
       /// <returns></returns>
        public static List<ulong> GetProvisionOwners(ulong productId)
       {
           return ProvisionDB.GetProvisionOwners(productId);
       }

        public static bool HasPropertyProvision(ulong productId)
        {
            return ProvisionDB.HasPropertyProvision(productId);
        }

        public static Dictionary<string, string> ProvisionContract(long docId)
        {
            return ProvisionDB.ProvisionContract(docId);
        }
    }
}
