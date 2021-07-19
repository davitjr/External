using ExternalBanking.DBManager;
using ExternalBanking.DBManager.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class CurrencyExchangeOrderTemplate : Template
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int CurrencyExchangeOrderTemplateId { get; set; }

        /// <summary>
        /// Փոխարկում
        /// </summary>
        public CurrencyExchangeOrder CurrencyExchangeOrder { get; set; }

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

            this.CurrencyExchangeOrder.Complete();

            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.CurrencyExchangeOrder.Validate(user, this.TemplateType);
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
                        result = CurrencyExchangeOrderTemplateDB.CurrencyExchangeOrderTemplate(this, action);
                        CurrencyExchangeOrderTemplateDB.SaveCurrencyExchangeOrderTemplateDetails(this, userName, source);

                        if (result.Errors.Count == 0)
                        {
                            if (TemplateType == TemplateType.CreatedAsGroupService)
                                OrderGroupDB.SaveGroupTemplateShortInfo(GroupTemplateShrotInfo, result.Id, action);
                            ActionResult resultOrderFee = base.SaveTemplateFee(this.CurrencyExchangeOrder);
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
            this.CurrencyExchangeOrder.Type = this.TemplateDocumentType;
            if (CurrencyExchangeOrder.DebitAccount.Currency != "AMD")
            {
                if (CurrencyExchangeOrder.ReceiverAccount.Currency != "AMD")
                    CurrencyExchangeOrder.SubType = 3;//Արտարժույթի առք ու վաճառք  
                else
                    CurrencyExchangeOrder.SubType = 1; //Արտարժույթի վաճառք    
            }
            else
                CurrencyExchangeOrder.SubType = 2; //Արտարժույթի առք

            if (TemplateType == TemplateType.CreatedAsGroupService)
            {
                GroupTemplateShrotInfo shortinfo = new GroupTemplateShrotInfo
                {
                    ReceiverAccount = CurrencyExchangeOrder.ReceiverAccount.AccountNumber,
                    ReceiverName = CurrencyExchangeOrder.Receiver,
                    DebitAccount = CurrencyExchangeOrder.DebitAccount.AccountNumber,
                    FeeAccount = CurrencyExchangeOrder.FeeAccount?.AccountNumber,
                    Amount = CurrencyExchangeOrder.Amount,
                    GroupId = TemplateGroupId
                };
                this.GroupTemplateShrotInfo = shortinfo;
            }
        }

    }
}
