using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public class LoanProductActivationOrder : Order
    {
        /// <summary>
        /// Ֆակտորինգային հաճախորդի հաշվեհամարը, որին պետք է փոխանցվի գումարը
        /// </summary>
        public ulong FactoringCustomerAccount { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }
        /// <summary>
        /// Վարկային պրոդուկտի տրման միջնորդավճարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }
        /// <summary>
        /// Վարկի տրման միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի տրման միջնորդավճարի հաշիվ ԱԱՀ-ով
        /// </summary>
        public Account FeeAccountWithTax { get; set; }

        /// <summary>
        /// Վարկի տրման միջնորդավճար ԱԱՀ-ով
        /// </summary>
        public double FeeAmountWithTax { get; set; }

        /// <summary>
        /// Ցույց է տալիս միջնորդավճարի գանձումը կատարվում է խնդրահարույց հաշվից թե ոչ
        /// </summary>
        public bool FeeForTrasitAccount { get; set; }

        /// <summary>
        /// Ապահովագրության ընդհանուր գումար
        /// </summary>
        public Double TotalInsuranceAmount { get; set; }

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = LoanProductActivationOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (this.Type == OrderType.LoanActivation)
            {
                result.Errors.AddRange(Validation.ValidateLoanActivationOrderDocument(this));
            }
            if (this.Type == OrderType.CreditLineActivation)
            {
                result.Errors.AddRange(Validation.ValidateCreditLineActivationOrderDocument(this));
            }

            if (this.Type == OrderType.GuaranteeActivation)
            {
                result.Errors.AddRange(Validation.ValidateGuaranteeActivationOrderDocument(this));
            }

            if (this.Type == OrderType.AccreditiveActivation)
            {
                result.Errors.AddRange(Validation.ValidateAccreditiveActivationOrderDocument(this));
            }

            if (this.Type == OrderType.FactoringActivation)
            {
                result.Errors.AddRange(Validation.ValidateFactoringActivationOrderDocument(this));
            }

            if (this.Type == OrderType.PaidGuaranteeActivation)
            {
                result.Errors.AddRange(Validation.ValidatePaidGuaranteeActivationOrderDocument(this));
            }
            if (this.Type == OrderType.PaidFactoringActivation)
            {
                result.Errors.AddRange(Validation.ValidatePaidFactoringActivationOrderDocument(this));
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            LoanProductActivationOrderDB.Get(this);
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>

        /// <summary>
        /// Ավանդի գրավով վարկի(վարկային գծի) հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

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
        /// Ավանդի գրավով վարկի(վարկային գծի) հայտի հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            DateTime nextOperDay = Utility.GetNextOperDay().Date;
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));



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
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;

            if (this.Type == OrderType.LoanActivation || this.Type == OrderType.PaidGuaranteeActivation)
            {
                this.SubType = 1;
                if (this.FeeForTrasitAccount && this.Type == OrderType.PaidGuaranteeActivation)
                {
                    this.FeeAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                }
            }
            if (this.Type == OrderType.CreditLineActivation)
            {
                CreditLine creditLine = CreditLine.GetCreditLine(this.ProductId, this.CustomerNumber);
                if (creditLine.Type != 25 && creditLine.Type != 18 && creditLine.Type != 46 && creditLine.Type != 60)
                {
                    this.SubType = 1;
                }
                else
                {
                    this.SubType = 2;
                }

            }
            if (this.Type == OrderType.GuaranteeActivation || this.Type == OrderType.AccreditiveActivation
                || this.Type == OrderType.FactoringActivation || this.Type == OrderType.PaidFactoringActivation)
            {
                this.SubType = 1;

                if (this.FeeForTrasitAccount)
                {
                    this.FeeAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                    this.FeeAccountWithTax = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                }
            }


            if (this.Source == SourceType.EContract)
            {
                if (this.Type == OrderType.LoanActivation)
                {

                    Loan loan = Loan.GetLoan(this.ProductId, this.CustomerNumber);
                    if (loan != null)
                    {
                        this.FeeAccount = loan.ConnectAccount;
                    }
                    if (loan != null)
                    {
                        this.Amount = Math.Abs(loan.ContractAmount) != 0 ? Math.Abs(loan.ContractAmount) : loan.StartCapital;
                        this.Currency = loan.Currency;
                    }
                }

                if (this.Type == OrderType.CreditLineActivation)
                {

                    CreditLine creditLine = CreditLine.GetCreditLine(this.ProductId, this.CustomerNumber);
                    if (creditLine != null)
                    {
                        this.FeeAccount = creditLine.ConnectAccount;
                    }
                    if (creditLine != null)
                    {
                        this.Amount = Math.Abs(creditLine.ContractAmount) != 0 ? Math.Abs(creditLine.ContractAmount) : creditLine.StartCapital;
                        this.Currency = creditLine.Currency;
                    }
                }

            }


            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            this.Fees = new List<OrderFee>();
            OrderFee fee = new OrderFee();
            if ((this.FeeAmount + this.TotalInsuranceAmount > 0 && this.FeeAccount != null) || this.Source == SourceType.EContract)
            {
                fee.Amount = this.FeeAmount;
                fee.Account = this.FeeAccount;
                if (this.Type == OrderType.LoanActivation || this.Type == OrderType.PaidGuaranteeActivation)
                {
                    fee.Type = 4;
                }
                else if (this.Type == OrderType.CreditLineActivation)
                {
                    fee.Type = 21;
                }
                else if (this.Type == OrderType.GuaranteeActivation)
                {
                    fee.Type = 26;
                }
                else if (this.Type == OrderType.AccreditiveActivation)
                {
                    fee.Type = 27;
                }
                else if (this.Type == OrderType.PaidFactoringActivation)
                {
                    fee.Type = 28;
                }

                fee.OrderNumber = this.OrderNumber;
                fee.Currency = "AMD";
                this.Fees.Add(fee);
            }

            if (this.FeeAmountWithTax > 0 && this.FeeAccountWithTax != null)
            {
                fee = new OrderFee();
                fee.Amount = this.FeeAmountWithTax;
                fee.Account = this.FeeAccountWithTax;
                if (this.Type == OrderType.CreditLineActivation)
                {
                    fee.Type = 22;
                }
                else if (this.Type == OrderType.GuaranteeActivation)
                {
                    fee.Type = 26;
                }

                else if (this.Type == OrderType.AccreditiveActivation)
                {
                    fee.Type = 27;
                }

                fee.OrderNumber = this.OrderNumber;
                fee.Currency = "AMD";
                this.Fees.Add(fee);
            }

            if (this.Fees.Count == 0)
            {
                this.Fees = null;
            }

        }

        /// <summary>
        ///Ավանդի գրավով վարկի(վարկային գծի) հայտի պահպանում և ուղղարկում
        /// </summary>
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

            string cardNumber = null;

            if (this.Type == OrderType.CreditLineActivation && source == SourceType.Bank)
            {
                cardNumber = Card.GetCardNumberWithCreditLineAppId(this.ProductId, CredilLineActivatorType.ActivateCreditLine);
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = LoanProductActivationOrderDB.Save(this, userName, source, cardNumber);

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

                if (this.Fees != null)
                {
                    result = base.SaveOrderFee();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
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

        public bool IsSecondActivation()
        {
            return LoanProductActivationOrderDB.IsSecondActivation(this);
        }

        public static long? CheckPreviousActivationOrderId(LoanProductActivationOrder order)
        {
            return LoanProductActivationOrderDB.CheckPreviousActivationOrderId(order);
        }

        public static double GetLoanProductActivationFee(ulong productId, short withTax)
        {
            return LoanProductActivationOrderDB.GetLoanProductActivationFee(productId, withTax);
        }

        public static bool CheckLoanDocumentAttachment(int loanType, long productId, int sourceType, double amount, ulong customerNumber)
        {
            return LoanProductActivationOrderDB.CheckLoanDocumentAttachment(loanType, productId, sourceType, amount, customerNumber);
        }

        public static List<ActionError> LoanActivationValidation(Loan loan, LoanProductActivationOrder order)
        {
            List<ActionError> errors = new List<ActionError>();
            if ((loan.Quality == 10 && loan.LoanType != 29) && loan.LoanType != 33 && loan.LoanType != 38 && loan.LoanType != 34)
            {
                errors = LoanProductActivationOrderDB.LoanActivationValidation(loan, order);
            }
            return errors;

        }

        public static List<string> GetLoanActivationWarnings(long productId, ulong customerNumber, short productType)
        {
            return LoanProductActivationOrderDB.GetLoanActivationWarnings(productId, customerNumber, productType);
        }

        public static List<ulong> GetOwnerCustomerNumbers(long productId, string forCheck)
        {
            return LoanProductActivationOrderDB.GetOwnerCustomerNumbers(productId, forCheck);
        }
        /// <summary>
        /// ԿԳ կողմից ձևակերպվող պայմանագրի <ստուգված> կարգավիճակի ստուգում
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckLoanProductActivationStatus(ulong productId)
        {
            return LoanProductActivationOrderDB.CheckLoanProductActivationStatus(productId);
        }

        /// <summary>
        /// Տրանսպորտային միջոցի/գյուղ․տեխնիկայի գծով ծախսի ստուգում
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool IsTranpsortExpensePaid(long productId)
        {
            return LoanProductActivationOrderDB.IsTransportExpensePaid(productId);
        }


        public static double GetLoanTotalInsuranceAmount(ulong productId)
        {
            return LoanProductActivationOrderDB.GetLoanTotalInsuranceAmount(productId);
        }
        /// <summary>
        /// Վարկի ակտիվացման հայտերի ավտոմատ ստեղծում նախնական հայտերի հիման վրա
        /// </summary>
        /// <param name="preOrderId"></param>
        /// <returns></returns>
        public static List<LoanProductActivationOrder> GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(long preOrderId)
        {

            return LoanProductActivationOrderDB.GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(preOrderId).Result;
        }
        public static List<Account> GetFactoringCustomerCardAndCurrencyAccounts(ulong productId, string currency)
        {
            List<Account> accounts = new List<Account>();
            List<Account> cardAccounts = new List<Account>();
            List<Account> currencyAccounts = new List<Account>();

            ulong factoringCustomerNumber = FactoringDB.GetFactoringCustomerNumber(productId);

            cardAccounts = Account.GetCardAccounts(factoringCustomerNumber);
            currencyAccounts = Account.GetCurrentAccounts(factoringCustomerNumber, ProductQualityFilter.Opened);
            currencyAccounts.RemoveAll(a => a.JointType != 0);
            accounts.AddRange(cardAccounts);
            accounts.AddRange(currencyAccounts);

            IEnumerable<Account> query = (from account in accounts
                                          where account.Currency == currency
                                          select account).ToList();

            accounts = (List<Account>)query;
            return accounts;
        }
        public static List<Account> GetFactoringCustomerFeeCardAndCurrencyAccounts(ulong productId)
        {
            List<Account> accounts = new List<Account>();
            List<Account> cardAccounts = new List<Account>();
            List<Account> currencyAccounts = new List<Account>();

            ulong factoringCustomerNumber = FactoringDB.GetFactoringCustomerNumber(productId);

            cardAccounts = Account.GetCardAccounts(factoringCustomerNumber);
            currencyAccounts = Account.GetCurrentAccounts(factoringCustomerNumber, ProductQualityFilter.Opened);
            currencyAccounts.RemoveAll(a => a.JointType != 0);
            accounts.AddRange(cardAccounts);
            accounts.AddRange(currencyAccounts);


            IEnumerable<Account> query = (from account in accounts
                                          where account.Currency == "AMD"
                                          select account).ToList();

            accounts = (List<Account>)query;
            return accounts;
        }

        /// <summary>
        /// Պայմանագրի բանկի տարածքից դուրս ստորագրվելու ստուգում
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        public static bool SignedOutOfBankCheck(ulong AppId)
        {
            return LoanProductActivationOrderDB.SignedOutOfBankCheck(AppId);
        }
    }
}

