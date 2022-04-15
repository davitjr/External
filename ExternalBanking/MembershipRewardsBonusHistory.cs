using System;

namespace ExternalBanking
{
    public class MembershipRewardsBonusHistory
    {
        public int ID { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// MR ID (հերթական համար)
        /// </summary>
        public int MRID { get; set; }

        /// <summary>
        /// Բոնուսի գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Բոնուսային միավորներ
        /// </summary>
        public float BonusScores { get; set; }

        /// <summary>
        /// Դեբետ կամ Կրեդիտ
        /// </summary>
        public string DebetCredit { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Description { get; set; }


        public ushort ReasonId { get; set; }

        /// <summary>
        /// Պատասխանատու կատարող
        /// </summary>
        public int SetNumber { get; set; }

        public int UniqueNumber { get; set; }

        public int turbo_acs_id { get; set; }

        /// <summary>
        /// Միավորների վերջին հաշվարկման օրը
        /// </summary>
        public DateTime? LastDayOfBonusCalculation { get; set; }

        public long TransferGroupNumber { get; set; }

        /// <summary>
        /// Քարտային գործարքի համար
        /// </summary>
        public int ClearingID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TransactionNumber { get; set; }


    }
}
