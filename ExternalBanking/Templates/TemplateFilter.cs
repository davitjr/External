using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Ձևանմուշի որոնման պարամետրեր
    /// </summary>
    public class TemplateFilter
    {
        /// <summary>
        /// Ձևանմուշի ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Ձևանմուշի անուն
        /// </summary>
        public string TemplateName { get; set; }
    }
}
