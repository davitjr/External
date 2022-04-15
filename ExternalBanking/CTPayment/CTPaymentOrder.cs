using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Փոխանցում հաշվին հայտ
    /// </summary>
    public class CTPaymentOrder : CTOrder
    {

        /// <summary>
        /// Կրեդիտագրվող հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Փոխանակման փոխարժեք
        /// </summary>
        public double ConvertationRate { get; set; }

        public PaymentRegistrationResult SaveCTPaymentOrder(string userName)
        {
            Complete();
            PaymentRegistrationResult result = Validate();
            if (result.ResultCode == 0)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    result = CTOrderDB.SaveCTPaymentOrder(this, userName);
                    base.Approve(1, userName);
                    scope.Complete();
                }
            }
            return result;
        }
        public PaymentRegistrationResult SaveAndApproveCTPaymentOrder(string userName)
        {
            Complete();
            PaymentRegistrationResult result = Validate();
            if (result.ResultCode == 0)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    result = CTOrderDB.SaveCTPaymentOrder(this, userName);
                    base.Approve(1, userName);
                    base.Confirm();
                    scope.Complete();
                }
            }
            return result;
        }
        public PaymentRegistrationResult Validate()
        {
            PaymentRegistrationResult result = new PaymentRegistrationResult();
            if (IsPaymentIdUnique())
            {
                result.ResultCode = 11;
                result.ResultDescription = "Կրկնվող համարով հայտ:";
            }

            return result;
        }

        private void Complete()
        {
            if (this.Source == SourceType.SSTerminal)
            {
                this.FilialCode = SSTerminal.GetTerminalFilial(this.TerminalID);
                this.CreditAccount.Currency = AccountDB.GetAccountCurrency(this.CreditAccount.AccountNumber);
                if (this.CreditAccount.Currency != "AMD")
                {
                    this.ConvertationRate = Utility.GetKursForDate(this.RegistrationDate, this.CreditAccount.Currency, 1, this.FilialCode);
                }
                return;
            }
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            if (this.PaymentType == 4)
            {
                this.Type = OrderType.LoanMatureByOtherCashTerminal;

                this.Description = this.CTCustomerDescription + " " + "Վարկի մարում գրաֆիկով այլ կանխիկ տերմինալով: " + this?.Description;
            }
            else
            {
                this.Description = this.CTCustomerDescription + " " + "Մուտք հաշվին այլ կանխիկ տերմինալով: " + this?.Description;
            }
            this.Quality = OrderQuality.Draft;
            this.CreditAccount = Account.GetAccount(this.CreditAccount.AccountNumber);
            this.CustomerNumber = this.CreditAccount.GetAccountCustomerNumber();
            this.FilialCode = (ushort)Customer.GetCustomerFilial(this.CustomerNumber).key;
            this.Currency = this.DebitAccount.Currency;
            if (this.CreditAccount.Currency != "AMD")
            {
                if (this.PaymentType != 4)
                {
                    this.Type = OrderType.CashConvertationByOtherCashTerminal;
                }
                this.ConvertationRate = Utility.GetKursForDate(this.RegistrationDate, this.CreditAccount.Currency, 1);
            }
            else
            {
                if (this.PaymentType != 4)
                {
                    this.Type = OrderType.CashInByOtherCashTerminal;
                }
                this.ConvertationRate = 0;
            }
        }

        public void Get()
        {
            CTOrderDB.GetCTPaymentOrder(this);
        }


    }
}
