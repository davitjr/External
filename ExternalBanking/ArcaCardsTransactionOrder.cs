using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Globalization;
using System.Transactions;

namespace ExternalBanking
{
    public class ArcaCardsTransactionOrder : Order
    {

        /// <summary>
        /// Գործարքի քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Գործարքի քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Գործողության տեսակ
        /// 1 = Բլոկավորում
        /// 2 = Ապաբլոկավորում
        /// </summary>
        public short ActionType { get; set; }

        /// <summary>
        /// Գործողության տեսակի նկարագրություն
        /// </summary>
        public string ActionTypeDescription { get; set; }

        /// <summary>
        /// Բլոկավորման/ապաբլոկավորման պատճառ
        /// </summary>
        public byte ActionReasonId { get; set; }

        /// <summary>
        /// Բլոկավորման/ապաբլոկավորման պատճառ
        /// </summary>
        public string ActionReasonDescription { get; set; }
        /// <summary>
        /// Գործարքի տեսակ
        /// </summary>
        public int? HotCardStatus { get; set; }

        /// <summary>
        /// Գործարքը կատարողի IP հասցե
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Բանկի կողմից գործարք կատարող անձի համար
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Գործողությունը կատարելու հիմնավորում
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Մերժման պատճառ
        /// </summary>
        public string RejectReasonDescription { get; set; }

        /// <summary>
        /// հիմնական քարտի ունիկալ համար
        /// </summary>
        public ulong MainCardProductID { get; set; }

        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.Card = Card.GetCard(this.CardNumber);
            if (this.Card == null)
            {
                var plasticCard = PlasticCard.GetPlasticCard(this.CardNumber);
                this.Card = new Card();
                this.Card.CardNumber = plasticCard.CardNumber;
                this.Card.ValidationDate = DateTime.ParseExact(plasticCard.ExpiryDate, "MMyyyy", CultureInfo.CurrentCulture).AddMonths(1).AddDays(-1);
            }

            if (this.Source == SourceType.Bank)
            {
                this.UserId = user.userID;
            }

            this.Type = OrderType.ArcaCardsTransactionOrder;
            this.SubType = (byte)this.ActionType;

            //HotCardStatus-ը դեպի ApiGate փոխանցվող պարամետր է, որը արժեք է ստանում կախված user-ի ընտրած գործողությունից և գործողության պատճառից։
            this.SetHotCardStatus();
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
                result = ArcaCardsTransactionOrderDB.Save(this, userName, source);

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
                result = ArcaCardsTransactionOrderDB.Save(this, userName, source);

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
                //Քարտի կարգավիճակի փոփոխություն միայն բանկի բազայում պատճառով գործողություն կարող է իրականացնել միայն քարտային կենտրոնը
                //Տվյալ դեպքում ձևակերպումը լինում է առանց ԱրՔա դիմելու
                if (this.ActionReasonId == 11)
                {
                    result = base.Confirm(user);
                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }


                scope.Complete();
            }
            if (this.ActionType == 2 && this.ActionReasonId == 11)
            {

                return result;
            }
            if (result.ResultCode == ResultCode.Normal)
            {
                var arcaCardsTransactionResponse = new ArcaCardsTransactionResponse();
                try
                {
                    ArcaCardsTransactionOrderData data = new ArcaCardsTransactionOrderData();
                    data.DocId = result.Id;
                    data.CardNumber = this.Card.CardNumber;
                    data.HotCardStatus = (ArcaDataServiceReference.HotCardStatus)this.HotCardStatus;
                    data.IPAddress = this.IPAddress;
                    data.ExpDate = this.Card.ValidationDate.ToString("yyyyMM");

                    //Order approvment
                    arcaCardsTransactionResponse = ArcaDataService.MakeCardTransaction((CardTransactionType)this.ActionType, data);
                }
                catch (Exception e)
                {
                    this.SetRejectReason(this.Id, "Processing Error");
                    this.Quality = OrderQuality.Declined;
                    base.UpdateQuality(this.Quality);
                    throw e;
                }

                //Եթե գործողությունը կատարվել է Արքայում
                if (arcaCardsTransactionResponse.ResultCode == TransactionProcessingResultType.Normal)
                {
                    result.ResultCode = ResultCode.Normal;
                    ActionResult res = base.Confirm(user);
                    if (res.ResultCode != ResultCode.Normal)
                    {
                        return res;
                    }
                }
                //Եթե գործողությունը չի կատարվել Արքայում, սակայն սխալը պրոցեսային է, ոչ թե տեխնիկական
                //Նման տեսակի մերժումները պետք է երևան հայտի մեջ՝ որպես մերժման պատճառ։
                else
                {
                    this.Quality = OrderQuality.Declined;
                    base.UpdateQuality(this.Quality);
                    base.SetQualityHistoryUserId(this.Quality, user.userID);

                    this.SetRejectReason(this.Id, arcaCardsTransactionResponse.ResultCodeDescription);
                    result.Errors.Add(new ActionError());
                    result.Errors[0].Description = arcaCardsTransactionResponse.ResultCodeDescription;
                    result.ResultCode = ResultCode.Failed;
                }
            }
            return result;
        }

        public ActionResult Approve(string userName, short schemaType)
        {
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

            this.Complete();

            ActionResult result = new ActionResult();
            ArcaDataServiceReference.ArcaCardsTransactionResponse arcaCardsTransactionResponse = new ArcaCardsTransactionResponse();
            try
            {
                ArcaCardsTransactionOrderData data = new ArcaCardsTransactionOrderData();
                data.DocId = this.Id;
                data.CardNumber = this.CardNumber;
                data.HotCardStatus = (ArcaDataServiceReference.HotCardStatus)this.HotCardStatus;
                data.IPAddress = this.IPAddress;
                data.ExpDate = this.Card.ValidationDate.ToString("yyyyMM");

                //Order approvment
                arcaCardsTransactionResponse = ArcaDataService.MakeCardTransaction((CardTransactionType)this.ActionType, data);
            }
            catch (Exception e)
            {
                this.SetRejectReason(this.Id, "Processing Error");
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                throw e;
            }

            if (arcaCardsTransactionResponse.ResultCode == TransactionProcessingResultType.Normal)
            {

                result.ResultCode = ResultCode.Normal;
                ActionResult res = base.Confirm(user);
                if (res.ResultCode != ResultCode.Normal)
                {
                    return res;
                }
                //Ձեր դիմումը  ուղարկվել է Բանկ:  Քարտը ապաբլոկավորելու համար կարող եք զանգահարել 010 31 88 88 հեռախոսահամարով կամ մոտենալ մոտակա մասնաճյուղ:
                result.Errors.Add(new ActionError(1566));

            }
            else
            {
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                base.SetQualityHistoryUserId(this.Quality, user.userID);

                this.SetRejectReason(this.Id, arcaCardsTransactionResponse.ResultCodeDescription);
                //Քարտի բլոկավորումը հնարավոր չէ իրականացնել օնլայն եղանակով:  Բլոկավորելու համար անհրաժեշտ է զանգահարել 010 31 88 88 հեռախոսահամարով:
                result.Errors.Add(new ActionError(1567));
                //result.Errors[0].Description = arcaCardsTransactionResponse.ResultCodeDescription;
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;

        }
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateArcaCardsTransactionOrder(this));
            return result;
        }

        public ArcaCardsTransactionOrder Get()
        {
            var arcaCardTransactionOrder = ArcaCardsTransactionOrderDB.Get(this);
            arcaCardTransactionOrder.Card = Card.GetCard(this.CardNumber);
            if (arcaCardTransactionOrder.Card == null)
            {
                var plasticCard = PlasticCard.GetPlasticCard(this.CardNumber);
                arcaCardTransactionOrder.Card = new Card();
                arcaCardTransactionOrder.Card.CardNumber = plasticCard.CardNumber;
                arcaCardTransactionOrder.Card.ValidationDate = DateTime.ParseExact(plasticCard.ExpiryDate, "MMyyyy", CultureInfo.CurrentCulture).AddMonths(1).AddDays(-1);
                arcaCardTransactionOrder.Card.CreditLine = null;
                arcaCardTransactionOrder.Card.Overdraft = null;
            }
            arcaCardTransactionOrder.MainCardProductID = Card.GetMainCardProductId(this.CardNumber);
            return arcaCardTransactionOrder;
        }

        private void SetHotCardStatus()
        {
            ArcaCardsTransactionOrderDB.SetHotCardStatus(this);
        }

        /// <summary>
        /// Եթե ընտրված է ապաբլոկավորում և «Սխալ փոխանցում» կամ «Հաշվից ելք» պատճառներից որևիցե մեկը, 
        /// ապա կատարում է ստուգում արդյոք քարտը front-office ծրագրի միջոցով բլոկավորվել է համապատասխան պատճառներով, 
        /// և տվյալ հայտի վերջին գործարքն է։
        /// </summary>
        /// <returns></returns>
        public bool CheckTransactionAvailabilityDependsOnActionReason()
        {
            return ArcaCardsTransactionOrderDB.CheckTransactionAvailabilityDependsOnActionReason(this.FilialCode, this.Card.CardNumber);
        }

        private void SetRejectReason(long docID, string rejectReason)
        {
            ArcaCardsTransactionOrderDB.SetRejectReason(docID, rejectReason);
        }

        internal static short GetBlockingReasonForBlockedCard(string cardNumber)
        {
            return ArcaCardsTransactionOrderDB.GetBlockingReasonForBlockedCard(cardNumber);
        }

        internal static string GetPreviousBlockUnblockOrderComment(string cardNumber)
        {
            return ArcaCardsTransactionOrderDB.GetPreviousBlockUnblockOrderComment(cardNumber);
        }

        public static byte CardBlockingActionAvailability(string cardNumber)
        {

            Card card = Card.GetCardMainData(cardNumber);
            CardIdentification cardIdentification = new CardIdentification
            {
                CardNumber = card.CardNumber,
                ExpiryDate = card.ValidationDate.ToString("yyyyMM")
            };
            var arcaResult = ArcaDataService.GetCardData(cardIdentification);

            if (arcaResult.cardDataField.hotCardStatusField != 0) //HotCardStatus.IsValid
            {
                return 2; //ապաբլոկավորել
            }
            return 1; //բլոկավորել
        }

        public static byte GetCardBlockingActionAvailabilityForFreezing(string cardNumber, int reasonId)
        {

            Card card = Card.GetCard(cardNumber);
            PlasticCard plasticCard = PlasticCard.GetPlasticCard(cardNumber);
            CardIdentification cardIdentification = new CardIdentification
            {
                CardNumber = card != null ? card.CardNumber : plasticCard.CardNumber,
                ExpiryDate = card != null ? card.ValidationDate.ToString("yyyyMM") : plasticCard.ExpiryDate.Substring(2, 4) + plasticCard.ExpiryDate.Substring(0, 2)
            };

            try
            {
                var arcaResult = ArcaDataService.GetCardData(cardIdentification);

                if (arcaResult.cardDataField.hotCardStatusField == 3) //HotCardStatus.DoNotHonor
                {
                    if (GetBlockingReasonForBlockedCard(cardNumber) != reasonId)
                    {
                        return 0;
                    }
                    return 2; //ապաբլոկավորել
                }
                else if (arcaResult.cardDataField.hotCardStatusField == 0) //HotCardStatus.ValidCard
                {
                    return 1; //բլոկավորել
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static long? GetPreviousBlockingOrderId(string cardNumber, DateTime? validationDate = null)
        {
            return ArcaCardsTransactionOrderDB.GetPreviousBlockingOrderId(cardNumber, validationDate);
        }
        public static long? GetPreviousUnBlockingOrderId(string cardNumber, DateTime? validationDate = null)
        {
            return ArcaCardsTransactionOrderDB.GetPreviousUnBlockingOrderId(cardNumber, validationDate);
        }
    }
}
