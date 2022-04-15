using ExternalBanking.DBManager;
using System.Transactions;


namespace ExternalBanking
{
    public class SwiftMessageRejectOrder : Order
    {
        public int SwiftMessageId { get; set; }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();

            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = SwiftMessageDB.SaveSwiftMessageRejectOrder(this, userName, source, user.filialCode);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }


                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
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





        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            if (!SwiftMessage.CheckSentSwiftStatus(this.SwiftMessageId))
            {
                result.Errors.Add(new ActionError(1442));
            }

            if (Validation.IsSwiftMessageConfirmed(this.SwiftMessageId))
            {
                result.Errors.Add(new ActionError(1450));
            }

            return result;
        }


        protected void Complete()
        {
            this.SubType = 1;
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);


        }

    }
}
