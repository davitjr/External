using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Ժամկետանցի պատմություն
    /// </summary>
    public class OverdueDetail
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long Productid { get; set; }
        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public short ProductType { get; set; }
        /// <summary>
        /// Պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }
        /// <summary>
        /// Եթե true հարգելի false անհարգելի
        /// </summary>
        public bool Checked { get; set; }
        /// <summary>
        /// Պրոդուկտի սկիզբ
        /// </summary>
        public DateTime ProductStartDate { get; set; }
        /// <summary>
        /// Պրոդուկտի վերջ
        /// </summary>
        public DateTime ProductEndDate { get; set; }
        /// <summary>
        /// Մայր գումար
        /// </summary>
        public double StartCapital { get; set; }
        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Տոկոսագումար
        /// </summary>
        public double RateAmount { get; set; }
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
        /// Հաճախորդի ամբողջ ժամկետանցի պատմություն
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<OverdueDetail> GetOverdueDetails(ulong customerNumber)
        {
            return OverdueDetailDB.GetOverdueDetails(customerNumber);
        }

        /// <summary>
        /// Հաճախորդի մեկ պրոդուկտի ժամկետանցի պատմություն
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<OverdueDetail> GetCurrentProductOverdueDetails(ulong customerNumber, long productId)
        {
            List<OverdueDetail> details = OverdueDetailDB.GetOverdueDetails(customerNumber);
            details.RemoveAll(m => m.Productid != productId);
            return details;
        }

        /// <summary>
        /// Հաճախորդի մեկ ժամկետանց պատմության ուղղում
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="updateReason"></param>
        public static void GenerateLoanOverdueUpdate(long productId, DateTime startDate, DateTime? endDate, string updateReason, short setNumber) => OverdueDetailDB.GenerateLoanOverdueUpdate(productId, startDate, endDate, updateReason, setNumber);
        

    }
}
