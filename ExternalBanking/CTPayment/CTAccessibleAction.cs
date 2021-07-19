using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Արտաքին կանխիկ տերմինալի գործողությունների հասանելիություն
    /// </summary>
    public class CTAccessibleAction
    {
        /// <summary>
        /// Հասանելիության ունիկալ համար
        /// </summary>
        public CTAction Action { get; set; }
        
        /// <summary>
        /// Հասանելիություն ունենալու արժեք օր․՝1-այո,0-ոչ
        /// </summary>
        public uint Access { get; set; }

    }
}
