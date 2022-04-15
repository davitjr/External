using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Բյուջե փոխանցման ձևանմուշ
    /// </summary>
    public class BudgetPaymentOrderTemplate : Template
    {
        /// <summary>
        /// Մարամասների ունիկալ համար
        /// </summary>
        public int BudgetPaymentOrderTemplateId { get; set; }

        /// <summary>
        /// Փոխանցում
        /// </summary>
        public BudgetPaymentOrder BudgetPaymentOrder { get; set; }




        /// <summary>
        /// Ձևանմուշի/խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            this.Complete(source);
            this.BudgetPaymentOrder.Complete();
            this.BudgetPaymentOrder.GetPayerDocumentNumber();
            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.BudgetPaymentOrder.Validate(this.TemplateType);
                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                }
                else
                {
                    Action action;
                    if (this.ID > 0)
                    {
                        action = Action.Update;
                    }
                    else
                    {
                        action = Action.Add;
                    }
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                    {
                        result = BudgetPaymentOrderTemplateDB.SaveBudgetPaymentOrderTemplate(this, action);

                        if (result.Errors.Count == 0)
                        {
                            if (TemplateType == TemplateType.CreatedAsGroupService)
                                OrderGroupDB.SaveGroupTemplateShortInfo(GroupTemplateShrotInfo, result.Id, action);
                            ActionResult resultOrderFee = base.SaveTemplateFee(this.BudgetPaymentOrder);
                        }

                        scope.Complete();
                    }
                }
            }

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }

        /// <summary>
        /// Ավտոոմատ լրացվող դաշտեր
        /// </summary>
        /// <param name="source"></param>
        protected void Complete(SourceType source)
        {
            this.TemplateSourceType = source;
            this.BudgetPaymentOrder.Type = this.TemplateDocumentType;
            this.BudgetPaymentOrder.SubType = this.TemplateDocumentSubType;


            if (this.TemplateType != TemplateType.CreatedAsGroupService)
            {
                if (this.BudgetPaymentOrder.DebitAccount != null && ((!this.BudgetPaymentOrder.ValidateForTransit && this.BudgetPaymentOrder.ValidateDebitAccount) || this.BudgetPaymentOrder.Type == OrderType.CardServiceFeePayment))
                {
                    this.BudgetPaymentOrder.DebitAccount = Account.GetAccount(this.BudgetPaymentOrder.DebitAccount.AccountNumber);
                }
                else
                {
                    //ԱԳՍ-ով փոխանցումների դեպք
                    //Նշված դեպքում դրամարկղի հաշիվ պետք չէ որոշել
                    if (this.BudgetPaymentOrder.DebitAccount != null && this.BudgetPaymentOrder.DebitAccount.Status == 7)
                    {
                        this.BudgetPaymentOrder.DebitAccount = Account.GetSystemAccount(this.BudgetPaymentOrder.DebitAccount.AccountNumber);
                    }

                }
            }

            if (TemplateType == TemplateType.CreatedAsGroupService)
            {
                GroupTemplateShrotInfo shortinfo = new GroupTemplateShrotInfo
                {
                    ReceiverAccount = BudgetPaymentOrder.ReceiverAccount.AccountNumber,
                    ReceiverName = BudgetPaymentOrder.Receiver,
                    DebitAccount = BudgetPaymentOrder.DebitAccount.AccountNumber,
                    FeeAccount = BudgetPaymentOrder.FeeAccount?.AccountNumber,
                    Amount = BudgetPaymentOrder.Amount,
                    GroupId = TemplateGroupId
                };
                this.GroupTemplateShrotInfo = shortinfo;
            }


            //Եթե փոխանցում է ՀՀ տարածքում,որոշում ենք փոխանցման ենթատեսակը
            if ((this.BudgetPaymentOrder.Type == OrderType.RATransfer || this.BudgetPaymentOrder.Type == OrderType.CashForRATransfer) && this.BudgetPaymentOrder.SubType != 3 && this.BudgetPaymentOrder.ReceiverAccount != null && this.BudgetPaymentOrder.ReceiverAccount.AccountNumber != "0")
            {
                if (this.BudgetPaymentOrder.ReceiverAccount.AccountNumber.Length > 5)
                {
                    ulong creditBankCode = ulong.Parse(this.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString().Substring(0, 5));

                    if (creditBankCode >= 22000 && creditBankCode < 22300)
                        this.BudgetPaymentOrder.SubType = 1; // Փոխանցում բանկի ներսում
                    else if ((this.BudgetPaymentOrder.ReceiverBankCode == 10300 && this.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString()[5] == '9') || this.BudgetPaymentOrder.ReceiverBankCode.ToString()[0] == '9')
                    {
                        this.BudgetPaymentOrder.SubType = 5; // Փոխանցում բյուջե


                        if (this.BudgetPaymentOrder.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.BudgetPaymentOrder.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1)
                        {
                            this.BudgetPaymentOrder.SubType = 6;
                        }
                    }
                    else if (this.BudgetPaymentOrder.ReceiverBankCode == 10300 && this.BudgetPaymentOrder.ReceiverAccount.AccountNumber.ToString()[0] == '9' && this.BudgetPaymentOrder.Source == SourceType.MobileBanking)
                    {
                        this.BudgetPaymentOrder.SubType = 5; // Փոխանցում բյուջե


                        if (this.BudgetPaymentOrder.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.BudgetPaymentOrder.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1)
                        {
                            this.BudgetPaymentOrder.SubType = 6;
                        }
                    }

                    else
                        this.BudgetPaymentOrder.SubType = 2; // Փոխանցում ՀՀ տարածքում

                }

            }


            if (this.BudgetPaymentOrder.Source != SourceType.Bank || (this.BudgetPaymentOrder.Source == SourceType.Bank && this.BudgetPaymentOrder.OPPerson == null))
            {
                this.BudgetPaymentOrder.OPPerson = Order.SetOrderOPPerson(this.BudgetPaymentOrder.CustomerNumber);
            }


            if (this.BudgetPaymentOrder.Type == OrderType.RATransfer && this.BudgetPaymentOrder.SubType == 3 && this.BudgetPaymentOrder.Source == SourceType.MobileBanking)
            {
                this.BudgetPaymentOrder.ReceiverAccount = Account.GetAccount(this.BudgetPaymentOrder.ReceiverAccount.AccountNumber);
            }

        }

        /// <summary>
        /// Վերադարձնում է բյուջե փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="paymentOrderTemplateId"></param>
        /// <returns></returns>
        public static BudgetPaymentOrderTemplate Get(int id, ulong customerNumber)
        {
            BudgetPaymentOrderTemplate paymentOrderTemplate = BudgetPaymentOrderTemplateDB.GetBudgetPaymentOrderTemplate(id, customerNumber);

            paymentOrderTemplate.BudgetPaymentOrder.Fees = GetTemplateFees(id);

            if (paymentOrderTemplate.BudgetPaymentOrder.Fees.Exists(m => m.Type == 7))
            {
                paymentOrderTemplate.BudgetPaymentOrder.CardFee = paymentOrderTemplate.BudgetPaymentOrder.Fees.Find(m => m.Type == 7).Amount;
                paymentOrderTemplate.BudgetPaymentOrder.Currency = paymentOrderTemplate.BudgetPaymentOrder.Fees.Find(m => m.Type == 7).Currency;
                paymentOrderTemplate.BudgetPaymentOrder.Fees.Find(m => m.Type == 7).Account = paymentOrderTemplate.BudgetPaymentOrder.DebitAccount;
            }
            if (paymentOrderTemplate.BudgetPaymentOrder.Fees.Exists(m => m.Type == 20))
            {
                paymentOrderTemplate.BudgetPaymentOrder.TransferFee = paymentOrderTemplate.BudgetPaymentOrder.Fees.Find(m => m.Type == 20).Amount;
                paymentOrderTemplate.BudgetPaymentOrder.FeeAccount = paymentOrderTemplate.BudgetPaymentOrder.Fees.Find(m => m.Type == 20).Account;
            }

            return paymentOrderTemplate;
        }
    }
}
