using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{

    public enum SupplementaryType : uint
    {
        /// <summary>
        /// Անորոշ
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// Հիմնական քարտ
        /// </summary>
        Main = 1,

        /// <summary>
        /// Կից քարտ
        /// </summary>
        Linked = 2,

        /// <summary>
        /// Լրացուցիչ քարտ
        /// </summary>
        Attached = 3

    }

    public enum CardChangeType : short
    {
        /// <summary>
        /// Նոր քարտ
        /// </summary>
        New = 0,

        /// <summary>
        /// Վերաթողարկում
        /// </summary>
        ReNew = 1,

        /// <summary>
        /// Տեղափոխում
        /// </summary>
        RePlace = 2,


        /// <summary>
        /// Փոխարինում
        /// </summary>
        Change = 3,
    }

    public class PlasticCard
    {
        ///// <summary>
        ///// Քարտային հայտի համար
        ///// </summary>
        //public int OrderId { get; set; }

        ///// <summary>
        ///// Քարտապանի ունիկալ համար
        ///// </summary>
        //public uint IdentityId { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Ցույց է տալիս քարտը կից է թե ոչ
        /// </summary>
        public SupplementaryType SupplementaryType { get; set; }

        ///// <summary>
        ///// Գաղտնաբառ
        ///// </summary>
        //public string MotherName { get; set; }

        /// <summary>
        /// Քարտի գործ. ամսաթիվ
        /// </summary>
        public string ExpiryDate { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        /// <summary>
        /// Աշխատավարձային ծրագրի համար
        /// </summary>
        public int RelatedOfficeNumber { get; set; }

        /// <summary>
        /// Քարտի տեսակ
        /// </summary>
        public uint CardType { get; set; }

        /// <summary>
        /// Քարտի կարգավիճակ
        /// </summary>
        public uint Quality { get; set; }

        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        // <summary>
        // ԱրՔա-ում հաճախորդի համար
        // </summary>
        //public string ArcaCustomerId { get; set; }

        // <summary>
        // ԱրՔա-ում անձի համար
        // </summary>
        //public string ArcaPersonId { get; set; }

        ///// <summary>
        ///// ԱրՔա-ում անձի հասցեի համար
        ///// </summary>
        //public string ArcaAddressId { get; set; }

        ///// <summary>
        ///// ԱրՔա-ում պայմանգրի համար
        ///// </summary>
        //public string ArcaContractId { get; set; }

        ///// <summary>
        ///// ԱրՔա-ում հաշվեհամար
        ///// </summary>
        //public string ArcaAccountNumber { get; set; }

        ///// <summary>
        ///// ?????????????
        ///// </summary>
        //public string EmbossingName { get; set; }


        ///// <summary>
        ///// Քարտի սկզբնաժամկետ
        ///// </summary>
        //public DateTime ValidFrom { get; set; }


        /// <summary>
        /// Քարտի տեսակի նկարագրություն
        /// </summary>
        public string CardTypeDescription { get; set; }

        /// <summary>
        /// Քարտային համակարգ
        /// </summary>
        public int CardSystem { get; set; }

        /// <summary>
        /// Քարտապանի անուն
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// Քարտապանի ազգանուն
        /// </summary>
        public string CardHolderLastName { get; set; }

        public CardChangeType CardChangeType { get; set; }

        public string AddInf { get; set; }


        //public bool IsMotherNameGenerated { get; set; }

        /// <summary>
        /// Հիմնակակն քարտի համար
        /// </summary>
        public string MainCardNumber { get; set; }

        public string CVV { get; set; }

        /// <summary>
        /// Քարտի գրանցման օր
        /// </summary>
        public DateTime OpenDate { get; set; }

        public int MainCardSystem { get; set; }
        /// <summary>
        /// Վերադարձնում է մուտքագրման ենթակա քարտերը
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="filialCode">Աշխատակցի մասնաճյուղ</param>
        /// <returns></returns>
        public static List<PlasticCard> GetCardsForRegistration(ulong customerNumber, int filialCode)
        {
            return PlasticCardDB.GetCardsForRegistration(customerNumber, filialCode);
        }

        /// <summary>
        /// վերադարձնում է պլաստիկ քարտը, որը դեռ գործող չէ, հաշիվներ կցված չեն
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productidType">եթե true է , ապա productId - ն հասկանում է որպես նոր քարտի app_id, հակառակ դեպքում ՝ հին քարտի</param>
        /// <returns></returns>
        public static PlasticCard GetPlasticCard(ulong productId,bool productidType)
        {
            return PlasticCardDB.GetPlasticCard(productId,productidType);
        }

        public static List<PlasticCard> GetCustomerMainCards(ulong customerNumber)
        {
            return PlasticCardDB.GetCustomerMainCards(customerNumber);
        }

        public static List<PlasticCard> GetCustomerPlasticCards(ulong customerNumber)
        {
            return PlasticCardDB.GetCustomerMainCards(customerNumber, true);
        }


        public static ActionResult UpdateCardStatusWithoutOrder(ulong productId)
        {
            ActionResult actionResult = new ActionResult() { ResultCode = ResultCode.Normal };         
            try
            {
                if( PlasticCardDB.UpdateCardStatusWithoutOrder(productId) != 1)                          
                   actionResult.ResultCode = ResultCode.Failed;                
            }
            catch(Exception ex)
            {
                actionResult.ResultCode = ResultCode.Failed;                
            }
            return actionResult;
        }

    public static List<PlasticCard> GetCustomerPlasticCardsForAdditionalData(ulong customerNumber, bool IsClosed)
        {
            return PlasticCardDB.GetCustomerPlasticCardsForAdditionalData(customerNumber, IsClosed);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հիմնական քարտերը, լրացուցիչ քարտի հայտի համար 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<PlasticCard> GetCustomerMainCardsForAttachedCardOrder(ulong customerNumber)
        {
            return PlasticCardDB.GetCustomerMainCardsForAttachedCardOrder(customerNumber);
        }
    }
}
