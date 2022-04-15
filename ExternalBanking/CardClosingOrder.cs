using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class CardClosingOrder : Order
    {
        /// <summary>
        /// Փակվող քարտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Քարտի փակման պատճառ
        /// </summary>
        public short ClosingReason { get; set; }

        /// <summary>
        /// Քարտի փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReasonDescription { get; set; }

        /// <summary>
        /// Քարտի փակման լրացուցիչ պատճառ
        /// </summary>
        public string ClosingReasonAdd { get; set; }

        /// <summary>
        /// Քարտի քարտային հաշվի փակում
        /// </summary>
        public bool CloseCardAccount { get; set; }

        /// <summary>
        /// հիմնական քարտի ունիկալ համար
        /// </summary>
        public ulong MainCardProductID { get; set; }

        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete(source);
            ActionResult result = this.Validate(user.filialCode);


            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                OrderGroup orderGroup = new OrderGroup();
                orderGroup.GroupName = "Card closing and card blocking orders";
                orderGroup.Type = OrderGroupType.CreatedAutomatically;
                orderGroup.Status = OrderGroupStatus.Active;
                ActionResult actionResult = orderGroup.Save();
                if (actionResult.ResultCode == ResultCode.Normal)
                {
                    this.GroupId = orderGroup.ID;
                }
                result = CardClosingOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }
        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ushort filialCode)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateCardClosingOrderDocument(this, filialCode));
            return result;
        }
        /// <summary>
        /// Վերադարձնում է քարտի փակման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            CardClosingOrderDB.GetCardClosingOrder(this);
            this.MainCardProductID = Card.GetMainCardProductId(Card.GetCardNumber(Convert.ToInt64(this.ProductId)));
        }
        /// <summary>
        /// Քարտի փակման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user, string clientIp)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = base.Approve(schemaType, userName);
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
                        {
                            user.userName = userName;
                            if (Order.GetOrderQualityByDocID(this.Id) != OrderQuality.Approved)
                                result = RedirectToDocFlow(schemaType, clientIp);
                            this.Quality = OrderQuality.Sent3;
                        }
                        else
                        {
                            base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                            base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        }
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
        /// Քարտի փակման հաստատաման ստուգումներ
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
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);


        }
        /// <summary>
        ///Քարտի փակման հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete(source);
            ActionResult result = this.Validate(user.filialCode);

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

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardClosingOrderDB.Save(this, userName, source);

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

            if (this.Source != SourceType.AcbaOnline && this.Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }


            return result;
        }

        /// <summary>
        /// Քարտի փակման պատճառի ստուգում
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static List<ActionError> CheckCardClosingReason(ulong productId, int reason)
        {
            return CardClosingOrderDB.CheckCardClosingReason(productId, reason);
        }

        /// <summary>
        /// Քարտի մասնակցության ստուգում պարբերական փոխանցման մեջ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static List<ActionError> CheckCardPeriodicTransfer(string accountNumber)
        {
            return CardClosingOrderDB.CheckCardPeriodicTransfer(accountNumber);
        }

        /// <summary>
        /// Քարտի չձևակերպված գործարքների առկայության ստուգում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public static List<ActionError> CheckCardTransactions(string cardNumber, short cardType)
        {
            return CardClosingOrderDB.CheckCardTransactions(cardNumber, cardType);
        }

        /// <summary>
        /// Քարտի փակման վերաբերյալ զգուշացումների վերադարձ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<string> GetCardClosingWarnings(ulong productId, ulong customerNumber, Culture culture)
        {
            Card card = Card.GetCard(productId, customerNumber);
            List<string> warnings = new List<string>();

            if (card.MainCardNumber != "")
            {
                //CashBack NFC                     
                if (card.Type == 23 || card.Type == 34 || card.Type == 40 || card.Type == 50)
                {
                    double cashBackAmount = 0;
                    Card mainCard = Card.GetCard(card.MainCardNumber);                   //CashBack NFC               
                    if (mainCard.Type == 23 || mainCard.Type == 34 || card.Type == 40 || card.Type == 50)
                    {
                        cashBackAmount = 0;
                    }
                    else
                    {
                        cashBackAmount = Card.GetCashBackAmount((ulong)mainCard.ProductId);
                    }
                    if (cashBackAmount != 0)
                    {
                        warnings.Add(Info.GetTerm(537, new string[] { cashBackAmount.ToString("#,0.00") + "AMD" }, culture.Language));
                    }
                }
            }
            if (card.SMSApplicationPresent)
            {
                warnings.Add(Info.GetTerm(538, new string[] { }, culture.Language));
            }

            double serviceFee = Card.GetCardTotalDebt(card.CardNumber);
            if (serviceFee != 0)
            {
                warnings.Add(Info.GetTerm(539, new string[] { serviceFee.ToString("#,0.00") + "AMD" }, culture.Language));
            }
            //CashBack NFC  
            if (card.MainCardNumber == "" && (card.Type == 23 || card.Type == 34 || card.Type == 40 || card.Type == 50))
            {
                double cashBackAmount = Card.GetCashBackAmount((ulong)card.ProductId);
                if (cashBackAmount != 0)
                {
                    warnings.Add(Info.GetTerm(537, new string[] { cashBackAmount.ToString("#,0.00") + "AMD" }, culture.Language));
                }
            }
            return warnings;
        }

        /// <summary>
        /// Տվյալ քարտի փակման հայտի առկայության ստուգում
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool IsSecondClosing(ulong productId)
        {
            return CardClosingOrderDB.IsSecondClosing(productId);
        }
        /// <summary>
        /// Ստուգում է քարտի համար գոյություն ունեն կենսաթոշակային հայտարագիր
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static bool CheckCardPensionApplication(string cardNumber)
        {
            return CardClosingOrderDB.CheckCardPensionApplication(cardNumber);
        }


        private ActionResult RedirectToDocFlow(short schemaType, string clientIp)
        {
            return DocFlowManagement.DocFlow.SendCardClosingOrderToConfirm(this, user, schemaType, clientIp);
        }

    }
}
