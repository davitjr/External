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
    /// <summary>
    /// Բանկի քարտերից դեպի միջազգային Visa և MasterCard երի փոխանցման հայտ
    /// </summary>
    public class CardToOtherCardsOrder : Order
    {
        /// <summary>
        /// Փոխանցման տեսակն է Visa Direct, MasterCard MoneySend
        /// </summary>
        public InternationalCardTransferTypes TransferType { get; set; }
        /// <summary>
        /// Ուղարկողի քարտի համար
        /// </summary>
        public string SenderCardNumber { get; set; }
        /// <summary>
        /// Ստացողի քարտի համար
        /// </summary>
        public string ReceiverCardNumber { get; set; }
        /// <summary>
        /// Ստացողի անուն ազգանուն
        /// </summary>
        public string ReceiverName { get; set; }

        public ulong ArcaExtensionID { get; set; }
        public string RRN { get; set; }
        public string AuthId { get; set; }
        public string VisaAlias { get; set; }

        protected void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType =Convert.ToByte(TransferType);
            
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardToOtherCardsOrderDB.Save(this, userName, source);
                if (result.ResultCode == ResultCode.Normal)
                {
                   ActionResult resultFee = base.SaveOrderFee();
                    if (resultFee.ResultCode!=ResultCode.Normal)
                    {
                        return resultFee;
                    }
                }
                else
                {
                    return result;
                }

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                scope.Complete();
            }

            return result;
        }
        public ActionResult Approve(string userName, short schemaType, Culture culture)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    result = base.Approve(schemaType, userName);
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        Quality = OrderQuality.Sent3;
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                }
            }
            if (result.ResultCode!=ResultCode.Normal)
            {
                return result;
            }
            var names = this.ReceiverName.Split(' ');
            string name = names.Length > 0 ? names[0]:"";
            string lastname = names.Length > 1 ? names[1] : "";
            if (ACBAOperationService.CheckCriminal(name,lastname))
            {
                this.Quality = OrderQuality.Declined;
                base.Reject(47, user);
                base.SetQualityHistoryUserId(this.Quality, user.userID);
                result.ResultCode = ResultCode.ValidationError;
                result.Errors.Add(new ActionError(1847, new string[] { TransferType==InternationalCardTransferTypes.MasterCardMoneySend? "MasterCard MoneySend":"Visa Direct", Info.GetOrderRejectTypeDescription((ushort)47, culture.Language) }));
                return result;
            }

            result = new ActionResult();

            CreditCardEcommerceOrderData ecommerce = new CreditCardEcommerceOrderData();

            try
            {
                string receiverName = this.ReceiverName.Split(' ')[0] + ", " + this.ReceiverName.Split(' ')[1];

                ecommerce.Amount = Convert.ToDecimal(this.Amount);
                ecommerce.Currency = Convert.ToInt32(Utility.GetCurrencyCode(this.Currency));
                ecommerce.SourceCardIdentification = new CardIdentification();
                ecommerce.SourceCardIdentification.CardNumber = this.SenderCardNumber;
                CustomerMainData customer = ACBAOperationService.GetCustomerMainData(this.CustomerNumber);
                ecommerce.SourceCardIdentification.EmbossedName = Card.GetEmbossingName(SenderCardNumber);
                ecommerce.SourceCardIdentification.ExpiryDate = Card.GetCardExpiryDateForArca(this.SenderCardNumber);
                ecommerce.DestinationCardIdentification = new CardIdentification();
                ecommerce.DestinationCardIdentification.CardNumber = this.ReceiverCardNumber;
                ecommerce.DestinationCardIdentification.EmbossedName = receiverName;
                ecommerce.ExtId = this.ArcaExtensionID = Utility.GetLastKeyNumber(32, 22000);
                ecommerce.MerchanId = ConfigurationManager.AppSettings["c2ocMerchantId"].ToString();
                ecommerce.Sender = new PersonMoneySendType();
                var address = customer.Addresses.Find(m => m.priority.key == 1);
                ecommerce.Sender.City = address.address.TownVillage.value.Length > 25 ? Utility.TranslateToEnglish(address.address.TownVillage.value).Substring(0, 25) : Utility.TranslateToEnglish(address.address.TownVillage.value);
                ecommerce.Sender.Country = address.address.Country.value;
                ecommerce.Sender.PostalCode = address.address.PostCode;
                ecommerce.Sender.Street = address.address.fullAddress.Length > 35 ? Utility.TranslateToEnglish(address.address.fullAddress).Substring(0, 35) : Utility.TranslateToEnglish(address.address.fullAddress);
                ecommerce.Sender.Name = customer.CustomerDescriptionEng;
                ecommerce.Recepient = new PersonMoneySendType();


                ecommerce.Recepient.Name = receiverName.Length > 30 ? receiverName.Substring(0, 30) : receiverName;
                ecommerce.TerminalId = ConfigurationManager.AppSettings["c2ocTerminalId"].ToString();

                var response = ArcaDataService.CreditCardEcommerce(ecommerce);
                SaveArcaResponseData(response);
                if (response.ResponseCode == "00")
                {
                    result.ResultCode = ResultCode.Normal;
                    ActionResult res = OrderDB.UpdateHBdocumentQuality(Id,user);
                    if (res.ResultCode != ResultCode.Normal)
                    {
                        return res;
                    }
                }
                else if (response.ResponseCode.Length == 4)
                {
                    result.ResultCode = ResultCode.Normal;
                }
                else
                {
                    this.Quality = OrderQuality.Declined;
                    var rejectId = GetRejectIdFromResponse(response.ResponseCode);
                    rejectId = rejectId == 0 ? (short)52 : rejectId;
                    base.Reject(rejectId, user);
                    base.SetQualityHistoryUserId(this.Quality, user.userID);
                    result.ResultCode = ResultCode.ValidationError;
                    result.Errors.Add(new ActionError(1847, new string[] { TransferType == InternationalCardTransferTypes.MasterCardMoneySend ? "MasterCard MoneySend" : "Visa Direct", Info.GetOrderRejectTypeDescription((ushort)rejectId, culture.Language) }));
                }

            }
            catch (Exception ex)
            {
                this.Quality = OrderQuality.Declined;
                base.UpdateQuality(this.Quality);
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(1690));
                result.Errors.Add(new ActionError(1501, new string[] { ex.Message }));
                return result;
            }


            return result;
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            result.Errors.AddRange(Validation.ValidateCardToOtherCardsOrder(this));
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            return result;
        }

        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            return result;
        }

        public List<ActionError> CheckLimits()
        {
            int orderDailyCount = GetOrderDailyCount();
            double orderdailyAmount = GetOrderDailyAmount();
            orderdailyAmount += Amount * Utility.GetCBKursForDate(RegistrationDate, Currency);
            return CardToOtherCardsOrderDB.CheckLimits(Currency, orderDailyCount, Amount, orderdailyAmount);
        }
        /// <summary>
        /// Ստուգում ենք ելքագրվող քարտը Arca պրոցեսինգ կենտրոնի քարտերից է թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool IsNotArcaProcessingCard()
        {
            return CardToOtherCardsOrderDB.IsNotArcaProcessingCard(SenderCardNumber);
        }
        /// <summary>
        /// Ստուգում ենք ելքագրվող քարտի Bin ը արդյոք բացառությունների շարքում է թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool IsNotAllowedCardType()
        {
            return CardToOtherCardsOrderDB.IsNotAllowedCardType(SenderCardNumber);
        }

        public bool IsBinUnderSanctions()
        {
            bool result = false;
            if (CardToOtherCardsOrderDB.IsBinUnderSanctions(ReceiverCardNumber))
            {
                return true;
            }
            else
            {
                if (TransferType == InternationalCardTransferTypes.VisaDirect)
                {
                    result = CardToOtherCardsOrderDB.IsBinUnderVisaSanctions(ReceiverCardNumber);
                }
                else if (TransferType == InternationalCardTransferTypes.MasterCardMoneySend)
                {
                    result = CardToOtherCardsOrderDB.IsBinUnderMasterSanctions(ReceiverCardNumber);
                }
                return result;
            }
        }

        public static short GetRejectIdFromResponse(string responseCode)
        {
            return CardToOtherCardsOrderDB.GetRejectIdFromResponse(responseCode);
        }

        private ActionResult SaveArcaResponseData(CreditCardEcommerceResponse response)
        {
            return CardToOtherCardsOrderDB.SaveArcaResponseData((ulong)Id, ArcaExtensionID, response);
        }

        public void Get()
        {
            CardToOtherCardsOrderDB.Get(this);
        }

        public static double GetCardToOtherCardTransferFee(double amount, string currency,SourceType sourceType)
        {
            return CardToOtherCardsOrderDB.GetCardToOtherCardFee(amount,currency,sourceType);
        }
    }
}
