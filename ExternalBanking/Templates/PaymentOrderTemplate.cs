using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// ՀՀ տարածքում/հաշիվների միջև փոխանցման ձևանմուշ
    /// </summary>
    public class PaymentOrderTemplate : Template
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int PaymentOrderTemplateId { get; set; }

        /// <summary>
        /// Փոխանցում
        /// </summary>
        public PaymentOrder PaymentOrder { get; set; }

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
            this.PaymentOrder.Complete();
            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.PaymentOrder.Validate(user, this.TemplateType);
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
                        result = PaymentOrderTemplateDB.SavePaymentOrderTemplate(this, action);

                        if(result.Errors.Count == 0)
                        {
                            if (TemplateType == TemplateType.CreatedAsGroupService)
                                OrderGroupDB.SaveGroupTemplateShortInfo(GroupTemplateShrotInfo,result.Id,action);
                            ActionResult resultOrderFee = base.SaveTemplateFee(this.PaymentOrder);
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
            this.PaymentOrder.Type = this.TemplateDocumentType;
            this.PaymentOrder.SubType = this.TemplateDocumentSubType;

            if(TemplateType == TemplateType.CreatedAsGroupService)
            {
                GroupTemplateShrotInfo shortinfo = new GroupTemplateShrotInfo
                {
                    ReceiverAccount = PaymentOrder.ReceiverAccount.AccountNumber,
                    ReceiverName = PaymentOrder.Receiver,
                    DebitAccount = PaymentOrder.DebitAccount.AccountNumber,
                    FeeAccount = PaymentOrder.FeeAccount?.AccountNumber,
                    Amount = PaymentOrder.Amount,
                    GroupId = TemplateGroupId
                };
                this.GroupTemplateShrotInfo = shortinfo;
            }
        }

        /// <summary>
        /// Վերադարձնում է Փոխանցում ՀՀ տարածքում/Փոխանցում հաշիվների միջև ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static PaymentOrderTemplate Get(int templateId, ulong customerNumber)
        {
            PaymentOrderTemplate paymentOrderTemplate = PaymentOrderTemplateDB.GetPaymentOrderTemplate(templateId, customerNumber);

            paymentOrderTemplate.PaymentOrder.Fees = GetTemplateFees(templateId);

            if (paymentOrderTemplate.PaymentOrder.Fees.Exists(m => m.Type == 7))
            {
                paymentOrderTemplate.PaymentOrder.CardFee = paymentOrderTemplate.PaymentOrder.Fees.Find(m => m.Type == 7).Amount;
                paymentOrderTemplate.PaymentOrder.Currency = paymentOrderTemplate.PaymentOrder.Fees.Find(m => m.Type == 7).Currency;
                paymentOrderTemplate.PaymentOrder.Fees.Find(m => m.Type == 7).Account = paymentOrderTemplate.PaymentOrder.DebitAccount;
            }
            if (paymentOrderTemplate.PaymentOrder.Fees.Exists(m => m.Type == 20))
            {
                paymentOrderTemplate.PaymentOrder.TransferFee = paymentOrderTemplate.PaymentOrder.Fees.Find(m => m.Type == 20).Amount;
                paymentOrderTemplate.PaymentOrder.FeeAccount = paymentOrderTemplate.PaymentOrder.Fees.Find(m => m.Type == 20).Account;
            }

            return paymentOrderTemplate;
        }

    }
}
