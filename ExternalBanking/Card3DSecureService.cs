using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class Card3DSecureService
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductID { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Էլ․հասցե
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// գործողության տեսակը
        /// 1-Գրանցել
        /// 2-Հանել
        /// </summary>
        public short ActionType { get; set; }

        /// <summary>
        /// գործողության տեսակի նկարագրություն
        /// </summary>
        public string ActionTypeDescription { get; set; }

        /// <summary>
        /// Մուտքագրման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        public short? ArcaResponse { get; set; }
        /// <summary>
        /// ՊԿ
        /// </summary>
        public int SetNumber { get; set; }
        public string RequestFileName { get; set; }
        public CardServiceQualities Quality { get; set; }
    }


}
