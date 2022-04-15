using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;
namespace ExternalBanking
{
    /// <summary>
    /// Պարբերական փոխանցման հայտ(ՀՀ տարածքում,բանկի ներսում կամ սեփական հաշիվների միջև)
    /// </summary>
    public class PeriodicPaymentOrder : PeriodicOrder
    {


        public PaymentOrder PaymentOrder { get; set; }


        /// <summary>
        /// Հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicPaymentOrder(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate(user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = PeriodicTransferOrderDB.SavePeriodicPaymentOrder(this, userName, source);

                if (this.Fee > 0 && this.FeeAccount != null || (this.ChargeType == 2 && this.Fee == -1 && this.FeeAccount != null) || (this.PeriodicType == 2 && this.FeeAccount != null))
                {
                    this.Fees = new List<OrderFee>();
                    OrderFee fee = new OrderFee();
                    fee.Amount = this.Fee;
                    fee.Account = this.FeeAccount;
                    fee.Currency = "AMD";
                    fee.Type = 20;
                    this.Fees.Add(fee);
                }
                base.SaveOrderFee();


                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;

        }
        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.PaymentOrder.OPPerson = this.OPPerson;
            this.DebitAccount = this.PaymentOrder.DebitAccount;

            if (this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline)
            {
                this.StartDate = Utility.GetNextOperDay();

                if (this.SubType == 1 && this.PeriodicType == 2)
                {
                    Customer customer = new Customer();
                    customer.CustomerNumber = this.CustomerNumber;
                    Order paymentOrder = this.PaymentOrder;
                    paymentOrder.Type = OrderType.PeriodicTransfer;
                    paymentOrder.OperationDate = Utility.GetNextOperDay();

                    this.Fee = customer.GetPaymentOrderFee(this.PaymentOrder);



                }

                if (this.FeeAccount != null && !String.IsNullOrEmpty(this.FeeAccount.AccountNumber) && this.Fees == null)
                {
                    this.Fees = new List<OrderFee>();
                    OrderFee orderFee = new OrderFee();
                    Customer customer = new Customer();
                    customer.CustomerNumber = this.CustomerNumber;
                    Order paymentOrder = this.PaymentOrder;
                    paymentOrder.Type = OrderType.PeriodicTransfer;
                    paymentOrder.OperationDate = Utility.GetNextOperDay();

                    this.Fee = customer.GetPaymentOrderFee(this.PaymentOrder);

                    orderFee.Account = this.FeeAccount;
                    orderFee.Currency = this.FeeAccount?.Currency;
                    orderFee.Amount = this.Fee;
                    this.Fees.Add(orderFee);
                }

            }
        }
        /// <summary>
        /// Հայտի պահմապնման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrder());
            this.PaymentOrder.OrderNumber = this.OrderNumber;
            this.PaymentOrder.CustomerNumber = this.CustomerNumber;
            this.PaymentOrder.RegistrationDate = this.RegistrationDate;
            this.PaymentOrder.Amount = this.Amount;
            this.PaymentOrder.Id = this.Id;
            this.PaymentOrder.TransferFee = 0;
            this.PaymentOrder.UrgentSign = false;
            this.PaymentOrder.Source = this.Source;
            this.PaymentOrder.Description = this.PeriodicDescription;

            result.Errors.AddRange(PaymentOrder.Validate(user).Errors);
            //Եթե ընտրված է ամբողջ պարտքը տեսակը գումարի ստուգման մասը ջնջում ենք 
            if (this.ChargeType == 2)
            {
                ActionError err = result.Errors.Find(m => m.Code == 22);
                result.Errors.Remove(err);
            }

            //Եթե գործարքը կատարվում է տարանցիկ հաշվից
            //հաճախորդի հետ կապված ստուգումները
            //չեն կատարվում և հեռացվում են
            if (PaymentOrder.DebitAccount.AccountType == 21)
            {
                result.Errors.RemoveAll(m => m.Code == 584);
                result.Errors.RemoveAll(m => m.Code == 765);
            }

            if (PaymentOrder.DebitAccount.IsCardAccount())
            {
                result.Errors.RemoveAll(m => m.Code == 1523);
                result.Errors.RemoveAll(m => m.Code == 1524);
                result.Errors.RemoveAll(m => m.Code == 1525);
                result.Errors.RemoveAll(m => m.Code == 1526);

            }


            if (PaymentOrder.DebitAccount.IsCardAccount() && PaymentOrder.ReceiverAccount.AccountNumber == Constants.LEASING_ACCOUNT_NUMBER)
            {
                //Քարտային հաշվից դեպի լիզինգային հաշիվ պարբերական փոխանցում մուտքագրել հնարավոր չէ։
                result.Errors.Add(new ActionError(1710));
            }

            if (result.Errors.Count > 0)
            {
                return result;
            }
            if (this.PaymentOrder.ReceiverAccount != null && !String.IsNullOrEmpty(this.PaymentOrder.ReceiverAccount.AccountNumber))
            {
                if (this.PaymentOrder.ReceiverBankCode < 22000 || this.PaymentOrder.ReceiverBankCode >= 22300)
                {
                    if (this.FirstTransferDate == Utility.GetNextOperDay())
                    {
                        //Տվյալ փոխանցման առաջին օրը չպետք է համընկնի պարբերական փոխանցման սկզբի ամսաթվի հետ:
                        result.Errors.Add(new ActionError(389));
                    }

                }
                //if (this.PaymentOrder.ReceiverBankCode == 10300 && this.PaymentOrder.ReceiverAccount.AccountNumber.ToString()[5] == '9' && this.PaymentOrder.LTACode == 0)
                //{
                //    //ՏՀՏ կոդը լրացված չէ: Հարկային և մաքսային ծառայություններ փոխանցումներ կատարելու համար անհրաժեշտ է պարտադիր նշել տեսչության կոդը: Այլ փոխանցումների դեպքում պետք է լրացնել «99 Այլ»:
                //    result.Errors.Add(new ActionError(356));
                //}
                if (Info.GetBank(Convert.ToInt32(this.PaymentOrder.ReceiverBankCode), Languages.hy) == "")
                {
                    //Ստացողի բանկը գտնված չէ:
                    result.Errors.Add(new ActionError(252));
                }

                if (this.PaymentOrder.ReceiverBankCode < 22000 || this.PaymentOrder.ReceiverBankCode >= 22300)
                {
                    if (this.PaymentOrder.DebitAccount.Currency != "AMD" && this.PaymentOrder.DebitAccount.Currency != "USD" && this.PaymentOrder.DebitAccount.Currency != "EUR")
                    {
                        //ՀՀ տարածքում այլ բանկեր փոխանցման դեպքում դեբետ հաշվի արժույթը պետք է լինի «AMD», «USD» կամ «EUR» 
                        result.Errors.Add(new ActionError(262));
                    }
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                    {
                        Account account;
                        account = Account.GetAccount(this.PaymentOrder.ReceiverAccount.AccountNumber);
                        if (account != null)
                        {
                            if (Account.IsAccountForbiddenForTransfer(account))
                            {
                                //...հաշիվը ժամանակավոր է, խնդրում ենք դիմել Բանկ
                                result.Errors.Add(new ActionError(1548, new string[] { account.AccountNumber.ToString() }));
                            }
                        }
                    }
                }
                if (this.PaymentOrder.ReceiverBankCode >= 22000 && this.PaymentOrder.ReceiverBankCode < 22300)
                {
                    this.PaymentOrder.ReceiverAccount = Account.GetAccount(this.PaymentOrder.ReceiverAccount.AccountNumber);
                    if (this.PaymentOrder.Currency != this.PaymentOrder.DebitAccount.Currency && this.PaymentOrder.Currency != this.PaymentOrder.ReceiverAccount.Currency)
                    {
                        //Արժույթը պետք է համապատասխանի դեբետ կամ կրեդիտ հաշվին:
                        result.Errors.Add(new ActionError(263));
                    }
                }

                //if (this.DebitAccount.AccountType != 11 && this.DebitAccount.AccountType != 10)
                //{
                //Դեբետ հաշվի տեսակը կարող է լինել միայն ընթացիք կամ քարտային
                //  result.Errors.Add(new ActionError(660));
                //}


                if (this.SubType == 3)
                {

                    if (this.DebitAccount.IsCardAccount() && !(this.PaymentOrder.ReceiverAccount.IsDepositAccount()))
                    {
                        if (!this.PaymentOrder.ReceiverAccount.IsCurrentAccount())
                        {
                            //Կրեդիտագրվող հաշիվը կարող է լինել միայն ավանդային կամ ընթացիկ
                            result.Errors.Add(new ActionError(1543));
                        }

                    }

                    if (this.DebitAccount.IsCardAccount() && !CheckForAmountLimit())
                    {
                        //Բոլոր պարբերականների գումարները միասին չեն կարող գերազանցել 1,000,000 ՀՀ դրամը
                        result.Errors.Add(new ActionError(1544));
                    }

                }

                if (this.PaymentOrder.ReceiverAccount.AccountNumber == Account.GetRAFoundAccount().AccountNumber)
                {

                    Card card = Card.GetCardWithOutBallance(this.DebitAccount.AccountNumber);
                    if (card?.Type != 30 && card?.Type != 46)
                    {
                        // Նվիրատվություն Հայաստան հիմնադրամին նշման դեպքում
                        // հնարավոր է կատարել միայն Visa Barerar քարտից
                        result.Errors.Add(new ActionError(1504));
                    }
                }
            }

            if (this.DebitAccount.IsCardAccount() && (this.FilialCode == 22007 || this.FilialCode == 22011 || this.FilialCode == 22025) && !CheckForAmountLimit())
            {
                //Բոլոր պարբերականների գումարները միասին չեն կարող գերազանցել 1,000,000 ՀՀ դրամը
                result.Errors.Add(new ActionError(1544));
            }

            //Online/Mobile API նոր ստուգումներ
            //**********************************************************************************
            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {

                if (this.PaymentOrder.ReceiverAccount == null && String.IsNullOrEmpty(this.PaymentOrder.ReceiverAccount.AccountNumber))
                {
                    // Մուտքագրվող (կրեդիտ) հաշիվը մուտքագրված չէ:
                    result.Errors.Add(new ActionError(18));
                }

                if (this.PaymentOrder.ReceiverAccount.IsDepositAccount())
                {
                    Deposit deposit = Deposit.GetActiveDeposit(this.PaymentOrder.ReceiverAccount.AccountNumber);
                    if (DateTime.Now.AddMonths(3) > deposit.EndDate)
                    {
                        // Ավանդատուն իրավունք ունի ավանդային պայմանագրի գործողության ժամկետի ընթացքում ավանդային 
                        // հաշվին մուտքագրել գումարներ, բացառությամբ ավանդի գործողության ժամկետի վերջին երեք ամիսների ընթացքում:
                        result.Errors.Add(new ActionError(1502));
                    }

                    if (this.FirstTransferDate.AddMonths(3) > deposit.EndDate)
                    {
                        // Տվյալ ավանդի գումարը ավելացնել հնարավոր չէ (ըստ սակագնի) 
                        result.Errors.Add(new ActionError(1069));
                    }
                }

                if (this.SubType == 1)
                {
                    if (this.PaymentOrder.ReceiverAccount != null && !String.IsNullOrEmpty(this.PaymentOrder.ReceiverAccount.AccountNumber))
                    {


                        if (this.PaymentOrder.ReceiverAccount != null && !String.IsNullOrEmpty(this.PaymentOrder.ReceiverAccount.AccountNumber)
                        && this.DebitAccount != null && !String.IsNullOrEmpty(this.DebitAccount.AccountNumber))
                        {

                            if (Customer.GetCustomerType(this.CustomerNumber) == 2)
                            {
                                if (PaymentOrder.IsTransferFromBusinessmanToOwnerAccount(DebitAccount.AccountNumber, this.PaymentOrder.ReceiverAccount.AccountNumber))
                                {

                                    if (this.Fees != null)
                                    {
                                        if (this.ChargeType != 1)
                                        {
                                            this.Fees[0].Amount = AccountingTools.GetFromBusinessmanToOwnerAccountTransferRateAmount(this.PaymentOrder);
                                        }
                                        else
                                        {
                                            this.Fees[0].Amount = -1;
                                        }

                                        this.Fees[0].Currency = "AMD";
                                        this.Fees[0].Type = 11;
                                    }

                                    if (this.Fees == null || this.Fees[0] == null || this.Fees[0].Account == null ||
                                        String.IsNullOrEmpty(this.Fees[0].Account.AccountNumber))
                                    {
                                        //Միջնորդավճարի հաշիվը բացակայում է:
                                        result.Errors.Add(new ActionError(90));
                                    }
                                }
                            }

                        }

                    }
                }


                if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
                {
                    //Նշված խումբը գոյություն չունի։
                    result.Errors.Add(new ActionError(1628));
                }
                //**********************************************************************************


            }

            if (this.PaymentOrder?.ReceiverAccount?.AccountNumber == Account.GetRAFoundAccount().AccountNumber)
            {

                Card card = Card.GetCardWithOutBallance(this.DebitAccount.AccountNumber);
                if (card?.Type != 30 && card?.Type != 46)
                {
                    // Նվիրատվություն Հայաստան հիմնադրամին նշման դեպքում
                    // հնարավոր է կատարել միայն Visa Barerar քարտից
                    result.Errors.Add(new ActionError(1504));
                }
            }
            return result;
        }
        /// <summary>
        /// Պարբեարականի հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public ActionResult ApprovePeriodicPaymentOrder(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }



            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = base.Approve(schemaType, userName);
                if (result.ResultCode == ResultCode.Normal)
                {
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
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
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrderForSend().Errors);
            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                ActionError err = new ActionError();
                err = Validation.CheckAccountOperation(this.PaymentOrder.DebitAccount.AccountNumber, this.PaymentOrder.ReceiverAccount.AccountNumber, user.userPermissionId, this.Amount);
                if (err.Code != 0 && !(err.Code == 564 && this.SubType == 4) && this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.InterBankTransferNonCash)
                {
                    result.Errors.Add(err);
                }
                else
                {
                    if (this.PaymentOrder.ReceiverAccount.IsDepositAccount())
                    {
                        Deposit deposit = Deposit.GetActiveDeposit(this.PaymentOrder.ReceiverAccount.AccountNumber);
                        if (this.FirstTransferDate.AddMonths(3) > deposit.EndDate)
                        {
                            err.Code = 1069;
                            result.Errors.Add(err);
                        }

                        if (DateTime.Now.AddMonths(3) > deposit.EndDate)
                        {
                            err.Code = 1502;
                            result.Errors.Add(err);
                        }
                    }

                }

                if (this.SubType == 1 &&
                    (Convert.ToUInt32(this.PaymentOrder.ReceiverAccount?.AccountNumber.Substring(0, 5)) < 22000
                    || Convert.ToUInt32(this.PaymentOrder.ReceiverAccount?.AccountNumber.Substring(0, 5)) >= 22300))
                {
                    Order order = this;
                    PaymentOrder paymentOrder = new PaymentOrder();
                    paymentOrder.Type = OrderType.PeriodicTransfer;
                    paymentOrder.DebitAccount = this.PaymentOrder.DebitAccount;
                    paymentOrder.Amount = this.Amount;
                    paymentOrder.Source = this.Source;
                    paymentOrder.OperationDate = this.OperationDate;
                    paymentOrder.ReceiverAccount = new Account { AccountNumber = this.PaymentOrder.ReceiverAccount.AccountNumber };

                    this.Fees = Order.GetOrderFees(this.Id);

                    double fee = AccountingTools.GetFeeForLocalTransfer(paymentOrder);
                    if (fee > 0)
                    {
                        if (!this.Fees.Exists(m => m.Type == 20 && m.Amount == fee))
                        {
                            //Միջնորդավճարի գումարը սխալ է:
                            result.Errors.Add(new ActionError(1423));
                        }

                    }
                }

                if (this.PaymentOrder.ReceiverAccount != null && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking))
                {
                    if (this.SubType == 1 && this.SubType == 3)
                    {
                        Account account;
                        account = Account.GetAccount(this.PaymentOrder.ReceiverAccount.AccountNumber);
                        if (account != null)
                        {

                            if (Account.IsAccountForbiddenForTransfer(account))
                            {
                                //...հաշիվը ժամանակավոր է, խնդրում ենք դիմել Բանկ
                                result.Errors.Add(new ActionError(1548, new string[] { account.AccountNumber.ToString() }));
                            }
                        }
                    }

                }

            }

            return result;
        }
        /// <summary>
        /// Նոր պարբերականի հայտի հաստատում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);

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
                result = PeriodicTransferOrderDB.SavePeriodicPaymentOrder(this, userName, source);
                if (this.Fee > 0 && this.FeeAccount != null || (this.ChargeType == 2 && this.Fee == -1 && this.FeeAccount != null) || (this.PeriodicType == 2 && this.FeeAccount != null))
                {
                    this.Fees = new List<OrderFee>();
                    OrderFee fee = new OrderFee();
                    fee.Amount = this.Fee;
                    fee.Account = this.FeeAccount;
                    fee.Currency = this.FeeAccount.Currency;
                    fee.Type = 20;
                    this.Fees.Add(fee);
                }
                base.SaveOrderFee();

                if (this.PaymentOrder.AdditionalParametrs != null && this.PaymentOrder.AdditionalParametrs.Exists(m => m.AdditionValue == "LeasingAccount"))
                {
                    this.PaymentOrder.Id = this.Id;
                    LeasingDB.SaveLeasingPaymentDetails(this.PaymentOrder);
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

            if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }

            return result;
        }

        /// <summary>
        /// Սեփական հաշիվների միջև և ՀՀ տարածքում պարբերական հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            PeriodicTransferOrderDB.GetPeriodicPaymentOrder(this);
        }

        public bool CheckForAmountLimit()
        {
            return PeriodicTransferOrderDB.CheckForAmountLimit(this);
        }
    }


}
