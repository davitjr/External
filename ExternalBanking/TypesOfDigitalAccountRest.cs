using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class TypesOfDigitalAccountRest
    {
        /// <summary>
        /// Կարգավորումների տեսակի ունիկալ համար
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Կարգավորման տեսակի նկարագրություն
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Եթե կարգավորումը փոփոխման ենթակա է True եթե ոչ False
        /// </summary>
        public bool IsEditable { get; set; }
        /// <summary>
        /// Լռելյայն արժեք
        /// </summary>
        public bool DefaultValue { get; set; }
        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
    }
}
