using ExternalBanking.DBManager;
using System;
using System.Data;
using System.Transactions;

namespace ExternalBanking
{
    public class CardReOpenOrder : Order
    {
        /// <summary>
        /// Վերաբացման պատճառի նկարագրություն
        /// </summary>

        public string ReopenDescription { get; set; }
        /// <summary>
        /// Վերաբացման պատճառ
        /// </summary>
        public ushort ReopenReason { get; set; }
        /// <summary>
        /// Վերաբացման պատճառ
        /// </summary>
        public string ReopenReasonString { get; set; }

        /// <summary>
        /// Վերաբացվող քարտ
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Վերաբացվող քարտի հիմնական քարտ
        /// </summary>
        public string MainCardNumber { get; set; }
        /// <summary>
        /// Վերաբացվող քարտի տեսակ
        /// </summary>
        public byte CardType { get; set; }
        /// <summary>
        /// Վերաբացվող քարտի տեսակ նկարագրությունով
        /// </summary>
        public string CardTypeDescription { get; set; }
        /// <summary>
        /// Վերաբացվող քարտի App_Id
        /// </summary>
        public ulong ProductID { get; set; }
        /// <summary>
        /// Հայտ մուտքագրողի ՊԿ
        /// </summary>
        public ushort SetNumber { get; set; }
        /// <summary>
        /// Հայտ անուն
        /// </summary>
        public string OrderName { get; set; }
        /// <summary>
        /// քարտի մասնաճյուղ
        /// </summary>
        public int Filial { get; set; }




        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Քարտի գրանցման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = new ActionResult();
            ActionResult resultConfirm = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardReOpenOrder(this, user));

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = CardReOpenOrderDB.SaveCardReOpenOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                result = base.SaveOrderOPPerson();
                result = base.SaveOrderFee();
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                // if (source == SourceType.Bank)
                this.Quality = OrderQuality.Draft;

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    // if (source == SourceType.Bank)
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    return result;
                }
            }

            resultConfirm = base.Confirm(user);

            return resultConfirm;


        }



        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardReOpenOrder(this, user));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է քարտի մուտքագրման հայտի տվյալները
        /// </summary>
        /// <returns></returns>
        public void Get()
        {
            CardReOpenOrderDB.GetCardReOpenOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }

        /// <summary>
        /// Վերադարցնում է վերաթողարկման պատճառների ցուցակը
        /// </summary>
        /// <returns></returns>

        public static DataTable GetCardReOpenReason()
        {
            string cacheKey = "CardReOpenOrder_GetCardReOpenReason";
            DataTable dt = CacheHelper.Get(cacheKey);
            if (dt == null)
            {
                dt = CardReOpenOrderDB.GetCardReOpenReason();
                CacheHelper.Add(dt, cacheKey);
            }
            return dt;
        }


        public static bool IsCardOpen(string CardNumber)
        {
            return CardReOpenOrderDB.IsCardOpen(CardNumber);
        }
    }
}
