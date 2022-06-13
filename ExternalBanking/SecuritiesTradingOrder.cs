using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class SecuritiesTradingOrder : Order
    {
        /// <summary>
        /// Արժեթղթերի քանակ
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Արժեթղթերի ծավալ
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Հանձնարարականի համար
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակ
        /// </summary>
        public SharesTypes SecurityType { get; set; }

        /// <summary>
        /// Արժեթղթի տեսակի նկարագրություն
        /// </summary>
        public string SecurityTypeDescription { get; set; }

        /// <summary>
        /// Արժեթղթի ենթատեսակ
        /// </summary>
        public byte SecuritySubType { get; set; }

        /// <summary>
        /// Արժեթղթի ԱՄՏԾ
        /// </summary>
        public string ISIN { get; set; }

        /// <summary>
        /// Բանկային հաշվեհամար վաճառքի հայտի դեպքում
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Շուկայական արժեք
        /// </summary>
        public double MarketPrice { get; set; }

        /// <summary>
        /// Գործարքի տեսակի կոդ
        /// </summary>
        public SecuritiesTradingOrderTypes TradingOrderType { get; set; }

        /// <summary>
        /// Գործարքի տեսակի նկարագրություն
        /// </summary>
        public string TradingOrderTypeDescription { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշիվ
        /// </summary>
        public DepositaryAccount DepositoryAccount { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված արժեթղթի գին լիմիտային / ստոպ լիմիտային գործարքի դեպքում
        /// </summary>
        public double LimitPrice { get; set; }

        /// <summary>
        /// Հաճախորդի կողմից մուտքագրված ստոպ գին ՍՏՈՊ / ստոպ լիմիտային գործարքի դեպքում
        /// </summary>
        public double StopPrice { get; set; }

        /// <summary>
        /// true` եթե հաճախորդը նշել է checkbox-ը, false հակառակ դեպքում
        /// </summary>
        public bool AcknowledgedByCheckBox { get; set; }

        /// <summary>
        /// Եթե checkbox-ը նշված է, ապա checkbox-ի դիմաց ցուցադրված տեքստը
        /// </summary>
        public string AcknowledgementText { get; set; }

        /// <summary>
        /// Հանձնարարականի ուժի մեջ լինելու տեսակ
        /// </summary>
        public SecuritiesTradingOrderExpirationType ExpirationType { get; set; }

        /// <summary>
        /// Գործարքի ժամկետի նկարագրություն
        /// </summary>
        public string ExpirationTypeDescription { get; set; }

        /// <summary>
        /// Գործարքի կնքման վայր
        /// </summary>
        public string TradingPlatform { get; set; }

        /// <summary>
        /// SecuritiesMarketTradingOrder Տեսակի հայտերի քանակ (quantity) դաշտի համագումարը
        /// </summary>
        public int MarketTradedQuantity { get; set; }

        /// <summary>
        /// Եկամտաբերություն
        /// </summary>
        public double Yield { get; set; }

        /// <summary>
        /// Ստոպ եկամտաբերություն 
        /// </summary>
        public double StopYield { get; set; }

        /// <summary>
        /// Թողարկողի անվանում
        /// </summary>
        public string IssuerNameAM { get; set; }

        /// <summary>
        /// Թողարկողի անվանում անգլերեն
        /// </summary>
        public string IssuerNameEN { get; set; }


        /// <summary>
        /// Փոխանցման միջնորդավճարի հաշվեհամար
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// Միջնորդավճարի գումար
        /// </summary>
        public double TransferFee { get; set; }

        /// <summary>
        /// Բրոքերային կոդ
        /// </summary>
        public string BrokerageCode { get; set; }

        /// <summary>
        /// Բորսայական հապավում
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// 1 եթե արժեթուղթը բանկի կողմից է թողարկված, հակառակ դեպքում 0
        /// </summary>
        public bool IsBankSecurity { get; set; }

        /// <summary>
        /// Ֆոնդային բորսայում կատարված գործարքների ցանկ
        /// </summary>
        public List<SecuritiesTrading.SecuritiesMarketTradingOrder> SecuritiesMarketTradingOrderList { get; set; }

        /// <summary>
        /// Մերժման նկարագրություն
        /// </summary>
        public string RejectReasonDescription { get; set; }

        /// <summary>
        /// Բրոքերային պայմանագրի համարը
        /// </summary>
        public string BrokerContractId { get; set; }

        /// <summary>
        /// Ցուցակ
        /// </summary>
        public string ListingType { get; set; }

        /// <summary>
        /// Արտարժութային արժեթղթերի դեպքում հայտի մուտքագրման պահի ԿԲ փոխարժեք
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// Արժեթղթերի առուվաճառքի հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            IsBankSecurity = BondIssue.IsACBASecurity(ISIN);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = SecuritiesTradingOrderDB.SaveSecuritiesTradingOrder(this, userName, source);
                if (result.ResultCode == ResultCode.Normal)
                {
                    if (Type == OrderType.SecuritiesBuyOrder)
                    {
                        ActionResult resultOrderFee = SaveOrderFee();
                    }


                    ActionResult resultOpPerson = base.SaveOrderOPPerson();
                    if (resultOpPerson.Errors.Count > 0)
                    {
                        resultOpPerson.ResultCode = ResultCode.Failed;
                        return resultOpPerson;
                    }
                }
                   

                //**********
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if ((this.TradingOrderType == SecuritiesTradingOrderTypes.Stop || this.TradingOrderType == SecuritiesTradingOrderTypes.Limit)
                && !Utility.IsCorrectAmount(this.LimitPrice, this.Currency))
            {
                //Մուտքագրված գինը սխալ է
                result.Errors.Add(new ActionError(2067));
            }

            if (this.TradingOrderType == SecuritiesTradingOrderTypes.StopLimit)
            {
                if (!Utility.IsCorrectAmount(this.LimitPrice, this.Currency) || !Utility.IsCorrectAmount(this.StopPrice, this.Currency))
                {
                    //Մուտքագրված գինը սխալ է
                    result.Errors.Add(new ActionError(2067));
                }
            }

            if (Type == OrderType.SecuritiesBuyOrder && Currency != Account.GetAccountCurrency(DebitAccount.AccountNumber))
            {
                //Ընտրված բանկային հաշվի արժույթը պետք է համընկնի արժեթղթի արժույթի հետ։
                result.Errors.Add(new ActionError(2063));
            }

            if (Type == OrderType.SecuritiesSellOrder && Currency != Account.GetAccountCurrency(ReceiverAccount.AccountNumber))
            {
                //Ընտրված բանկային հաշվի արժույթը պետք է համընկնի արժեթղթի արժույթի հետ։
                result.Errors.Add(new ActionError(2063));
            }
            if (Type == OrderType.SecuritiesBuyOrder)
            {
                double DepositedAmount = GetSecuritiesTradingOrderDepositedAmount(this);
                if (DepositedAmount != Amount)
                {
                    //Մուտքագրված գումարը սխալ է:
                    result.Errors.Add(new ActionError(22));
                }

                (double TotalFee, double ACBA_Fee) = GetSecuritiesTradingOrderFee(0, ISIN, DepositedAmount, SecurityType, ListingType, Currency, 0, 1);
                if (TotalFee != TransferFee)
                {
                    //Միջնորդավճարի գումարը սխալ է:
                    result.Errors.Add(new ActionError(1423));
                }
            }

            return result;
        }

        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;

            if (SecurityType == SharesTypes.Bonds)
            {
                this.SubType = 1;
            }
            else
            {
                this.SubType = 2;
            }

            this.ReferenceNumber = GenerateReferenceNumber();
            Utility.GetLastKeyNumber(86, this.FilialCode);
            //this.ExpirationType = SecuritiesTradingOrderExpirationType.MarketDayEnd;
            this.TradingPlatform = "Կարգավորվող շուկա";
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            //this.Amount = this.Quantity * this.MarketPrice;

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            bool hasAccount = false;
            this.DepositoryAccount = DepositaryAccount.GetCustomerDepositaryAccount(CustomerNumber, ref hasAccount);

            if (Type == OrderType.SecuritiesBuyOrder)
            {
                this.Fees = new List<OrderFee>();
                OrderFee orderFee = new OrderFee();
                orderFee.Type = 20;
                orderFee.Account = this.FeeAccount;
                if (this.FeeAccount != null)
                {
                    orderFee.Currency = this.FeeAccount.Currency;
                }
                orderFee.Amount = this.TransferFee;
                this.Fees.Add(orderFee);
            }
        }

        public void GetSecuritiesTradingOrder(Languages Language, bool getMarketTrandingOrder = true)
        {
            SecuritiesTradingOrderDB.GetSecuritiesTradingOrder(this, Language, getMarketTrandingOrder);
            Fees = GetOrderFees(Id);
            //if (Fees.Exists(m => m.Type == 20))
            //{
            //    TransferFee = this.Fees.Find(m => m.Type == 20).Amount;
            //    FeeAccount = this.Fees.Find(m => m.Type == 20).Account;
            //}
        }

        public ActionResult Approve(string userName, short schemaType, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
                {

                    result = base.Approve(schemaType, userName);

                    if (result.ResultCode == ResultCode.Normal)
                    {
                        Quality = OrderQuality.Sent3;
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Հայտի ուղարկման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (Type == OrderType.SecuritiesBuyOrder)
            {
                double debetAccountBalance = Account.GetAcccountAvailableBalance(DebitAccount.AccountNumber);
                double feeAccountBalance = Account.GetAcccountAvailableBalance(FeeAccount.AccountNumber);

                if (debetAccountBalance < Amount)
                {
                    if (Account.AccountAccessible(this.DebitAccount.AccountNumber, user.AccountGroup))
                    {
                        //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                        result.Errors.Add(new ActionError(499, new string[] { this.DebitAccount.AccountNumber, debetAccountBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency }));
                    }
                    else
                    {
                        //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                        result.Errors.Add(new ActionError(788, new string[] { this.DebitAccount.AccountNumber }));
                    }
                }

                if (feeAccountBalance < TransferFee)
                {
                    if (Account.AccountAccessible(this.FeeAccount.AccountNumber, user.AccountGroup))
                    {
                        //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                        result.Errors.Add(new ActionError(499, new string[] { this.FeeAccount.AccountNumber, debetAccountBalance.ToString("#,0.00") + " " + this.FeeAccount.Currency, this.FeeAccount.Balance.ToString("#,0.00") + " " + this.FeeAccount.Currency }));
                    }
                    else
                    {
                        //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                        result.Errors.Add(new ActionError(788, new string[] { this.FeeAccount.AccountNumber }));
                    }
                }


                (double TotalFee, double ACBA_Fee) = GetSecuritiesTradingOrderFee(0, ISIN, Amount, SecurityType, ListingType, Currency, 0, 1);
                if (TotalFee != TransferFee)
                {
                    //Միջնորդավճարի գումարը սխալ է:
                    result.Errors.Add(new ActionError(1423));
                }

            }

            else
            {
                if(Currency != "AMD")
                {
                    if(Rate != Utility.GetCBKursForDate(DateTime.Now.Date, Currency))
                    {
                        //Հնարավոր չէ կատարել:Տեղի է ունեցել փոխարժեքի փոփոխություն:
                        result.Errors.Add(new ActionError(121));
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
        /// Հայտի հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult ValidateForConfirm()
        {
            ActionResult result = new ActionResult();


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

        /// <summary>
        /// Հայտի մերժման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult ValidateForReject()
        {
            ActionResult result = new ActionResult();


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }


        /// <summary>
        /// Արժեթղթերի առուվաճառքի հայտի ստացում հայտի ունիկալ համարով
        /// </summary>
        public static SecuritiesTradingOrder GetSecuritiesTradingOrderById(long docId, Languages Language)
        {
            var securitiesTrandingOrder = SecuritiesTradingOrderDB.GetSecuritiesTradingOrderById(docId, Language);

            List<OrderHistory> orderHistory = Order.GetOrderHistory(docId);
            securitiesTrandingOrder.SentDate = orderHistory.Where(m => m.Quality == OrderQuality.Sent3)?.FirstOrDefault()?.ChangeDate;
            securitiesTrandingOrder.RejectionDate = orderHistory.Where(m => m.Quality == OrderQuality.Declined)?.FirstOrDefault()?.ChangeDate;
            securitiesTrandingOrder.CancellationDate = orderHistory.Where(m => m.Quality == OrderQuality.Canceled)?.FirstOrDefault()?.ChangeDate;

            return securitiesTrandingOrder;
        }
        private string GenerateReferenceNumber()
        {
            return "O-" + Utility.GetLastKeyNumber(86, this.FilialCode).ToString() + this.RegistrationDate.ToString("ddMMyyyy");
        }


        public static List<SecuritiesTradingOrder> GetSecuritiesTradingOrders(ulong CustomerNumber, short QualityType, DateTime StartDate, DateTime EndDate, Languages Language)
        {
            return SecuritiesTradingOrderDB.GetSecuritiesTradingOrders(CustomerNumber, QualityType, StartDate, EndDate, Language);
        }

        protected void RejectOrder(long id, string description, OrderType type, int setNumber) => SecuritiesTradingOrderDB.RejectOrder(id, description, type, setNumber);


        public virtual ActionResult Confirm()
        {
            ActionResult result = ValidateForConfirm();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }

            return base.Confirm(user);
        }

        public virtual ActionResult Reject()
        {
            ActionResult result = ValidateForReject();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }

            RejectOrder(Id, RejectReasonDescription, Type, user.userID);
            result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            return result;
        }

        public static void UpdateDeposited(ulong docId) => SecuritiesTradingOrderDB.UpdateDeposited(docId);


        public static int GetCustomerSentSecuritiesTradingOrdersQuantity(string iSIN, ulong customerNumber)
            => SecuritiesTradingOrderDB.GetCustomerSentSecuritiesTradingOrdersQuantity(iSIN, customerNumber);

        public virtual ActionResult CompleteSecuritiesTradingOrder()
        {
            ActionResult result = ValidateForComplete();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }

            SecuritiesTradingOrderDB.CompleteSecuritiesTradingOrder(this.Id, user.userID);
            result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            return result;
        }

        public ActionResult ValidateForComplete()
        {
            ActionResult result = new ActionResult();


            if (Quality != OrderQuality.SBQprocessed)
            {
                //Տվյալ կարգավիճակով հայտի համար հնարավոր չէ իրականացնել Կատարել գործողությունը
                result.Errors.Add(new ActionError(2069));
            }


            return result;
        }
        public static double GetSecuritiesTradingOrderDepositedAmount(SecuritiesTradingOrder order)
        {
            double DepositedAmount = 0;
            if (order.TradingOrderType == SecuritiesTradingOrderTypes.Market)
            {
                DepositedAmount = order.Quantity * order.MarketPrice * 1.1;
            }
            else if (order.TradingOrderType == SecuritiesTradingOrderTypes.Limit)
            {
                DepositedAmount = order.Quantity * order.LimitPrice;
            }
            else if (order.TradingOrderType == SecuritiesTradingOrderTypes.Stop)
            {
                DepositedAmount = order.Quantity * order.LimitPrice * 1.1;
            }
            else if (order.TradingOrderType == SecuritiesTradingOrderTypes.StopLimit)
            {
                DepositedAmount = order.Quantity * order.LimitPrice;
            }
            if (order.Currency == "AMD")
            {
                return Math.Round(DepositedAmount, 1);
            }
            else
            {
                return Math.Round(DepositedAmount, 2);
            }
        }

        public static (double, double) GetSecuritiesTradingOrderFee(long OrderId, string ISIN, double DepositedAmount, SharesTypes types, string ListingType, string Currency, int OperationQuantity, short CalculatingType)
        {
            return SecuritiesTradingOrderDB.GetSecuritiesTradingOrderFee(OrderId, ISIN, DepositedAmount, types, ListingType, Currency, OperationQuantity, CalculatingType);
        }
    }
}
