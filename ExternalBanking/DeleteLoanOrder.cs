using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    public class DeleteLoanOrder : Order
    {
        /// <summary>
        /// Վարկի ունիկալ համար
        /// </summary>
        public ulong AppId { get; set; }

        /// <summary>
        /// Վարկի հեռացման պատճառ
        /// </summary>
        public byte DeleteReasonType { get; set; }

        /// <summary>
        /// Վարկի հեռացման պատճառի նկարագրություն
        /// </summary>
        public string DeleteReasonDescription { get; set; }

        /// <summary>
        /// Պատ կատարող
        /// </summary>
        public uint ConfirmationSetNumber { get; set; }


        public ActionResult SaveAndApprove(string userName, short schemaType, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = DeleteLoanOrderDB.LoanDeleteOrder(this, user.userID);

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.Approve(schemaType, user.userName);

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

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            this.Type = OrderType.DeleteLoanOrder;

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
            {
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            }
        }

        /// <summary>
        /// Վարկի հեռացման ստուգումներ
        /// </summary>
        /// <returns></returns>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            if (this.DeleteReasonType == 0)
                result.Errors.Add(new ActionError(1965));
            return result;
        }


    }
}
