using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Ապահովագրություն
    /// </summary>
    public class Insurance
    {

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }


        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long MainProductId { get; set; }

        /// <summary>
        /// Կցված պրոդուկտի ունիկալ համար
        /// </summary>
        public long ConectedProductId { get; set; }


        /// <summary>
        /// Ապահովագրական ընկերություն
        /// </summary>
        public ushort Company { get; set; }

        /// <summary>
        /// Ապահովագրական ընկերության անվանում
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Ապահովագրության տեսակ
        /// </summary>
        public ushort InsuranceType { get; set; }

        /// <summary>
        /// Ապահովագրության տեսակի նկարագրություն
        /// </summary>
        public string InsuranceTypeDescription { get; set; }


        /// <summary>
        /// Ապահովագրավական գումար
        /// </summary>
        public double Amount { get; set; }


        /// <summary>
        /// Ապահովագրավճար
        /// </summary>
        public double CompensationAmount { get; set; }

        /// <summary>
        /// Ապահովագրավճարի արժույթ
        /// </summary>
        public string CompensationCurrency { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }


        /// <summary>
        /// Ներգրավող ՊԿ
        /// </summary>
        public int InvolvingSetNumber { get; set; }


        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }


        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FillialCode { get; set; }


        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        ///// <summary>
        /////apahovagrakan paymanagri tesak
        ///// </summary>
        public int InsuranceContractType { get; set; }

        ///// <summary>
        /////ashxatakci pashton
        ///// </summary>
        public int PositionNumber { get; set; }

        ///// <summary>
        /////Gravi hamar
        ///// </summary>
        public long IdPro { get; set; }

        /// <summary>
        /// Ապահովագրության պայմանագրի տեսակի նկարագրություն
        /// </summary>
        public string InsuranceContractTypeDescription { get; set; }

        /// <summary>
        /// Ապահովագրության ձեռքով հեռացված
        /// </summary>
        public bool IsManualDeleted { get; set; }




        /// <summary>
        /// Վերադարձնում է հաճախորդի ապահովագրությունները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<Insurance> GetInsurances(ulong customerNumber, ProductQualityFilter filter)
        {
            List<Insurance> insurances = new List<Insurance>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                insurances.AddRange(InsuranceDB.GetInsurances(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                insurances.AddRange(InsuranceDB.GetClosedInsurances(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                insurances.AddRange(InsuranceDB.GetInsurances(customerNumber));
                insurances.AddRange(InsuranceDB.GetClosedInsurances(customerNumber));
            }
            return insurances;
        }


        /// <summary>
        /// Վերադարձնում է վճարված ապահովագրություն վարկի ապահովագրությունը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<Insurance> GetPaidInsurance(ulong customerNumber, ulong loanProductId)
        {
            return InsuranceDB.GetPaidInsurance(customerNumber, loanProductId);
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ ապահովագրությունը
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static Insurance GetInsurance(ulong productId, ulong customerNumber)
        {
            return InsuranceDB.GetInsurance(customerNumber, productId);
        }

        /// <summary>
        /// Վերադարձնում է ապահովագրական ընկերության սիստեմային հաշվի համարը
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="insuranceType"></param>
        /// <returns></returns>
        public static uint GetInsuranceCompanySystemAccountNumber(ushort companyID, ushort insuranceType)
        {
            return InsuranceDB.GetInsuranceCompanySystemAccountNumber(companyID, insuranceType);
        }

        /// <summary>
        /// Վերադարձնում է ապահովագրական ընկերության սիստեմային հաշվը
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="insuranceType"></param>
        /// <returns></returns>
        public static Account GetInsuraceCompanySystemAccount(ushort companyID, ushort insuranceType)
        {
            Account companySystemAccount = null;
            uint systemAccountNumber = GetInsuranceCompanySystemAccountNumber(companyID, insuranceType);

            if (systemAccountNumber != 0)
                companySystemAccount = Account.GetSystemAccountByNN(systemAccountNumber);

            return companySystemAccount;

        }

    }
}
