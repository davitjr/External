using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
    public class TransitPaymentOrder : Order
    {

        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ (դրամարկղի հաշիվ)
        /// </summary>
        // public override Account DebitAccount { get; set; }

        /// <summary>
        /// Տարանցիկ հաշվի տեսակ
        /// </summary>
        public TransitAccountTypes DebitTransitAccountType { get; set; }

        /// <summary>
        /// Տարանցիկ հաշիվ
        /// </summary>
        public Account TransitAccount { get; set; }

        /// <summary>
        /// Տարանցիկ հաշվի տեսակ
        /// </summary>
        public TransitAccountTypes TransitAccountType { get; set; }

        /// <summary>
        /// Տարանցիկ հաշվի տեսակի նկարագրություն
        /// </summary>
        public string TransitAccountTypeDescription { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Պրոդուկտի արժույթ
        /// </summary>
        public string ProductCurrency { get; set; }

        /// <summary>
        /// Հաճախորդի տեսակ «Ռեեստրի վարման և պահառության վճարներ» -ի համար
        /// </summary>
        public ushort CustomerType { get; set; } = 0;

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
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
                result = TransitPaymentOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }
        /// <summary>
        /// Տարանցիկ հաշվին հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateTransitPaymentOrder(this, user));
            return result;
        }
        /// <summary>
        /// Տարանցիկ հաշվին փոխանցման հայտի հաստատում
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



            return result;
        }
        /// <summary>
        /// Տարանցիկ հաշվին փոխանցման հայտի հաստատման ստուգումներ:
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }

            if (this.Type == OrderType.NonCashTransitPaymentOrder)
            {
                result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));
            }



            if (this.Fees != null && this.Fees.Count > 0)
            {
                foreach (OrderFee fee in this.Fees)
                {
                    if (fee.Type == 29)
                    {
                        double feeAmount = this.Fees.FindAll(m => m.Account.AccountNumber == fee.Account.AccountNumber).Sum(m => m.Amount);
                        double feeAccountBalance = Account.GetAcccountAvailableBalance(fee.Account.AccountNumber);

                        if (feeAccountBalance < feeAmount)
                        {

                            if (Account.AccountAccessible(fee.Account.AccountNumber, user.AccountGroup))
                            {
                                //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                                result.Errors.Add(new ActionError(499, new string[] { fee.Account.AccountNumber, feeAccountBalance.ToString("#,0.00") + " " + this.Fees.Find(m => m.Currency == "AMD").Account.Currency, this.Fees.Find(m => m.Currency == "AMD").Account.Balance.ToString("#,0.00") + " " + this.Fees.Find(m => m.Currency == "AMD").Account.Currency }));
                            }
                            else
                            {
                                //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                                result.Errors.Add(new ActionError(788, new string[] { fee.Account.AccountNumber }));
                            }
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
        /// Վերադարձնում է տարանցիկ հաշվին փոխանցման հայտի տվյալները:
        /// </summary>
        public void Get()
        {
            TransitPaymentOrderDB.Get(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
            this.Fees = Order.GetOrderFees(this.Id);
        }
        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.SubType = 1;
            //Դեբետ հաշվի լրացում
            //Եթե անկանխիկ փոխանցում է դեբետ հաշիվը հաճախորդի հաշիվն է
            if (this.Type != OrderType.NonCashTransitPaymentOrder)
            {
                //ԱԳՍ-ով փոխանցումների դեպք
                //Նշված դեպքում դրամարկղի հաշիվ պետք չէ որոշել
                if (this.DebitAccount != null && this.DebitAccount.Status == 7)
                {
                    this.DebitAccount = Account.GetSystemAccount(this.DebitAccount.AccountNumber);
                }
                else
                {
                    this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.Currency, user.filialCode);
                }

            }
            else
            {

                if (this.DebitTransitAccountType != TransitAccountTypes.None)
                {
                    this.DebitAccount = Account.GetOperationSystemAccount(GetTransitPaymentOrderSystemAccountType(this.DebitTransitAccountType), this.Currency, user.filialCode, this.CustomerType);
                }
                else
                {
                    this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
                }

            }

            //Տարանցիկ հաշվի լրացում
            if (this.TransitAccountType == TransitAccountTypes.ForMatureByCreditCode)
            {
                ushort accountType = 0;
                if ((this.Currency == "AMD" && this.ProductCurrency == "AMD") || (this.Currency != "AMD" && this.ProductCurrency != "AMD"))
                {
                    accountType = 224;
                }
                else if (this.Currency == "AMD" && this.ProductCurrency != "AMD")
                {
                    accountType = 279;
                }

                this.TransitAccount = Account.GetProductAccount(this.ProductId, 18, accountType);


            }
            else
            {
                this.TransitAccount = Account.GetOperationSystemAccount(GetTransitPaymentOrderSystemAccountType(this.TransitAccountType), this.Currency, user.filialCode, this.CustomerType);
            }


            if (this.Fees != null)
            {
                this.Fees.ForEach(m =>
                {
                    if (m.Type == 8 || m.Type == 28)
                    {
                        m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                    }
                    m.CreditAccount = OperationAccountHelper.GetOperationSystemAccountForFee(this, m.Type);
                });
            }

        }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
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
                result = TransitPaymentOrderDB.Save(this, userName, source);
                if (this.TransitAccountType == TransitAccountTypes.ForLeasingLoans && this.AdditionalParametrs != null)
                {
                    LeasingDB.SaveLeasingPaymentDetails(this);
                }

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

                result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }



                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);


                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }

            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                warnings.AddRange(user.CheckForNextCashOperation(this));
            }
            catch
            {

            }


            result.Errors = warnings;
            return result;
        }

        public static ushort GetTransitPaymentOrderSystemAccountType(TransitAccountTypes transitAccountType)
        {
            return TransitPaymentOrderDB.GetTransitPaymentOrderSystemAccountType(transitAccountType);
        }

    }
}
