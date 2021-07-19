using System;
using System.Collections.Generic;
using ExternalBanking.DBManager;


namespace ExternalBanking
{
    /// <summary>
    /// Հայտի պատմություն
    /// </summary>
    public class OrderHistory
    {
        /// <summary>
        /// Հայտի ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public OrderQuality Quality { get; set; }
        /// <summary>
        /// Փոփոխման ա/թ(մուտքագրում,հաստատում և այլն....)
        /// </summary>
        public DateTime ChangeDate { get; set; }

        /// <summary>
        /// Փոփոխողի անուն
        /// </summary>
        public string ChangeUserName { get; set; }

        /// <summary>
        /// Փոփոխողի համար
        /// </summary>
        public long ChangeUserId { get; set; }

        /// <summary>
        /// Փոփոխման պատճառ
        /// </summary>
        public ushort ReasonId { get; set; }

        /// <summary>
        /// Փոփոխման պատճառի նկարագրություն
        /// </summary>
        public string ReasonDescription { get; set; }

        /// <summary>
        /// User-ի անուն/ազգանուն
        /// </summary>
        public string CustomerFullName { get; set; }


        /// <summary>
        /// Գործողության տեսակ
        /// </summary>
        public string  ActionDescription { get; set; }
    }
}
