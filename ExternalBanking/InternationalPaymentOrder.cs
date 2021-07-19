using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class InternationalPaymentOrder : PaymentOrder
    {

        /// <summary>
        /// Ուղարկողի անուն/անվանում
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Ուղարկողի հասցե
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաք
        /// </summary>
        public string SenderTown { get; set; }

        /// <summary>
        /// Ուղարկողի երկիր
        /// </summary>
        public string SenderCountry { get; set; }

        /// <summary>
        /// Ուղարկողի անձնագիր
        /// </summary>
        public string SenderPassport { get; set; }

        /// <summary>
        /// Ուղարկողի ծննդյան ա/թ
        /// </summary>
        public DateTime SenderDateOfBirth { get; set; }

        /// <summary>
        /// Ուղարկողի էլ. հասցե
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Ուղարկողի ՀՎՀՀ
        /// </summary>
        public string SenderCodeOfTax { get; set; }

        /// <summary>
        /// Ուղարկողի հեռախոս
        /// </summary>
        public string SenderPhone { get; set; }

        /// <summary>
        /// Ուղարկող հաճախորդի տեսակ
        /// </summary>
        public short SenderType { get; set; }

        /// <summary>
        /// Այլ բանկերում Ուղարկողի հաշվեհամարներ
        /// </summary>
        public string SenderOtherBankAccount { get; set; }

        /// <summary>
        /// Միջնորդ բանկի SWIFT կոդ
        /// </summary>
        public string IntermediaryBankSwift { get; set; }

        /// <summary>
        /// Միջնորդ բանկի անվանում
        /// </summary>
        public string IntermediaryBank { get; set; }

        /// <summary>
        /// Ստացող բանկի SWIFT կոդ
        /// </summary>
        public string ReceiverBankSwift { get; set; }

        /// <summary>
        /// Ստացող բանկի անվանում
        /// </summary>
        public string ReceiverBank { get; set; }

        /// <summary>
        /// Ստացող բանկի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverBankAddInf { get; set; }

        /// <summary>
        /// ԲԻԿ (9 նիշ)
        /// </summary>
        public string BIK { get; set; }

        /// <summary>
        /// Թղթակցային հաշիվ (20 նիշ)
        /// </summary>
        public string CorrAccount { get; set; }

        /// <summary>
        /// ԿՊՊ (9 նիշ)
        /// </summary>
        public string KPP { get; set; }


        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverAddInf { get; set; }

        /// <summary>
        /// Ստացողի տեսակ
        /// </summary>
        public byte ReceiverType { get; set; }

        /// <summary>
        /// Ստացողի ԻՆՆ (10 կամ 12 նիշ)
        /// </summary>
        public string INN { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPayment { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR1 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR2 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR3 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR4 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR5 { get; set; }

        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPaymentRUR6 { get; set; }

        /// <summary>
        /// Երկիր
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Փոխանցման եղանակ
        /// </summary>
        public string DetailsOfCharges { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի անվանում
        /// </summary>
        public string SenderCountryName { get; set; }

        /// <summary>
        /// Երկրի անվանում
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Փոխարկման ունիկալ համար, փոխարկումներում
        /// </summary>
        public ulong UniqueNumber { get; set; }

        /// <summary>
        /// Բանկի Fedwire Routing կոդ
        /// </summary>
        public string FedwireRoutingCode { get; set; }

        /// <summary>
        /// MT
        /// </summary>
        public string MT { get; set; }

        /// <summary>
        /// Ստացողի SWIFT
        /// </summary>
        public string ReceiverSwift { get; set; }
        /// <summary>
        /// Փոխանցման լրացուցիչ տվյալներ
        /// </summary>
        /// 
        public TransferAdditionalData TransferAdditionalData { get; set; }


        /// <summary>
        /// Փոխանցման նպատակի կոդ
        /// </summary>
        /// 
        public string SwiftPurposeCode { get; set; }

        /// <summary>
        /// Փոխանցման նպատակի կոդ  այլ
        /// </summary>
        /// 
        public string PurposeCodeOther { get; set; }

        public string ReceiverTypeDescription { get; set; }


        public new void Get()
        {
            InternationalPaymentOrderDB.Get(this);
            this.Fees = Order.GetOrderFees(this.Id);

            if (this.Fees.Exists(m => m.Type == 7))
            {
                this.CardFee = this.Fees.Find(m => m.Type == 7).Amount;
                this.CardFeeCurrency = this.Fees.Find(m => m.Type == 7).Currency;
                this.Fees.Find(m => m.Type == 7).Account = this.DebitAccount;
            }
            if (this.Fees.Exists(m => m.Type == 20 || m.Type == 5))
            {
                this.TransferFee = this.Fees.Find(m => m.Type == 20 || m.Type == 5).Amount;
                this.FeeAccount = this.Fees.Find(m => m.Type == 20 || m.Type == 5).Account;
            }
        }

        private void Get(InternationalPaymentOrder order)
        {
            InternationalPaymentOrderDB.Get(order);
        }


        public static short IsCustomerSwiftTransferVerified(ulong customerNummber, SourceType source, string swiftCode = "", string receiverAaccount = "")
        {
            return InternationalPaymentOrderDB.IsCustomerSwiftTransferVerified(customerNummber, source, swiftCode, receiverAaccount);
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public new ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.FilialCode = user.filialCode;
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
                result = InternationalPaymentOrderDB.Save(this, userName, source); 
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }


        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();

            ActionResult result = this.Validate();
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

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = InternationalPaymentOrderDB.Save(this, userName, source);

                if (this.TransferAdditionalData != null)
                {
                    this.TransferAdditionalData.OrderId = result.Id;
                    TransferAdditionalDataDB.Save(this.TransferAdditionalData);
                }
                //if ((this.TransferFee > 0 || this.CardFee > 0) && this.FeeAccount != null)
                //{
                //    this.Fees = new List<OrderFee>();
                //    OrderFee fee = new OrderFee();
                //    fee.Amount = this.TransferFee;
                //    fee.Account = this.FeeAccount;
                //    fee.Currency = this.FeeAccount.Currency;
                //    fee.Type = 20;
                //    this.Fees.Add(fee);
                //    //if (this.CardFee > 0)
                //    //{
                //    //    fee = new OrderFee();
                //    //    fee.Amount = this.CardFee;
                //    //    fee.Account = this.DebitAccount;
                //    //    fee.Currency = this.FeeAccount.Currency;
                //    //    fee.Type = 20;
                //    //    this.Fees.Add(fee);
                //    //}
                //}

                if (this.SwiftMessageID != 0)
                {
                    Order.SaveOrderLinkId(this.SwiftMessageID, this.Id, 1);
                }
                
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }
                if (this.DebitAccount.Currency != this.Currency)
                {
                    CurrencyExchangeOrder exchangeOrder = new CurrencyExchangeOrder((InternationalPaymentOrder)this);
                    exchangeOrder.UniqueNumber = this.UniqueNumber;
                    CurrencyExchangeOrderDB.Save(exchangeOrder, userName, Source);
                }
                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

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

            if (this.Source != SourceType.AcbaOnline && this.Source != SourceType.MobileBanking && this.Source != SourceType.AcbaOnlineXML && this.Source != SourceType.ArmSoft)
            {
                result = base.Confirm(user);
            }


            return result;
        }


        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected new void Complete()
        {

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking || Source == SourceType.AcbaOnlineXML || Source == SourceType.ArmSoft)
            {
                this.RegistrationDate = DateTime.Now.Date;
                if (this.Currency == "RUR")
                    this.Description = DescriptionForPaymentRUR1 + " " + DescriptionForPaymentRUR2 + " " + DescriptionForPaymentRUR3 +
                        " " + DescriptionForPaymentRUR4 + " " + DescriptionForPaymentRUR5 + " " + DescriptionForPaymentRUR6;
                else
                    this.Description = this.DescriptionForPayment;

                InitCustomer();
            }

            if (this.DebitAccount != null && this.Type != OrderType.CashInternationalTransfer)
            {
                this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
            }
            else if (this.Type == OrderType.CashInternationalTransfer)
            {
                //Տարանցիկ հաշվի լրացում
                this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.Currency, user.filialCode);
            }

            if (this.Source == SourceType.MobileBanking || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }

            if (string.IsNullOrEmpty(this.OrderNumber))
                this.OrderNumber = GenerateNewOrderNumber(OrderNumberTypes.InternationalOrder, 22000).ToString();

            Customer customer = new Customer(this.CustomerNumber, Languages.hy);
            this.TransferFee = customer.GetInternationalPaymentOrderFee(this);


            if (this.DebitAccount.Currency == "AMD")
            {
                this.ConvertationRate1 = 0;
            }
            if (this.Currency == this.DebitAccount.Currency || this.Type == OrderType.CashInternationalTransfer)
            {
                this.ConvertationRate = 0;
                this.ConvertationRate1 = 0;
            }

            if (this.Type != OrderType.CashInternationalTransfer)
            {
                this.CardFee = customer.GetCardFee(this);
            }

            if (this.Type == OrderType.CashInternationalTransfer)
            {
                this.FeeAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", user.filialCode);

            }
            if (this.DebitAccount.Currency != this.Currency)
            {
                this.UniqueNumber = Utility.GetLastKeyNumber(4, FilialCode);
            }

            this.ReceiverAccount.Currency = this.Currency;
            CheckDillingConfirm();

        }
        /// <summary>
        /// Միջազգային վճարման հանձնարարականի ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            if (templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));
            }

            if (this.TransferAdditionalData != null)
            {
                result.Errors.AddRange(Validation.ValidateTransferAdditionalData(this.TransferAdditionalData, this.Amount));
            }

            if (this.Type != OrderType.CashInternationalTransfer)
            {
                result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
                if (result.Errors.Count > 0)
                {
                    return result;
                }
                if (templateType == TemplateType.None)
                {
                    result.Errors.AddRange(Validation.ValidateFeeAccount(this.CustomerNumber, this.FeeAccount));
                }
            }
            else
            {
                result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber));
            }

            result.Errors.AddRange(ValidateData(templateType));
            result.Errors.AddRange(Validation.ValidateAttachmentDocument(this));

            if(Source == SourceType.Bank)
            {
                if (this.UrgentSign)
                {
                    TimeSpan start = new TimeSpan(15, 00, 0);
                    TimeSpan end = new TimeSpan(15, 30, 0);
                    TimeSpan now = DateTime.Now.TimeOfDay;

                    if (!((now >= start) && (now <= end)))
                    {
                        //Միջազգային փոխանցումը կարող է ընդունվել Շտապ պայմանով Ժամը 15:00-ից մինչև 15:30-ը
                        result.Errors.Add(new ActionError(1537));
                    }
                }
            }

            if (Source == SourceType.Bank && this.DebitAccount.IsCardAccount())
            {

                Card card = Card.GetCardWithOutBallance(DebitAccount.AccountNumber);
                if (card.RelatedOfficeNumber == 2405 || card.RelatedOfficeNumber == 2572 || card.RelatedOfficeNumber == 2573 || card.RelatedOfficeNumber == 2574)
                {
                    result.Errors.Add(new ActionError(1525));
                }

                if (this.ReasonId == 0)
                {
                    //Պատճառ դաշտը ընտրված չէ։
                    result.Errors.Add(new ActionError(1523));
                }

                if (this.ReasonId == 99 && string.IsNullOrEmpty(this.ReasonIdDescription))
                {
                    //Պատճառի այլ դաշտը լրացված չէ։
                    result.Errors.Add(new ActionError(1524));
                }
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
            {
                if (this.Currency == "RUR" && this.Type == OrderType.InternationalTransfer && this.SubType == 1)
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.ArmSoft || this.Source == SourceType.AcbaOnlineXML)
                    {
                        result.Errors.AddRange(Validation.CheckRussianLetters(this.Receiver, this.ReceiverBank, this.Sender, this.SenderAddress));
                    }
                }
            }
            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
            {
                if ((this.Type == OrderType.InternationalTransfer || this.Type == OrderType.RATransfer) && this.SubType == 1)
                {
                    result.Errors.AddRange(Validation.CheckSymbolsExistance(this.SenderAddress, this.SenderTown,
                    this.SenderOtherBankAccount, this.IntermediaryBankSwift, this.ReceiverBankAddInf, this.DescriptionForPayment,
                    this.Receiver, this.ReceiverAccount.AccountNumber, this.ReceiverAddInf, this.ReceiverBankSwift, this.ReceiverBankCode));
                }
            }
               
            
            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգում ուղարկելուց
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (this.Quality != OrderQuality.Draft && this.Quality != OrderQuality.Approved)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }
            if (this.CustomerNumber == 0)
            {
                //Հաճախորդը գտնված չէ:
                result.Errors.Add(new ActionError(771));
            }
            if (this.Type != OrderType.CashInternationalTransfer)
            {
                Account debitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
                Account feeAccount = Account.GetAccount(this.FeeAccount.AccountNumber);

                double debitAccountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);
                double feeAccountBalance = Account.GetAcccountAvailableBalance(this.FeeAccount.AccountNumber);

                if (OnlySaveAndApprove)
                {
                    debitAccountBalance += Order.GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.CashDebit);
                    feeAccountBalance += Order.GetSentNotConfirmedAmounts(this.FeeAccount.AccountNumber, OrderType.CashDebit);
                }


                string debitCurrency = debitAccount.Currency;
                double cardFee = 0;
                ///Եթե փոխանցումը կատարվում է քարտից, ապա ստուգվում է մնացորդը ԱՐՔԱ-ում
                if (debitAccount.IsCardAccount())
                {
                    Card card = new Card();
                    card = Card.GetCardWithOutBallance(debitAccount.AccountNumber);

                    KeyValuePair<String, double> arcaBalance = card.GetArCaBalance(this.user.userID);

                    if (arcaBalance.Key == "00" && arcaBalance.Value <= debitAccountBalance)
                    {
                        debitAccountBalance = arcaBalance.Value;
                    }
                    if (this.CardFee != 0)
                    {
                        cardFee = this.CardFee;
                    }
                }

                ///Եթե միջնորդավճարը գանձվում է քարտից, ապա ստուգվում է մնացորդը ԱՐՔԱ-ում
                if (feeAccount.IsCardAccount())
                {
                    Card feeAccountCard = new Card();
                    feeAccountCard = Card.GetCardWithOutBallance(feeAccount.AccountNumber);

                    KeyValuePair<String, double> arcaBalanceFeeCard = feeAccountCard.GetArCaBalance(this.user.userID);

                    if (arcaBalanceFeeCard.Key == "00" && arcaBalanceFeeCard.Value <= feeAccountBalance)
                    {
                        feeAccountBalance = arcaBalanceFeeCard.Value;
                    }

                }

                if (debitAccount.Currency != this.Currency && this.Source != SourceType.Bank)
                {
                    this.ReceiverAccount.Currency = this.Currency;
                    result.Errors.AddRange(Validation.ValidateRate(this));
                }

                if(this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                {
                    if (this.UrgentSign)
                    {
                        //Տվյալ տեսակի գործարքը չի կարող ընդունվել շտապ պայմանով:
                        result.Errors.Add(new ActionError(1536));
                    }
                }
              
                double debitBalance = debitAccountBalance + Order.GetSentOrdersAmount(debitAccount.AccountNumber, this.Source);
                double feeBalance = feeAccountBalance + Order.GetSentOrdersAmount(feeAccount.AccountNumber, this.Source);

                //Եթե փոխանցումն ու միջնորդավճարի գանձումը կատարվում են նույն հաշվից(այդ դեպքում արժույթը միշտ AMD է)
                if (debitAccount == feeAccount)
                {
                    if (debitBalance < this.Amount * this.ConvertationRate + this.TransferFee + cardFee)
                    {
                        //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                        if (Account.AccountAccessible(debitAccount.AccountNumber, user.AccountGroup))
                            result.Errors.Add(new ActionError(499, new string[] { debitAccount.AccountNumber.ToString(), debitBalance.ToString("#,0.00") + " " + debitAccount.Currency, debitAccount.Balance.ToString("#,0.00") + " " + debitAccount.Currency }));
                        else
                            result.Errors.Add(new ActionError(788, new string[] { debitAccount.AccountNumber }));
                    }
                }
                else
                {

                    //Amundi միջնորդավճարի հաշիվներ , բացառել միջնորդավճարի ստուգումը
                    //if (!(this.Source == SourceType.AcbaOnline && (this.CustomerNumber == 104100001841 || this.CustomerNumber == 104100001842 || this.CustomerNumber == 104100001843)))
                    //{
                    if (feeBalance < this.TransferFee)
                    {
                        if (Account.AccountAccessible(feeAccount.AccountNumber, user.AccountGroup))
                            result.Errors.Add(new ActionError(499, new string[] { feeAccount.AccountNumber.ToString(), feeBalance.ToString("#,0.00") + " " + feeAccount.Currency, feeAccount.Balance.ToString("#,0.00") + " " + feeAccount.Currency }));
                        else
                            result.Errors.Add(new ActionError(788, new string[] { feeAccount.AccountNumber }));
                    }
                    //}

                    if (debitCurrency == "AMD")
                    {
                        this.ReceiverAccount.Currency = this.Currency;
                        if (debitBalance < (this.Amount * this.ConvertationRate) + cardFee)
                        {
                            if (Account.AccountAccessible(debitAccount.AccountNumber, user.AccountGroup))
                                result.Errors.Add(new ActionError(499, new string[] { debitAccount.AccountNumber.ToString(), debitBalance.ToString("#,0.00") + " " + debitAccount.Currency, debitAccount.Balance.ToString("#,0.00") + " " + debitAccount.Currency }));
                            else
                                result.Errors.Add(new ActionError(788, new string[] { debitAccount.AccountNumber }));
                        }
                    }
                    else if (debitCurrency == this.Currency)
                    {
                        if (debitBalance < this.Amount + cardFee)
                        {
                            if (Account.AccountAccessible(debitAccount.AccountNumber, user.AccountGroup))
                                result.Errors.Add(new ActionError(499, new string[] { debitAccount.AccountNumber.ToString(), debitBalance.ToString("#,0.00") + " " + debitAccount.Currency, debitAccount.Balance.ToString("#,0.00") + " " + debitAccount.Currency }));
                            else
                                result.Errors.Add(new ActionError(788, new string[] { debitAccount.AccountNumber }));
                        }
                    }
                    else
                    {
                        this.ReceiverAccount.Currency = this.Currency;
                        if (debitBalance < this.Amount * Math.Round(this.ConvertationRate1 / this.ConvertationRate, 5) + cardFee)
                        {
                            if (Account.AccountAccessible(debitAccount.AccountNumber, user.AccountGroup))
                                result.Errors.Add(new ActionError(499, new string[] { debitAccount.AccountNumber.ToString(), debitBalance.ToString("#,0.00") + " " + debitAccount.Currency, debitAccount.Balance.ToString("#,0.00") + " " + debitAccount.Currency }));
                            else
                                result.Errors.Add(new ActionError(788, new string[] { debitAccount.AccountNumber }));
                        }
                    }
                }
            }
            else if (this.Type == OrderType.CashInternationalTransfer)
            {

                double debitAccountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);
                double feeAccountBalance = Account.GetAcccountAvailableBalance(this.FeeAccount.AccountNumber);

                if (OnlySaveAndApprove)
                {
                    debitAccountBalance += Order.GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.TransitPaymentOrder);
                    feeAccountBalance += Order.GetSentNotConfirmedAmounts(this.FeeAccount.AccountNumber, OrderType.TransitPaymentOrder);
                }

                if (debitAccountBalance < this.Amount)
                {
                    result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", debitAccountBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency }));

                }

                if (feeAccountBalance < this.TransferFee)
                {
                    result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ դրամային", feeAccountBalance.ToString("#,0.00") + " " + this.FeeAccount.Currency, this.FeeAccount.Balance.ToString("#,0.00") + " " + this.FeeAccount.Currency }));
                }

            }
            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
            {
                if (this.Currency == "RUR" && this.Type == OrderType.InternationalTransfer && this.SubType == 1)
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.ArmSoft || this.Source == SourceType.AcbaOnlineXML)
                    {
                        result.Errors.AddRange(Validation.CheckRussianLetters(this.Receiver, this.ReceiverBank, this.Sender, this.SenderAddress));
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
        /// Ստուգում է տեքստային տվյլաները
        /// </summary>
        /// <returns></returns>
        public List<ActionError> ValidateData(TemplateType templateType)
        {
            List<ActionError> result = new List<ActionError>();
            bool isNumeric = true;
            double output;

            if (string.IsNullOrEmpty(this.Currency))
            {
                //Արժույթը ընտրված չէ:
                result.Add(new ActionError(254));
            }

            if (templateType == TemplateType.CreatedAsGroupService || templateType == TemplateType.None)
            {
                if (this.Amount <= 0.01)
                {
                    //Մուտքագրված գումարը սխալ է:
                    result.Add(new ActionError(22));
                }

            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                //Գումարը սխալ է մուտքագրած:
                result.Add(new ActionError(25));
            }


            if (templateType == TemplateType.None || templateType == TemplateType.CreatedAsGroupService)
            {
                if (Description.Length > 150)
                {
                    //«Վճարման նպատակ» դաշտի առավելագույն նիշերի քանակն է 150:
                    result.Add(new ActionError(646));
                }

                if (Utility.IsExistForbiddenCharacter(Description))
                {
                    //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                    result.Add(new ActionError(647));
                }
            }


            if (this.MT != "202")
            {
                if (string.IsNullOrEmpty(this.ReceiverBank))
                {
                    //Ստացողի բանկի տվյալները բացակայում են:
                    result.Add(new ActionError(85));
                }
                else if (this.ReceiverBank.Length > 140)
                {
                    //Ստացողի բանկի տվյալները առավելագույնը 140 նիշ է:
                    result.Add(new ActionError(95));
                }
            }
            if (string.IsNullOrEmpty(this.ReceiverAccount.AccountNumber))
            {
                //Ստացողի հաշիվը բացակայում է:
                result.Add(new ActionError(86));
            }
            else if (this.ReceiverAccount.AccountNumber.Length > 34)
            {
                //Ստացողի հաշիվը առավելագույնը 34 նիշ է:
                result.Add(new ActionError(99));
            }

            if (string.IsNullOrEmpty(this.Receiver))
            {
                //Ստացողի տվյալները բացակայում են:
                result.Add(new ActionError(87));
            }
            else if (this.Receiver.Length > 140 || (!string.IsNullOrEmpty(this.ReceiverAddInf) && this.Receiver.Length + this.ReceiverAddInf.Length > 140))
            {
                //Ստացողի տվյալները առավելագույնը 140 նիշ է:
                result.Add(new ActionError(100));
            }
            else if (Utility.IsExistForbiddenCharacter(this.Receiver))
            {
                //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                result.Add(new ActionError(433));
            }
            else
            {
                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                {
                    string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.Receiver);

                    if (!String.IsNullOrEmpty(checkSymbols))
                    {
                        //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(159, new string[] { checkSymbols }));
                    }
                }

            }

            if (this.Currency != "RUR")
            {

                bool isArabianEmiratTransfer = false;
                if (!string.IsNullOrEmpty(this.ReceiverBankSwift))
                {
                    if (this.ReceiverBankSwift.Substring(4, 2) == "AE")
                    {
                        isArabianEmiratTransfer = true;
                    }
                    else
                    {
                        this.SwiftPurposeCode = "";
                        this.PurposeCodeOther = "";
                    }

                }

                if (!string.IsNullOrEmpty(this.SwiftPurposeCode) && isArabianEmiratTransfer)
                {
                    if (this.SwiftPurposeCode == "999" &&  string.IsNullOrEmpty(this.PurposeCodeOther))
                    {
                        // ՈՒշադրություն: Այլ ընտրելու դեպքում նպատակի կոդ դաշտը պարտադիր է 
                        result.Add(new ActionError(1599));
                    }
                    if (!string.IsNullOrEmpty(this.PurposeCodeOther))
                    {
                        if (this.SwiftPurposeCode.Length > 3 || this.PurposeCodeOther.Length > 3)
                        {
                            // Փոխանցման նպատակի կոդ դաշտը պետք է լինի 3 նիշ:
                            result.Add(new ActionError(1598));
                        }
                    }
                    if (templateType == TemplateType.CreatedAsGroupService || templateType == TemplateType.None)
                    {
                        if (this.Description == null || this.Description == "")
                        {
                            //Վճարման նպատակը մուտքագրված չէ:
                            result.Add(new ActionError(645));
                        }

                    }
                }
              
             


                if (!string.IsNullOrEmpty(this.IntermediaryBankSwift) && this.IntermediaryBankSwift.Length != 11)
                {
                    //S.W.I.F.T . կոդը պետք է լինի 11 նիշ:
                    result.Add(new ActionError(82));
                }

                if (!string.IsNullOrEmpty(this.IntermediaryBankSwift) && string.IsNullOrEmpty(this.IntermediaryBank))
                {
                    //Միջնորդ բանկի տվյալները բացակայում են:
                    result.Add(new ActionError(83));
                }
                //else if (!string.IsNullOrEmpty(this.IntermediaryBank ) && string.IsNullOrEmpty(this.IntermediaryBankSwift ))
                //{
                //    //Միջնորդ բանկի S.W.I.F.T. կոդը բացակայում է:
                //    result.Add(new ActionError(103));
                //}

                if (!string.IsNullOrEmpty(this.IntermediaryBank) && this.IntermediaryBank.Length > 250)
                {
                    //Միջնորդ բանկի տվյալները առավելագույնը 255 նիշ է:
                    result.Add(new ActionError(98));
                }


                if (string.IsNullOrEmpty(this.ReceiverAddInf))
                {
                    // Լրացուցիչ տվյալներ(հասցե կամ ծննդյան ամսաթիվ, կամ անձնագրի/ միջ.ID տվյալներ) դաշտը լրացված չէ
                    result.Add(new ActionError(1281));
                }
                //if (string.IsNullOrEmpty(this.ReceiverBankSwift))
                //{
                //    //Ստացողի բանկի S.W.I.F.T. կոդը բացակայում է:
                //    result.Add(new ActionError(84));
                //}
                //else
                if (!string.IsNullOrEmpty(this.ReceiverBankSwift) && this.ReceiverBankSwift.Length != 11)
                {
                    //S.W.I.F.T . կոդը պետք է լինի 11 նիշ:
                    result.Add(new ActionError(82));
                }


                if (!string.IsNullOrEmpty(this.ReceiverBankAddInf) && this.ReceiverBankAddInf.Length > 210)
                {
                    //Ստացողի բանկի լրացուցիչ տվյալները առավելագույնը 210 նիշ է:
                    result.Add(new ActionError(96));
                }

                if (!string.IsNullOrEmpty(this.ReceiverBankAddInf) && !string.IsNullOrEmpty(this.DescriptionForPayment) && this.Source == SourceType.Bank)
                {
                    if (this.ReceiverBankAddInf.Length + this.DescriptionForPayment.Length > 140)
                    {
                        //ՈՒշադրություն: Տեղեկատվությունը գերազանցում է նիշերի թույլատրելի քանակը: «Վճարման մանրամասներ» և «Ստացող բանկ/Բանկի լրացուցիչ տվյալներ» դաշտերի նիշերի քանակը միասին պետք է կազմի​ առավելագույնը 140 նիշ
                        result.Add(new ActionError(1535));
                    }
                }

                if (string.IsNullOrEmpty(this.DescriptionForPayment))
                {
                    //Վճարման մանրամասները բացակայում են:
                    result.Add(new ActionError(88));
                }
                else if (this.DescriptionForPayment.Length > 135)
                {
                    //Վճարման մանրամասները առավելագույնը 135 նիշ է:
                    result.Add(new ActionError(101));
                }
                else if (Utility.IsExistForbiddenCharacter(this.DescriptionForPayment))
                {
                    //«Վճարման նպատակ դաշտը պարունակում է անթույլատրելի նիշ/նիշեր: 
                    result.Add(new ActionError(988));
                }
                else
                {

                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                    {
                        if (templateType == TemplateType.CreatedAsGroupService || templateType == TemplateType.None)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(Description);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                                result.Add(new ActionError(78, new string[] { checkSymbols }));
                            }
                        }

                    }
                }

                if (templateType == TemplateType.CreatedAsGroupService || templateType == TemplateType.None)
                {
                    if (string.IsNullOrEmpty(this.SenderTown))
                    {
                        //Ուղարկողի քաղաքը բացակայում է:
                        result.Add(new ActionError(501));
                    }
                    else if (!Utility.IsLatinLetter(this.SenderTown))
                    {
                        //Ուղարկողի քաղաք դաշտում թույլատրվում են միայն լատինական տառեր և թվեր
                        result.Add(new ActionError(1063));
                    }


                    if (string.IsNullOrEmpty(this.SenderCountry))
                    {
                        //Ուղարկողի երկիրը բացակայում է:
                        result.Add(new ActionError(502));
                    }
                }




                string symbol = "";
                if (!string.IsNullOrEmpty(ReceiverBank))
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.ReceiverBank, 1);
                    if (symbol != "")
                    {
                        // Ստացողի բանկի տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                        result.Add(new ActionError(588, new string[] { symbol }));
                    }
                }
                if (!string.IsNullOrEmpty(this.ReceiverBankAddInf))
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.ReceiverBankAddInf);
                    if (symbol != "")
                    {
                        //Ստացողի բանկի լրացուցիչ տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                        result.Add(new ActionError(589, new string[] { symbol }));
                    }
                    if (Utility.IsExistForbiddenCharacter(this.ReceiverBankAddInf))
                    {
                        //Ստացողի բանկի լրացուցիչ տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                        result.Add(new ActionError(589, new string[] { "" }));
                    }
                    else
                    {
                        if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.ReceiverBankAddInf);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //Ստացողի բանկի լրացուցիչ տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                                result.Add(new ActionError(588, new string[] { checkSymbols }));
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(this.IntermediaryBank))
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.IntermediaryBank, 1);
                    if (symbol != "")
                    {
                        //Միջնորդ բանկի տվյալներ դաշտի մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(590, new string[] { symbol }));
                    }
                }


                symbol = Utility.IsExistSwiftForbiddenCharacter(this.Receiver);
                if (symbol != "")
                {
                    //Ստացողի տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                    result.Add(new ActionError(591, new string[] { symbol }));
                }
                else
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                    {
                        string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.Receiver);

                        if (!String.IsNullOrEmpty(checkSymbols))
                        {
                            //Ստացողի տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                            result.Add(new ActionError(591, new string[] { checkSymbols }));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(this.ReceiverAddInf))
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.ReceiverAddInf);
                    if (symbol != "")
                    {
                        //Ստացողի տվյալներ դաշտի մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(591, new string[] { symbol }));
                    }
                    if (Utility.IsExistForbiddenCharacter(this.ReceiverAddInf))
                    {
                        //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(1101));
                    }
                    else
                    {
                        if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.ReceiverAddInf);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //Ստացողի տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                                result.Add(new ActionError(591, new string[] { checkSymbols }));
                            }
                        }
                    }
                }
                else
                {
                    // Լրացուցիչ տվյալներ(հասցե կամ ծննդյան ամսաթիվ, կամ անձնագրի/ միջ.ID տվյալներ) դաշտը լրացված չէ
                    result.Add(new ActionError(1281));
                }


                if (this.Description != null)
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.DescriptionForPayment);
                    if (symbol != "")
                    {
                        //Վճարման մանրամասներ դաշտի մեջ կա անթույլատրելի նշան`  
                        result.Add(new ActionError(592, new string[] { symbol }));
                    }
                    else
                    {
                        if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.DescriptionForPayment);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //Վճարման մանրամասներ դաշտի մեջ կա անթույլատրելի նշան`  
                                result.Add(new ActionError(592, new string[] { checkSymbols }));
                            }
                        }
                    }
                }

                if (templateType == TemplateType.CreatedAsGroupService || templateType == TemplateType.None)
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.Sender);
                    if (symbol != "")
                    {
                        //Ուղարկողի անվանում դաշտի մեջ կա անթույլատրելի նշան`  
                        result.Add(new ActionError(593, new string[] { symbol }));
                    }
                    else
                    {
                        if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.Sender);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //Ուղարկողի անվանում դաշտի մեջ կա անթույլատրելի նշան`  
                                result.Add(new ActionError(593, new string[] { checkSymbols }));
                            }
                        }
                    }


                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.SenderAddress);
                    if (symbol != "")
                    {
                        //Ուղարկողի հասցե դաշտի մեջ կա անթույլատրելի նշան`  
                        result.Add(new ActionError(594, new string[] { symbol }));
                    }
                    else if (Utility.IsExistForbiddenCharacter(this.SenderAddress))
                    {
                        //Ուղարկողի հասցե դաշտի մեջ կա անթույլատրելի նշան`  
                        result.Add(new ActionError(594, new string[] { symbol }));
                    }
                    else
                    {
                        if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.SenderAddress);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //Ուղարկողի հասցե դաշտի մեջ կա անթույլատրելի նշան`  
                                result.Add(new ActionError(594, new string[] { checkSymbols }));
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(this.SenderPassport) && this.SenderType == (short)CustomerTypes.physical)
                {
                    symbol = Utility.IsExistSwiftForbiddenCharacter(this.SenderPassport);
                    if (symbol != "" && this.SenderType == (short)CustomerTypes.physical)
                    {
                        //Ուղարկողի անձնագիր դաշտի մեջ կա անթույլատրելի նշան` (  ) 
                        result.Add(new ActionError(595, new string[] { symbol }));
                    }
                    else
                    {
                        if ((this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft) && this.SenderType == (short)CustomerTypes.physical)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.SenderAddress);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //Ուղարկողի անձնագիր դաշտի մեջ կա անթույլատրելի նշան` (  ) 
                                result.Add(new ActionError(595, new string[] { checkSymbols }));
                            }
                        }
                    }
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(this.Receiver) && !string.IsNullOrEmpty(this.ReceiverAddInf))
                {
                    if ((this.MT == "202" || this.MT == "103") && (this.Receiver.Length + this.ReceiverAddInf.Length) > 140)
                    {
                        // ՈՒշադրություն: Տեղեկատվությունը գերազանցում է նիշերի թույլատրելի քանակը: Ստացողի տվյալներ  Անվանում  և  Լրացուցիչ տվյալներ  դաշտերի նիշերի քանակը միասին պետք է կազմի​ առավելագույնը 140 նիշ:
                        result.Add(new ActionError(1597));
                    }
                }
                string DescriptionForPaymentRUR;
                isNumeric = Double.TryParse(String.IsNullOrEmpty(this.BIK) ? "" : this.BIK.Trim(), out output);
                if (string.IsNullOrEmpty(this.BIK))
                {
                    ////ԲԻԿ դաշտը լրացված չէ:
                    result.Add(new ActionError(190));
                }
                else if (this.BIK.Length > 9)
                {
                    //ԲԻԿ դաշտը պետք է լինի 9 նիշ:
                    result.Add(new ActionError(191));
                }
                else if (!isNumeric)
                {
                    // դաշտը պետք է լինի միայն թվանշաններ:
                    result.Add(new ActionError(205, new string[] { "ԲԻԿ" }));
                }

                isNumeric = Double.TryParse(String.IsNullOrEmpty(this.CorrAccount.Substring(0, 10)) ? "" : this.CorrAccount.Substring(0, 10).Trim(), out output);
                if (isNumeric == true)
                    isNumeric = Double.TryParse(String.IsNullOrEmpty(this.CorrAccount.Substring(10)) ? "" : this.CorrAccount.Substring(10).Trim(), out output);
                if (string.IsNullOrEmpty(this.CorrAccount))
                {
                    //Թղթակցային հաշվի դաշտը լրացված չէ:
                    result.Add(new ActionError(192));
                }
                else if (this.CorrAccount.Length != 20)
                {
                    //Թղթակցային հաշվը պետք է լինի 20 նիշ:
                    result.Add(new ActionError(193));
                }
                else if (!isNumeric)
                {
                    // դաշտը պետք է լինի միայն թվանշաններ:
                    result.Add(new ActionError(205, new string[] { "Թղթակցային հաշիվ" }));
                }

                if (!string.IsNullOrEmpty(this.KPP))
                {
                    isNumeric = Double.TryParse(String.IsNullOrEmpty(this.KPP) ? "" : this.KPP.Trim(), out output);
                    if (this.KPP.Length != 9)
                    {
                        //ԿՊՊ դաշտը պետք է լինի 9 նիշ:
                        result.Add(new ActionError(194));
                    }
                    else if (!isNumeric)
                    {
                        // դաշտը պետք է լինի միայն թվանշաններ:
                        result.Add(new ActionError(205, new string[] { "ԿՊՊ" }));
                    }
                }

                if (!string.IsNullOrEmpty(this.ReceiverAddInf))
                {
                    if (Utility.IsExistForbiddenCharacter(this.ReceiverAddInf))
                    {
                        //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                        result.Add(new ActionError(1101));
                    }
                    else
                    {
                        if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                        {
                            string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.ReceiverAddInf);

                            if (!String.IsNullOrEmpty(checkSymbols))
                            {
                                //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                                result.Add(new ActionError(1101, new string[] { checkSymbols }));
                            }
                        }
                    }
                }
                if (this.ReceiverType == 0)
                {
                    //Ստացողի տեսակ դաշտը ընտրված չէ:
                    result.Add(new ActionError(196));
                }
                else
                {
                    if (this.ReceiverType != 1 && this.ReceiverType != 3 && string.IsNullOrEmpty(this.INN))
                    {
                        //ԻՆՆ դաշտը լրացված չէ:
                        result.Add(new ActionError(197));
                    }
                    else if ((this.ReceiverType == 1 || this.ReceiverType == 3) && !string.IsNullOrEmpty(this.INN) && this.INN.Length != 12)
                    {
                        //ԻՆՆ դաշտը պետք է լինի 12  նիշ:
                        result.Add(new ActionError(198, new string[] { "12" }));
                    }
                    else if (this.ReceiverType == 2 && this.INN.Length != 10)
                    {
                        //ԻՆՆ դաշտը պետք է լինի 10 նիշ:
                        result.Add(new ActionError(198, new string[] { "10" }));
                    }

                }
                isNumeric = Double.TryParse(String.IsNullOrEmpty(this.INN) ? "" : this.INN.Trim(), out output);
                if (!string.IsNullOrEmpty(this.INN) && !isNumeric)
                {
                    // դաշտը պետք է լինի միայն թվանշաններ:
                    result.Add(new ActionError(205, new string[] { "Ստացողի ԻՆՆ" }));
                }

                if (string.IsNullOrEmpty(this.DescriptionForPaymentRUR1))
                {
                    //Վճարման մանրամասները բացակայում են:
                    result.Add(new ActionError(88));
                }
                if (this.DescriptionForPaymentRUR1 != "Материальная помощь" && this.DescriptionForPaymentRUR1 != "Другое" && string.IsNullOrEmpty(this.DescriptionForPaymentRUR2))
                {
                    //Վճարման մանրամասները լրացված չեն:
                    result.Add(new ActionError(199));
                }

                if (!string.IsNullOrEmpty(this.DescriptionForPaymentRUR2) && this.DescriptionForPaymentRUR2.Length > 125)
                {
                    //Վճարման մանրամասների նպատակ դաշտը առավելագույնը 125 նիշ է:
                    result.Add(new ActionError(200));
                }


                if (this.DescriptionForPaymentRUR1 != "Материальная помощь" && this.DescriptionForPaymentRUR1 != "Другое" && string.IsNullOrEmpty(this.DescriptionForPaymentRUR4))
                {
                    //Վճարման մանրամասները լրացված չեն:
                    result.Add(new ActionError(199));
                }
                else if (!string.IsNullOrEmpty(this.DescriptionForPaymentRUR4) && this.DescriptionForPaymentRUR4.Length > 50)
                {
                    //Վճարման մանրամասների պայմանագրի դաշտը առավելագույնը 50 նիշ է:
                    result.Add(new ActionError(201));
                }


                if (this.DescriptionForPaymentRUR1 != "Материальная помощь" && this.DescriptionForPaymentRUR1 != "Другое" && this.DescriptionForPaymentRUR5 == "с НДС")
                {
                    if (!string.IsNullOrEmpty(this.DescriptionForPaymentRUR6) && this.DescriptionForPaymentRUR6.Length > 10)
                    {
                        //Վճարման մանրամասների ԱԱՀ-ի գումարի դաշտը առավելագույնը 10 նիշ է:
                        result.Add(new ActionError(204));
                    }
                    isNumeric = Double.TryParse(String.IsNullOrEmpty(this.DescriptionForPaymentRUR6) ? "" : this.DescriptionForPaymentRUR6.Trim(), out output);
                    if (!isNumeric && !string.IsNullOrEmpty(this.DescriptionForPaymentRUR6))
                    {
                        // դաշտը պետք է լինի միայն թվանշաններ:
                        result.Add(new ActionError(205, new string[] { "Վճարման մանրամասների ԱԱՀ-ի գումար" }));
                    }

                }

                DescriptionForPaymentRUR = this.DescriptionForPaymentRUR1;

                if (!string.IsNullOrEmpty(this.DescriptionForPaymentRUR2))
                {
                    DescriptionForPaymentRUR = DescriptionForPaymentRUR + " " + this.DescriptionForPaymentRUR2;
                }

                if (this.DescriptionForPaymentRUR1 != "Материальная помощь" && this.DescriptionForPaymentRUR1 != "Другое")
                {
                    DescriptionForPaymentRUR = DescriptionForPaymentRUR + " " + this.DescriptionForPaymentRUR3 + " " + this.DescriptionForPaymentRUR4 + " " + this.DescriptionForPaymentRUR5;
                }
                if (this.DescriptionForPaymentRUR1 != "Материальная помощь" && this.DescriptionForPaymentRUR1 != "Другое" && this.DescriptionForPaymentRUR5 == "с НДС")
                {
                    DescriptionForPaymentRUR = DescriptionForPaymentRUR + " " + this.DescriptionForPaymentRUR6 + " RUB";
                }

                if (DescriptionForPaymentRUR.Length > 135)
                {
                    //Վճարման մանրամասների բոլոր դաշտերի նիշերի հանրագումարը չպետք է գերազանցի 135 նիշ:
                    result.Add(new ActionError(202));
                }

                if (this.DetailsOfCharges != "OUR")
                {
                    //ՌԴ ռուբլով փոխանցումները կատարվում են միայն OUR գանձման տարբերակով:
                    result.Add(new ActionError(203));
                }
            }

            if (string.IsNullOrEmpty(this.Country) || this.Country == "0")
            {
                //Երկիր դաշտը ընտրված չէ:
                result.Add(new ActionError(81));
            }

            if (templateType == TemplateType.CreatedAsGroupService || templateType == TemplateType.None)
            {



                if (string.IsNullOrEmpty(this.Sender))
                {
                    //Ուղարկողի անվանումը բացակայում է:
                    result.Add(new ActionError(104));
                }
                else if (Utility.IsExistForbiddenCharacter(this.Sender))
                {
                    //Ուղարկողի անվանում դաշտի մեջ կա անթույլատրելի նշան`  
                    result.Add(new ActionError(593, new string[] { "" }));
                }
                else
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                    {
                        string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.Sender);

                        if (!String.IsNullOrEmpty(checkSymbols))
                        {
                            //Ուղարկողի անվանում դաշտի մեջ կա անթույլատրելի նշան`  
                            result.Add(new ActionError(593, new string[] { checkSymbols }));
                        }
                    }
                }

                if (string.IsNullOrEmpty(this.SenderAddress))
                {
                    //Ուղարկողի հասցեն բացակայում է:
                    result.Add(new ActionError(105));
                }
                if (Utility.IsExistForbiddenCharacter(this.SenderAddress))
                {
                    //Ուղարկողի հասցե դաշտի մեջ կա անթույլատրելի նշան`  
                    result.Add(new ActionError(594, new string[] { "" }));
                }
                else
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                    {
                        string checkSymbols = Utility.CheckTextForUnpermittedSymbols(this.SenderAddress);

                        if (!String.IsNullOrEmpty(checkSymbols))
                        {
                            //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                            result.Add(new ActionError(594, new string[] { checkSymbols }));
                        }
                    }
                }
                if (string.IsNullOrEmpty(this.SenderPhone))
                {
                    //Ուղարկողի հեռախոսը բացակայում է:
                    result.Add(new ActionError(136));
                }


                if (this.SenderType == (short)CustomerTypes.physical)
                {
                    if (string.IsNullOrEmpty(this.SenderDateOfBirth.ToString()))
                    {
                        //Ուղարկողի ծննդյան ամսաթիվը բացակայում է:
                        result.Add(new ActionError(108));
                    }
                    if (string.IsNullOrEmpty(this.SenderPassport))
                    {
                        //Ուղարկողի անձնագիրը բացակայում է:
                        result.Add(new ActionError(107));
                    }
                    else if (this.Sender.Length + ",Adr. ".Length + this.SenderAddress.Length + ",Pass. ".Length + this.SenderPassport.Length + ",d/b. ".Length + this.SenderDateOfBirth.ToString().Length > 140)
                    {
                        //Ուղարկողի տվյալները առավելագույնը 140 նիշ է:
                        result.Add(new ActionError(137));
                    }

            }
            else
            {
                var customer = ACBAOperationService.GetCustomer(this.CustomerNumber);

                    if (string.IsNullOrEmpty(this.SenderCodeOfTax))
                    {
                        //Ուղարկողի ՀՎՀՀ-ն բացակայում է:
                        result.Add(new ActionError(106));
                    }

                    if (this.SenderCodeOfTax.Length != 8 && customer.residence.key == 1)
                    {
                        //Ուղարկողի ՀՎՀՀ-ն պետք է լինի 8 նիշ:
                        result.Add(new ActionError(113));
                    }
                    if (this.Sender.Length + ",Adr. ".Length + this.SenderAddress.Length + ",Tax code. ".Length + this.SenderCodeOfTax.Length > 140)
                    {
                        //Ուղարկողի տվյալները առավելագույնը 140 նիշ է:
                        result.Add(new ActionError(137));
                    }

                }
            }

            if (this.DetailsOfCharges == "OUROUR" && this.Currency != "USD")
            {
                //OUROUR փոխանցման համար թույլատրելի արժույթն է ԱՄՆ դոլար
                result.Add(new ActionError(117));
            }

            if (this.Amount != 0 && this.UrgentSign && !(((this.Currency == "USD" || this.Currency == "EUR") && this.Amount < 500000) || (this.Currency == "RUR" && this.Amount < 10000000)))
            {
                //Հնարավոր չէ կատարել շտապ փոխանցում ընտրված արժույթի գումարի համար:
                result.Add(new ActionError(610));
            }

            //Ռիսկային երկրների ստուգում
            if (this.Country == "364" || this.Country == "760" || this.Country == "729" || this.Country == "728")
            {
                //Ստացողի երկիրը ռիսկային է (ԻՐԱՆ, ՍԻՐԻԱ, ՍՈՒԴԱՆ, ՀԱՐԱՎԱՅԻՆ ՍՈՒԴԱՆ)
                result.Add(new ActionError(626));
            }
            if (!string.IsNullOrEmpty(this.ReceiverBankSwift) && this.ReceiverBankSwift.Length == 11 && this.Currency != "RUR" && (this.ReceiverBankSwift.Substring(5, 2) == "IR" || this.ReceiverBankSwift.Substring(5, 2) == "SY" || this.ReceiverBankSwift.Substring(5, 2) == "SD" || this.ReceiverBankSwift.Substring(5, 2) == "SS"))
            {
                //Ստացող բանկի երկիրը ռիսկային է (ԻՐԱՆ, ՍԻՐԻԱ, ՍՈՒԴԱՆ, ՀԱՐԱՎԱՅԻՆ ՍՈՒԴԱՆ)
                result.Add(new ActionError(627));
            }
            if (!string.IsNullOrEmpty(this.IntermediaryBankSwift) && this.IntermediaryBankSwift.Length == 11 && this.Currency != "RUR" && (this.IntermediaryBankSwift.Substring(5, 2) == "IR" || this.IntermediaryBankSwift.Substring(5, 2) == "SY" || this.IntermediaryBankSwift.Substring(5, 2) == "SD" || this.IntermediaryBankSwift.Substring(5, 2) == "SS"))
            {
                //Միջնորդ բանկի երկիրը ռիսկային է (ԻՐԱՆ, ՍԻՐԻԱ, ՍՈՒԴԱՆ, ՀԱՐԱՎԱՅԻՆ ՍՈՒԴԱՆ)
                result.Add(new ActionError(628));
            }
            if (this.Type == OrderType.CashInternationalTransfer && this.DebitAccount.Currency != this.Currency)
            {
                //Դեբետագրվող արժույթը տարբերվում է գործարքի արժույթից
                result.Add(new ActionError(640));
            }

            if (this.DebitAccount.Currency != "AMD" && this.Type != OrderType.CashInternationalTransfer && this.DebitAccount.Currency != this.Currency)
            {
                ushort crossVariant = CurrencyExchangeOrder.GetCrossConvertationVariant(this.DebitAccount.Currency, this.Currency);
                if (crossVariant == 0)
                {
                    //Տվյալ զույգ արժույթների համար փոխարկում չի նախատեսված:
                    result.Add(new ActionError(658));
                }

            }

            if (this.Type == OrderType.CashInternationalTransfer &&
                                ((this.Currency == "USD" && this.Amount > 3000) ||
                                                                       (this.Currency != "USD" && (this.Amount * Utility.GetLastCBExchangeRate(this.Currency)) / Utility.GetLastCBExchangeRate("USD") > 3000)))
            {
                //Կանխիկ փոխանցման գումարը չի կարող գերազանցել 3000USD
                result.Add(new ActionError(713));
            }

            if (this.Type == OrderType.CashInternationalTransfer && this.OPPerson != null)
            {
                if (this.OPPerson.CustomerNumber != this.CustomerNumber)
                {
                    //Որպես գործարք կարատող անձ ընտրված է այլ հաճախորդ
                    result.Add(new ActionError(714));
                }
            }

            if (Account.IsUserAccounts(user.userCustomerNumber, this.DebitAccount.AccountNumber, "0"))
            {
                //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                result.Add(new ActionError(544));
            }

            if (InternationalPaymentOrderDB.CheckIBANCodeLength(this.ReceiverAccount.AccountNumber) != 1)
            {
                //Ստացողի հաշվի երկարությունը չի համապատասխանում IBAN ֆորմատով հաշվեհամարի երկարությանը
                result.Add(new ActionError(1279));
            }

            if (this.Country == "840" && this.Currency == "USD")
            {
                if ((!String.IsNullOrEmpty(this.ReceiverBankSwift) && !String.IsNullOrEmpty(this.FedwireRoutingCode)) || (String.IsNullOrEmpty(this.ReceiverBankSwift) && String.IsNullOrEmpty(this.FedwireRoutingCode)))
                {
                    //Լրացրեք ստացող բանկի SWIFT կամ Fedwire Routing կոդերից  մեկը
                    result.Add(new ActionError(1282));
                }
                else if (!string.IsNullOrEmpty(this.FedwireRoutingCode))
                {
                    isNumeric = Double.TryParse(String.IsNullOrEmpty(this.FedwireRoutingCode) ? "" : this.FedwireRoutingCode.Trim(), out output);
                    if (this.FedwireRoutingCode.Length != 9)
                    {
                        //Fedwire Routing դաշտը պետք է լինի 9 նիշ:
                        result.Add(new ActionError(1283));
                    }
                    else if (!isNumeric)
                    {
                        // դաշտը պետք է լինի միայն թվանշաններ:
                        result.Add(new ActionError(205, new string[] { "Ստացող բանկի Fedwire Routing կոդ" }));
                    }
                }
            }
            else if (!String.IsNullOrEmpty(this.FedwireRoutingCode))
            {
                //Fedwire Routing կոդը, լրացվում է միայն ԱՄՆ դոլարով դեպի ԱՄՆ փոխանցման դեպքում 
                result.Add(new ActionError(1287));
            }

            if (this.SwiftMessageID != 0)
            {
                if (Validation.IsSwiftMessageConfirmed(this.SwiftMessageID))
                {
                    //Տվյալ SWIFT հաղորդագրության հաստատման հայտ արդեն գոյություն ունի
                    result.Add(new ActionError(1450));
                }
            }

            return result;
        }



        /// <summary>
        /// Վճարման հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    OrderQuality nextQuality;

                    if (this.Source == SourceType.Bank || this.Source == SourceType.PhoneBanking)
                    {
                        nextQuality = GetNextQuality(schemaType);
                    }
                    else
                    {
                        nextQuality = GetNextQualityByStep();
                    }

                    result = base.Approve(schemaType, userName);
                                       
                    if (result.ResultCode == ResultCode.Normal)
                    {
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


        public static string GetInternationalTransferSentTime(int docID)
        {
            return InternationalPaymentOrderDB.GetInternationalTransferSentTime(docID);
        }

        public static InternationalPaymentOrder GetCustomerDateForInternationalPayment(ulong customerNumber)
        {
            return InternationalPaymentOrderDB.GetCustomerDateForInternationalPayment(customerNumber);
        }

        private void InitCustomer()
        {
           
            short customerType = (short)Customer.GetCustomerType(CustomerNumber);

            this.SenderType = customerType;// customerData.customerType.key;
            
            //List<CustomerAddress> addressList;

            //if (SenderType == 6)
            //{
            //    Person person = ((customerData as PhysicalCustomer).person as Person);
            //    PhysicalCustomer physicalCustomer = customerData as PhysicalCustomer;
            //    this.Sender = person.fullName.firstNameEng + " " + person.fullName.lastNameEng;
            //    CustomerDocument pass = person.documentList.Find(cd => cd.documentNumber == physicalCustomer.DefaultDocument && cd.defaultSign);
            //    this.SenderPassport = physicalCustomer.DefaultDocument + ", " + pass.givenBy + ", " + String.Format("{0:dd/MM/yy}", pass.givenDate);
            //    this.SenderDateOfBirth = person.birthDate != null? (DateTime)person.birthDate : default(DateTime);
            //    this.SenderEmail = person.emailList != null && person.emailList.Count > 0 ? person.emailList[0].email.emailAddress : null;
            //    addressList = person.addressList;
            //}
            //else
            //{
            //    LegalCustomer legalCustomer = customerData as LegalCustomer;
            //    Organisation organisation = legalCustomer.Organisation;
            //    this.Sender = legalCustomer.customerType.key == 2 ? "Private entrepreneur " + organisation.DescriptionEnglish : organisation.DescriptionEnglish;
            //    this.SenderCodeOfTax = Utility.ConvertAnsiToUnicode(legalCustomer.CodeOfTax);
            //    this.SenderEmail = organisation.emailList != null && organisation.emailList.Count > 0 ? organisation.emailList[0].email.emailAddress : null;
            //    addressList = organisation.addressList;
            //}
            //CustomerAddress defaultAddress = addressList.Find(m => m.priority.key == 1);
            //this.SenderCountry = defaultAddress.address.Country.value;

        }
    }
}
