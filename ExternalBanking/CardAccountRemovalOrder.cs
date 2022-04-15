using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտային հաշվի հեռացման հայտ, հեռացնում ենք միայն քարտին կցված հաշիվները
    /// </summary>
    public class CardAccountRemovalOrder : Order
    {
        /// <summary>
        /// Հեռացվող քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// Քարտի հեռացման պատճառ
        /// </summary>
        public int RemovalReason { get; set; }
        /// <summary>
        /// Քարտի հեռացման պատճառի նկարագրություն
        /// </summary>
        public string RemovalReasonDescription { get; set; }
        /// <summary>
        /// Քարտի հեռացման "այլ" տարբերակի նկարագրություն
        /// </summary>
        public string RemovalReasonInfo { get; set; }

        public void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.CardAccountRemovalOrder;
            this.SubType = 1;

            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source == SourceType.Bank)
            {
                this.UserId = user.userID;
            }
        }

        public ActionResult SaveAndApprove(ACBAServiceReference.User user, SourceType source, ulong customerNumber, short schemaType)
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
                result = CardAccountRemovalOrderDB.Save(this, user, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                LogOrderChange(user, action);

                ActionResult res = base.Approve(schemaType, user.userName);

                if (res.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    this.SubType = 1;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);

                }
                else
                {
                    return res;
                }
                scope.Complete();
            }

            result = base.Confirm(user);

            return result;
        }
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            result.Errors.AddRange(Validation.ValidateCardAccountRemovalOrder(this));
            return result;
        }

        public CardAccountRemovalOrder Get()
        {
            return CardAccountRemovalOrderDB.GetCardAccountRemovalOrder(this);
        }

    }
}
