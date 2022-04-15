using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class AccountReOpenOrder : AccountOrder
    {
        /// <summary>
        /// Միջնորդավճարի գանձման տեսակ
        /// </summary>
        public ushort FeeChargeType { get; set; }

        /// <summary>
        /// Վերաբացման պատճառի նկարագրություն
        /// </summary>
        public string ReopenReasonDescription { get; set; }

        /// <summary>
        /// Փակման ենթակա հաշիվները
        /// </summary>
        public List<Account> ReOpeningAccounts { get; set; }
        /// <summary>
        ///Հաշվի հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete(source);
            ActionResult result = this.Validate(user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = AccountOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }


        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateAccountReOpenOrder(this));

            if (FeeChargeType == 1)
            {
                result.Errors.AddRange(Validation.ValidateCashOperationAvailability(this, user));
            }
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի վերաբացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public new void Get()
        {
            AccountReOpenOrderDB.GetAccountReOpenOrder(this);
            this.Fees = Order.GetOrderFees(this.Id);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }

        /// <summary>
        /// Հաշվի վերաբացման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if ((this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft) && result.ResultCode == ResultCode.Normal)
            {
                Account.GetCurrentAccountContractBefore(Id, CustomerNumber, 5);
            }

            if ((Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                || (((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking) && OrderAttachment.HasAttachedFile(this.Id, 5))))
            {
                if (result.ResultCode == ResultCode.Normal)
                {

                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                    {
                        result = base.Approve(schemaType, userName);
                        if (result.ResultCode == ResultCode.Normal)
                        {
                            LogOrderChange(user, Action.Update);
                            scope.Complete();
                        }
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
        /// Հաշվի վերաբացման հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public new ActionResult ValidateForSend()
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

            this.Currency = this.ReOpeningAccounts[0].Currency;
            for (int i = 0; i < this.ReOpeningAccounts.Count; i++)
            {
                this.ReOpeningAccounts[i] = Account.GetAccount(this.ReOpeningAccounts[i].AccountNumber);
            }


            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber))
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            if (this.FeeChargeType != 1)
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if (this.Fees != null)
            {
                if (this.Fees.Exists(m => m.Type == 12))
                {
                    this.Fees.Find(m => m.Type == 12).Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                }
                this.Fees.ForEach(m =>
                {
                    m.CreditAccount = OperationAccountHelper.GetOperationSystemAccountForFee(this, m.Type);
                });
            }

        }

        /// <summary>
        ///Հաշվի վերաբացման հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete(source);
            ActionResult result = this.Validate(user);
            List<ActionError> warnings = new List<ActionError>();

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
                result = AccountOrderDB.Save(this, userName, source);
                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

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

            if (resultConfirm.ResultCode == ResultCode.Normal)
            {
                warnings.AddRange(CheckAfterOrderConfirm(this));
                resultConfirm.Errors = warnings;
            }

            return resultConfirm;
        }

        /// <summary>
        /// Հաշվի վերաբացման կրկնակի հայտի առկայություն
        /// </summary>
        internal static bool IsSecondReOpen(ulong customerNumber, string accountNumber)
        {
            bool secondReOpen = AccountReOpenOrderDB.IsSecondReOpen(customerNumber, accountNumber);
            return secondReOpen;
        }

        /// <summary>
        /// Հաշվի վերաբացման թույլատրում:Հաշվի սինթետիկ հաշվի համապատասխանություն հաճախորդի կարգավիճակին:
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public bool CanReOpenAccount(Account ReOpenAccount)
        {
            bool canReOpen = AccountReOpenOrderDB.CheckSintAcc(this.CustomerNumber, ReOpenAccount.AccountNumber);

            if (canReOpen)
            {
                canReOpen = AccountReOpenOrderDB.CheckNewSintAcc(this.CustomerNumber, ReOpenAccount.AccountNumber);
            }

            return canReOpen;
        }

    }
}
