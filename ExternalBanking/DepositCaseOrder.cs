using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Պահատուփի ստացման հայտ
    /// </summary>
    public class DepositCaseOrder : Order
    {
        /// <summary>
        /// Պահատուփ
        /// </summary>
        public DepositCase DepositCase { get; set; }


        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);


            if (this.Type == OrderType.DepositCaseActivationOrder)
            {
                this.Fees.ForEach(m =>
                {
                    if (m.Type == 23)
                    {
                        m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", this.FilialCode);
                        m.OrderNumber = this.OrderNumber;
                    }

                });
            }
            this.DepositCase.CaseNumber = Convert.ToInt32(this.DepositCase.CaseNumber).ToString("000");

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateDepositCaseOrder(this));
            return result;
        }
        /// <summary>
        /// Վերադարձնում է վերջի ազատ պայմանգրի համարը
        /// </summary>
        /// <returns></returns>
        public static ulong GetContractNumber()
        {
            return DepositCaseOrderDB.GetContractNumber();
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

            if (this.Type == OrderType.DepositCaseActivationOrder)
            {
                result = this.ValidateForSend();
                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = DepositCaseOrderDB.SaveDepositCaseOrder(this, userName, source);

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
            DepositCaseOrderDB.GetDepositCaseOrder(this);
        }
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }
    }
}
