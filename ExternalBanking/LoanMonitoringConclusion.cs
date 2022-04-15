using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LoanMonitoringConclusion
    {
        /// <summary>
        /// Վարկի մոնիտորինգի ունիկալ համար
        /// </summary>
        public long MonitoringId { get; set; }

        /// <summary>
        /// Մոնիտորինգի ենթարկվող պրոդուկտի համար
        /// </summary>
        public long LoanProductId { get; set; }

        /// <summary>
        /// Մոնիտորինգի եզրակացություն
        /// </summary>
        public short Conclusion { get; set; }

        /// <summary>
        /// Մոնիտորինգի եզրակացության նկարագրություն
        /// </summary>
        public string ConclusionDescription { get; set; }

        /// <summary>
        /// Մոնիտորինգի տեսակ
        /// </summary>
        public short MonitoringType { get; set; }

        /// <summary>
        /// Մոնիտորինգի տեսակի նկարագրություն
        /// </summary>
        public string MonitoringTypeDescription { get; set; }

        /// <summary>
        /// Մոնիթորինգի ենթատեսակ
        /// </summary>
        public short MonitoringSubType { get; set; }

        /// <summary>
        /// Մոնիթորինգի ենթատեսակի նկարագրություն
        /// </summary>
        public string MonitoringSubTypeDescription { get; set; }
        /// <summary>
        /// Մոնիտորինգի ա/թ
        /// </summary>
        public DateTime MonitoringDate { get; set; }

        /// <summary>
        /// Մոնիթորինգ իրականցրած աշխատակից
        /// </summary>
        public int MonitoringSetNumber { get; set; }
        /// <summary>
        /// Հիմնական գործոններ
        /// </summary>
        public List<MonitoringConclusionFactor> MonitoringFactors { get; set; }

        /// <summary>
        /// Հասույթները նվազել են թե ոչ
        /// </summary>
        public bool ProfitReduced { get; set; }

        /// <summary>
        /// Հասույթների նվազման տեսակ
        /// </summary>
        public short ProfitReduceType { get; set; }

        /// <summary>
        /// Հասույթների նվազման տեսակի նկարագրություն
        /// </summary>
        public string ProfitReduceTypeDescritpion { get; set; }
        /// <summary>
        /// Հասույթների նվազման չափ
        /// </summary>
        public float ProfitReductionSize { get; set; }

        /// <summary>
        /// Գրավի վիճակի վերաբերյալ եզրակացություն
        /// </summary>
        public List<short> ProvisionQualityConclusion { get; set; }

        /// <summary>
        /// Գրավի վիճակի վերաբերյալ եզրակացության նկարագրություն
        /// </summary>
        public string ProvisionQualityConclusionsDescription { get; set; }
        /// <summary>
        /// Գրավի շուկայական արժեքի վերաբերյալ եզրակացություն 
        /// </summary>
        public short ProvisionCostConclusion { get; set; }

        /// <summary>
        /// Գրավի շուկայական արժեքի վերաբերյալ եզրակացության նկարագրություն 
        /// </summary>
        public string ProvisionCostConclusionDescription { get; set; }
        /// <summary>
        /// Գրավի ծածկման գործակից
        /// </summary>
        public float ProvisionCoverCoefficient { get; set; }

        /// <summary>
        /// Հաճախորդի և փոխկապակցվածների պարտավորություններ
        /// </summary>
        public List<MonitoringConclusionLinkedLoan> LinkedMonitoringLoans { get; set; }

        /// <summary>
        /// Մեկնաբանություններ
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Կարգավիճակ(1-պահպանված,2-հաստատված)
        /// </summary>
        public short Status { get; set; }

        public LoanMonitoringConclusion()
        {
            this.LinkedMonitoringLoans = new List<MonitoringConclusionLinkedLoan>();
            this.MonitoringFactors = new List<MonitoringConclusionFactor>();
        }

        public static List<LoanMonitoringConclusion> GetLoanMonitoringConclusions(long productId)
        {
            return LoanMonitoringConclusionDB.GetConclusions(productId);
        }

        public static LoanMonitoringConclusion GetLoanMonitoringConclusion(long monitoringId, long productId)
        {
            return LoanMonitoringConclusionDB.Get(monitoringId, productId);
        }

        public void GetFactors()
        {
            this.MonitoringFactors = LoanMonitoringConclusionDB.GetFactors(this.MonitoringId);
        }

        public void GetProvisionQualityConclusion()
        {
            this.ProvisionQualityConclusion = LoanMonitoringConclusionDB.GetProvisionQualityConclusion(this.MonitoringId, this.LoanProductId);
        }

        public void GetLinkedMonitoringLoans()
        {
            this.LinkedMonitoringLoans = LoanMonitoringConclusionDB.GetLinkedMonitoringLoans(this.MonitoringId);
        }

        public ActionResult Save(ACBAServiceReference.User user)
        {
            return LoanMonitoringConclusionDB.SaveMonitoringConclusion(this, user.userID);
        }

        public static ActionResult Approve(long monitoringId)
        {
            ActionResult result = Validate(monitoringId);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = LoanMonitoringConclusionDB.ApproveMonitoringConclusion(monitoringId);
            return result;
        }

        public static ActionResult Delete(long monitoringId)
        {
            return LoanMonitoringConclusionDB.DeleteMonitoringConclusion(monitoringId);
        }

        public static float GetProvisionCoverCoefficient(long productId)
        {
            return LoanMonitoringConclusionDB.GetProvisionCoverCoefficient(productId);
        }
        public static List<MonitoringConclusionLinkedLoan> GetLinkedLoans(long productId, ulong customerNumber)
        {
            return LoanMonitoringConclusionDB.GetLinkedLoans(productId, customerNumber);
        }

        public static short GetLoanMonitoringType(int departmentId)
        {
            return LoanMonitoringConclusionDB.GetLoanMonitoringType(departmentId);
        }

        public static ActionResult Validate(long monitoringId)
        {
            ActionResult result = new ActionResult();
            return result;
        }

        public static bool IsSetProvisionConclusions(long monitoringId)
        {
            return LoanMonitoringConclusionDB.IsSetProvisionConclusions(monitoringId);
        }
    }

    public class MonitoringConclusionFactor
    {
        /// <summary>
        /// Հիմնական գործոն
        /// </summary>
        public short FactorId { get; set; }

        /// <summary>
        /// Հիմնական գործոնի նկարագրություն
        /// </summary>
        public string FactorDescription { get; set; }

    }

    public class MonitoringConclusionLinkedLoan
    {
        /// <summary>
        /// Փոխկապակցված վարկի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// Փոխկապակցված վարկի հաճախորդի համար
        /// </summary>
        public long CustomerNumber { get; set; }
        /// <summary>
        /// Փոխկապակցված վարկի հաճախորդի անվանում
        /// </summary>
        public string CustomerDescription { get; set; }
        /// <summary>
        /// Փոխկապակցված վարկի գումար
        /// </summary>
        public double StartCapital { get; set; }
        /// <summary>
        /// Փոխկապակցված վարկի արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Փոխկապակցված վարկի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Փոխկապակցված վարկի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Վարկը կցված է տվյալ մոնիտորինգին թե ոչ
        /// </summary>
        public bool Linked { get; set; }

        /// <summary>
        /// Մոնիթորինգի եզրակացության գլխավոր պրոդուկտ
        /// </summary>
        public bool MainProduct { get; set; }
    }
}
