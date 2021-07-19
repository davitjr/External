using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public enum CommunalTypes : short
    {
        None = 0,
        /// <summary>
        ///«Հայաստանի Էլեկտրական ցանցեր» ծառայության դիմաց վճար
        /// </summary>
        ENA = 3,
        /// <summary>
        /// «Հայռուսգազարդ» ծառայության սպառած գազի դիմաց վճար
        /// </summary>
        Gas = 4,
        /// <summary>
        /// «ՀայՋրմուղԿոյուղի» ծառայության դիմաց վճար
        /// </summary>
        ArmWater = 5,
        /// <summary>
        /// «Երևան Ջուր» ծառայության դիմաց վճար
        /// </summary>
        YerWater = 6,
        /// <summary>
        /// «ԱրմենՏել» ծառայության դիմաց վճար
        /// </summary>
        ArmenTel = 7,
        /// <summary>
        /// «Ղ-Տելեկոմ» ծառայության դիմաց վճար
        /// </summary>
        VivaCell = 8,
        /// <summary>
        /// «ՅուՔոմ» ծառայության դիմաց վճար
        /// </summary>
        UCom = 11,
        /// <summary>
        /// «Օրանժ» ծառայության դիմաց վճար
        /// </summary>
        Orange = 10,
        ///// <summary>
        ///// «ՅուՔոմ» ծառայության դիմաց վճար սխալ -- Mobile տարբերակի փոփոխումից հետո հանել
        ///// </summary>
        //UComWrong=9,

        /// <summary>
        /// Աղբահանության ծառայության դիմաց վճար
        /// </summary>
        Trash=9,

        /// <summary>
        /// ՋՕԸ ծառայության դիմաց վճար
        /// </summary>
        COWater = 14,

        /// <summary>
        /// Բիլայն Ինտերնետ ծառայության դիմաց վճար
        /// </summary>
        BeelineInternet = 17

    }

    public class Communal
    {
        /// <summary>
        /// Կոմունալի տեսակ
        /// </summary>
        public CommunalTypes ComunalType { get; set; }
        
        /// <summary>
        /// Աբոնենտի համար
        /// </summary>
        public string AbonentNumber { get; set; }
               
        /// <summary>
        /// Աբոնենտի մասնաճյուղ
        /// </summary>
        public string BranchCode { get; set; }

        /// <summary>
        /// Նկարագրություն (աբոնենտի անուն,հասցե և այլ ինֆորմացիա)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///  Հեռախոսահամարի տեսակը կանխավճարային կամ հետվճարային
        /// </summary>
        public PrepaidSign PrepaidSign { get; set; }

        /// <summary>
        ///  Պարտք
        /// </summary>
        public double? Debt { get; set; }

        /// <summary>
        /// Աբոնենտի բանկային մասնաճյուղ
        /// </summary>
        public string AbonentFilialCode { get; set; }
        /// <summary>
        /// Գազի օգտագործման և սպասարկման վճարների տեղաբաշխման հաշվարկ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="gasUsageDebt"></param>
        /// <param name="serviceFeeDebt"></param>
        /// <returns></returns>
        public static void CalculateGasPromPaymentAmounts(UtilityPaymentOrder order, double gasUsageDebt, double serviceFeeDebt)
        {
            double gasUsagePayment = 0;
            double serviceFeePayment = 0;
            double paidAmount = order.Amount;

            if (gasUsageDebt < 0 && serviceFeeDebt < 0)
            {
                if (paidAmount <= Math.Abs(gasUsageDebt))
                    gasUsagePayment = paidAmount;
                else if (paidAmount > Math.Abs(gasUsageDebt) && paidAmount <= Math.Abs(gasUsageDebt + serviceFeeDebt))
                {
                    gasUsagePayment = Math.Abs(gasUsageDebt);
                    serviceFeePayment = paidAmount - gasUsagePayment;
                }
                else if (paidAmount > Math.Abs(gasUsageDebt + serviceFeeDebt))
                {
                    serviceFeePayment = Math.Abs(serviceFeeDebt);
                    gasUsagePayment = paidAmount - serviceFeePayment;
                }
            }
            else if (gasUsageDebt >= 0 && serviceFeeDebt < 0)
            {
                if (paidAmount <= Math.Abs(serviceFeeDebt))
                    serviceFeePayment = paidAmount;
                else
                {
                    serviceFeePayment = Math.Abs(serviceFeeDebt);
                    gasUsagePayment = paidAmount - serviceFeePayment;
                }
            }
            else
                gasUsagePayment = paidAmount;

            order.Amount = Math.Round(gasUsagePayment, 1);
            order.ServiceAmount = Math.Round(serviceFeePayment, 1);

            if (order.Amount + order.ServiceAmount > paidAmount) order.Amount = paidAmount - order.ServiceAmount;
        }
        public static string GetCommunalDescriptionByType(int type, Languages lang)
        {
            return CommunalDB.GetCommunalDescriptionByType(type, lang);
        }


    }
}