using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    public class LoanEquipment
    {
        /// <summary>
        /// Վարկի ունիկալ համար
        /// </summary>
        public double AppID { get; set; }

        /// <summary>
        /// Խնդրահարույց վարկի գրավի ունիկալ համար
        /// </summary>
        public int EquipmentID { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public double CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի անուն ազգանուն/անվանում
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Վարկը տրամադրող մասնաճյուղի կոդ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Գրավի նկարագրություն
        /// </summary>
        public string EquipmentDescription { get; set; }

        /// <summary>
        /// Գրավի հասցե
        /// </summary>
        public string EquipmentAddress { get; set; }

        /// <summary>
        /// Շուկայական արժեք
        /// </summary>
        public double MarketPrice { get; set; }

        /// <summary>
        /// Աճուրդի կազմակաերպիչ
        /// </summary>
        public string AuctionOrganizer { get; set; }

        /// <summary>
        /// Առաջին աճուրդի մեկնարկային գին
        /// </summary>
        public double FirstAuctionInitialPrice { get; set; }

        /// <summary>
        /// Ընթացիկ աճուրդի վերջ
        /// </summary>
        public DateTime? AuctionEndDate { get; set; }

        /// <summary>
        /// Ընթացիկ աճուրդի մեկնարկային գին
        /// </summary>
        public double EquipmentPrice { get; set; }

        /// <summary>
        /// Բանկի կողմից գնման գնի սահմանաչափ
        /// </summary>
        public double PriceLimitByTheBank { get; set; }

        /// <summary>
        /// Աճուրդին մասնակցության զեկուզագիր փուլի այլ տեղեկատվություն
        /// </summary>
        public string EventAddInf { get; set; }

        /// <summary>
        /// Գույքի վաճառքի գին
        /// </summary>
        public double EquipmentSalePrice { get; set; }

        /// <summary>
        /// Վաճառված գույքից բանկ փոխանցված գումար
        /// </summary>
        public double BankTransferedMoney { get; set; }

        /// <summary>
        /// Գնորդի անուն ազգանուն/անվանում
        /// </summary>
        public string BuyersFullName { get; set; }

        /// <summary>
        /// Աճուրդում վաճառքի գործընթաց փուլի այլ տեղեկատվություն
        /// </summary>
        public string AddInf { get; set; }

        /// <summary>
        /// Առքուվաճառքի պայամանագրի ամսաթիվ
        /// </summary>
        public DateTime? DateOfContractSale { get; set; }

        /// <summary>
        /// Փորձագետի կողմից գնահատված արժեք
        /// </summary>
        public double ExpertAppraisedValue { get; set; }

        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public double LoanFullNumber { get; set; }

        /// <summary>
        /// Գրավի փակման օր
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        ///Վարկի մարման սահմանափակում
        /// </summary>
        public int AllowMature { get; set; }

        /// <summary>
        /// Վարկի տրման ամսաթիվ
        /// </summary>
        public DateTime? DateOfBeginning { get; set; }

        /// <summary>
        ///Բանկ փոխանցված գումար
        /// </summary>
        public double BankTransferredMoney { get; set; }

        /// <summary>
        /// Գրավի տեսակ
        /// </summary>
        public string EquipmentType { get; set; }

        /// <summary>
        /// Գրավի սեփականատեր(եր)
        /// </summary>
        public string EquipmentOwner { get; set; }

        /// <summary>
        ///Գնահատված արժեք
        /// </summary>
        public double ProvisionAmount { get; set; }

        /// <summary>
        ///Վերագնահատված արժեք
        /// </summary>
        public double HistoryAmount { get; set; }

        /// <summary>
        ///Լիկվիդային արժեք
        /// </summary>
        public double SalePrice { get; set; }

        /// <summary>
        /// Գնահատման ամսաթիվ (Վերջնական)
        /// </summary>
        public DateTime? LastAppraisingDate { get; set; }

        /// <summary>
        /// Գնահատող ընկերություն
        /// </summary>
        public string AppraiserOrganisation { get; set; }

        /// <summary>
        /// Առաջին աճուրդի սկիզբ
        /// </summary>
        public DateTime? FirstAuctionStartDate { get; set; }

        /// <summary>
        /// Ընթացիկ աճուրդի սկիզբ
        /// </summary>
        public DateTime? AuctionStartDate { get; set; }

        /// <summary>
        /// Վաճառված գույքից Բանկ փոխանցված գումարի ամսաթիվ
        /// </summary>
        public DateTime? DateOfBankTransferredMoney { get; set; }

        /// <summary>
        /// Ձեռք է բերվել բանկի կողմից
        /// </summary>
        public string AcquiredByTheBank { get; set; }

        /// <summary>
        /// Գրավը փակող ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }

        /// <summary>
        /// Գրավը փակման պատճառ
        /// </summary>
        public string ClosingReason { get; set; }

        /// <summary>
        /// Ընթացիկ աճուրդի մեկնարկային գների գումար
        /// </summary>
        public double SumOfEquipmentPrice { get; set; }

        /// <summary>
        /// Բանկի կողմից գնման գնի սահմանաչափերի գումար
        /// </summary>
        public double SumOfPriceLimitByTheBank { get; set; }

        /// <summary>
        /// Վաճառքի գների գումար
        /// </summary>
        public double SumOfEquipmentSalePrice { get; set; }

        /// <summary>
        ///Գումարային բանկ փոխանցված գումար
        /// </summary>
        public double SumOfBankTransferredMoney { get; set; }

        /// <summary>
        /// Քանակ
        /// </summary>
        public int ListCount { get; set; }


        public static LoanEquipment GetLoanEquipmentDetails(int equipmentID)
        {
            LoanEquipment loanEquipment = LoanEquipmentDB.GetLoanEquipmentDetails(equipmentID);
            return loanEquipment;
        }
        public static string GetEquipmentClosingReason(int equipmentID)
        {
            string closingReason = LoanEquipmentDB.GetEquipmentClosingReason(equipmentID);
            return closingReason;
        }

        public static ActionResult LoanEquipmentClosing(int equipmentID, int setNumber, string closingReason)
        {
            ActionResult result = LoanEquipmentDB.LoanEquipmentClosing(equipmentID, setNumber, closingReason);
            return result;
        }

        public static ActionResult ChangeCreditProductMatureRestriction(double appID, int setNumber, int allowMature)
        {
            ActionResult result = LoanEquipmentDB.ChangeCreditProductMatureRestriction(appID, setNumber, allowMature);
            return result;
        }


        
    }
}
