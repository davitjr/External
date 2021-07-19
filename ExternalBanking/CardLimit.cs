using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class CardLimit
    {
        /// <summary>
        /// Լիմիտի տեսակ
        /// </summary>
        public LimitType Limit { get; set; }

        /// <summary>
        /// Լիմիտի արժեք
        /// </summary>
        public double LimitValue { get; set; }

        /// <summary>
        /// Լիմիտը ԱրՔայում իդենտիֆիկանում է այս դաշտով։
        /// </summary>
        public string LimitArcaType { get; set; }
    }
}
