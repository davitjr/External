using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;

namespace ExternalBanking
{
    public class InterestMarginOrder : Order
    {
        public InterestMargin InterestMargin { get; set; }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();
            return result;
        }

        public void SaveInterestMarginDetails()
        {
            InterestMarginDB.SaveInterestMarginDetails(this);
        }

        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
        }

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


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = InterestMarginDB.SaveInterestMarginOrder(this, userName, source);


                if (result.ResultCode == ResultCode.Normal)
                {
                    SaveInterestMarginDetails();
                }

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

            ActionResult resultConfirm = new ActionResult();
            resultConfirm.ResultCode = ResultCode.SavedNotConfirmed;


            InterestMarginOrder threadOrder = this;

            Thread thread = new Thread(() => ConfirmInterestMarginOrder(threadOrder, user));
            thread.Start();


            return resultConfirm;
        }

        public static void ConfirmInterestMarginOrder(InterestMarginOrder order, ACBAServiceReference.User user)
        {
            order.Confirm(user);
        }

        public void Get()
        {
            InterestMarginOrderDB.GetInterestMarginOrder(this);
        }


    }
}
