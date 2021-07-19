using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class DepositProductPrices
    {   /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Պրոդուկտի ունիկալ կոդ
        /// </summary>
        public byte ProductCode { get; set; }
        /// <summary>
        /// Դեպոզիտի արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public Double InterestRate { get; set; }
        /// <summary>
        /// Գործող ամսաթիվ
        /// </summary>
        public DateTime DateOfBeginning { get; set; }
        /// <summary>
        /// Մինիմալ տևողությունը՝ ամիսներով
        /// </summary>
        public int PeriodInMonthsMin { get; set; }
        /// <summary>
        /// Մաքսիմալ տևողությունը՝ ամիսներով
        /// </summary>
        public int PeriodInMonthsMax { get; set; }
        /// <summary>
        /// Գումարի մինիմալ չափ
        /// </summary>
        public Double MinAmount { get; set; }
        /// <summary>
        ///Բոնուսային տոկոսադրույք ֆիզ․ անձանց համար
        /// </summary>
        public Double BonusInterestRate { get; set; }
        /// <summary>
        ///Բոնուսային տոկոսադրույք ACBA Online անձանց համար
        /// </summary>
        public Double BonusInterestRateForHB { get; set; }
        /// <summary>
        ///Բոնուսային տոկոսադրույք աշխատակիցների համար
        /// </summary>
        public Double BonusInterestRateForEmployee { get; set; }
        /// <summary>
        ///Բոնուսային տոկոսադրույք իրավ․ անձանց համար
        /// </summary>
        public Double MinAmountJur { get; set; }
        /// <summary>
        ///Հաճախորդի տեսակը
        /// </summary>
        public short TypeOfClient { get; set; }
        /// <summary>
        //Վճարման տեսակից կախված բոնուսային տոկոսադրույք
        /// </summary>
        public Double BonusInterestRateForRepaymentType { get; set; }
        /// <summary>
        //Գումարի ավելացման դեպքում տոկոսադրույք
        /// </summary>
        public Double InterestRateForAllowAddition { get; set; }
        /// <summary>
        //Գումարի նվազեցման դեպքում տոկոսադրույք
        /// </summary>
        public Double InterestRateForAllowDecreasing { get; set; }
        /// <summary>
        //Գումարի ավելացման և նվազեցման դեպքում տոկոսադրույք
        /// </summary>
        public Double InterestRateForAllowAdditionAndDecreasing { get; set; }
        /// <summary>
        ///Կլասսիկ տիպի համար  բոնուսային տոկոսադրույք
        /// </summary>
        public Double BonusInterestRateForClassic { get; set; }
        /// <summary>
        ///Ավանգարդ տիպի համար  բոնուսային տոկոսադրույք
        /// </summary>
        public Double BonusInterestRateForAvangard { get; set; }
        /// <summary>
        ///Պրեմիում տիպի համար  բոնուսային տոկոսադրույք
        /// </summary>
        public Double BonusInterestRateForPremium { get; set; }
        /// <summary>
        ///Ավելացման դեպքում մաքիսմալ տոկոս
        /// </summary>  
        public Double MaxAdditionPercent { get; set; }
        /// <summary>
        ///Նվազեցման դեպքում մաքիսմալ տոկոս
        /// </summary>              
        public Double MaxDecreasingPercent { get; set; }
        /// <summary>
        ///Գրանցման համարը
        /// </summary>  
        public int RegistrationSetNumber { get; set; }
        /// <summary>
        ///Գրանցման ամսաթիվը
        /// </summary>  
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        ///Հաստատման համարը
        /// </summary>  
        public int ConfirmationSetNumber { get; set; }
        /// <summary>
        ///Հաստատման ամսաթիվը
        /// </summary>  
        public DateTime ConfirmationDate { get; set; }
        /// <summary>
        ///Կարգավիճակ
        /// </summary>  
        public byte Status { get; set; }
        /// <summary>
        ///Մերժման նկարագրությունը
        /// </summary>  
        public string RejectionDescription { get; set; }

        /// <summary>
        /// Վերադարձնում է մերժված կամ հաստատված դեպոզիտային պրոդուկտի արժեքը
        /// </summary>
        /// <returns></returns>
        public static ActionResult ConfirmOrRejectDepositProductPrices(string listOfId, int confirmationSetNumber, byte status, string rejectionDescription)
        {
            return DepositProductPricesDB.ConfirmOrRejectDepositProductPrices(listOfId, confirmationSetNumber, status, rejectionDescription);
        }
        /// <summary>
        ///Վերադարձնում է ջնջված դեպոզիտային պրոդուկտի արժեքը
        /// </summary>
        /// <returns></returns>
        public static ActionResult DeleteDepositProductPrices(int id, int registrationSetNumber)
        {
            return DepositProductPricesDB.DeleteDepositProductPrices(id,registrationSetNumber);
        }
        /// <summary>
        ///Վերադարձնում է ավելացրած դեպոզիտային պրոդուկտի արժեքը
        /// </summary>
        /// <returns></returns>
        public static ActionResult AddDepositProductPrices(DepositProductPrices product)
        {
            return DepositProductPricesDB.AddDepositProductPrices(product);
        }
        /// <summary>
        ///Վերադարձնում է դեպոզիտային պրոդուկտների արժեքները
        /// </summary>
        /// <returns></returns>
        public static List<DepositProductPrices> GetDepositProductPrices(SearchDepositProductPrices searchProduct)
        {
            return DepositProductPricesDB.GetDepositProductPrices(searchProduct);
        }
        /// <summary>
        ///Թարմացնում է դեպոզիտների արժեքները
        /// </summary>
        /// <returns></returns>
        public static ActionResult UpdateDepositPrices(DepositProductPrices product)
        {
            return DepositProductPricesDB.UpdateDepositPrices(product);

        }
    }
}

