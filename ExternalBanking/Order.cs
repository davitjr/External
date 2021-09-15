using System;
using System.Collections.Generic;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Linq;
using ExternalBanking.ServiceClient;
using System.Configuration;
using ExternalBanking.CreditLineActivatorARCA;

namespace ExternalBanking
{
    public class Order
    {
        /// <summary>
        /// Հանձնարարականի ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Հարցման նույնականացուցիչ։ Գեներացվում է վճարումն ընդունող համակարգի կողմից
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Տեսակ 
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// Հանձնարարականի ենթատեսակ
        /// </summary>
        public byte SubType { get; set; }

        /// <summary>
        /// Հանձնարարականի հերթական համար
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Ենթատեսակի նկարագրություն
        /// </summary>
        public string SubTypeDescription { get; set; }

        /// <summary>
        /// Հանձնարարականի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public OrderQuality Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Տվյալների աղբյուր
        /// </summary>
        public SourceType Source { get; set; }

        /// <summary>
        /// Գործարքի կատարման մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; }

        /// <summary>
        /// Գործարքը կատարող անձ
        /// </summary>
        public OPPerson OPPerson { get; set; }

        /// <summary>
        /// Կցված փաստաթղթեր
        /// </summary>
        public List<OrderAttachment> Attachments { get; set; }

        /// <summary>
        /// Հայտի միջնորդավճարներ
        /// </summary>
        public List<OrderFee> Fees { get; set; }

        public User user { get; set; }

        /// <summary>
        /// Գործառնական օրվա ամսաթիվ
        /// </summary>
        public DateTime? OperationDate { get; set; }

        public Account DebitAccount { get; set; }

        /// <summary>
        ///Լրացուցիչ տվյալներ
        /// </summary>
        public List<AdditionalDetails> AdditionalParametrs { get; set; }

        /// <summary>
        /// Ցույց է տալից գործարքը կատարվել է երրորդ անձի համար թե ոչ
        /// </summary>
        public bool ForThirdPerson { get; set; }

        /// <summary>
        /// Վճարման տեսակ 1-Ստանդարտ, 2-Մասնակի, 99-Այլ
        /// </summary>
        public int? PayType { get; set; }

        /// <summary>
        /// Գործարքի օրեկան սահմանաչափ
        /// </summary>
        public double DailyTransactionsLimit = 0;

        /// <summary>
        /// PhoneBanking`Օրական սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double LimitOfDayToOwnAccount = 0;

        /// <summary>
        /// PhoneBanking`Օրական սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double LimitOfDayToAnothersAccount = 0;
        /// <summary>
        /// Վճարային տերմինալի ունիկալ նունականացուցիչ
        /// </summary>
        /// 
        public string TerminalID { get; set; }
        /// <summary>
        /// Վճարման կատարման օր, ժամ
        /// </summary>
        public DateTime? PaymentDateTime { get; set; }
        /// <summary>
        /// Հեռախոսահամար
        /// </summary>
        public String PhoneNumber { get; set; }
        /// <summary>
        /// Գործարքների սեսսիայի համար
        /// </summary>
        public string PaymentSessionID { get; set; }
        /// <summary>
        /// Գործարքի կատարման ամսաթիվ
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }


        /// <summary>
        /// Տարանցիկ հաշվի ստուգման անհրաժեշտություն
        /// </summary>
        public bool ValidateForTransit
        {
            get
            {
                if (this.Type == OrderType.CashTransitCurrencyExchangeOrder || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder
                    || this.Type == OrderType.CardServiceFeePayment || this.Type == OrderType.InterBankTransferCash || this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount)
                {
                    return true;
                }
                else
                    return false;
            }
        }
        /// <summary>
        /// Կանխիկի ստուգման անհրաժեշտություն
        /// </summary>
        public bool ValidateForCash
        {
            get
            {
                if (this.Type == OrderType.CashForRATransfer || this.Type == OrderType.CashDebit || this.Type == OrderType.CashCredit ||
                    this.Type == OrderType.CashCreditConvertation || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.CashConvertation ||
                    this.Type == OrderType.CashCommunalPayment || this.Type == OrderType.CashTransitCurrencyExchangeOrder
                    || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder || this.Type == OrderType.TransitCashOut
                    || this.Type == OrderType.CardServiceFeePayment || this.Type == OrderType.InterBankTransferCash)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Կոնվերտացիաների ստուգման անհրաժեշտություն
        /// </summary>
        public bool ValidateForConvertation
        {
            get
            {
                if (this.Type == OrderType.CashConvertation || this.Type == OrderType.Convertation
                || this.Type == OrderType.InBankConvertation || this.Type == OrderType.CashDebitConvertation
                || this.Type == OrderType.CashCreditConvertation || this.Type == OrderType.CashTransitCurrencyExchangeOrder
                || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder
                || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                || this.Type == OrderType.NonCashTransitCurrencyExchangeOrder)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Դեբետ հաշվի ստուգման անհրաժեշտություն
        /// </summary>
        public bool ValidateDebitAccount
        {
            get
            {
                if (this.Type != OrderType.CashDebit && this.Type != OrderType.CashConvertation
                && this.Type != OrderType.CashDebitConvertation && this.Type != OrderType.CashForRATransfer && this.Type != OrderType.TransitCashOutCurrencyExchangeOrder
                && this.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && this.Type != OrderType.InterBankTransferCash
                && this.Type != OrderType.TransitCashOut && this.Type != OrderType.TransitNonCashOut
                && this.Type != OrderType.ReestrTransferOrder && this.Type != OrderType.CashTransitCurrencyExchangeOrder
                && this.Type != OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount && this.Type != OrderType.CardServiceFeePayment
                && this.Type != OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// Հաճախորդի տցյալների թարմացման ստուգման անհրաժեշտություն
        /// </summary>
        public bool CheckCustomerUpdateExpired
        {
            get
            {
                if (this.Type != OrderType.CashDebit && this.Type != OrderType.CashDebitConvertation
                       && this.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && this.Type != OrderType.LoanMature
                       && this.Type != OrderType.CashCommunalPayment && this.Type != OrderType.TransitNonCashOut
                       && this.Type != OrderType.CashConvertation && this.Type != OrderType.TransitPaymentOrder
                       && this.Type != OrderType.CashTransitCurrencyExchangeOrder && this.Type != OrderType.ReestrTransferOrder
                       && this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.CashForRATransfer
                       && this.Type != OrderType.CashPosPayment && this.Type != OrderType.CardServiceFeePayment
                       && this.Type != OrderType.NonCashTransitPaymentOrder && this.Type != OrderType.NonCashTransitCurrencyExchangeOrder)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// խմբի id    
        /// </summary>
        public int GroupId { get; set; }

        /// Դրամարկղի կողմից Confirm արվող հայտերի համար true, մնացած դեպքերում false 
        public bool OnlySaveAndApprove { get; set; }

        /// <summary>
        /// Նշում դիլինգի հաստատում ուղարկելու համար
        /// </summary>
        internal bool SendDillingConfirm { get; private set; }


        /// <summary>
        /// Վերադարձնում է տվյալ ժամանակահատվածում պահպանած և խմբագրվող կարգավիճակով հանձնարարականները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        internal static List<Order> GetDraftOrders(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = OrderDB.GetDraftOrders(customerNumber, dateFrom, dateTo);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ժամանակահատվածում պահպանած և բանկ ուղարկված հանձնարարականները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        internal static List<Order> GetSentOrders(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = OrderDB.GetSentOrders(customerNumber, dateFrom, dateTo);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է որոնման տվյալների համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="searchParams">Որոնման պարամետրեր</param>
        /// <returns></returns>
        internal static List<Order> GetOrders(SearchOrders searchParams)
        {
            List<Order> orders = OrderDB.GetOrders(searchParams);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է օգտագործողի չհաստատված գործարքները
        /// </summary>
        /// <returns></returns>
        internal static List<Order> GetNotConfirmedOrders(short setNumber, int start, int end)
        {
            List<Order> orders = OrderDB.GetNotConfirmedOrders(setNumber, start, end);
            return orders;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ հաշվով բանկ ուղարկված և/կամ չհաստատված գործարքների հանրագումարը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="source">Բանկից ուղարկված հայտերի դեպքում ֆունկցիան վերադարձնում է միշտ 0</param>
        /// <returns></returns>
        internal static double GetSentOrdersAmount(string accountNumber, SourceType source)
        {
            double amount = 0;
            if (source == SourceType.AcbaOnline || source == SourceType.AcbaOnlineXML || source == SourceType.MobileBanking || source == SourceType.ArmSoft)
            {
                amount = OrderDB.GetSentOrdersAmount(accountNumber);
            }
            return amount;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի տվյալ օրվա ուղարկված գործարքների հանրագումարը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderId"></param>
        /// <param name="registrationDate"></param>
        /// <returns></returns>
        internal static double GetDayOrdersAmount(ulong customerNumber, long orderId, DateTime registrationDate, SourceType source = 0, short isToOwnAccount = 0)
        {
            if (source != SourceType.PhoneBanking)
            {
                return OrderDB.GetDayOrdersAmount(customerNumber, orderId, registrationDate);
            }
            else
            {
                return OrderDB.GetDayOrdersAmount(customerNumber, orderId, registrationDate, source, isToOwnAccount);
            }

        }

        /// <summary>
        /// Վերադարձնում է հաստատման հաջորդ կարգավիճակը:
        /// </summary>
        /// <param name="schemaType">Հաստատման սխեմայի տեսակ</param>
        /// <returns></returns>
        internal OrderQuality GetNextQuality(short schemaType)
        {
            OrderQuality result;
            Account debitAccount = new Account();
            if (Source == SourceType.Bank || Source == SourceType.ExternalCashTerminal || Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking 
                                    || Source == SourceType.ArmSoft || Source == SourceType.AcbaOnlineXML || Source == SourceType.STAK)
            {

                if (this.Type == OrderType.CashOutFromTransitAccountsOrder)
                {
                    PaymentOrder paymentOrder = (PaymentOrder)this;
                    if (this.user.TransactionLimit < 0)
                    {
                        result = OrderQuality.Sent;
                    }
                    else
                    {

                        double transactionAmount;
                        if (this.Currency == "AMD")
                        {
                            transactionAmount = this.Amount;
                        }
                        else
                        {
                            transactionAmount = this.Amount * Utility.GetLastCBExchangeRate(this.Currency);
                        }

                        if (PaymentOrder.IsRequireApprovalCashOutFromTransitAccounts(paymentOrder.DebitAccount.AccountNumber)
                            || this.user.TransactionLimit < transactionAmount)
                        {
                            result = OrderQuality.TransactionLimitApprovement;
                        }
                        else
                        {
                            result = OrderQuality.Sent;
                        }
                    }
                }
                else
                {
                    if (this.GetType().Name == "PaymentOrder" || this.GetType().Name == "FastTransferPaymentOrder"
                        || this.GetType().Name == "TransitPaymentOrder" || this.GetType().Name == "UtilityPaymentOrder"
                        || this.GetType().Name == "ReestrTransferOrder" || this.GetType().Name == "BudgetPaymentOrder")
                    {
                        ActionResult check = user.CheckForTransactionLimit(this);
                        if (check.ResultCode == ResultCode.ValidationError && this.Type != OrderType.TransitNonCashOut && this.Type != OrderType.CashDebit && !(this.Type == OrderType.RATransfer && this.SubType == 3))
                        {
                            result = OrderQuality.TransactionLimitApprovement;
                        }
                        else
                        {
                            result = OrderQuality.Sent;
                        }

                        if (this.Type == OrderType.Convertation && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking))
                        {
                            PaymentOrder paymentOrder = (PaymentOrder)this;
                            if (paymentOrder.ForDillingApprovemnt == true)
                            {
                                result = OrderQuality.CurrencyExchangeWithVariation;
                                OrderDB.ChangeQuality(paymentOrder.Id, OrderQuality.CurrencyExchangeWithVariation, paymentOrder.user.userName);
                            }
                        }

                    }
                    else if (this.GetType().Name == "InternationalPaymentOrder")
                    {

                        CurrencyExchangeOrder order = new CurrencyExchangeOrder((InternationalPaymentOrder)this);

                        if (order.IsVariation != ExchangeRateVariationType.None || this.SendDillingConfirm)
                        {
                            result = OrderQuality.CurrencyExchangeWithVariation;
                        }
                        else
                        {
                            ActionResult check = user.CheckForTransactionLimit(this);
                            if (check.ResultCode == ResultCode.ValidationError)
                            {
                                result = OrderQuality.TransactionLimitApprovement;
                            }
                            else
                            {
                                result = OrderQuality.Sent;
                            }
                        }
                    }
                    else if (this.GetType().Name == "CurrencyExchangeOrder" || this.GetType().Name == "TransitCurrencyExchangeOrder")
                    {
                        CurrencyExchangeOrder order = (CurrencyExchangeOrder)this;

                        if (order.IsVariation != ExchangeRateVariationType.None || this.SendDillingConfirm)
                        {
                            result = OrderQuality.CurrencyExchangeWithVariation;
                        }
                        else
                        {
                            ActionResult check = user.CheckForTransactionLimit(this);

                            if (check.ResultCode == ResultCode.ValidationError && this.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && this.Type != OrderType.CashDebitConvertation && this.Type != OrderType.Convertation)
                            {
                                result = OrderQuality.TransactionLimitApprovement;
                            }
                            else
                            {
                                result = OrderQuality.Sent;
                            }
                        }
                    }
                    else if (this.GetType().Name == "RemittanceCancellationOrder" || this.GetType().Name == "RemittanceAmendmentOrder")
                    {
                        result = OrderQuality.TransactionLimitApprovement;
                    }
                    else
                    {
                        result = OrderQuality.Sent;
                    }

                    if (result == OrderQuality.Sent)
                    {
                        if (Confirmation.IsConfirmatioRequiredForDebitFromCard(this, user))
                        {
                            result = OrderQuality.DebitFromNotGivenCard;
                        }
                    }
                }
            }
            else
            {

                if (this.Source == SourceType.Bank || this.Source == SourceType.PhoneBanking)
                {
                    result = OrderDB.GetNextQuality(schemaType, Id);
                }
                else
                {
                    result = GetNextQualityByStep();
                }

            }

            return result;
        }

        internal ActionResult Approve(short schemaType, string userName, short amlResult = 0)
        {
            ActionResult result = new ActionResult();

            if (Id == 0)
            {
                result.ResultCode = ResultCode.Failed;
            }
            else if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(785));
            }



            if (this.Source == SourceType.PhoneBanking)
            {
                double dayLimit = 0;
                short isToOwnAccount = 0;

                if ((this.Type == OrderType.RATransfer && this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.DepositTermination || this.Type == OrderType.Deposit)
                {
                    isToOwnAccount = 1;
                    dayLimit = this.LimitOfDayToOwnAccount;
                }
                else
                {
                    isToOwnAccount = 2;
                    dayLimit = this.LimitOfDayToAnothersAccount;
                }
                if (Order.GetDayOrdersAmount(this.CustomerNumber, this.Id, DateTime.Now, Source, isToOwnAccount) > dayLimit)
                {
                    ////Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                    result.Errors.Add(new ActionError(74));
                    return result;
                }

            }

            ////Գործարքների օրեկան սահմանաչափի ստուգում stugum@ hanvel e barelavman dzev 6920
            //if (this.Source == SourceType.MobileBanking && Order.GetDayOrdersAmount(this.CustomerNumber, this.Id, DateTime.Now) > this.DailyTransactionsLimit)
            //{
            //    ////Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
            //    result.Errors.Add(new ActionError(74));
            //    return result;
            //}
            //else
            //{

            OrderQuality nextQuality;
            if ((this.Source == SourceType.Bank || this.Source == SourceType.PhoneBanking || this.Source == SourceType.ExternalCashTerminal || this.Source == SourceType.STAK) || this.Type == OrderType.RemoveTransaction || this.Type == OrderType.CancelTransaction)
            {
                nextQuality = GetNextQuality(schemaType);
            }
            else
            {

                if (this.Source == SourceType.MobileBanking && this.Type == OrderType.HBServletRequestTokenActivationOrder)
                {
                    nextQuality = GetNextQuality(schemaType);
                }
                else
                {
                    nextQuality = GetNextQualityByStep();
                    if (this.GetType().Name == "PaymentOrder")
                    {
                        PaymentOrder paymentOrder = (PaymentOrder)this;
                        if (paymentOrder.ForDillingApprovemnt == true && nextQuality == OrderQuality.Sent)
                        {
                            user.userName = userName;
                            this.FilialCode = OrderDB.GetHBDocumentOperationFilialcode(this.Id);
                            nextQuality = GetNextQuality(schemaType);
                        }
                    }
                }


            }



            if (nextQuality == OrderQuality.NotDefined)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(35));
            }
            else if (Quality == OrderQuality.Draft || Quality == OrderQuality.Approved)
            {

                //Փոխարկումների դեպքում եթե գործարքի կարգավիճակը գալիս է (Շեղումով փոխարկման հաստատման ուղարկելու կարգավիճակ:) 
                //փոխում ենք կարգավիճաակը (Ձևակերպվող գումարի սահմանաչափը անցնելիս հաստման ուղղարկելու կարգավիճակ:) 
                // Tbl_HB_quality_history -ում ճիշտ գործարքի կարգավիճակ գրելու համար
                if (nextQuality == OrderQuality.CurrencyExchangeWithVariation || this.SendDillingConfirm)
                {
                    OrderDB.ChangeQuality(Id, OrderQuality.TransactionLimitApprovement, userName);
                }
                //Թերի տրամադրված քարտերից ելքագրման ժամանակ, գործարքը ուղղարկվում է հաստատմա,
                //փոխում ենք կարգավիճաակը (Ձևակերպվող գումարի սահմանաչափը անցնելիս հաստման ուղղարկելու կարգավիճակ:) 
                // Tbl_HB_quality_history -ում ճիշտ գործարքի կարգավիճակ գրելու համար
                else if (nextQuality == OrderQuality.DebitFromNotGivenCard)
                {
                    OrderDB.ChangeQuality(Id, OrderQuality.TransactionLimitApprovement, userName);
                }
                else if (amlResult == 1 && this.Source == SourceType.Bank)
                {
                    OrderDB.ChangeQuality(Id, OrderQuality.TransactionLimitApprovement, userName);
                }
                else
                {
                    OrderDB.ChangeQuality(Id, nextQuality, userName);
                }
                if (Source != SourceType.ExternalCashTerminal && Source!=SourceType.CashInTerminal)
                {
                    if (nextQuality == OrderQuality.Sent)
                    {
                        OrderDB.ChangeBOOrderQuality(Id, OrderQuality.Sent3, user);
                    }
                    else
                    {
                        OrderDB.ChangeBOOrderQuality(Id, nextQuality, user);
                    }
                }

                if (nextQuality == OrderQuality.TransactionLimitApprovement)
                {
                    WriteToJournalOfConfirmations();
                    SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, user.userID);
                    OrderDB.ChangeBOOrderQuality(Id, OrderQuality.TransactionLimitApprovement, user);
                    //Տվյալ գումարի ձևակերպման իրավունք չկա: Գործարքը կգրանցվի հաստատման համար: 
                    ActionError err = new ActionError(this.Type != OrderType.RemittanceCancellationOrder && this.Type != OrderType.RemittanceAmendmentOrder ? (short)624 : (short)1505);
                    result.Errors.Add(err);
                }
                else if (nextQuality == OrderQuality.CurrencyExchangeWithVariation || this.SendDillingConfirm)
                {
                    WriteToJournalOfConfirmations();
                    SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, user.userID);
                    OrderDB.ChangeBOOrderQuality(Id, OrderQuality.TransactionLimitApprovement, user);
                    if (this.GetType().Name == "PaymentOrder")
                    {
                        PaymentOrder paymentOrder = (PaymentOrder)this;
                        if (paymentOrder.ForDillingApprovemnt == true)
                        {
                            OrderDB.PostCurrencyMarketStatus(this.Id);
                        }
                    }

                    if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
                    {
                        //Տվյալ գումարի ձևակերպման իրավունք չկա: Գործարքը կգրանցվի հաստատման համար: 
                        ActionError err = new ActionError(624);
                        result.Errors.Add(err);
                    }


                }
                else if (nextQuality == OrderQuality.DebitFromNotGivenCard)
                {
                    WriteToJournalOfConfirmations();
                    SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, user.userID);
                    OrderDB.ChangeBOOrderQuality(Id, OrderQuality.TransactionLimitApprovement, user);
                    //Տվյալ քարտային հաշվից գումարի ելքագրման իրավունք չկա: Գործարքը կգրանցվի հասատատման համար: 
                    ActionError err = new ActionError(754);
                    result.Errors.Add(err);

                }
                else if (amlResult == 1 && this.Source == SourceType.Bank)
                {
                    //WriteToJournalOfConfirmations();
                    SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, user.userID);
                    OrderDB.ChangeBOOrderQuality(Id, OrderQuality.TransactionLimitApprovement, user);
                    //Նշված փոխանցումը կասկածելի է, պահանջում է AML բաժնի կողմից հաստատում 
                    ActionError err = new ActionError(803);
                    result.Errors.Add(err);

                }

                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(35));
            }


            if (result.ResultCode == ResultCode.Normal && (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking || Source == SourceType.AcbaOnlineXML || Source == SourceType.ArmSoft) && this.Type != OrderType.RemoveTransaction && this.Type != OrderType.CancelTransaction)
            {
                UpdateApprovementProcess(Convert.ToInt32(this.Id), userName);
            }
            //}

            return result;
        }

        internal ActionResult Delete(string userName)
        {
            ActionResult result = new ActionResult();

            if (CheckDocumentID(Id, CustomerNumber) == false)
            {
                //Փաստաթուղթը գոյություն չունի
                result.Errors.Add(new ActionError(477));
            }
            else
            {
                if (Id == 0)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    if (Quality == OrderQuality.Draft)
                    {
                        OrderDB.ChangeQuality(Id, OrderQuality.Removed, userName);
                        result.ResultCode = ResultCode.Normal;
                    }
                    else
                    {
                        //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ հեռացնել:
                        result.ResultCode = ResultCode.Failed;
                        result.Errors.Add(new ActionError(33));
                    }
                }
            }
            return result;
        }

        public static string GenerateNextOrderNumber(ulong customerNumber)
        {
            return OrderDB.GenerateNextOrderNumber(customerNumber);
        }

        /// <summary>
        /// Ստուգում է Doc_Id-ի և customerNumber-ի համատեղելիությունը
        /// </summary>
        /// <param name="ID">Doc_ID</param>
        /// <param name="customerNumber">customerNumber</param>
        /// <returns>Վերադարձնում է true, եթե համատեղելի են:</returns>
        internal static bool CheckDocumentID(long ID, ulong customerNumber)
        {
            bool check = OrderDB.CheckDocumentID(ID, customerNumber);
            return check;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ունիկալ համարով հանձնարարականը
        /// </summary>
        public static Order GetOrder(long orderId, ulong customerNumber)
        {
            Order order = OrderDB.GetOrder(orderId, customerNumber);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ունիկալ համարով հայտի տվյալները
        /// </summary>
        public static Order GetOrder(long ID)
        {
            Order order = OrderDB.GetOrder(ID);
            order.OPPerson = OrderDB.GetOrderOPPerson(ID);
            return order;
        }

        /// <summary>
        /// Մեկ հայտի մուտքագրման պատմություն
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<OrderHistory> GetOrderHistory(long orderId)
        {
            return OrderDB.GetOrderHistory(orderId);
        }

        /// <summary>
        /// Կատարում է հայտի փոփոխություն կատարողի վերաբեյայալ նշում
        /// </summary>
        /// <param name="user">Փոփոխություն կատարող օգտագործող</param>
        /// <param name="action">Փոփոխության տեսակ</param>
        internal void LogOrderChange(ACBAServiceReference.User user, Action action)
        {
            ChangeLog change = new ChangeLog
            {
                ObjectID = (ulong)this.Id,
                ObjectType = ObjectTypes.Order,
                Action = action,
                ActionDate = DateTime.Now,
                ActionSetNumber = user.userID
            };

            change.Insert();
        }

        /// <summary>
        /// Հանձնարարականի պահպանում:
        /// </summary>        
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        internal ulong Save(Order order, SourceType source, ACBAServiceReference.User user)
        {
            ulong result = 0;

            result = OrderDB.Save(order, user.userID, source);

            return result;
        }

        internal static ActionResult SaveLinkHBDocumentOrder(long documentId, ulong orderId)
        {
            return OrderDB.SaveLinkHBDocumentOrder(documentId, orderId);
        }

        /// <summary>
        /// Ստեղծել հերթական հայտի համար
        /// </summary>
        /// <returns></returns>
        public static ulong GenerateNewOrderNumber(OrderNumberTypes orderNumberType, ushort filialCode)
        {
            return Utility.GenerateNewOrderNumber(orderNumberType, filialCode);
        }


        /// <summary>
        /// Կատարում է հայտի հաստատում
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        virtual public ActionResult Confirm(ACBAServiceReference.User user, ConfirmationSourceType confirmationSourceType = ConfirmationSourceType.None)
        {
            ActionResult result = new ActionResult();

            try
            {
                if (Id == 0)
                {
                    result.ResultCode = ResultCode.Failed;
                }
                else
                {
                    if (Quality == OrderQuality.Sent3 || (Quality == OrderQuality.SBQprocessed &&
                        (Type == OrderType.VisaAlias || Type == OrderType.ReestrPaymentOrder || Type == OrderType.AddFondOrder || Type == OrderType.ChangeFondOrder || Type == OrderType.ChangeFTPRateOrder || Type == OrderType.InterestMarginOrder))
                        || ((this.Source != SourceType.Bank && this.Source != SourceType.SSTerminal && this.Source != SourceType.CashInTerminal) && this.Quality == OrderQuality.SBQprocessed)
                        || (Quality == OrderQuality.TransactionLimitApprovement && Type == OrderType.RemittanceCancellationOrder)
                        || (Quality == OrderQuality.TransactionLimitApprovement && Type == OrderType.FastTransferPaymentOrder && SubType == 23)
                         || (Quality == OrderQuality.TransactionLimitApprovement && Type == OrderType.RemittanceAmendmentOrder)
                         || (Quality == OrderQuality.TransactionLimitApprovement && Type == OrderType.ArcaCardsTransactionOrder))
                    {
                        if (!IsAutomatConfirm(this.Type, this.SubType) && Source != SourceType.SSTerminal && this.Source != SourceType.CashInTerminal && ((this.Source == SourceType.Bank || this.Source == SourceType.SSTerminal || this.Source == SourceType.CashInTerminal) && this.Type != OrderType.RosterTransfer
                                                && this.Type != OrderType.ReceivedFastTransferPaymentOrder && this.Type != OrderType.CredentialOrder && this.Type != OrderType.ArcaCardsTransactionOrder && this.Type != OrderType.CardLimitChangeOrder && this.Type != OrderType.BillSplitReminder && this.Type != OrderType.BillSplitSenderRejection) && this.Type != OrderType.DeleteLoanOrder && this.Type != OrderType.AccountRemove && this.Type != OrderType.ThirdPersonAccountRightsTransfer && this.Type != OrderType.MRDataChangeOrder)
                        {
                            result.ResultCode = ResultCode.NoneAutoConfirm;
                            ActionError actionError = new ActionError();
                            actionError.Code = 0;
                            actionError.Description = "Տվյալ տեսակի հայտը ավտոմատ չի կատարվում: Խնդրում ենք սպասել կատարմանը:";
                            result.Errors.Add(actionError);
                        }
                        else
                        {
                            if (!OnlySaveAndApprove)
                            {
                                if (Type == OrderType.VirtualCardStatusChangeOrder)
                                {
                                    result = VirtualCardStatusChangeOrder.Confirm(this.Id, user);
                                }
                                else
                                {
                                    OrderDB.ConfirmOrder(this, user, confirmationSourceType);
                                    result = ConfirmOrderStep2(user);
                                    result.ResultCode = ResultCode.Normal;
                                    result.Id = this.Id;
                                }
                            }
                            else
                            {
                                result.ResultCode = ResultCode.Normal;
                                result.Id = this.Id;
                            }
                        }


                    }
                    else
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Errors.Add(new ActionError(35));
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.SavedNotConfirmed;
                ActionError actionError = new ActionError();
                actionError.Code = 0;
                actionError.Description = ex.Message;
                result.Errors.Add(actionError);
            }


            return result;
        }

        /// <summary>
        /// Հայտին կցված փաստաթղթերի (scan) պահպանում
        /// </summary>
        /// <returns></returns>
        internal ActionResult SaveOrderAttachments()
        {

            ActionResult result = new ActionResult();

            if (this.Attachments != null && this.Attachments.Count > 0)
            {
                OrderDB.SaveOrderAttachments(this);
            }

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        /// <summary>
        /// Գործարք կատարող անձի պահպանում
        /// </summary>
        /// <returns></returns>
        internal ActionResult SaveOrderOPPerson()
        {
            ActionResult result = new ActionResult();

            OrderDB.SaveOrderOPPerson(this);

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        /// <summary>
        /// Հայտի կարգավիճակի պատմության մեջ ՊԿ-ի 
        /// </summary>
        /// <param name="quality"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal ActionResult SetQualityHistoryUserId(OrderQuality quality, int userId)
        {
            ActionResult result = new ActionResult();

            OrderDB.SetQualityHistoryUserId(this.Id, quality, userId);

            result.ResultCode = ResultCode.Normal;
            return result;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հայտին կցված փաստաթղթերը (առանց scan-ի)
        /// </summary>
        /// <param name="orderId">Հայտի ունիկալ համար</param>
        /// <returns></returns>
        public static List<OrderAttachment> GetOrderAttachments(long orderId)
        {
            return OrderDB.GetOrderAttachments(orderId);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հայտին կցված փաստաթղթի սկանը
        /// </summary>
        /// <param name="attachmentId">Սկանի ունիկալ համար</param>
        /// <returns></returns>
        public static OrderAttachment GetOrderAttachment(string attachmentId)
        {
            return OrderDB.GetOrderAttachment(attachmentId);
        }

        internal ActionResult SaveBOOrderAttachments(ulong orderId)
        {
            ActionResult result = new ActionResult();

            if (this.Attachments != null && this.Attachments.Count > 0)
            {
                OrderDB.SaveBOOrderAttachments(this, orderId);
            }
            result.ResultCode = ResultCode.Normal;
            return result;
        }
        internal void WriteToJournalOfConfirmations()
        {
            if (this.GetType().Name == "PaymentOrder" || this.GetType().Name == "FastTransferPaymentOrder"
                || this.GetType().Name == "ReestrTransferOrder" || this.GetType().Name == "BudgetPaymentOrder")
            {

                if (this.Type == OrderType.Convertation && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking))
                {
                    PaymentOrder paymentOrder = (PaymentOrder)this;
                    if (paymentOrder.ForDillingApprovemnt == true || paymentOrder.SendDillingConfirm)
                    {
                        int status = user.TransactionLimit == -1 ? 1 : 0;
                        short direction = 0;
                        if (paymentOrder.DebitAccount.Currency == "AMD")
                        {
                            direction = 2;
                        }
                        else
                        {
                            direction = 1;
                        }
                        CurrencyExchangeOrder exchangeOrder = new CurrencyExchangeOrder(paymentOrder);

                        exchangeOrder.SendDillingConfirm = paymentOrder.SendDillingConfirm;

                        Confirmation confirmation = new Confirmation(exchangeOrder);

                        confirmation.Result = 1;

                        confirmation.InsertCurrencyMarket(exchangeOrder, paymentOrder.FilialCode, status, direction);

                        confirmation.WriteToJournal();

                        if (paymentOrder.SendDillingConfirm)
                            confirmation.AutomaticallyConfirmJournal(confirmation.OrderId, user.userID);
                    }
                    else
                    {

                        Confirmation confirmation = new Confirmation((PaymentOrder)this);

                        confirmation.WriteToJournal();
                    }

                }
                else
                {

                    Confirmation confirmation = new Confirmation((PaymentOrder)this);

                    confirmation.WriteToJournal();
                }

            }



            else if (this.GetType().Name == "InternationalPaymentOrder")
            {

                CurrencyExchangeOrder exchangeOrder = new CurrencyExchangeOrder((InternationalPaymentOrder)this);
                Confirmation confirmation = new Confirmation((InternationalPaymentOrder)this, exchangeOrder);

                confirmation.WriteToJournal();
                if (exchangeOrder.IsVariation == ExchangeRateVariationType.DillingVariation)
                {

                    int status = user.TransactionLimit == -1 ? 1 : 0;
                    short direction = 0;
                    if (exchangeOrder.DebitAccount.Currency == "AMD")
                    {
                        direction = 2;
                    }
                    else
                    {
                        direction = 1;
                    }
                    confirmation.InsertCurrencyMarketForInternational(exchangeOrder, user.filialCode, status, direction);

                    confirmation.AutomaticallyConfirmJournal(confirmation.OrderId, user.userID);
                }
            }
            else if (this.GetType().Name == "TransitPaymentOrder")
            {

                Confirmation confirmation = new Confirmation((TransitPaymentOrder)this);
                confirmation.WriteToJournal();

            }
            else if (this.GetType().Name == "CurrencyExchangeOrder" || this.GetType().Name == "TransitCurrencyExchangeOrder")
            {
                CurrencyExchangeOrder exchangeOrder = (CurrencyExchangeOrder)this;
                Confirmation confirmation = new Confirmation(exchangeOrder);
                confirmation.WriteToJournal();
                if (exchangeOrder.IsVariation == ExchangeRateVariationType.DillingVariation || this.SendDillingConfirm)
                {

                    int status = user.TransactionLimit == -1 ? 1 : 0;
                    short direction = 0;
                    if (exchangeOrder.DebitAccount.Currency == "AMD")
                    {
                        direction = 2;
                    }
                    else
                    {
                        direction = 1;
                    }
                    confirmation.InsertCurrencyMarket(exchangeOrder, user.filialCode, status, direction);

                    if (!confirmation.IsNeedBranchAccountantApprovement)
                        confirmation.AutomaticallyConfirmJournal(confirmation.OrderId, user.userID);
                }
            }
            else if (this.GetType().Name == "UtilityPaymentOrder")
            {

                Confirmation confirmation = new Confirmation((UtilityPaymentOrder)this);
                confirmation.WriteToJournal();

            }
        }

        internal ActionResult SaveOrderFee()
        {
            ActionResult result = new ActionResult();

            if (this.Fees != null && this.Fees.Count > 0)
            {
                OrderDB.SaveOrderFee(this);
            }

            result.ResultCode = ResultCode.Normal;
            return result;
        }


        internal List<ActionError> GetActionResultWarnings(ActionResult result)
        {
            List<ActionError> warnings = new List<ActionError>();

            if (result != null)
            {
                if (result.Errors != null && result.Errors.Count > 0)
                {
                    warnings = result.Errors;
                }
            }

            return warnings;
        }
        internal static OPPerson SetOrderOPPerson(ulong customerNumber)
        {
            ACBAServiceReference.Customer customer;
            short customerType;
            string organisationName = "";
            string firstName = "";
            string lastName = "";
            string passport = "";
            string email = "";
            string socialNumber = "";
            string noSocialNumber = "";
            string phoneNumber = "";
            string address = "";
            short residence = 0;
            OPPerson opPerson = new OPPerson();
            CustomerPhone customerPhone = new CustomerPhone();
            CustomerAddress customerAddress = new CustomerAddress();


            customer = ACBAOperationService.GetCustomer(customerNumber);
            customerType = customer.customerType.key;

            if (customerType == 6)
            {
                PhysicalCustomer physicalCustomer = (PhysicalCustomer)customer;
                Person person = physicalCustomer.person;
                firstName = person.fullName.firstName;
                lastName = person.fullName.lastName;
                passport = Utility.ConvertAnsiToUnicode(physicalCustomer.DefaultDocument);

                string documentGivenBy = physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.givenBy;
                string documentGivenDate = physicalCustomer.person.documentList.Find(m => m.defaultSign == true)?.givenDate?.Date.ToString("dd/MM/yy");

                if (documentGivenBy != null && documentGivenDate != null)
                {
                    passport = passport + ", " + documentGivenBy + ", " + documentGivenDate;
                }

                CustomerDocument document = person.documentList.Find(cd => cd.documentType.key == 56);
                if (document != null)
                    socialNumber = document.documentNumber;
                else
                {
                    CustomerDocument documentNoSoc = person.documentList.Find(cd => cd.documentType.key == 57);
                    if (documentNoSoc != null)
                        noSocialNumber = documentNoSoc.documentNumber;
                }
                if (person.PhoneList.FindAll(p => p.phoneType.key == 1 || p.phoneType.key == 2).Count > 0)
                {
                    customerPhone = person.PhoneList.Where(p => p.phoneType.key == 1 || p.phoneType.key == 2).OrderBy(p => p.phoneType.key).First();
                }
                if (person.addressList.FindAll(a => a.addressType.key == 2).Count > 0)
                {
                    customerAddress = person.addressList.First(a => a.addressType.key == 2);
                }
                if (person.emailList != null && person.emailList.Count != 0)
                {
                    email = person.emailList.First().email.emailAddress;
                }

                residence = physicalCustomer.residence.key;
            }
            else
            {
                LegalCustomer legalCustomer = (LegalCustomer)customer;
                organisationName = legalCustomer.Organisation.Description;
                if (legalCustomer.Organisation.phoneList.FindAll(p => p.phoneType.key == 1 || p.phoneType.key == 2).Count > 0)
                {
                    customerPhone = legalCustomer.Organisation.phoneList.Where(p => p.phoneType.key == 1 || p.phoneType.key == 2).OrderBy(p => p.phoneType.key).First();
                }
                if (legalCustomer.Organisation.addressList.FindAll(a => a.addressType.key == 2).Count > 0)
                {
                    customerAddress = legalCustomer.Organisation.addressList.First(a => a.addressType.key == 2);
                }
                if (legalCustomer.Organisation.emailList != null && legalCustomer.Organisation.emailList.Count != 0)
                {
                    email = legalCustomer.Organisation.emailList.First().email.emailAddress;
                }
                residence = legalCustomer.residence.key;

            }

            if (customerPhone.phone != null)
            {
                phoneNumber = customerPhone.phone.countryCode + customerPhone.phone.areaCode + customerPhone.phone.phoneNumber;
            }

            if (customerAddress.address != null)
            {
                address = customerAddress.address.PostCode + " " + customerAddress.address.Region.value + " " + customerAddress.address.Street.value + " " + customerAddress.address.House + " " + customerAddress.address.Appartment;
            }

            opPerson.CustomerNumber = customer.customerNumber;
            if (customerType == 6)
            {
                opPerson.PersonName = Utility.ConvertAnsiToUnicode(firstName);
                opPerson.PersonLastName = Utility.ConvertAnsiToUnicode(lastName);
                opPerson.PersonDocument = passport;
                opPerson.PersonSocialNumber = socialNumber;
                opPerson.PersonNoSocialNumber = Utility.ConvertAnsiToUnicode(noSocialNumber);
            }
            else
            {
                opPerson.PersonName = Utility.ConvertAnsiToUnicode(organisationName);
            }

            opPerson.PersonAddress = Utility.ConvertAnsiToUnicode(address);
            opPerson.PersonPhone = phoneNumber;
            opPerson.PersonEmail = email;
            if (residence == 1)
                opPerson.PersonResidence = 1;
            else
                opPerson.PersonResidence = 2;


            return opPerson;

        }


        internal static List<OrderFee> GetOrderFees(long orderId)
        {
            List<OrderFee> fees = new List<OrderFee>();
            fees = OrderDB.GetOrderFees(orderId);
            return fees;
        }

        /// <summary>
        /// Ստուգում է եղել է տվյալ հաշվեհամարի համար չհաստատված հայտ թե ոչ
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Accountnumber"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static bool IsExistRequest(OrderType type, string Accountnumber, ulong customerNumber)
        {
            return OrderDB.IsExistRequest(type, Accountnumber, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ օգտագործողը կարող է ուղարկել հաստատման տվյալ վճարման հանձնարարականը:
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userGroups"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsAbleToChangeQuality(string userName, string userGroups, int id)
        {
            return OrderDB.IsAbleToChangeQuality(userName, userGroups, id);
        }
        internal static bool IsAutomatConfirm(OrderType type, byte subType)
        {
            return OrderDB.IsAutomatConfirm(type, subType);
        }

        /// <summary>
        /// Վերադարձնում է հաստատման հաջորդ կարգավիճակը:
        /// </summary>
        /// <returns></returns>
        internal OrderQuality GetNextQualityByStep()
        {
            return OrderDB.GetNextQualityByStep(Id);
        }

        /// <summary>
        /// Ընթացիկ քայլի կարգավիճակը դարձնում է 2` կատարված, լրացնում է քայլը կատարողին և հաջորդ քայլի կարգավիճակը դարձնում է 1` ակտիվ
        /// </summary>
        /// <returns></returns>
        internal static void UpdateApprovementProcess(int id, string userName)
        {
            OrderDB.UpdateApprovementProcess(id, userName);
        }


        /// <summary>
        /// Պահպանում է հայտին կցված պրուդուկտի app_id-ն
        /// </summary>
        /// <param name="additionalParametr"></param>
        /// <param name="orderId"></param>
        internal static void SaveOrderProductId(ulong appId, long orderId)
        {
            OrderDB.SaveOrderProductId(appId, orderId);
        }

        /// <summary>
        /// Պահպանում է հայտին կցված պրուդուկտի id-ն
        /// </summary>
        internal static void SaveOrderLinkId(int linkId, long orderId, ushort linkType)
        {
            OrderDB.SaveOrderLinkId(linkId, orderId, linkType);
        }

        /// <summary>
        ///  Վերադարձնում մերժված գործարքների ընդհանուր քանակը
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetTotalNotConfirmedOrder(int userId)
        {
            return OrderDB.GetTotalNotConfirmedOrder(userId);
        }
        /// <summary>
        /// Պահպանում է հայտին կցված հաճախորդի համարները
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerNumber"></param>
        internal static void SaveOrderJointCustomer(long orderId, ulong customerNumber)
        {

            OrderDB.SaveOrderJointCustomer(orderId, customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հայտին կցված հաճախորդի համարները
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerNumber"></param>
        internal static List<KeyValuePair<ulong, string>> GetOrderJointCustomer(long orderId)
        {

            return OrderDB.GetOrderJointCustomer(orderId);
        }

        internal static List<Order> GetApproveReqOrder(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = OrderDB.GetApproveReqOrder(customerNumber, dateFrom, dateTo);
            return orders;
        }

        internal List<ActionError> ValidateOrderDescription()
        {
            List<ActionError> result = new List<ActionError>();

            if (string.IsNullOrEmpty(this.Description))
            {
                //Վճարման նպատակը մուտքագրված չէ:
                result.Add(new ActionError(23));
            }
            else if (Description.Length > 130 && (this.Source == SourceType.Bank || this.Source == SourceType.PhoneBanking))
            {
                //«Վճարման նպատակ» դաշտի առավելագույն նիշերի քանակն է 130:
                result.Add(new ActionError(764));
            }
            else if (Description.Length > 100 && this.Source != SourceType.Bank && this.Source != SourceType.PhoneBanking)
            {
                //«Վճարման նպատակ» դաշտի առավելագույն նիշերի քանակն է 100:
                result.Add(new ActionError(122));
            }
            else if (Utility.IsExistForbiddenCharacter(Description))
            {
                //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                result.Add(new ActionError(78));
            }
            else if (Description.Length > 170 && this.Source == SourceType.STAK)
            {
                //«Վճարման նկարագրություն» դաշտի առավելագույն նիշերի քանակն է 170:
                result.Add(new ActionError(1755));
            }

            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաստատման ենթակա վճարման հանձնարարականները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="userName"></param>
        /// <param name="subTypeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="langId"></param>
        /// <param name="receiverName"></param>
        /// <param name="account"></param>
        /// <param name="period"></param>
        /// <param name="groups"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static List<Order> GetConfirmRequiredOrders(ulong customerNumber, string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1)
        {

            List<Order> orders = OrderDB.GetConfirmRequiredOrders(customerNumber, userName, subTypeId, startDate, endDate, langId, receiverName, account, period, groups, quality);
            return orders;
        }



        /// <summary>
        /// Ստուգում է հայտը ունի հաճախորդի համար թե ոչ
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal static bool CheckOrderHasCustomerNumber(long orderId)
        {
            return OrderDB.CheckOrderHasCustomerNumber(orderId);
        }


        /// <summary>
        /// Վերադարձնում է հայտի տրանզակցիայի համարը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static ulong GetOrderTransactionsGroupNumber(long orderId)
        {
            return OrderDB.GetOrderTransactionsGroupNumber(orderId);
        }

        /// <summary>
        /// Հայտի միջնորդավճարի խմբագրում
        /// </summary>
        /// <returns></returns>
        internal void UpdateOrderFee()
        {
            if (this.Fees != null && this.Fees.Count > 0)
            {
                OrderDB.UpdateOrderFee(this);
            }

        }


        internal static List<OrderHistory> GetOnlineOrderHistory(long orderId, Culture culture)
        {
            ulong customerNumber;
            List<OrderHistory> history = Order.GetOrderHistory(orderId);
            history.Remove(history.Find(m => m.Quality == OrderQuality.Sent));

            foreach (OrderHistory item in history)
            {
                if (item.Quality == OrderQuality.Draft || item.Quality == OrderQuality.Sent || item.Quality == OrderQuality.Sent3 || item.Quality == OrderQuality.DeclinedByApprover)
                {
                    customerNumber = ExternalBanking.XBManagement.HBUser.GetHBUserCustomerNumber(item.ChangeUserName);
                    CustomerMainData customer;
                    customer = ACBAOperationService.GetCustomerMainData(customerNumber);
                    if (culture.Language == Languages.hy)
                    {
                        item.CustomerFullName = customer.CustomerDescription;
                    }
                    else
                    {
                        item.CustomerFullName = customer.CustomerDescriptionEng;
                    }
                }
                else
                {
                    item.ChangeUserName = null;
                }
            }

            return history;
        }

        /// <summary>
        /// Հայտի կարգավիճակի փոփոխում
        /// </summary>
        /// <param name="quality"></param> 
        /// <returns></returns>
        public ActionResult UpdateQuality(OrderQuality quality)
        {
            ActionResult result = new ActionResult();

            OrderDB.UpdateQuality(this.Id, quality);

            result.ResultCode = ResultCode.Normal;
            return result;
        }
        public bool IsPaymentIdUnique()
        {
            return OrderDB.IsPaymentIdUnique(this);
        }
        public static Order GetOrderDetails(long orderId)
        {
            Order order = OrderDB.GetOrderDetails(orderId);
            return order;
        }
        public virtual bool CheckConditionForMakingTransactionIn24_7Mode()
        {
            return false;
        }
        /// <summary>
        /// Վերադարձնում է որոնման տվյալներին համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="searchParams">Որոնման պարամետրեր</param>
        /// <returns></returns>
        internal static List<Order> GetOrders(OrderFilter searchParams, ulong customerNumber)
        {
            List<Order> orders = OrderDB.GetOrders(searchParams, customerNumber);
            return orders;
        }


        internal static List<Order> GetOrdersList(ulong customerNumber, OrderListFilter orderListFilter)
        {
            List<Order> orders = OrderDB.GetOrdersList(customerNumber, orderListFilter);
            return orders;
        }

        /// <summary>
        /// Լրացնում հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        /// <param name="order">Հայտ</param>
        public static void InitOrder(Order order, SourceType source, ACBAServiceReference.User user, double dailyTransactionsLimit, ulong customerNumber)
        {
            if (source != SourceType.SSTerminal && source != SourceType.AcbaOnline && source != SourceType.MobileBanking)
            {
                order.FilialCode = user.filialCode;
            }

            order.CustomerNumber = customerNumber;
            order.Source = source;
            order.user = user;

            if (source == SourceType.MobileBanking || source == SourceType.AcbaOnline || source == SourceType.SSTerminal)
            {
                order.OperationDate = Utility.GetNextOperDay();
            }
            else
            {
                order.OperationDate = Utility.GetCurrentOperDay();
            }

            order.DailyTransactionsLimit = dailyTransactionsLimit;
        }
        public static ActionResult ConfirmOrderOnline(long docID, User user)
        {
            ActionResult result = new ActionResult();
            OrderDB.ConfirmOrderOnline(docID, user);

            if (bool.Parse(ConfigurationManager.AppSettings["IsCreditLineOnline"].ToString()))
            {
                Order order = GetOrder(docID);
                if (order.Quality == OrderQuality.Completed || order.Quality == OrderQuality.SBQprocessed)
                {
                    order.ConfirmOrderStep2(user);
                }
            }
           
            result.ResultCode = ResultCode.Normal;
            result.Id = docID;

            return result;
        }

        public static OrderQuality GetOrderQualityByDocID(long docID)
        {
            return OrderDB.GetOrderQualityByDocID(docID);
        }

        public static List<OrderAttachment> GetFullOrderAttachments(long orderId)
        {
            return OrderDB.GetFullOrderAttachments(orderId);
        }
        public static string GetOrderAttachmentInBase64(string attachememntId)
        {
            return OrderDB.GetOrderAttachmentInBase64(attachememntId);
        }
        /// <summary>
        /// Վերադարձնում է հայտի մուտքագրման տվյալների աղբյուրը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SourceType GetOrderSourceType(long id)
        {
            return CurrencyExchangeOrderDB.GetOrderSourceType(id);
        }

        /// <summary>
        /// Ստուգում է անհրաժեշտ է ուղարկել դիլինգ գործարքի հաստատում 
        /// 25,000 USD անցնելու դեպում
        /// </summary>
        internal void CheckDillingConfirm()
        {
            double amount = 0;
            string currency = "";
            ulong customerNumber = 0;
            double rate = 0;
            DateTime? orderDate = null;
            bool isCross = false;


            if (this.GetType() == typeof(CurrencyExchangeOrder)
                || this.GetType() == typeof(TransitCurrencyExchangeOrder)
                || this.GetType() == typeof(PaymentOrder)
                || this.GetType() == typeof(InternationalPaymentOrder))
            {
                PaymentOrder order = (PaymentOrder)this;
                if (order?.DebitAccount?.Currency != order?.ReceiverAccount?.Currency && order?.ReceiverAccount?.Currency != "AMD")
                {
                    currency = order.Currency;
                    amount = order.Amount;
                    orderDate = order.OperationDate;
                    rate = order.ConvertationRate;
                    customerNumber = order.CustomerNumber;
                    if (order?.DebitAccount?.Currency != "AMD" && order?.ReceiverAccount?.Currency != "AMD" &&
                        order?.DebitAccount?.Currency != order?.ReceiverAccount?.Currency)
                    {
                        isCross = true;
                    }
                }
            }

            if (customerNumber != 0)
            {
                SendDillingConfirm = OrderDB.CheckCustomerConvertationLimit(customerNumber, amount, currency, rate, orderDate, isCross);
            }

            //if (this.Source == SourceType.STAK)
            //{
            //    SendDillingConfirm = false;
            //}
        }


        public static string GetOrderRejectReason(long orderId, OrderType type, Culture culture)
        {
            if (type == OrderType.ReceivedFastTransferPaymentOrder)
            {
                return ReceivedFastTransferPaymentOrderDB.GetReceivedFastTransferOrderRejectReason(orderId, culture.Language);
            }
            if (type == OrderType.CardClosing || type == OrderType.PlasticCardOrder || type == OrderType.AttachedPlasticCardOrder || type == OrderType.LinkedPlasticCardOrder || type == OrderType.CurrentAccountClose)
            {
                string rejectReason = OrderDB.GetDocFlowRejectReason(orderId);
                if (!string.IsNullOrEmpty(rejectReason))
                {
                    return rejectReason;
                }
            }
            OrderHistory orderHistory = new OrderHistory();

            List<OrderHistory> orderHistoryList = new List<OrderHistory>();

            orderHistoryList = OrderDB.GetOrderHistory(orderId);

            orderHistory = orderHistoryList.Find(m => m.Quality == OrderQuality.Declined);

            orderHistory.ReasonDescription = Info.GetOrderRejectTypeDescription(orderHistory.ReasonId, culture.Language);
            return orderHistory.ReasonDescription;
        }

        internal static double GetSentNotConfirmedAmounts(string debitaccountNumber, OrderType ordertype, bool withConvertation = true)
        {
            return OrderDB.GetSentNotConfirmedAmounts(debitaccountNumber, ordertype, withConvertation);
        }
        /// <summary>
        /// Վերադարձնում է տվյալ հայտի տեսակով տվյալ օրվա ընթացքում կատարված գործարքների քանակը
        /// </summary>
        /// <returns></returns>
        public int GetOrderDailyCount()
        {
            return OrderDB.GetOrderDailyCount((short)Type,SubType, RegistrationDate, CustomerNumber);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հայտի տեսակով տվյալ օրվա ընթացքում կատարված գործարքների հանրագումարը արտահայտված ՀՀ դրամով
        /// </summary>
        /// <returns></returns>
        public double GetOrderDailyAmount()
        {
            return OrderDB.GetOrderDailyAmount((short)Type,(short)SubType, RegistrationDate, CustomerNumber);
        }

        public ActionResult Reject(int rejectId, User user)
        {
            return OrderDB.Reject(Id, Quality, rejectId, user.userID);
        }

        public ActionResult ConfirmOrderStep2(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            if (   (this.Type == OrderType.CreditLineActivation && 
                    (Source == SourceType.Bank || Source == SourceType.EContract))
                || (this.Type == OrderType.CreditLineSecureDeposit && 
                   (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline))
                || (this.Type == OrderType.FastOverdraftApplication &&
                   (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)))
            {
                LoanProductActivationOrder activationOrder = new LoanProductActivationOrder();
                activationOrder.Id = Id;
                activationOrder.Get();

                Utility.SaveCreditLineLogs(activationOrder.ProductId,  "ConfirmOrderStep2", " ");
                try
                {
                    result.Errors = ChangeExceedLimitRequest.ActivateCreditLine(activationOrder.CustomerNumber, activationOrder.ProductId, activationOrder.Id, user.userID,Source);
                }
                catch(Exception ex)
                {
                    result.Errors.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
                    
                    string message = (ex.Message != null ? ex.Message : " ") +
                    Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "")
                    + " stacktrace:" +(ex.StackTrace != null?ex.StackTrace:" ") ;
                    
                    Utility.SaveCreditLineLogs(activationOrder.ProductId, " ", message);
                }
            }
            if (Type == OrderType.CardRenewOrder)
            {
                CardRenewOrder cardRenewOrder = CardRenewOrder.GetCardRenewOrder(Id);
                bool? withCreditLineClosing = cardRenewOrder.WithCreditLineClosing;
                if (withCreditLineClosing is true)
                {
                    try
                    {
                        long creditLineAppId = cardRenewOrder.CreditLineProductId;
                        string expiryDate = Card.GetCardExpireDateActivatedInArCa(cardRenewOrder.Card.CardNumber);
                        result.Errors = ChangeExceedLimitRequest.CloseCreditLine(CustomerNumber, (ulong)creditLineAppId, Id, user.userID, expiryDate);
                    }
                    catch
                    {
                        result.Errors.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });
                    }
                }
            }

            if (this.Type == OrderType.CreditLineMature &&
                (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline))
            {
                
                Utility.SaveCreditLineLogs(Convert.ToUInt64(OrderNumber), "ConfirmOrderStepTerm2", " ");
                try
                {
                    result.Errors = ChangeExceedLimitRequest.CloseCreditLine(Convert.ToUInt64(CustomerNumber), Convert.ToUInt64(OrderNumber), OrderId, user.userID);
                }
                catch (Exception ex)
                {
                    result.Errors.AddRange(new List<ActionError> { new ActionError { Description = "Ուղարկվելու է ԱՐՔԱ՝ հերթական ֆայլով" } });

                    string message = (ex.Message != null ? ex.Message : " ") +
                    Environment.NewLine + " InnerException:" + (ex.InnerException != null ? ex.InnerException.Message : "")
                    + " stacktrace:" + (ex.StackTrace != null ? ex.StackTrace : " ");

                    Utility.SaveCreditLineLogs(Convert.ToUInt64(OrderNumber), " ", message);
                }
            }

            result.ResultCode = ResultCode.Normal;

            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաստատման ենթակա գործարքների քանակը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="userName"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static int GetConfirmRequiredOrdersCount(ulong customerNumber, string userName, string groups = "")
        {
            return OrderDB.GetConfirmRequiredOrdersCount(customerNumber, userName, groups);
        }

        public static DateTime GetTransferSentDateTime(int docID)
        {
            return OrderDB.GetTransferSentDateTime(docID);
        }

        public static void SetDefaultRestConfigs(string userName)
        {
            OrderDB.SetDefaultRestConfigs(userName);
        }

        internal static double GetSentNotConfirmedWithdrawalAmount(string debitaccountNumber)
        {
            return OrderDB.GetSentNotConfirmedWithdrawalAmount(debitaccountNumber);
        }
        public static List<long> GetAttachedCardOrdersByDocId(List<int> docIds)
        {
            return OrderDB.GetAttachedCardOrdersByDocId(docIds);
        }

        /// <summary>
        /// Վերադարձնում է հայտի մուտքագրման տվյալների աղբյուրը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static OrderType GetOrderType(long id)
        {
            return OrderDB.GetOrderType(id);
        }

    }
}
