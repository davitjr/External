using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Աշխատանքային ծրագրի սակագներ
    /// </summary>
    public class CardTariff
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public int CardType { get; set; }

        /// <summary>
        /// Անվանում
        /// </summary>
        public string CardTypeDescription { get; set; }
                
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Կանխիկացման % ACBA-ի կետերում
        /// </summary>
        public double CashRateOur { get; set; }

        /// <summary>
        /// Այլ կետում կանխ %
        /// </summary>
        public double CashRateOther { get; set; }

        /// <summary>
        /// Այլ կետում կանխ % (Միջազգային)
        /// </summary>
        public double CashRateInternational { get; set; }
        
        /// <summary>
        /// Ոչ տոկոսային ժ-ն
        /// </summary>
        public int GracePeriod { get; set; }

        /// <summary>
        /// Դրական %
        /// </summary>
        public double PositiveRate { get; set; }
        /// <summary>
        /// Բացասական %
        /// </summary>
        public double NegativeRate { get; set; }
        
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public byte Quality { get; set; }
        
        /// <summary>
        /// Կանխիկացման նվազագույն գանձվող գումար տեղական 
        /// </summary>
        public double MinFeeLocal { get; set; }

        /// <summary>
        /// Կանխիկացման նվազագույն գանձվող գումար միջազգային
        /// </summary>
        public double MinFeeInternational { get; set; }

        /// <summary>
        /// Կանխիկացման մուտքի % ACBA
        /// </summary>
        public double CashInFeeRateOur { get; set; }

        /// <summary>
        /// Կանխիկացման մուտքի % այլ
        /// </summary>
        public double CashInFeeRateOther { get; set; }

        /// <summary>
        /// SMS միջն/վճար հաճախորդից
        /// </summary>
        public double SMSFeeFromCustomer { get; set; }

        /// <summary>
        /// SMS միջն/վճար բանկից
        /// </summary>
        public double SMSFeeFromBank { get; set; }

        /// <summary>
        /// Քարտից քարտ փոխանցում` մեր քարտերին
        /// </summary>
        public double CardToCardFeeOur { get; set; }

        /// <summary>
        /// Քարտից քարտ փոխանցում` այլ քարտերին
        /// </summary>
        public double CardToCardFeeOther { get; set; }

        public int CardSystem { get; set; }

        public int Period { get; set; }

        public float ServiceFee { get; set; }
        /// <summary>
        /// Քարտի գործողության ժամկետ
        /// </summary>
        public int CardValidityPeriod { get; set; }
    }
}
