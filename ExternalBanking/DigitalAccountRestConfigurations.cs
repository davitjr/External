using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class DigitalAccountRestConfigurations
    {
        /// <summary>
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի որոշման տարբերակների (default, custom...) աղյուսակ
        /// </summary>
        public List<DigitalAccountRestConfigurationItem> Configurations { get; set; }

        /// <summary>
        /// Ընդհանուր հասանելի մնացորդի տեսակ
        /// </summary>
        public DigitalAccountRestConfigurationType ConfigurationType { get; set; }

        /// <summary>
        /// Ընդհանուր հասանելի մնացորդի տեսակի նկարագորություն
        /// </summary>
        public string ConfigurationTypeDescription { get; set; }

        public DigitalAccountRestConfigurations()
        {
            Configurations = new List<DigitalAccountRestConfigurationItem>();
        }

        /// <summary>
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի Կարգավորումների ստացում
        /// </summary>
        /// <param name="DigitalUserId"></param>
        /// <returns></returns>
        public DigitalAccountRestConfigurations GetCustomerAccountRestConfig(int DigitalUserId, ulong customerNumber, int lang) => DigitalAccountRestConfigurationsDB.GetCustomerAccountRestConfig(DigitalUserId, customerNumber, lang);

        /// <summary>
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի կարգավորումների փոփոխում
        /// </summary>
        /// <param name="DigitalUserId"></param>
        /// <returns></returns>
        public ActionResult UpdateCustomerAccountRestConfig(List<DigitalAccountRestConfigurationItem> ConfigurationItems) => DigitalAccountRestConfigurationsDB.UpdateCustomerAccountRestConfig(ConfigurationItems);

        /// <summary>
        /// ACBA Digital տիրույթում ընդհանուր հասանելի մնացորդի կարգավորումների նախնական վիճակի բերում
        /// </summary>
        /// <param name="DigitalUserId"></param>
        /// <returns></returns>
        public ActionResult ResetCustomerAccountRestConfig(int DigitalUserId, ulong customerNumber) => DigitalAccountRestConfigurationsDB.ResetCustomerAccountRestConfig(DigitalUserId, customerNumber);
        
    }
}
