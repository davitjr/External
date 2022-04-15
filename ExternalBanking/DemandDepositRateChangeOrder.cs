using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;


namespace ExternalBanking
{
    /// <summary>
    /// Ցպահանջ ավանդի տոկոսադրույքի փոփոխման հայտ
    /// </summary>
    public class DemandDepositRateChangeOrder : Order
    {
        /// <summary>
        /// Ցպահանջ ավանդային հաշիվ
        /// </summary>
        public Account DemandDepositAccount { get; set; }

        /// <summary>
        /// Հրամանի համար
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Հրամանի ամսաթիվ
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// Սակագնի տեսակ
        /// </summary>
        public int TariffGroup { get; set; }

        /// <summary>
        /// Սակագնի տեսակի նկարագրություն
        /// </summary>
        public string TariffGroupDescription { get; set; }

        /// <summary>
        /// Կրեդիտագրվող հաշվեհամար ցպահանջ ավանդի համար
        /// </summary>
        public double PercentCreditAccount { get; set; }

        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.Rate = this.Rate / 100;
            this.DemandDepositAccount = Account.GetAccount(this.DemandDepositAccount.AccountNumber);


            if (this.TariffGroup == 3 || this.TariffGroup == 1)
            {
                KeyValuePair<string, DateTime>? document = new KeyValuePair<string, DateTime>();
                if (this.TariffGroup == 3)
                {
                    document = DemandDepositRate.GetDemandDepositRateTariffDocument(1);
                }
                else
                {
                    document = DemandDepositRate.GetDemandDepositRateTariffDocument(2);
                }
                this.DocumentDate = document.Value.Value;
                this.DocumentNumber = document.Value.Key;

            }

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateDemandDepositRateChangeOrder(this));
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
                result = DemandDepositRateChangeOrderDB.SaveDemandDepositRateChangeOrder(this, userName);

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

        public void Get()
        {
            DemandDepositRateChangeOrderDB.GetDemandDepositRateChangeOrder(this);
        }


    }
}
