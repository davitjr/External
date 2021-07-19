using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{
    public class CardLimitChangeOrder : Order
    {
        /// <summary>
        /// Փոփոխման ենթակա լիմիտներ
        /// </summary>
        public List<CardLimit> Limits { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Քարտ
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Մերժման պատճառ
        /// </summary>
        public string RejectReasonDescription { get; set; }

        /// <summary>
        /// Գործարքը կատարողի IP հասցե
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// հիմնական քարտի ունիկալ համար
        /// </summary>
        public ulong MainCardProductID { get; set; }


        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;

            if ((this.OrderNumber == null || this.OrderNumber == ""))
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.Type = OrderType.CardLimitChangeOrder;

            this.Card = Card.GetCard(this.CardNumber, this.CustomerNumber);

            this.Limits.ForEach(item =>
            {
                switch (item.Limit)
                {
                    case LimitType.DailyCashingAmountLimit:
                        item.LimitArcaType = "LMTTZ08";
                        break;
                    case LimitType.DailyCashingQuantityLimit:
                        item.LimitArcaType = "LMTTZ05";
                        break;
                    case LimitType.DailyPaymentsAmountLimit:
                        item.LimitArcaType = "LMTTC02";
                        break;
                    case LimitType.MonthlyAggregateLimit:
                        item.LimitArcaType = "LMTTZC12";
                        break;
                }
            });

            if(this.Source != SourceType.Bank)
            {
                this.SubType = 1;
            }
        }

        public ActionResult SaveAndApprove(string userName, SourceType source, User user, short schemaType)
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
                result = CardLimitChangeOrderDB.Save(this, userName, source);

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

                if (this.SubType == 2)
                {
                    try
                    {
                        result = base.Confirm(user);
                        if (result.ResultCode != ResultCode.Normal)
                        {
                            return result;
                        }

                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                }

                scope.Complete();
            }
            if (this.SubType == 2)
            {
                return result;
            }
            if (result.ResultCode == ResultCode.Normal)
            {
                ArcaDataServiceReference.CardLimitChangeResponse cardLimitChangeResponse = new CardLimitChangeResponse();
                try
                {
                        CardLimitChangeOrderData data = new CardLimitChangeOrderData();
                        data.Limits = new List<ArcaDataServiceReference.CardLimit>();
                        data.DocId = result.Id;
                        data.CardNumber = this.Card.CardNumber;
                        data.IPAddress = this.IPAddress;
                        data.ExpDate = this.Card.ValidationDate.ToString("yyyyMM");

                        ArcaDataServiceReference.CardLimit cardLimit = new ArcaDataServiceReference.CardLimit();

                        this.Limits.ForEach(item =>
                        {
                            data.Limits.Add(
                                            new ArcaDataServiceReference.CardLimit()
                                            {
                                                LimitValue = Convert.ToUInt32(item.LimitValue),
                                                LimitArcaType = item.LimitArcaType
                                            }
                                        );
                        });

                        //Order approvment
                        cardLimitChangeResponse = ArcaDataService.ChangeCardLimit(data);
                }
                catch (Exception e)
                {
                    this.SetRejectReasonFromArca(this.Id, "Processing Error");
                    this.Quality = OrderQuality.Declined;
                    base.UpdateQuality(this.Quality);
                    throw e;
                }

                //Եթե գործողությունը կատարվել է Արքայում
                if (cardLimitChangeResponse.ResultCode == TransactionProcessingResultType.Normal)
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


                    this.SetRejectReasonFromArca(this.Id, cardLimitChangeResponse.ResultCodeDescription);
                    result.Errors.Add(new ActionError());
                    result.Errors[0].Description = cardLimitChangeResponse.ResultCodeDescription;
                    result.ResultCode = ResultCode.Failed;
                }
            }
            return result;
        }

        public ActionResult Save(string userName, SourceType source, User user, short schemaType)
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
                result = CardLimitChangeOrderDB.Save(this, userName, source);

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
            ArcaDataServiceReference.CardLimitChangeResponse cardLimitChangeResponse = new CardLimitChangeResponse();
            try
            {
                using (ArcaDataServiceClient proxy = new ArcaDataServiceClient())
                {
                    CardLimitChangeOrderData data = new CardLimitChangeOrderData();
                    data.Limits = new List<ArcaDataServiceReference.CardLimit>();
                    data.DocId = this.Id;
                    data.CardNumber = this.Card.CardNumber;
                    data.IPAddress = this.IPAddress;
                    data.ExpDate = this.Card.ValidationDate.ToString("yyyyMM");

                    ArcaDataServiceReference.CardLimit cardLimit = new ArcaDataServiceReference.CardLimit();

                    this.Limits.ForEach(item =>
                    {
                        data.Limits.Add(
                                        new ArcaDataServiceReference.CardLimit()
                                        {
                                            LimitValue = Convert.ToUInt32(item.LimitValue),
                                            LimitArcaType = item.LimitArcaType
                                        }
                                    );
                    });

                    //Order approvment
                    cardLimitChangeResponse = proxy.ChangeCardLimit(data);
                }
            }
            catch (Exception e)
            {
                this.SetRejectReasonFromArca(this.Id, "Processing Error");
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                throw e;
            }

            //Եթե գործողությունը կատարվել է Արքայում
            if (cardLimitChangeResponse.ResultCode == TransactionProcessingResultType.Normal)
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


                this.SetRejectReasonFromArca(this.Id, cardLimitChangeResponse.ResultCodeDescription);
                result.Errors.Add(new ActionError());
                result.Errors[0].Description = cardLimitChangeResponse.ResultCodeDescription;
                result.ResultCode = ResultCode.Failed;
            }

            return result;
        }
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardLimitChangeOrder(this));
            return result; 
        }

        private void SetRejectReasonFromArca(long docID, string rejectReason)
        {
            CardLimitChangeOrderDB.SetRejectReasonFromArca(docID, rejectReason);
        }

        public CardLimitChangeOrder Get()
        {
            var order = CardLimitChangeOrderDB.Get(this);
            order.MainCardProductID = Card.GetMainCardProductId(this.CardNumber);
            return order;
        }

        internal static Dictionary<string, string> GetCardLimits(long productId)
        {
            return CardLimitChangeOrderDB.GetCardLimits(productId);
        }
    }
}

