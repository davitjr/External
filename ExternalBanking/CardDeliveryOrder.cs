using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;

namespace ExternalBanking
{
    public class CardDeliveryOrder
    {

        /// <summary>
        /// Ներբեռնում կատարող օգտագործող
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Սկզբնական ա/թ
        /// </summary>
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Վերջնական ա/թ
        /// </summary>
        public DateTime DateFrom { get; set; }


        /// <summary>
        ///  ì»ñ³¹³ñÓÝáõÙ ¿ Ýßí³Í Å³Ù³Ý³Ï³Ñ³ïí³ÍÇ Ñ³Ù³ñ XML-Ý»ñÁ
        /// </summary>
        /// <returns></returns>
        public static ActionResult DownloadOrderXMLs(DateTime DateFrom, DateTime DateTo, User user)
        {
            ActionResult result = new ActionResult();
            //List<DateTime> Operdays = new List<DateTime>();
            try
            {
                if (user.DepartmentId != 148)
                {
                    //Տվյալ գործողությունը հասանելի չէ:
                    result.Errors.Add(new ActionError(639));
                    result.ResultCode = ResultCode.ValidationError;
                    Localization.SetCulture(result, new Culture(Languages.hy));
                    return result;
                }
                ///Operdays = CardDeliveryOrderDB.GetOperDaysByPeriod(DateTo, DateFrom);                       
                result = CardDeliveryOrderDB.DownloadOrderXMLs(DateFrom, DateTo);
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                ActionError actionError = new ActionError();
                actionError.Code = 0;
                actionError.Description = ex.Message;
                result.Errors.Add(actionError);
            }
            return result;
        }



    }
}
