using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Լիազորագրի ատիվացման հայտ
    /// </summary>
    public class CredentialActivationOrder : Order
    {
        /// <summary>
        /// Լիազորագիր
        /// </summary>
        public Credential Credential { get; set; }


        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշիվ
        /// </summary>
        public Account ReceiverAccount { get; set; }

        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            if (this.Credential.GivenByBank)
            {
                FeeForServiceProvidedOrder order = new FeeForServiceProvidedOrder();
                if (this.Type == OrderType.CredentialActivationOrder)
                {
                    order.Type = OrderType.FeeForServiceProvided;
                }
                else
                {
                    order.Type = OrderType.CashFeeForServiceProvided;
                }
                order.CustomerNumber = this.CustomerNumber;
                order.DebitAccount = this.DebitAccount;
                order.FilialCode = this.FilialCode;
                order.ServiceType = 215;
                order.OrderNumber = this.OrderNumber;
                order.user = this.user;
                order.Complete();
                this.DebitAccount = order.DebitAccount;
                this.ReceiverAccount = order.ReceiverAccount;
                this.Currency = this.DebitAccount.Currency;
                OrderFee fee = new OrderFee();
                fee.Currency = this.Currency;
                fee.Amount = this.Amount;
                fee.Account = this.DebitAccount;
                fee.CreditAccount = this.ReceiverAccount;
                if (this.Type == OrderType.CredentialActivationOrder)
                {
                    fee.Type = 27;
                }
                else
                {
                    fee.Type = 26;
                }
                this.Fees = new List<OrderFee>();
                this.Fees.Add(fee);
            }

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

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCredentialActivationOrder(this));
            return result;
        }

        public void Get()
        {
            CredentialOrderDB.GetCredentialActivationOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }

        public ActionResult SaveCredentialDetails(long orderId)
        {
            ActionResult result = new ActionResult();
            CredentialOrder credentialOrder = new CredentialOrder();
            credentialOrder.Credential = this.Credential;
            credentialOrder.CustomerNumber = this.CustomerNumber;
            result = CredentialOrderDB.SaveCredentialOrderDetails(credentialOrder, orderId);
            return result;
        }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (this.Type == OrderType.CredentialActivationOrder && this.Credential.GivenByBank)
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
                result = CredentialOrderDB.SaveCredentialActivationOrder(this, userName);

                result = SaveCredentialDetails(this.Id);

                result = this.SaveOrderFee();

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
                else
                {
                    return result;
                }
            }

            ActionResult resultConfirm = base.Confirm(user);

            return resultConfirm;
        }

    }
}
