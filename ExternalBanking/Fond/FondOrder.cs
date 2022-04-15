using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;

namespace ExternalBanking
{
    public class FondOrder : Order
    {
        public Fond Fond { get; set; }

        //public List<FondProvidingDetail> FondProvidingDetails { get; set; }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();
            //result.Errors.AddRange(Validation.ValidateDepositCaseOrder(this));
            return result;
        }

        public void SaveFondProvidingDetails()
        {
            FondDB.SaveFondProvidingDetails(this);

        }

        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            if (this.Fond.ID == 0)
                this.Fond.ID = FondOrderDB.GenerateNextFondID();
            //generacnel fondi hertakan hamar
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
                result = FondDB.SaveFondOrder(this, userName, source);


                if (result.ResultCode == ResultCode.Normal)
                {
                    SaveFondProvidingDetails();
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


            FondOrder threadOrder = this;

            Thread thread = new Thread(() => ConfirmFondOrder(threadOrder, user));
            thread.Start();


            return resultConfirm;
        }

        public static void ConfirmFondOrder(FondOrder order, ACBAServiceReference.User user)
        {
            order.Confirm(user);
        }


        public void Get()
        {
            FondOrderDB.GetFondOrder(this);
        }
    }
}
