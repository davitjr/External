using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class RemittanceChangeHistory
    {
        /// <summary>
        /// Փոխանցման ունիկալ համար
        /// </summary>
        public ulong TransferId { get; set; }

        /// <summary>
        /// Փոխանցման կարգավիճակի փոփոխության պատմություն
        /// </summary>
        public List<RemittanceChangeHistoryItem> ChangeHistory { get; set; }

        /// <summary>
        /// Վերադարձնում է փոխանցման կարգավիճակի փոփոխության պատմությունը
        /// </summary>
        public void GetRemittanceChangeHistory ()
        {
            ChangeHistory = RemittanceChangeHistoryDB.GetRemittanceChangeHistory(TransferId);                
        }

    }

    public class RemittanceChangeHistoryItem
    {
        /// <summary>
        /// Կարգավիճակի անվանում (Նորմալ, ետ վերադարձրած, Ուղարկված/Վերադարձված, Ստացված/Չեղարկված, Փոփոխությունը հաստատված է)
        /// </summary>
        public string QualityName { get; set; }

        /// <summary>
        /// Փոփոխությունը կատարած աշխատակից
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Փոփոխության իրականացման ամսաթիվ, ժամ
        /// </summary>
        public DateTime ChangeDate { get; set; }
    }
}
