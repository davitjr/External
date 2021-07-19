using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class DigitalAccountRestConfigurationItem
    {
        /// <summary>
        /// Կարգավորման ունիկալ համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հեռահար բանկինգի օգտագործողի ունիկալ համար
        /// </summary>
        public int DigitalUserId { get; set; }

        /// <summary>
        /// Ընդհանուր հասանելի մնացորդի տարբերակ
        /// </summary>
        public int ConfigurationTypeId { get; set; }

        /// <summary>
        /// Կարգավորման տեսակ
        /// </summary>
        public int AccountRestTypeId { get; set; }

        /// <summary>
        /// Կարգավորման արժեք (եթե հաճախորդը նշել է կարգավորումը ապա True հակառակ դեպքում False)
        /// </summary>
        public bool AccountRestAttributeValue { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Կարգավորման նկարագրություն
        /// </summary>
        public string AccountRestTypeDescription { get; set; }

    }
}
