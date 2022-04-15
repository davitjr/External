using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Պահատուփի տուժանքի դադարեցման հայտ
    /// </summary>
    public class DepositCaseStoppingPenaltyCalculationOrder : Order
    {
        /// <summary>
        /// Փոփոխման հիմք
        /// </summary>
        public string ChangingReason { get; set; }

        /// <summary>
        /// Տույժի դադարեցման ամսաթիվ
        /// </summary>
        public DateTime DateOfStoppingPenaltyCalculation { get; set; }

        /// <summary>
        /// Փաստաթղթի ամսաթիվ
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }


        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateDepositCaseStoppingPenaltyCalculationOrder(this));
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
                result = DepositCaseStoppingPenaltyCalculationOrderDB.SaveDepositCaseStoppingPenaltyCalculationOrder(this, userName);

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


        public void Get()
        {
            DepositCaseStoppingPenaltyCalculationOrderDB.GetDepositCaseStoppingPenaltyCalculationOrder(this);
        }

        /// <summary>
        /// Ստուգում է որ դադարեցման չհաստատված հայտ չլինի
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static bool IsSecondPenaltyStoppingOrder(ulong customerNumber, ulong productId)
        {
            bool secondClosing = DepositCaseStoppingPenaltyCalculationOrderDB.IsSecondPenaltyStoppingOrder(customerNumber, productId);
            return secondClosing;
        }

    }
}
