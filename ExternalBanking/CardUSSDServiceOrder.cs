using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի USSD ծառայության ակտիվացման/ապաակտիվացման հայտ
    /// </summary>
    public class CardUSSDServiceOrder : Order
    {
        /// <summary>
        /// Քարտի ունիկալ համար
        /// </summary>
        public ulong ProductID { get; set; }

        /// <summary>
        /// Բջջային հեռախոսահամար
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// 1-Գրանցել, 2- Հանել
        /// </summary>
        public short ActionType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CardNumber { get; set; }
        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardUSSDServiceOrder(this));
            return result;
        }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardUSSDServiceOrderDB.SaveUSSDServiceGenerationOrder(this, userName);

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

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    return result;
                }
            }

            ActionResult resultConfirm = base.Confirm(user);

            return resultConfirm;
        }

        public static string GetCardMobilePhone(ulong productID)
        {
            return CardUSSDServiceOrderDB.GetCardMobilePhone(productID);
        }

        internal static bool IsSecondUSSDServiceOrder(ulong productID)
        {
            bool secondService = CardUSSDServiceOrderDB.IsSecondUSSDServiceOrder(productID);
            return secondService;
        }

        public void Get()
        {
            CardUSSDServiceOrder order = CardUSSDServiceOrderDB.GetCardUSSDServiceOrder(this);
            order.CardNumber = Card.GetCardWithOutBallance(order.ProductID).CardNumber;
        }

    }
}
