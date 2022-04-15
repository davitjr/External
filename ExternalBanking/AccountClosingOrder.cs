using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class AccountClosingOrder : Order
    {
        /// <summary>
        /// Հաշվի փակման պատճառի տեսակ
        /// </summary>
        public ushort ClosingReasonType { get; set; }

        /// <summary>
        /// Հաշվի փակման պատճառի տեսակի նկարագրություն
        /// </summary>
        public string ClosingReasonTypeDescription { get; set; }

        /// <summary>
        /// Հաշվի փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReasonDescription { get; set; }
        /// <summary>
        /// Փակման ենթակա հաշիվիվները
        /// </summary>
        public List<Account> ClosingAccounts { get; set; }
        /// <summary>
        /// Հաշվի փակման հայտ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult CloseAccountOrder(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete(source);
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountClosingOrderDB.CloseAccountOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }





        /// <summary>
        /// Ընթացիկ հաշվի փակման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete(source);
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = AccountClosingOrderDB.CloseAccountOrder(this, userName, source);

                ActionResult resultOpPerson = base.SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }


        /// <summary>
        /// Ընթացիկ հաշվի փակման հայտի ուղարկում
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {

                    result = base.Approve(schemaType, userName);


                    if (result.ResultCode == ResultCode.Normal)
                    {
                        if ((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking) && (this.ClosingReasonType == 8 || this.ClosingReasonType == 9 || this.ClosingReasonType == 10))
                        {
                            user.userName = userName;
                            result = DocFlowManagement.DocFlow.SendAccountClosingOrderToConfirm(this, user, schemaType);
                            this.Quality = OrderQuality.Sent3;
                        }
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                }
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

        /// <summary>
        /// Հաշվի փակման հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        private ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
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
        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Հաշվի փակման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAccountClosingOrder(this));
            return result;
        }
        /// <summary>
        /// Վերադարձնում է հաշվի փակման հայտի տվյալները
        /// </summary>
        /// <returns></returns>
        public void Get()
        {
            AccountClosingOrderDB.GetAccountClosingOrder(this);
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

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountClosingOrderDB.CloseAccountOrder(this, userName, source);

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
        /// Հաշվի փակման հայտի համար
        /// </summary>
        internal static bool IsSecondClosing(ulong customerNumber, string accountNumber, SourceType sourceType)
        {
            bool secondClosing = AccountClosingOrderDB.IsSecondClosing(customerNumber, accountNumber, sourceType);
            return secondClosing;
        }
    }
}
