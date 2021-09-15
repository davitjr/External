using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static ExternalBanking.ReceivedBillSplitRequest;

namespace ExternalBanking
{
    public class BillSplitOrder : Order
    {
        /// <summary>
        /// 
        /// Մուտքագրվող հաշիվ
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Այլ նկարագրություն
        /// </summary>
        public string OtherDescription { get; set; }

        /// <summary>
        /// Փոխանցողների ցուցակ
        /// </summary>
        public List<BillSplitSenderInfo> Senders { get; set; }

        /// <summary>
        /// Ստացող
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Ստացող Բանկի կոդ
        /// </summary>
        public int ReceiverBankCode { get; set; }

        ///// <summary>
        /////  Հայտի կարգավիճակ
        ///// </summary>
        //BillSplitStatus Status { get; set; }

        /// <summary>
        /// Հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {

            this.Complete();
            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, 15, 0)
            }))
            {

                if (Senders.Where(x => x.IsLinkPayment).ToList().Count > 0)
                {

                    ActionResult resultLinkPaymnet = new ActionResult();

                    foreach (BillSplitSenderInfo sender in Senders.Where(x => x.IsLinkPayment).ToList())
                    {
                        LinkPaymentOrder linkPaymentOrder = new LinkPaymentOrder();
                        linkPaymentOrder.CreditAccount = ReceiverAccount;
                        linkPaymentOrder.Currency = Currency;
                        linkPaymentOrder.PaymentSourceType = LinkPaymentSourceType.FromBillSplit;
                        linkPaymentOrder.LinkPaymentDescription = Description;
                        linkPaymentOrder.user = user;
                        linkPaymentOrder.user.userName = userName;
                        linkPaymentOrder.OperationDate = OperationDate;
                        linkPaymentOrder.CustomerNumber = this.CustomerNumber;
                        linkPaymentOrder.Source = Source;

                        linkPaymentOrder.Amount = sender.Amount;
                        resultLinkPaymnet = linkPaymentOrder.Save();


                        if (resultLinkPaymnet.ResultCode != ResultCode.Normal)
                        {

                            result.Errors.AddRange(resultLinkPaymnet.Errors);
                            result.ResultCode = resultLinkPaymnet.ResultCode;
                            Transaction currentTransaction = Transaction.Current;
                            currentTransaction.Rollback();

                            return result;
                        }
                        else
                        {
                            sender.LinkPaymnentOrderId = resultLinkPaymnet.Id;
                        }
                    }
                }


                result = BillSplitOrderDB.Save(this, userName, source);



                ActionResult resultOpPerson = base.SaveOrderOPPerson();

                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                result.ResultCode = SaveOrderAttachments().ResultCode;

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    LogOrderChange(user, action);
                }


                scope.Complete();
            }
            return result;
        }

        internal void Complete()
        {

            if (this.Source != SourceType.Bank)
                this.RegistrationDate = DateTime.Now.Date;


            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            this.SubType = 1;

            this.Currency = "AMD";


            if (this.Source != SourceType.Bank || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }

            this.ReceiverAccount = Account.GetAccount(this.ReceiverAccount.AccountNumber);


            ReceiverBankCode = Convert.ToInt32(ReceiverAccount?.AccountNumber?.Substring(0, 5));

            Receiver = Account.GetAccountDescription(ReceiverAccount.AccountNumber);

        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user, TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (GetBillSplitOrdersCount(OrderQuality.Sent3, this.CustomerNumber) >= 50)
            {
                //Հարցումը հնարավոր չէ ուղարկել։ Դուք գերազանցում եք ակտիվ հարցումների քանակը ՝ առավելագույնը 50 հարցում։
                result.Errors.Add(new ActionError(1951, new string[] { "50" }));
                return result;
            }

            if (Account.CheckAccessToThisAccounts(this.ReceiverAccount?.AccountNumber) > 0)
            {
                //Նշված հաշվին մուտքերն արգելված են
                result.Errors.Add(new ActionError(1891));
                return result;
            }

            if (Senders?.Count > 30)
            {
                //Փոխանցողներ առավելագույն քանակը պետք է լինի 30:
                result.Errors.Add(new ActionError(1956, new string[] { "30" }));
                return result;
            }

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            if (templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));
            }

            //Կրեդիտ հաշվի ստուգում
            PaymentOrder paymentOrder = new PaymentOrder();
            paymentOrder.ReceiverAccount = ReceiverAccount;
            paymentOrder.Source = this.Source;
            paymentOrder.Type = OrderType.RATransfer;
            paymentOrder.SubType = 1;
            result.Errors.AddRange(Validation.ValidateReceiverAccount(paymentOrder));



            if (result.Errors.Count > 0)
            {
                return result;
            }


            if (result.Errors.Count > 0)
            {
                return result;
            }


            string creditAccountCurrency = Account.GetAccountCurrency(this.ReceiverAccount.AccountNumber);

            if (creditAccountCurrency != this.Currency)
            {
                //Տարբեր արժույթներով հաշիվների միջև փոխանցումն անհրաժեշտ է կատարել «Փոխարկում» բաժնի միջոցով:
                result.Errors.Add(new ActionError(328));
            }


            if (this.ReceiverBankCode == 0)
            {
                ///Ստացողի բանկը նշված չէ:
                result.Errors.Add(new ActionError(16));
            }
            else if (this.ReceiverBankCode.ToString().Length > 5 || this.ReceiverBankCode < 0)
            {
                ///Ստացողի բանկի կոդը սխալ է նշված:
                result.Errors.Add(new ActionError(17));
            }


            if (templateType != TemplateType.CreatedByCustomer)
            {
                if (this.Amount < 0.01)
                {
                    //Մուտքագրված գումարը սխալ է:
                    result.Errors.Add(new ActionError(22));
                }
            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                result.Errors.Add(new ActionError(25));
            }


            result.Errors.AddRange(base.ValidateOrderDescription());


            if (!Validation.CheckReciverBankStatus(this.ReceiverBankCode))
            {
                //Ստացողի բանկը փակ է:
                result.Errors.Add(new ActionError(786));
            }


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }

        /// <summary>
        /// BillSplit հայտերի քանակի ստացում
        /// </summary>
        /// <param name="quality"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static int GetBillSplitOrdersCount(OrderQuality quality, ulong customerNumber)
        {
            return BillSplitOrderDB.GetBillSplitOrdersCount(quality, customerNumber);
        }

        /// <summary>
        /// Հայտի ստացում
        /// </summary>
        public void Get(Languages language)
        {
            BillSplitOrderDB.GetBillSplitOrder(this);
            this.Senders = GetBillSplitSenders(this.Id, 0, language);
            this.Attachments = Order.GetOrderAttachments(this.Id);
            OPPerson = OrderDB.GetOrderOPPerson(Id);
        }

        /// <summary>
        /// Հայտի ուղարկում Բանկ
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ContentResult<List<BillSplitLinkResult>> Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ContentResult<List<BillSplitLinkResult>> result = new ContentResult<List<BillSplitLinkResult>>();
            result.Content = new List<BillSplitLinkResult>();
            ActionResult validationResult = ValidateForSend(user);
            result.Errors = validationResult.Errors;
            result.ResultCode = validationResult.ResultCode;
            if (validationResult.ResultCode == ResultCode.Normal)
            {
                Action action = Id == 0 ? Action.Add : Action.Update;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {

                    List<BillSplitSenderInfo> billSplitSenders = Senders.Where(x => x.IsLinkPayment).ToList();


                    if (billSplitSenders.Count > 0)
                    {
                        LinkPaymentOrder linkPaymentOrder = new LinkPaymentOrder();
                        foreach (BillSplitSenderInfo sender in billSplitSenders)
                        {

                            linkPaymentOrder.Id = sender.LinkPaymnentOrderId;
                            linkPaymentOrder = LinkPaymentOrder.Get(linkPaymentOrder.Id);
                            linkPaymentOrder.user = user;
                            linkPaymentOrder.user.userName = userName;
                            linkPaymentOrder.OperationDate = OperationDate;
                            linkPaymentOrder.CustomerNumber = this.CustomerNumber;
                            linkPaymentOrder.Source = Source;

                            ContentResult<string> linkPaymentResult = linkPaymentOrder.Approve(schemaType, userName, user);

                            if (linkPaymentResult.ResultCode != ResultCode.Normal)
                            {

                                result.ResultCode = linkPaymentResult.ResultCode;
                                result.Errors = linkPaymentResult.Errors;
                                Transaction currentTransaction = Transaction.Current;
                                currentTransaction.Rollback();
                                return result;
                            }
                            else
                            {
                                BillSplitLinkResult billSplitLinkResult = new BillSplitLinkResult();
                                billSplitLinkResult.Email = sender.EmailAddress;
                                billSplitLinkResult.PhoneNumber = sender.PhoneNumber;
                                billSplitLinkResult.Link = linkPaymentResult.Content;
                                result.Content.Add(billSplitLinkResult);
                            }

                        }
                    }

                    BillSplitOrderDB.SendBillSplitSendersNotifications(this.Id);

                    ActionResult actionResult = base.Approve(schemaType, userName);


                    if (actionResult.ResultCode == ResultCode.Normal)
                    {
                        result.ResultCode = ResultCode.Normal;
                        Quality = OrderQuality.Sent3;
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

            return result;

        }

        private ActionResult ValidateForSend(User user)
        {
            ActionResult result = new ActionResult();
            result.ResultCode = result.Errors.Count > 0 ? ResultCode.ValidationError : ResultCode.Normal;
            return result;
        }

        public static List<BillSplitSenderInfo> GetBillSplitSenders(long orderId = 0, int senderId = 0, Languages language = Languages.hy)
        {
            return BillSplitOrderDB.GetBillSplitSenders(orderId, senderId, language);
        }

        public static bool HasBillSplitSenderSentTransfer(int senderId)
        {
            return BillSplitOrderDB.HasBillSplitSenderSentTransfer(senderId);
        }
    }


    /// <summary>
    /// Bill Split փոխանցող
    /// </summary>
    public class BillSplitSenderInfo
    {
        /// <summary>
        /// Փոխանցողի ունիկալ համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Նույնականացված հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Նույնականացված հաճախորդի անուն, ազգանուն՝ հայերենով
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// true` եթե հղումով փոխանցում է, false` հակառակ դեպքում
        /// </summary>
        public bool IsLinkPayment { get; set; }

        /// <summary>
        /// Հաճախորդի հեռախոսահամար
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Հաճախորդի էլ.հասցե
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Տվյալ փոխանցողի կարգավիճակ
        /// 0՝ Ընթացքում է, 1՝ Կատարված, 40` Չեղարկված
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Տվյալ փոխանցողի կարգավիճակի նկարագրություն
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Հղում, եթե փոխանցումը հղումով է
        /// </summary>
        public string LinkURL { get; set; }

        /// <summary>
        /// Հղումով փոխանցման հայտի համար, եթե փոխանցումը հղումով է
        /// </summary>
        public long LinkPaymnentOrderId { get; set; }

    }

    public class SentBillSplitRequest
    {
        /// <summary>
        /// Bill Split հայտի ունիկալ համար
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Նպատակ
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ընդհանուր գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Մասնակիցների քանակ
        /// </summary>
        public int SendersCount { get; set; }

        /// <summary>
        /// Գումարը հետադարձ փոխանցում կատարած անձանց թիվը
        /// </summary>
        public int CompletedSendersCount { get; set; }

        /// <summary>
        /// Տվյալ փոխանցողի կարգավիճակ
        /// 0՝ Ընթացքում է, 1՝ Կատարված
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Հայտի կարգավիճակը՝ Կատարված կամ Ընթացիկ
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Այլ նկարագրություն
        /// </summary>
        public string OtherDescription { get; set; }

        /// <summary>
        /// Մուտքագրվող հաշիվ
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Փոխանցողների ցուցակ
        /// </summary>
        public List<BillSplitSenderInfo> Senders { get; set; }

        /// <summary>
        /// Կցված նկար
        /// </summary>
        public OrderAttachment Attachment { get; set; }

        public static List<SentBillSplitRequest> GetSentBillSplitRequests(ulong customerNumber, Culture culture)
        {
            return BillSplitOrderDB.GetSentBillSplitRequests(customerNumber, culture.Language);
        }

        public static SentBillSplitRequest GetSentBillSplitRequest(ulong customerNumber, long orderId, Culture culture)
        {
            SentBillSplitRequest request = BillSplitOrderDB.GetSentBillSplitRequest(customerNumber, orderId, culture.Language);
            if (request != null)
            {
                request.Senders = BillSplitOrder.GetBillSplitSenders(request.OrderId);
                List<OrderAttachment> attachments = Order.GetFullOrderAttachments(request.OrderId);
                request.Attachment = (attachments != null && attachments.Count > 0) ? attachments[0] : null;
            }
            return request;
        }
    }


    public class ReceivedBillSplitRequest
    {
        /// <summary>
        /// Bill Split հայտի մասնակցի ունիկալ համար
        /// </summary>
        public long SenderId { get; set; }

        /// <summary>
        /// Նպատակ
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ընդհանուր գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Տվյալ փոխանցողի կարգավիճակ
        /// 0՝ Ընթացքում է, 1՝ Կատարված
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Հայտի կարգավիճակը՝ Կատարված կամ Նոր
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Ստացողի հաշվեհամար
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Ստացող
        /// </summary>
        public string Receiver { get; set; }


        public static List<ReceivedBillSplitRequest> GetReceivedBillSplitRequests(ulong customerNumber, Culture culture)
        {
            return BillSplitOrderDB.GetReceivedBillSplitRequests(customerNumber, culture.Language);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ մասնակցին համապատասղան ստացված հայտը
        /// </summary>
        /// <param name="billSplitSenderId"></param>
        /// <returns></returns>
        public static ReceivedBillSplitRequest GetReceivedBillSplitRequest(int billSplitSenderId)
        {
            return BillSplitOrderDB.GetReceivedBillSplitRequest(billSplitSenderId);
        }

        public class BillSplitLinkResult
        {
            public string Link { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }

        }


    }
}
