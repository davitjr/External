using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForREST
{
   public class ReferenceOrder:Order
    {
        /// <summary>
        /// Տեղեկանքի տեսակ
        /// </summary>
        public ushort ReferenceType { get; set; }
        /// <summary>
        /// Շարժի սկզբնաժամկետ
        /// </summary>
        public string DateFrom { get; set; }
        /// <summary>
        /// Շարժի վերջնաժամկետ
        /// </summary>
        public string DateTo { get; set; }
        /// <summary>
        /// Հաշվեհամարներ
        /// </summary>
        public List<Account> Accounts { get; set; }
        /// <summary>
        /// Տեղեկանքը նեկայացվելու է
        /// </summary>
        public ushort ReferenceEmbasy { get; set; }


        /// <summary>
        /// Եթե ընտրվում ե տեղեկանքը ներկայացվելու է "ԱՅԼ" տարբերակը
        /// </summary>
        public string ReferenceFor { get; set; }
        /// <summary>
        /// Տեղեկանքի ստացման մասնաճյուղ
        /// </summary>
        public int ReferenceFilial { get; set; }
        /// <summary>
        /// Նույն օրվա ընթացքում
        /// </summary>
        public ushort Urgent { get; set; }
        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }
        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }
        /// <summary>
        /// Տեղեկանքի լեզու
        /// </summary>
        public ushort ReferenceLanguage { get; set; }
    }
}
