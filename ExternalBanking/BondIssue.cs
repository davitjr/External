using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public class BondIssue
    {
        #region Properties
        /// <summary>
        /// Արժեթղթի ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Արժեթուղթ թողարկող կազմակերպություններ
        /// </summary>
        public BondIssuerType IssuerType { get; set; }

        /// <summary>
        /// Արժեթուղթ թողարկող կազմակերպությունների նկարագրություններ
        /// </summary>
        public string IssuerTypeDescription { get; set; }

        /// <summary>
        /// Տվյալ թողարկման արժեթղթի ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Արժեթղթի արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Թողարկման ընդհանուր ծավալ(Թողարկման ընդհանուր անվանական արժեք)
        /// </summary>
        public double TotalVolume { get; set; }

        /// <summary>
        /// Մեկ արժեթղթի անվանական արժեք
        /// </summary>
        public double NominalPrice { get; set; }

        /// <summary>
        /// Պարտատոմսի շրջանառության ժամկետ  ( ամիսներ )
        /// </summary>
        public int EditionCirculation { get; set; }

        /// <summary>
        /// Տարեկան արժեկտրոնային դրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Արժեկտրոնների վճարման պարբերականություն
        /// </summary>
        public int CouponPaymentPeriodicity { get; set; }

        /// <summary>
        /// Պարտատոմսերի մարման օր
        /// </summary>
        public DateTime? RepaymentDate { get; set; }

        /// <summary>
        /// Արժեթղթերի տեղաբաշխման սկիզբ 
        /// </summary>
        public DateTime ReplacementDate { get; set; }

        /// <summary>
        /// Արժեթղթերի տեղաբաշխման ավարտ (նախատեսվող)
        /// </summary>
        public DateTime ReplacementEndDate { get; set; }

        /// <summary>
        /// Պարտատոմսերի քանակ
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Մեկ ներդրողի նկատմամբ կիրառվող ձեռք բերվող պարտատոմսերի ծավալի սահմանափակում՝ այն է նվազագույն ձեռք բերման քանակ
        /// </summary>
        public int MinSaleQuantity { get; set; }

        /// <summary>
        /// Մեկ ներդրողի նկատմամբ կիրառվող ձեռք բերվող պարտատոմսերի ծավալի սահմանափակում՝ այն է առավելագույն ձեռք բերման քանակ
        /// </summary>
        public int MaxSaleQuantity { get; set; }

        /// <summary>
        /// Պարտատոմսի թողարկման կարգավիճակ
        /// </summary>
        public BondIssueQuality Quality { get; set; }

        /// <summary>
        /// Պարտատոմսի թողարկման կարգավիճաի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Պարտատոմսի գրանցման օր
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Պարտատոմսի ձեռք բերման համար կատարվող վճարման վերջնաժամկետ
        /// </summary>
        public TimeSpan PurchaseDeadlineTime { get; set; }

        /// <summary>
        /// Արժեթղթերի թողարկման ամսաթիվ 
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Պարտատոմսերի վճարման քանակ պարտատոմսի ողջ գործողության ընթացքում
        /// </summary>
        public int? CouponPaymentCount { get; set; }

        /// <summary>
        /// Պարտատոմսի ժամկետի տեսակի կոդ (1՝ Երկարաժամկետ, 2՝ կարճաժամկետ)
        /// </summary>
        public BondIssuePeriod PeriodType { get; set; }

        /// <summary>
        /// Պարտատոմսի ժամկետի տեսակի նկարագրություն (1՝ Երկարաժամկետ, 2՝ կարճաժամկետ)
        /// </summary>
        public string PeriodTypeDescription { get; set; }

        /// <summary>
        /// Բանկի՝ արժեթղթերի թողարկմանը համապատասխան պասիվային հաշիվ(Ռեսիդենտների համար)
        /// </summary>
        public Account BankAccount { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes ShareType { get; set; }

        /// <summary>
        /// Թողարկման սերիա
        /// </summary>
        public int? IssueSeria { get; set; }

        /// <summary>
        /// Բաժնետոմսերի Տեղաբաշխման փաստացի ավարտ
        /// </summary>
        public DateTime? ReplacementFactualEndDate { get; set; }

        /// <summary>
        /// Մեկ բաժնետոմսի տեղաբաշխման գին 
        /// </summary>
        public double? PlacementPrice { get; set; }

        ///// <summary>
        ///// Տեղաբաշխման ենթակա քանակ 
        ///// </summary>
        //public int? PlacementCount { get; set; }

        /// <summary>
        /// Տեղաբաշխման փաստացի քանակ 
        /// </summary>
        public int? PlacementFactualCount { get; set; }

        /// <summary>
        /// Գործողության նկարագրություն 
        /// </summary>
        public string OperationDescription { get; set; }

        /// <summary>
        /// Գործողության նկարագրություն 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Որոշման ամսաթիվ 
        /// </summary>
        public DateTime? DecisionDate { get; set; }

        /// <summary>
        /// Բանկի՝ արժեթղթերի թողարկմանը համապատասխան պասիվային հաշիվ Ոչ Ռեզիդենտների համար
        /// </summary>
        public Account BankAccountForNonResident { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// չտեղաբաշխված պարտատոմսերի քանակը՝ ըստ ԱՄՏԾ-ի
        /// </summary>
        public int NonDistributedBondsCount { get; set; }
        #endregion


        /// <summary>
        /// Պարտատոմսի թողարկման հաստատում
        /// </summary>
        /// <returns></returns>
        public ActionResult ApproveBondIssue(ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForApprove();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = BondIssueDB.ApproveBondIssue(this);
                if (result.ResultCode != ResultCode.Normal)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    result.ResultCode = BondIssueDB.SaveBondIssueHistory(this.ID, Action.Update, user.userID).ResultCode;
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Պարտատոմսի թողարկման հեռացում
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteBondIssue(ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForDelete();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = BondIssueDB.DeleteBondIssue(this);
                if (result.ResultCode != ResultCode.Normal)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    result.ResultCode = BondIssueDB.SaveBondIssueHistory(this.ID, Action.Delete, user.userID).ResultCode;
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Պարտատոմսի թողարկման պահպանում
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult SaveBondIssue(ACBAServiceReference.User user)
        {
            this.SetNumber = user.userID;

            this.Complete();
            ActionResult result = Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                Action action = this.ID == 0 ? Action.Add : Action.Update;
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = BondIssueDB.SaveBondIssue(this, action);
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        result.ResultCode = BondIssueDB.SaveBondIssueHistory(this.ID, action, user.userID).ResultCode;
                    }

                    scope.Complete();
                }
            }
            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Պարտատոմսի թողարկման Get
        /// </summary>
        /// <returns></returns>
        public BondIssue GetBondIssue()
        {
            return BondIssueDB.GetBondIssue(this);
        }

        /// <summary>
        /// Պարտատոմսի թողարկման ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (this.ShareType == SharesTypes.Bonds)
                result.Errors = Validation.ValidateBondIssue(this);
            else
                result.Errors = Validation.ValidateStockIssue(this);

            return result;
        }

        /// <summary>
        /// Պարտատոմսի ակտիվացման ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForApprove()
        {
            ActionResult result = new ActionResult();
            result.Errors = Validation.ValidateBondIssueForApprove(this);
            return result;
        }

        /// <summary>
        /// Պարտատոմսի հեռացման ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForDelete()
        {
            ActionResult result = new ActionResult();
            result.Errors = Validation.ValidateBondIssueForDelete(this);
            return result;
        }

        /// <summary>
        /// Լրացնում է պարտատոմսի թողարկման ավտոմատ լրացման դաշտերը
        /// </summary>
        public void Complete()
        {

            if (this.ID == 0)
            {
                this.RegistrationDate = DateTime.Now.Date;

                this.Quality = BondIssueQuality.New;
            }

            //Կորպորատիվ պարտատոմսեր
            if (this.IssuerType == BondIssuerType.ACBA)
            {
                if (this.ShareType == SharesTypes.Bonds)
                {
                    this.TotalCount = Convert.ToInt32(this.TotalVolume / this.NominalPrice);
                    this.RepaymentDate = this.ReplacementDate.AddMonths(this.EditionCirculation);
                    this.CouponPaymentCount = this.EditionCirculation / 12 * this.CouponPaymentPeriodicity;
                    this.IssueDate = this.ReplacementDate;
                    this.PeriodType = BondIssuePeriod.LongTerm;
                }
                else
                {
                    this.TotalCount = Convert.ToInt32(this.TotalVolume / this.NominalPrice);
                    //this.RepaymentDate = this.ReplacementDate.AddMonths(this.EditionCirculation);
                    //this.CouponPaymentCount = this.EditionCirculation / 12 * this.CouponPaymentPeriodicity;
                    //this.PeriodType = BondIssuePeriod.LongTerm;
                }
            }

        }

        /// <summary>
        /// Վերադարձնում է տվյալ պարտատոմսի արժեկտրոնների հաշվարկման օրերի ցանկը
        /// </summary>
        /// <returns></returns>
        public List<DateTime> CalculateCouponRepaymentSchedule()
        {

            List<DateTime> schedule = new List<DateTime>();
            if (this != null && this.CouponPaymentPeriodicity > 0 && this.CouponPaymentPeriodicity <= 12 && 12 % this.CouponPaymentPeriodicity == 0)
            {
                int monthsCount = 12 / this.CouponPaymentPeriodicity;

                DateTime startDate = this.ReplacementDate.Date.AddMonths(monthsCount);

                while (startDate <= this.RepaymentDate)
                {
                    schedule.Add(startDate);
                    startDate = startDate.AddMonths(monthsCount);
                }
            }


            return schedule;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ պարտատոմսի թողարկման արժեկտրոնների վճարման գրաֆիկը բազայից
        /// </summary>
        /// <returns></returns>
        public List<DateTime> GetCouponRepaymentSchedule()
        {
            return BondIssueDB.GetCouponRepaymentSchedule(this.ID);
        }

        /// <summary>
        /// Վերադարձնում է չտեղաբաշխված պարտատոմսերի քանակը՝ ըստ ԱՄՏԾ-ի
        /// </summary>
        /// <param name="ISIN">Պարտատոմսի ԱՄՏԾ</param>
        /// <returns></returns>
        public static int GetNonDistributedBondsCount(int bondIssueId)
        {
            BondIssueFilter bondIssueFilter = new BondIssueFilter
            {
                BondIssueId = bondIssueId
            };

            BondIssue bondIssue = BondIssueFilter.SearchBondIssues(bondIssueFilter).First();

            int distributedBondsCount = Bond.GetDistributedBondCount(bondIssueId, bondIssue.ShareType);

            int nonDistributedBondsCount = bondIssue.TotalCount - distributedBondsCount < 0 ? 0 : bondIssue.TotalCount - distributedBondsCount;

            return nonDistributedBondsCount;
        }

        public static double GetUnitPrice(int bondIssueId)
        {
            BondIssueFilter bondIssueFilter = new BondIssueFilter();
            bondIssueFilter.BondIssueId = bondIssueId;
            BondIssue bondIssue = BondIssueFilter.SearchBondIssues(bondIssueFilter).First();

            return bondIssue.PlacementPrice.Value;
        }

        public static bool IsACBASecurity(string ISIN)
        {
            return BondIssueDB.IsACBASecurity(ISIN);
        }

    }
}
