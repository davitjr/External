using Excel.FinancialFunctions;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExternalBanking
{
    /// <summary>
    /// Վաճառված պարտատոմս
    /// </summary>
    public class Bond
    {
        #region Properties

        /// <summary>
        /// Վաճառված պարտատոմսի ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long AppId { get; set; }

        /// <summary>
        /// Պարտատոմսի ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Ձեռք բերվող պարտատոմսերի քանակ
        /// </summary>
        public int BondCount { get; set; }

        /// <summary>
        /// Մեկ պարտատոմսի ձեռքբերման գինը
        /// </summary>
        public double UnitPrice { get; set; }

        /// <summary>
        /// Ձեռքբերվող պարտատոմսերի ընդհանուր գումար
        /// </summary>
        public double TotalPrice { get; set; }

        /// <summary>
        /// Տարածքային ստորաբաժանման համար
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Ընթացիկ հաշիվ
        /// </summary>
        public Account AccountForBond { get; set; }

        /// <summary>
        /// Արժույթային պարտատոմսերի դեպքում տոկոսագումարի վճարման
        /// համար նախատեսված տվյալ արժույթով ընթացիկ հաշիվ 
        /// </summary>
        public Account AccountForCoupon { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Գրանցման օր
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public BondQuality Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// HB-ի գործարքի համար
        /// </summary>
        public ulong HBDocId { get; set; }
        /// <summary>
        /// Հայտի համար
        /// </summary>
        public int DocumentNumber { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշվի առկայության տեսակի կոդ
        /// </summary>
        public DepositaryAccountExistence DepositaryAccountExistenceType { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշվի առկայության տեսակի նկարագրություն
        /// </summary>
        public string DepositaryAccountExistenceTypeDescription { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշիվ
        /// </summary>
        public DepositaryAccount CustomerDepositaryAccount { get; set; }

        /// <summary>
        /// Պարտատոմսի գումարի գանձման օր
        /// </summary>
        public DateTime AmountChargeDate { get; set; }

        /// <summary>
        /// Պարտատոմսի գումարի գանձման ժամ
        /// </summary>
        public TimeSpan AmountChargeTime { get; set; }

        /// <summary>
        /// Մերժման պատճառի կոդ
        /// </summary>
        public BondRejectReason RejectReasonId { get; set; }

        /// <summary>
        /// Մերժման պատճառի նկարագրություն
        /// </summary>
        public string RejectReasonDescription { get; set; }

        /// <summary>
        /// Թողարկման ունիկալ համար
        /// </summary>
        public int BondIssueId { get; set; }

        /// <summary>
        /// Պարտատոմսի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Պարտատոմսի տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes ShareType { get; set; }

        /// <summary>
        /// Գումարի ապահովում
        /// true- Գումարի գանձում false-Գումարի ապահովում ապագայում
        /// </summary>
        public bool? SecuringMoney { get; set; }

        /// <summary>
        /// Մասնակի բավարարված բաժնետոմսերի քանակ
        /// </summary>
        public int PartiallySatisfiedCount { get; set; }

        /// <summary>
        /// Թողարկման սերիա
        /// </summary>
        public int IssueSeria { get; set; }

        /// <summary>
        /// Արժեթուղթ թողարկող կազմակերպությունների նկարագրություններ
        /// </summary>
        public string IssuerTypeDescription { get; set; }
        #endregion

        /// <summary>
        /// Վերադարձնում է վաճառված մեկ պարտատոմսի մանրամասները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static Bond GetBondByID(int ID)
        {
            return BondDB.GetBondByID(ID);
        }


        /// <summary>
        /// Վաճառված պարտատոմսերի որոնում տրված պարամետրերով
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns></returns>
        public static List<Bond> GetBonds(BondFilter searchParams)
        {
            return BondDB.GetBonds(searchParams);
        }

        /// <summary>
        /// Վերադարձնում է պարտատոմսի լրիվ գինը(մաքուր գնի և կուտակված տոկոսագումարի հանրագումար):
        /// </summary>
        /// <param name="bondIssueId"></param>
        /// <returns></returns>
        public static double GetBondPrice(int bondIssueId)
        {
            BondIssue bondIssue = new BondIssue();

            BondIssueFilter filter = new BondIssueFilter();
            filter.BondIssueId = bondIssueId;

            bondIssue = BondIssueFilter.SearchBondIssues(filter).Count > 0 ? BondIssueFilter.SearchBondIssues(filter).First() : null;

            DateTime firstCouponRepaymentDate = default(DateTime);

            if (bondIssue != null)
            {
                firstCouponRepaymentDate = bondIssue.GetCouponRepaymentSchedule().Count > 0 ? bondIssue.GetCouponRepaymentSchedule().Min() : default(DateTime);
            }

            double rate = bondIssue.InterestRate;

            double priceWithoutPercent = Financial.Price(DateTime.Now.Date, bondIssue.RepaymentDate.Value, rate, rate, 100, (Frequency)bondIssue.CouponPaymentPeriodicity, DayCountBasis.ActualActual);
            double roundPriceWithoutPercent = Math.Round(priceWithoutPercent, 8);

            double roundaccumulativeInterest = 0;
            if (DateTime.Now.Date == bondIssue.ReplacementDate)
            {
                roundaccumulativeInterest = 0;
            }
            else
            {
                double accumulativeInterest = Financial.AccrInt(bondIssue.IssueDate, firstCouponRepaymentDate, DateTime.Now.Date, rate, 100, (Frequency)bondIssue.CouponPaymentPeriodicity, DayCountBasis.ActualActual);
                roundaccumulativeInterest = Math.Round(accumulativeInterest, 8);
            }
            double bondPriceFor100Nominal = roundPriceWithoutPercent + roundaccumulativeInterest;

            double bondPrice = Math.Round((bondPriceFor100Nominal * bondIssue.NominalPrice / 100), 4);

            return bondPrice;
        }

        public static List<Bond> GetBondsForDealing(BondFilter searchParams, string bondFilterType)
        {
            List<Bond> list = new List<Bond>();
            if (bondFilterType == "1")
            {
                list = GetBonds(searchParams);
                list.RemoveAll(b => DepositaryAccount.HasCustomerDepositaryAccount(b.CustomerNumber) && b.Quality != BondQuality.Deleted && b.Quality != BondQuality.Closed && b.Quality != BondQuality.Rejected && b.Quality != BondQuality.AvailableForApproveDilingBackOffice
                && b.Quality != BondQuality.Satisfied && b.Quality != BondQuality.PartiallySatisfied);
            }
            else if (bondFilterType == "2")
            {
                list = GetBonds(searchParams);
                list.RemoveAll(b => !DepositaryAccount.HasCustomerDepositaryAccount(b.CustomerNumber) && b.Quality != BondQuality.AvailableForApprove && b.Quality != BondQuality.AvailableForApproveDiling && b.Quality != BondQuality.Satisfied && b.Quality != BondQuality.PartiallySatisfied);
            }
            return list;
        }

        public static BondCertificateDetails GetBondCertificateDetailsByDocId(ulong docId)
        {
            return BondDB.GetBondCertificateDetailsByDocId(docId);
        }

        internal static int GetDistributedBondCount(int bondIssueId, SharesTypes shareType)
        {
            int distrbondCount = BondDB.GetDistributedBondCount(bondIssueId, shareType);
            return distrbondCount;
        }

        internal List<Bond> GetGovernmentBonds(ulong CustomerNumber)
        {
            return BondDB.GetGovernmentBonds(CustomerNumber);
        }
    }
}
