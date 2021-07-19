using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForREST
{
    class SearchCommunal
    {
        /// <summary>
        /// Կոմունալի տեսակ
        /// </summary>
        public short CommunalType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public short IsUtilityPayment { get; set; }
        /// <summary>
        /// Աբոնենտի տեսակ ֆիզ անձ,իրավ.անձ
        /// </summary>
        public short AbonentType { get; set; }
        /// <summary>
        /// Աբոնենտի համար
        /// </summary>
        public string AbonentNumber { get; set; }
        /// <summary>
        /// Հեոախոսի համար
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Կոմունալի մասնաճյուղ
        /// </summary>
        public string Branch { get; set; }  
    }
}
