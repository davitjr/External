using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class Guarantee : Loan
    {
        /// <summary>
        /// Բենեֆիցիար
        /// </summary>
        public string Benefeciar { get; set; }

        /// <summary>
        /// Կոմիսիոն վճարի գանձման եղանակ
        /// </summary>
        public ushort PercentCummulation { get; set; }

        /// <summary>
        /// Պայմանագրի վերջ
        /// </summary>
        public DateTime? ContractEndDate { get; set; }



        /// <summary>
        /// Երկարաձգումների ա/թ-ներ
        /// </summary>
        public List<DateTime> ProlongationDates { get; set; }

        /// <summary>
        ///  Հաստատող բանկ
        /// </summary>
        public string ConfirmatorBank { get; set; }

        /// <summary>
        /// Հաստատողի բանկի վերջին ա/թ
        /// </summary>
        public DateTime? ConfirmatorBankEndDate { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public uint SetNumber { get; set; }


        /// <summary>
        /// Ստանում է հաճախորդի ակրեդիտիվները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>

        public static List<Guarantee> GetGuarantees(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Guarantee> guarantees = new List<Guarantee>();

            if (filter == ProductQualityFilter.NotSet || filter == ProductQualityFilter.Opened)
            {
                guarantees.AddRange(GuaranteeDB.GetGuarantees(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                guarantees.AddRange(GuaranteeDB.GetClosedGuarantees(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                guarantees.AddRange(GuaranteeDB.GetGuarantees(customerNumber));
                guarantees.AddRange(GuaranteeDB.GetClosedGuarantees(customerNumber));
            }
            return guarantees;

        }
        /// <summary>
        /// Ստանում է ակրեդիտիվը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static Guarantee GetGuarantee(ulong customerNumber, ulong productId)
        {
            return GuaranteeDB.GetGuarantee(customerNumber, productId);
        }

        /// <summary>
        /// Ստուգում է դրամական միջոցների գրավի արժույթը համապատասխանում է վարկի արժույթին
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static bool CheckFGuaranteeProvisionCurrency(long productId, string currency)
        {
            return GuaranteeDB.CheckFGuaranteeProvisionCurrency(productId, currency);
        }


        /// <summary>
        /// Ստուգում է տրանսպորտային միջոցի գրավ առկա է թե ոչ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool HasTransportProvison(long productId)
        {
            return GuaranteeDB.HasTransportProvison(productId);
        }


        /// <summary>
        /// Վերադարձնում է նվազեցումը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<GivenGuaranteeReduction> GetGivenGuaranteeReductions(ulong productId)
        {
            return GuaranteeDB.GetGivenGuaranteeReductions(productId);
        }


    }
}
