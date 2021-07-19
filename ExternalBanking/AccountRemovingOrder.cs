using System;
using System.Transactions;
using ExternalBanking.DBManager;
namespace ExternalBanking
{
    public class AccountRemovingOrder:Order
    {
        ///<summary>
        ///Հեռացման ենթակա հաշիվ
        /// </summary>
       public  Account RemovingAccount { get; set; }

        ///<summary>
        /// Հայտի ավտոմատ լրացվող դաշտերի ստացում 
        ///</summary>
        private void Complete (SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.AccountRemove ;
            //Հայտի համարի ստացում 
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        ///<summary>
        ///Հաշվի հեռացման հայտի պահպանման ստուգումներ
        ///<summary>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAccountRemovingOrder(this));
            return result;
        }

        /// <summary>
        /// Հաշվի փակման հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        private ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate != DateTime.Now.Date )
            {
                //Հաշվի բացման ամսաթիվը տարբերվում է այսօրվա ամսաթվից
                result.Errors.Add(new ActionError(451));
            }
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
        internal static bool IsSecondClosing(ulong customerNumber, string accountNumber, SourceType sourceType)
        {
            bool secondClosing = AccountRemovingOrderDB.IsSecondClosing(customerNumber, accountNumber);
            return secondClosing;
        }
        /// Հաշվի հեռացման հայտ
        ///</summary>
        ///<param name = "userName"></param>
        ///<param name="source"></param>
        ///<param name="user"></param>
        ///<returns></returns>
        public ActionResult RemoveAccountOrder(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete(source);
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            Action action = this.Id == 0 ? Action.Add : Action.Delete;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountRemovingOrderDB .RemoveAccountOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }

        /// <summary>
        ///Հաշվի փակման հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = this.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Delete;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountRemovingOrderDB.RemoveAccountOrder(this, userName, source);

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
        /// Վերադարձնում է հաշվի հեռացման հայտի տվյալները
        /// </summary>
        /// <returns></returns>
        public void Get()
        {
            AccountRemovingOrderDB.GetAccountRemovingOrder(this);
        }
    }
}
