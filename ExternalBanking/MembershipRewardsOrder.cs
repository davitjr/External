using ExternalBanking.DBManager;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class MembershipRewardsOrder : Order
    {
        /// <summary>
        /// Պրոդուկտի համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();

            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Action.Add;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = MembershipRewardsOrderDB.SaveCardMembershipRewardsOrder(this, userName);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
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

        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            if (this.Type == OrderType.CardMRReNewOrder || this.Type == OrderType.CardMRCancelOrder)
            {
                result.Errors.AddRange(Validation.ValidateMembershipRewardsOrder(this));
            }
            return result;
        }

        public void Get()
        {
            MembershipRewardsOrderDB.GetCardMembershipRewardsOrder(this);
        }
    }
}
