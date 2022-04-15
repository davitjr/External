using ExternalBanking.CTPayment;
using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Վարկի մարում հայտ
    /// </summary>
    public class CTLoanMatureOrder : CTOrder
    {
        /// <summary>
        /// Կրեդիտագրվող հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Փոխանակման փոխարժեք
        /// </summary>
        public double ConvertationRate { get; set; }

        /// <summary>
        /// Վարկային կոդով է թե ոչ
        /// </summary>
        public bool WithCreditCode { get; set; }

        /// <summary>
        /// Վարկի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Պրոդուկտի հաշիվ
        /// </summary>
        public Account ConnectAccount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MatureMode MatureMode { get; set; }

        public PaymentRegistrationResult SaveCTLoanMatureOrder(string userName)
        {
            Complete();
            PaymentRegistrationResult result = Validate();
            if (result.ResultCode == 0)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = CTOrderDB.SaveCTLoanMatureOrder(this, userName);
                    base.Approve(1, userName);
                    scope.Complete();
                }
            }
            return result;
        }
        public PaymentRegistrationResult SaveAndApproveCTLoanMatureOrder(string userName)
        {
            Complete();
            PaymentRegistrationResult result = Validate();
            if (result.ResultCode == 0)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = CTOrderDB.SaveCTLoanMatureOrder(this, userName);
                    base.Approve(1, userName);
                    base.Confirm();
                    scope.Complete();
                }
            }
            return result;
        }
        private void Complete()
        {
            if (this.Source != SourceType.SSTerminal)
            {
                this.RegistrationDate = DateTime.Now.Date;

                this.Description = this.CTCustomerDescription + " " + "Վարկի մարում այլ կանխիկ տերմինալով: " + this?.Description;

                this.Quality = OrderQuality.Draft;
                this.Type = OrderType.LoanMature;
                this.SubType = 6;
                this.Currency = this.DebitAccount.Currency;
            }

            Loan loan = null;
            loan = Loan.GetLoan(this.CreditAccount.AccountNumber);
            this.CreditAccount = loan.LoanAccount;
            this.ConnectAccount = loan.ConnectAccount;
            this.ProductId = (ulong)loan.ProductId;
            this.CustomerNumber = loan.ConnectAccount.GetAccountCustomerNumber();
            this.FilialCode = (ushort)Customer.GetCustomerFilial(this.CustomerNumber).key;

            if (this.IdentifierType == 4)
            {
                this.WithCreditCode = true;
            }
            if (this.CreditAccount.Currency != "AMD")
            {
                this.ConvertationRate = Utility.GetKursForDate(this.RegistrationDate, this.CreditAccount.Currency, 1);
            }
            else
            {
                this.ConvertationRate = 0;
            }

        }

        public void Get()
        {
            CTOrderDB.GetCTLoanMatureOrder(this);
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
        public static Loan GetLoanByLoanFullNumber(string loanFullNumber)
        {
            return Loan.GetLoan(loanFullNumber);
        }
    }
}
