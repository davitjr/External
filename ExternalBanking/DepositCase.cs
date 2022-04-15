using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Պահատուփեր
    /// </summary>
    public class DepositCase
    {
        /// <summary>
        /// Պրուդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Պահատուփի համար
        /// </summary>
        public string CaseNumber { get; set; }
        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public short FilialCode { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Տուժանք
        /// </summary>
        public double PenaltyAmount { get; set; }

        /// <summary>
        ///  Պայմանագրի համար
        /// </summary>
        public ulong ContractNumber { get; set; }

        /// <summary>
        /// Հաշվարկի ա/թ
        /// </summary>
        public DateTime? LastDayOfRateCalculation { get; set; }

        /// <summary>
        /// Տուգանքի ա/թ
        /// </summary>
        public DateTime? PenaltyDate { get; set; }

        /// <summary>
        /// Դադարեցնողի ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }

        /// <summary>
        /// Տրամադրող ՊԿ
        /// </summary>
        public int ServicingSetNumber { get; set; }

        /// <summary>
        /// Որոշում է հաշիվը համատեղ է թե ոչ
        /// </summary>
        public short JointType { get; set; }

        /// <summary>
        ///  Համատեղի ժամանակ պայմանագրի տեսակ(միաժամանակյա/ոչ միաժամանակյա)
        /// </summary>
        public short ContractType { get; set; }

        /// <summary>
        /// Համատեղ հաճախորդի համարը  
        /// </summary>
        public List<KeyValuePair<ulong, string>> JointCustomers { get; set; }

        /// <summary>
        /// Կցված ընթացիկ հաշիվ
        /// </summary>
        public Account ConnectAccount { get; set; }

        /// <summary>
        /// Պայմանագրի ժամկետ
        /// </summary>
        public short ContractDuration { get; set; }

        /// <summary>
        /// Արտաբալանսային հաշիվ
        /// </summary>
        public Account OutBalanceAccount { get; set; }

        /// <summary>
        /// Ավտոմատ երկարաձգում
        /// </summary>
        public bool RecontractPossibility { get; set; }


        /// <summary>
        /// Փոփոխման հիմք
        /// </summary>
        public string ChangingReason { get; set; }

        /// <summary>
        /// Տույժի դադարեցման ամսաթիվ
        /// </summary>
        public DateTime? DateOfStoppingPenaltyCalculation { get; set; }

        /// <summary>
        /// Փաստաթղթի ամսաթիվ
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Տույժի դադարեցնողի ՊԿ
        /// </summary>
        public int? ChangeSetNumber { get; set; }



        /// <summary>
        /// Բոլոր պահատուփերը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<DepositCase> GetDepositCases(ulong customerNumber, ProductQualityFilter filter)
        {
            List<DepositCase> depositCases = new List<DepositCase>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                depositCases.AddRange(DepositCaseDB.GetDepositCases(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                depositCases.AddRange(DepositCaseDB.GetClosedDepositCases(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                depositCases.AddRange(DepositCaseDB.GetDepositCases(customerNumber));
                depositCases.AddRange(DepositCaseDB.GetClosedDepositCases(customerNumber));
            }
            return depositCases;
        }
        /// <summary>
        /// Վերադարձնում է մեկ պահատուփ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static DepositCase GetDepositCase(ulong productId, ulong customerNumber)
        {
            return DepositCaseDB.GetDepositCase(productId, customerNumber);
        }

        /// <summary>
        /// Ստուգում է պահատուփի համարի ազատ լինելը
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static bool CheckDepositCaseNumber(string caseNumber, int filialCode)
        {
            return DepositCaseDB.CheckDepositCaseNumber(caseNumber, filialCode);
        }

        /// <summary>
        /// Վերդարձնում է պահատուփերի քարտեզը
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static List<DepositCaseMap> GetDepositCaseMap(int filialCode, short caseSide)
        {
            return DepositCaseDB.GetDepositCaseMap(filialCode, caseSide);
        }

        /// <summary>
        /// Վերադարձնում է պահատուփի ակտիվացման համար գումարը
        /// </summary>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static double GetDepositCasePrice(string caseNumber, int filialCode, short contractDuration)
        {
            return DepositCaseDB.GetDepositCasePrice(caseNumber, filialCode, contractDuration);
        }
    }


}
