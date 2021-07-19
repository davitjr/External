using System;
namespace ExternalBanking
{
    public class CrossExchangeRate
    {
        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        /// EUR վաճառք
        /// </summary>
        public double EURSale { get; set; }
        /// <summary>
        /// EUR գնում
        /// </summary>
        public double EURBuy { get; set; }
        /// <summary>
        /// USD վաճառք
        /// </summary>
        public double USDSale { get; set; }
        /// <summary>
        /// USD գնում
        /// </summary>
        public double USDBuy { get; set; }
        /// <summary>
        /// RUR վաճառք
        /// </summary>
        public double RURSale { get; set; }
        /// <summary>
        /// RUR գնում
        /// </summary>
        public double RURBuy { get; set; }
        /// <summary>
        /// GBP վաճառք
        /// </summary>
        public double GBPSale { get; set; }
        /// <summary>
        /// GBP գնում
        /// </summary>
        public double GBPBuy { get; set; }
        /// <summary>
        /// CHF վաճառք
        /// </summary>
        public double CHFSale { get; set; }
        /// <summary>
        /// CHF գնում
        /// </summary>
        public double CHFBuy { get; set; }


    }
}
