using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{   
    /// <summary>
    /// Հայտի/երի որոնման պարամետրեր
    /// </summary>
    public class SearchOrders
    {

        /// <summary>
        /// Հանձնարարականի ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
       public ulong CustomerNumber {get;set;}

        /// <summary>
        /// Սկիզբ (հայտի ա/թ)
        /// </summary>
       public DateTime? DateFrom {get;set;}

        /// <summary>
       /// Վերջ (հայտի ա/թ)
        /// </summary>
       public DateTime? DateTo {get;set;}

        /// <summary>
        /// Հայտի կարգավիճակ
        /// </summary>
       public OrderQuality OrderQuality { get; set; }
        
       /// <summary>
       /// Հայտի տեսակ 
       /// </summary>
       public OrderType Type { get; set; }

       /// <summary>
       /// Տվյալների աղբյուր
       /// </summary>
       public SourceType Source { get; set; }

       /// <summary>
       /// Աշխատակիցի համարը
       /// </summary>
       public int RegisteredUserID { get; set; }

        /// <summary>
        /// Հայտը պահպանած մասնաճյուղ
        /// </summary>
       public int OperationFilialCode { get; set; }

       /// <summary>
       /// Գումար
       /// </summary>
       public double Amount { get; set; }

        /// <summary>
        /// ԱԳՍ-ով փոխանցումների գործարքներ
        /// </summary>
        public bool IsATSAccountOrders { get; set; }

        /// <summary>
        /// Դրամարկղի մատյանի գործարքներ
        /// </summary>
        public bool IsCashBookOrder { get; set; }


        /// <summary>
        /// Ֆոնդերի գործարքներ
        /// </summary>
        public bool IsFondOrder { get; set; }


        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Անուն
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Հայրանուն
        /// </summary>
        public string MiddleName { get; set; }
        
        /// <summary>
        /// Անվանում
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// Փաստաթուղթ
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Վարկային կոդ
        /// </summary>
        public string LoanNumber { get; set; }

        /// <summary>
        /// Գործարքներ
        /// </summary>
        public int CurrentState { get; set; }

        /// <summary>
        /// Քարտի վերաթողարկում հայտի տեսակ
        /// </summary>
        public string CardRenewType { get; set; }
    }
}
