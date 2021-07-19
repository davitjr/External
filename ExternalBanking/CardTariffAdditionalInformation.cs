using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class CardTariffAdditionalInformation
    {   
        /// <summary>
        ///Միջնորդավճար կազմակերպությունից
        /// </summary>
        public double ServiceFeeFromOrganization { get; set; }

        /// <summary>
        /// Գանձման պարբերականություն
        /// </summary>
        public int PeriodFromOrganization { get; set; }

        /// <summary>
        /// Միջնորդավճար կազմակերպությունից տոտալ
        /// </summary>
        public double ServiceFeeTotalFromOrganization { get; set; }

        /// <summary>
        /// Միջնորդավճարի գանձում կատարվում  է սկզբում կամ վերջում
        /// </summary>
        public int DeductionStartFromOrganization { get; set; }

        /// <summary>
        /// Մինիմալ մնացորդ (դրամով)
        /// </summary>
        public double MinRest { get; set; }

        /// <summary>
        /// Փոխարինման միջնորդավճար կազմակերպությունից 
        /// </summary>
        public double ReplaceFeeFromOrganization { get; set; }

        /// <summary>
        /// Փոխարինման միջնորդավճար
        /// </summary>
        public double ReplaceFee { get; set; }

        /// <summary>
        /// Վերաթողարկման միջնորդավճար կազմակերպությունից 
        /// </summary>
        public double ReNewFeeFromOrganization { get; set; }

       public static CardTariffAdditionalInformation GetCardTariffAdditionalInformation (int officeID, int cardType)
        {
            return Info.GetCardTariffAdditionalInformation(officeID, cardType);
        }
    }
}
