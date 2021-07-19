using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;

namespace ExternalBanking
{
    public class CashPosPaymentOrder : Order
    {
        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Հավաստարման կոդ
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        ///  Ելքագրվող (դեբետ POS) հաշիվ
        /// </summary>
        public Account PosAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ դրամարկղ) հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }


        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CashPosPaymentOrderDB.Save(this, userName, source);
               
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                result = base.SaveOrderOPPerson();
                result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);


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
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateCashOperationAvailability(this, user));
            result.Errors.AddRange(CheckCurrencies());
            result.Errors.AddRange(Validation.ValidateOPPerson(this));

            if (string.IsNullOrEmpty(this.Currency))
            {
                result.Errors.Add(new ActionError(254));
            }

            if (this.Amount <= 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Errors.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                result.Errors.Add(new ActionError(25));
            }


            if (this.Currency != "AMD" && !Validation.IsCurrencyAmountCorrect(this.Amount, this.Currency))
            {
                //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                result.Errors.Add(new ActionError(1053, new string[] { this.Currency }));
            }


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
        private void Complete()
        {
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            //Կանխիկացում POS  տերմինալով եթե մջնորդավճարը կանխիկ է 
            if (this.Type == OrderType.CashPosPayment )
            {
                if (this.Fees != null)
                {
                    foreach (OrderFee fee in this.Fees)
                    {
                        if (fee.Type == 6)
                        {
                            fee.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", user.filialCode);
                            fee.CreditAccount = OperationAccountHelper.GetOperationSystemAccountForFee(this, fee.Type);
                        }

                    }
                }
            }

             this.PosAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.Currency, user.filialCode);

             if (this.CreditAccount != null && this.CreditAccount.Status == 7)
            {
                this.CreditAccount = Account.GetSystemAccount(this.CreditAccount.AccountNumber);
            }
            else
                this.CreditAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), this.PosAccount.Currency, user.filialCode);

        }
        private List<ActionError> CheckCurrencies()
        {
            List<ActionError> result = new List<ActionError>();
            if (this.Currency != "AMD" && this.Currency != "USD" && this.Currency != "EUR")
                result.Add(new ActionError(667));
            return result;  
        }

        private List<ActionError> ValidateCardNumber()
        {
            List<ActionError> result = new List<ActionError>();
            if(this.CardNumber.Length!=15 || this.CardNumber.Length!=16 || this.CardNumber.Length!=19)
                result.Add(new ActionError(534));
            return result;
        }


        public void Get()
        {
            CashPosPaymentOrderDB.GetCashPosPaymentOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
            this.Fees = Order.GetOrderFees(this.Id);

        }
    }
}
