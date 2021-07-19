using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class UserMessage
    {
        /// <summary>
        /// Աշխատակցի համար
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Հաղորդագրության վերնագիր
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Հաղորդագրության բովանդակություն
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Հաղորդագրության տեսակ
        /// </summary>
        public int MessageType { get; set;}
        /// <summary>
        ///  Հաղորդագրության տեսակի նկարագրությունը
        /// </summary>
        public string MessageTypeDescription { get; set; }
        /// <summary>
        /// Կարդալու ամսաթիվ
        /// </summary>
        public DateTime? ReadDate { get; set; }
        /// <summary>
        /// Մուտքագրման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

    }
}
