using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{

    public class ServicePaymentOrder : Order
    {
        /// <summary>
        /// Խնդրահարույց վարկերի տարանցիկ հաշիվ
        /// </summary>
       // public override Account DebitAccount { get; set; }

        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete();
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = ServicePaymentOrderDB.Save(this, userName, source);
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }

        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = ServicePaymentOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
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
        /// հաճախորդի հաշիվների և HB սպասարկման վճարների հայտի ստուգւոմներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {

            ActionResult result = new ActionResult();

            if ((this.Type == OrderType.AccountServicePayment || this.Type == OrderType.HBServicePayment))
            {
                result.Errors.AddRange(ValidateForDAHKAvailability());

                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));
            result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));

            result.Errors.AddRange(ValidateForCustomerDebts());


            if (this.Type != OrderType.AccountServicePaymentXnd && this.Type != OrderType.HBServicePaymentXnd)
                result.Errors.AddRange(ValidateForAmountRest());
            else
                result.Errors.AddRange(ValidateForXndraharujcAmountRest());


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }



        /// <summary>
        /// Հաճախորդի ԴԱՀԿ ենթահաշիվը գտնված չէ կամ ունի փակված կարգավիճակ 
        /// </summary>
        /// <returns></returns>
        public List<ActionError> ValidateForDAHKAvailability()
        {
            List<ActionError> result = new List<ActionError>();
            List<Account> dahkAccountList;
            bool isDahk = Validation.IsDAHKAvailability(this.CustomerNumber);
            if (Validation.IsDAHKAvailability(this.CustomerNumber))
            {
                dahkAccountList = Account.GetDAHKAccountList(this.CustomerNumber);

                if (dahkAccountList == null || dahkAccountList.Count == 0)
                    result.Add(new ActionError(612));
            }
            return result;
        }

        private void Complete()
        {
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Type == OrderType.AccountServicePaymentXnd || this.Type == OrderType.HBServicePaymentXnd)
                this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
        }
        /// <summary>
        /// ստտուգում է հաճախորդի սպասարկման գծով պարտավորությունները
        /// </summary>
        /// <returns></returns>
        public List<ActionError> ValidateForCustomerDebts()
        {
            List<ActionError> result = new List<ActionError>();
            double amount = 0;
            //Հաճախորդը չունի պարտավորություն հաշիվների սպասարկման գծով: 
            if (this.Type == OrderType.AccountServicePayment || this.Type == OrderType.AccountServicePaymentXnd)
            {
                amount = CustomerDebts.GetCustomerServiceFeeDebt(this.CustomerNumber, DebtTypes.CurrentAccount);
                if (amount == 0)
                    result.Add(new ActionError(614));
                else this.Amount = amount;
            }
            else
                //Հաճախորդը չունի պարտավորություն ՀԲ սպասարկման գծով:
                if (this.Type == OrderType.HBServicePayment || this.Type == OrderType.HBServicePaymentXnd)
            {
                amount = CustomerDebts.GetCustomerServiceFeeDebt(this.CustomerNumber, DebtTypes.HomeBanking);
                if (amount == 0)
                    result.Add(new ActionError(616));
                else this.Amount = amount;
            }
            return result;
        }
        /// <summary>
        /// վերադարձնում է հաճախորդի բոլոր հաշիվների հասանելի գումարները
        /// </summary>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <param name="debt">հաճախորդի սպասարկման գծով պարտքը</param>
        /// <param name="setDate"></param>
        /// <param name="creditAccount">հաշվեհամարը որից կատարվում է գանձումը</param>
        /// <param name="provisionSign">նշում է թե հաճախորդը ունի՞  գրավադրված գումար</param>
        /// <returns></returns>
        public static double ServicePaymentPreparation(ulong customerNumber, double debt, DateTime setDate, string creditAccount, bool provisionSign)
        {
            return ServicePaymentOrderDB.ServicePaymentPreparation(customerNumber, debt, setDate, creditAccount, provisionSign);
        }
        /// <summary>
        /// հաճախորդի հասանելի գումարի ստուգում
        /// </summary>
        /// <returns></returns>
        public List<ActionError> ValidateForAmountRest()
        {
            List<ActionError> result = new List<ActionError>();
            double accountsRest = 0;
            double accountRest = 0;
            double sentNotConfirmer = 0;

            string accountNumber = Account.GetNotFreezedCurrentAccount(this.CustomerNumber, "AMD");
            if (accountNumber == "" || accountNumber == null)
            {
                result.Add(new ActionError(531));//Հաճախորդը չունի ընթացիկ դրամային հաշիվ:
                return result;
            }
            Account account = Account.GetAccount(accountNumber);
            bool provisionSign = Customer.CheckForProvision(this.CustomerNumber);
            double debt = 0;
            if (this.Type == OrderType.AccountServicePayment)
            {
                debt = CustomerDebts.GetCustomerServiceFeeDebt(this.CustomerNumber, DebtTypes.CurrentAccount);
            }
            else
                if (this.Type == OrderType.HBServicePayment)
            {
                debt = CustomerDebts.GetCustomerServiceFeeDebt(this.CustomerNumber, DebtTypes.HomeBanking);
            }

            accountsRest = ServicePaymentPreparation(this.CustomerNumber, debt, this.RegistrationDate, account.AccountNumber, provisionSign);
            accountRest = Account.GetAcccountAvailableBalance(account.AccountNumber);

            if (this.OnlySaveAndApprove && this.Type == OrderType.AccountServicePayment)
            {
                List<string> accountNumbers = Account.GetAccountsForServicePaymentChecking(this.CustomerNumber);

                foreach (var acc in accountNumbers)
                {
                    sentNotConfirmer += Order.GetSentNotConfirmedAmounts(acc, OrderType.CashDebit);
                }

                if (accountsRest + accountRest + sentNotConfirmer < debt)
                {
                    result.Add(new ActionError(665));//Հաճախորդի հաշիվների գումարը բավարար չէ գործարքի համար
                }
            }
            else if (accountsRest + accountRest <= 0)
            {
                result.Add(new ActionError(665));//Հաճախորդի հաշիվների գումարը բավարար չէ գործարքի համար
            }

            return result;
        }

        /// <summary>
        /// Խնդրահարույց վարիկերի տարանցիկ հաշվի հասանելի գումարի ստուգում
        /// </summary>
        /// <returns></returns>
        public List<ActionError> ValidateForXndraharujcAmountRest()
        {
            List<ActionError> result = new List<ActionError>();//Karen
            double accountRest = 0;
            accountRest = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);

            if (OnlySaveAndApprove)
            {
                accountRest += Order.GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.TransitPaymentOrder);
            }

            DebitAccount.Balance = accountRest;
            if (accountRest <= 0)
                result.Add(new ActionError(701));
            return result;

        }




    }
}
