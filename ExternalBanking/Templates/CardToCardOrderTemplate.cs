using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտից քարտ փոխանցման ձևանմուշ
    /// </summary>
    public class CardToCardOrderTemplate : Template
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int CardToCardOrderTemplateId { get; set; }

        /// <summary>
        /// Փոխանցում
        /// </summary>
        public CardToCardOrder CardToCardOrder { get; set; }


        
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
            result = base.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result = this.CardToCardOrder.Validate(this.TemplateType);
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
                        result = CardToCardOrderTemplateDB.SaveCardToCardOrderTemplate(this, action);

                        scope.Complete();
                    }
                }
            }

            Localization.SetCulture(result, new Culture(Languages.hy));
            return result;
        }


        private void Complete(SourceType source)
        {
            this.TemplateSourceType = source;
            this.CardToCardOrder.Type = this.TemplateDocumentType;
            this.CardToCardOrder.SubType = this.TemplateDocumentSubType;

            this.Status = TemplateStatus.Active;

            this.CardToCardOrder.DebitCard = Card.GetCard(this.CardToCardOrder.DebitCardNumber, this.TemplateCustomerNumber);
            this.CardToCardOrder.IsOurCard = Card.IsOurCard(this.CardToCardOrder.CreditCardNumber);

        }

        /// <summary>
        /// Վերադարձնում է քարտից քարտ փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static CardToCardOrderTemplate Get(int templateId, ulong customerNumber)
        {
            var template =  CardToCardOrderTemplateDB.GetCardToCardOrderTemplate(templateId, customerNumber);
            template.CardToCardOrder.IsOurCard = Card.IsOurCard(template.CardToCardOrder.CreditCardNumber);
            if (template.CardToCardOrder.IsOurCard)
            {
                template.CardToCardOrder.IsBetweenOwnCards = Utility.ValidateCardNumber(template.TemplateCustomerNumber, template.CardToCardOrder.CreditCardNumber);
                template.CardToCardOrder.ReceiverCard = Card.GetCard(template.CardToCardOrder.CreditCardNumber);
            }
            template.CardToCardOrder.DebitCard = Card.GetCard(template.CardToCardOrder.DebitCardNumber);
            return template;
        }
    }
}
