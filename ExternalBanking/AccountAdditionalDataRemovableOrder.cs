using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի լրացուցիչ տվյալների հեռացման հայտ
    /// </summary>
    public class AccountAdditionalDataRemovableOrder : AccountDataChangeOrder
    {

        /// <summary>
        /// Հաշվի տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <returns></returns>
        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                result = AccountDataChangeOrderDB.Save(this, userName, source, user.filialCode);
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.SaveOrderOPPerson();

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

            }
            result = base.Confirm(user);

            return result;
        }
    }
}
