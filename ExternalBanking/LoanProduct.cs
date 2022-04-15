using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LoanProduct
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public Account LoanAccount { get; set; }

        /// <summary>
        /// Կցված ընթացիկ հաշիվ
        /// </summary>
        public Account ConnectAccount { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի Տեսակ
        /// </summary>
        public short ProductType { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }

        /// <summary>
        /// Գումար	
        /// </summary>
        public double StartCapital { get; set; }

        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double CurrentCapital { get; set; }

        /// <summary>
        /// Կուտակված տոկոսագումար
        /// </summary>
        public double CurrentRateValue { get; set; }

        /// <summary>
        /// Որից ժամկետանց
        /// </summary>
        public double InpaiedRestOfRate { get; set; }

        /// <summary>
        /// Տուգանային տոկոսագումար
        /// </summary>
        public double PenaltyRate { get; set; }


        /// <summary>
        /// Որից ժամկետանց
        /// </summary>
        public double PenaltyAdd { get; set; }

        /// <summary>
        /// Ընդամենը տոկոսագումար
        /// </summary>
        public double TotalRateValue { get; set; }

        /// <summary>
        /// Տոկոսագումարի հաշվարկի օր
        /// </summary>
        public DateTime? DayOfRateCalculation { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public ulong ContractNumber { get; set; }

        /// <summary>
        /// Գլխավոր պայմանագրի համար
        /// </summary>
        public string MainContractNumber { get; set; }

        /// <summary>
        /// Դուրսգրված մաս
        /// </summary>
        public double OutCapital { get; set; }

        /// <summary>
        /// Դուրսգրված տուգանք
        /// </summary>
        public double OutPenalty { get; set; }

        /// <summary>
        /// Պետ տուրքի փոխանցումից կուտակված տուժանք
        /// </summary>
        public double JudgmentRate { get; set; }

        /// <summary>
        /// Պետ տուրքի փոխանցումից կուտակված վճարված չվճարված տուժանք
        /// </summary>
        public double TotalJudgmentPenaltyRate { get; set; }

        /// <summary>
        /// Դատական պրոցեսի սկիզբ
        /// </summary>
        public DateTime? JudgmentStartDate { get; set; }

        /// <summary>
        /// Դատական պրոցեսի վերջ
        /// </summary>
        public DateTime? JudgmentEndDate { get; set; }

        /// <summary>
        /// Դատական տուգանային տոկոսադրույք
        /// </summary>
        public float JudgmentPenaltyPercent { get; set; }
        /// <summary>
        /// Ժամկետանց գումար
        /// </summary>
        public double OverdueCapital { get; set; }

        /// <summary>
        /// Փաստացի տոկոսադրույք(առանց ծախսերի)
        /// </summary>
        public float InterestRateEffective { get; set; }

        /// <summary>
        /// Փաստացի տոկոսադրույք(ծախսերով)
        /// </summary>
        public float InterestRateFull { get; set; }

        /// <summary>
        /// Վերջին վճարման օր
        /// </summary>
        public DateTime? LastDateOfRateRepair { get; set; }

        /// <summary>
        /// Ժամկետանցի ա/թ
        /// </summary>
        public DateTime? OverdueLoanDate { get; set; }

        /// <summary>
        /// Ֆոնդ
        /// </summary>
        public short Fond { get; set; }

        /// <summary>
        /// Ֆոնդի նկարագրություն
        /// </summary>
        public string FondDescription { get; set; }

        /// <summary>
        /// Ծրագիր
        /// </summary>
        public short LoanProgram { get; set; }

        /// <summary>
        /// Ծրագրի նկարագրություն
        /// </summary>
        public string LoanProgramDescription { get; set; }

        /// <summary>
        /// Ակցիա
        /// </summary>
        public short Sale { get; set; }

        /// <summary>
        /// Ակցիայի նկարագրություն
        /// </summary>
        public string SaleDescription { get; set; }

        /// <summary>
        /// Վարկի մասնաճյուղ
        /// </summary>
        public int FillialCode { get; set; }

        /// <summary>
        /// Որոշում ենք վարկը ունի դատական գործընթաց թե ոչ
        /// </summary>
        public bool HasClaim { get; set; }

        /// <summary>
        /// Տուժանքի օրական տոկոսադրույք
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public float DailyPenaltyInterestRate { get; set; }

        /// <summary>
        /// Չվճարված գումար
        /// </summary>
        public double AmountNotPaid { get; set; }

        /// <summary>
        /// Վճարված  տոկոսագումար
        /// </summary>
        public double MaturedCurrentRateValue { get; set; }

        /// <summary>
        /// Ընդամենը վճարված տուժանք
        /// </summary>
        public double MaturedPenaltyRate { get; set; }

        /// <summary>
        /// Պետ. տուրքի փոխանցումից հետո վճարված տուժանք
        /// </summary>
        public double MaturedJudgmentPenaltyRate { get; set; }

        /// <summary>
        /// Վարկային կոդ
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        /// Տոկոսի հաշվարկի դադարեցման օր
        /// </summary>
        public DateTime? DateOfStoppingCalculation { get; set; }

        /// <summary>
        /// Տուժանքի հաշվարկի դադարեցման օր
        /// </summary>
        public DateTime? DateOfStoppingPenaltyCalculation { get; set; }

        /// <summary>
        /// Ժամկետանցի օր դասակարգման համար
        /// </summary>
        public DateTime? OverdueLoanDateForClassification { get; set; }

        /// <summary>
        /// Դուրսգրման ա/թ
        /// </summary>
        public DateTime? OutLoanDate { get; set; }

        /// <summary>
        /// Հետհաշվեկշռից հանելու ա/թ
        /// </summary>
        public DateTime? OutFromOutbalDate { get; set; }

        /// <summary>
        /// Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ      
        /// </summary>
        public string NextPeriodRateAccount { get => LoanProduct.GetNextPeriodRateAccount(this.ProductId); set { } }

        /// <summary>
        /// Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշվի մնացորդ
        /// </summary>
        public double NextPeriodRateAccountBalance { get => LoanProduct.GetNextPeriodRateAccountBalance(this.NextPeriodRateAccount); set { } }

        public double? InterestRateEffectiveWithOnlyBankProfit { get; set; }

        /// <summary>
        /// Պայմանագրի ամսաթիվ (հայտի մուտքագրման օրացուցային ամսաթիվ)
        /// </summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>
        /// Պայմանագրի գումար (24/7 վարկի դեպքում հայտի մասնակի կատարված կարգավիճակի պարագայում գումարը հաշվարկվում է այլ կերպ)
        /// </summary>
        public double ContractAmount { get; set; }



        public static double GetPenaltyRateForDate(DateTime date)
        {
            return LoanProductDB.GetPenaltyRateForDate(date);
        }


        public static List<LoanProductProlongation> GetLoanProductProlongations(ulong productId)
        {
            return LoanProductDB.GetLoanProductProlongations(productId);
        }

        public static bool CheckLoanProductClaimAvailability(long productId)
        {
            return LoanProductDB.CheckLoanProductClaimAvailability(productId);
        }

        /// <summary>
        /// Վերադարձնում է վարկային կոդը կախված ունիկալ համարից
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static string GetCreditCode(long productId, short productType)
        {
            return LoanProductDB.GetCreditCode(productId, productType);
        }

        /// <summary>
        /// Վերադարձնում է վարկի ունիկալ համարը 
        /// </summary>
        /// <param name="creditCode"></param>
        /// <returns></returns>
        public static string GetApplicationIdByCreditCode(string creditCode)
        {
            return LoanProductDB.GetApplicationIdByCreditCode(creditCode);
        }
        public static ulong GetCustomerNumberByLoanApp(ulong productID)
        {
            return LoanProductDB.GetCustomerNumberByLoanApp(productID);
        }


        internal static short CheckForEffectiveSign(int loanType)
        {
            return LoanProductDB.CheckForEffectiveSign(loanType);
        }


        /// <summary>
        /// Վերադարձնում է ապագա ժամանակաշրջանի տոկոսի հաշիվը կախված ունիկալ համարից
        /// </summary>
        public static string GetNextPeriodRateAccount(long productId)
        {
            return LoanProductDB.GetNextPeriodRateAccount(productId);
        }


        /// <summary>
        /// Վերադարձնում է ապագա ժամանակաշրջանի տոկոսի հաշվի մնացորդը կախված ունիկալ համարից
        /// </summary>
        public static double GetNextPeriodRateAccountBalance(string nextPeriodRateAccount)
        {
            return LoanProductDB.GetNextPeriodRateAccountBalance(nextPeriodRateAccount);
        }
    }
}
