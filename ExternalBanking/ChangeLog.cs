using ExternalBanking.DBManager;
using System;
namespace ExternalBanking
{
    /// <summary>
    /// Հնարավորություն է տալիս կատարել փոփոխությունների լոգավորում 
    /// </summary>
    public class ChangeLog
    {
        /// <summary>
        /// Փոփոխության գրառման հերթական համար
        /// </summary>
        public ulong Id { get; set; }
        /// <summary>
        /// Փոփոխության ենթարկված օբեկտի տեսակը
        /// </summary>
        public ObjectTypes ObjectType { get; set; }
        /// <summary>
        /// Փոփոխության ենթարկված օբեկտի հերթական համար
        /// </summary>
        public ulong ObjectID { get; set; }
        /// <summary>
        /// Կատարվող գործուղությունը 
        /// </summary>
        public Action Action { get; set; }
        /// <summary>
        /// Գործողության ամսաթիվ
        /// </summary>
        public DateTime ActionDate { get; set; }
        /// <summary>
        /// Գործողություն կատարող օգտագործող
        /// </summary>
        public int ActionSetNumber { get; set; }
        /// <summary>
        /// Գործողության նկարագրություն
        /// </summary>
        public string ActionDescription { get; set; }

        /// <summary>
        /// Փոփոխության գրանցում
        /// </summary>
        public void Insert()
        {
            ChangeLogDB.InsertChangeLog(this);
        }

        /// <summary>
        /// Փոփոխության գրանցում HB_Logins-ում
        /// </summary>
        public void InsertHBLoginsChangeLog()
        {
            ChangeLogDB.InsertHBLoginsChangeLog(this);
        }
    }
}