using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class ConsumeLoanApplicationOrder : Order
    {
        /// <summary>
        /// Վարկային պրոդուկտի տեսակ
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Վարկի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վարկի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Վարկի տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }


        /// <summary>
        /// Առաջին վճարման օր
        /// </summary>
        public DateTime FirstRepaymentDate { get; set; }

      
        /// <summary>
        /// Վարկային պրոդուկտի հաշիվ
        /// </summary>
        public Account ProductAccount { get; set; }

        /// <summary>
        /// Հաղորդակցման եղանակի նկարագրություն
        /// </summary>
        public string CommunicationTypeDescription { get; set; }

        /// <summary>
        /// Դրամային գումար
        /// </summary>
        public double AmountInAMD { get; set; }


        /// <summary>
        /// true` եթե հաճախորդը նշել է checkbox-ը, false հակառակ դեպքում
        /// </summary>
        public bool AcknowledgedByCheckBox { get; set; }

        /// <summary>
        /// Եթե checkbox-ը նշված է ապա checkbox-ի դիմաց ցուցադրված տեքստը, հակառակ դեպքում՝ հաճախորդի կողմից մուտքագրված տեքստը 
        /// </summary>
        public string AcknowledgementText { get; set; }
    }
}
