using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի սպասարկման վարձի տվյալներ
    /// </summary>
    public class CardServiceFee
    {
        /// <summary>
        /// Տարեկան սպասարկման վարձ
        /// </summary>
        public double ServiceFeeTotal { get; set; }

        /// <summary>
        /// Սպասարկման պարբ. գանձումների գումար (AMD)
        /// </summary>
        public double ServiceFee { get; set; }

        /// <summary>
        /// Սպասարկման պարբերականություն
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Առաջին գանձում
        /// </summary>
        public string FirstCharge { get; set; }

        /// <summary>
        /// Վճարված սպասարկման վարձ
        /// </summary>
        public double ServiceFeePayed { get; set; }

        /// <summary>
        /// Վճարվման ամսաթիվ վերջին
        /// </summary>
        public DateTime? LastDayOfServiceFeePayment { get; set; }

        /// <summary>
        /// Վճարվման ամսաթիվ հաջորդ
        /// </summary>
        public DateTime? NextDayOfServiceFeePayment { get; set; }

        /// <summary>
        /// Պարտք
        /// </summary>
        public double Debt { get; set; }

        /// <summary>
        /// Փոխարինման միջնորդավճար
        /// </summary>
        public double ReplacementFee { get; set; }

        /// <summary>
        /// Վճարված փոխարինման միջնորդավճար
        /// </summary>
        public double PayedReplacementFee { get; set; }

    }
}
