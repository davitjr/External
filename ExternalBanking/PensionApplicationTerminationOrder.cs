using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Կենսաթոշակի դադարեցման տվյալներ
    /// </summary>
    public class PensionApplicationTerminationOrder : PensionApplicationOrder
    {
        /// <summary>
        /// Դադարեցման հիմք
        /// </summary>
        public short ClosingType { get; set; }
        /// <summary>
        /// Դադարեցման հիմքի նկարագրություն
        /// </summary>
        public string ClosingTypeDescription { get; set; }

        /// <summary>
        ///  Դադարեցման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Մահվան ա/թ
        /// </summary>
        public DateTime? DeathDate { get; set; }

        /// <summary>
        /// Մահվան վկայականի համար
        /// </summary>
        public string DeathCertificateNumber { get; set; }

        public ActionResult SaveAndApproveTerminationOrder(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                result = PensionApplicationOrderDB.SavePensionApplicationTerminationOrder(this, userName, source);

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

        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.SubType = 1;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidatePensionApplicationTerminationOrder(this));
            return result;
        }


        public void Get()
        {
            PensionApplicationOrderDB.GetPensionApplicationTerminationOrder(this);
        }
    }
}
