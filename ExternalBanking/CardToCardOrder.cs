using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class CardToCardOrder : Order
    {
        /// <summary>
        /// Ելքագրվող քարտ 
        /// </summary>
        public Card DebitCard { get; set; }

        /// <summary>
        /// Ելքագրվող քարտի համար
        /// </summary>
        public string DebitCardNumber { get; set; }

        /// <summary>
        /// Մուտքագրվող քարտի համար
        /// </summary>
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Մուտքագրվող քարտի վրա գրված հաճախորդի անուն, ազգանուն
        /// </summary>
        public string EmbossingName { get; set; }

        /// <summary>
        /// Միջնորդավճարի գումար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Կրեդիտագրվող հաշիվը պատկանում է բանկին
        /// </summary>
        public bool IsOurCard { get; set; }

        /// <summary>
        /// Գործարքի ունիկալ համար։ Գեներացվում է Արքայում
        /// </summary>
        public string AuthId { get; set; }

        /// <summary>
        /// Հարցման համար։ Գեներացվում է Արքայում
        /// </summary>
        public string RRN { get; set; }

        /// <summary>
        /// Սեփական քարտերի միջև
        /// </summary>
        public bool IsBetweenOwnCards { get; set; }

        /// <summary>
        /// Կցված քարտ
        /// </summary>
        public bool IsAttachedCard { get; set; }
        public string BindingId { get; set; }
        /// <summary>
        /// մուտքագրվող քարտ
        /// </summary>
        public Card ReceiverCard { get; set; }

        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.Type = OrderType.CardToCardOrder;
            this.SubType = 1;

            this.DebitCard = Card.GetCard(this.DebitCardNumber, this.CustomerNumber);
            this.IsOurCard = Card.IsOurCard(this.CreditCardNumber);
            this.FeeAmount = Card.GetCardToCardTransferFee(this.DebitCard.CardNumber, this.CreditCardNumber, this.Amount, this.DebitCard.Currency);
        }

        public ActionResult Validate(TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardToCardOrder(this, templateType));
            return result;
        }
        public ActionResult ValidateAttachedCardOrder(TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateAttachedCardToCardOrder(this, templateType));
            return result;
        }
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                //Հայտի ձևակերպում
                result = CardToCardOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                LogOrderChange(user, action);

                ActionResult res = base.Approve(schemaType, userName);

                if (res.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
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

            C2CTransferOrder c2CTransferOrder = new C2CTransferOrder();
            c2CTransferOrder.SourceCard = new CardIdentificationData();
            c2CTransferOrder.SourceCard.CardNumber = this.DebitCard.CardNumber;
            c2CTransferOrder.SourceCard.ExpiryDate = this.DebitCard.ValidationDate.ToString("yyyyMM");


            c2CTransferOrder.DestinationCardNumber = this.CreditCardNumber;
            c2CTransferOrder.Amount = (decimal)this.Amount;
            c2CTransferOrder.Currency = this.DebitCard.Currency;
            c2CTransferOrder.OrderID = this.Id;

            //Նշանակում է փոխանցումը կատարվում է CardToCardOrder-ի միջոցով։
            c2CTransferOrder.SourceID = 0;

            c2CTransferOrder.Receiver = this.EmbossingName;
            C2CTransferResult transactionResult = c2CTransferOrder.Save();

            if (transactionResult.ResultCode == 0)
            {
                result.ResultCode = ResultCode.Normal;
                ActionResult res = base.Confirm(user);
                if (res.ResultCode != ResultCode.Normal)
                {
                    return res;
                }
            }
            else
            {
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                base.SetQualityHistoryUserId(this.Quality, user.userID);

                result.ResultCode = ResultCode.Failed;

                //Սխալի նկարագրություն
                //result.Errors.Add(new ActionError());
                //result.Errors[0].Description = transactionResult.ResponseCodeDescription;
            }
            return result;
        }

        public CardToCardOrder Get()
        {
            CardToCardOrder order;
            if (GetOrder(Id, CustomerNumber).Type == OrderType.AttachedCardToCardOrder)
            {
                order = CardToCardOrderDB.GetAttachedCardOrder(this);
                if (ValidationDB.IsExistAttachedCard(CustomerNumber, order.CreditCardNumber))
                {
                    order.IsBetweenOwnCards = true;
                }
                else
                {
                    order.IsOurCard = Card.IsOurCard(order.CreditCardNumber);
                    if (order.IsOurCard)
                    {
                        order.IsBetweenOwnCards = Utility.ValidateCardNumber(this.CustomerNumber, order.CreditCardNumber);
                        order.ReceiverCard = Card.GetCard(order.CreditCardNumber);
                    }
                }
            }
            else
            {
                order = CardToCardOrderDB.Get(this);
                order.IsOurCard = Card.IsOurCard(order.CreditCardNumber);
                if (order.IsOurCard)
                {
                    order.IsBetweenOwnCards = Utility.ValidateCardNumber(this.CustomerNumber, order.CreditCardNumber);
                    order.ReceiverCard = Card.GetCard(order.CreditCardNumber);
                }
                order.DebitCard = Card.GetCard(order.DebitCardNumber);
            }
            return order;
        }

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                //Հայտի ձևակերպում
                result = CardToCardOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                LogOrderChange(user, action);

                scope.Complete();
            }
            return result;
        }


        public ActionResult Approve(string userName, short schemaType)
        {
            this.Complete();

            ActionResult resultApprove = base.Approve(schemaType, userName);

            if (resultApprove.ResultCode == ResultCode.Normal)
            {
                this.Quality = OrderQuality.Sent3;
                base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                LogOrderChange(user, Action.Update);
            }
            else
            {
                return resultApprove;
            }

            ActionResult result = new ActionResult();
            C2CTransferOrder c2CTransferOrder = new C2CTransferOrder();
            c2CTransferOrder.SourceCard = new CardIdentificationData();
            try
            {
                c2CTransferOrder.SourceCard.CardNumber = this.DebitCard.CardNumber;
                c2CTransferOrder.SourceCard.ExpiryDate = this.DebitCard.ValidationDate.ToString("yyyyMM");

                c2CTransferOrder.DestinationCardNumber = this.CreditCardNumber;
                c2CTransferOrder.Amount = (decimal)this.Amount;
                c2CTransferOrder.Currency = this.DebitCard.Currency;
                c2CTransferOrder.OrderID = this.Id;

                //Նշանակում է փոխանցումը կատարվում է CardToCardOrder-ի միջոցով։
                c2CTransferOrder.SourceID = 0;

                c2CTransferOrder.Receiver = this.EmbossingName;
                C2CTransferResult transactionResult = c2CTransferOrder.Save();

                if (transactionResult.ResultCode == 0)
                {
                    result.ResultCode = ResultCode.Normal;
                    ActionResult res = OrderDB.UpdateHBdocumentQuality(this.Id, user);
                    if (res.ResultCode != ResultCode.Normal)
                    {
                        return res;
                    }
                }
                else
                {
                    this.Quality = OrderQuality.Declined;
                    base.UpdateQuality(this.Quality);
                    base.SetQualityHistoryUserId(this.Quality, user.userID);

                    result.ResultCode = ResultCode.Failed;
                    result.Errors.Add(new ActionError(1691));
                    //Սխալի նկարագրություն
                    //result.Errors.Add(new ActionError());
                    //result.Errors[0].Description = transactionResult.ResponseCodeDescription;
                }
            }
            catch (Exception e)
            {
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(1690));
                return result;
            }
            return result;
        }
        public ActionResult ToCardWithECommerce()
        {
            //jamanakavor minchev apigatei harc@ lucvi , ogtagorcel Ecommerce
            ActionResult result = new ActionResult();
            C2CTransferOrder c2CTransferOrder = new C2CTransferOrder();
            c2CTransferOrder.SourceCard = new CardIdentificationData();
            try
            {
                c2CTransferOrder.SourceCard.CardNumber = this.DebitCard.CardNumber;
                c2CTransferOrder.SourceCard.ExpiryDate = this.DebitCard.ValidationDate.ToString("yyyyMM");

                c2CTransferOrder.DestinationCardNumber = this.CreditCardNumber;
                c2CTransferOrder.Amount = (decimal)this.Amount;
                c2CTransferOrder.Currency = this.DebitCard.Currency;
                c2CTransferOrder.OrderID = this.Id;

                //Նշանակում է փոխանցումը կատարվում է CardToCardOrder-ի միջոցով։
                c2CTransferOrder.SourceID = 0;

                c2CTransferOrder.Receiver = this.EmbossingName;
                C2CTransferResult transactionResult = c2CTransferOrder.Save();

                if (transactionResult.ResultCode == 0)
                {
                    result.ResultCode = ResultCode.Normal;
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    result.Errors.Add(new ActionError(1691));
                }
            }
            catch (Exception)
            {
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(1690));
                return result;
            }
            return result;
        }


        public ActionResult SaveAttachedCardtoCardOrder(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.RegistrationDate = DateTime.Now.Date;

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.Type = OrderType.AttachedCardToCardOrder;
            this.SubType = 1;

            ActionResult result = this.ValidateAttachedCardOrder();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                //Հայտի ձևակերպում
                result = CardToCardOrderDB.SaveAttachedCardtoCardOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                LogOrderChange(user, action);

                scope.Complete();
            }
            return result;
        }

        public ActionResult DeclineAttachedCardToCardOrderQuality()
        {
            UpdateQuality(OrderQuality.Declined);
            return SetQualityHistoryUserId(OrderQuality.Declined, user.userID);
        }
        public ActionResult ApproveAttachedCardToCardOrder()
        {
            ActionResult result = new ActionResult();
            try
            {
                CreditCardEcommerceOrderData ecommerce = new CreditCardEcommerceOrderData
                {
                    ExtId = Utility.GetLastKeyNumber(32, 22000),
                    Amount = Convert.ToDecimal(Amount),
                    Currency = Convert.ToInt32(Utility.GetCurrencyCode(Currency)),
                    MerchanId = ConfigurationManager.AppSettings["MerchantId"],
                    TerminalId = ConfigurationManager.AppSettings["TerminalId"],
                    DestinationCardIdentification = new CardIdentification
                    {                      
                        CardNumber = CreditCardNumber
                    },
                    Sender = new PersonMoneySendType(),
                    Recepient = new PersonMoneySendType
                    {
                        Name = EmbossingName
                    }
                };
                var response = ArcaDataService.CreditCardEcommerce(ecommerce);
                SaveAttachedCardToCardArcaResponseData(response, ecommerce.ExtId);
                if (response.ResponseCode == "00")
                {
                    result.ResultCode = ResultCode.Normal;
                    OrderDB.UpdateQuality(Id, OrderQuality.Completed);
                    SetQualityHistoryUserId(OrderQuality.Completed, user.userID);
                    return result;
                }
                else
                {
                    OrderDB.UpdateQuality(Id, OrderQuality.Declined);
                    SetQualityHistoryUserId(OrderQuality.Declined, user.userID);
                    result.ResultCode = ResultCode.Failed;
                }
            }
            catch (Exception)
            {
                OrderDB.UpdateQuality(Id, OrderQuality.Declined);
                SetQualityHistoryUserId(OrderQuality.Declined, user.userID);
                result.ResultCode = ResultCode.Failed;
            }
            return result;
        }
        private ActionResult SaveAttachedCardToCardArcaResponseData(CreditCardEcommerceResponse response, ulong ArcaExtensionID)
        {
            return CardToCardOrderDB.SaveAttachedCardToCardArcaResponseData((ulong)Id, ArcaExtensionID, response);
        }
    }
}
