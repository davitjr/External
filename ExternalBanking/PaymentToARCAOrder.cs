using ExternalBanking.ArcaDataServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking;
using ExternalBanking.XBManagement;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{
    public class PaymentToARCAOrder : PaymentOrder
    {
        /// <summary>
        /// Քարտի համարը
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public decimal AmountAMD { get; set; }

        public string ARCAResponseCode { get; set; }


        public PaymentToARCAStatus PaymentToARCAStatus;


        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;

            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            double accountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);

            if (Source == SourceType.SSTerminal && base.IsPaymentIdUnique())
            {
                result.Errors.Add(new ActionError(1498));
            }

            if (accountBalance < (this.Amount + this.CardFee))
            {
                //Գործարքը մերժված է: Անբավարար միջոցներ:
                result.Errors.Add(new ActionError(1642, new string[] { this.DebitAccount.AccountNumber }));
            }

            if (this.ReceiverAccount != null && this.DebitAccount != null && this.ReceiverAccount.AccountNumber == this.DebitAccount.AccountNumber && this.Type != OrderType.CashConvertation)
            {
                //Կրեդիտ և դեբիտ հաշիվները նույնն են:
                result.Errors.Add(new ActionError(11));
            }
            else
            {
                if (this.ValidateForConvertation && this.DebitAccount.Currency == this.ReceiverAccount.Currency)
                {
                    //Ընտրված արժույթները նույնն են
                    result.Errors.Add(new ActionError(599));
                    return result;
                }

                //todo arpine transit account
                //if (this.ValidateDebitAccount && this.Type != OrderType.CashOutFromTransitAccountsOrder)
                if (this.ValidateDebitAccount && this.Type != OrderType.CashOutFromTransitAccountsOrder && this.Type != OrderType.SSTerminalCashInOrder && this.Type != OrderType.SSTerminalCashOutOrder)
                {
                    //Դեբետ հաշվի ստուգում
                    result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
                }
                //else if (this.Type != OrderType.CashDebit && this.Type != OrderType.CashDebitConvertation)
                //{
                //    result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber ));
                //}            

                if (CreditorStatus != 0)
                {
                    int creditorCustomerType = 0;
                    long output;
                    if (this.CreditorStatus == 10 || this.CreditorStatus == 20)
                        creditorCustomerType = 6;
                    else if (this.CreditorStatus == 13 || this.CreditorStatus == 23)
                        creditorCustomerType = 2;
                    else if (this.CreditorStatus != 10 && this.CreditorStatus != 20 && this.CreditorStatus != 0)
                        creditorCustomerType = 1;

                    if (this.CreditorStatus == 10)
                    {

                        if (this.CreditorDocumentType == 1 && this.CreditorDocumentNumber != null && this.CreditorDocumentNumber.Trim() != "" && this.CreditorDocumentNumber.Trim().Length != 10)
                        {//Պարտատիրոջ հանրային ծառայությունների համարանիշ դաշտը սխալ է լրացված
                            result.Errors.Add(new ActionError(404));
                        }
                        if (this.CreditorDocumentType == 2 && this.CreditorDocumentNumber != null && this.CreditorDocumentNumber.Trim() != "" && this.CreditorDocumentNumber.Trim().Length != 10)
                        {//Պարտատիրոջ ՀԾՀ չստանալու մասին տեղեկանքի համար դաշտը սխալ է լրացված
                            result.Errors.Add(new ActionError(474));
                        }

                        if (this.CreditorDocumentType == 1 && this.CreditorDocumentNumber != null && this.CreditorDocumentNumber.Trim() != "" && !Int64.TryParse(this.CreditorDocumentNumber.Trim(), out output))
                        {//Պարտատիրոջ ՀԾՀ-ն մուտքագրեք թվերով:
                            result.Errors.Add(new ActionError(476));
                        }
                    }
                    else if (this.CreditorStatus == 20)
                    {
                        if (String.IsNullOrEmpty(this.CreditorDescription))
                        {//§Պարտատիրոջ¦ անուն ազգանուն¦ դաշտը լրացված չէ
                            result.Errors.Add(new ActionError(400));
                        }
                    }
                    else if (creditorCustomerType == 2 || creditorCustomerType == 1)
                    {
                        if (!String.IsNullOrEmpty(this.CreditorDocumentNumber))
                        {
                            if (this.CreditorDocumentNumber.Trim().Length != 8)
                            {//Պարտատիրոջ ՀՎՀՀ դաշտը պետք է լինի 8 նիշ
                                result.Errors.Add(new ActionError(443));
                            }
                        }
                    }
                }

            }

            if (result.Errors.Count > 0)
            {
                return result;
            }
            //else if (!this.ValidateForConvertation)
            //{
            //    ActionError err = new ActionError();
            //    err = Validation.CheckAccountOperation(this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber, user.userPermissionId, this.Amount);
            //    if (err.Code != 0 && !(err.Code == 564 && this.SubType == 4) && this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.InterBankTransferNonCash)
            //        result.Errors.Add(err);
            //}

            if (result.Errors.Count > 0)
            {
                return result;
            }

            //Փոխանցում սեփական հաշիվների մեջ կամ բանկի ներսում կամ փոխարկում

            if (string.IsNullOrEmpty(this.Currency))
            {
                result.Errors.Add(new ActionError(254));
            }
            if (this.Amount < 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Errors.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                result.Errors.Add(new ActionError(25));
            }

            //if (Account.IsUserAccounts(user.userCustomerNumber, this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber))
            //{
            //    //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
            //    result.Errors.Add(new ActionError(544));
            //}


            result.Errors.AddRange(Validation.ValidatePaymentToARCAOrder(this));
            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգում բանկ ուղարկելուց
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            //todo5 Arthur
            if (this.Quality != OrderQuality.Draft && this.Quality != OrderQuality.Approved)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            else if (!this.ValidateForConvertation)
            {
                ActionError err = new ActionError();
                err = Validation.CheckAccountOperation(this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber, user.userPermissionId, this.Amount);
                if (err.Code != 0 && !(err.Code == 564 && this.SubType == 4) && this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.InterBankTransferNonCash)
                    result.Errors.Add(err);
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
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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


            result = PaymentToARCAOrderDB.Save(this, user, source);
            this.Id = result.Id;

            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }
            else
            {
                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
            }

            result = base.SaveOrderOPPerson();
            // result = base.SaveOrderFee();
            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }

            LogOrderChange(user, action);

            result = this.PaymentToArcaTransfer((ulong)this.Id);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {


                if (result.ResultCode == ResultCode.Normal)
                {
                    result = base.Approve(schemaType, userName);
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);

                    OrderDB.ChangeQuality(this.Id, OrderQuality.Completed, user.userID.ToString());
                    this.PaymentToARCAStatus = PaymentToARCAStatus.Success;
                    PaymentToARCAOrderDB.AddTransactionToCardsPayments(this);
                }
                else
                {
                    OrderDB.ChangeQuality(this.Id, OrderQuality.Declined, user.userID.ToString());
                    this.PaymentToARCAStatus = PaymentToARCAStatus.Failure;//status
                    PaymentToARCAOrderDB.AddTransactionToCardsPayments(this);
                    this.Id = 0;
                }

                LogOrderChange(user, Action.Update);
                scope.Complete();

            }


            return result;
        }
        public ActionResult PaymentToArcaTransfer(ulong orderId)
        {
            ActionResult result = new ActionResult();

            TransactionRequest cardTransactionRequest = new TransactionRequest();
            TransactionResponse arcaResponseData = new TransactionResponse();

            cardTransactionRequest.Amount = (decimal)(this.Amount + this.CardFee);
            cardTransactionRequest.Currency = GetTransferCurrencyCode(this.Currency);

            CardIdentification sourceCard = new CardIdentification();
            Card card = Card.GetCardWithOutBallance(this.DebitAccount.AccountNumber);
            sourceCard.CardNumber = card.CardNumber;
            sourceCard.ExpiryDate = card.ValidationDate.ToString("yyyyMM");
            cardTransactionRequest.Card = sourceCard;


            cardTransactionRequest.ExtensionID = orderId;//???todo
            cardTransactionRequest.DebitCredit = Validation.CashOperationDirection(this);
            try
            {
                arcaResponseData = ArcaDataService.MakeTransaction(cardTransactionRequest);
                this.ARCAResponseCode = arcaResponseData.ResponseCode;

                if (arcaResponseData.ResponseCode != "00")
                {
                    if (arcaResponseData.ResponseCode == "51")
                    {
                        //Գործարքը մերժված է: Անբավարար միջոցներ:
                        result.ResultCode = ResultCode.Failed;
                        result.Errors.Add(new ActionError(1642));
                        return result;
                    }
                    else
                    {
                        TransactionStatusRequest req = new TransactionStatusRequest();
                        req.ExtensionID = orderId;

                            TransactionDetailsBResponse r = ArcaDataService.Check(req);

                            //response.ResponseCode = r.ResponseCode;
                            //response.ResponseCodeDescription = r.ResponseCodeDescription;

                            if (r.ResponseCode != "-1")
                            { 
                                //Գործարքը մերժված է: Խնդրում ենք դիմել Բանկ:
                                result.ResultCode = ResultCode.Failed;
                                result.Errors.Add(new ActionError(1643));
                                return result;
                            }
                            else
                            {
                                result.ResultCode = ResultCode.Normal;
                                return result;
                            }
                    }
                }
                else
                {
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
            catch (Exception ex)
            {
                //Գործարքը մերժված է: Խնդրում ենք փորձել մի փոքր ուշ:
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(1644));
                return result;
            }
        }
        public short GetTransferCurrencyCode(string currency)
        {
            short code = 0;
            string currencyCodeN = Utility.GetCurrencyCode(currency);
            if (!String.IsNullOrEmpty(currencyCodeN))
            {
                code = Convert.ToInt16(currencyCodeN);
            }

            return code;
        }

    }
}
