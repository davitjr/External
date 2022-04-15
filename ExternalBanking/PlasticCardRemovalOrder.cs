using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class PlasticCardRemovalOrder : Order
    {
        public Card Card { get; set; }
        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// Քարտի հեռացման պատճառ
        /// </summary>
        public int RemovalReason { get; set; }
        /// <summary>
        /// Քարտի հեռացման պատճառի նկարագրություն
        /// </summary>
        public string RemovalReasonDescription { get; set; }
        /// <summary>
        /// Քարտի հեռացման "այլ" տարբերակի նկարագրություն
        /// </summary>
        public string RemovalReasonInfo { get; set; }

        public void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.PlasticCardRemovalOrder;
            this.SubType = 1;

            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source == SourceType.Bank)
            {
                this.UserId = user.userID;
            }
        }

        public ActionResult SaveAndApprove(ACBAServiceReference.User user, SourceType source, ulong customerNumber, short schemaType)
        {
            this.Complete();

            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PlasticCardRemovalOrderDB.Save(this, user, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                LogOrderChange(user, action);

                ActionResult res = base.Approve(schemaType, user.userName);

                if (res.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    this.SubType = 1;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);

                }
                else
                {
                    return res;
                }
                scope.Complete();
            }

            result = base.Confirm(user);

            return result;
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            result.Errors.AddRange(Validation.ValidateCardRemovalOrder(this, user));
            return result;
        }

        public PlasticCardRemovalOrder Get()
        {
            return PlasticCardRemovalOrderDB.GetPlasticCardRemovalOrder(this);
        }

        /// <summary>
        /// Ստուգում ենք ARCA ֆայլեր ուղարկվել են թե ոչ
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        public static PlasticCardSentToArcaStatus PlasticCardSentToArca(long productId, CardChangeType cardChangeType)
        {
            return PlasticCardRemovalOrderDB.PlasticCardSentToArca(productId, cardChangeType);
        }

        /// <summary>
        /// Ստուգում ենք Արքա ուղղարկման ֆայլի ձևավորման պրոցեսը սկսել է թե ոչ
        /// </summary>
        /// <returns></returns>
        public static bool CheckForArcaFileGenerationProcess()
        {
            return PlasticCardRemovalOrderDB.CheckForArcaFileGenerationProcess();
        }

        public static List<string> CheckPlasticCardRemovalOrder(PlasticCardRemovalOrder order, bool fromCardDepartment)
        {
            List<string> messages = new List<string>();
            order.Card.CardChangeType = (CardChangeType)Card.GetCardChangeType(order.Card.ProductId);
            List<Card> supplementaryCards = new List<Card>();
            supplementaryCards.AddRange(CardDB.GetLinkedCards(order.Card.CardNumber));
            if (supplementaryCards.Count > 0 && (order.Card.CardChangeType == CardChangeType.RenewWithSameType || order.Card.CardChangeType == CardChangeType.RenewWithNewType))
            {
                messages.Add("Նշված քարտի համար առկա են կից/լրացուցիչ քարտեր: Շարունակե՞լ ");
            }

            if (fromCardDepartment && (order.Card.CardChangeType == CardChangeType.RenewWithSameType || order.Card.CardChangeType == CardChangeType.RenewWithNewType))
            {
                if (PlasticCardSentToArca(order.Card.ProductId, order.Card.CardChangeType) == PlasticCardSentToArcaStatus.SentToArca)
                {
                    messages.Add("Քարտի վերաթողարկման ֆայլն ուղարկված է ԱրՔա։ Շարունակե՞լ");
                }
            }
            if (order.Card.CardChangeType == CardChangeType.New)
            {
                messages.Add("Քարտի հեռացման հետ մեկտեղ հեռացվելու են նաև քարտին կցված հաշիվները: Շարունակե՞լ");
            }

            return messages;
        }
    }
}
