using ExternalBanking.DBManager;
using ExternalBanking.SberTransfers.Order;
using ExternalBanking.UtilityPaymentsManagment;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Արտարժույթի փոխանակման հայտ
    /// </summary>
    public class CurrencyExchangeOrder : PaymentOrder
    {
        /// <summary>
        /// Գումարը ՀՀ դրամով
        /// </summary>
        public double AmountInAmd { get; set; }

        /// <summary>
        /// Կրկնակի փոխարկման դեպքում գումարը հակառակ արժույթով
        /// </summary>
        public double AmountInCrossCurrency { get; set; }

        /// <summary>
        /// Դրամով վերադարձվող մանրադրամ
        /// </summary>
        public double ShortChange { get; set; }


        /// <summary>
        /// Կլորացման ուղղության նշում
        /// </summary>
        public ExchangeRoundingDirectionType RoundingDirection { get; set; }

        /// <summary>
        /// Կանխիկ մութքի հայտի համար
        /// </summary>
        public string OrderNumberForDebet { get; set; }
        /// <summary>
        /// Կանխիկ ելքի հայտի համար
        /// </summary>
        public string OrderNumberForCredit { get; set; }
        /// <summary>
        /// Կանխիկ վերադարցվող մանրադրամի հայտի համար
        /// </summary>
        public string OrderNumberForShortChange { get; set; }
        /// <summary>
        /// Անձնագրով թե առանց անձնագրի
        /// </summary>
        public bool HasPassport { get; set; }


        /// <summary>
        /// Շեղված փոխարժեքով գործարքի նշում: 
        /// </summary>
        public ExchangeRateVariationType IsVariation { get; set; }

        /// <summary>
        /// Փոխարկման ունիկալ համար, փոխարկումներում
        /// </summary>
        public ulong UniqueNumber { get; set; }


        public CurrencyExchangeOrder()
        { }
        public CurrencyExchangeOrder(InternationalPaymentOrder internationalOrder)
        {
            this.Id = internationalOrder.Id;
            this.Currency = internationalOrder.Currency;
            this.DebitAccount = internationalOrder.DebitAccount;
            this.ReceiverAccount = new Account();
            this.ReceiverAccount.Currency = internationalOrder.Currency;
            this.ConvertationRate = internationalOrder.ConvertationRate;
            this.ConvertationRate1 = internationalOrder.ConvertationRate1;
            this.Type = OrderType.InternationalTransfer;
            this.FilialCode = internationalOrder.FilialCode;
            this.Amount = internationalOrder.Amount;
            this.OPPerson = internationalOrder.OPPerson;
            this.UniqueNumber = internationalOrder.UniqueNumber;
            if (this.DebitAccount.Currency != this.ReceiverAccount.Currency)
            {
                GetVariation();

                if (this.DebitAccount.Currency != "AMD")
                {

                    ushort crossVariant = GetCrossConvertationVariant(internationalOrder.DebitAccount.Currency, internationalOrder.Currency);
                    this.AmountInCrossCurrency = Math.Round(this.Amount * (this.ConvertationRate1 / this.ConvertationRate), 2);

                    if (crossVariant == 1)
                    {
                        this.ConvertationCrossRate = Math.Round(this.ConvertationRate / this.ConvertationRate1, 7);
                    }
                    else
                    {
                        this.ConvertationCrossRate = Math.Round(this.ConvertationRate1 / this.ConvertationRate, 7);
                    }


                }
            }
            else
            {
                this.IsVariation = ExchangeRateVariationType.None;
            }
        }

        public CurrencyExchangeOrder(PaymentOrder paymentOrder)
        {
            this.Id = paymentOrder.Id;
            this.Currency = paymentOrder.Currency;
            this.DebitAccount = paymentOrder.DebitAccount;
            this.ReceiverAccount = new Account();
            this.ReceiverAccount = paymentOrder.ReceiverAccount;
            this.ReceiverAccount.Currency = paymentOrder.ReceiverAccount.Currency;
            this.ConvertationRate = paymentOrder.ConvertationRate;
            this.ConvertationRate1 = paymentOrder.ConvertationRate1;
            this.Type = OrderType.Convertation;
            this.FilialCode = paymentOrder.FilialCode;
            this.ForDillingApprovemnt = paymentOrder.ForDillingApprovemnt;
            this.RegistrationDate = paymentOrder.RegistrationDate;
            this.RegistrationTime = paymentOrder.RegistrationTime;
            if (this.Currency == "AMD")
            {
                this.Amount = paymentOrder.Amount;
                this.AmountInAmd = 0;
            }
            else
            {
                this.AmountInAmd = Utility.RoundAmount(paymentOrder.Amount * Utility.GetCBKursForDate(Convert.ToDateTime(paymentOrder.OperationDate), paymentOrder.Currency), paymentOrder.Currency);
                this.Amount = paymentOrder.Amount;
            }
            this.Amount = paymentOrder.Amount;
            this.OPPerson = paymentOrder.OPPerson;
            this.Source = paymentOrder.Source;
            this.user = paymentOrder.user;
            this.OperationDate = paymentOrder.OperationDate;
            if (Customer.GetCustomerType(paymentOrder.CustomerNumber) == 6)
            {
                this.OPPerson.PersonName = Utility.GetCustomerDescription(paymentOrder.CustomerNumber);
            }
            this.UniqueNumber = Utility.GetLastKeyNumber(4, FilialCode);
            this.CustomerNumber = paymentOrder.CustomerNumber;

            if (this.DebitAccount.Currency != this.ReceiverAccount.Currency)
            {
                GetVariation();

                if (this.DebitAccount.Currency != "AMD")
                {

                    ushort crossVariant = GetCrossConvertationVariant(paymentOrder.DebitAccount.Currency, paymentOrder.ReceiverAccount.Currency);
                    this.AmountInCrossCurrency = Math.Round(this.Amount * (this.ConvertationRate / this.ConvertationRate1), 2);

                    if (crossVariant == 1)
                    {
                        this.ConvertationCrossRate = Math.Round(this.ConvertationRate / this.ConvertationRate1, 7);
                    }
                    else
                    {
                        this.ConvertationCrossRate = Math.Round(this.ConvertationRate1 / this.ConvertationRate, 7);
                    }


                }
            }
            else
            {
                this.IsVariation = ExchangeRateVariationType.None;
            }
        }

        public CurrencyExchangeOrder(STAKPaymentOrder stakOrder)
        {
            this.Id = stakOrder.Id;
            this.Currency = stakOrder.Currency;
            this.DebitAccount = stakOrder.DebitAccount;
            this.ReceiverAccount = new Account();
            this.ReceiverAccount = stakOrder.ReceiverAccount;
            //this.ReceiverAccount.Currency = STAKPayment.ReceiverAccount.Currency;
            this.OrderNumber = stakOrder.OrderNumber;
            this.TransferID = stakOrder.TransferID;
            this.Amount = stakOrder.Amount;
            this.CustomerNumber = stakOrder.CustomerNumber;
            this.Type = stakOrder.Type;   //   OrderType.TransitNonCashOutCurrencyExchangeOrder;
            this.SubType = stakOrder.SubType;
            this.Description = stakOrder.Description;
            this.ReceiverBankCode = stakOrder.ReceiverBankCode;
            this.RoundingDirection = ExchangeRoundingDirectionType.ToCurrency;    //   "1" 
            this.Quality = stakOrder.Quality;
            this.OPPerson = stakOrder.OPPerson;
            this.Source = stakOrder.Source;
            this.OperationDate = stakOrder.OperationDate;
            this.FilialCode = stakOrder.FilialCode;
            this.RegistrationDate = stakOrder.RegistrationDate;
            this.user = stakOrder.user;

            this.UniqueNumber = Utility.GetLastKeyNumber(4, FilialCode);


            //this.ForDillingApprovemnt = STAKPayment.ForDillingApprovemnt;
            //this.RegistrationTime = STAKPayment.RegistrationTime;          

        }
        public CurrencyExchangeOrder(SberIncomingTransferOrder order, Transfer transfer, Customer customer)
        {
            Quality = OrderQuality.Draft;
            OperationDate = order.OperationDate;

            OPPerson = SetOrderOPPerson(order.CustomerNumber);

            FilialCode = 22000;
            CustomerNumber = order.CustomerNumber;
            RegistrationDate = order.RegistrationDate;
            Source = order.Source;
            user = order.user;
            OnlySaveAndApprove = false;
            HasPassport = true;
            TransferID = transfer.Id;

            Amount = transfer.Amount;

            Currency = order.Currency;

            DebitAccount = new Account();
            DebitAccount = transfer.DebitAccount;

            ReceiverAccount = new Account();
            ReceiverAccount = transfer.CreditAccount;
            ReceiverBankCode = Convert.ToInt32(ReceiverAccount.AccountNumber.Substring(0, 5));

            if (ReceiverAccount.Currency != "AMD")
            {
                ConvertationRate = (double)order.CurrencyRateCrossBuy; //RUR->AMD
                ConvertationRate1 = (double)order.CurrencyRateCrossSell; //AMD->USD/EUR
                ConvertationCrossRate = (double)order.CurrencyCrossRateFull; //RUR->USD/EUR
                RoundingDirection = ExchangeRoundingDirectionType.ToCurrency; //1 Արժույթի նկատմամբ
                AmountInCrossCurrency = (double)order.CreditAmount;
                AmountInAmd = (double)order.AmountAMD;
            }
            else
            {
                AmountInAmd = (double)order.CreditAmount;
                this.ConvertationRate = (double)order.CurrencyRateCrossBuy;
                RoundingDirection = ExchangeRoundingDirectionType.ToAMD; //2 ՀՀ Դրամի նկատմամբ
            }

            Type = OrderType.TransitNonCashOutCurrencyExchangeOrder; //82 Տարանցիկ հաշվից անկանխիկ ելք փոխարկումով
            SetOrderSubTypeCurrencyExchange(DebitAccount.Currency, ReceiverAccount.Currency);

            OrderNumberTypes orderNumberTypes = OrderNumberTypes.Convertation; //7 Փոխանակում
            OrderNumber = Convert.ToString(GenerateNewOrderNumber(orderNumberTypes, customer.User.filialCode));

            OrderNumberTypes orderNumberTypesCredit = OrderNumberTypes.CashOut; //2 Կանխիկ ելք
            OrderNumberForCredit = Convert.ToString(GenerateNewOrderNumber(orderNumberTypesCredit, customer.User.filialCode));

            OrderNumberTypes orderNumberTypesDebet = OrderNumberTypes.CashIn; //1 Կանխիկ մուտք
            OrderNumberForDebet = Convert.ToString(GenerateNewOrderNumber(orderNumberTypesDebet, customer.User.filialCode));
        }

        /// <summary>
        /// Կրկնակի փոխարկման նշում
        /// </summary>
        public bool IsDoulbeExchange
        {
            get
            {
                if (DebitAccount != null && ReceiverAccount != null)
                {
                    if (DebitAccount.Currency != "AMD" && ReceiverAccount.Currency != "AMD")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }

            }

        }



        /// <summary>
        /// Վերադարձնում է փոխարկման հայտը:
        /// </summary>
        public new void Get()
        {
            base.Get();
            if (CurrencyExchangeOrderDB.GetOrderSourceType(this.Id) != SourceType.AcbaOnline && CurrencyExchangeOrderDB.GetOrderSourceType(this.Id) != SourceType.MobileBanking)
            {
                CurrencyExchangeOrderDB.GetCurrencyExchangeDetails(this);
            }
            else
            {
                CurrencyExchangeOrderDB.GetHBCurrencyExchangeDetails(this);
            }
        }

        public new ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            base.Complete();
            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = PaymentOrderDB.Save(this, userName, source);
                long id = result.Id;
                CurrencyExchangeOrderDB.Save(this, userName, Source);



                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
                else
                {
                    result = base.SaveOrderFee();
                    result.Id = id;
                }



                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;

        }



        public new ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result = base.Validate(user);
            if (result.Errors.Count == 0)
            {
                if ((this.ReceiverAccount?.AccountType == 118 && this.DebitAccount?.AccountNumber != "220004130948000") ||
                (this.ReceiverAccount?.AccountType == 119 && this.Type != OrderType.CashDebitConvertation && this.Type != OrderType.InBankConvertation && this.Type != OrderType.Convertation))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if (this.DebitAccount?.AccountType == 119 && this.Type != OrderType.CashDebitConvertation && this.Type != OrderType.InBankConvertation && this.Type != OrderType.Convertation)
                {
                    //Նշված հաշվից ելքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }


                if (this.RoundingDirection == ExchangeRoundingDirectionType.ToCurrency)
                {
                    if (this.Source == SourceType.SSTerminal || this.Source == SourceType.CashInTerminal)
                    {
                        if (Math.Abs(Utility.RoundAmount((double)((decimal)this.Amount * (decimal)this.ConvertationRate), "AMD") - this.AmountInAmd) >= 10)
                        {
                            //Դրամային գումարը սխալ է:
                            result.Errors.Add(new ActionError(781));
                        }
                    }
                    else
                    {
                        if (Utility.RoundAmount((double)((decimal)this.Amount * (decimal)this.ConvertationRate), "AMD") != this.AmountInAmd)
                        {
                            //Դրամային գումարը սխալ է:
                            result.Errors.Add(new ActionError(781));
                        }
                    }
                }
                else if (this.RoundingDirection == ExchangeRoundingDirectionType.ToAMD)
                {
                    if (Utility.RoundAmount(this.AmountInAmd / this.ConvertationRate, this.Currency) != this.Amount)
                    {
                        //Արժույթի գումարը սխալ է:
                        result.Errors.Add(new ActionError(781));
                    }
                }

                if (this.TransferID != 0)
                {
                    Transfer transfer = new Transfer();
                    transfer.Id = this.TransferID;
                    transfer.Get();
                    if (this.Amount >= (transfer.Currency == "EUR" ? 5 : 1) && transfer.TransferGroup == 4)
                    {
                        //Փոխարկումով հնարավոր է վճարել միայն փոխանցման գումարի մանրը
                        result.Errors.Add(new ActionError(911));
                    }
                    if (transfer.TransferGroup == 4 && transfer.PaidAmount == 0)
                    {
                        //Նախ կատարեք գումարի ամբողջ մասի վճարումը
                        result.Errors.Add(new ActionError(912));
                    }

                }

                if (DebitAccount.Currency != "AMD" && ReceiverAccount.Currency != "AMD")
                {

                    if ((this.Type == OrderType.CashConvertation || this.Type == OrderType.CashCreditConvertation) && !Validation.IsCurrencyAmountCorrect(this.AmountInCrossCurrency, this.ReceiverAccount.Currency))
                    {
                        //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                        result.Errors.Add(new ActionError(1053, new string[] { this.ReceiverAccount.Currency }));
                    }
                    if ((this.Type == OrderType.CashConvertation || this.Type == OrderType.CashDebitConvertation
                        || this.Type == OrderType.CashTransitCurrencyExchangeOrder) && !Validation.IsCurrencyAmountCorrect(this.Amount, this.DebitAccount.Currency))
                    {
                        //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                        result.Errors.Add(new ActionError(1053, new string[] { this.DebitAccount.Currency }));
                    }

                }
                else
                {

                    if (this.ReceiverAccount.Currency != "AMD" && (this.Type == OrderType.CashConvertation || this.Type == OrderType.CashCreditConvertation) && !Validation.IsCurrencyAmountCorrect(this.Amount, this.ReceiverAccount.Currency))
                    {
                        //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                        result.Errors.Add(new ActionError(1053, new string[] { this.ReceiverAccount.Currency }));
                    }
                    if (this.DebitAccount.Currency != "AMD" && (this.Type == OrderType.CashConvertation || this.Type == OrderType.CashDebitConvertation
                        || this.Type == OrderType.CashTransitCurrencyExchangeOrder) && !Validation.IsCurrencyAmountCorrect(this.Amount, this.DebitAccount.Currency))
                    {
                        //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                        result.Errors.Add(new ActionError(1053, new string[] { this.DebitAccount.Currency }));
                    }

                }

                if (this.Type != OrderType.CashConvertation && this.Type != OrderType.CashCreditConvertation &&
                    this.Type != OrderType.TransitCashOutCurrencyExchangeOrder && this.ReceiverAccount.IsDepositAccount())
                {

                    ActionError err = new ActionError();
                    if (!IsDoulbeExchange)
                    {
                        err = Deposit.IsAllowedAmountAddition(this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber, this.AmountInAmd, this.Amount, this.Source);
                    }
                    else
                    {
                        err = Deposit.IsAllowedAmountAddition(this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber, this.AmountInAmd, this.AmountInCrossCurrency, this.Source);
                    }
                    if (err.Code != 0)
                    {
                        result.Errors.Add(err);
                    }
                }

                if (this.IsDoulbeExchange)
                {
                    bool isCallTransferPayment = false;
                    if (this.TransferID != 0)
                    {
                        Transfer transfer = new Transfer();
                        transfer.Id = this.TransferID;
                        transfer.Get();

                        if (transfer.AddTableName == "Tbl_transfers_by_call")
                            isCallTransferPayment = true;


                    }

                    double rate = default;

                    if (this.Source == SourceType.STAK && this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                    {
                        rate = Utility.GetLastExchangeRate(this.DebitAccount.Currency, RateType.Cross, ExchangeDirection.Buy, this.FilialCode);
                    }
                    else
                    {
                        rate = Utility.GetLastExchangeRate(this.DebitAccount.Currency, RateType.Cross, ExchangeDirection.Buy, user.filialCode);
                    }

                    if (rate != this.ConvertationRate && !isCallTransferPayment && Source != SourceType.SSTerminal && Source != SourceType.CashInTerminal)
                    {
                        //Տեղի է ունեցել փոխարժեքների փոփոխություն:Խնդրում ենք նորից մուտքագրել գործարքը:
                        result.Errors.Add(new ActionError(1095));
                    }
                }

            }
            if (Source == SourceType.SSTerminal && base.IsPaymentIdUnique())
            {
                result.Errors.Add(new ActionError(1498));
            }
            return result;
        }

        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = this.ValidateForSend(user);

            List<ActionError> warnings = new List<ActionError>();

            if (result.ResultCode == ResultCode.Normal)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {

                    result = base.Approve(schemaType, userName);


                    if (result.ResultCode == ResultCode.Normal)
                    {
                        warnings.AddRange(base.GetActionResultWarnings(result));

                        this.Quality = OrderQuality.Sent3;
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                    else
                    {
                        result.ResultCode = ResultCode.Failed;
                        return result;
                    }
                }
            }

            result = base.Confirm(user);
            warnings.AddRange(base.GetActionResultWarnings(result));
            result.Errors = warnings;

            return result;


        }


        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            Complete();
            ActionResult result = new ActionResult();

            List<ActionError> warnings = new List<ActionError>();

            if (source != SourceType.SberBankTransfer)
            {
                result = this.Validate(user);

                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }

                result = this.ValidateForSend(user);
                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                if (this.Type == OrderType.CashConvertation ||
                    this.Type == OrderType.CashCredit ||
                    this.Type == OrderType.CashCreditConvertation ||
                    this.Type == OrderType.CashDebit ||
                    this.Type == OrderType.CashDebitConvertation ||
                    this.Type == OrderType.CashForRATransfer ||
                    this.Type == OrderType.TransitCashOutCurrencyExchangeOrder ||
                    this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                    )
                {
                    result = PaymentOrderDB.SaveCash(this, userName, source);
                }
                else
                {
                    result = PaymentOrderDB.Save(this, userName, source);

                }
                if (source == SourceType.Bank || ((source == SourceType.MobileBanking || source == SourceType.AcbaOnline) && bool.Parse(ConfigurationManager.AppSettings["TransactionTypeByAMLForMobile"].ToString())))
                {
                    result = base.SaveTransactionTypeByAML(this);
                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                CurrencyExchangeOrderDB.Save(this, userName, Source);
                if (this.AdditionalParametrs != null && this.AdditionalParametrs.Exists(m => m.AdditionValue == "LeasingAccount"))
                {
                    LeasingDB.SaveLeasingPaymentDetails(this);
                }

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

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                //Վիվասել ՄՏՍ ընթացիկ հաշվին կատարված փոխանցում
                if (this.ReceiverAccount != null && (this.Type == OrderType.Convertation || this.Type == OrderType.CashDebitConvertation
                || this.Type == OrderType.CashConvertation || this.Type == OrderType.CashDebit || this.Type == OrderType.InBankConvertation)
                                && this.ReceiverAccount.AccountNumber == Constants.VIVACELL_PAYMENT_BY_TRANSFER_ACCOUNT_NUMBER)
                {

                    DateTime currentOperDay = Utility.GetNextOperDay().Date;
                    if (currentOperDay == DateTime.Now.Date)
                    {
                        VivaCellBTFCheckDetails vivaCellBTFCheck = new VivaCellBTFCheckDetails();
                        DateTime actionStartDate = DateTime.Now;
                        vivaCellBTFCheck.CheckTransferPossibility(Description, Amount);

                        if (vivaCellBTFCheck.PaymentTransactionID != null)
                        {
                            result = SaveUtilityTransferDetails(vivaCellBTFCheck.PaymentTransactionID, this.Id, CommunalTypes.VivaCell, vivaCellBTFCheck.PaymentTransactionDateTime != null ? vivaCellBTFCheck.PaymentTransactionDateTimeFormated : null, actionStartDate);
                        }
                    }

                }


                ////////////////////////////////////////////////////////////////////////////////​
                short amlResult = 0;
                //Բանկի ներսում հաշիվների միջև փոխանցման և կոնվերտացիայի դեպքում ստուգում ենք ԱՄԼ ստուգման անհրաժեշտությունը
                if (this.Type == OrderType.RATransfer && this.SubType == 1 || this.Type == OrderType.InBankConvertation)
                {
                    amlResult = PaymentOrderDB.MatchAMLConditions(this);
                    if (amlResult != 1)
                    {
                        ulong receiver_customer_number = this.ReceiverAccount.GetAccountCustomerNumber();
                        amlResult = PaymentOrderDB.MatchAMLConditions(this, receiver_customer_number);
                    }
                    //եթե կա ԱՄԼ ստուգման անհրաժեշտություն, պահպանում ենք փոխանցման տվյալները ԱՄԼ ծրագրում ցուցադրելու համար
                    if (amlResult == 1)
                    {
                        PaymentOrderDB.AddInBankTransferDetailsForAML(this, amlResult);

                    }
                }
                result = base.Approve(schemaType, userName, amlResult);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }
            }



            result = base.Confirm(user);
            warnings.AddRange(base.GetActionResultWarnings(result));
            result.Errors = warnings;

            return result;
        }

        public new void Complete()
        {

            if (this.ForThirdPerson)
            {
                if (this.OPPerson != null)
                    this.CustomerNumber = this.OPPerson.CustomerNumber;
            }


            base.Complete();


            if (!(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking))
            {
                if (Source != SourceType.SSTerminal && Source != SourceType.CashInTerminal && Source != SourceType.SberBankTransfer)
                {
                    GetVariation();
                }

                if (this.TransferID != 0)
                {
                    Transfer transfer = new Transfer();
                    transfer.Id = this.TransferID;
                    transfer.Get();

                    if (transfer.AddTableName == "Tbl_transfers_by_call"
                            && (((this.ConvertationRate1 == 0 && (this.ConvertationRate == transfer.RateBuy || this.ConvertationRate == transfer.RateSell))) || (this.ConvertationRate1 != 0 && this.ConvertationRate1 == transfer.RateSell)))
                    {
                        IsVariation = ExchangeRateVariationType.None;
                    }


                }

                if (IsVariation != ExchangeRateVariationType.None || this.SendDillingConfirm)
                {
                    this.UniqueNumber = Utility.GetLastKeyNumber(4, FilialCode);
                }
            }



        }

        private void GetVariation()
        {
            if (DebitAccount.Currency != "AMD" && ReceiverAccount.Currency != "AMD")
                SetVariationForDoubleConvertation();
            else
                SetVariationForSimpleConvertation();
        }

        /// <summary>
        /// Ստուգում է փոխարկման շեղումով լինելը:
        /// </summary>
        private void SetVariationForSimpleConvertation()
        {
            string currency;
            ExchangeDirection direction;
            if (DebitAccount.Currency == "AMD")
            {
                direction = ExchangeDirection.Sell;
                currency = ReceiverAccount.Currency;
            }
            else
            {
                direction = ExchangeDirection.Buy;
                currency = DebitAccount.Currency;
            }



            RateType rateType = RateType.Cash;

            switch (Type)
            {
                case OrderType.Convertation:
                case OrderType.NonCashTransitCurrencyExchangeOrder:
                    rateType = RateType.NonCash;
                    break;
                case OrderType.InternationalTransfer:
                    rateType = RateType.NonCash;
                    break;
                case OrderType.CashCreditConvertation:
                    if (direction == ExchangeDirection.Sell)
                        rateType = RateType.Cash;
                    else
                        rateType = RateType.NonCash;
                    break;
                case OrderType.CashDebitConvertation:
                    if (direction == ExchangeDirection.Sell)
                        rateType = RateType.NonCash;
                    else
                        rateType = RateType.Cash;
                    break;
                case OrderType.TransitNonCashOutCurrencyExchangeOrder:
                    rateType = RateType.NonCash;
                    break;
                case OrderType.TransitCashOutCurrencyExchangeOrder:
                    rateType = RateType.NonCash;
                    break;
                case OrderType.CashTransitCurrencyExchangeOrder:
                    rateType = RateType.NonCash;
                    break;
            }



            double headOfficeRate = Utility.GetLastExchangeRate(currency, rateType, direction);

            if ((direction == ExchangeDirection.Sell && ConvertationRate < headOfficeRate) ||
                (direction == ExchangeDirection.Buy && ConvertationRate > headOfficeRate))
            {
                IsVariation = ExchangeRateVariationType.DillingVariation;
            }
            else
            {
                double branchRate = Utility.GetLastExchangeRate(currency, rateType, direction, FilialCode);

                if (ConvertationRate != branchRate && this.user.TransactionLimit != -1)//Փոխվել է 17/02/2017 համաձայնեցվել է Անի Միրզոյանի հետ
                {
                    IsVariation = ExchangeRateVariationType.BranchVariation;
                }
                else
                    IsVariation = ExchangeRateVariationType.None;
            }



        }

        private void SetVariationForDoubleConvertation()
        {
            double headOfficeRate = Utility.GetLastExchangeRate(ReceiverAccount.Currency, RateType.Cross, ExchangeDirection.Sell);

            if (ConvertationRate1 < headOfficeRate)
            {
                IsVariation = ExchangeRateVariationType.DillingVariation;
            }
            else
            {
                double branchRate = Utility.GetLastExchangeRate(ReceiverAccount.Currency, RateType.Cross, ExchangeDirection.Sell, FilialCode);
                if (ConvertationRate1 != branchRate && this.user.TransactionLimit != -1)//Փոխվել է 17/02/2017 համաձայնեցվել է Անի Միրզոյանի հետ
                {
                    IsVariation = ExchangeRateVariationType.BranchVariation;
                }
                else
                    IsVariation = ExchangeRateVariationType.None;
            }
        }

        /// <summary>
        /// Վերադարձնում է մանրադրամը դրամով
        /// </summary>
        /// <returns></returns>
        public CurrencyExchangeOrder GetShortChangeAmount()
        {
            decimal shortChangeAmount = (decimal)this.AmountInCrossCurrency % 1;

            int amountInCrossCurrency = (int)this.AmountInCrossCurrency;

            decimal currencyMinAmount = (decimal)Utility.GetCurrencyMinCashAmount(this.ReceiverAccount.Currency);

            shortChangeAmount = (shortChangeAmount + (amountInCrossCurrency % currencyMinAmount)) * (decimal)this.ConvertationRate1;

            this.ShortChange = Math.Round((double)shortChangeAmount, 0);

            this.AmountInCrossCurrency = amountInCrossCurrency - (int)(amountInCrossCurrency % currencyMinAmount);

            return this;


        }

        /// <summary>
        /// Վերադարձնում է կրոսս կոնվերտացիայի տեսակը
        /// </summary>
        /// <param name="debitCurrency"></param>
        /// <param name="creditCurrency"></param>
        /// <returns></returns>
        public static ushort GetCrossConvertationVariant(string debitCurrency, string creditCurrency)
        {
            ushort crossVariant = 0;
            if (debitCurrency == "EUR" && (creditCurrency == "USD" || creditCurrency == "RUR"))
            {
                crossVariant = 1;
            }
            else
                if (debitCurrency == "USD" && creditCurrency == "RUR")
            {
                crossVariant = 1;
            }
            else
                    if (debitCurrency == "GBP" && creditCurrency == "USD")
            {
                crossVariant = 1;
            }
            else
                        if (debitCurrency == "GBP" && creditCurrency == "EUR")
            {
                crossVariant = 2;
            }
            else
                            if (debitCurrency == "CHF" && (creditCurrency == "USD" || creditCurrency == "EUR"))
            {
                crossVariant = 2;
            }
            else
                                if (debitCurrency == "USD" && (creditCurrency == "EUR" || creditCurrency == "GBP"))
            {
                crossVariant = 2;
            }
            else
                                    if (debitCurrency == "USD" && creditCurrency == "CHF")
            {
                crossVariant = 1;
            }
            else
                                        if (debitCurrency == "RUR" && (creditCurrency == "USD" || creditCurrency == "EUR"))
            {
                crossVariant = 2;
            }
            else
                                            if (debitCurrency == "EUR" && (creditCurrency == "GBP" || creditCurrency == "CHF"))
            {
                crossVariant = 1;
            }

            return crossVariant;

        }

        public static Dictionary<string, string> GetOrderDetailsForReport(long orderId, ulong customerNumber)
        {
            return CurrencyExchangeOrderDB.GetOrderDetailsForReport(orderId, customerNumber);
        }


        private void SetDescription(Transfer transfer)
        {
            string strForTransfer;

            if (transfer.TransferGroup != 4)
            {
                strForTransfer = ' ' + OPPerson.PersonName + ' ' + OPPerson.PersonLastName + ' ' + '(' + transfer.TransferSystemDescription + ' ' + transfer.SenderReferance + ')';
            }
            else
                if (transfer.TransferGroup == 4)
            {
                strForTransfer = ' ' + transfer.Receiver + " (" + transfer.UnicNumber + ')';
            }
            else
                strForTransfer = "";


            if (DebitAccount.Currency != "AMD")
            {
                if (ReceiverAccount.Currency != "AMD")
                {
                    //Արտարժույթի առք ու վաճառք
                    Description = "Արտարժույթի վաճառք " + DebitAccount.Currency + "/" + ReceiverAccount.Currency + strForTransfer;
                }
                else
                {
                    //Արտարժույթի առք (Բանկի տեսանկյունից)                          
                    Description = "Արտարժույթի գնում " + DebitAccount.Currency + strForTransfer;

                }
            }
            else
            {
                if (ReceiverAccount.Currency != "AMD")
                {
                    //Արտարժույթի վաճառք (Բանկի տեսանկյունից)
                    Description = "Արտարժույթի վաճառք" + ReceiverAccount.Currency + strForTransfer;
                }
            }
            /*
             if (!$scope.$root.SessionProperties.IsNonCustomerService && $scope.transfer == undefined) {
                        if ($scope.customer.OrganisationName == null)
                            $scope.additional = $scope.additional + " " + $scope.order.OPPerson.PersonName + " " + $scope.order.OPPerson.PersonLastName + (!$scope.order.OPPerson.CustomerNumber ? "" : "(" + $scope.order.OPPerson.CustomerNumber.toString() + ") ");
                        else
                            $scope.additional = $scope.additional + " " + (!$scope.customer.CustomerNumber ? "" : $scope.order.ForThirdPerson == true ? " " : "(" + $scope.customer.CustomerNumber.toString() + ") ");
                    }
             */
        }
        //Ընտրված հաշիվներից կախված նշվում է  արժույթը
        private void SetCurrency(string debitCurrency, string creditCurrency)
        {
            if (debitCurrency != creditCurrency)
            {
                if (debitCurrency == "AMD")
                {
                    if (creditCurrency != "AMD")
                        Currency = creditCurrency;
                    else
                        Currency = "";
                }
                else
                    Currency = debitCurrency;
            }
            else
                Currency = debitCurrency;
        }
        private void SetOrderSubTypeCurrencyExchange(string debitCurrency, string creditCurrency)
        {
            if (debitCurrency == "AMD" && creditCurrency != "AMD")
            {
                this.SubType = 2;
            }
            else if (debitCurrency != "AMD" && creditCurrency == "AMD")
            {
                this.SubType = 1;
            }
            else if (debitCurrency != "AMD" && creditCurrency != "AMD")
            {
                this.SubType = 3;
            }
        }

        public void CalculateConvertationAmount(double Rate, int ConvertationTypeNum, string ConvertationType, ushort crossVariant = 0)
        {
            double AmountConvertation;
            if (Amount != 0 && Rate != 0 && ConvertationType != "")
            {
                if (ConvertationTypeNum == 3)//"Կրկնակի փոխարկում"
                {
                    AmountConvertation = Math.Round(Amount * Rate);

                    if (crossVariant == 1)
                    {
                        AmountInCrossCurrency = Math.Round((Amount * (Rate)), 2);
                    }
                    else
                    {
                        AmountInCrossCurrency = Math.Round((Amount * (1 / (Rate))), 2);
                    }
                    AmountInAmd = Math.Round((Amount * ConvertationRate) + 0.00000001, 1);
                }
                else
                {
                    AmountConvertation = Math.Round((Amount * Rate) * 100) / 100;
                    AmountInAmd = Math.Round((Amount * Rate), 1, MidpointRounding.AwayFromZero);
                }
            }
        }

        private void SetTransferRates(Transfer transfer, string debitCurrency, string creditCurrency)
        {
            double Rate;
            int ConvertationTypeNum;
            string ConvertationType;
            ushort crossVariant;
            if (debitCurrency == "AMD" || creditCurrency == "AMD")
            {
                if (debitCurrency != creditCurrency)
                {
                    if (transfer.RateSell != 0)
                    {
                        Rate = Math.Round(transfer.RateSell * 100) / 100;
                        this.ConvertationRate = Math.Round(transfer.RateSell * 100) / 100;
                        ConvertationTypeNum = 1;
                        ConvertationType = "Վաճառք";//2
                        CalculateConvertationAmount(Rate, ConvertationTypeNum, ConvertationType);
                    }
                    else
                    {
                        Rate = Math.Round(transfer.RateBuy * 100) / 100;
                        this.ConvertationRate = Math.Round(transfer.RateBuy * 100) / 100;
                        ConvertationTypeNum = 2;
                        ConvertationType = "Գնում";//2
                        CalculateConvertationAmount(Rate, ConvertationTypeNum, ConvertationType);
                    }
                }
            }
            else
            {
                if (debitCurrency != creditCurrency)
                {
                    crossVariant = GetCrossConvertationVariant(debitCurrency, creditCurrency);
                    /* if (crossVariant == 0) {
                         return ShowMessage('Տվյալ զույգ արժույթների համար փոխարկում չի նախատեսված:', 'error');
                     }*/
                    this.ConvertationRate = transfer.RateBuy;
                    this.ConvertationRate1 = transfer.RateSell;
                    if (crossVariant == 1)
                    {
                        Rate = this.ConvertationRate / this.ConvertationRate1;
                    }
                    else
                    {
                        Rate = this.ConvertationRate1 / this.ConvertationRate;
                    }
                    Rate = Math.Round((Rate + 0.00000001) * 10000000) / 10000000;

                    ConvertationType = "Կրկնակի փոխարկում";//3
                    ConvertationTypeNum = 3;
                    ConvertationCrossRate = Rate;
                    CalculateConvertationAmount(Rate, ConvertationTypeNum, ConvertationType, crossVariant);

                }

            }

        }

        public void InitOrderForCurrencyPaymentOrder(TransferByCallChangeOrder order, Transfer transfer, Customer customer)
        {
            Quality = OrderQuality.Draft;

            OperationDate = Utility.GetCurrentOperDay();
            FilialCode = order.FilialCode;

            if (order.OPPerson != null)
            {
                OPPerson = order.OPPerson;
                CustomerNumber = order.OPPerson.CustomerNumber;
            }

            RegistrationDate = order.RegistrationDate;

            Source = order.Source;
            user = order.user;

            Amount = transfer.Amount;

            DebitAccount = new Account();
            DebitAccount.AccountNumber = "0";
            DebitAccount.Currency = transfer.DebitAccount.Currency;

            ReceiverAccount = new Account();
            ReceiverAccount = transfer.CreditAccount;

            OnlySaveAndApprove = false;
            //CustomerNumber = transfer.CustomerNumber;
            TransferID = transfer.Id;

            Type = OrderType.TransitNonCashOutCurrencyExchangeOrder; //82 Տարանցիկ հաշվից անկանխիկ ելք փոխարկումով
            SetOrderSubTypeCurrencyExchange(DebitAccount.Currency, ReceiverAccount.Currency);

            OrderNumberTypes orderNumberTypes = OrderNumberTypes.Convertation; //7 Փոխանակում
            OrderNumber = Convert.ToString(GenerateNewOrderNumber(orderNumberTypes, customer.User.filialCode));

            OrderNumberTypes orderNumberTypesCredit = OrderNumberTypes.CashOut; //2 Կանխիկ ելք
            OrderNumberForCredit = Convert.ToString(GenerateNewOrderNumber(orderNumberTypesCredit, customer.User.filialCode));

            OrderNumberTypes orderNumberTypesDebet = OrderNumberTypes.CashIn; //1 Կանխիկ մուտք
            OrderNumberForDebet = Convert.ToString(GenerateNewOrderNumber(orderNumberTypesDebet, customer.User.filialCode));

            ReceiverBankCode = Convert.ToInt32(ReceiverAccount.AccountNumber.Substring(0, 5));
            //currencyorder.Description = "Փոխանցման հաստատում " + transfer.TransferSystemDescription + " " + transfer.SenderReferance;

            HasPassport = true;
            if (ReceiverAccount.Currency != "AMD")
                RoundingDirection = ExchangeRoundingDirectionType.ToCurrency; //1 Արժույթի նկատմամբ
            else
                RoundingDirection = ExchangeRoundingDirectionType.ToAMD; //2 ՀՀ Դրամի նկատմամբ

            SetTransferRates(transfer, DebitAccount.Currency, ReceiverAccount.Currency);
            SetCurrency(DebitAccount.Currency, ReceiverAccount.Currency);
            SetDescription(transfer);

            //Քանի որ էստեղ card fee-ն չի հաշվարկվում, դրա համար getCardFee() function-ը չի կանչվում
            CardFee = 0;
            CardFeeCurrency = null;

        }

    }
}
