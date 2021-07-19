using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForREST
{
    public class Message
    {
        /// <summary>
        /// Հաղորդագրության ունիկալ համար (Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Հաղորդագրության վերնագիր
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Հաղորդագրության բովանդակություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ուղարկման ամսաթիվ
        /// </summary>
        public string SentDate { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public int Type { get; set; }
    }
}
