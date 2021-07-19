using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class LoanRepaymentDelayDetails
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductAppId { get; set; }

        /// <summary>
        /// Վարկի մարման վերսկսման ամսաթիվ
        /// </summary>
        public DateTime DelayDate { get; set; }
        
        /// <summary>
        /// Փոփոխման հիմք
        /// </summary>
        public string DelayReason { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int SetNumber { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public static LoanRepaymentDelayDetails GetLoanRepaymentDelayDetails(ulong productId)
        {
            return LoanDB.GetLoanRepaymentDelayDetails(productId);
        }

    }
}
