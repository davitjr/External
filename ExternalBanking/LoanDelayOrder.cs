using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Վարկի հետաձգման  հայտ
    /// </summary>
    public class LoanDelayOrder : Order
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductAppId { get; set; }

        /// <summary>
        /// Վարկի մարման վերսկսման ամսաթիվ
        /// </summary>
        public DateTime DelayDate { get; set; }

        /// <summary>
        /// Փոփոխման հիմք
        /// </summary>
        public string DelayReason { get; set; }



        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            //result.Errors.AddRange(Validation.ValidateLoanDelayOrder(this));
            return result;
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
                result = LoanDelayOrderDB.SaveLoanDelayOrder(this, userName, source);

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

        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        public void Get()
        {
            LoanDelayOrderDB.GetLoanDelayOrderData(this);
        }
    }
}
