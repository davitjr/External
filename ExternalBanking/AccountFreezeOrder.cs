using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի սառեցման հայտ
    /// </summary>
    public class AccountFreezeOrder : Order
    {
        /// <summary>
        /// Սառեցման ենթակա հաշվեհամար
        /// </summary>
        public Account FreezeAccount { get; set; }

        /// <summary>
        /// Սառեցման պատճառ
        /// </summary>
        public ushort FreezeReason { get; set; }

        /// <summary>
        /// Սառեցման պատճառի նկարագրություն
        /// </summary>
        public string FreezeReasonDescription { get; set; }

        /// <summary>
        /// Սառեցման  նկարագրություն
        /// </summary>
        public string FreezeReasonAddInf { get; set; }

        /// <summary>
        /// Հաշվի սառեցման ամսաթիվ
        /// </summary>
        public DateTime? FreezeDate { get; set; }

        /// <summary>
        /// Գումարի սառեցման ամսաթիվ
        /// </summary>
        public DateTime? AmountFreezeDate { get; set; }

        /// <summary>
        /// Սառեցված գումար
        /// </summary>
        public double FreezeAmount { get; set; }


        /// <summary>
        /// Լրացնում է հաշվի սառեցման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.SubType = 1;
            this.RegistrationDate = DateTime.Now.Date;
            this.FreezeAccount = Account.GetAccount(this.FreezeAccount.AccountNumber);

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Հաշվի սառեցման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            int filialCode = user.filialCode;
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountFreezeOrderDB.Save(this, userName, source, filialCode);

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

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

            }
            result = base.Confirm(user);

            return result;
        }


        /// <summary>
        /// Հաշվի սառեցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAccountFreezeOrder(this));
            return result;
        }


        /// <summary>
        /// Հաշվի սառեցման հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            AccountFreezeOrderDB.Get(this);
        }

        /// <summary>
        /// Ստուգում է՝ արդյոք տվյալ հաշվեհամարը հնարավոր է սառեցնել, թե ոչ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool ValidateFreezing(string accountNumber)
        {
            bool isValid = AccountFreezeOrderDB.ValidateFreezing(accountNumber);
            return isValid;
        }
    }
}
