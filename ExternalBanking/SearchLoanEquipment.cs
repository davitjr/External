using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Վաճառված գրավի որոնում
    /// </summary>
    public class SearchLoanEquipment
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public double CustomerNumber { get; set; }
        
        /// <summary>
        /// Վարկի տրամադրման մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public double LoanFullNumber { get; set; }

        /// <summary>
        /// Ընթացիկ աճուրդի վերջ (սկիզբ)
        /// </summary>
        public DateTime? AuctionEndDateFrom { get; set; }

        /// <summary>
        /// Ընթացիկ աճուրդի վերջ (վերջ)
        /// </summary>
        public DateTime? AuctionEndDateTo { get; set; }

        /// <summary>
        /// Գրավի նկարագրություն
        /// </summary>
        public string EquipmentDescription { get; set; }

        /// <summary>
        /// Գրավի հասցե
        /// </summary>
        public string EquipmentAddress { get; set; }

        /// <summary>
        /// Վաճառքի գին (սկիզբ)
        /// </summary>
        public double? EquipmentSalePriceFrom { get; set; }

        /// <summary>
        /// Վաճառքի գին (վերջ)
        /// </summary>
        public double? EquipmentSalePriceTo { get; set; }
        /// <summary>
        /// Գրավի կարգավիճակ
        /// </summary>
        public int EquipmentQuality { get; set; }

        /// <summary>
        /// Վաճառքի փուլեր
        /// </summary>
        public int SaleStage {get; set; }

        public List<LoanEquipment> GetSearchedLoanEquipments()
        {
            return LoanEquipmentDB.GetSearchedLoanEquipments(this);
        }

        public LoanEquipment GetSumsOfEquipmentPrices()
        {
            return LoanEquipmentDB.GetSumsOfEquipmentPrices(this);
        }

    }
}
