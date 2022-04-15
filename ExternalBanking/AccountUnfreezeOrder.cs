using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի ապասառեցման հայտ
    /// </summary>
    public class AccountUnfreezeOrder : Order
    {
        /// <summary>
        /// Սառեցման ունիկալ համար
        /// </summary>
        public long FreezeId { get; set; }

        /// <summary>
        /// Ապասառեցման ենթակա հաշվեհամար
        /// </summary>
        public Account FreezedAccount { get; set; }

        /// <summary>
        /// Ապասառեցման պատճառ
        /// </summary>
        public ushort UnfreezeReason { get; set; }

        /// <summary>
        /// Ապասառեցման պատճառի նկարագրություն
        /// </summary>
        public string UnfreezeReasonDescription { get; set; }

        /// <summary>
        /// Ապասառեցման նկարագրություն
        /// </summary>
        public string UnfreezeReasonAddInf { get; set; }


        /// <summary>
        /// Լրացնում է հաշվի ապասառեցման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.SubType = 1;
            this.RegistrationDate = DateTime.Now.Date;

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Հաշվի ապասառեցման հայտի պահպանում
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
                result = AccountUnfreezeOrderDB.Save(this, userName, source, filialCode);

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
        /// Հաշվի ապասառեցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAccountUnfreezeOrder(this));
            return result;
        }

        /// <summary>
        /// Հաշվի ապասառեցման կրկնակի հայտի առկայություն
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondUnfreeze(long freezeId, string accountNumber)
        {
            bool secondClosing = AccountUnfreezeOrderDB.IsSecondUnfreeze(freezeId, accountNumber);
            return secondClosing;
        }


        /// <summary>
        /// Հաշվի ապասառեցման հնարավորության ստուգում սառեցվածության տեսակների համար
        /// </summary>
        /// <param name="freezeId"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsInaccessibleUnfreeze(long freezeId, string accountNumber, bool isOnlineAcc, bool isBranchDiv)
        {
            bool inaccessibleUnfreeze = AccountUnfreezeOrderDB.IsInaccessibleUnfreeze(freezeId, accountNumber, isOnlineAcc, isBranchDiv);
            return inaccessibleUnfreeze;
        }

        /// <summary>
        /// Հաշվի ապասառեցման հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            AccountUnfreezeOrderDB.Get(this);
        }

    }
}
