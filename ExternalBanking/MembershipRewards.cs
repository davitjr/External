using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;


namespace ExternalBanking
{
   public class MembershipRewards
    {
       /// <summary>
       /// MR-ի համար
       /// </summary>
       public int Id { get; set; }

       /// <summary>
       /// MR կարգավիճակ
       /// </summary>
       public ushort Status { get; set; }

       /// <summary>
       /// MR կարգավիճակի նկարագրություն
       /// </summary>
       public string StatusDescription { get; set; }

       /// <summary>
       /// MR գրանցման ա/թ
       /// </summary>
       public DateTime RegistrationDate { get; set; }

       /// <summary>
       /// MR վերջի ա/թ
       /// </summary>
       public DateTime EndDate { get; set; }

       /// <summary>
       /// MR փակման ա/թ
       /// </summary>
       public DateTime? ClosingDate { get; set; }

       /// <summary>
       /// MR գործողության ժամկետ
       /// </summary>
       public DateTime? ValidationDate { get; set; }

       /// <summary>
       /// MR անդամակցության վճար
       /// </summary>
       public double ServiceFee { get; set; }

       /// <summary>
       /// MR անդամակցության վճարման ենթակա գումար
       /// </summary>
       public double ServiceFeeReal { get; set; }

       /// <summary>
       /// MR անդամակցության վճարված գումար
       /// </summary>
       public double ServiceFeePayed { get; set; }

       /// <summary>
       /// Գրանցողի ՊԿ
       /// </summary>
       public int SetNumber { get; set; }

       /// <summary>
       /// Վերջին միավորների հաշվարկման օրը
       /// </summary>
       public DateTime LastDayOfBonusCalculation { get; set; }

       /// <summary>
       /// MR բոնուսային միավորների մնացորդ
       /// </summary>
       public double BonusBalance { get; set; }

        /// <summary>
        /// MR սպասարկման վարձի վճարման ա/թ
        /// </summary>
        public DateTime? FeePaymentDate { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }


        public static MembershipRewards GetCardMembershipRewards(string cardNumber)
       {
           return MembershipRewardsDB.GetCardMembershipRewards(cardNumber);
       }

        internal static MembershipRewards GetCardMembershipRewardByID(long ID)
        {
            return MembershipRewardsDB.GetCardMembershipRewardByID(ID);
        }

       public static List<MembershipRewardsStatusHistory> GetCardMembershipRewardsStatusHistory(string cardNumber)
       {
           return MembershipRewardsDB.GetCardMembershipRewardsStatusHistory(cardNumber);
       }

        public static List<MembershipRewardsBonusHistory> GetCardMembershipRewardsBonusHistory(string cardNumber, DateTime startDate, DateTime endDate)
        {
            return MembershipRewardsDB.GetCardMembershipRewardsBonusHistory(cardNumber, startDate, endDate);
        }
        

    }
}
