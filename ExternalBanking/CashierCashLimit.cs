using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class CashierCashLimit
    {

        public int Id { get; set; }

        /// <summary>
        /// Թղթակից ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Մուտքագրման ա/թ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Փոփոխող ՊԿ
        /// </summary>
        public int ChangeBySetNumber { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FillialCode { get; set; }



        public static ActionResult SaveCashierCashLimits(List<CashierCashLimit> limits)
        {
            ActionResult result = new ActionResult();

            int setNumber = limits.Find(m => m.SetNumber != 0).SetNumber;

            List<CashierCashLimit> oldLimits = CashierCashLimit.GetCashierLimits(setNumber);

            limits.ForEach(m =>
            {
                if (!oldLimits.Exists(o=> o.Currency == m.Currency && o.Amount == m.Amount))
                {
                    CashierCashLimitDB.SaveCashierCashLimits(m);
                }
                
            });

            result.ResultCode = ResultCode.Normal;
            return result;
        }


        public static  ActionResult GenerateCashierCashDefaultLimits(int setNumber, int changeSetNumber)
        {
            return CashierCashLimitDB.GenerateCashierCashDefaultLimits(setNumber, changeSetNumber);
        }


        public static  List<CashierCashLimit> GetCashierLimits(int setNumber)
        {
            return CashierCashLimitDB.GetCashierLimits(setNumber);
        }


        public static int GetCashierFilialCode(int setNumber)
        {
            return CashierCashLimitDB.GetCashierFilialCode(setNumber);
        }
    }
}
