using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class InterestMargin
    {

        /// <summary>
        /// Տոկոսային մարժայի տեսակ
        /// </summary>
        public InterestMarginType marginType { get; set; }

        /// <summary>
        /// մարժայի ամիս
        /// </summary>
        public DateTime marginDate { get; set; }

        /// <summary>
        /// տոկոսադրույքներ
        /// </summary>
        public List<InterestMarginDetail> marginDetails { get; set; }

        public static InterestMargin GetInterestMarginDetails(InterestMarginType interestMarginType)
        {
            InterestMargin rate = InterestMarginDB.GetInterestMarginDetails(interestMarginType);

            return rate;
        }

        /// <summary>
        /// տոկոսադրույքներ ըստ ամսաթվի
        /// </summary>
        public static InterestMargin GetInterestMarginDetailsByDate(InterestMarginType interestMarginType, DateTime marginDate)
        {
            InterestMargin rate = InterestMarginDB.GetInterestMarginDetailsByDate(interestMarginType, marginDate);

            return rate;
        }

    }



    public class InterestMarginDetail
    {
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }
    }
}
