using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    public class LoanMatureOrderTemplate : Template
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int LoanMatureOrderTemplateId { get; set; }

        /// <summary>
        /// Փոխանցում
        /// </summary>
        public MatureOrder MatureOrder { get; set; }

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
            this.MatureOrder.Complete(source);
            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.MatureOrder.Validate();
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
                        result = LoanMatureOrderTemplateDB.SaveLoanMatureOrderTemplate(this, action);
                        if(result.ResultCode == ResultCode.Normal && TemplateType == TemplateType.CreatedAsGroupService)
                            OrderGroupDB.SaveGroupTemplateShortInfo(GroupTemplateShrotInfo,result.Id,action);
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
            this.MatureOrder.Type = this.TemplateDocumentType;
            this.MatureOrder.SubType = (byte)MatureType.PartialRepayment;


            if (this.MatureOrder.IsProblematic)
            {
                if (source != SourceType.SSTerminal)
                {
                    if (this.MatureOrder.MatureType == MatureType.ClaimRepayment)
                    {
                        this.MatureOrder.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this.MatureOrder, OrderAccountType.DebitAccount), this.MatureOrder.ProductCurrency, MatureOrder.user.filialCode);
                    }
                    else
                    {
                        this.MatureOrder.Account = Account.GetProductAccount(this.MatureOrder.ProductId, 18, 224);

                        //this.MatureOrder.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.MatureOrder.ProductCurrency, user.filialCode);
                        if (this.MatureOrder.ProductCurrency != "AMD")
                        {
                            //this.MatureOrder.PercentAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                            this.MatureOrder.PercentAccount = Account.GetProductAccount(this.MatureOrder.ProductId, 18, 279);
                        }
                    }
                }
            }
            else if (this.MatureOrder.ProductType == ProductType.PaidFactoring && this.MatureOrder.ProductCurrency != "AMD"
                && (this.MatureOrder.PercentAccount == null || string.IsNullOrEmpty(this.MatureOrder.PercentAccount.AccountNumber) || this.MatureOrder.PercentAccount.AccountNumber == "0"))
            {
                this.MatureOrder.PercentAccount = Account.GetProductAccount(this.MatureOrder.ProductId, 18, 179);
                this.MatureOrder.PercentAmount = 0;
            }


            if (this.MatureOrder.Source != SourceType.Bank)
                this.MatureOrder.RepaymentSourceType = 1;//Սեփական միջոցների հաշվին

            if (TemplateType == TemplateType.CreatedAsGroupService)
            {
                GroupTemplateShrotInfo shortinfo = new GroupTemplateShrotInfo
                {
                    DebitAccount = MatureOrder.Account.AccountNumber,
                    Amount = MatureOrder.Amount,
                    GroupId = TemplateGroupId,
                    LoanAppId = MatureOrder.ProductId,
                    PercentAccount = MatureOrder?.PercentAccount?.AccountNumber
                };
                this.GroupTemplateShrotInfo = shortinfo;
            }


        }

        /// <summary>
        /// Վերադարձնում է վարկի մարման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="paymentOrderTemplateId"></param>
        /// <returns></returns>
        public static LoanMatureOrderTemplate Get(int id, ulong customerNumber)
        {
            var matureOrderTemplate = LoanMatureOrderTemplateDB.GetLoanMatureOrderTemplate(id, customerNumber);
            matureOrderTemplate = Complete(matureOrderTemplate);
            return matureOrderTemplate;
        }

        private static LoanMatureOrderTemplate Complete(LoanMatureOrderTemplate matureOrderTemplate)
        {
            matureOrderTemplate.MatureOrder.SubType = 2;
            matureOrderTemplate.TemplateDocumentSubType = matureOrderTemplate.MatureOrder.SubType;
            if (matureOrderTemplate.MatureOrder.MatureType == MatureType.PartialRepayment)
            {
                matureOrderTemplate.MatureOrder.MatureType = MatureType.PartialRepaymentByGrafik;
            }
            else if (matureOrderTemplate.MatureOrder.MatureType == MatureType.RepaymentByCreditCode)
            {
                matureOrderTemplate.MatureOrder.MatureType = MatureType.PartialRepayment;
            }
            else if (matureOrderTemplate.MatureOrder.MatureType == MatureType.FullRepayment && matureOrderTemplate.MatureOrder.Quality == OrderQuality.Draft)
            {
                matureOrderTemplate.MatureOrder.MatureType = MatureType.PartialRepayment;
            }
            return matureOrderTemplate;
        }
    }
}
