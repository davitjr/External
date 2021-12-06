using ExternalBanking.DBManager;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class DepositaryAccountOrder : Order
    {
        public DepositaryAccount AccountNumber { get; set; }
        
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            Complete();

            ActionResult result = Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = DepositaryAccountOrderDB.SaveDepositaryAccountOrder(this, userName);

                result = base.SaveOrderOPPerson();

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
                    Quality = OrderQuality.Sent3;
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
            //Հայտի համար   
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
            {
                OrderNumber = Order.GenerateNextOrderNumber(CustomerNumber);
            }

            OPPerson = Order.SetOrderOPPerson(CustomerNumber);
        }

        public ActionResult Validate()
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateDepositaryAccountOrder(this));

            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }

        public void GetDepositaryAccountOrder()
        {
            DepositaryAccountOrderDB.GetDepositaryAccountOrder(this);
        }

        public static bool ExistsNotConfirmedDepositaryAccountOrder(ulong customerNumber, byte subType)
        {
            return DepositaryAccountOrderDB.ExistsNotConfirmedDepositaryAccountOrder(customerNumber, subType);
        }

        public ActionResult Save(string userName, ACBAServiceReference.User user)
        {
            Complete();
            ActionResult result = Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                //Հայտի ձևակերպում
                result = DepositaryAccountOrderDB.SaveDepositaryAccountOrder(this, userName);

                ActionResult resultOpPerson = base.SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
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

                scope.Complete();
            }

            return result;
        }

        public ActionResult Approve(string userName, short schemaType)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                ActionResult result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
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

        public static ActionResult UpdateDepositoryAccountOrder(DepositaryAccountOrder order)
        {          
            return DepositaryAccountOrderDB.UpdateDepositoryAccountOrder(order);
        }
    }
}
