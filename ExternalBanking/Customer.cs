using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ARUSDataService;
using ExternalBanking.DBManager;
using ExternalBanking.Events;
using ExternalBanking.PreferredAccounts;
using ExternalBanking.SberTransfers.Order;
using ExternalBanking.SecuritiesTrading;
using ExternalBanking.ServiceClient;
using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static ExternalBanking.ReceivedBillSplitRequest;

namespace ExternalBanking
{
    public class Customer
    {
        /// <summary>
        /// Ավտորիզացված օգտագործող
        /// </summary>
        public ACBAServiceReference.User User { get; set; }
        public ulong CustomerNumber { get; set; }

        public Culture Culture { get; set; }

        public double OneOperationAmountLimit { get; set; }

        public double DailyOperationAmountLimit { get; set; }

        public SourceType Source { get; set; }


        /// <summary>
        /// PhoneBanking` Մեկ գործարքի սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double OneTransactionLimitToOwnAccount { get; set; }

        /// <summary>
        /// PhoneBanking` Մեկ գործարքի սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double OneTransactionLimitToAnothersAccount { get; set; }

        /// <summary>
        /// PhoneBanking`Օրական սահմանաչափ (փոխանցում սեփական հաշիվներ միջև)
        /// </summary>
        public double DayLimitToOwnAccount { get; set; }

        /// <summary>
        /// PhoneBanking`Օրական սահմանաչափ (փոխանցում այլ հաճախորդի հաշվին)
        /// </summary>
        public double DayLimitToAnothersAccount { get; set; }


        public Customer()
        {
            Culture = new Culture(Languages.hy);
        }

        public Customer(ulong customerNumber, Languages language)
        {
            CustomerNumber = customerNumber;
            Culture = new Culture(language);
        }

        public Customer(ACBAServiceReference.User user, ulong customerNumber, Languages language)
        {
            User = user;
            CustomerNumber = customerNumber;
            Culture = new Culture(language);
        }


        public static ACBAServiceReference.Customer GetCustomer(ulong customerNumber)
        {
            var customer = ACBAOperationService.GetCustomer(customerNumber);
            return customer;
        }


        public static ACBAServiceReference.KeyValue GetCustomerFilial(ulong customerNumber)
        {
            var filial = ACBAOperationService.GetCustomerFilial(customerNumber);
            return filial;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ հաշվի տվյալները
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public Account GetAccount(string accountNumber)
        {
            Account account = Account.GetAccount(accountNumber, CustomerNumber);
            Localization.SetCulture(account, this.Culture);
            return account;
        }

        /// <summary>
        /// ընթացիք հաշվի կամ ապառիկ տեղում հաշվի մանրամասներ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public Account GetCurrentAccount(string accountNumber)
        {
            Account account = Account.GetCurrentAccount(accountNumber, this.CustomerNumber);
            Localization.SetCulture(account, this.Culture);
            return account;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հաշիվները
        /// </summary>
        /// <returns></returns>
        public List<Account> GetAccounts()
        {
            List<Account> accounts = Account.GetAccounts(this.CustomerNumber);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի ընթացիկ հաշիվները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Account> GetCurrentAccounts(ProductQualityFilter filter)
        {
            List<Account> accounts = Account.GetCurrentAccounts(this.CustomerNumber, filter);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտերի ցուցակը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Card> GetCards(ProductQualityFilter filter, bool includingAttachedCards = true)
        {
            List<Card> cards = Card.GetCards(this.CustomerNumber, filter, includingAttachedCards);
            Localization.SetCulture(cards, this.Culture);

            return cards;
        }

        /// <summary>
        /// Վերադարձնում է մեկ քարտի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public Card GetCard(ulong productId)
        {
            Card card = Card.GetCard(productId, this.CustomerNumber);
            Localization.SetCulture(card, this.Culture);
            return card;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի վարկերի ցուցակը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Loan> GetLoans(ProductQualityFilter filter, SourceType source)
        {
            List<Loan> loans = Loan.GetLoans(this.CustomerNumber, filter);
            if (source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
            {
                foreach (Loan loan in loans)
                {
                    if (loan.Quality == 5 && loan.EndDate < DateTime.Now.Date)
                    {
                        loan.NextRepayment = null;
                    }
                }
            }
            Localization.SetCulture(loans, this.Culture);
            return loans;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ապառիկ տեղում վարկերի ցուցակը
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public List<Loan> GetAparikTexumLoans()
        {
            List<Loan> loans = Loan.GetAparikTexumLoans(this.CustomerNumber);
            Localization.SetCulture(loans, this.Culture);
            return loans;
        }

        /// <summary>
        /// Վերադարձնում է մեկ վարկի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public Loan GetLoan(ulong productId)
        {
            Loan loan = Loan.GetLoan(productId, this.CustomerNumber);
            Localization.SetCulture(loan, this.Culture);
            return loan;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի վարկային գծերը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<CreditLine> GetCreditLines(ProductQualityFilter filter)
        {
            List<CreditLine> creditLines = CreditLine.GetCreditLines(this.CustomerNumber, filter);
            Localization.SetCulture(creditLines, this.Culture);
            return creditLines;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտի փակված վարկային գծերը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<CreditLine> GetCardClosedCreditLines(string cardNumber)
        {
            List<CreditLine> closedCreditLines = CreditLine.GetCardClosedCreditLines(this.CustomerNumber, cardNumber);
            Localization.SetCulture(closedCreditLines, this.Culture);
            return closedCreditLines;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ ավանդի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public Deposit GetDeposit(ulong productId)
        {
            Deposit deposit = Deposit.GetDeposit(productId, this.CustomerNumber);
            Localization.SetCulture(deposit, this.Culture);
            return deposit;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ավանդները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Deposit> GetDeposits(ProductQualityFilter filter, SourceType source)
        {
            List<Deposit> deposits = Deposit.GetDeposits(this.CustomerNumber, filter);
            if (source == SourceType.AcbaOnline || source == SourceType.AcbaOnlineXML || source == SourceType.MobileBanking || source == SourceType.ArmSoft)
            {
                foreach (var deposit in deposits)
                {
                    deposit.ProductNote = ProductNote.GetProductNote(deposit.ProductId);
                }
            }
            Localization.SetCulture(deposits, this.Culture);
            return deposits;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ պարբերական փոխանցման տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public PeriodicTransfer GetPeriodicTransfer(ulong productId)
        {
            PeriodicTransfer periodicTransfer = PeriodicTransfer.GetPeriodicTransfer(productId, this.CustomerNumber);
            Localization.SetCulture(periodicTransfer, this.Culture);
            return periodicTransfer;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ պարբերական փոխանցման տվյալները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<PeriodicTransfer> GetPeriodicTransfers(ProductQualityFilter filter)
        {
            List<PeriodicTransfer> periodicTransfers = PeriodicTransfer.GetPeriodicTransfers(this.CustomerNumber, filter);
            Localization.SetCulture(periodicTransfers, this.Culture);

            return periodicTransfers;
        }

        /// <summary>
        /// Վերադարձնում է խմբագրվող կարգավիճակով հայտերը նշված ժամանակահատվածում
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public List<Order> GetDraftOrders(DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = Order.GetDraftOrders(this.CustomerNumber, dateFrom, dateTo);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է ուղարկված հայտերը նշված ժամանակահատվածում
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public List<Order> GetSentOrders(DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = Order.GetSentOrders(this.CustomerNumber, dateFrom, dateTo);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է որոնման տվյալների համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="searchParams">Որոնման պարամետրեր</param>
        /// <returns></returns>
        public List<Order> GetOrders(SearchOrders searchParams, User user)
        {
            string accessAllBranchesOrders = null;
            if (user?.AdvancedOptions != null)
            {
                user.AdvancedOptions.TryGetValue("accessAllBranchesOrders", out accessAllBranchesOrders);
            }

            string accessToSeeAllArcaCardsTransactionOrders = null;
            user?.AdvancedOptions.TryGetValue("accessToSeeAllArcaCardsTransactionOrders", out accessToSeeAllArcaCardsTransactionOrders);

            string accessToSeeAllCardLimitChangeOrders = null;
            user?.AdvancedOptions.TryGetValue("accessToSeeAllCardLimitChangeOrders", out accessToSeeAllCardLimitChangeOrders);

            string accessToSeeAllPlasticCardOrders = null;
            user?.AdvancedOptions.TryGetValue("accessToSeeAllPlasticCardOrders", out accessToSeeAllPlasticCardOrders);

            if (accessAllBranchesOrders != "1")
            {
                searchParams.OperationFilialCode = user.filialCode;
            }

            //Առկա են այնպիսի խմբեր, որոնք պետք է ունենան 206 և 207 տեսակի բոլոր հայտերը տեսնելու հասանելիություն
            if ((accessToSeeAllArcaCardsTransactionOrders == "1" && searchParams.Type == OrderType.ArcaCardsTransactionOrder) || (accessToSeeAllCardLimitChangeOrders == "1" && searchParams.Type == OrderType.CardLimitChangeOrder))
            {
                searchParams.RegisteredUserID = 0;
                searchParams.OperationFilialCode = 0;
            }

            //Քարտային բաժնի աշխատակիցները պետք է փոխարինման բոլոր հայտերը տեսնելու հասանելիություն ունենան
            string isCardDepartment = null;
            user.AdvancedOptions.TryGetValue("isCardDepartment", out isCardDepartment);
            if (isCardDepartment == "1" && (searchParams.Type == OrderType.NonCreditLineCardReplaceOrder || searchParams.Type == OrderType.CreditLineCardReplaceOrder ||
                                             searchParams.Type == OrderType.ReplacedCardAccountRegOrder || searchParams.Type == OrderType.PINRegenerationOrder ||
                                             searchParams.Type == OrderType.CreditLineActivation || searchParams.Type == OrderType.CreditLineMature || searchParams.Type == OrderType.CreditLineProlongationOrder ||
                                             searchParams.Type == OrderType.CardRenewOrder || searchParams.Type == OrderType.RenewedCardAccountRegOrder || searchParams.Type == OrderType.CardNotRenewOrder))
            {
                searchParams.OperationFilialCode = 0;
            }

            //210,211,212 տեսակի հայտեր որոնելիս բոլոր մասնաճյուղերի հայտերի դիտարկման հասանելիություն ունեցող խմբերի դեպքում հաշվի չառնել մասնաճյուղը
            if (accessToSeeAllPlasticCardOrders == "1" && (searchParams.Type == OrderType.PlasticCardOrder || searchParams.Type == OrderType.AttachedPlasticCardOrder || searchParams.Type == OrderType.LinkedPlasticCardOrder))
            {
                searchParams.OperationFilialCode = 0;
            }

            List<Order> orders = Order.GetOrders(searchParams);

            if (searchParams.IsCashBookOrder)
            {
                orders = orders.FindAll(m => m.FilialCode == user.filialCode);
            }

            Localization.SetCulture(orders, this.Culture);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է որոնման տվյալների համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="searchParams">Որոնման պարամետրեր</param>
        /// <returns></returns>
        public List<Order> GetNotConfirmedOrders(User user, int start, int end)
        {
            List<Order> orders = Order.GetNotConfirmedOrders(user.userID, start, end);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է ACBA Online և Mobile Banking-ի միջոցով ուղարկված/ ստացված հաղորդագրությունները
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<Message> GetMessages(DateTime dateFrom, DateTime dateTo, short type)
        {
            List<Message> messages = Message.GetMessages(this.CustomerNumber, dateFrom, dateTo, type);
            return messages;
        }

        /// <summary>
        /// Վերադարձնում է նշված քանակի, նշված տեսակի հաղորդագրությունները
        /// </summary>
        /// <param name="messagesCount">Հաղորդագրությունների քանակ</param>
        /// <param name="type">Հաղորդագրության տեսակ</param>
        /// <returns></returns>
        public List<Message> GetMessages(short messagesCount, MessageType type)
        {
            List<Message> messages = Message.GetMessages(this.CustomerNumber, messagesCount, type);
            return messages;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում
        /// </summary>
        /// <param name="order">Վճարման հանձնարարական</param>
        /// <returns></returns>
        public ActionResult SavePaymentOrder(PaymentOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            if (order.DebitAccount.IsRestrictedAccount())
            {
                result.Errors.Add(new ActionError(1814));
                result.ResultCode = ResultCode.ValidationError;
                Localization.SetCulture(result, Culture);
                return result;
            }
            if (order.DebitAccount.IsAttachedCard && !Utility.ValidateAttachedCardOpeartionLimit(order.Amount, order.Currency))
            {
                result.Errors.Add(new ActionError(1899));
                result.ResultCode = ResultCode.ValidationError;
                Localization.SetCulture(result, Culture);
                return result;
            }
            if ((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
                && !order.ReceiverAccount.IsCardAccount()
                && ((order.SubType != 2 && order.Type == OrderType.RATransfer)
                || (order.SubType != 3 && order.Type == OrderType.Convertation)))
            {
                result = order.Save(userName, Source, User);
            }
            else
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
                {
                    result = order.Save(userName, Source, User);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Բյուջե վճարման հանձնարարականի պահպանում
        /// </summary>
        /// <param name="order">Վճարման հանձնարարական</param>
        /// <returns></returns>
        public ActionResult SaveBudgetPaymentOrder(BudgetPaymentOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                if (order.IsPoliceInspectorAct)
                    order.TransferFee = GetPaymentOrderFee(order);
                result = order.Save(userName, Source, User);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Միջնորդավճարների Վճարման հանձնարարականի պահպանում
        /// </summary>
        /// <param name="order">հանձնարարական</param>
        /// <param name="userName">Օգտագործողի անուն</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveServicePaymentOrder(ServicePaymentOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);

            return result;
        }

        /// <summary>
        /// Կոմունալ վճարման հանձնարարականի պահպանում
        /// </summary>
        /// <param name="order">Վճարման հանձնարարական</param>
        /// <returns></returns>
        public ActionResult SaveUtiliyPaymentOrder(UtilityPaymentOrder order, string userName)
        {
            double operationAmount = order.Amount + order.ServiceAmount;
            order.CustomerNumber = this.CustomerNumber;
            ActionResult result = Utility.ValidateOpeartionLimits(operationAmount, order.Currency, OneOperationAmountLimit, order.DebitAccount.IsAttachedCard);
            if (result.ResultCode == ResultCode.Normal)
            {
                result = order.Save(userName, Source, User);
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Կարարում է հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveOrder(PaymentOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();
            if ((Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking) && !order.ReceiverAccount.IsCardAccount()
                && ((order.Type == OrderType.RATransfer && order.SubType == 3) || order.Type == OrderType.Convertation))
            {

                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Գործարքների օրեկան սահմանաչափի ստուգում
                if (Source != SourceType.PhoneBanking)
                {
                    if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now, Source) <= this.DailyOperationAmountLimit)
                    {
                        result = order.Approve(schemaType, userName, User);
                    }
                    else
                    {
                        //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                        result.Errors.Add(new ActionError(74));
                        result.ResultCode = ResultCode.ValidationError;
                    }
                }
                else
                {
                    double dayLimit = 0;
                    short isToOwnAccount = 0;

                    if ((order.Type == OrderType.RATransfer && order.SubType == 3) || order.Type == OrderType.Convertation || order.Type == OrderType.DepositTermination || order.Type == OrderType.Deposit)
                    {
                        isToOwnAccount = 1;
                        dayLimit = this.DayLimitToOwnAccount;
                    }
                    else
                    {
                        isToOwnAccount = 2;
                        dayLimit = this.DayLimitToAnothersAccount;
                    }
                    if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now, Source, isToOwnAccount) < dayLimit)
                    {
                        result = order.Approve(schemaType, userName, User);
                    }
                    else
                    {
                        //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                        result.Errors.Add(new ActionError(74));
                        result.ResultCode = ResultCode.ValidationError;
                    }

                }

            }

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Կատարում է հայտի հեռացում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult DeleteOrder(Order order, string userName)
        {
            order.CustomerNumber = this.CustomerNumber;
            ActionResult result = order.Delete(userName);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է փոխանցման միջնորդավճարը (շտապ փոխանցում,արտարժույթային փոխանցում)
        /// </summary>
        /// <param name="paymentOrder"></param>
        /// <returns></returns>
        public double GetPaymentOrderFee(Order order, int feeType = 0)
        {
            double transferFee = 0;



            OrderType orderType = order.Type;

            PaymentOrder paymentOrder = new PaymentOrder();
            CashPosPaymentOrder cashPosPaymentOrder = new CashPosPaymentOrder();
            TransitPaymentOrder transitOrder = new TransitPaymentOrder();
            CurrencyExchangeOrder currencyExchangeOrder = new CurrencyExchangeOrder();

            if (orderType == OrderType.NonCashTransitPaymentOrder)
                return 0;

            if (orderType == OrderType.CashPosPayment)
            {
                cashPosPaymentOrder = (CashPosPaymentOrder)order;
            }
            else if (orderType == OrderType.TransitPaymentOrder || orderType == OrderType.FastTransferPaymentOrder || orderType == OrderType.CashInternationalTransfer)
            {
                transitOrder = (TransitPaymentOrder)order;
            }
            else if (orderType == OrderType.CashDebitConvertation || orderType == OrderType.CashTransitCurrencyExchangeOrder)
            {
                currencyExchangeOrder = (CurrencyExchangeOrder)order;
            }
            else
            {
                paymentOrder = (PaymentOrder)order;

                if (!string.IsNullOrEmpty(paymentOrder?.ReceiverAccount?.AccountNumber) && Order.CheckAccountForNonFee(paymentOrder?.ReceiverAccount?.AccountNumber))
                {
                    return 0;
                }
            }




            if ((order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
                && (order.SubType == 1 && order.Type == OrderType.RATransfer || order.Type == OrderType.PeriodicTransfer))
            {
                //Ստուգում է Ա/Ձ է թե ոչ
                if (GetCustomerType(order.CustomerNumber) == 2 && PaymentOrder.IsTransferFromBusinessmanToOwnerAccount(paymentOrder.DebitAccount.AccountNumber, paymentOrder.ReceiverAccount.AccountNumber))
                {
                    feeType = 11;
                }
            }

            //Ա/Ձ ից հաշվից տնօրենի հաշվին փոխանցման միջնորդավճար 
            if ((orderType == OrderType.RATransfer && order.SubType == 1 || order.Type == OrderType.PeriodicTransfer) && feeType == 11)
            {
                transferFee = AccountingTools.GetFromBusinessmanToOwnerAccountTransferRateAmount(paymentOrder);
            }

            if ((
                orderType != OrderType.Convertation && orderType != OrderType.CashCredit &&
                    (paymentOrder.UrgentSign || paymentOrder.Currency != "AMD") &&
                    (paymentOrder.SubType == 2 || paymentOrder.SubType == 5)
                )
                ||
                (orderType == OrderType.RATransfer &&
                    (paymentOrder.SubType == 2 ||
                    paymentOrder.SubType == 5 ||
                    paymentOrder.SubType == 6
                   ) && (Source == SourceType.Bank || Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline || Source == SourceType.ArmSoft || Source == SourceType.AcbaOnlineXML)))
            {
                transferFee = AccountingTools.GetFeeForLocalTransfer(paymentOrder);
            }


            if (orderType == OrderType.CashForRATransfer)
            {
                transferFee = AccountingTools.GetFeeForLocalTransfer(paymentOrder);

            }

            if (orderType == OrderType.PeriodicTransfer && !(paymentOrder.SubType == 1 && paymentOrder.ReceiverBankCode >= 22000 && paymentOrder.ReceiverBankCode <= 22300))
            {
                transferFee = AccountingTools.GetFeeForLocalTransfer(paymentOrder);
            }
            if (orderType == OrderType.CashDebit || orderType == OrderType.ReestrTransferOrder)
            {
                if ((paymentOrder.Currency == "RUR" || paymentOrder.Currency == "CHF" || paymentOrder.Currency == "GBP") && paymentOrder.ReceiverAccount != null)
                {
                    string fieldName = "percent";
                    int index = 93;
                    if (paymentOrder.Currency == "RUR")
                    {
                        index = 930;
                    }
                    double percent = AccountingTools.GetCashPaymentOrderFee(index, fieldName, paymentOrder.ReceiverAccount.AccountNumber);
                    double kurs = Utility.GetCBKursForDate(DateTime.Now.Date, paymentOrder.Currency);
                    transferFee = Utility.RoundAmount((double)Convert.ToDecimal(percent * paymentOrder.Amount * kurs), "AMD");
                }

                if (orderType == OrderType.CashDebit && paymentOrder.Currency == "AMD")
                {
                    transferFee = AccountingTools.GetCashInOperationFeeForAMDAccount(paymentOrder?.ReceiverAccount?.AccountNumber, (decimal)paymentOrder.Amount);
                }
            }
            if (orderType == OrderType.CashDebitConvertation || orderType == OrderType.CashTransitCurrencyExchangeOrder)
            {
                if (feeType == 0)
                {
                    return 0;
                }

                if (currencyExchangeOrder?.DebitAccount?.Currency == "AMD")
                {
                    transferFee = AccountingTools.GetCashInOperationFeeForAMDAccount(currencyExchangeOrder?.ReceiverAccount?.AccountNumber, (decimal)currencyExchangeOrder.AmountInAmd);
                }
                else
                {
                    return 0;
                }

            }

            if ((orderType == OrderType.CashCredit && paymentOrder.DebitAccount.AccountType != (short)ProductType.Card) || orderType == OrderType.CashPosPayment)
            {

                if (feeType == 0)
                {
                    return 0;
                }

                if (feeType == 6)
                {
                    double pricePercent = AccountingTools.GetCashPaymentOrderFee(92, "percent", "0");
                    double orderAmount = order.Type == OrderType.CashPosPayment ? cashPosPaymentOrder.Amount : paymentOrder.Amount;
                    string orderCurrency = order.Type == OrderType.CashPosPayment ? cashPosPaymentOrder.Currency : paymentOrder.Currency;
                    double pricePercentAmount = pricePercent * orderAmount * Utility.GetCBKursForDate(DateTime.Now.Date, orderCurrency);
                    transferFee = pricePercentAmount;
                }
                else if (feeType == 5)
                {
                    int index = 0;

                    if (paymentOrder.DebitAccount.Currency == "AMD")
                    {
                        index = 90;
                    }
                    else
                    {
                        index = 91;
                    }
                    double priceAmount = AccountingTools.GetCashPaymentOrderFee(index, "price", "0");
                    double pricePercent = AccountingTools.GetCashPaymentOrderFee(index, "percent", "0");
                    double pricePercentAmount = pricePercent * paymentOrder.Amount * Utility.GetCBKursForDate(DateTime.Now.Date, paymentOrder.Currency);
                    if (priceAmount > pricePercentAmount)
                    {
                        transferFee = priceAmount;
                    }
                    else
                    {
                        transferFee = pricePercentAmount;
                    }
                }
                else
                {
                    transferFee = AccountingTools.GetCashoutOrderFee(paymentOrder, feeType);
                }
            }
            if (paymentOrder.TransferID != 0 || orderType == OrderType.InterBankTransferNonCash || orderType == OrderType.InterBankTransferCash)
            {

                double priceAmount = 0;
                double pricePercent = 0;
                short transferType = (short)orderType;
                AccountingTools.GetPriceForTransferCashOut(paymentOrder.Amount, paymentOrder.DebitAccount.Currency, paymentOrder.OperationDate, transferType, paymentOrder.TransferID, out pricePercent, out priceAmount);

                double pricePercentAmount = pricePercent * paymentOrder.Amount * Utility.GetCBKursForDate(DateTime.Now.Date, paymentOrder.Currency);
                if (priceAmount > pricePercentAmount)
                {
                    transferFee = priceAmount;
                }
                else
                {
                    transferFee = pricePercentAmount;
                }

            }
            if (orderType == OrderType.TransitPaymentOrder || orderType == OrderType.FastTransferPaymentOrder || orderType == OrderType.CashInternationalTransfer)
            {
                if (transitOrder.Currency == "RUR" || transitOrder.Currency == "CHF" || transitOrder.Currency == "GBP")
                {
                    if (orderType == OrderType.TransitPaymentOrder || orderType == OrderType.FastTransferPaymentOrder || orderType == OrderType.CashInternationalTransfer)
                    {
                        transitOrder.TransitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(transitOrder, OrderAccountType.CreditAccount), transitOrder.Currency, User.filialCode);
                    }
                    if (transitOrder.TransitAccount != null)
                    {
                        string fieldNameTransit = "percent";
                        int indexTransit = 93;
                        if (transitOrder.Currency == "RUR")
                        {
                            indexTransit = 930;
                        }
                        double percent = AccountingTools.GetCashPaymentOrderFee(indexTransit, fieldNameTransit, transitOrder.TransitAccount.AccountNumber);
                        double kurs = Utility.GetCBKursForDate(DateTime.Now.Date, transitOrder.Currency);
                        transferFee = Utility.RoundAmount(percent * transitOrder.Amount * kurs, "AMD");
                    }
                }
                else
                {
                    if (transitOrder.Currency == "AMD" && (transitOrder.TransitAccountType == TransitAccountTypes.ForMatureByCreditCode || transitOrder.TransitAccountType == TransitAccountTypes.ForLeasingLoans))
                    {
                        transferFee = AccountingTools.GetCashInOperationFeeForAMDAccount(null, (decimal)transitOrder.Amount);
                    }
                }
            }

            if (orderType == OrderType.CashOutFromTransitAccountsOrder && paymentOrder.DebitAccount != null)
            {
                double minFeeAmount = 0;
                double percent = AccountingTools.CashOutFromTransitAccountsOrderFee(paymentOrder.DebitAccount, User.filialCode, ref minFeeAmount);

                double kurs = Utility.GetCBKursForDate(DateTime.Now.Date, paymentOrder.Currency);
                transferFee = Utility.RoundAmount(percent * paymentOrder.Amount * kurs, "AMD");

                if (transferFee < minFeeAmount)
                {
                    transferFee = minFeeAmount;
                }

            }

            if (orderType == OrderType.CardLessCashOrder)
            {
                transferFee = CardlessCashoutOrder.GetOrderFee(order.Amount);
            }

            transferFee = Utility.RoundAmount(transferFee, "AMD");
            return transferFee;
        }


        public InternationalPaymentOrder GetInternationalPaymentOrder(long id)
        {
            InternationalPaymentOrder internationalPaymentOrder = new InternationalPaymentOrder();
            internationalPaymentOrder.Id = id;
            internationalPaymentOrder.CustomerNumber = this.CustomerNumber;
            internationalPaymentOrder.Get();
            if (internationalPaymentOrder.Currency == "RUR")
            {
                DataRow[] dr = Info.GetTransferReceiverTypes().Select($"id = {internationalPaymentOrder.ReceiverType}");
                internationalPaymentOrder.ReceiverTypeDescription =
                    Culture.Language == Languages.hy ? dr[0]["Description_arm"].ToString() : dr[0]["Description_eng"].ToString();

            }
            Localization.SetCulture(internationalPaymentOrder, Culture);
            return internationalPaymentOrder;
        }


        public double GetCardFee(PaymentOrder order)
        {
            double cardFee = 0;
            CurrencyExchangeOrder currencyExchangeOrder = new CurrencyExchangeOrder();

            if (order.DebitAccount.AccountType == (short)ProductType.Card)
            {
                List<Card> cards = Card.GetCards(this.CustomerNumber, ProductQualityFilter.NotSet);
                Card card = cards.Find(m => m.CardAccount.AccountNumber == order.DebitAccount.AccountNumber && m.MainCardNumber == "");
                if (card != null)
                {
                    double amount = 0;
                    if (order.DebitAccount.Currency == "AMD" && (order.Type == OrderType.Convertation || order.Type == OrderType.CashCreditConvertation ||
                        order.Type == OrderType.InternationalTransfer || order.Type == OrderType.CashInternationalTransfer || order.Type == OrderType.InBankConvertation))
                    {

                        if ((order.Type == OrderType.Convertation || order.Type == OrderType.CashCreditConvertation || order.Type == OrderType.InBankConvertation) && order.GetType().Name == "CurrencyExchangeOrder")
                        {
                            currencyExchangeOrder = (CurrencyExchangeOrder)order;
                            amount = currencyExchangeOrder.AmountInAmd;
                        }
                        else
                        {
                            amount = order.Amount * order.ConvertationRate;
                        }


                    }
                    else if ((order.Type == OrderType.InternationalTransfer || order.Type == OrderType.CashInternationalTransfer) && order.DebitAccount.Currency != order.Currency)
                    {
                        amount = order.Amount * Math.Round(order.ConvertationRate1 / order.ConvertationRate, 5);
                    }
                    else
                    {
                        amount = order.Amount;
                    }

                    cardFee = card.WithdrawalFee(amount, Source);

                }
            }
            cardFee = Utility.RoundAmount(cardFee, order.DebitAccount.Currency, Source);
            return cardFee;
        }


        /// <summary>
        /// Վերադարձնում է միջազգային փոխանցման միջնորդավճարը (շտապ փոխանցում,արտարժույթային փոխանցում)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public double GetInternationalPaymentOrderFee(InternationalPaymentOrder order)
        {
            double transferFee = 0;
            if (order.Type == OrderType.CashInternationalTransfer)
            {
                if (order.DebitAccount == null)
                {
                    //Տարանցիկ հաշվի լրացում
                    order.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, OrderAccountType.DebitAccount), order.Currency, order.FilialCode);
                }
                else if (order.DebitAccount.AccountNumber == "0")
                {
                    //Տարանցիկ հաշվի լրացում
                    order.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, OrderAccountType.DebitAccount), order.Currency, order.FilialCode);
                }
            }
            else
            {
                if (order.DebitAccount == null)
                {
                    //Տարանցիկ հաշվի լրացում
                    order.DebitAccount = new Account();
                }
            }

            order.CustomerNumber = CustomerNumber;
            transferFee = AccountingTools.GetFeeForInternationalTransfer(order);

            return transferFee;
        }


        internal ulong GetIdentityId(ulong customerNumber)
        {
            ulong result = ACBAOperationService.GetIdentityId(customerNumber);
            return result;
        }


        internal short GetCustomerType()
        {
            short result = ACBAOperationService.GetCustomerType(CustomerNumber);
            return result;
        }

        internal static byte GetCustomerType(ulong customerNumber)
        {
            byte result = (byte)ACBAOperationService.GetCustomerType(customerNumber);
            return result;
        }

        internal static string GetCustomerDescription(ulong customerNumber)
        {
            string result = ACBAOperationService.GetCustomerDescription(customerNumber);
            return result;
        }


        /// <summary>
        /// Վերադարձնում է վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PaymentOrder GetPaymentOrder(long id)
        {
            PaymentOrder paymentOrder = new PaymentOrder();
            paymentOrder.Id = id;
            paymentOrder.CustomerNumber = this.CustomerNumber;
            paymentOrder.Get();
            Localization.SetCulture(paymentOrder, Culture);
            if (!String.IsNullOrEmpty(paymentOrder.CreditCode))
                Localization.SetLoanMatureTypeDescription(paymentOrder, Culture);
            return paymentOrder;
        }

        /// <summary>
        /// Վերադարձնում է բյուջե վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BudgetPaymentOrder GetBudgetPaymentOrder(long id)
        {
            BudgetPaymentOrder budgetPaymentOrder = new BudgetPaymentOrder();
            budgetPaymentOrder.Id = id;
            budgetPaymentOrder.CustomerNumber = this.CustomerNumber;
            budgetPaymentOrder.Get();
            DataRow[] LTARow = Info.GetLTACodes().Select($"Code = {budgetPaymentOrder.LTACode}");
            if (budgetPaymentOrder.LTACode != 0)
                budgetPaymentOrder.LTACodeDescription = Culture.Language == Languages.hy ? Utility.ConvertAnsiToUnicode(LTARow[0]["Description"].ToString()) : Utility.ConvertAnsiToUnicode(LTARow[0]["Description_engl"].ToString());
            if (budgetPaymentOrder.PoliceCode != 0)
            {
                DataTable dt = new DataTable();
                if (budgetPaymentOrder.Source == SourceType.AcbaOnline || budgetPaymentOrder.Source == SourceType.MobileBanking)
                {
                    dt = Info.GetPoliceCodes(budgetPaymentOrder.ReceiverBankCode.ToString() + budgetPaymentOrder.ReceiverAccount.AccountNumber);
                }
                else
                {
                    dt = Info.GetPoliceCodes(budgetPaymentOrder.ReceiverAccount.AccountNumber);
                }
                if (dt.Rows.Count > 0)
                {
                    DataRow[] PoliceRow = dt.Select($"code = {budgetPaymentOrder.PoliceCode}");
                    if (Convert.ToUInt16(PoliceRow[0]["ACKIND"].ToString()) == 1)
                        budgetPaymentOrder.PoliceCodeDescription = Culture.Language == Languages.hy ? Utility.ConvertAnsiToUnicode(PoliceRow[0]["Description"].ToString()) : PoliceRow[0]["Description_eng"].ToString();
                    else
                        budgetPaymentOrder.PoliceCodeDescription = Utility.ConvertAnsiToUnicode(PoliceRow[0]["Description"].ToString());
                }
            }
            if (budgetPaymentOrder.PoliceResponseDetailsId != 0)
                budgetPaymentOrder.ViolationID = BudgetPaymentOrder.GetOrderViolationID(budgetPaymentOrder.PoliceResponseDetailsId);

            Localization.SetCulture(budgetPaymentOrder, Culture);
            return budgetPaymentOrder;
        }

        /// <summary>

        /// <summary>
        /// Վերադարձնում է արագ փոխանցման վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FastTransferPaymentOrder GetFastTransferPaymentOrder(long id, string authorizedUserSessionToken, string userName, string clientIP)
        {
            FastTransferPaymentOrder fastTransferPaymentOrder = new FastTransferPaymentOrder();
            fastTransferPaymentOrder.Id = id;
            fastTransferPaymentOrder.CustomerNumber = this.CustomerNumber;
            fastTransferPaymentOrder.Get(userName, authorizedUserSessionToken, clientIP);
            Localization.SetCulture(fastTransferPaymentOrder, Culture);
            return fastTransferPaymentOrder;
        }

        /// <summary>
        /// Վերադարձնում է արագ փոխանցման վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReceivedFastTransferPaymentOrder GetReceivedFastTransferPaymentOrder(long id, string userName, string authorizedUserSessionToken, string clientIP, SourceType source)
        {
            ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder = new ReceivedFastTransferPaymentOrder();
            receivedFastTransferPaymentOrder.Id = id;
            receivedFastTransferPaymentOrder.CustomerNumber = this.CustomerNumber;
            receivedFastTransferPaymentOrder.Get(userName, authorizedUserSessionToken, clientIP, source);
            Localization.SetCulture(receivedFastTransferPaymentOrder, Culture);
            return receivedFastTransferPaymentOrder;
        }
        public List<Account> GetAccountsForOrder(OrderType orderType, byte orderSubType, OrderAccountType accountType, SourceType source, bool includingAttachedCards = true)
        {
            List<Account> accounts = new List<Account>();
            if ((orderType == OrderType.PreferredAccountActivationOrder || orderType == OrderType.PreferredAccountDeactivationOrder || orderType == OrderType.VehicleInsuranceOrder) && orderSubType == 1) // Mobile/Email Preferred Accounts
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
                currentAccounts.RemoveAll(m => m.Currency != "AMD" || m.TypeOfAccount == 282 || m.AccountType == (ushort)ProductType.CardDahkAccount);

                Localization.SetCulture(currentAccounts, Culture);
                accounts.AddRange(currentAccounts);

                List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                cardAccounts.RemoveAll(m => m.Currency != "AMD" || m.ClosingDate != null);

                Localization.SetCulture(cardAccounts, Culture);
                cardAccounts.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card != null && card.Type != 21)
                    {
                        m.AccountDescription = card.CardNumber + " " + card.CardType;
                        accounts.Add(m);
                    }
                });


                return accounts;
            }
            else if ((orderType == OrderType.PreferredAccountActivationOrder || orderType == OrderType.PreferredAccountDeactivationOrder) && orderSubType == 2) // Visa Alias Preferred Accounts
            {
                List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                List<Account> notVisaMasterCards = new List<Account>();
                cardAccounts.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card.CardSystem != 4 && card.CardSystem != 5)
                        notVisaMasterCards.Add(m);
                });
                cardAccounts.RemoveAll(m => notVisaMasterCards.Contains(m));
                cardAccounts.RemoveAll(m => m.ClosingDate != null);

                Localization.SetCulture(cardAccounts, Culture);
                cardAccounts.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card != null && card.Type != 21)
                    {
                        m.AccountDescription = card.CardNumber + " " + card.CardType;
                        accounts.Add(m);
                    }
                });

                return accounts;
            }
            else if ((orderType == OrderType.PreferredAccountActivationOrder || orderType == OrderType.PreferredAccountDeactivationOrder) && orderSubType == 3) // Sberbank Preferred Accounts
            {
                List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                cardAccounts.RemoveAll(m => m.ClosingDate != null);

                Localization.SetCulture(cardAccounts, Culture);
                cardAccounts.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card != null && card.Type != 21)
                    {
                        m.AccountDescription = card.CardNumber + " " + card.CardType;
                        accounts.Add(m);
                    }
                });

                return accounts;
            }

            if (orderType == OrderType.PeriodicTransfer)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
                currentAccounts.RemoveAll(m => m.TypeOfAccount == 282);   //Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ
                //Միայն FrontOffice ծրագրում պետք է երևան տարանցիկ հաշիվները
                if (source == SourceType.Bank)
                {
                    currentAccounts.AddRange(Account.GetCustomerTransitAccounts(this.CustomerNumber, ProductQualityFilter.Opened));
                }
                if (currentAccounts != null && currentAccounts.Count > 0)
                    Localization.SetCulture(currentAccounts, Culture);

                if (orderSubType == 2)
                {
                    accounts.AddRange(currentAccounts);
                    return accounts;
                }
                if (orderSubType == 5 || accountType == OrderAccountType.FeeAccount)
                {
                    currentAccounts = currentAccounts.FindAll(m => m.Currency == "AMD");
                    accounts.AddRange(currentAccounts);

                    if (accountType == OrderAccountType.FeeAccount)
                    {
                        List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                        Localization.SetCulture(cardAccounts, Culture);

                        cardAccounts.RemoveAll(m => m.Currency != "AMD");

                        cardAccounts.ForEach(m =>
                        {
                            Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                            if (card != null)
                                m.AccountDescription = card.CardNumber + " " + card.CardType;
                        });

                        accounts.AddRange(cardAccounts);

                    }

                    currentAccounts.RemoveAll(m => m.TypeOfAccount == 282);  //Սահմանափակ հասանելիությամ հաշվիներ

                    return accounts;

                }
                else
                {
                    List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                    Localization.SetCulture(cardAccounts, Culture);

                    currentAccounts.RemoveAll(m => m.TypeOfAccount == 282);   //Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ

                    cardAccounts.ForEach(m =>
                    {
                        Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                        if (card != null)
                            m.AccountDescription = card.CardNumber + " " + card.CardType;
                    });

                    accounts.AddRange(currentAccounts);
                    accounts.AddRange(cardAccounts);
                    return accounts;
                }
            }

            if (orderType == OrderType.BondRegistrationOrder || orderType == OrderType.SecuritiesBuyOrder || orderType == OrderType.SecuritiesSellOrder)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);

                currentAccounts.RemoveAll(m => m.TypeOfAccount == 283 || m.TypeOfAccount == 282);   //Սահմանափակ հասանելիությամ հաշվիներ

                if (currentAccounts != null && currentAccounts.Count > 0)
                    Localization.SetCulture(currentAccounts, Culture);

                currentAccounts = currentAccounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount && m.JointType == 0);

                if (accountType == OrderAccountType.FeeAccount && orderType == OrderType.SecuritiesBuyOrder)
                {
                    List<Card> Cards = Card.GetCards(this.CustomerNumber, ProductQualityFilter.Opened);
                    List<Account> cardAccount = Cards.Where(a => a.Currency == "AMD").Select(a => a.CardAccount).ToList();
                    currentAccounts.AddRange(cardAccount.GroupBy(u => u.AccountNumber).Select(grp => grp.First()));
                }
                accounts.AddRange(currentAccounts);
                return accounts;
            }

            if (orderType == OrderType.DepositCaseActivationOrder)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);

                currentAccounts.RemoveAll(m => m.TypeOfAccount == 283 || m.TypeOfAccount == 282);   //Սահմանափակ հասանելիությամ հաշվիներ

                if (currentAccounts != null && currentAccounts.Count > 0)
                    Localization.SetCulture(currentAccounts, Culture);

                currentAccounts = currentAccounts.FindAll(m => m.Currency == "AMD" && m.AccountType == (ushort)ProductType.CurrentAccount);
                accounts.AddRange(currentAccounts);
                return accounts;

            }
            if (orderType == OrderType.PensionApplicationOrder)
            {
                if (orderSubType == 1)
                {
                    List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
                    if (currentAccounts != null && currentAccounts.Count > 0)
                        currentAccounts = currentAccounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount);
                    Localization.SetCulture(currentAccounts, Culture);

                    currentAccounts.RemoveAll(m => m.TypeOfAccount == 282);   //Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ

                    accounts.AddRange(currentAccounts);
                }
                if (orderSubType == 2)
                {
                    List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                    Localization.SetCulture(cardAccounts, Culture);
                    cardAccounts.ForEach(m =>
                    {
                        Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                        if (card != null && card.Type != 21)
                        {
                            m.AccountDescription = card.CardNumber + " " + card.CardType;
                            accounts.Add(m);
                        }
                    });

                }
                if (orderSubType == 3)
                {
                    List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                    Localization.SetCulture(cardAccounts, Culture);
                    cardAccounts.ForEach(m =>
                    {
                        Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                        if (card != null && card.Type == 21)
                        {
                            m.AccountDescription = card.CardNumber + " " + card.CardType;
                            accounts.Add(m);
                        }
                    });

                }
                if (orderSubType == 4)
                {
                    List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
                    if (currentAccounts != null && currentAccounts.Count > 0)
                        currentAccounts = currentAccounts.FindAll(m => m.AccountType == (ushort)ProductType.SocialSecurityAccount);
                    Localization.SetCulture(currentAccounts, Culture);
                    accounts.AddRange(currentAccounts);
                }


            }
            else if (orderType == OrderType.TransferCallContractOrder)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
                if (currentAccounts != null && currentAccounts.Count > 0)
                    currentAccounts = currentAccounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount && m.Currency == "RUR");
                Localization.SetCulture(currentAccounts, Culture);


                List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                Localization.SetCulture(cardAccounts, Culture);

                cardAccounts.ForEach(m =>
                {
                    Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                    if (card != null)
                        m.AccountDescription = card.CardNumber + " " + card.CardType;
                });

                currentAccounts.RemoveAll(m => m.TypeOfAccount == 283 || m.TypeOfAccount == 282);  //Սահմանափակ հասանելիությամ հաշվիներ

                accounts.AddRange(currentAccounts);
                accounts.AddRange(cardAccounts);
            }
            else if (orderType == OrderType.RosterTransfer)
            {
                List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
                if (currentAccounts != null && currentAccounts.Count > 0)
                    currentAccounts = currentAccounts.FindAll(m => m.AccountType == (ushort)ProductType.CurrentAccount && m.Currency == "AMD");
                Localization.SetCulture(currentAccounts, Culture);

                currentAccounts.RemoveAll(m => m.TypeOfAccount == 283 || m.TypeOfAccount == 282);  //Սահմանափակ հասանելիությամ հաշվիներ
                accounts.AddRange(currentAccounts);
            }
            else
            {
                if (orderType != OrderType.CommunalPayment && orderType != OrderType.EventTicketOrder && (accountType == OrderAccountType.DebitAccount || accountType == OrderAccountType.CreditAccount))
                {
                    //Ընթացիկ հաշիվներ
                    List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);

                    if ((orderType == OrderType.RATransfer && (orderSubType == 3 || orderSubType == 5) &&
                        ((accountType != OrderAccountType.DebitAccount && orderType == OrderType.RATransfer) || orderSubType == 5))
                        || ((orderType == OrderType.RATransfer || orderType == OrderType.BillSplit) && (orderSubType == 1 || orderSubType == 2) && (source == SourceType.MobileBanking || source == SourceType.AcbaOnline))
                        )
                    {
                        currentAccounts.RemoveAll(m => m.TypeOfAccount == 282);   //Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ
                    }
                    else
                    {
                        currentAccounts.RemoveAll(m => m.TypeOfAccount == 283 || m.TypeOfAccount == 282);   //283 Սահմանափակ հասանելիությամ հաշվիներ
                    }


                    //Միայն FrontOffice ծրագրում պետք է երևան տարանցիկ հաշիվները
                    if (source == SourceType.Bank)
                    {
                        currentAccounts.AddRange(Account.GetCustomerTransitAccounts(this.CustomerNumber, ProductQualityFilter.Opened));
                        if (accountType == OrderAccountType.DebitAccount)
                        {
                            currentAccounts.AddRange(Account.GetCreditCodesTransitAccounts(this.CustomerNumber, ProductQualityFilter.Opened));
                        }
                    }

                    if ((source == SourceType.MobileBanking || source == SourceType.AcbaOnline || source == SourceType.AcbaOnlineXML || source == SourceType.ArmSoft) && accountType == OrderAccountType.DebitAccount)
                    {
                        currentAccounts.RemoveAll(m => m.ISDahkCardTransitAccount(m.AccountNumber));
                    }




                    if (!(((orderType == OrderType.RATransfer && orderSubType == 3) || (orderType == OrderType.RATransfer && orderSubType == 5)) && accountType == OrderAccountType.DebitAccount))
                    {
                        currentAccounts.RemoveAll(m => m.AccountType == (ushort)ProductType.CardDahkAccount);
                    }

                    if (!(orderType == OrderType.RATransfer && orderSubType == 5))
                    {
                        currentAccounts.RemoveAll(m => m.TypeOfAccount == 282);  //Սահմանափակ հասանելիությամ հաշվիներ    m.TypeOfAccount == 283 ||
                    }

                    Localization.SetCulture(currentAccounts, Culture);
                    accounts.AddRange(currentAccounts);

                    //Քարտի սպասարկման վարձի գանձման հաշիվներ
                    if (orderType == OrderType.CardServiceFeePayment)
                    {
                        currentAccounts = currentAccounts.FindAll(m => m.Currency == "AMD" && m.AccountType == (ushort)ProductType.CurrentAccount);
                        return currentAccounts;
                    }


                    //Դեբետ հաշիվ Սեփական հաշիվների մեջ 
                    if (accountType == OrderAccountType.DebitAccount && orderType == OrderType.RATransfer && (orderSubType == 3 || orderSubType == 4))
                    {
                        //Առևտրային վարկային գիծ , Օվերդրաֆտ
                        List<CreditLine> creditLines = GetCreditLines(ProductQualityFilter.NotSet);

                        creditLines.ForEach(m =>
                        {
                            if ((m.Type == 18 || m.Type == 25 || m.Type == 60 || m.Type == 46) && (m.Quality != 2 && m.Quality != 5 && m.Quality != 10 && m.Quality != 11))
                            {
                                m.LoanAccount.AccountTypeDescription = m.TypeDescription;
                                accounts.Add(m.LoanAccount);
                            }
                        }
                                            );
                    }

                    short customerType = 0;



                    if (accountType == OrderAccountType.CreditAccount)
                        customerType = GetCustomerType();

                    List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);

                    if (accountType == OrderAccountType.DebitAccount)
                    {
                        //Փակված քարտերի հաշիվներ//////////////////////
                        List<Card> closingCards = Card.GetCards(this.CustomerNumber, ProductQualityFilter.Closed);

                        List<Account> allClosedCardsAccounts = new List<Account>();
                        closingCards.ForEach(
                        m =>
                        {
                            if (m.CardAccount.ClosingDate == null)
                            {
                                allClosedCardsAccounts.Add(m.CardAccount);
                            }
                        }
                        );

                        List<Account> closedCardsAccounts = new List<Account>();
                        foreach (Account closedcardAccount in allClosedCardsAccounts)
                        {
                            if (!cardAccounts.Exists(m => m.AccountNumber == closedcardAccount.AccountNumber) && closedcardAccount.AvailableBalance > 0)
                            {
                                closedCardsAccounts.Add(closedcardAccount);
                            }
                        }

                        foreach (Account closedcardAccount in closedCardsAccounts)
                        {
                            closedcardAccount.AccountType = 111;
                            if (this.Culture.Language != Languages.eng)
                            {
                                closedcardAccount.AccountTypeDescription = "փակված քարտի հաշիվ";
                            }
                            else
                            {
                                closedcardAccount.AccountTypeDescription = "closed card account";
                            }

                        }
                        closedCardsAccounts = closedCardsAccounts.GroupBy(i => i.AccountNumber).Select(g => g.First()).ToList();

                        accounts.AddRange(closedCardsAccounts);
                    }


                    Localization.SetCulture(cardAccounts, Culture);

                    cardAccounts.ForEach(m =>
                    {
                        Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                        if (card != null)
                            m.AccountDescription = card.CardNumber + " " + card.CardType;
                    });

                    accounts.AddRange(cardAccounts);


                    if (accountType == OrderAccountType.CreditAccount)
                    {
                        if ((orderType == OrderType.Convertation && customerType == 6) || (orderType == OrderType.RATransfer && orderSubType == 3))
                        {
                            List<Account> depositAccounts = Account.GetDepositAccounts(this.CustomerNumber);
                            Localization.SetCulture(depositAccounts, Culture);
                            accounts.AddRange(depositAccounts);
                        }
                    }


                    if (accountType == OrderAccountType.DebitAccount && orderType == OrderType.RATransfer && orderSubType == 3)
                    {
                        List<Account> depositAccounts = Account.GetDecreasingDepositAccountList(this.CustomerNumber);
                        Localization.SetCulture(depositAccounts, Culture);
                        accounts.AddRange(depositAccounts);
                    }

                    //Բյուջե փոխանցման հաշիվներ
                    if (accountType == OrderAccountType.DebitAccount && orderSubType == 5)
                    {
                        accounts = accounts.FindAll(m => m.Currency == "AMD");
                    }

                }
                //Միջնորդավճարի հաշիվներ
                else if (accountType == OrderAccountType.FeeAccount || orderType == OrderType.CommunalPayment || orderType == OrderType.EventTicketOrder)
                {
                    //Ընթացիկ հաշիվներ
                    List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);

                    if (orderType == OrderType.OverdraftRepayment)
                        currentAccounts.RemoveAll(m => m.AccountType == (ushort)ProductType.DevelopersAccount);


                    //Davit                                             
                    if (currentAccounts != null && currentAccounts.Count > 0 && !(orderType == OrderType.CommunalPayment || orderType == OrderType.EventTicketOrder || (orderType == OrderType.RATransfer && accountType == OrderAccountType.FeeAccount && orderSubType == 2 && (source == SourceType.MobileBanking || source == SourceType.AcbaOnline))))
                    {
                        currentAccounts = currentAccounts.FindAll(m => m.AccountType != (ushort)ProductType.OtherLabilities && m.AccountType != (ushort)ProductType.AparikTexum && m.AccountType != (ushort)ProductType.CardDahkAccount);
                    }

                    //Eduard
                    if ((source == SourceType.MobileBanking || source == SourceType.AcbaOnline || source == SourceType.AcbaOnlineXML || source == SourceType.ArmSoft) && (orderType == OrderType.CommunalPayment) || orderType == OrderType.EventTicketOrder)
                    {
                        currentAccounts.RemoveAll(m => m.ISDahkCardTransitAccount(m.AccountNumber));
                    }

                    if (orderType == OrderType.CommunalPayment || orderType == OrderType.EventTicketOrder || (orderType == OrderType.RATransfer && accountType == OrderAccountType.FeeAccount && orderSubType == 2 && (source == SourceType.MobileBanking || source == SourceType.AcbaOnline))) //Davit
                        accounts.RemoveAll(m => m.TypeOfAccount == 282);  //283 Սահմանափակ հասանելիությամ հաշվիներ  
                    else
                        accounts.RemoveAll(m => m.TypeOfAccount == 282 || m.TypeOfAccount == 283);

                    //Միայն FrontOffice ծրագրում պետք է երևան տարանցիկ հաշիվները
                    if (source == SourceType.Bank && accountType == OrderAccountType.FeeAccount)
                    {
                        currentAccounts.AddRange(Account.GetCustomerTransitAccounts(this.CustomerNumber, ProductQualityFilter.Opened));
                    }
                    Localization.SetCulture(currentAccounts, Culture);

                    //Միայն դրամային ընթացիկ հաշիվներ 
                    currentAccounts.ForEach(m =>
                    {
                        if (m.Currency == "AMD")
                            accounts.Add(m);
                    });
                    List<Account> cardAccounts = Account.GetCardAccounts(this.CustomerNumber);
                    Localization.SetCulture(cardAccounts, Culture);

                    cardAccounts.RemoveAll(m => m.Currency != "AMD");

                    cardAccounts.ForEach(m =>
                    {
                        Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                        if (card != null)
                            m.AccountDescription = card.CardNumber + " " + card.CardType;
                    });
                    accounts.AddRange(cardAccounts);
                }

                if (includingAttachedCards && accountType == OrderAccountType.DebitAccount && CheckOtherCardAvailability(orderType, orderSubType))
                {
                    List<Account> otherBankCards = Account.GetOtherBankAttachedCards(CustomerNumber);
                    accounts.AddRange(otherBankCards);
                }
            }

            //Հեռացնում ենք ԴԱՀԿ և Սոց. փաթեթի հաշիվները:
            accounts.RemoveAll(m => m.AccountType == 61 || m.AccountType == 57 || m.TypeOfAccount == 282); //282 Ապագա ժամանակաշրջանի տոկոսի կուտակման հաշիվ

            //Ավանդի գրավով վարկի դեպքում վերցնում ենք ընթացիկ և ավանդային հաշիվները
            if ((orderType == OrderType.CreditSecureDeposit || orderType == OrderType.CreditLineSecureDeposit) && accountType == OrderAccountType.DebitAccount)
            {
                accounts.RemoveAll(m => m.AccountType == 11);
                List<Account> depositAccounts = Account.GetDepositAccounts(this.CustomerNumber);
                Localization.SetCulture(depositAccounts, Culture);
                accounts.AddRange(depositAccounts);
            }

            accounts.RemoveAll(m => m.Status == 1);

            if (orderType == OrderType.BillSplit || orderType == OrderType.LinkPaymentOrder)
            {
                accounts.RemoveAll(m => m.Currency != "AMD");
            }

            return accounts;
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UtilityPaymentOrder GetUtilityPaymentOrder(long id)
        {
            UtilityPaymentOrder utilityPaymentOrder = new UtilityPaymentOrder();
            utilityPaymentOrder.Id = id;
            utilityPaymentOrder.CustomerNumber = this.CustomerNumber;
            utilityPaymentOrder.Get();
            Localization.SetCulture(utilityPaymentOrder, Culture);
            return utilityPaymentOrder;
        }

        /// <summary>
        /// Վերադարձնում է չկարդացված հաղորդագրությունների քանակը
        /// </summary>
        /// <returns></returns>
        public int GetUnreadedMessagesCount()
        {
            return Message.GetUnreadedMessagesCount(this.CustomerNumber);
        }

        /// <summary>
        /// Կատարում է կոմունալի հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveUtilityPaymentOrder(UtilityPaymentOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            //Գործարքների օրեկան սահմանաչափի ստուգում
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                order.CustomerNumber = this.CustomerNumber;
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է չկարդացված հաղորդագրությունների քանակը
        /// </summary>
        /// <returns></returns>
        public int GetUnreadMessagesCount(MessageType type)
        {
            return Message.GetUnreadMessagesCount(CustomerNumber, type);
        }
        /// <summary>
        /// Ավանդի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveDepositOrder(DepositOrder order, string userName)
        {
            ActionResult result = new ActionResult();



            if (Source != SourceType.PhoneBanking)
            {

                order.CustomerNumber = CustomerNumber;
                if (String.IsNullOrEmpty(order.OrderNumber))
                    order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

                if (Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.MobileBanking || Source == SourceType.ArmSoft)
                {
                    result = order.Save(userName, Source, User);
                }
                else
                {
                    if (Utility.CheckOperationLimit(order.Amount, order.Deposit.Currency, OneOperationAmountLimit))
                    {


                        result = order.Save(userName, Source, User);
                    }
                    else
                    {
                        result.Errors.Add(new ActionError(66));
                        result.ResultCode = ResultCode.ValidationError;
                    }
                }

            }
            else if (order.Type == OrderType.Deposit)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToOwnAccount))
                {
                    order.CustomerNumber = CustomerNumber;
                    if (String.IsNullOrEmpty(order.OrderNumber) && order.Id == 0)
                        order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

                    result = order.Save(userName, Source, User);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդի դադարեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveDepositTermination(DepositTerminationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.SaveDepositTermination(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է ավանդի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public DepositOrder GetDepositorder(long ID)
        {
            DepositOrder order = new DepositOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Ավանդի հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveDepositOrder(DepositOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                if (Source != SourceType.PhoneBanking)
                {
                    if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
                    {
                        result = order.ApproveDepositOrder(schemaType, userName, User);
                    }
                    else
                    {
                        //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                        result.Errors.Add(new ActionError(74));
                        result.ResultCode = ResultCode.ValidationError;
                    }


                }
                else if (order.Type == OrderType.Deposit)
                {
                    double dayLimit = 0;
                    short isToOwnAccount = 0;

                    if (order.Type == OrderType.Deposit)
                    {
                        isToOwnAccount = 1;
                        dayLimit = this.DayLimitToOwnAccount;
                    }

                    if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now, Source, isToOwnAccount) < dayLimit)
                    {
                        result = order.ApproveDepositOrder(schemaType, userName, User);
                    }
                    else
                    {
                        //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                        result.Errors.Add(new ActionError(74));
                        result.ResultCode = ResultCode.ValidationError;
                    }
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Կատարում է ավանդի դադարեցման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveDepositTermination(DepositTerminationOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();
            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                if (Source != SourceType.PhoneBanking)
                {
                    if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
                    {
                        result = order.Approve(schemaType, userName, User);
                    }
                    else
                    {
                        //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                        result.Errors.Add(new ActionError(74));
                        result.ResultCode = ResultCode.ValidationError;
                    }

                }
                else
                {
                    short isToOwnAccount = 1;
                    double dayLimit = this.DayLimitToOwnAccount;

                    if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now, Source, isToOwnAccount) < dayLimit)
                    {
                        result = order.Approve(schemaType, userName, User);
                    }
                    else
                    {
                        //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                        result.Errors.Add(new ActionError(74));
                        result.ResultCode = ResultCode.ValidationError;
                    }
                }

            }


            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վարկային գծի դադարեցման հայտ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cardId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCreditLineTerminationOrder(CreditLineTerminationOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Տեղեկանքի ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveReferenceOrder(ReferenceOrder order, string userName)
        {
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Տեղեկանքի ստացման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveReferenceOrder(ReferenceOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            //Գործարքների օրեկան սահմանաչափի ստուգում
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        ///  Վերադարձնում է տեղեկանքի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ReferenceOrder GetReferenceOrder(long ID)
        {
            ReferenceOrder order = new ReferenceOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            order.ReferenceTypeDescription = Info.GetReferenceTypes(Culture.Language)
                .Where(f => f.Key == order.ReferenceType)
                .Select(f => f.Value)
                .First();

            order.ReferenceLanguageDescription = Info.GetReferenceLanguages(Culture.Language)
                .Where(f => f.Key == order.ReferenceLanguage.ToString())
                .Select(f => f.Value)
                .First();

            order.ReferenceEmbasyDescription = Info.GetEmbassyList(Culture.Language, new List<ushort>() { order.ReferenceType })
                .Where(f => f.Key == order.ReferenceEmbasy.ToString())
                .Select(f => f.Value)
                .First();

            order.ReferenceFilialDescription = Info.GetFilialList(Culture.Language)
                .Where(f => f.Key == order.ReferenceFilial.ToString())
                .Select(f => f.Value)
                .First();

            if (order.ReferenceReceiptType != ReferenceReceiptTypes.None)
            {
                DataTable dt = Info.GetReferenceReceiptTypes((byte)Culture.Language);
                DataRow[] PoliceRow = dt.Select($"id = {(int)order.ReferenceReceiptType}");
                order.ReferenceReceiptTypeDescription = PoliceRow[0]["description"].ToString();
            }


            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Չեկային գրքույքի  պատվիրման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveChequeBookOrder(ChequeBookOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.Save(userName, Source, User);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Չեկային գրքույքի պատվիրման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveChequeBookOrder(ChequeBookOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                result = order.Approve(schemaType, userName);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է չեկային գրքույքի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ChequeBookOrder GetChequeBookOrder(long ID)
        {
            ChequeBookOrder order = new ChequeBookOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Չեկային գրքույքի ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveChequeBookReceiveOrder(ChequeBookReceiveOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();


            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է չեկային գրքույքի հայտի ստացման  տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ChequeBookReceiveOrder GetChequeBookReceiveOrder(long ID)
        {
            ChequeBookReceiveOrder order = new ChequeBookReceiveOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Գումարի ստացման կամ փոխանցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCashOrder(CashOrder order, string userName)
        {
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Գումարի ստացման կամ փոխանցման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveCashOrder(CashOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է գումարի ստացման կամ փոխանցման հայտի տվյալները:
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CashOrder GetCashOrder(long ID)
        {
            CashOrder order = new CashOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Dictionary<string, string> FilialList = Info.GetFilialList(Culture.Language);
            order.CashFillialDescription = Utility.ConvertAnsiToUnicode(FilialList[order.CashFillial.ToString()]);
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Քաղվածքների էլեկտրոնային ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveStatmentByEmailOrder(StatmentByEmailOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Քաղվածքների էլեկտրոնային ստացման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveStatmentByEmailOrder(StatmentByEmailOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է քաղվածքների էլեկտրոնային ստացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public StatmentByEmailOrder GetStatmentByEmailOrder(long ID)
        {
            StatmentByEmailOrder order = new StatmentByEmailOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            var frequency = Info.GetStatementFrequency().Select($"id = {order.Periodicity}");
            if (frequency.Length != 0)
                order.PeriodicityDescription = Utility.ConvertAnsiToUnicode(frequency[0]["description"].ToString());
            else
                order.PeriodicityDescription = "";

            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Swift հաղորդագրության պատճենի ստացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveSwiftCopyOrder(SwiftCopyOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                result = order.Save(userName, Source, User);
            }
            else if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.Save(userName, Source, User);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Swift հաղորդագրության պատճենի ստացման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveSwiftCopyOrder(SwiftCopyOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            //Գործարքների օրեկան սահմանաչափի ստուգում
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է Swift հաղորդագրության պատճենի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public SwiftCopyOrder GetSwiftCopyOrder(long ID)
        {
            SwiftCopyOrder order = new SwiftCopyOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCustomerDataOrder(CustomerDataOrder order, string userName)
        {
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Տվյալների խմբագրման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveCustomerDataOrder(CustomerDataOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնոմ է տվյալների խմբագրման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CustomerDataOrder GetCustomerDataOrder(long ID)
        {
            CustomerDataOrder order = new CustomerDataOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Վերադարձնում է երրորդ անձանց անունները և CustomerNumber-ները
        /// </summary>
        /// <returns></returns>
        public Dictionary<ulong, string> GetThirdPersons()
        {
            Dictionary<ulong, string> allThirdPerson = new Dictionary<ulong, string>();
            List<ulong> thirdPersonCustomerNumbers = CustomerDB.GetThirdPersonsCustomerNumbers(CustomerNumber, Source);
            if (thirdPersonCustomerNumbers != null)
            {
                for (int i = 0; i < thirdPersonCustomerNumbers.Count; i++)
                {
                    ACBAServiceReference.Customer thirdCustomer = ACBAOperationService.GetCustomer(thirdPersonCustomerNumbers[i]);
                    DateTime? date = (thirdCustomer as PhysicalCustomer).person.birthDate;
                    if (date.Value.AddYears(18).Date > DateTime.Now.Date)
                    {
                        string FullName = (thirdCustomer as PhysicalCustomer).person.fullName.firstName + " " + (thirdCustomer as PhysicalCustomer).person.fullName.lastName;
                        FullName = Utility.ConvertAnsiToUnicode(FullName);
                        ulong ThirdCustomerNumber = (thirdCustomer as PhysicalCustomer).customerNumber;
                        allThirdPerson.Add(ThirdCustomerNumber, FullName);
                    }
                }
            }
            return allThirdPerson;

        }

        /// <summary>
        /// Վերադարձնում է ավանդի տոկոսադրույքը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public DepositOrderCondition GetDepositTariffPercent(DepositOrder order)
        {
            return Deposit.GetDepositOrderCondition(order, Source);
        }

        /// <summary>
        /// Ավանդի գրաֆիկ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<DepositRepayment> GetDepositRepaymentsWithProductId(ulong productId)
        {
            List<DepositRepayment> repayments = Deposit.GetDepositRepayment(productId);
            return repayments;
        }

        /// <summary>
        /// Վերադարձնում է տվյալ ավանդին կցված երրորդ անձանց
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<ulong> GetDepositJointCustomers(ulong productId)
        {
            return Deposit.GetDepositJointCustomers(productId, CustomerNumber);
        }

        /// <summary>
        /// Վերադարձնում է վարկային գիծ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public CreditLine GetCreditLine(ulong productId)
        {
            CreditLine creditline = CreditLine.GetCreditLine(productId, this.CustomerNumber);
            Localization.SetCulture(creditline, this.Culture);
            return creditline;
        }
        /// <summary>
        /// Ունի AcbaOnline թե ոչ
        /// </summary>
        /// <returns></returns>
        public HasHB HasACBAOnline()
        {
            return CustomerDB.HasACBAOnline(this.CustomerNumber);
        }
        /// <summary>
        /// Ունի AcbaOnline թե ոչ
        /// </summary>
        /// <returns></returns>
        public void SendReminderNote(ulong customerNumber)
        {
            Message.SendReminderNote(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է պահատուփերը
        /// </summary>
        /// <returns></returns>
        public List<DepositCase> GetDepositCases(ProductQualityFilter filter)
        {
            List<DepositCase> depositCases = DepositCase.GetDepositCases(this.CustomerNumber, filter);
            Localization.SetCulture(depositCases, this.Culture);
            return depositCases;
        }
        /// <summary>
        /// Վերադարձնում է մեկ պահատուփ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public DepositCase GetDepositCase(ulong productId)
        {
            DepositCase depositCase = DepositCase.GetDepositCase(productId, this.CustomerNumber);
            Localization.SetCulture(depositCase, this.Culture);

            return depositCase;
        }
        /// <summary>
        /// Մեկ հայտի պատմություն 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<OrderHistory> GetOrderHistory(long orderId)
        {
            List<OrderHistory> orderHistory = Order.GetOrderHistory(orderId);
            Localization.SetCulture(orderHistory, this.Culture);
            return orderHistory;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի պարտավորությունները
        /// </summary>
        /// <returns></returns>
        public List<CustomerDebts> GetCustomerDebts()
        {
            return CustomerDebts.GetCustomerDebts(this.CustomerNumber);
        }

        /// <summary>
        /// Ավանդի հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveDepositOrder(DepositOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }



            if (Source != SourceType.PhoneBanking)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Deposit.Currency, OneOperationAmountLimit))
                {
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }


            }
            else if (order.Type == OrderType.Deposit)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToOwnAccount))
                {
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePaymentOrder(PaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (order.Source != SourceType.SSTerminal && order.Source != SourceType.CashInTerminal)
            {
                result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
                if (result.Errors.Count > 0)
                {
                    Localization.SetCulture(result, Culture);
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

            if (Source != SourceType.PhoneBanking)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
                {
                    order.user = User;
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }
            else if (order.SubType == 3)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToOwnAccount))
                {
                    order.user = User;
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }

            else if (order.SubType == 1)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToAnothersAccount))
                {
                    order.user = User;
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Բյուջե վճարման հանձնարարականի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveBudgetPaymentOrder(BudgetPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.SaveAndApprove(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        ///Միջազգային վճարման հանձնարարականի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveInternationalPaymentOrder(InternationalPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                order.user = User;
                result = order.SaveAndApprove(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }

        public ActionResult SaveAndApproveFastTransferPaymentOrder(FastTransferPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            if (order.Currency == "RUB")
            {
                order.Currency = "RUR";
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.SaveAndApprove(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }


        public ActionResult SaveAndApproveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            // result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.SaveAndApprove(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }

        public ActionResult SaveAndApproveCallTransferChangeOrder(TransferByCallChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            result = order.SaveAndApprove(Source, schemaType);

            Localization.SetCulture(result, Culture);
            if (result.ResultCode != ResultCode.Normal)
                return result;
            ActionResult resultPayment = new ActionResult();
            if (result.ResultCode == ResultCode.Normal && order.SubType == 5)
            {
                Transfer transfer = new Transfer();
                transfer.Id = transfer.GetTransferIdFromTransferByCallId(Convert.ToUInt64(order.ReceivedFastTransfer.TransferByCallID));
                transfer.Get(User);

                if (transfer.DebitAccount.Currency == transfer.CreditAccount.Currency)
                {
                    PaymentOrder paymentorder = new PaymentOrder();
                    paymentorder.InitOrderForPaymentOrder(order, transfer, this);
                    resultPayment = SaveAndApprovePaymentOrder(paymentorder, userName, schemaType);
                }
                else
                {
                    CurrencyExchangeOrder currencyorder = new CurrencyExchangeOrder();
                    currencyorder.InitOrderForCurrencyPaymentOrder(order, transfer, this);
                    resultPayment = SaveAndApproveCurrencyExchangeOrder(currencyorder, userName, schemaType);
                }
            }
            if (resultPayment.ResultCode != ResultCode.Normal)
            {
                result.ResultCode = ResultCode.Warning;
                result.Errors = resultPayment.Errors;
            }

            return result;

        }

        /// <summary>
        /// Կոմունալ վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveUtilityPaymentOrder(UtilityPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (order.Source != SourceType.SSTerminal && order.Source != SourceType.CashInTerminal)
            {
                result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
                if (result.Errors.Count > 0)
                {
                    Localization.SetCulture(result, Culture);
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }


        public ActionResult SaveAndApproveReferenceOrder(ReferenceOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public ActionResult SaveAndApproveChequeBookOrder(ChequeBookOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public ActionResult SaveAndApproveCashOrder(CashOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public ActionResult SaveAndApproveSwiftCopyOrder(SwiftCopyOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public ActionResult SaveAndApproveStatmentByEmailOrder(StatmentByEmailOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public ActionResult SaveAndApproveCustomerDataOrder(CustomerDataOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public List<KeyValuePair<ulong, double>> GetAccountJointCustomers(string accountNumber)
        {
            return Account.GetAccountJointCustomers(accountNumber);
        }

        /// <summary>
        /// Ավանդի դադարեցման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveDepositTermination(DepositTerminationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Հաճախորդի ամբողջ ժամկետանցի պատմություն
        /// </summary>
        /// <returns></returns>
        public List<OverdueDetail> GetOverdueDetails()
        {

            List<OverdueDetail> details = OverdueDetail.GetOverdueDetails(CustomerNumber);
            Localization.SetCulture(details, this.Culture);
            return details;
        }

        /// <summary>
        /// Հաճախորդի մեկ պրոդուկտի ժամկետանցի պատմություն
        /// </summary>
        /// <returns></returns>
        public List<OverdueDetail> GetCurrentProductOverdueDetails(long productID)
        {

            List<OverdueDetail> details = OverdueDetail.GetCurrentProductOverdueDetails(CustomerNumber, productID);
            Localization.SetCulture(details, this.Culture);
            return details;
        }

        /// <summary>
        /// Հաճախորդին ուղարկված SMS-ներ
        /// </summary>
        /// <returns></returns>
        public List<SMSMessage> GetSendMessages(DateTime dateFrom, DateTime dateTo)
        {
            List<SMSMessage> smsMessages = SMSMessage.GetSMSMessages(this.CustomerNumber, dateFrom, dateTo);
            return smsMessages;
        }


        /// <summary>
        ///  PhoneBanking-ում ավտորիզացիա անցնող հաճախորդի հեռախոսահամար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public string GetPhoneBankingAuthorizationPhoneNumber(ulong customerNumber)
        {
            string phoneNumber = CustomerDB.GetPhoneBankingAuthorizationPhoneNumber(customerNumber);
            return phoneNumber;
        }
        /// <summary>
        /// Պահպանում է ExternalBanking ի մուտքի և ելքի պատմությունը 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="source"></param>
        public void SaveExternalBankingLogHistory(ushort action, int setNumber, SourceType source, string sessionID, string clientIp, string onlineUserName = " ", string authorizedUserSessionToken = " ")
        {
            CustomerDB.SaveExternalBankingLogHistory(CustomerNumber, action, setNumber, (short)source, sessionID, clientIp, onlineUserName, authorizedUserSessionToken);
        }
        /// <summary>
        /// Հաշվի բացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveAccountOrder(AccountOrder order, string userName)
        {
            ActionResult result;
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            string accountNumber = Account.GetClosingCurrentAccountsNumber(order.CustomerNumber, order.Currency);

            if (!string.IsNullOrEmpty(accountNumber) && (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking))
            {
                AccountReOpenOrder reOpenOrder = new AccountReOpenOrder();

                reOpenOrder.Type = OrderType.CurrentAccountReOpen;
                reOpenOrder.ReopenReasonDescription = "Հաշվի վերաբացում";
                reOpenOrder.StatementDeliveryType = order.StatementDeliveryType;
                reOpenOrder.AccountType = 1;
                reOpenOrder.OrderNumber = order.OrderNumber;
                reOpenOrder.CustomerNumber = order.CustomerNumber;
                reOpenOrder.ReOpeningAccounts = new List<Account>();
                reOpenOrder.ReOpeningAccounts.Add(Account.GetAccount(accountNumber));

                result = reOpenOrder.Save(userName, Source, User);
            }
            else
                result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է հաշվի բացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AccountOrder GetAccountOrder(long ID)
        {
            AccountOrder order = new AccountOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Հաշվի բացման հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveAccountOrder(AccountOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Հաշվի բացման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAccountOrder(AccountOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտը
        /// </summary>
        /// <param name="cardNumber">Քարտի համար</param>
        /// <returns></returns>
        public Card GetCard(string cardNumber)
        {
            Card card = Card.GetCard(cardNumber, this.CustomerNumber);
            Localization.SetCulture(card, this.Culture);
            return card;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտը
        /// </summary>
        /// <param name="cardNumber">Քարտի համար</param>
        /// <returns></returns>
        public Card GetCardMainData(string cardNumber)
        {
            Card card = Card.GetCardMainData(cardNumber);
            return card;
        }

        public ActionResult SaveAndApproveMatureOrder(MatureOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            if (order.Source != SourceType.SSTerminal && order.Source != SourceType.CashInTerminal)
            {
                result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
                if (result.Errors.Count > 0)
                {
                    Localization.SetCulture(result, Culture);
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        public MatureOrder GetMatureOrder(long ID)
        {
            MatureOrder order = new MatureOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Պարբերական Կոմունալ վճարման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.SavePeriodicUtilityPaymentOrder(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;

        }
        /// <summary>
        /// Պարբերական Կոմունալ վճարման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Պարբերական փոխանցման(ՀՀ տարածքում սեփական հաշիվների միջև) վճարման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicPaymentOrder(PeriodicPaymentOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            if (order.PaymentOrder.DebitAccount.IsRestrictedAccount())
            {
                result.Errors.Add(new ActionError(1814));
                result.ResultCode = ResultCode.ValidationError;
                Localization.SetCulture(result, Culture);
                return result;
            }
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null))
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            result = order.SavePeriodicPaymentOrder(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;

        }
        /// <summary>
        /// Պարբերական փոխանցման(ՀՀ տարածքում սեփական հաշիվների միջև) վճարման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApprovePeriodicPaymentOrder(Order order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Պարբերական փոխանցման(ՀՀ տարածքում սեփական հաշիվների միջև) վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndAprovePeriodicPaymentOrder(PeriodicPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            order.CustomerNumber = CustomerNumber;

            if (this.User.userCustomerNumber == order.CustomerNumber)
            {
                //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                result.Errors.Add(new ActionError(544));
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }
        /// <summary>
        /// Պարբերական փոխանցման(կոմունալ) վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndAprovePeriodicUtilityPaymentOrder(PeriodicUtilityPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            order.CustomerNumber = CustomerNumber;

            if (this.User.userCustomerNumber == order.CustomerNumber)
            {
                //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                result.Errors.Add(new ActionError(544));
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Վարկի մարման հայտի պահպանում
        /// </summary>
        /// <param name="order">Վարկի մարման հայտ</param>
        /// <param name="userName">Հաճախորդի մուտքի անուն</param>
        /// <param name="schemaType">Հաստատման սխեմա</param>
        /// <returns></returns>
        public ActionResult SaveMatureOrder(MatureOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վարկի մարման հայտի հաստատում
        /// </summary>
        /// <param name="order">Վարկի մարման հայտ</param>
        /// <param name="userName">Հաճախորդի մուտքային անուն</param>
        /// <param name="schemaType">Հաստատման սխեմա</param>
        /// <returns></returns>
        public ActionResult ApproveMatureOrder(MatureOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ երաշխիքը
        /// </summary>
        /// <param name="productId">երաշխիքի ունիկալ համար</param>
        /// <returns></returns>
        public Guarantee GetGuarantee(ulong productId)
        {
            Guarantee guarantee = Guarantee.GetGuarantee(this.CustomerNumber, productId);

            Localization.SetCulture(guarantee, this.Culture);
            return guarantee;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի երաշխիքների ցուցակը
        /// </summary>
        /// <param name="filter">Պրոդուկտի կարգավիճակի ֆիլտր</param>
        /// <returns></returns>
        public List<Guarantee> GetGuarantees(ProductQualityFilter filter)
        {
            List<Guarantee> guarantees = Guarantee.GetGuarantees(this.CustomerNumber, filter);
            Localization.SetCulture(guarantees, this.Culture);

            return guarantees;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ ակրեդիտիվը
        /// </summary>
        /// <param name="productId">ակրեդիտիվի ունիկալ համար</param>
        /// <returns></returns>
        public Accreditive GetAccreditive(ulong productId)
        {
            Accreditive accreditive = Accreditive.GetAccreditive(this.CustomerNumber, productId);

            Localization.SetCulture(accreditive, this.Culture);
            return accreditive;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի ակրեդիտիվների ցուցակը
        /// </summary>
        /// <param name="filter">Պրոդուկտի կարգավիճակի ֆիլտր</param>
        /// <returns></returns>
        public List<Accreditive> GetAccreditives(ProductQualityFilter filter)
        {
            List<Accreditive> accreditives = Accreditive.GetAccreditives(this.CustomerNumber, filter);
            Localization.SetCulture(accreditives, this.Culture);
            return accreditives;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ վճարված երաշխիքը
        /// </summary>
        /// <param name="productId">վճարված երաշխիքի ունիկալ համար</param>
        /// <returns></returns>
        public PaidGuarantee GetPaidGuarantee(ulong productId)
        {
            PaidGuarantee paidGuarantee = PaidGuarantee.GetPaidGuarantee(this.CustomerNumber, productId);
            Localization.SetCulture(paidGuarantee, this.Culture);
            return paidGuarantee;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի վճարված երաշխիքների ցուցակը
        /// </summary>
        /// <param name="filter">Պրոդուկտի կարգավիճակի ֆիլտր</param>
        /// <returns></returns>
        public List<PaidGuarantee> GetPaidGuarantees(ProductQualityFilter filter)
        {
            List<PaidGuarantee> paidGuarantees = PaidGuarantee.GetPaidGarantees(this.CustomerNumber, filter);
            Localization.SetCulture(paidGuarantees, this.Culture);
            return paidGuarantees;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ վճարված ակրեդիտիվը
        /// </summary>
        /// <param name="productId">վճարված ակրեդիտիվի ունիկալ համար</param>
        /// <returns></returns>
        public PaidAccreditive GetPaidAccreditive(ulong productId)
        {
            PaidAccreditive paidAccreditive = PaidAccreditive.GetPaidAccreditive(this.CustomerNumber, productId);
            Localization.SetCulture(paidAccreditive, this.Culture);
            return paidAccreditive;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի վճարված ակրեդիտիվների ցուցակը
        /// </summary>
        /// <param name="filter">Պրոդուկտի կարգավիճակի ֆիլտր</param>
        /// <returns></returns>
        public List<PaidAccreditive> GetPaidAccreditives(ProductQualityFilter filter)
        {
            List<PaidAccreditive> paidAccreditives = PaidAccreditive.GetPaidAccreditives(this.CustomerNumber, filter);
            Localization.SetCulture(paidAccreditives, this.Culture);
            return paidAccreditives;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ ֆակտորինգը
        /// </summary>
        /// <param name="productId">ֆակտորինգի ունիկալ համար</param>
        /// <returns></returns>
        public Factoring GetFactoring(ulong productId)
        {
            Factoring factoring = Factoring.GetFactoring(this.CustomerNumber, productId);

            Localization.SetCulture(factoring, this.Culture);
            return factoring;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի ֆակտորինգի ցուցակը
        /// </summary>
        /// <param name="filter">Պրոդուկտի կարգավիճակի ֆիլտր</param>
        /// <returns></returns>
        public List<Factoring> GetFactorings(ProductQualityFilter filter)
        {
            List<Factoring> listFactoring = Factoring.GetFactorings(this.CustomerNumber, filter);
            Localization.SetCulture(listFactoring, this.Culture);
            return listFactoring;

        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ վճարված ֆակտորինգը
        /// </summary>
        /// <param name="productId">վճարված ակրեդիտիվի ունիկալ համար</param>
        /// <returns></returns>
        public PaidFactoring GetPaidFactoring(ulong productId)
        {
            PaidFactoring paidFactoring = PaidFactoring.GetPaidFactoring(this.CustomerNumber, productId);
            Localization.SetCulture(paidFactoring, this.Culture);
            return paidFactoring;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի վճարված ֆակտորինգների ցուցակը
        /// </summary>
        /// <param name="filter">Պրոդուկտի կարգավիճակի ֆիլտր</param>
        /// <returns></returns>
        public List<PaidFactoring> GetPaidFactorings(ProductQualityFilter filter)
        {
            List<PaidFactoring> paidFactorings = PaidFactoring.GetPaidFactorings(this.CustomerNumber, filter);
            Localization.SetCulture(paidFactorings, this.Culture);
            return paidFactorings;
        }

        public bool CheckCardOwner(string cardNumber)
        {
            return Card.CheckCardOwner(cardNumber, this.CustomerNumber);
        }

        /// <summary>
        /// Հաշվի փակման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult CloseAccountOrder(AccountClosingOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.CloseAccountOrder(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Հաշվի փակման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveAccountClosing(AccountClosingOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Հաշվի փակման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAccountClosing(AccountClosingOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է հաշվի փակման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AccountClosingOrder GetAccountClosingOrder(long ID)
        {
            AccountClosingOrder order = new AccountClosingOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Պարբերական Բյուջե վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndAprovePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            order.CustomerNumber = CustomerNumber;

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Պարբերական Բյուջե հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null))
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            ActionResult result = order.SavePeriodicBudgetPaymentOrder(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;

        }
        /// <summary>
        /// Պարբերական Բյուջե հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApprovePeriodicBudgetPaymentOrder(PeriodicBudgetPaymentOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի փակման հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardClosingOrder GetCardClosingOrder(long ID)
        {
            CardClosingOrder order = new CardClosingOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();

            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Քարտի փակման հայտի  հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveCardClosingOrder(CardClosingOrder order, short schemaType, string userName, string clientIp)
        {
            ActionResult result = order.Approve(schemaType, userName, order.user, clientIp);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի փակման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCardClosingOrder(CardClosingOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի փակման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardClosingOrder(CardClosingOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public List<string> GetCredentialClosingWarnings(ulong assignId)
        {
            return CredentialTerminationOrder.GetCredentialClosingWarnings(assignId, Culture);
        }

        public List<string> GetCardClosingWarnings(ulong productId)
        {
            return CardClosingOrder.GetCardClosingWarnings(productId, this.CustomerNumber, Culture);
        }
        /// <summary>
        /// Վերադարձնում է Սեփական հաշիվների միջև և ՀՀ տարածքում պարբերական հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PeriodicPaymentOrder GetPeriodicPaymentOrder(long ID)
        {
            PeriodicPaymentOrder order = new PeriodicPaymentOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Վերադարձնում է Բյուջե պարբերական հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PeriodicBudgetPaymentOrder GetPeriodicBudgetPaymentOrder(long ID)
        {
            PeriodicBudgetPaymentOrder order = new PeriodicBudgetPaymentOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            DataRow[] LTARow = Info.GetLTACodes().Select($"Code = {order.BudgetPaymentOrder.LTACode}");
            if (order.BudgetPaymentOrder.LTACode != 0)
                order.BudgetPaymentOrder.LTACodeDescription = Culture.Language == Languages.hy ? Utility.ConvertAnsiToUnicode(LTARow[0]["Description"].ToString()) : Utility.ConvertAnsiToUnicode(LTARow[0]["Description_engl"].ToString());
            if (order.BudgetPaymentOrder.PoliceCode != 0)
            {
                DataTable dt = Info.GetPoliceCodes(order.BudgetPaymentOrder.ReceiverAccount.AccountNumber);
                DataRow[] PoliceRow = dt.Select($"code = {order.BudgetPaymentOrder.PoliceCode}");
                if (Convert.ToUInt16(PoliceRow[0]["ACKIND"].ToString()) == 1)
                    order.BudgetPaymentOrder.PoliceCodeDescription = Culture.Language == Languages.hy ? Utility.ConvertAnsiToUnicode(PoliceRow[0]["Description"].ToString()) : PoliceRow[0]["Description_eng"].ToString();
                else
                    order.BudgetPaymentOrder.PoliceCodeDescription = Utility.ConvertAnsiToUnicode(PoliceRow[0]["Description"].ToString());
            }
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Վերադարձնում է Կոմունալ պարբերական հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PeriodicUtilityPaymentOrder GetPeriodicUtilityPaymentOrder(long ID)
        {
            PeriodicUtilityPaymentOrder order = new PeriodicUtilityPaymentOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            DataTable dt = Info.GetPeriodicsSubTypes(Culture.Language);
            order.ServicePaymentTypeDescription = Utility.ConvertAnsiToUnicode(dt.Select("amount_type = " + order.ServicePaymentType)[0]["Description"].ToString());
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Պարբերականի փակման հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PeriodicTerminationOrder GetPeriodicTerminationOrder(long ID)
        {
            PeriodicTerminationOrder order = new PeriodicTerminationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();

            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Պարբերականի փակման հայտի  հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApprovePeriodicTerminationOrder(PeriodicTerminationOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Պարբերականի փակման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicTerminationOrder(PeriodicTerminationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Պարբերականի փակման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePeriodicTerminationOrder(PeriodicTerminationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Հաշվի վերաբացման հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveAccountReOpenOrder(AccountReOpenOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Հաշվի վերաբացման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAccountReOpenOrder(AccountReOpenOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի Վերաբացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AccountReOpenOrder GetAccountReOpenOrder(long ID)
        {
            AccountReOpenOrder order = new AccountReOpenOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերդարձնում է նոր ավանդի համար հաշիվները կախված հաշվի տեսակից
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<Account> GetAccountsForNewDeposit(DepositOrder order)
        {
            List<Account> currentAccounts = Account.GetAccountsForNewDeposit(order);
            Localization.SetCulture(currentAccounts, Culture);
            return currentAccounts;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի լրացուցիչ տվյալները
        /// </summary>
        /// <returns></returns>
        public List<AdditionalDetails> GetAccountAdditionalDetails(string accountNumber)
        {
            Account account = new Account();
            account.AccountNumber = accountNumber;
            return account.GetAccountAdditions();
        }

        /// <summary>
        /// Վերադարձնում է քաղվածքների ստացման տեսակը
        /// </summary>
        /// <returns></returns>
        public int GetAccountStatementDeliveryType(string accountNumber)
        {
            Account account = new Account();
            account.AccountNumber = accountNumber;
            return account.GetAccountStatementDeliveryType();
        }

        public List<string> GetAccountOpenWarnings()
        {
            return AccountOrder.GetAccountOpenWarnings(this.CustomerNumber, Culture);
        }

        public ActionResult SaveAndApproveAccountDataChangeOrder(AccountDataChangeOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public List<string> GetReceiverAccountWarnings(string accountNumber)
        {
            return PaymentOrder.GetReceiverAccountWarnings(accountNumber, Culture, this.CustomerNumber);
        }

        public List<OPPerson> GetOrderOPPersons(string accountNumber, OrderType orderType)
        {
            List<OPPerson> opPersons = new List<OPPerson>();




            int customerType = this.GetCustomerType();

            if (customerType == (int)CustomerTypes.physical)
            {
                ACBAServiceReference.PhysicalCustomer customer = (ACBAServiceReference.PhysicalCustomer)this.GetCustomerData();


                OPPerson opPerson = new OPPerson();
                opPerson.CustomerNumber = this.CustomerNumber;
                opPerson.PersonName = Utility.ConvertAnsiToUnicode(customer.person.fullName.firstName);
                opPerson.PersonLastName = Utility.ConvertAnsiToUnicode(customer.person.fullName.lastName);
                opPerson.PersonDocument = Utility.ConvertAnsiToUnicode(customer.DefaultDocument);

                if (customer.person.documentList != null && customer.person.documentList.Count != 0)
                {
                    if (customer.person.documentList.Exists(cd => cd.documentType.key == 56))
                    {
                        opPerson.PersonSocialNumber = Utility.ConvertAnsiToUnicode(customer.person.documentList.Find(cd => cd.documentType.key == 56).documentNumber);
                    }
                    else if (customer.person.documentList.Exists(cd => cd.documentType.key == 57))
                    {
                        opPerson.PersonNoSocialNumber = Utility.ConvertAnsiToUnicode(customer.person.documentList.Find(cd => cd.documentType.key == 57).documentNumber);
                    }
                    CustomerDocument pass = customer.person.documentList.Find(cd => cd.documentNumber == customer.DefaultDocument && cd.defaultSign);
                    if (pass != null)
                    {
                        opPerson.PersonDocument += "," + pass.givenBy + "," + String.Format("{0:dd/MM/yy}", pass.givenDate);
                    }

                }
                if (!string.IsNullOrEmpty(opPerson.PersonDocument))
                {
                    opPerson.PersonDocument = Utility.ConvertAnsiToUnicode(opPerson.PersonDocument);
                }
                if (customer.person.addressList != null && customer.person.addressList.Any() && customer.person.addressList.Exists(add => add.addressType.key == 2))
                {
                    opPerson.PersonAddress = Utility.ConvertAnsiToUnicode(Utility.GenerateAddress(customer.person.addressList.Find(add => add.addressType.key == 2)));
                }

                opPerson.PersonBirth = customer.person.birthDate;

                if (customer.person.PhoneList != null && customer.person.PhoneList.Count != 0)
                {
                    CustomerPhone phone;
                    if (customer.person.PhoneList.Exists(ph => ph.phoneType.key == 1))
                    {
                        phone = customer.person.PhoneList.Find(ph => ph.phoneType.key == 1);
                        opPerson.PersonPhone = phone.phone.countryCode + phone.phone.areaCode + phone.phone.phoneNumber;
                    }
                    else if (customer.person.PhoneList.Exists(ph => ph.phoneType.key == 2))
                    {
                        phone = customer.person.PhoneList.Find(ph => ph.phoneType.key == 2);
                        opPerson.PersonPhone = phone.phone.countryCode + phone.phone.areaCode + phone.phone.phoneNumber;
                    }
                    else if (customer.person.PhoneList.Exists(ph => ph.phoneType.key == 3))
                    {
                        phone = customer.person.PhoneList.Find(ph => ph.phoneType.key == 3);
                        opPerson.PersonPhone = phone.phone.countryCode + phone.phone.areaCode + phone.phone.phoneNumber;
                    }

                }

                opPerson.PersonResidence = customer.residence.key;

                if (customer.person.emailList != null && customer.person.emailList.Count != 0)
                {
                    for (int i = 0; i < customer.person.emailList.Count; i++)
                    {
                        if ((opPerson.PersonEmail + customer.person.emailList[i].email.emailAddress).Length < 50)
                        {
                            opPerson.PersonEmail += customer.person.emailList[i].email.emailAddress;
                            if (i < customer.person.emailList.Count - 1)
                            {
                                opPerson.PersonEmail += ",";
                            }
                        }
                    }
                }

                opPersons.Add(opPerson);
            }

            if (orderType != OrderType.CashOutFromTransitAccountsOrder)
                opPersons.AddRange(Account.GetAccountAssigneeCustomers(accountNumber, orderType));

            opPersons.ForEach(m =>
            {
                if (m.CustomerNumber != this.CustomerNumber)
                {
                    ACBAServiceReference.PhysicalCustomer assingneeCustomer = (ACBAServiceReference.PhysicalCustomer)GetCustomer(m.CustomerNumber);
                    if (assingneeCustomer.person.documentList != null && assingneeCustomer.person.documentList.Count != 0)
                    {
                        if (assingneeCustomer.person.documentList.Exists(cd => cd.documentType.key == 56))
                        {
                            m.PersonSocialNumber = Utility.ConvertAnsiToUnicode(assingneeCustomer.person.documentList.Find(cd => cd.documentType.key == 56).documentNumber);
                        }
                        else if (assingneeCustomer.person.documentList.Exists(cd => cd.documentType.key == 57))
                        {
                            m.PersonNoSocialNumber = Utility.ConvertAnsiToUnicode(assingneeCustomer.person.documentList.Find(cd => cd.documentType.key == 57).documentNumber);
                        }
                    }
                    if (assingneeCustomer.person.addressList != null && assingneeCustomer.person.addressList.Any() && assingneeCustomer.person.addressList.Exists(add => add.addressType.key == 2))
                    {
                        m.PersonAddress = Utility.ConvertAnsiToUnicode(Utility.GenerateAddress(assingneeCustomer.person.addressList.Find(add => add.addressType.key == 2)));
                    }

                    m.PersonBirth = assingneeCustomer.person.birthDate;

                    if (assingneeCustomer.person.PhoneList != null && assingneeCustomer.person.PhoneList.Count != 0)
                    {
                        CustomerPhone phone;
                        if (assingneeCustomer.person.PhoneList.Exists(ph => ph.phoneType.key == 1))
                        {
                            phone = assingneeCustomer.person.PhoneList.Find(ph => ph.phoneType.key == 1);
                            if (phone != null)
                            {
                                m.PersonPhone = phone.phone.countryCode + phone.phone.areaCode + phone.phone.phoneNumber;
                            }

                        }
                        else
                        {
                            phone = assingneeCustomer.person.PhoneList.Find(ph => ph.phoneType.key == 2);
                            if (phone != null)
                            {
                                m.PersonPhone = phone.phone.countryCode + phone.phone.areaCode + phone.phone.phoneNumber;
                            }

                        }

                    }
                    m.PersonResidence = assingneeCustomer.residence.key;

                    if (assingneeCustomer.person.emailList != null && assingneeCustomer.person.emailList.Count != 0)
                    {
                        for (int i = 0; i < assingneeCustomer.person.emailList.Count; i++)
                        {
                            if ((m.PersonEmail + assingneeCustomer.person.emailList[i].email.emailAddress).Length < 50)
                            {
                                m.PersonEmail += assingneeCustomer.person.emailList[i].email.emailAddress;
                                m.PersonEmail += ",";
                            }

                        }
                        if (!string.IsNullOrEmpty(m.PersonEmail))
                            m.PersonEmail = m.PersonEmail.Substring(0, m.PersonEmail.Length - 1);
                    }
                }
            });

            return opPersons;
        }


        public ACBAServiceReference.Customer GetCustomerData()
        {
            var customer = ACBAOperationService.GetCustomer(this.CustomerNumber);
            return customer;

        }

        public List<string> GetCustomerDocumentWarnings(ulong customerNumber)
        {
            List<string> warnings = new List<string>();
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCustomerDocument(customerNumber));
            Localization.SetCulture(result, Culture);

            result.Errors.ForEach(m =>
            {
                warnings.Add(m.Description);
            }
            );
            return warnings;
        }

        public double GetThreeMonthLoanRate(ulong productId)
        {
            bool exists = Loan.CheckLoanExists(CustomerNumber, productId);
            double rate = 0;
            if (exists)
            {
                rate = MatureOrder.GetThreeMonthLoanRate(productId);
            }
            return rate;
        }

        public double GetLoanMatureCapitalPenalty(MatureOrder order, ACBAServiceReference.User user)
        {
            double loanMature = 0;
            loanMature = MatureOrder.GetLoanMatureCapitalPenalty(order, user);

            return loanMature;
        }


        /// <summary>
        /// Վերադարձնում է հաշվի տվյալների խմբագրման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AccountDataChangeOrder GetAccountDataChageOrder(long ID)
        {
            AccountDataChangeOrder order = new AccountDataChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        public static bool IsCustomerUpdateExpired(ulong customerNumber)
        {
            byte expired = ACBAOperationService.CheckCustomerUpdateExpired(customerNumber);
            return expired == 1;

        }
        public double GetOrderServiceFee(OrderType type, int urgent)
        {
            return ChequeBookOrder.GetOrderServiceFee(this.CustomerNumber, type, urgent);
        }
        /// <summary>
        /// Ստուգում է վարկերի առկայությունը
        /// </summary>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <returns></returns>
        public static bool CheckForProvision(ulong customerNumber)
        {
            return CustomerDB.CheckForProvision(customerNumber);
        }


        public ActionResult SaveAndApproveTransitPaymentOrder(TransitPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }

        public TransitPaymentOrder GetTransitPaymentOrder(long ID)
        {
            TransitPaymentOrder order = new TransitPaymentOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult ConfirmOrder(long orderID)
        {
            Order order = Order.GetOrder(orderID, CustomerNumber);
            string isCardDepartment = null;
            User.AdvancedOptions.TryGetValue("isCardDepartment", out isCardDepartment);
            if (isCardDepartment == "1")
            {
                order = Order.GetOrder(orderID);
            }

            ActionResult result = new ActionResult();

            if (order == null)
            {
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(636));
            }

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            else
            {
                List<OrderHistory> orderHistories = Order.GetOrderHistory(order.Id).FindAll(m => m.Quality == OrderQuality.Sent3 || m.Quality == OrderQuality.Sent || m.Quality == OrderQuality.TransactionLimitApprovement);

                if (!orderHistories.Exists(m => (short)m.ChangeUserId == User.userID))
                {
                    if (order.Source == SourceType.Bank || order.Source == SourceType.SSTerminal || order.Source == SourceType.CashInTerminal)
                    {
                        //TODO: Ավելացնել սյուն, և ստուգել այդ սյան արժեքով
                        if (!(order.Type == OrderType.AddFondOrder || order.Type == OrderType.ChangeFondOrder || order.Type == OrderType.ChangeFTPRateOrder || order.Type == OrderType.InterestMarginOrder || order.Type == OrderType.ArcaCardsTransactionOrder))
                        {
                            if (!(User.AdvancedOptions.ContainsKey("withCashRegister") && orderHistories.Exists(m => User.CanSendToCashier(m.ChangeUserId))))
                            {
                                //Գործարքի կատարումը հնարավոր է միայն մուտքագրողի կողմից:
                                result.Errors.Add(new ActionError(770));
                                Localization.SetCulture(result, Culture);
                                result.ResultCode = ResultCode.ValidationError;
                                return result;
                            }

                        }
                    }
                }
            }


            result = order.Confirm(User);
            Localization.SetCulture(result, Culture);

            return result;
        }


        /// <summary>
        /// Արտարժույթի փոխանակման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCurrencyExchangeOrder(CurrencyExchangeOrder order, string userName)
        {
            ActionResult result = new ActionResult();




            if (Source != SourceType.PhoneBanking)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
                {
                    result = order.Save(userName, Source, User);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }

            }
            else if (order.Type == OrderType.Convertation)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToOwnAccount))
                {
                    result = order.Save(userName, Source, User);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }

            else if (order.Type == OrderType.InBankConvertation)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToAnothersAccount))
                {
                    result = order.Save(userName, Source, User);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }



            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Արտարժույթի փոխանակման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCurrencyExchangeOrder(CurrencyExchangeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (order.Source != SourceType.SSTerminal && order.Source != SourceType.CashInTerminal)
            {
                result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
                if (result.Errors.Count > 0)
                {
                    Localization.SetCulture(result, Culture);
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

            if (Source != SourceType.PhoneBanking)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
                {
                    order.user = User;
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }


            }
            else if (order.Type == OrderType.Convertation)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToOwnAccount))
                {
                    order.user = User;
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }

            else if (order.Type == OrderType.InBankConvertation)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToAnothersAccount))
                {
                    order.user = User;
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }

            Localization.SetCulture(result, Culture);
            return result;

        }
        public double GetCustomerCashOuts(string currency)
        {
            return CustomerDB.GetCustomerCashOuts(this.CustomerNumber, currency);
        }

        /// <summary>
        /// Վերադարդարձնում է մանրադրամը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public CurrencyExchangeOrder GetShortChangeAmount(CurrencyExchangeOrder order)
        {
            return order.GetShortChangeAmount();

        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի փաստաթղթերի ցանկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<CustomerDocument> GetCustomerDocumentList(ulong customerNumber)
        {
            return ACBAOperationService.GetCustomerDocumentList(customerNumber);
        }

        public int GetCardType(string cardNumber)
        {
            return Card.GetCardType(cardNumber, this.CustomerNumber);
        }

        public ActionResult SaveAndApproveHBActivationOrder(HBActivationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveHBActivationOrder(HBActivationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        ///POS տերմինալով կանխիկացում բանկում
        /// </summary>
        /// <param name="order">հանձնարարական</param>
        /// <param name="userName">Օգտագործողի անուն</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCashPosPaymentOrder(CashPosPaymentOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);

            return result;
        }


        public ActionResult ApproveHBActivationOrder(HBActivationOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User, Source);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public double GetHBServiceFee(DateTime setDate, HBServiceFeeRequestTypes requestType, HBTokenTypes tokenType, HBTokenSubType tokenSubType)
        {
            return HBToken.GetHBServiceFee(CustomerNumber, setDate, requestType, tokenType, tokenSubType);
        }

        public List<HBActivationRequest> GetHBRequests()
        {
            return HBActivationOrder.GetHBRequests(CustomerNumber);
        }

        public HBActivationOrder GetHBActivationOrder(long Id)
        {
            HBActivationOrder order = new HBActivationOrder();
            order.Id = Id;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Ավանդի գրավով վարկային պրոդուկտի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveLoanProductOrder(LoanProductOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LoanProductOrder GetLoanOrder(long ID)
        {
            LoanProductOrder order = new LoanProductOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetLoanOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Ավանդի գրավով վարկային գծի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LoanProductOrder GetCreditLineOrder(long ID)
        {
            LoanProductOrder order = new LoanProductOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetCreditLineOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Ավանդի գրավով վարկային պրոդուկտի հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveLoanProductOrder(LoanProductOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդի գրավով վարկային պրոդուկտի հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveLoanProductOrder(LoanProductOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի տվյալ պահին տվյալ արժույթով հասանելի գումարը
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static double GetCustomerAvailableAmount(ulong customerNumber, string currency)
        {
            return CustomerDB.GetCustomerAvailableAmount(customerNumber, currency);
        }



        public static bool HasCommitment(ulong customerNumber)
        {
            return CustomerDB.HasCustomerCommitment(customerNumber);
        }

        public static short GetCustomerSyntheticStatus(ulong customerNumber)
        {
            return CustomerDB.GetCustomerSyntheticStatus(customerNumber);
        }


        /// <summary>
        /// Հաշվի սառեցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAccountFreezeOrder(AccountFreezeOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            if (result.ResultCode == ResultCode.Normal && AutomateCardBlockingUnblocking.FreezingReasonsForBlocking.Contains(order.FreezeReason))
            {
                Card freezingCard = Card.GetCard(order.FreezeAccount);
                if (freezingCard != null)
                {
                    try
                    {
                        foreach (var cardNumber in AutomateCardBlockingUnblocking.GetAllCardNumbers(freezingCard, CustomerNumber))
                        {
                            ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, AutomateCardBlockingUnblocking.GetCardTransactionReasonByFreezeReasonId(order.FreezeReason), order.OperationDate, order.RegistrationDate);
                            SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի սառեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AccountFreezeOrder GetAccountFreezeOrder(long ID)
        {
            AccountFreezeOrder order = new AccountFreezeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Հաշվի ապասառեցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAccountUnfreezeOrder(AccountUnfreezeOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            if (result.ResultCode == ResultCode.Normal)
            {
                List<AccountFreezeDetails> freezeDetails = AccountFreezeDetails.GetAccountFreezeHistory(order.FreezedAccount.AccountNumber, 1, 0);
                if (freezeDetails.Count == 0 && !CustomerDB.HasBankrupt(CustomerNumber) && AutomateCardBlockingUnblocking.FreezingReasonsForBlocking.Contains(order.UnfreezeReason))
                {
                    Card unFreezingCard = Card.GetCard(order.FreezedAccount);
                    if (unFreezingCard != null)
                    {
                        foreach (var cardNumber in AutomateCardBlockingUnblocking.GetAllCardNumbers(unFreezingCard, CustomerNumber))
                        {
                            ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Unblock, AutomateCardBlockingUnblocking.GetCardTransactionReasonByFreezeReasonId(order.UnfreezeReason), order.OperationDate, order.RegistrationDate);
                            SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                        }
                    }
                }
            }

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի ապասառեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AccountUnfreezeOrder GetAccountUnfreezeOrder(long ID)
        {
            AccountUnfreezeOrder order = new AccountUnfreezeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Հաճախորդին տրամադրվող ծառայությունների դիմաց միջնորդավճարի գանդձման հայտ
        /// </summary>
        /// <param name="order">հանձնարարական</param>
        /// <param name="userName">Օգտագործողի անուն</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveFeeForServiceProvidedOrder(FeeForServiceProvidedOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);

            return result;
        }

        /// <summary>
        /// Ունի PhoneBanking թե ոչ
        /// </summary>
        /// <returns></returns>
        public HasHB HasPhoneBanking()
        {
            return CustomerDB.HasPhoneBanking(this.CustomerNumber);
        }

        /// <summary>
        /// Վերադարձնում է Հաճախորդին տրամադրվող ծառայությունների դիմաց միջնորդավճարի գանդձման հայտը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FeeForServiceProvidedOrder GetFeeForServiceProvidedOrder(long id)
        {
            FeeForServiceProvidedOrder paymentOrder = new FeeForServiceProvidedOrder();
            paymentOrder.Id = id;
            paymentOrder.CustomerNumber = this.CustomerNumber;
            paymentOrder.Get();
            Localization.SetCulture(paymentOrder, Culture);
            return paymentOrder;
        }

        /// <summary>
        /// Վերադարձնում է POS տերմինալով կանխիկացման հանձնարարականը 
        /// </summary>
        /// <param name="id">օրդերի համարը</param>
        /// <returns></returns>
        public CashPosPaymentOrder GetCashPosPaymentOrder(long id)
        {
            CashPosPaymentOrder cashPosPaymentOrder = new CashPosPaymentOrder();
            cashPosPaymentOrder.Id = id;
            cashPosPaymentOrder.CustomerNumber = this.CustomerNumber;
            cashPosPaymentOrder.Get();
            Localization.SetCulture(cashPosPaymentOrder, Culture);
            return cashPosPaymentOrder;
        }

        /// <summary>
        /// Ստանում է քարտային հաշվի չվճարված դրական տոկոսագումարի վճարման հայտի տվյալները
        /// </summary>
        /// <param name="ID">Վճարման հայտի ID</param>
        /// <returns>Վճարման հայտ</returns>
        public CardUnpaidPercentPaymentOrder GetCardUnpaidPercentPaymentOrder(long ID)
        {
            CardUnpaidPercentPaymentOrder order = new CardUnpaidPercentPaymentOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();

            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Քարտային հաշվի չվճարված դրական տոկոսագումարի վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order">Վճարման հայտ</param>
        /// <param name="userName">Հաճախորդի մուտքանուն(login)</param>
        /// <param name="schemaType">Հայտի հաստատման սխեմա</param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardUnpaidPercentPaymentOrder(CardUnpaidPercentPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SaveLoanProductActivationOrder(LoanProductActivationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LoanProductActivationOrder GetLoanProductActivationOrder(long ID)
        {
            LoanProductActivationOrder order = new LoanProductActivationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Ավանդի գրավով վարկային պրոդուկտի հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveLoanProductActivationOrder(LoanProductActivationOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդի գրավով վարկային պրոդուկտի հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveLoanProductActivationOrder(LoanProductActivationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }





        /// <summary>
        /// Գործարքի հեռացման հայտ
        /// </summary>
        /// <param name="order">Հեռացման հայտ</param>
        /// <param name="userName">Օգտագործողի անուն</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveRemovalOrder(RemovalOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;

            ActionResult result = new ActionResult();
            if (Source != SourceType.MobileBanking && Source != SourceType.AcbaOnline)
                result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Source != SourceType.MobileBanking && Source != SourceType.AcbaOnline && !order.CanRemoveOrder(order.RemovingOrderId, User.userID))
            {
                //Հրաժարման հայտի ձևակերպման հասանելիութուն ունի միայն հայտը մուտքագրողը և տվյալ մասնաճյուղի ԳԳՀՄ/ՆՀՊ-ն։
                result.Errors.Add(new ActionError(1739));
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);

            return result;
        }

        /// <summary>
        /// Վերադարձնում է գործարքի հեռացման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RemovalOrder GetRemovalOrder(long id)
        {
            RemovalOrder removalOrder = new RemovalOrder();
            removalOrder.Id = id;
            removalOrder.CustomerNumber = this.CustomerNumber;
            removalOrder.Get();
            Localization.SetCulture(removalOrder, Culture);
            return removalOrder;
        }

        public LoanMainContract GetLoanMainContract(ulong productId)
        {
            return Loan.GetLoanMainContract(productId, this.CustomerNumber);

        }

        /// <summary>
        /// Վերադարձնում է ավանդի տվյալների աղբյուրը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public SourceType GetDepositSource(ulong productId)
        {
            return Deposit.GetDepositSource(productId, CustomerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի բացման աղբյուրը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public SourceType GetAccountSource(string accountNumber)
        {
            return Account.GetAccountSource(accountNumber, CustomerNumber);
        }

        public List<LoanMainContract> GetCreditLineMainContract()
        {
            return CreditLine.GetCreditLineMainContract(CustomerNumber);
        }

        public List<LoanProductProlongation> GetLoanProductProlongations(ulong productId)
        {
            return LoanProduct.GetLoanProductProlongations(productId);
        }

        public List<Claim> GetProductClaims(ulong productId, short productType)
        {
            List<Claim> claims = null;
            bool loanExists = false;
            CreditLine line = null;
            if (productType == 1)
            {
                loanExists = Loan.CheckLoanExists(this.CustomerNumber, productId);
            }
            else if (productType == 2)
            {
                line = CreditLine.GetCreditLine(productId, this.CustomerNumber);
                productId = (ulong)line.ProductId;
            }


            if (loanExists || line != null)
            {
                claims = Claim.GetProductClaims(productId);
                Localization.SetCulture(claims, Culture);
                foreach (var claim in claims)
                {
                    Localization.SetCulture(claim.Events, Culture);
                    foreach (var events in claim.Events)
                    {
                        if (events.EventTax.Count > 0)
                        {
                            Localization.SetCulture(events.EventTax[0], Culture);
                        }

                    }
                }
            }
            return claims;
        }

        public List<ClaimEvent> GetClaimEvents(int claimNumber)
        {
            List<ClaimEvent> claimEvents = null;

            claimEvents = ClaimEvent.GetClaimEvents(claimNumber);
            Localization.SetCulture(claimEvents, Culture);
            return claimEvents;
        }

        public Tax GetTax(int claimNumber, int eventNumber)
        {
            Tax tax = Tax.GetTax(claimNumber, eventNumber);
            Localization.SetCulture(tax, Culture);
            return tax;
        }
        /// <summary>
        /// Պետ. տուրքի հաշվարկ
        /// </summary>
        /// <param name="claimNumber"></param>
        /// <param name="eventNumber"></param>
        /// <returns></returns>
        public ProblemLoanCalculationsDetail GetProblemLoanCalculationsDetail(int claimNumber, int eventNumber)
        {
            return Tax.GetProblemLoanCalculationsDetail(claimNumber, eventNumber);
        }




        /// <summary>
        /// Վերադարձնում է վարկի միջնորդավճար և այլ մուծումների ցանկը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<ProductOtherFee> GetProductOtherFees(ulong productId)
        {
            List<ProductOtherFee> otherFees = Loan.GetProductOtherFees(productId, this.CustomerNumber);
            return otherFees;
        }

        /// <summary>
        /// Վերադարձնում է հաշվին կցված կենսաթոշակի դիմում համաձայնագրի առկայությունը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public bool HasAccountPensionApplication(string accountNumber)
        {
            bool hasPensionApplication = false;

            Account account = Account.GetAccount(accountNumber, this.CustomerNumber);

            if (account != null)
            {
                hasPensionApplication = account.HasPensionApplication();
            }

            return hasPensionApplication;
        }


        /// <summary>
        /// Վերադարձնում է քարտի սպասարկման վարձի գրաֆիկը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<CardServiceFeeGrafik> GetCardServiceFeeGrafik(ulong productId)
        {
            List<CardServiceFeeGrafik> cardServiceFeeGrafik = null;
            Card card = Card.GetCardMainData(productId, this.CustomerNumber);

            if (card != null)
            {
                cardServiceFeeGrafik = Card.GetCardServiceFeeGrafik(card.CardNumber, card.OpenDate);
            }

            return cardServiceFeeGrafik;
        }

        /// <summary>
        /// Ստանում է քարտի աշխատանքային ծրագիրը
        /// </summary>
        /// <param name="tariffContractID">քարտի աշխատանքային ծրագրի տեսակը</param>
        /// <returns></returns>
        public CardTariffContract GetCardTariffContract(long tariffID)
        {
            CardTariffContract contract = new CardTariffContract();
            contract.TariffID = tariffID;
            contract.Get();
            Localization.SetCulture(contract, Culture);
            return contract;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդին մատուցվող ծառայությունների գանձման հայտի կրեդիտ հաշիվը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Account GetFeeForServiceProvidedOrderCreditAccount(FeeForServiceProvidedOrder order)
        {
            return order.GetCreditAccount();
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտի սակագները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public CardTariff GetCardTariff(ulong productId)
        {
            CardTariff cardTariff = Card.GetCardTariff(productId, this.CustomerNumber);
            return cardTariff;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտի կարգավիճակը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public CardStatus GetCardStatus(ulong productId)
        {
            CardStatus cardStatus = Card.GetCardStatus(productId, this.CustomerNumber);
            return cardStatus;
        }

        public double GetLoanProductActivationFee(ulong productId, short withTax)
        {
            return LoanProductActivationOrder.GetLoanProductActivationFee(productId, withTax);
        }

        /// <summary>
        /// Վերադարձնում է փոխարկման վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CurrencyExchangeOrder GetCurrencyExchangeOrder(long id)
        {
            CurrencyExchangeOrder paymentOrder = new CurrencyExchangeOrder();
            paymentOrder.Id = id;
            paymentOrder.CustomerNumber = this.CustomerNumber;
            paymentOrder.Get();
            Localization.SetCulture(paymentOrder, Culture);
            return paymentOrder;
        }





        /// <summary>
        /// Չեկային գրքույքի վերաբերյալ զգուշացումներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public List<string> GetChequeBookReceiveOrderWarnings(ulong customerNumber, string accountNumber)
        {
            return ChequeBookReceiveOrder.GetChequeBookReceiveOrderWarnings(customerNumber, accountNumber, Culture);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի Լիազորագրերը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Credential> GetCustomerCredentialsList(ProductQualityFilter filter)
        {
            List<Credential> credentials = Credential.GetCustomerCredentialsList(this.CustomerNumber, filter);
            return credentials;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի Հաշիվները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Account> GetAccountsForCredential(int operationType)
        {
            List<Account> accounts = AssigneeOperation.GetAccountsForCredential(this.CustomerNumber, operationType);
            return accounts;
        }

        /// <summary>
        /// Լիազորագրի հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCredentialOrder(CredentialOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SaveCredentialOrder(CredentialOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Save(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult ApproveCredentialOrder(CredentialOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Approve(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }



        /// <summary>
        /// Լիազորագրի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CredentialOrder GetCredentialOrder(long ID)
        {
            CredentialOrder order = new CredentialOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է բոլոր լիազորագրի գործողությունները
        /// </summary>
        /// <returns></returns>
        public List<AssigneeOperation> GetAllOperations()
        {
            int CustomerType = GetCustomerType();

            return CredentialOrder.GetAllOperations(CustomerType);
        }

        /// <summary>
        /// Լիազորագրի հայտի փակում/հեռացում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCredentialTerminationOrder(CredentialTerminationOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Վերադարձնում է վարկային գծի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CreditLineTerminationOrder GetCreditLineTerminationOrder(long ID)
        {
            CreditLineTerminationOrder order = new CreditLineTerminationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Վերադարձնում է ավանդի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public DepositTerminationOrder GetDepositTerminationOrder(long ID)
        {
            DepositTerminationOrder order = new DepositTerminationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture((Order)order, Culture);
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Դադարեցված վարկային գծի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public CreditLine GetClosedCreditLine(ulong productId)
        {
            CreditLine creditline = CreditLine.GetClosedCreditLine(productId, this.CustomerNumber);
            Localization.SetCulture(creditline, this.Culture);
            return creditline;
        }


        public List<string> GetLoanActivationWarnings(long productId, short productType)
        {
            return LoanProductActivationOrder.GetLoanActivationWarnings(productId, this.CustomerNumber, productType);
        }

        public List<Provision> GetProductProvisions(ulong productId)
        {
            return Provision.GetProductProvisions(productId, this.CustomerNumber);
        }

        public List<LoanRepaymentGrafik> GetDecreaseLoanGrafik(CreditLine creditLine)
        {
            List<LoanRepaymentGrafik> loanGrafik = null;
            if (creditLine != null)
            {
                loanGrafik = creditLine.GetDecreaseLoanGrafik();
            }

            return loanGrafik;
        }

        /// <summary>
        /// Տարանցիկ հաշվին մուտք փոխարկումով
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveTransitCurrencyExchangeOrder(TransitCurrencyExchangeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                order.user = User;
                result = order.SaveAndApprove(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Վերադարձնում է քարտի Վերաթողարկման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardReReleaseOrder GetCardReReleaseOrder(long ID)
        {
            CardReReleaseOrder order = new CardReReleaseOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Քարտի վերաթողարկման հայտի հաստատում
        /// </summary>
        /// <param name="order">Քարտի վերաթողարկման հայտ</param>
        /// <param name="userName">Հաճախորդի մուտքային անուն</param>
        /// <param name="schemaType">Հաստատման սխեմա</param>
        /// <returns></returns>
        public ActionResult ApproveCardReReleaseOrder(CardReReleaseOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Վարկային գծի դադարեցման հայտի հաստատում
        /// </summary>
        /// <param name="order">Վարկային գծի դադարեցման հայտ</param>
        /// <param name="userName">Հաճախորդի մուտքային անուն</param>
        /// <param name="schemaType">Հաստատման սխեմա</param>
        /// <returns></returns>
        public ActionResult ApproveCreditLineTerminationOrder(CreditLineTerminationOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Կարարում է միջազգային փոխանցման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveInternationalPaymentOrder(InternationalPaymentOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            //Գործարքների օրեկան սահմանաչափի ստուգում
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                //schemaType = 2;  //Diana's set for World Vision
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        public List<AccountClosingHistory> GetAccountClosinghistory()
        {
            return Account.GetAccountClosinghistory(this.CustomerNumber);
        }


        public List<GoodsDetails> GetGoodsDetails(ulong productId)
        {
            return Loan.GetGoodsDetails(productId, this.CustomerNumber);
        }


        /// <summary>
        /// Վերադարձնում է հաշվի շարժը
        /// </summary>
        /// <param name="accountNumber">հաշվեհամար</param>
        /// <param name="startDate">սկիզբ</param>
        /// <param name="endDate">վերջ</param>
        /// <returns></returns>
        public AccountFlowDetails GetAccountFlowDetails(string accountNumber, DateTime startDate, DateTime endDate)
        {
            AccountFlowDetails accountFlowDetails = new AccountFlowDetails();
            Account account = Account.GetAccount(accountNumber, this.CustomerNumber);
            if (account != null)
            {
                accountFlowDetails = account.GetAccountFlowDetails(startDate, endDate);
            }


            return accountFlowDetails;

        }

        /// <summary>
        ///  Վերադարձնում է սպասարկման վարձի գանձման նշումները
        /// </summary>
        /// <returns></returns>
        public List<ServicePaymentNote> GetServicePaymentNoteList()
        {

            return ServicePaymentNote.GetServicePaymentNoteList(this.CustomerNumber);
        }



        /// <summary>
        /// Սպասարկամ վարձի գանձման նշման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveServicePaymentNoteOrder(ServicePaymentNoteOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        ///  Վերադարձնում է սպասարկամ վարձի գանձման նշման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ServicePaymentNoteOrder GetServicePaymentNoteOrder(long ID)
        {
            ServicePaymentNoteOrder order = new ServicePaymentNoteOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetServicePaymentNoteOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        ///  Վերադարձնում է սպասարկամ վարձի գանձման նշման հեռացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ServicePaymentNoteOrder GetDelatedServicePaymentNoteOrder(long ID)
        {
            ServicePaymentNoteOrder order = new ServicePaymentNoteOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetDelatedServicePaymentNoteOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }




        /// <summary>
        /// Կենսաթոշակի ստացման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePensionApplicationOrder(PensionApplicationOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public List<PensionApplication> GetPensionApplicationHistory(ProductQualityFilter filter)
        {
            List<PensionApplication> list = PensionApplication.GetPensionApplicationHistory(this.CustomerNumber, filter);
            Localization.SetCulture(list, Culture);
            return list;
        }



        /// <summary>
        /// Կենսաթոշակի ստացման դադարեցում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePensionApplicationTerminationOrder(PensionApplicationTerminationOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApproveTerminationOrder(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է կենսաթոշակի դադարեցման դիմումի տվյալները 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PensionApplicationTerminationOrder GetPensionApplicationTerminationOrder(long id)
        {
            PensionApplicationTerminationOrder pensionApplicationTerminationOrder = new PensionApplicationTerminationOrder();
            pensionApplicationTerminationOrder.Id = id;
            pensionApplicationTerminationOrder.CustomerNumber = this.CustomerNumber;
            pensionApplicationTerminationOrder.Get();
            Localization.SetCulture(pensionApplicationTerminationOrder, Culture);
            return pensionApplicationTerminationOrder;
        }

        /// <summary>
        /// Վերադարձնում է կենսաթոշակի դիմումի տվյալները 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PensionApplicationOrder GetPensionApplicationOrder(long id)
        {
            PensionApplicationOrder pensionApplicationOrder = new PensionApplicationOrder();
            pensionApplicationOrder.Id = id;
            pensionApplicationOrder.CustomerNumber = this.CustomerNumber;
            pensionApplicationOrder.Get();
            Localization.SetCulture(pensionApplicationOrder, Culture);
            return pensionApplicationOrder;
        }

        /// <summary>
        /// Վերադարձնում է փակված ավանդների բաց հաշիվները
        /// </summary>
        /// <returns></returns>
        public List<Account> GetClosedDepositAccountList(DepositOrder order)
        {
            List<Account> accounts = Account.GetClosedDepositAccountList(order);


            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }
        public ActionResult SaveAndApproveTransferCallContractOrder(TransferCallContractOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public TransferCallContractOrder GetTransferCallContractOrder(long id)
        {
            TransferCallContractOrder transferCallContractOrder = new TransferCallContractOrder();
            transferCallContractOrder.Id = id;
            transferCallContractOrder.CustomerNumber = this.CustomerNumber;
            transferCallContractOrder.Get();
            Localization.SetCulture(transferCallContractOrder, Culture);
            return transferCallContractOrder;
        }

        public List<TransferCallContractDetails> GetTransferCallContractsDetails()
        {
            List<TransferCallContractDetails> contracts = TransferCallContractDetails.GetTransferCallContractsDetails(this.CustomerNumber);
            Localization.SetCulture(contracts, Culture);
            return contracts;
        }

        public TransferCallContractDetails GetTransferCallContractDetails(long contractId)
        {
            TransferCallContractDetails contract = new TransferCallContractDetails();
            Localization.SetCulture(contract);
            return TransferCallContractDetails.GetTransferCallContractDetails(contractId, this.CustomerNumber);
        }


        public ActionResult SaveAndApproveTransferCallContractTerminationOrder(TransferCallContractTerminationOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public TransferCallContractTerminationOrder GetTransferCallContractTerminationOrder(long id)
        {
            TransferCallContractTerminationOrder terminationOrder = new TransferCallContractTerminationOrder();
            terminationOrder.Id = id;
            terminationOrder.CustomerNumber = this.CustomerNumber;
            terminationOrder.Get();
            Localization.SetCulture(terminationOrder, Culture);
            return terminationOrder;
        }



        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveReestrTransferOrder(ReestrTransferOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                order.user = User;
                result = order.SaveAndApprove(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }


        /// <summary>
        /// Վերադարձնում է ռեեստրով վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReestrTransferOrder GetReestrTransferOrder(long id)
        {
            ReestrTransferOrder paymentOrder = new ReestrTransferOrder();
            paymentOrder.Id = id;
            paymentOrder.CustomerNumber = this.CustomerNumber;
            paymentOrder.Get();
            Localization.SetCulture(paymentOrder, Culture);
            return paymentOrder;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ ունիկալ համարով հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Order GetOrder(long ID)
        {
            Order order = Order.GetOrder(ID);
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է մերժված հաղորդագրությունները
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<RejectedOrderMessage> GetRejectedMessages(User user, int filter, int start, int end)
        {
            List<RejectedOrderMessage> userRejectedMessages = RejectedOrderMessage.GetRejectedMessages(user.userID, filter, start, end);
            Localization.SetCulture(userRejectedMessages, Culture);
            return userRejectedMessages;
        }

        /// <summary>
        /// Պահատուփի մուտքագրում,ակտիվացում,դադարեցում,հեռացում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveDepositCaseOrder(DepositCaseOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public DepositCaseOrder GetDepositCaseOrder(long id)
        {
            DepositCaseOrder order = new DepositCaseOrder();
            order.Id = id;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public List<CardTariffContract> GetCustomerCardTariffContracts(ulong customerNumber, ProductQualityFilter filter)
        {
            List<CardTariffContract> customerCardTariffContracts = CardTariffContract.GetCustomerCardTariffContracts(customerNumber, filter);
            Localization.SetCulture(customerCardTariffContracts, Culture);
            return customerCardTariffContracts;
        }
        public bool HasCardTariffContract(ulong customerNumber)
        {
            return CardTariffContract.HasCardTariffContract(customerNumber);
        }


        public bool HasPosTerminal(ulong customerNumber)
        {
            return PosTerminal.HasPosTerminal(customerNumber);
        }
        public List<PosLocation> GetCustomerPosLocations(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PosLocation> customerPosLocations = PosLocation.GetCustomerPosLocations(customerNumber, filter);
            Localization.SetCulture(customerPosLocations, Culture);
            return customerPosLocations;
        }
        public PosLocation GetPosLocation(int posLocationId)
        {
            PosLocation posLocation = new PosLocation();
            posLocation.Id = posLocationId;
            posLocation.Get();
            Localization.SetCulture(posLocation, Culture);
            return posLocation;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի գրանցման ենթակա քարտերի ցուցակը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<PlasticCard> GetCardsForRegistration(ushort filialCode) => PlasticCard.GetCardsForRegistration(this.CustomerNumber, filialCode);


        /// <summary>
        /// Վերադարձնում է որոնման տվյալների համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public List<Order> GetApproveReqOrder(DateTime dateFrom, DateTime dateTo)
        {
            List<Order> orders = Order.GetApproveReqOrder(this.CustomerNumber, dateFrom, dateTo);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }


        /// <summary>
        /// Պահատուփի տուժանքի մարման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է պահատուփի տուժանքի մարման հայտի տվյալները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DepositCasePenaltyMatureOrder GetDepositCasePenaltyMatureOrder(long id)
        {
            DepositCasePenaltyMatureOrder order = new DepositCasePenaltyMatureOrder();
            order.Id = id;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }





        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի վճարման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            ActionResult result = order.SavePeriodicSwiftStatementOrder(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;

        }
        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի վճարման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApprovePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndAprovePeriodicSwiftStatementOrder(PeriodicSwiftStatementOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            order.CustomerNumber = CustomerNumber;

            if (this.User.userCustomerNumber == order.CustomerNumber)
            {
                //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                result.Errors.Add(new ActionError(544));
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Պարբերական SWIFT-ով ուղարկվող քաղվածքի հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PeriodicSwiftStatementOrder GetPeriodicSwiftStatementOrder(long ID)
        {
            PeriodicSwiftStatementOrder order = new PeriodicSwiftStatementOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Քարտի մուտքագրման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardRegistrationOrder(CardRegistrationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            if (order.Source == SourceType.Bank && result.ResultCode == ResultCode.Normal)
            {
                List<AccountFreezeDetails> freezeDetails = new List<AccountFreezeDetails>();
                ushort freezeReason = 0;
                if (order.Card.SupplementaryType == SupplementaryType.Linked || order.Card.SupplementaryType == SupplementaryType.Attached)
                {
                    freezeDetails = AccountFreezeDetails.GetAccountFreezeHistory(order.CardAccount.AccountNumber, 1, 0).Where(m => AutomateCardBlockingUnblocking.FreezingReasonsForBlocking.Contains(m.ReasonId)).OrderBy(m => m.RegistrationDate).ToList();
                    freezeReason = (ushort)(freezeDetails?.Any() == true ? freezeDetails.FirstOrDefault().ReasonId : 0);
                }
                bool hasBankrupt = CustomerDB.HasBankrupt(CustomerNumber);
                if (hasBankrupt || freezeDetails?.Any() == true)
                {
                    if (ArcaCardsTransactionOrder.GetPreviousBlockingOrderId(order.Card.CardNumber) == null)
                    {
                        ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(order.Card.CardNumber, ArcaCardsTransationActionType.Block, hasBankrupt ? ArcaCardsTransationActionReason.Bankrupt : AutomateCardBlockingUnblocking.GetCardTransactionReasonByFreezeReasonId(freezeReason), order.OperationDate, order.RegistrationDate);
                        SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, userName, schemaType);
                    }
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի գրանցման համար հասանելի քարտային հաշիվներ
        /// </summary>
        /// <param name="cardCurrency">քարտի արժույթ</param>
        /// <param name="cardFililal">քարտի մասնաճյուղ</param>
        /// <returns></returns>
        public List<Account> GetAccountListForCardRegistration(string cardCurrency, int cardFililal)
        {
            List<Account> accounts = Account.GetAccountListForCardRegistration(this.CustomerNumber, cardCurrency, cardFililal);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }

        /// <summary>
        /// /// Քարտի գրանցման համար հասանելի գերածախսի հաշիվներ
        /// </summary>
        /// <param name="cardCurrency">քարտի արժույթ</param>
        /// <param name="cardFililal">քարտի մասնաճյուղ</param>
        /// <returns></returns>
        public List<Account> GetOverdraftAccountsForCardRegistration(string cardCurrency, int cardFililal)
        {
            List<Account> accounts = Account.GetOverdraftAccountsForCardRegistration(this.CustomerNumber, cardCurrency, cardFililal);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }

        /// <summary>
        /// Վերադարձնում է քարտի մուտքագրման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardRegistrationOrder GetCardRegistrationOrder(long ID)
        {
            CardRegistrationOrder order = new CardRegistrationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Փոխանցում խանութի հաշվին հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveTransferToShopOrder(TransferToShopOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }



        /// <summary>
        /// Վերադարձնում է Փոխանցում խանութի հաշվին հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public TransferToShopOrder GetTransferToShopOrder(long ID)
        {
            TransferToShopOrder order = new TransferToShopOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public List<string> GetCardRegistrationWarnings(PlasticCard plasticCard)
        {
            return CardRegistrationOrder.GetCardRegistrationWarnings(plasticCard, Culture);
        }
        public List<Provision> GetCustomerProvisions(string currency, ushort type, ushort quality)
        {
            return Provision.GetCustomerProvisions(this.CustomerNumber, currency, type, quality);
        }
        public List<ProvisionLoan> GetProvisionLoans(ulong provisionId)
        {
            List<ProvisionLoan> provisionLoans = Provision.GetProvisionLoans(provisionId);
            Localization.SetCulture(provisionLoans, Culture);
            return provisionLoans;
        }



        /// <summary>
        /// Վերադարձնում է հաճախորդի ապահովագրությունների ցուցակը
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Insurance> GetInsurances(ProductQualityFilter filter)
        {
            List<Insurance> insurances = Insurance.GetInsurances(this.CustomerNumber, filter);
            Localization.SetCulture(insurances, Culture);
            return insurances;
        }

        /// <summary>
        /// Վերադարձնում է վճարված ապահովագրություն վարկի ապահովագրությունը
        /// </summary>
        /// <param name="productId">Վարկի productId</param>
        /// <returns></returns>
        public List<Insurance> GetPaidInsurance(ulong loanProductId)
        {
            List<Insurance> insurances = Insurance.GetPaidInsurance(this.CustomerNumber, loanProductId);
            Localization.SetCulture(insurances, Culture);
            return insurances;
        }



        /// <summary>
        /// Վերադարձնում է մեկ ապահովագրության տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public Insurance GetInsurance(ulong productId)
        {
            Insurance insurance = Insurance.GetInsurance(productId, this.CustomerNumber);
            Localization.SetCulture(insurance, Culture);
            return insurance;
        }


        /// <summary>
        /// Ապահովագրության հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveInsuranceOrder(InsuranceOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է Ապահովագրության հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public InsuranceOrder GetInsuranceOrder(long ID)
        {
            InsuranceOrder order = new InsuranceOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            Localization.SetCulture(order.Insurance, Culture);
            return order;
        }


        /// <summary>
        /// Քարտի սպասարկման վարձի տվյալների փոփոխման հայտի պհպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardServiceFeeDataChangeOrder(CardDataChangeOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է Քարտի սպասարկման վարձի տվյալների փոփոխման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardDataChangeOrder GetCardDataChangeOrder(long ID)
        {
            CardDataChangeOrder order = new CardDataChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Քարտի սպասարկման վարձի գրաֆիկի տվյալների փոփոխման հայտի պհպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardServiceFeeGrafikDataChangeOrder(CardServiceFeeGrafikDataChangeOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի սպասարկման վարձի գրաֆիկի տվյալների փոփոխման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardServiceFeeGrafikDataChangeOrder GetCardServiceFeeGrafikDataChangeOrder(long ID)
        {
            CardServiceFeeGrafikDataChangeOrder order = new CardServiceFeeGrafikDataChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Քարտի սպասարկման վարձի նոր գրաֆիկի մուտքագրում
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<CardServiceFeeGrafik> SetNewCardServiceFeeGrafik(ulong productId)
        {
            return Card.SetNewCardServiceFeeGrafik(productId, this.CustomerNumber);
        }

        /// <summary>







        /// <summary>
        /// Վերադարձնում է հաճախորդի հասանելիության խմբերը
        /// </summary>
        /// <returns></returns>
        public List<XBUserGroup> GetXBUserGroups()
        {
            return XBUserGroup.GetXBUserGroups(this.CustomerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հասանելիության խմբում առկա օգտագործողներին
        /// </summary>
        /// <returns></returns>
        public List<HBUser> GetHBUsersByGroup(int id)
        {
            return XBUserGroup.GetHBUsersByGroup(id);
        }




        /// <summary>
        ///  Վերադարձնում է հաճախորդի հաստատման սխեման
        /// </summary>
        /// <returns></returns>
        public ApprovementSchema GetApprovementSchema()
        {
            return ApprovementSchema.Get(this.CustomerNumber);
        }

        /// <summary>
        ///  Վերադարձնում է հաճախորդի հաստատման սխեմայի մանրամասները
        /// </summary>
        /// <returns></returns>
        public List<ApprovementSchemaDetails> GetApprovementSchemaDetails(int schemaId)
        {
            return ApprovementSchemaDetails.Get(schemaId);
        }

        /// <summary>

        /// Վերադարձնում է նվազեցումը
        /// </summary>
        /// <param name="productId">երաշխիքի ունիկալ համար</param>
        /// <returns></returns>
        public List<GivenGuaranteeReduction> GetGivenGuaranteeReductions(ulong productId)
        {
            return Guarantee.GetGivenGuaranteeReductions(productId);
        }


        /// <summary>
        /// Ռեեստրով կոմունալ վճարման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveReestrUtilityPaymentOrder(ReestrUtilityPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման հայտը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReestrUtilityPaymentOrder GetReestrUtilityPaymentOrder(long id)
        {
            ReestrUtilityPaymentOrder utilityPaymentOrder = new ReestrUtilityPaymentOrder();
            utilityPaymentOrder.Id = id;
            utilityPaymentOrder.CustomerNumber = this.CustomerNumber;
            utilityPaymentOrder.GetReestrUtilityPaymentOrder();
            Localization.SetCulture(utilityPaymentOrder, Culture);
            return utilityPaymentOrder;
        }

        /// <summary>
        /// Վերադարձնում է քարտի սպասարկման վարձի գրաֆիկը
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<CreditLineGrafik> GetCreditLineGrafik(ulong productId)
        {
            List<CreditLineGrafik> creditLineGrafik = CreditLine.GetCreditLineGrafik(productId);
            return creditLineGrafik;
        }

        /// <summary>
        /// Հաշվի լրացուցիչ տվյալների հեռացման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAccountAdditionalDataRemovableOrder(AccountAdditionalDataRemovableOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է քարտի վրա եղած արգելանքի գումարը և հաշիվը
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public DAHKDetail GetCardDAHKDetails(string cardNumber)
        {
            DAHKDetail DAHK_Detail = null;
            DAHK_Detail = Card.GetCardDAHKDetails(cardNumber);
            return DAHK_Detail;
        }

        /// <summary>
        /// Վերադարձնում է մեկ պլաստիկ քարտի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="filialCode"></param>
        /// <param name="productIdType"></param>
        /// <returns></returns>
        public PlasticCard GetPlasticCard(ulong productId, int filialCode, bool productIdType)
        {
            PlasticCard card = PlasticCard.GetPlasticCard(productId, productIdType);

            if (card != null && card.FilialCode == 0)
            {
                return card;
            }

            if (card != null && card.FilialCode != filialCode - 22000)
                card = null;
            return card;
        }


        public List<LoanProductClassification> GetLoanProductClassifications(ulong productId, DateTime dateFrom)
        {
            return LoanProductClassification.GetLoanProductClassifications(productId, dateFrom);
        }


        /// <summary>
        /// Վերդարձնում է հաճախորդի Ի պահ ընդունված արժեքները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<SafekeepingItem> GetSafekeepingItems(ProductQualityFilter filter) => SafekeepingItem.GetSafekeepingItems(this.CustomerNumber, filter);

        public SafekeepingItem GetSafekeepingItem(ulong productId) => SafekeepingItem.GetSafekeepingItem(this.CustomerNumber, productId);

        /// <summary>
        /// Քարտի կարգավիճակի փոփոխման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardStatusChangeOrder(CardStatusChangeOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի կարգավիճակի փոփոխման հայտի մանրամասները
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public CardStatusChangeOrder GetCardStatusChangeOrder(long orderId)
        {
            CardStatusChangeOrder order = new CardStatusChangeOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveFactoringTerminationOrder(FactoringTerminationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ֆակտորինգի դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public FactoringTerminationOrder GetFactoringTerminationOrder(long ID)
        {
            FactoringTerminationOrder order = new FactoringTerminationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Ֆակտորինգի դադարեցման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveFactoringTerminationOrder(FactoringTerminationOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ֆակտորինգի դադարեցման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveFactoringTerminationOrder(FactoringTerminationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        public ActionResult SaveLoanProductTerminationOrder(LoanProductTerminationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Երաշխիքի դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LoanProductTerminationOrder GetLoanProductTerminationOrder(long ID)
        {
            LoanProductTerminationOrder order = new LoanProductTerminationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Երաշխիքի դադարեցման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveLoanProductTerminationOrder(LoanProductTerminationOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ֆակտորինգի դադարեցման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveLoanProductTerminationOrder(LoanProductTerminationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }





        /// <summary>
        /// Վերադարձնում է հաճախորդի գործող կոմունալի քարտերը
        /// </summary>
        /// <returns></returns>
        public List<CustomerCommunalCard> GetCustomerCommunalCards()
        {
            List<CustomerCommunalCard> list = CustomerCommunalCard.GetCustomerCommunalCards(this.CustomerNumber);
            return list;
        }

        /// <summary>
        /// Կոմունալի քարտի պահպանում
        /// </summary>
        /// <param name="customerCommunalCard"></param>
        /// <returns></returns>
        public ActionResult SaveCustomerCommunalCard(CustomerCommunalCard customerCommunalCard)
        {
            customerCommunalCard.CustomerNumber = this.CustomerNumber;
            customerCommunalCard.OpenDate = DateTime.Now.Date;
            customerCommunalCard.OpenerSetNumber = (ushort)User.userID;
            customerCommunalCard.OpenerFilialCode = User.filialCode;
            customerCommunalCard.Quality = 1;
            ActionResult result = customerCommunalCard.SaveCustomerCommunalCard();
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Կոմունալի քարտի կարգավիճակի փոփոխում
        /// </summary>
        /// <param name="customerCommunalCard"></param>
        /// <returns></returns>
        public ActionResult ChangeCustomerCommunalCardQuality(CustomerCommunalCard customerCommunalCard)
        {
            customerCommunalCard.Quality = 0;
            customerCommunalCard.CustomerNumber = this.CustomerNumber;
            customerCommunalCard.EditingDate = DateTime.Now.Date;
            customerCommunalCard.EditorSetNumber = (ushort)User.userID;
            customerCommunalCard.Id = CustomerCommunalCard.GetCustomerCommunalCards(this.CustomerNumber).Find(m => m.AbonentNumber == customerCommunalCard.AbonentNumber && m.BranchCode == customerCommunalCard.BranchCode).Id;
            ActionResult result = customerCommunalCard.ChangeCustomerCommunalCardQuality();
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Ավանդային տվյալների փոփոխման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveDepositDataChangeOrder(DepositDataChangeOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if ((order.OrderNumber == "" || order.OrderNumber == null) && order.Id == 0)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդային տվյալների փոփոխման հայտ տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public DepositDataChangeOrder GetDepositDataChangeOrder(long ID)
        {
            DepositDataChangeOrder order = new DepositDataChangeOrder();
            order.Id = ID;
            order.CustomerNumber = CustomerNumber;
            order.Get();
            return order;
        }
        /// <summary>
        /// Ավանդային տվյալների փոփոխման հայտ հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApproveDepositDataChangeOrder(DepositDataChangeOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Ավանդային տվյալների փոփոխման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveDepositDataChangeOrder(DepositDataChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();


            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, schemaType);
            Localization.SetCulture(result, Culture);

            return result;
        }

        public List<DepositAction> GetDepositActions(DepositOrder order)
        {
            List<DepositAction> actions = DepositOrder.GetDepositActions(order);
            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public HBApplication GetHBApplication()
        {
            HBApplication hbApplication = HBApplicationDB.GetHBApplication(this.CustomerNumber);
            if (hbApplication != null)
            {
                var customerType = ACBAOperationService.GetCustomerType(this.CustomerNumber);
                if (customerType != 6)
                {
                    ulong linkedCustomerNumber = ACBAOperationService.GetLinkedCustomerNumber(this.CustomerNumber);
                    if (linkedCustomerNumber != 0)
                    {
                        hbApplication.FullName = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(linkedCustomerNumber));
                        if (customerType == 2)
                        {
                            hbApplication.Manager = "";
                        }
                        else
                        {
                            hbApplication.Manager = hbApplication.FullName;

                        }
                    }
                    hbApplication.Position = "Տնօրեն";
                    hbApplication.Description = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(this.CustomerNumber));
                    LinkedCustomer chiefAcc = ACBAOperationService.GetCustomerLinkedPersonsList(this.CustomerNumber, 1).Find(m => m.linkType.key == 8);
                    if (chiefAcc != null)
                    {
                        hbApplication.ChiefAcc = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(chiefAcc.linkedCustomerNumber));

                    }
                }
            }
            return hbApplication;
        }

        public HBApplication GetHBApplicationShablon()
        {
            HBApplication hbApplication = new HBApplication();

            var customerType = ACBAOperationService.GetCustomerType(this.CustomerNumber);
            if (customerType != 6)
            {

                ulong linkedCustomerNumber = ACBAOperationService.GetLinkedCustomerNumber(this.CustomerNumber);
                if (linkedCustomerNumber != 0)
                {
                    hbApplication.FullName = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(linkedCustomerNumber));
                    if (customerType == 2)
                    {
                        hbApplication.Manager = "";
                    }
                    else
                    {
                        hbApplication.Manager = hbApplication.FullName;

                    }
                }
                hbApplication.Position = "Տնօրեն";
                hbApplication.Description = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(this.CustomerNumber));
                LinkedCustomer chiefAcc = ACBAOperationService.GetCustomerLinkedPersonsList(this.CustomerNumber, 1).Find(m => m.linkType.key == 8);
                if (chiefAcc != null)
                {
                    hbApplication.ChiefAcc = Utility.ConvertAnsiToUnicode(ACBAOperationService.GetCustomerDescription(chiefAcc.linkedCustomerNumber));

                }
            }
            return hbApplication;
        }

        /// <summary>
        /// Վերադարձնում է հեռահար բանկինգի պայմանագրի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public HBApplicationOrder GetHBApplicationOrder(long ID)
        {
            HBApplicationOrder order = new HBApplicationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetHBApplicationOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }
        public ActionResult SaveAndApproveHBApplicationOrder(HBApplicationOrder order, string userName, short schemaType)
        {
            if ((!bool.Parse(ConfigurationManager.AppSettings["EnabledOldMobileCompatibility"])) && order.HBApplicationUpdate.AddedItems.OfType<HBToken>().Where(x => x.TokenType != HBTokenTypes.Token)?.Any() == true)
            {
                List<string> tempTokens = order.GetTempTokenList(order.HBApplicationUpdate.AddedItems.OfType<HBToken>().Where(x => x.TokenType != HBTokenTypes.Token).Count());
                for (int i = 0; i < tempTokens.Count; i++)
                {
                    order.HBApplicationUpdate.AddedItems.OfType<HBToken>().Where(x => x.TokenType != HBTokenTypes.Token).ElementAt(i).TokenNumber = tempTokens[i];
                }
            }
            order.CustomerNumber = CustomerNumber;
            ActionResult result = new ActionResult()
            {
                ResultCode = ResultCode.Normal,
                Errors = new List<ActionError>()
            };

            if (order.Source != SourceType.MobileBanking && order.Source != SourceType.AcbaOnline)
            {
                result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            }


            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է պայմանագրի տվյալ կարգավիճակով ՀԲ օգտագործողներին
        /// </summary>
        /// <param name="hbAppID"></param>
        /// <returns></returns>
        public List<HBUser> GetHBUsers(int hbAppID, ProductQualityFilter filter) => HBUser.GetHBUsers(hbAppID, filter);

        /// <summary>
        /// Վերադարձնում է պայմանագրի տվյալ կարգավիճակով ՀԲ օգտագործողին
        /// </summary>
        /// <param name="hbUserID"></param>
        /// <returns></returns>
        public HBUser GetHBUser(int hbUserID)
        {
            HBUser hbUser = HBUser.GetHBUser(hbUserID);
            return hbUser;
        }
        /// <summary>
        /// Վերադարձնում է պայմանագրի տվյալ կարգավիճակով ՀԲ օգտագործողին
        /// </summary>
        /// <param name="hbUserName"></param>
        /// <returns></returns>
        public HBUser GetHBUserByUserName(string hbUserName)
        {
            HBUser hbUser = HBUser.GetHBUserByUserName(hbUserName);
            return hbUser;
        }

        /// <summary>
        /// Վերադարձնում է ՀԲ օգտագործողի տվյալ կարգավիճակով բոլոր թոկենները
        /// </summary>
        /// <param name="HBUserID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<HBToken> GetHBTokens(int HBUserID, ProductQualityFilter filter) => HBToken.GetHBTokens(HBUserID, filter);

        /// <summary>
        /// Վերադարձնում է նշված օգտագործողի ֆիլտրված թոքեները
        /// </summary>
        /// <param name="HBUserID"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<HBToken> GetHBTokens(int HBUserID, HBTokenQuality filter)
        {
            return HBToken.GetHBTokens(HBUserID, filter);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի հեռախոսային բանկինգի պայմանագիրը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public PhoneBankingContract GetPhoneBankingContract()
        {
            PhoneBankingContract phoneBankingContract = PhoneBankingContract.Get(this.CustomerNumber);
            return phoneBankingContract;
        }

        ///// <summary>
        ///// Տոկենի ապաբլոկավորման  հայտի պահպանում և ուղղարկում
        ///// </summary>
        ///// <param name="order"></param>
        ///// <param name="userName"></param>
        ///// <param name="schemaType"></param>
        ///// <returns></returns>
        //public ActionResult SaveAndApproveHBTokenUnBlockOrder(HBTokenUnBlockOrder order, string userName, short schemaType)
        //{
        //    order.CustomerNumber = CustomerNumber;
        //    ActionResult result = new ActionResult();
        //    result = order.SaveAndApprove(userName, Source, User, schemaType);
        //    //Localization.SetCulture(result, Culture);
        //    return result;
        //}

        /// <summary>
        /// Հեռախոսային Բանկինգի պայմանագրի մուտքագրման հայտի պահպանում և կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePhoneBankingContractOrder(PhoneBankingContractOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Հեռահար բանկինգի դադարեցում կամ ակտիվացում 119,120
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveHBApplicationQualityChangeOrder(HBApplicationQualityChangeOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի MR ծրագրի գրանցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardMembershipRewardsOrder(MembershipRewardsOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public MembershipRewardsOrder GetCardMembershipRewardsOrder(long ID)
        {
            MembershipRewardsOrder order = new MembershipRewardsOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }



        /// <summary>
        /// Հեռախոսային Բանկինգի պայմանագրի մուտքագրման հայտի պահպանում և կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePhoneBankingContractClosingOrder(PhoneBankingContractClosingOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հեռախոսային բանկինգի պայմանագրի դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PhoneBankingContractClosingOrder GetPhoneBankingContractClosingOrder(long ID)
        {
            PhoneBankingContractClosingOrder order = new PhoneBankingContractClosingOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetPhoneBankingContractClosingOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է հեռախոսային բանկինգի պայմանագրի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PhoneBankingContractOrder GetPhoneBankingContractOrder(long ID)
        {
            PhoneBankingContractOrder order = new PhoneBankingContractOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetPhoneBankingContractOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Հեռահար բանկինգի հարցւոմների պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <param name="ClientIp"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveHBServletRequestOrder(HBServletOrder order, string userName, short schemaType, string ClientIp, byte Language = 1)
        {
            order.ClientIP = ClientIp;
            order.CustomerNumber = CustomerNumber;
            order.Language = Language;
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType, ClientIp);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public List<AssigneeCustomer> GetHBAssigneeCustomers(ulong customerNumber)
        {
            return AssigneeCustomer.GetHBAssigneeCustomers(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հեռահար բանկինգի դադարեցման(վերականգման)   հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public HBApplicationQualityChangeOrder GetHBApplicationQualityChangeOrder(long ID)
        {
            HBApplicationQualityChangeOrder order = new HBApplicationQualityChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetHBApplicationQualityChangeOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է հեռահար բանկինգի  տոկենի բլոկավորման(ապաբլոկավորման)   հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public HBServletRequestOrder GetHBServletRequestOrder(long ID)
        {
            HBServletRequestOrder order = new HBServletRequestOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetHBServletRequestOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Դրամարկղի մատյանի հայտի մանրամասներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CashBookOrder GetCashBookOrder(long ID)
        {
            CashBookOrder order = new CashBookOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// Դրամարկղի մատյանի հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCashBookOrder(CashBookOrder order, ExternalBanking.ACBAServiceReference.User user, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(User, userName, schemaType, Source);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ վարկային դիմումի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public LoanApplication GetLoanApplication(ulong productId)
        {
            LoanApplication loanApplication = LoanApplication.GetLoanApplication(productId, this.CustomerNumber);
            Localization.SetCulture(loanApplication, this.Culture);
            return loanApplication;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի վարկային դիմումները
        /// </summary>
        /// <returns></returns>
        public List<LoanApplication> GetLoanApplications()
        {
            List<LoanApplication> loanApplications = LoanApplication.GetLoanApplications(this.CustomerNumber);
            Localization.SetCulture(loanApplications, this.Culture);
            return loanApplications;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի վարկային դիմումի FICO սքորի արդյունքները
        /// </summary>
        /// <returns></returns>
        public List<FicoScoreResult> GetLoanApplicationFicoScoreResults(ulong productId, DateTime requestDate)
        {
            List<FicoScoreResult> results = LoanApplication.GetLoanApplicationFicoScoreResults(this.CustomerNumber, productId, requestDate);
            return results;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի մեկ վարկային դիմումի տվյալները հայտի համարով
        /// </summary>
        /// <param name="docId"></param>
        /// <returns></returns>
        public LoanApplication GetLoanApplicationByDocId(long docId)
        {
            LoanApplication loanApplication = LoanApplication.GetLoanApplicationByDocId(docId, this.CustomerNumber);
            Localization.SetCulture(loanApplication, this.Culture);
            return loanApplication;
        }

        /// <summary>
        /// Արագ օվերդրաֆտի ստուգումներ
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public List<ActionError> FastOverdraftValidations(string cardNumber, SourceType source)
        {
            List<ActionError> errors = LoanProductOrder.FastOverdraftValidations(CustomerNumber, source, cardNumber);
            Localization.SetCulture(errors, Culture);
            return errors;
        }



        /// <summary>
        /// Հեռահար բանկինգի մոբայլ տոկենի գրանցման կոդի վերաուղարկման հայտի պահպանում
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult SaveHBRegistrationCodeResendOrder(HBRegistrationCodeResendOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Հեռահար բանկինգի ծառայությունների ակտիվացումից հրաժարման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveHBActivationRejectionOrder(HBActivationRejectionOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// ՀԲ օգտագործողի լոգ
        /// </summary>
        /// <param name="hbuser"></param>
        /// <returns></returns>
        public List<HBUserLog> GetHBUserLog(String userName)
        {
            List<HBUserLog> HBUserLog = HBUser.GetHBUserLog(userName);
            return HBUserLog;
        }

        /// <summary>
        /// Պահատուփի տուժանքի դադարեցման հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public DepositCaseStoppingPenaltyCalculationOrder GetDepositCaseStoppingPenaltyCalculationOrder(long ID)
        {
            DepositCaseStoppingPenaltyCalculationOrder order = new DepositCaseStoppingPenaltyCalculationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Պահատուփի տուժանքի դադարեցման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveDepositCaseStoppingPenaltyCalculationOrder(DepositCaseStoppingPenaltyCalculationOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }



        /// <summary>
        /// Հեռախոսային բանկինգի ծառայությունների հարցում
        /// </summary>
        /// <returns></returns>
        public PhoneBankingContractActivationRequest GetPhoneBankingRequests()
        {
            return PhoneBankingContractActivationOrder.GetPhoneBankingRequests(CustomerNumber);
        }

        public ActionResult SaveAndApprovePhoneBankingContractActivationOrder(PhoneBankingContractActivationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        public List<LoanMonitoringConclusion> GetLoanMonitoringConclusions(long productId)
        {

            return LoanMonitoringConclusion.GetLoanMonitoringConclusions(productId);
        }

        public LoanMonitoringConclusion GetLoanMonitoringConclusion(long monitoringId, long productId)
        {
            return LoanMonitoringConclusion.GetLoanMonitoringConclusion(monitoringId, productId);
        }

        public ActionResult SaveLoanMonitoringConclusion(LoanMonitoringConclusion monitroing) => monitroing.Save(User);

        public ActionResult ApproveLoanMonitoringConclusion(long monitoringId)
        {
            ActionResult result = LoanMonitoringConclusion.Approve(monitoringId);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult DeleteLoanMonitoringConclusion(long monitoringId) => LoanMonitoringConclusion.Delete(monitoringId);

        public float GetProvisionCoverCoefficient(long productId)
        {
            return LoanMonitoringConclusion.GetProvisionCoverCoefficient(productId);
        }

        public List<MonitoringConclusionLinkedLoan> GetLinkedLoans(long productId)
        {
            return LoanMonitoringConclusion.GetLinkedLoans(productId, this.CustomerNumber);
        }

        public short GetLoanMonitoringType()
        {
            return LoanMonitoringConclusion.GetLoanMonitoringType(User.DepartmentId);
        }

        /// <summary>
        /// Լիազորագրի ատիվացման հայտի մանրամասներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CredentialActivationOrder GetCredentialActivationOrder(long ID)
        {
            CredentialActivationOrder order = new CredentialActivationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Լիազորագրի ատիվացման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCredentialActivationOrder(CredentialActivationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Լիազորված անձի նույնականացման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveAssigneeIdentificationOrder(AssigneeIdentificationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }




        public ActionResult SaveAndApproveCreditHereAndNowActivationOrders(CreditHereAndNowActivationOrders creditHereAndNowActivationOrders, string userName)
        {
            ActionResult result = creditHereAndNowActivationOrders.SaveAndApprove(userName, User, Source, 1);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SaveAndApproveClassifiedLoanActionOrders(ClassifiedLoanActionOrders classifiedLoanActionOrders, string userName)
        {
            ActionResult result = classifiedLoanActionOrders.SaveAndApprove(userName, User, Source, 1);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SaveAndApproveLoanProductClassificationRemoveOrder(LoanProductClassificationRemoveOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SaveAndApproveLoanProductMakeOutOrder(LoanProductMakeOutOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public bool HasPropertyProvision(ulong productId)
        {
            return Provision.HasPropertyProvision(productId);
        }


        public DemandDepositRateChangeOrder GetDemandDepositRateChangeOrder(long ID)
        {
            DemandDepositRateChangeOrder order = new DemandDepositRateChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveAndApproveDemandDepositRateChangeOrder(DemandDepositRateChangeOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Վերադարձնում է լիազորված անձի նույնականացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public AssigneeIdentificationOrder GetAssigneeIdentificationOrder(long ID)
        {
            AssigneeIdentificationOrder order = new AssigneeIdentificationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public static bool HasCustomerBankruptBlockage(ulong customerNumber)
        {
            return ACBAOperationService.HasCustomerBankruptBlockage(customerNumber);
        }

        public ActionResult SaveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.Save(userName, Source, User);


            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult ApproveReceivedFastTransferPaymentOrder(ReceivedFastTransferPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.ApproveReceivedFastTransferOrder(userName, Source, User, schemaType, order);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }


        /// <summary>
        /// Վերադարձնում է ստացված արագ փոխանցման հայտի մերժման պատճառը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string GetReceivedFastTransferOrderRejectReason(int orderId)
        {
            ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder = new ReceivedFastTransferPaymentOrder();
            receivedFastTransferPaymentOrder.Id = orderId;
            receivedFastTransferPaymentOrder.CustomerNumber = this.CustomerNumber;

            return receivedFastTransferPaymentOrder.GetReceivedFastTransferOrderRejectReason(this.Culture.Language);
        }



        public int GetStatementFixedReceivingType(string cardnumber)
        {
            return Card.GetStatementFixedReceivingType(cardnumber);
        }

        public List<OrderHistory> GetOnlineOrderHistory(long orderId)
        {
            List<OrderHistory> orderHistory = Order.GetOnlineOrderHistory(orderId, this.Culture);
            Localization.SetCulture(orderHistory, this.Culture);
            return orderHistory;
        }

        /// <summary>
        /// Պարբերական փոխանցման(ՀՀ տարածքում սեփական հաշիվների միջև) վճարման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 
        public ActionResult ApprovePeriodicPaymentOrder(PeriodicPaymentOrder order, short schemaType, string userName)
        {
            ActionResult result = order.ApprovePeriodicPaymentOrder(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveProductNotificationConfigurationsOrder(ProductNotificationConfigurationsOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդին վաճառված մեկ պարտատոմսի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Bond GetBondByID(int ID)
        {
            Bond bond = Bond.GetBondByID(ID);
            return bond;
        }
        /// <summary>
        /// Վերադարձնում է հաճախորդին վաճառված բոլոր պարտատոմսերի տվյալները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Bond> GetBonds(BondFilter filter)
        {
            filter.CustomerNumber = this.CustomerNumber;
            List<Bond> bond = Bond.GetBonds(filter);
            return bond;
        }


        /// <summary>
        /// Պարտատոմսի վաճառքի հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveBondOrder(BondOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }



            if (Source != SourceType.PhoneBanking)
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
                {
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }


            }
            else
            {
                if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneTransactionLimitToOwnAccount))
                {
                    result = order.SaveAndApprove(userName, Source, User, schemaType);
                }
                else
                {
                    result.Errors.Add(new ActionError(66));
                    result.ResultCode = ResultCode.ValidationError;
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }


        public BondOrder GetBondOrder(long ID)
        {
            BondOrder order = new BondOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերդարձնում է պարտատոմսի արժեկտրոնների վճարման համար նախատեսված հաշիվները (դրամային ընթացիկ)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<Account> GetAccountsForCouponRepayment()
        {
            List<Account> transitAccount = Account.GetCustomerTransitAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
            List<Account> currentAccounts = Account.GetAccountsForCouponRepayment(this.CustomerNumber);


            List<Account> allAccounts = new List<Account>();
            allAccounts.AddRange(transitAccount);
            allAccounts.AddRange(currentAccounts);
            Localization.SetCulture(allAccounts, Culture);
            return allAccounts;
        }

        /// <summary>
        /// Վերդարձնում է պարտատոմսի գումարների մարման համար նախատեսված հաշիվները (պարտատոմսի արժույթով)
        /// </summary>
        /// <param name="currency">Պարտատոմսի արժույթ</param>
        /// <returns></returns>
        public List<Account> GetAccountsForBondRepayment(string currency)
        {
            List<Account> transitAccount = Account.GetCustomerTransitAccounts(this.CustomerNumber, ProductQualityFilter.Opened);
            List<Account> accounts = Account.GetAccountsForBondRepayment(this.CustomerNumber, currency);

            List<Account> allAccounts = new List<Account>();
            allAccounts.AddRange(transitAccount);
            allAccounts.AddRange(accounts);
            Localization.SetCulture(allAccounts, Culture);
            return allAccounts;

        }


        /// <summary>
        /// Պարտատոմսի հեռացման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveBondQualityUpdateOrder(BondQualityUpdateOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public BondQualityUpdateOrder GetBondQualityUpdateOrder(long ID)
        {
            BondQualityUpdateOrder order = new BondQualityUpdateOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetBondQualityUpdateOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Պարտատոմսի գումարի գանձման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveBondAmountChargeOrder(BondAmountChargeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }


        public BondAmountChargeOrder GetBondAmountChargeOrder(long ID)
        {
            BondAmountChargeOrder order = new BondAmountChargeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetBondAmountChargeOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        public ActionResult SaveAndApproveDepostitaryAccountOrder(DepositaryAccountOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public DepositaryAccountOrder GetDepositaryAccountOrder(long ID)
        {
            DepositaryAccountOrder order = new DepositaryAccountOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetDepositaryAccountOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        public ActionResult SaveAndApproveTransactionSwiftConfirmOrder(TransactionSwiftConfirmOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveCardUSSDServiceOrder(CardUSSDServiceOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Քարտի USSD ծառայության ակտիվացման հայտի մանրամասները
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public CardUSSDServiceOrder GetCardUSSDServiceOrder(long orderId)
        {
            CardUSSDServiceOrder order = new CardUSSDServiceOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveAndApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        ///  Քարտի SMS ծառայության ակտիվացման/փոփոխման կամ դադարեցման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult ApprovePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Approve(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        ///  Քարտի SMS ծառայության ակտիվացման/փոփոխման կամ դադարեցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SavePlasticCardSMSServiceOrder(PlasticCardSMSServiceOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            result = order.Save(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Քարտի SMS ծառայության ակտիվացման,դադարեցման, փոփոխման  հայտերի մանրամասները
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public PlasticCardSMSServiceOrder GetPlasticCardSMSServiceOrder(long orderId)
        {
            PlasticCardSMSServiceOrder order = new PlasticCardSMSServiceOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }




        public List<ProductNotificationConfigurations> GetProductNotificationConfigurations(ulong productId)
        {
            return ProductNotificationConfigurations.GetProductNotificationConfigurations(productId);
        }

        public ActionResult SaveAndApproveSwiftMessageRejectOrder(SwiftMessageRejectOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public static List<HBProductPermission> GetHBUserProductsPermissions(string hbUserName)
        {
            return HBUser.GetHBUserProductsPermissions(hbUserName);
        }

        public TransactionSwiftConfirmOrder GetTransactionSwiftConfirmOrder(long orderId)
        {
            TransactionSwiftConfirmOrder order = new TransactionSwiftConfirmOrder();
            order.Id = orderId;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveAndApproveCard3DSecureServiceOrder(Card3DSecureServiceOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ProductNotificationConfigurationsOrder GetProductNotificationConfigurationOrder(long ID)
        {
            ProductNotificationConfigurationsOrder order = new ProductNotificationConfigurationsOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        public ActionResult SaveAndApproveFondOrderr(FondOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        public Fond GetFondByID(int ID)
        {
            Fond fond = Fond.GetFondByID(ID);
            return fond;
        }

        public List<Fond> GetFonds(ProductQualityFilter filter)
        {
            List<Fond> fonds = Fond.GetFonds(filter);
            Localization.SetCulture(fonds, this.Culture);
            return fonds;
        }

        public FondOrder GetFondOrder(long ID)
        {
            FondOrder order = new FondOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդի տարանցիկ հաշիվները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Account> GetCustomerTransitAccounts(ProductQualityFilter filter)
        {
            List<Account> accounts = Account.GetCustomerTransitAccounts(this.CustomerNumber, filter);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }


        public Loan GetLoan(string loanFullNumber)
        {
            Loan loan = Loan.GetLoan(loanFullNumber);
            Localization.SetCulture(loan, this.Culture);
            return loan;
        }
        public ActionResult SaveAndApproveLoanProductMakeOutBalanceOrder(LoanProductMakeOutBalanceOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Պարբերական փոխանցման փոփոխման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է պարբերական փոխանցման փոփոխման հայտի տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PeriodicTransferDataChangeOrder GetPeriodicDataChangeOrder(long ID)
        {
            PeriodicTransferDataChangeOrder order = new PeriodicTransferDataChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveAndApproveCreditLineProlongationOrder(CreditLineProlongationOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }




        public CreditLineProlongationOrder GetCreditLineProlongationOrder(int ID)
        {
            CreditLineProlongationOrder order = new CreditLineProlongationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetCreditLineProlongationOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        public ActionResult SaveAndApproveLoanProductDataChangeOrder(LoanProductDataChangeOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }


        public LoanProductDataChangeOrder GetLoanProductDataChangeOrder(long ID)
        {
            LoanProductDataChangeOrder order = new LoanProductDataChangeOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        public List<Account> GetCreditCodesTransitAccounts(ProductQualityFilter filter)
        {
            List<Account> accounts = Account.GetCreditCodesTransitAccounts(this.CustomerNumber, filter);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }

        public FTPRate GetFTPRateDetails(FTPRateType rateType)
        {
            return FTPRate.GetFTPRateDetails(rateType); ;
        }

        public ActionResult SaveAndApproveFTPRateOrder(FTPRateOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public FTPRateOrder GetFTPRateOrder(long ID)
        {
            FTPRateOrder order = new FTPRateOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Ընթացիկ հաշվի փակման հայտի պահպանում
        /// </summary>
        /// <param name="order">Ընթացիկ հաշվի փակման հայտ</param>
        /// <returns></returns>
        public ActionResult SaveAccountClosingOrder(AccountClosingOrder order, string userName)
        {
            order.CustomerNumber = this.CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Ընթացիկ հաշվի փակման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveAccountClosingOrder(AccountClosingOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Միջազգային փոխանցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveInternationalPaymentOrder(InternationalPaymentOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)

            {
                result = order.Save(userName, Source, User);
            }

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վարկային գծի դադարեցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCreditLineTerminationOrder(CreditLineTerminationOrder order, string userName)
        {
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է որոնման տվյալներին համապատասխանող հանձնարարականները
        /// </summary>
        /// <param name="orderFilter">Որոնման պարամետրեր</param>
        /// <returns></returns>
        public List<Order> GetOrders(OrderFilter orderFilter)
        {
            List<Order> orders = Order.GetOrders(orderFilter, this.CustomerNumber);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրաֆիկը՝ նախքան ավանդի ձևակերպումը
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<DepositRepayment> GetDepositRepayments(DepositRepaymentRequest request)
        {
            request.CustomerNumber = this.CustomerNumber;
            List<DepositRepayment> repayments = Deposit.GetDepositRepayment(request);
            return repayments;
        }

        /// <summary>
        /// Ռեեստրի  հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveReestrTransferOrder(ReestrTransferOrder order, short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            //Գործարքների օրեկան սահմանաչափի ստուգում
            if (Order.GetDayOrdersAmount(this.CustomerNumber, order.Id, DateTime.Now) < this.DailyOperationAmountLimit)
            {
                result = order.Approve(schemaType, userName, User);
            }
            else
            {
                //Դուք գերազանցում եք կատարվող գործարքների օրեկան սահմանաչափը:
                result.Errors.Add(new ActionError(74));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder, string userName, short schemaType)
        {
            ActionResult result = arcaCardsTransactionOrder.Save(userName, Source, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ArcaCardsTransactionOrder GetArcaCardsTransactionOrder(long orderId)
        {
            ArcaCardsTransactionOrder order = new ArcaCardsTransactionOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            Localization.SetCulture(order, Culture.Language);
            return order;
        }

        public ActionResult ApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder, string userName, short approvementScheme)
        {
            ActionResult result = arcaCardsTransactionOrder.Approve(userName, approvementScheme);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public string GetPasswordForCustomerDataOrder()
        {
            return CustomerDB.GetPasswordForCustomerDataOrder(this.CustomerNumber);
        }

        public string GetEmailForCustomerDataOrder()
        {
            uint identityId = (uint)GetIdentityId(this.CustomerNumber);
            return CustomerDB.GetEmailForCustomerDataOrder(identityId);
        }

        public short GetBlockingReasonForBlockedCard(string cardNumber)
        {
            return ArcaCardsTransactionOrder.GetBlockingReasonForBlockedCard(cardNumber);
        }

        public ActionResult SaveAndApproveCardToCardOrder(CardToCardOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public CardToCardOrder GetCardToCardOrder(long orderId)
        {
            CardToCardOrder order = new CardToCardOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        public AttachedCardTransactionReceipt GetAttachedCardTransactionDetails(long orderId)
        {
            AttachedCardTransactionReceipt details = new AttachedCardTransactionReceipt();
            details.Doc_Id = orderId;
            details.CustomerNumber = CustomerNumber;
            details.GetAttachedCardTransactionDetails();
            return details;
        }

        public ActionResult SaveCardToCardOrder(CardToCardOrder cardToCardOrder, string userName, short schemaType)
        {
            ActionResult result = cardToCardOrder.Save(userName, Source, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ActionResult ApproveCardToCardOrder(CardToCardOrder cardToCardOrder, string userName, short approvementScheme)
        {
            ActionResult result = cardToCardOrder.Approve(userName, approvementScheme);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ActionResult ToCardWithECommerce(CardToCardOrder cardToCardOrder)
        {
            ActionResult result = cardToCardOrder.ToCardWithECommerce();
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public Dictionary<string, string> GetCardLimits(long productId)
        {
            return CardLimitChangeOrder.GetCardLimits(productId);
        }

        public ActionResult SaveAndApproveCardLimitChangeOrder(CardLimitChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ActionResult SaveCardLimitChangeOrder(CardLimitChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Save(userName, Source, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public CardLimitChangeOrder GetCardLimitChangeOrder(long orderId)
        {
            CardLimitChangeOrder order = new CardLimitChangeOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult ApproveCardLimitChangeOrder(CardLimitChangeOrder order, string userName, short approvementScheme)
        {
            ActionResult result = order.Approve(userName, approvementScheme);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        /// <summary>
        /// Պահպանում է ՀՀ տարածքում/սեփական հաշիվների միջև փոխանցման ձևանմուշը
        /// </summary>
        /// <param name="template"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SavePaymentOrderTemplate(PaymentOrderTemplate template, string userName)
        {
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վարկի մարման խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveLoanMatureOrderTemplate(LoanMatureOrderTemplate template, string userName)
        {
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Կոմունալ վճարման խմբային ծառայության պահպանում
        /// </summary>
        /// <param name="template"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveUtilityPaymentOrderTemplate(UtilityPaymentOrderTemplate template, string userName)
        {
            template.UtilityPaymentOrder.CustomerNumber = this.CustomerNumber;
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ ծառայությունների խմբերը
        /// </summary>
        /// <param name="status">Խմբի կարգավիճակ</param>
        /// <returns></returns>
        public List<OrderGroup> GetOrderGroups(OrderGroupStatus status, OrderGroupType groupType)
        {
            List<OrderGroup> groups = OrderGroup.GetOrderGroups(this.CustomerNumber, status, groupType);
            return groups;
        }

        /// <summary>
        /// Վերադարձնում է Փոխանցում ՀՀ տարածքում/Փոխանցում հաշիվների միջև ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PaymentOrderTemplate GetPaymentOrderTemplate(int id)
        {
            PaymentOrderTemplate template = PaymentOrderTemplate.Get(id, this.CustomerNumber);
            return template;
        }

        /// <summary>
        /// Վերադարձնում է բյուջե փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BudgetPaymentOrderTemplate GetBudgetPaymentOrderTemplate(int id)
        {
            BudgetPaymentOrderTemplate template = BudgetPaymentOrderTemplate.Get(id, this.CustomerNumber);
            return template;
        }

        /// <summary>
        ///  Վերադարձնում է վարկի մարման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LoanMatureOrderTemplate GetLoanMatureOrderTemplate(int id)
        {
            LoanMatureOrderTemplate template = LoanMatureOrderTemplate.Get(id, this.CustomerNumber);
            return template;
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="id">Ձևանմուշի ունիկալ համար</param>
        /// <returns></returns>
        public UtilityPaymentOrderTemplate GetUtilityPaymentOrderTemplate(int id)
        {
            UtilityPaymentOrderTemplate template = UtilityPaymentOrderTemplate.Get(id, this.CustomerNumber);
            return template;
        }

        public ActionResult SaveHBServletRequestOrder(HBServletRequestOrder order, string userName, short schemaType, String clientIp)
        {
            ActionResult result = order.SaveOrder(userName, Source, User, schemaType, clientIp);
            Localization.SetCulture(result, Culture);
            return result;
        }


        public ActionResult ApproveHBServletRequestOrder(HBServletRequestOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();


            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Approve(userName, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }



        public ReadXmlFileAndLog ReadXmlFile(string fileId, short filial, ulong customerNumber, string userName)
        {
            ReadXmlFileAndLog readXmlFileAndLog = new ReadXmlFileAndLog();
            readXmlFileAndLog.actionResult = new ActionResult();

            readXmlFileAndLog = UploadedFile.ReadXmlFile(fileId, filial, customerNumber, userName);
            Localization.SetCulture(readXmlFileAndLog.actionResult, this.Culture);
            readXmlFileAndLog.actionResult.ResultCode = ResultCode.ValidationError;
            return readXmlFileAndLog;
        }
        public PlasticCardOrder GetPlasticCardOrder(long ID)
        {
            PlasticCardOrder plasticCardOrder = new PlasticCardOrder();

            plasticCardOrder.Id = ID;
            plasticCardOrder.CustomerNumber = this.CustomerNumber;
            plasticCardOrder.Get();


            if (Culture.Language != Languages.hy)
            {
                if (plasticCardOrder.CardReportReceivingType != null)
                {
                    DataTable dt = Info.GetCardReportReceivingTypes();
                    DataRow[] cardReportReceivingRow = dt.Select($"type_id = {plasticCardOrder.CardReportReceivingType}");
                    plasticCardOrder.CardReportReceivingTypeDescription = Utility.ConvertAnsiToUnicode(cardReportReceivingRow[0]["Description_eng"].ToString());
                }

                if (plasticCardOrder.ProvidingFilialCode != 0)
                    plasticCardOrder.ProvidingFilialCodeDescription = Info.GetFilialName(plasticCardOrder.ProvidingFilialCode, Culture.Language);

            }
            Localization.SetCulture(plasticCardOrder, Culture);
            return plasticCardOrder;
        }
        /// Վերադարձնում է ներման ենթակա վարկային պարտավորությունների դաշտերը
        /// </summary>
        /// <param name="creditCommitmentForgiveness"></param>
        /// <returns></returns>
        public CreditCommitmentForgivenessOrder GetForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness)
        {
            CreditCommitmentForgivenessOrder creditCommitment = CreditCommitmentForgivenessOrder.GetForgivableLoanCommitment(this.CustomerNumber, creditCommitmentForgiveness);
            return creditCommitment;
        }


        public ActionResult SaveForgivableLoanCommitment(CreditCommitmentForgivenessOrder creditCommitmentForgiveness, string userName, short schemaType)
        {
            ActionResult result = creditCommitmentForgiveness.SaveForgivableLoanCommitment(userName, Source, User, schemaType, creditCommitmentForgiveness);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApprovePlasticCardOrder(PlasticCardOrder cardOrder, short schemaType)
        {
            ActionResult result = cardOrder.SaveAndApprove(User, Source, CustomerNumber, schemaType);

            if (cardOrder.Source == SourceType.Bank && result.ResultCode == ResultCode.DoneAndReturnedValues)
            {
                List<AccountFreezeDetails> freezeDetails = new List<AccountFreezeDetails>();
                ushort freezeReason = 0;
                if (cardOrder.Type == OrderType.AttachedPlasticCardOrder || cardOrder.Type == OrderType.LinkedPlasticCardOrder)
                {
                    Card mainCard = Card.GetCard(cardOrder.PlasticCard.MainCardNumber);
                    if (mainCard != null)
                    {
                        freezeDetails = AccountFreezeDetails.GetAccountFreezeHistory(mainCard.CardAccount.AccountNumber, 1, 0).Where(m => AutomateCardBlockingUnblocking.FreezingReasonsForBlocking.Contains(m.ReasonId)).OrderBy(m => m.RegistrationDate).ToList();
                        freezeReason = (ushort)(freezeDetails?.Any() == true ? freezeDetails.FirstOrDefault().ReasonId : 0);
                    }
                }
                bool hasBankrupt = CustomerDB.HasBankrupt(CustomerNumber);
                if (hasBankrupt || freezeDetails?.Any() == true)
                {
                    var cardNumber = result.Errors[0].Description;

                    ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, hasBankrupt ? ArcaCardsTransationActionReason.Bankrupt : AutomateCardBlockingUnblocking.GetCardTransactionReasonByFreezeReasonId(freezeReason), cardOrder.OperationDate, cardOrder.RegistrationDate);
                    SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Վերադարձնում է Հայտի համար անհրաժեշտ ավտոմատ դաշտերի ստացում
        /// </summary>
        /// <returns></returns>
        public InternationalPaymentOrder GetCustomerDateForInternationalPayment()
        {
            return InternationalPaymentOrder.GetCustomerDateForInternationalPayment(this.CustomerNumber);
        }

        public CreditCommitmentForgivenessOrder GetCreditCommitmentForgiveness(long ID)
        {
            CreditCommitmentForgivenessOrder order = new CreditCommitmentForgivenessOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order = order.Get();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            Localization.SetCulture(order, Culture);
            return order;
        }
        public ActionResult SaveAndApproveArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder, string userName, short schemaType)
        {
            ActionResult result = arcaCardsTransactionOrder.SaveAndApprove(userName, Source, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ActionResult SaveAndApproveAutomateArcaCardsTransactionOrder(ArcaCardsTransactionOrder arcaCardsTransactionOrder, string userName, short schemaType)
        {
            ActionResult result;
            byte cardBlockingActionAvailability = ArcaCardsTransactionOrder.GetCardBlockingActionAvailabilityForFreezing(arcaCardsTransactionOrder.CardNumber, arcaCardsTransactionOrder.ActionReasonId);
            if (cardBlockingActionAvailability == arcaCardsTransactionOrder.ActionType)
            {
                result = arcaCardsTransactionOrder.SaveAndApprove(userName, arcaCardsTransactionOrder.Source, arcaCardsTransactionOrder.user, schemaType);
            }
            else
            {
                result = arcaCardsTransactionOrder.Save(userName, arcaCardsTransactionOrder.Source, arcaCardsTransactionOrder.user, schemaType);
                arcaCardsTransactionOrder.UpdateQuality(OrderQuality.TransactionLimitApprovement);
                arcaCardsTransactionOrder.SetQualityHistoryUserId(OrderQuality.TransactionLimitApprovement, arcaCardsTransactionOrder.user.userID);
            }
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ArcaCardsTransactionOrder CreateArcaCardTransactionOrder(string cardNumber, ArcaCardsTransationActionType actionType, ArcaCardsTransationActionReason reason, DateTime? operDate, DateTime regDate)
        {
            ArcaCardsTransactionOrder arcaCardsTransactionOrder = new ArcaCardsTransactionOrder();
            arcaCardsTransactionOrder.Source = SourceType.Bank;
            arcaCardsTransactionOrder.CardNumber = cardNumber;
            arcaCardsTransactionOrder.CustomerNumber = CustomerNumber;
            arcaCardsTransactionOrder.OperationDate = operDate;
            arcaCardsTransactionOrder.RegistrationDate = regDate;
            arcaCardsTransactionOrder.ActionReasonId = (byte)reason;
            ACBAServiceReference.User user = new User { userID = 88 };
            user.AdvancedOptions = new System.Collections.Generic.Dictionary<string, string>
            {
                { "accessToUnblockCardForSpecificReasons", "1" },
                { "accessToMakeArcaCardTransactionForBankInitiative", "1" },
                { "accessToBlockUnblockCardForCourtProceedingsReason", "1" }
            };
            arcaCardsTransactionOrder.user = user;
            arcaCardsTransactionOrder.FilialCode = 22000;
            arcaCardsTransactionOrder.ActionType = (short)actionType;
            return arcaCardsTransactionOrder;
        }

        /// <summary>
        /// Պահպանում է քարտից քարտ փոխանցման ձևանմուշը
        /// </summary>
        /// <param name="template"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCardToCardOrderTemplate(CardToCardOrderTemplate template, string userName)
        {
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SavePlasticCardOrder(PlasticCardOrder cardOrder, short schemaType, string userName)
        {
            ActionResult result = cardOrder.Save(User, Source, CustomerNumber, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult ApprovePlasticCardOrder(PlasticCardOrder cardOrder, short schemaType, string userName)
        {
            ActionResult result = cardOrder.Approve(User, schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public List<Order> GetOrdersList(OrderListFilter orderListFilter)
        {
            List<Order> orders = Order.GetOrdersList(this.CustomerNumber, orderListFilter);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }



        public ActionResult SaveInternationalOrderTemplate(InternationalOrderTemplate template, string userName)
        {
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public InternationalOrderTemplate GetInternationalOrderTemplate(int id)
        {
            InternationalOrderTemplate template = InternationalOrderTemplate.Get(id, this.CustomerNumber);
            return template;
        }


        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի ձևանմուշների ցանկը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Template> GetCustomerTemplates(TemplateStatus status)
        {
            List<Template> customerTemplates = Template.GetCustomerTemplates(this.CustomerNumber, status);
            Parallel.ForEach(customerTemplates,
                x => Localization.SetCulture(x, Culture));
            return customerTemplates;
        }

        /// <summary>
        /// Վերադարձնում է բյուջե փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardToCardOrderTemplate GetCardToCardOrderTemplate(int templateId)
        {
            CardToCardOrderTemplate template = CardToCardOrderTemplate.Get(templateId, this.CustomerNumber);
            Localization.SetCulture(template, Culture);
            return template;
        }


        public ActionResult SaveReestrTransferOrder(ReestrTransferOrder order, string userName, short schemaType, string fileId)
        {
            ActionResult result = new ActionResult();
            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                order.user = User;
                result = order.SaveReestrTransferOrder(userName, Source, User, schemaType, fileId);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }


        public ActionResult CheckExcelRows(List<ReestrTransferAdditionalDetails> reestrTransferAdditionalDetails, string debetAccount, Languages languages, long orderId)
        {
            return ReestrTransferOrder.CheckExcelRows(reestrTransferAdditionalDetails, debetAccount, languages, orderId);
        }


        public CreditLine GetCardOverDraft(string cardNumber)
        {
            CreditLine creditline = CreditLine.GetCardOverdraft(cardNumber);
            if (creditline != null)
            {
                Localization.SetCulture(creditline, this.Culture);
            }

            return creditline;
        }

        public Dictionary<string, string> GetPlasticCardOrderCardTypes()
        {
            Dictionary<string, string> cardTypes;
            short customerType;
            var dt = Info.GetCardTypes();
            cardTypes = dt.AsEnumerable()
                .ToDictionary<DataRow, string, string>(row => row[0].ToString(),
                                       row => row[2].ToString());

            CustomerMainData mainData = ACBAOperationService.GetCustomerMainData(CustomerNumber);
            customerType = (short)mainData.CustomerType;

            if (customerType == (short)CustomerTypes.physical)
            {
                DateTime? BirthDate = ACBAOperationService.GetCustomerBirthDate(CustomerNumber);

                cardTypes = cardTypes.Where(x => x.Key == "11" || x.Key == "31" || x.Key == "36"
                     || x.Key == "47" || x.Key == "37" || x.Key == "48" || x.Key == "49" || x.Key == "42" || x.Key == "38"
                     || x.Key == "21" || x.Key == "46" || x.Key == "34" || x.Key == "50" || x.Key == "52" || x.Key == "54")
                        .ToDictionary(x => x.Key, x => x.Value);

                if (mainData.ResidenceType != 1)
                {
                    cardTypes = cardTypes.Where(x => x.Key != "34" || x.Key != "40" || x.Key != "41" || x.Key != "50")
                       .ToDictionary(x => x.Key, x => x.Value);
                }

                if (DateTime.Now.Year - BirthDate.Value.Year > 30)
                {
                    cardTypes = cardTypes.Where(x => x.Key != "38")
                        .ToDictionary(x => x.Key, x => x.Value);
                }
                return cardTypes;
            }
            else
            {
                return cardTypes.Where(x => x.Key == "45" || x.Key == "22")
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }


        public List<string> GetDepositAndCurrentAccountCurrencies(OrderType orderType, byte orderSubType, OrderAccountType accountType)
        {
            List<string> currencyList = new List<string>();

            List<Account> accounts = new List<Account>();

            //Ընթացիկ հաշիվներ
            List<Account> currentAccounts = Account.GetCurrentAccounts(this.CustomerNumber, ProductQualityFilter.Opened);

            currentAccounts.RemoveAll(m => m.AccountType == (ushort)ProductType.CardDahkAccount);

            Localization.SetCulture(currentAccounts, Culture);
            accounts.AddRange(currentAccounts);

            List<Account> depositAccounts = Account.GetDepositAccounts(this.CustomerNumber);
            Localization.SetCulture(depositAccounts, Culture);
            accounts.AddRange(depositAccounts);

            accounts.RemoveAll(m => m.Status == 1);

            foreach (var item in accounts)
            {
                string currency = Account.GetAccountCurrency(item.AccountNumber);
                currencyList.Add(currency);
            }

            List<string> noDupes = currencyList.Distinct().ToList();

            return noDupes;
        }

        /// <summary>
        /// Պարբերական փոխանցման փոփոխման հայտի պահպանում 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order, string userName, short schemaType)
        {
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Պարբերական փոխանցման փոփոխման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult ApprovePeriodicDataChangeOrder(PeriodicTransferDataChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Approve(User, Source, userName, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է կենսաթոշակային ֆոնդի մնացորդը
        /// </summary>
        /// <returns></returns>
        public PensionSystem GetPensionBalance()
        {
            PensionSystem pensionSystem = new PensionSystem();
            pensionSystem.GetPensionBalance(CustomerNumber);
            Localization.SetCulture(pensionSystem, Culture);
            return pensionSystem;
        }
        /// <summary>
        /// Վերադարձնում է վիրտուալ քարտի մանրամասն տվյալները
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="customerNumber"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public VirtualCardDetails GetVirtualCardDetails(string cardNumber, ulong customerNumber, short userID)
        {
            VirtualCardDetails virtualCardDetails = new VirtualCardDetails();
            return virtualCardDetails.GetVirtualCardDetails(cardNumber, customerNumber, userID);
        }

        public ActionResult SaveAccountReOpenOrder(AccountReOpenOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        ///  Ակտիվացնում է քարտը ԱՌՔԱ ում, բացում է քարտի քարտային հաշիվները
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="customerNumber"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult ActivateAndOpenProductAccounts(ulong productID, ulong customerNumber, string userName, short schemaType)
        {
            var customerFilial = Customer.GetCustomerFilial(customerNumber);
            User.filialCode = (ushort)customerFilial.key; //SHOULD BE DISCUSSED
            ActionResult actionResult = new ActionResult();
            actionResult.ResultCode = ResultCode.Normal;
            CardRegistrationOrder cardRegistrationOrder = new CardRegistrationOrder();
            cardRegistrationOrder.Card = new PlasticCard();
            cardRegistrationOrder.Card.ProductId = productID;
            cardRegistrationOrder.CustomerNumber = customerNumber;
            cardRegistrationOrder.Type = OrderType.CardRegistrationOrder;
            cardRegistrationOrder.Source = Source;
            cardRegistrationOrder.user = User;
            cardRegistrationOrder.FilialCode = (ushort)customerFilial.key;
            cardRegistrationOrder.OperationDate = DateTime.Now;
            actionResult = cardRegistrationOrder.SaveAndApprove(userName, Source, User, schemaType, true);
            Localization.SetCulture(actionResult, Culture);
            return actionResult;


        }
        /// <summary>
        /// Վերադարձնում է նշված ժամանակահատվածում ստացված աշխատավարձի տվյալները
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="customerNumber"></param>
        /// <param name="languageID"></param>
        /// <returns></returns>
        public List<EmployeeSalary> GetEmployeeSalaryList(DateTime startDate, DateTime endDate, ulong customerNumber, Languages languageID)
        {
            EmployeeSalary employeeSalary = new EmployeeSalary();

            return employeeSalary.GetEmployeeSalaryList(startDate, endDate, customerNumber, languageID);
        }
        /// <summary>
        /// Տվյալ աշխատավարձի մանրամամասն տվյալներ
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="customerNumber"></param>
        /// <param name="languageID"></param>
        /// <returns></returns>
        public EmployeeSalaryDetails GetEmployeeSalaryDetails(int ID, ulong customerNumber, Languages languageID)
        {
            EmployeeSalaryDetails employeeSalaryDetails = new EmployeeSalaryDetails();

            return employeeSalaryDetails.GetEmployeeSalaryDetails(ID, customerNumber, languageID);
        }
        /// <summary>
        /// Աշխատակցի մանրամասն տվյալներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public EmployeePersonalDetails GetEmployeePersonalDetails(ulong customerNumber)
        {
            EmployeePersonalDetails employeePersonalDetails = new EmployeePersonalDetails();

            return employeePersonalDetails.GetEmployeePersonalDetails(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է mobile/web համակարգերում մուտքից հետո բացվող պատուհանի համար անհրաժեշտ տվյալները (Հաշիվներ, Ավանդներ,Վարկեր, Քարտեր) 
        /// </summary>
        /// <returns></returns>
        public async Task<IBankingHomePage> GetIBankingHomePage()
        {
            IBankingHomePage homePage = new IBankingHomePage();
            homePage.Accounts = new ContentResult<List<Account>>();
            homePage.Cards = new ContentResult<List<Card>>();
            homePage.Deposits = new ContentResult<List<Deposit>>();
            homePage.Loans = new ContentResult<List<Loan>>();
            try
            {
                List<Account> accounts = await AccountDB.GetAccountsDigitalBankingAsync(this.CustomerNumber);
                Localization.SetCulture(accounts, this.Culture);
                homePage.Accounts.ResultCode = ResultCode.Normal;
                homePage.Accounts.Content = accounts;
            }
            catch (Exception)
            {
                homePage.Accounts.ResultCode = ResultCode.Failed;
                ActionError actionError = new ActionError(1684);
                Localization.SetCulture(actionError, this.Culture);
                homePage.Accounts.Errors.Add(actionError);
                homePage.Accounts.Description = actionError.Description;
            }

            try
            {
                List<Card> cards = await CardDB.GetCardsAsync(this.CustomerNumber);
                Localization.SetCulture(cards, this.Culture);
                homePage.Cards.ResultCode = ResultCode.Normal;
                homePage.Cards.Content = cards;
            }
            catch (Exception)
            {
                homePage.Cards.ResultCode = ResultCode.Failed;

                ActionError actionError = new ActionError(1682);
                Localization.SetCulture(actionError, this.Culture);
                homePage.Cards.Errors.Add(actionError);
                homePage.Cards.Description = actionError.Description;

            }

            try
            {
                List<Deposit> deposits = await DepositDB.GetDepositsAsync(this.CustomerNumber);
                Localization.SetCulture(deposits, this.Culture);
                homePage.Deposits.ResultCode = ResultCode.Normal;
                homePage.Deposits.Content = deposits;
            }
            catch (Exception)
            {
                homePage.Deposits.ResultCode = ResultCode.Failed;

                ActionError actionError = new ActionError(1681);
                Localization.SetCulture(actionError, this.Culture);
                homePage.Deposits.Errors.Add(actionError);
                homePage.Deposits.Description = actionError.Description;
            }

            try
            {
                List<Loan> loans = await LoanDB.GetLoansAsync(this.CustomerNumber);
                loans.AddRange(LoanDB.GetAparikTexumLoans(this.CustomerNumber));

                var creditLines = await CreditLineDB.GetCreditLinesAsync(this.CustomerNumber);

                Localization.SetCulture(loans, this.Culture);
                Localization.SetCulture(creditLines, this.Culture);
                foreach (var item in loans)
                {
                    if (item != null)
                    {
                        item.NextRepayment = item.GetLoanNextRepayment();
                    }
                }

                foreach (var item in creditLines)
                {
                    if (item != null)
                    {
                        List<CreditLineGrafik> grafik = GetCreditLineGrafik((ulong)item.ProductId);
                        if (grafik.Count != 0)
                        {
                            if (grafik.Count > 1)
                            {
                                grafik.Sort((x, y) => x.EndDate.CompareTo(y.EndDate));
                            }
                            var findedGRafik = grafik.Find(x => x.EndDate >= Utility.GetNextOperDay() && x.Amount - x.MaturedAmount > 0);

                            var NextRepaymentAmount = findedGRafik == null ? default : findedGRafik.Amount - findedGRafik.MaturedAmount;
                            var NextRepaymentDate = findedGRafik == null ? default : findedGRafik.EndDate;

                            Loan creditLine = new Loan
                            {
                                LoanTypeDescription = item.TypeDescription,
                                ProductId = item.ProductId,
                                ProductType = item.ProductType,
                                Currency = item.Currency,
                                LoanType = item.Type,
                                NextRepayment = new LoanRepaymentGrafik { RepaymentDate = NextRepaymentDate, TotalRepayment = NextRepaymentAmount }
                            };
                            loans.Add(creditLine);

                        }
                        else
                        {
                            Loan creditLine = new Loan
                            {
                                LoanTypeDescription = item.TypeDescription,
                                ProductId = item.ProductId,
                                ProductType = item.ProductType,
                                Currency = item.Currency,
                                LoanType = item.Type,
                                NextRepayment = new LoanRepaymentGrafik { RepaymentDate = default, TotalRepayment = 0 }
                            };
                            loans.Add(creditLine);
                        }
                    }
                }


                homePage.Loans.ResultCode = ResultCode.Normal;
                homePage.Loans.Content = loans;
            }
            catch (Exception)
            {
                homePage.Loans.ResultCode = ResultCode.Failed;

                ActionError actionError = new ActionError(1683);
                Localization.SetCulture(actionError, this.Culture);
                homePage.Loans.Errors.Add(actionError);
                homePage.Loans.Description = actionError.Description;
            }

            return homePage;
        }

        /// <summary>
        /// Պահպանում է բյուջե փոխանցման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="template"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveBudgetPaymentOrderTemplate(BudgetPaymentOrderTemplate template, string userName)
        {
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի հայտի համար հասանելի քարտերի ցանկը
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public List<Card> GetCardsForNewCreditLine(OrderType orderType)
        {
            List<Card> cards = Card.GetCardsForNewCreditLine(this.CustomerNumber, orderType);
            Localization.SetCulture(cards, this.Culture);

            return cards;
        }
        /// <summary>
        /// Աշխատակից հանդիսանալու մասին նշման ստացում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public bool IsEmployee(ulong customerNumber)
        {
            return CustomerDB.IsEmployee(customerNumber);
        }

        public string GetAccountsForLeasing(ulong CustomerNumber)
        {
            string accounts = Account.GetAccountsForLeasing(CustomerNumber);
            return accounts;
        }

        public InterestMargin GetInterestMarginDetails(InterestMarginType marginType) => InterestMargin.GetInterestMarginDetails(marginType);


        public InterestMargin GetInterestMarginDetailsByDate(InterestMarginType marginType, DateTime marinDate) => InterestMargin.GetInterestMarginDetailsByDate(marginType, marinDate);


        public ActionResult SaveAndApproveInterestMarginOrder(InterestMarginOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public InterestMarginOrder GetInterestMarginOrder(long ID)
        {
            InterestMarginOrder order = new InterestMarginOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }


        public ActionResult SaveAndApprovePlasticCardRemovalOrder(PlasticCardRemovalOrder cardRemovalOrder, short schemaType)
        {
            ActionResult result = cardRemovalOrder.SaveAndApprove(User, Source, CustomerNumber, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public List<PlasticCard> GetCustomerMainCards()
        {
            List<PlasticCard> cards = PlasticCard.GetCustomerMainCards(this.CustomerNumber);

            return cards;
        }

        public PlasticCardRemovalOrder GetPlasticCardRemovalOrder(long ID)
        {
            PlasticCardRemovalOrder plasticCardRemovalOrder = new PlasticCardRemovalOrder();

            plasticCardRemovalOrder.Id = ID;
            plasticCardRemovalOrder.CustomerNumber = this.CustomerNumber;
            plasticCardRemovalOrder.Get();
            Localization.SetCulture(plasticCardRemovalOrder, Culture);
            return plasticCardRemovalOrder;
        }

        public ActionResult SaveAndApproveCardAccountRemovalOrder(CardAccountRemovalOrder cardOrder, short schemaType)
        {
            ActionResult result = cardOrder.SaveAndApprove(User, Source, CustomerNumber, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public List<PlasticCard> GetCustomerPlasticCards()
        {
            List<PlasticCard> cards = PlasticCard.GetCustomerPlasticCards(this.CustomerNumber);

            return cards;
        }

        /// <summary>
        /// Պահպանում է փոխարկման ձևանմուշը/խմբային ծառայությունը
        /// </summary>
        /// <param name="template"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCurrencyExchangeOrderTemplate(CurrencyExchangeOrderTemplate template, string userName)
        {
            ActionResult result = template.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public CardAccountRemovalOrder GetCardAccountRemovalOrder(long ID)
        {
            CardAccountRemovalOrder cardAccountRemovalOrder = new CardAccountRemovalOrder();
            cardAccountRemovalOrder.Id = ID;
            cardAccountRemovalOrder.CustomerNumber = this.CustomerNumber;
            cardAccountRemovalOrder.Get();
            Localization.SetCulture(cardAccountRemovalOrder, Culture);
            return cardAccountRemovalOrder;
        }
        /// <summary>
        /// Իրավաբանական անձանց դեպքում գործարքի մերժում
        /// </summary>
        /// <param name="orderRejection"></param>
        /// <returns></returns>
        public ActionResult RejectOrder(OrderRejection orderRejection)
        {
            orderRejection.CustomerNumber = CustomerNumber;
            ActionResult result = orderRejection.Reject(this.Culture.Language);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveVirtualCardStatusChangeOrder(VirtualCardStatusChangeOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public VirtualCardStatusChangeOrder GetVirtualCardStatusChangeOrder(long orderId)
        {
            VirtualCardStatusChangeOrder order = new VirtualCardStatusChangeOrder();
            order.Id = orderId;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveAndApprovePaymentToARCAOrder(PaymentToARCAOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                order.user = User;
                result = order.SaveAndApprove(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        private bool CheckOtherCardAvailability(OrderType orderType, byte? orderSubType)
        {
            var orderTypes = new[]
            {
                new { OrderType = OrderType.CardToCardOrder, OrderSubType = 0 },
                  new { OrderType = OrderType.LoanMature, OrderSubType = 0 },
                    new { OrderType = OrderType.CommunalPayment, OrderSubType = 0 },
                      new { OrderType = OrderType.RATransfer, OrderSubType = 3 },
                        new { OrderType = OrderType.RATransfer, OrderSubType = 6 }
            };
            return orderTypes.Any(x => x.OrderType == orderType && (orderType == OrderType.RATransfer ? x.OrderSubType == orderSubType : true));
        }
        public string GetOrderRejectReason(long orderId, OrderType type)
        {
            return Order.GetOrderRejectReason(orderId, type, this.Culture);
        }


        public List<Account> GetAccountsDigitalBanking()
        {
            List<Account> accounts = Account.GetAccountsDigitalBanking(this.CustomerNumber);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }

        /// <summary>
        /// Վարկի հետաձգման հայտի տվյալների պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveLoanDelayOrder(LoanDelayOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հետաձգված հայտի  փոփոխման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LoanDelayOrder GetLoanDelayOrder(long ID)
        {
            LoanDelayOrder order = new LoanDelayOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public static LoanRepaymentDelayDetails GetLoanRepaymentDelayDetails(ulong productId)
        {
            return LoanRepaymentDelayDetails.GetLoanRepaymentDelayDetails(productId);
        }

        public Dictionary<string, string> GetOrderDetailsForReport(long orderId, ulong customerNumber)
        {
            return CurrencyExchangeOrder.GetOrderDetailsForReport(orderId, customerNumber);
        }

        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման չեղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveRemittanceCancellationOrder(RemittanceCancellationOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);

            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման չեղարկման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public ActionResult SaveRemittanceCancellationOrder(RemittanceCancellationOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Save(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);

            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման չեղարկման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public ActionResult ApproveRemittanceCancellationOrder(RemittanceCancellationOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Approve(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);

            Localization.SetCulture(result, Culture);
            return result;

        }


        /// <summary>
        /// Ստանում է արագ դրամական համակարգերով ուղարկված/ստացված փոխանցման չեղարկման հայտի մանրամասները
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public RemittanceCancellationOrder GetRemittanceCancellationOrder(long id, string authorizedUserSessionToken, string userName, string clientIP)
        {
            RemittanceCancellationOrder order = new RemittanceCancellationOrder();
            order.Id = id;
            order.CustomerNumber = this.CustomerNumber;
            order.Get(authorizedUserSessionToken, userName, clientIP);
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Արագ Դրամական Համակարգերով փոխանցման ուղարկման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveFastTransferOrder(FastTransferPaymentOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        ///  Արագ Դրամական Համակարգերով փոխանցման ուղարկման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public ActionResult ApproveFastTransferOrder(FastTransferPaymentOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Approve(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);

            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Վերադարձնում է արագ դրամական համակարգերով փոխանցման տվյալների փոփոխման հայտի մանրամասները
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RemittanceAmendmentOrder GetRemittanceAmendmentOrder(long id)
        {
            RemittanceAmendmentOrder order = new RemittanceAmendmentOrder();
            order.Id = id;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }



        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման տվյալների փոփոխման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveRemittanceAmendmentOrder(RemittanceAmendmentOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Save(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);

            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Արագ Դրամական Համակարգերով ստացված/ուղարկված փոխանցման տվյալների փոփոխման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public ActionResult ApproveRemittanceAmendmentOrder(RemittanceAmendmentOrder order, string userName, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = new ActionResult();

            //result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Approve(userName, Source, User, schemaType, authorizedUserSessionToken, clientIP);

            Localization.SetCulture(result, Culture);
            return result;

        }
        public List<Order> GetConfirmRequiredOrders(ulong customerNumber, string userName, int subTypeId, DateTime startDate, DateTime endDate, string langId = "", string receiverName = "", string account = "", bool period = true, string groups = "", int quality = -1)
        {
            List<Order> orders = Order.GetConfirmRequiredOrders(customerNumber, userName, subTypeId, startDate, endDate, langId, receiverName, account, period, groups, quality);
            Localization.SetCulture(orders, this.Culture);
            return orders;
        }

        public R2ARequestOutput SaveAndApproveSTAKPaymentOrder(STAKPaymentOrder order, string userName, short schemaType)
        {
            R2ARequestOutputResponse response = order.SaveAndApprove(userName, Source, User, schemaType);

            if (response.ActionResult.ResultCode == ResultCode.Normal)
            {
                response.R2ARequestOutput.StatusCode = "90";
                response.R2ARequestOutput.StatusCodeName = "Payout Completed";
                response.R2ARequestOutput.ResponseCode = "1";
                response.R2ARequestOutput.ResponseMessage = "R2A փոխանցումը վճարված է։";
            }
            else
            {
                Localization.SetCulture(response.ActionResult, Culture);

                response.R2ARequestOutput.StatusCode = "85";
                response.R2ARequestOutput.StatusCodeName = "Remittance to Account Failed";  // vercnel STAK-i tvac dictionary-ic
                response.R2ARequestOutput.ResponseCode = String.Join(" | ", response.ActionResult.Errors.Select(c => c.Code));
                response.R2ARequestOutput.ResponseMessage = String.Join(" | ", response.ActionResult.Errors.Select(d => d.Description));  // "ՍՏԱԿ համակարգով R2A փոխանցման վճարման ժամանակ տեղի ունեցավ սխալ։";

                Guid GUID = Guid.NewGuid();
                response.ActionResult.Errors.ForEach(m => order.SaveValidationErrors(m.Code, m.Description, GUID.ToString()));
            }


            return response.R2ARequestOutput;
        }
        /// <summary>
        ///  Վերադարձնում է հաճախորդի SAP SRM համակարգում գրանցված հիմնական հեռախոսահամարը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static string GetCustomerPhoneNumber(ulong customerNumber)
        {
            return CustomerDB.GetCustomerPhoneNumber(customerNumber);

        }
        ///  Հետաձգված վարկի հեռացման հայտի  տվյալների պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCancelLoanDelayOrder(CancelDelayOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է չեղարկված հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CancelDelayOrder GetCancelLoanDelayOrder(long ID)
        {
            CancelDelayOrder order = new CancelDelayOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Հեռահար բանկինգի պայմանագրի ամբողջական հասանելիությունների տրամադրում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveHBApplicationFullPermissionsGrantingOrder(HBApplicationFullPermissionsGrantingOrder order, string userName, short schemaType)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հեռահար բանկինգի պայմանագրի ամբողջական հասանելիությունների տրամադրման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public HBApplicationFullPermissionsGrantingOrder GetHBApplicationFullPermissionsGrantingOrder(long ID)
        {
            HBApplicationFullPermissionsGrantingOrder order = new HBApplicationFullPermissionsGrantingOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetHBApplicationFullPermissionsGrantingOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Ստուգում է մոբայլ բենկինգ ակտիվացնելիս, փոխարինելիս կամ հավելյալ մոբայլ բենկինգ կցելիս հաճախորդի հիմնական էլ.փոստի կամ հիմնական բջջ.հեռախոսահամարի ռիսկային փոփոխությունների առկայությունը
        /// </summary>
        /// <param name="TokenSerial"></param>
        /// <returns></returns>
        public static bool CheckMobileBankingCustomerDetailsRiskyChanges(string TokenSerial)
        {
            return CustomerDB.CheckMobileBankingCustomerDetailsRiskyChanges(TokenSerial);
        }
        public ActionResult SaveCardToOtherCardsOrder(CardToOtherCardsOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();


            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                order.user = User;
                result = order.Save(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }
        public ActionResult ApproveCardToOtherCardsOrder(CardToOtherCardsOrder cardToCardOrder, string userName, short approvementScheme)
        {
            ActionResult result = cardToCardOrder.Approve(userName, approvementScheme, Culture);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }
        public CardToOtherCardsOrder GetCardToOtherCardsOrder(long ID)
        {
            CardToOtherCardsOrder order = new CardToOtherCardsOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();

            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// "Փոխարինում` նոր համար, նոր ժամկետ - առանց վ․գ․" հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveNonCreditLineCardReplaceOrder(NonCreditLineCardReplaceOrder order, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(Source, User, schemaType, CustomerNumber);

            if (CustomerDB.HasBankrupt(CustomerNumber) && order.Source == SourceType.Bank && result.ResultCode == ResultCode.DoneAndReturnedValues)
            {
                var cardNumber = result.Errors[0].Description;
                if (ArcaCardsTransactionOrder.GetPreviousBlockingOrderId(cardNumber) == null)
                {
                    ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, ArcaCardsTransationActionReason.Bankrupt, order.OperationDate, order.RegistrationDate);
                    SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                }
            }

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է «Փոխարինում` նոր համար, նոր ժամկետ - առանց վ․գ․» հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public NonCreditLineCardReplaceOrder GetNonCreditLineCardReplaceOrder(long ID)
        {
            NonCreditLineCardReplaceOrder order = new NonCreditLineCardReplaceOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetNonCreditLineCardReplaceOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է "Փոխարինում` նոր համար, նոր ժամկետ - վ․գ․"  հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CreditLineCardReplaceOrder GetCreditLineCardReplaceOrder(long ID)
        {
            CreditLineCardReplaceOrder order = new CreditLineCardReplaceOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetCreditLineCardReplaceOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի քարտերը լրացուցիչ տվյալների հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public List<PlasticCard> GetCustomerPlasticCardsForAdditionalData(bool IsClosed)
        {
            return PlasticCard.GetCustomerPlasticCardsForAdditionalData(this.CustomerNumber, IsClosed);
        }

        public ActionResult SaveCardAdditionalDataOrder(CardAdditionalDataOrder additionalDataOrder, short approvementScheme, string userName)
        {
            ActionResult result = additionalDataOrder.SaveAndApprove(User, Source, CustomerNumber, approvementScheme);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public CardAdditionalDataOrder GetCardAdditionalDataOrder(long orderID)
        {
            CardAdditionalDataOrder order = new CardAdditionalDataOrder();
            order.Id = orderID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// "Փոխարինում` նոր համար, նոր ժամկետ - վ․գ․" հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCreditLineCardReplaceOrder(CreditLineCardReplaceOrder order, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(Source, User, schemaType, CustomerNumber);

            if (CustomerDB.HasBankrupt(CustomerNumber) && order.Source == SourceType.Bank && result.ResultCode == ResultCode.DoneAndReturnedValues)
            {
                var cardNumber = result.Errors[0].Description;
                if (ArcaCardsTransactionOrder.GetPreviousBlockingOrderId(cardNumber) == null)
                {
                    ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, ArcaCardsTransationActionReason.Bankrupt, order.OperationDate, order.RegistrationDate);
                    SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է քարտի փոխարինման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ReplacedCardAccountRegOrder GetReplacedCardAccountRegOrder(long ID)
        {
            ReplacedCardAccountRegOrder order = new ReplacedCardAccountRegOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetReplacedCardAccountRegOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Փոխարինված քարտի հաշվի կցման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveReplacedCardAccountRegOrder(ReplacedCardAccountRegOrder order, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(Source, User, schemaType, CustomerNumber);

            if (CustomerDB.HasBankrupt(CustomerNumber) && order.Source == SourceType.Bank && result.ResultCode == ResultCode.DoneAndReturnedValues)
            {
                var cardNumber = result.Errors[0].Description;
                if (ArcaCardsTransactionOrder.GetPreviousBlockingOrderId(cardNumber) == null)
                {
                    ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, ArcaCardsTransationActionReason.Bankrupt, order.OperationDate, order.RegistrationDate);
                    SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        /// <summary>
        /// Վերադարձնում է "Փոխարինում` նույն համար, նույն ժամկետ" հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public PINRegenerationOrder GetPINRegenerationOrder(long ID)
        {
            PINRegenerationOrder order = new PINRegenerationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetPINRegenerationOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// "Փոխարինում` նույն համար, նույն ժամկետ" հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePINRegOrder(PINRegenerationOrder order, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprovePINRegOrder(Source, User, schemaType, CustomerNumber);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է գրանցված վերջին բլոկավորման/ապաբլոկավորման հայտի մեկնաբանությունը առկայության դեպքում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public string GetPreviousBlockUnblockOrderComment(string cardNumber)
        {
            return ArcaCardsTransactionOrder.GetPreviousBlockUnblockOrderComment(cardNumber);
        }


        /// <summary>
        /// Քարտի մասնաճյուղի փոփոխություն
        /// </summary>
        public ChangeBranchOrder GetChangeBranchOrder(long ID)
        {
            ChangeBranchOrder order = new ChangeBranchOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();

            Localization.SetCulture(order, Culture);
            return order;
        }


        public ChangeBranchOrder GetFilialCode(long cardNumber)
        {
            ChangeBranchOrder order = new ChangeBranchOrder();
            order.CardNumber = cardNumber;
            order.GetFilialCode();

            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveAndApproveChangeBranchOrder(ChangeBranchOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաստատման ենթակա գործարքների քանակը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="userName"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public int GetConfirmRequiredOrdersCount(ulong customerNumber, string userName, string groups = "")
        {
            return Order.GetConfirmRequiredOrdersCount(customerNumber, userName, groups);

        }

        public static string GetCustomerHVHH(ulong customerNumber)
        {
            return CustomerDB.GetCustomerHVHH(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է "Չվերաթողարկել քարտը" հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardNotRenewOrder GetCardNotRenewOrder(long ID)
        {
            CardNotRenewOrder order = new CardNotRenewOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetCardNotRenewOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// "Չվերաթողարկել քարտը" հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardNotRenewOrder(CardNotRenewOrder order, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (Validation.BankOpDayIsClosed())
            {
                // Հնարավոր չէ կատարել գործողություն:Գործառնական օրվա կարգավիճակը փակ է:
                result.Errors.Add(new ActionError(766));
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApproveCardNotRenewOrder(Source, User, schemaType, CustomerNumber);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveCardAccountClosingOrder(CardAccountClosingOrder accountclosingorder, short approvementScheme)
        {
            ActionResult result = accountclosingorder.SaveAndApprove(Source, CustomerNumber, approvementScheme);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public CardAccountClosingOrder GetCardAccountClosingOrder(long orderID)
        {
            CardAccountClosingOrder order = new CardAccountClosingOrder();
            order.Id = orderID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        public List<CardDataChangeOrder> GetCardDataChangesByProduct(long productAppId, short fieldType)
        {
            return CardDataChangeOrder.GetCardDataChangesByProduct(productAppId, fieldType);
        }


        /// <summary>
        /// Վարկի տոկոսագումարների զիջման հայտի տվյալների պահպանում և հաստատում
        /// </summary>
        public ActionResult SaveAndApproveLoanInterestRateConcessionOrder(LoanInterestRateConcessionOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAttachedCardOrderInHB(CardToCardOrder attachedcardToCardOrder, string userName, short schemaType)
        {
            ActionResult result = Utility.ValidateOpeartionLimits(attachedcardToCardOrder.Amount, attachedcardToCardOrder.Currency, OneOperationAmountLimit, attachedcardToCardOrder.IsAttachedCard);
            if (result.ResultCode == ResultCode.Normal)
            {
                result = attachedcardToCardOrder.SaveAttachedCardtoCardOrder(userName, Source, User, schemaType);
            }
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult DeclineAttachedCardToCardOrderQuality(long docId)
        {
            CardToCardOrder order = new CardToCardOrder
            {
                user = User,
                Id = docId,
                CustomerNumber = CustomerNumber
            };
            ActionResult result = order.DeclineAttachedCardToCardOrderQuality();
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }
        public ActionResult ApproveAttachedCardToCardOrderQuality(CardToCardOrder cardToCardOrder)
        {
            ActionResult result = cardToCardOrder.ApproveAttachedCardToCardOrder();
            return result;
        }

        public ActionResult SavePensionPaymentOrder(PensionPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SavePensionPaymentOrder(order, userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Անքարտ կանխիկացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveCardLessCashOutOrder(CardlessCashoutOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            if (order.DebitAccount.IsRestrictedAccount())
            {
                result.Errors.Add(new ActionError(1814));
                result.ResultCode = ResultCode.ValidationError;
                Localization.SetCulture(result, Culture);
                return result;
            }

            result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Անքարտ կանխիկացման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ContentResult<string> ApproveCardLessCashOutOrder(CardlessCashoutOrder order, short schemaType, string userName)
        {
            ContentResult<string> result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է զիջված հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LoanInterestRateConcessionOrder GetLoanInterestRateConcessionOrder(long OrderId)
        {
            LoanInterestRateConcessionOrder order = new LoanInterestRateConcessionOrder();
            order.Id = OrderId;
            order.CustomerNumber = this.CustomerNumber;
            order.GetLoanInterestRateConcessionOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Վերադարձնում է վճարման հանձնարարականը 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardlessCashoutOrder GetCardLessCashOutOrder(long id)
        {
            CardlessCashoutOrder paymentOrder = new CardlessCashoutOrder();
            paymentOrder.Id = id;
            if (Culture == null)
                Culture = new Culture(Languages.eng);
            paymentOrder.Get(Culture.Language);
            Localization.SetCulture(paymentOrder, Culture);

            return paymentOrder;
        }
        /// <summary>
        /// Վերադարձնում է հաճախորդի հիմնական քարտերը, լրացուցիչ քարտի հայտի համար 
        /// </summary>
        /// <param name="ExceptionType"></param>
        /// <returns></returns>
        public List<PlasticCard> GetCustomerMainCardsForAttachedCardOrder()
        {
            List<PlasticCard> cards = PlasticCard.GetCustomerMainCardsForAttachedCardOrder(this.CustomerNumber);

            return cards;
        }

        public ActionResult SaveAndApproveSberPaymentOrder(SberIncomingTransferOrder order, string userName, short schemaType)
        {
            Transfer transfer = new Transfer();
            transfer.Id = transfer.GetTransferIdByDocId(order.Id);
            transfer.Get();

            CurrencyExchangeOrder currencyorder = new CurrencyExchangeOrder(order, transfer, this);
            ActionResult result = currencyorder.SaveAndApprove(userName, Source, User, schemaType);

            return result;
        }

        /// <summary>
        /// Generates SberBank Transfer order (type 239) and makes a payment if the confirmation of order type 239 is normal
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public (ActionResult, DateTime?) SaveAndApproveSberIncomingTransferOrder(SberIncomingTransferOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result = order.SaveAndApproveWithoutConfirm(userName, schemaType);

            order.Id = result.Id;

            if (result.ResultCode == ResultCode.Normal)
            {
                Task.Factory.StartNew(() =>
                {
                    ActionResult confirmResult = order.Confirm(order.user);

                    if (confirmResult.ResultCode == ResultCode.Normal)
                        SaveAndApproveSberPaymentOrder(order, userName, schemaType);
                });
            }
            return (result, order.RegistrationDate);
        }

        /// <summary>
        /// Գումարի փոխանցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveBillSplitOrder(BillSplitOrder order, string userName)
        {
            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// </summary>
        /// <param name="order">Հղումով փոխանցման</param>
        /// <returns></returns>
        public ActionResult SaveLinkPaymentOrder(LinkPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.Save();
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Հղումով փոխանցման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ContentResult<string> ApproveLinkPaymentOrder(LinkPaymentOrder order, short schemaType, string userName)
        {
            ContentResult<string> result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult SavePreferredAccountOrder(PreferredAccountOrder order, string userName)
        {
            ActionResult result = order.SavePreferredAccountOrder(order, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public PreferredAccountOrder GetPreferredAccountOrder(long id)
        {
            PreferredAccountOrder order = new PreferredAccountOrder
            {
                Id = id,
                CustomerNumber = CustomerNumber
            };
            order.Get();
            Localization.SetCulture(order, Culture);

            return order;
        }

        public List<Account> GetQrAccounts()
        {
            List<Account> accounts = new List<Account>();

            List<Account> currentAccounts = Account.GetCurrentAccounts(CustomerNumber, ProductQualityFilter.Opened);
            List<Account> depositAccounts = Account.GetDepositAccounts(CustomerNumber);
            List<Account> cardAccounts = Account.GetCardAccounts(CustomerNumber);

            cardAccounts.ForEach(m =>
            {
                Card card = Card.GetCardWithOutBallance(m.AccountNumber);
                if (card != null)
                    m.AccountDescription = card.CardNumber + " " + card.CardType;
            });
            accounts.AddRange(depositAccounts);
            currentAccounts.RemoveAll(m => m.Currency != "AMD" || m.TypeOfAccount == 282 || m.AccountType == (ushort)ProductType.CardDahkAccount || m.AccountType == (ushort)ProductType.AparikTexum);
            accounts.AddRange(currentAccounts);
            accounts.AddRange(cardAccounts);
            accounts.RemoveAll(m => m.Currency != "AMD" || m.ISDahkCardTransitAccount(m.AccountNumber));

            Localization.SetCulture(accounts, Culture);

            return accounts;
        }

        /// <summary>
        /// Քարտի վերաբացման մուտքագրման հայտի պահպանում և հաստատում
        /// Վերադարձնում է հաշվի հեռացման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardReOpenOrder(CardReOpenOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Վերադարձնում է քարտի վերաբացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardReOpenOrder GetCardReOpenOrder(long ID)
        {
            CardReOpenOrder order = new CardReOpenOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult ApprovePreferredAccountOrder(long id)
        {
            PreferredAccountOrder preferredAccountOrder = new PreferredAccountOrder();
            ActionResult result = preferredAccountOrder.ApprovePreferredAccountOrder(id);
            return result;
        }

        /// <summary>
        /// BillSplit հայտի ստացում
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BillSplitOrder GetBillSplitOrder(long id)
        {
            BillSplitOrder order = new BillSplitOrder();
            order.Id = id;
            order.CustomerNumber = this.CustomerNumber;
            order.Get(Culture.Language);
            Localization.SetCulture(order, Culture);
            return order;
        }


        /// <summary>
        /// Bill Split հայտի կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ContentResult<List<BillSplitLinkResult>> ApproveBillSplitOrder(BillSplitOrder order, short schemaType, string userName)
        {
            ContentResult<List<BillSplitLinkResult>> result = order.Approve(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }


        public ActionResult SaveAndApproveBillSplitSenderRejectionOrder(BillSplitSenderRejectionOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;

        }

        public BillSplitSenderRejectionOrder GetBillSplitSenderRejectionOrder(long ID)
        {
            BillSplitSenderRejectionOrder order = new BillSplitSenderRejectionOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public List<SentBillSplitRequest> GetSentBillSplitRequests()
        {
            List<SentBillSplitRequest> requests = SentBillSplitRequest.GetSentBillSplitRequests(this.CustomerNumber, this.Culture);
            return requests;
        }

        public List<ReceivedBillSplitRequest> GetReceivedBillSplitRequests()
        {
            List<ReceivedBillSplitRequest> requests = ReceivedBillSplitRequest.GetReceivedBillSplitRequests(this.CustomerNumber, this.Culture);
            return requests;
        }

        public SentBillSplitRequest GetSentBillSplitRequest(long orderId)
        {
            return SentBillSplitRequest.GetSentBillSplitRequest(this.CustomerNumber, orderId, Culture);
        }

        public ActionResult SaveAndApproveBillSplitReminderOrder(BillSplitReminderOrder order, string userName, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public BillSplitReminderOrder GetBillSplitReminderOrder(long ID)
        {
            BillSplitReminderOrder order = new BillSplitReminderOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.Get();
            Localization.SetCulture(order, Culture);
            return order;
        }
        /// <summary>
        /// "Վերաթողարկել քարտը" հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardRenewOrder(CardRenewOrder order, short schemaType)
        {
            ActionResult result = new ActionResult();
            if (Validation.BankOpDayIsClosed())
            {
                // Հնարավոր չէ կատարել գործողություն:Գործառնական օրվա կարգավիճակը փակ է:
                result.Errors.Add(new ActionError(766));
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(Source, User, schemaType, CustomerNumber);

            if (order.Source == SourceType.Bank && result.ResultCode == ResultCode.DoneAndReturnedValues && CustomerDB.HasBankrupt(CustomerNumber))
            {
                var cardNumber = PlasticCard.GetPlasticCard(order.GetCardNewAppID(), true).CardNumber;
                if (ArcaCardsTransactionOrder.GetPreviousBlockingOrderId(cardNumber) == null)
                {
                    ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, ArcaCardsTransationActionReason.Bankrupt, order.OperationDate, order.RegistrationDate);
                    SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է «Վերաթողարկել քարտը» հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CardRenewOrder GetCardRenewOrder(long ID)
        {
            CardRenewOrder order = new CardRenewOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order = CardRenewOrder.GetCardRenewOrder(ID);
            Localization.SetCulture(order, Culture);
            return order;
        }
        public ActionResult SaveVisaAliasOrder(VisaAliasOrder order, string userName)
        {
            order.user = User;
            ActionResult result = order.Save(userName, Source);
            Localization.SetCulture(result, Culture);
            return result;
        }
        public ActionResult ApproveVisaAliasOrder(VisaAliasOrder order, string userName, short approvementScheme)
        {
            ActionResult result = order.Approve(approvementScheme, userName, User);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }
        public VisaAliasOrder GetVisaAliasOrder(long ID)
        {
            VisaAliasOrder order = new VisaAliasOrder();
            order.Id = ID;
            order.CustomerNumber = CustomerNumber;
            order.Get();

            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Ստուգում է «Վերաթողարկել քարտը» հայտի համար անհրաժեշտ որոշ տվյալներ
        /// </summary>
        public List<string> CheckCardRenewOrder(CardRenewOrder order)
        {
            return CardRenewOrder.CheckRenew(order);
        }

        /// <summary>
        /// Քարտի վերաթողարկման հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveRenewedCardAccountRegOrder(RenewedCardAccountRegOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);

            if (order.Source == SourceType.Bank && result.ResultCode == ResultCode.DoneAndReturnedValues && CustomerDB.HasBankrupt(CustomerNumber))
            {
                var cardNumber = order.Card.CardNumber;
                var validationDate = Card.GetCard((ulong)order.Card.ProductId, order.CustomerNumber).ValidationDate;
                if (ArcaCardsTransactionOrder.GetPreviousBlockingOrderId(cardNumber, validationDate) == null)
                {
                    ArcaCardsTransactionOrder blockOrder = CreateArcaCardTransactionOrder(cardNumber, ArcaCardsTransationActionType.Block, ArcaCardsTransationActionReason.Bankrupt, order.OperationDate, order.RegistrationDate);
                    SaveAndApproveAutomateArcaCardsTransactionOrder(blockOrder, User.userName, schemaType);
                }
            }
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Քարտի վերաթողարկման հետ կապված զգուշացումները
        /// </summary>
        /// <param name="oldCard"></param>
        /// <returns></returns>
        public List<string> GetRenewedCardAccountRegWarnings(Card oldCard)
        {
            return RenewedCardAccountRegOrder.GetRenewedCardAccountRegWarnings(oldCard, Culture);
        }


        /// <summary>
        /// Վերադարձնում է քարտի վերաթողարկման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public RenewedCardAccountRegOrder GetRenewedCardAccountRegOrder(long ID)
        {
            RenewedCardAccountRegOrder order = new RenewedCardAccountRegOrder();
            Localization.SetCulture(order, Culture);
            return order.GetRenewedCardAccountRegOrder(ID);
        }

        /// <summary>
        /// Ավանդի գրավով սպառողական վարկի հեռացման հայտի պահպանում և հաստատում
        /// </summary>       
        /// <returns></returns>
        public ActionResult SaveAndApproveDeleteLoan(DeleteLoanOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckDeleteAvailability(order.AppId));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, schemaType, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult RemoveAccountOrder(AccountRemovingOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            ActionResult result = order.RemoveAccountOrder(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveAccountRemoving(AccountRemovingOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveThirdPersonAccountRightsTransfer(ThirdPersonAccountRightsTransferOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveMRDataChangeOrder(MRDataChangeOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.SaveAndApprove(userName, Source, User, schemaType);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveDepositaryAccountOrder(DepositaryAccountOrder depositaryAccountOrder, string userName)
        {
            ActionResult result = depositaryAccountOrder.Save(userName, User);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ActionResult ApproveDepositaryAccountOrder(DepositaryAccountOrder depositaryAccountOrder, string userName, short approvementScheme)
        {
            ActionResult result = depositaryAccountOrder.Approve(userName, approvementScheme);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        /// </summary>
        /// <param name="order">Հղումով փոխանցման</param>
        /// <returns></returns>
        public ActionResult SaveAndApprovePayerLinkPaymentOrder(PayerLinkPaymentOrder order)
        {
            User user = new User() { userID = 88 };
            order.user = user;
            ActionResult result = order.SaveAndApprove(user.userName, SourceType.AcbaOnline, user, 3);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveVisaAliasOrder(VisaAliasOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            order.user = User;
            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);

            return result;
        }

        public VisaAliasOrder VisaAliasOrderDetails(long orderId)
        {
            VisaAliasOrder visaAliasOrder = new VisaAliasOrder();
            visaAliasOrder = visaAliasOrder.GetVisaAliasOrder(orderId);
            return visaAliasOrder;
        }

        public CardHolderAndCardType GetCardTypeAndCardHolder(string CardNumber)
        {
            VisaAliasOrder visaAliasOrder = new VisaAliasOrder();
            return visaAliasOrder.GetCardTypeAndCardHolder(CardNumber);
        }

        public ActionResult SaveBondOrder(BondOrder order, string userName)
        {
            ActionResult result = order.Save(userName);
            if (result.ResultCode == ResultCode.ValidationError)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        public ActionResult ApproveBondOrder(BondOrder order, string userName, short approvementScheme)
        {
            ActionResult result = order.Approve(userName, User, approvementScheme);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        /// <summary>
        /// Վերդարձնում է բաժնտոմսի արժեկտրոնների վճարման համար նախատեսված հաշիվները (դրամային ընթացիկ)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<Account> GetAccountsForStock()
        {

            List<Account> currentAccounts = Account.GetAccountsForStock(this.CustomerNumber);
            Localization.SetCulture(currentAccounts, Culture);
            return currentAccounts;
        }


        /// <summary>
        /// Սպառողական վարկի դիմումի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Սպառողական վարկի դիմումի հայտի ստացում
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ConsumeLoanApplicationOrder GetConsumeLoanApplicationOrder(long ID)
        {
            ConsumeLoanApplicationOrder order = new ConsumeLoanApplicationOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetConsumeLoanApplicationOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Սպառողական վարկի ձևակերպման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveConsumeLoanSettlementOrder(ConsumeLoanSettlementOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ConsumeLoanSettlementOrder GetConsumeLoanSettlementOrder(long ID)
        {
            ConsumeLoanSettlementOrder order = new ConsumeLoanSettlementOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetConsumeLoanSettlementOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// </summary>
        /// <param name="order">Հղումով փոխանցման</param>
        /// <returns></returns>
        public ActionResult SaveAndApproveCardlessCashoutCancellationOrder(CardlessCashoutCancellationOrder order)
        {
            ActionResult result = order.SaveAndApprove();
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի բոլոր ընթացիկ հաշիվները
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Account> GetAllCurrentAccounts()
        {
            List<Account> accounts = Account.GetAllCurrentAccounts(this.CustomerNumber);
            Localization.SetCulture(accounts, this.Culture);
            return accounts;
        }


        public ActionResult ApproveConsumeLoanSettlementOrder(ConsumeLoanSettlementOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(userName, schemaType, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Բյուջե վճարման հանձնարարականի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult ApproveBudgetPaymentOrder(BudgetPaymentOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            if (Utility.CheckOperationLimit(order.Amount, order.Currency, OneOperationAmountLimit))
            {
                result = order.Approve(userName, Source, User, schemaType);
            }
            else
            {
                result.Errors.Add(new ActionError(66));
                result.ResultCode = ResultCode.ValidationError;
            }
            Localization.SetCulture(result, Culture);
            return result;

        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ ծառայությունների խումբը
        /// </summary>
        /// <param name="status">Խմբի կարգավիճակ</param>
        /// <returns></returns>
        public OrderGroup GetOrderGroup(int id) => OrderGroup.GetOrderGroup(this.CustomerNumber, id);


        /// <summary>
        /// ԱՊՊԱ պայմանագրի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveVehicleInsuranceOrder(VehicleInsuranceOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// ԱՊՊԱ պայմանագրի հայտի թարմացում
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public ActionResult UpdateVehicleInsuranceOrder(VehicleInsuranceOrder order, short schemaType, string userName)
        {
            order.CustomerNumber = CustomerNumber;

            ActionResult result = order.Update(Source, User, schemaType, userName);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Ստուգում է «Քարտի հեռացում» հայտի համար անհրաժեշտ որոշ տվյալներ
        /// </summary>
        public List<string> CheckPlasticCardRemovalOrder(PlasticCardRemovalOrder order, User user)
        {
            bool fromCardDepartment;
            user.AdvancedOptions.TryGetValue("isCardDepartment", out string isCardDepartment);
            fromCardDepartment = isCardDepartment == "1";
            return PlasticCardRemovalOrder.CheckPlasticCardRemovalOrder(order, fromCardDepartment);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public VehicleInsuranceOrder GetVehicleInsuranceOrder(long ID)
        {
            VehicleInsuranceOrder order = new VehicleInsuranceOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetVehicleInsuranceOrder();
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult ApproveVehicleInsuranceOrder(VehicleInsuranceOrder Order, short schemaType, string userName)
        {
            ActionResult result = Order.Approve(User, schemaType, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult ValidateVehicleInusuranceOrderForSending(VehicleInsuranceOrder VehicleInsuranceOrder)
        {
            ActionResult result = VehicleInsuranceOrder.ValidateForSend();
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Արժեթղթերի առուվաճառքի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveSecuritiesTradingOrder(SecuritiesTradingOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Արժեթղթերի առուվաճառքի հայտի ստացում
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public SecuritiesTradingOrder GetSecuritiesTradingOrder(long ID, Languages Language)
        {
            SecuritiesTradingOrder order = new SecuritiesTradingOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetSecuritiesTradingOrder(Language);
            Localization.SetCulture(order, Culture);
            return order;
        }

        /// <summary>
        /// Արժեթղթերի առուվաճառքի հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveSecuritiesTradingOrder(SecuritiesTradingOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(userName, schemaType, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult InsertASWAContractResponseDetails(long DocId, bool IsCompleted, short TypeOfFunction, string Description)
        {
            ActionResult result = VehicleInsuranceOrder.InsertASWAContractResponseDetails(DocId, IsCompleted, TypeOfFunction, Description);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAswaSearchResponseDetails(VehicleInsuranceOrder Order)
        {
            ActionResult result = Order.SaveAswaSearchResponseDetails();
            Localization.SetCulture(result, Culture);
            return result;
        }

        public VehicleInsuranceOrder GetAswaSearchResponseDetails(long Id)
        {
            VehicleInsuranceOrder order = new VehicleInsuranceOrder();
            order.CustomerNumber = this.CustomerNumber;
            order.GetAswaSearchResponseDetails(Id);
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult SaveBrokerContractOrder(BrokerContractOrder order, string userName)
        {
            ActionResult result = order.Save(User, userName);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveAndApproveBrokerContractOrder(BrokerContractOrder order, short schemaType)
        {
            ActionResult result = order.SaveAndApprove(schemaType, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        public BrokerContractOrder GetBrokerContractOrder(long id)
        {
            BrokerContractOrder order = new BrokerContractOrder
            {
                Id = id,
                CustomerNumber = CustomerNumber
            };
            order.GetBrokerContractOrder();
            Localization.SetCulture(order, Culture);

            return order;
        }

        public ActionResult ApproveBrokerContractOrder(BrokerContractOrder order, string userName, short schemaType)
        {
            ActionResult result = order.Approve(userName, User, schemaType);
            if (result.ResultCode == ResultCode.ValidationError || result.ResultCode == ResultCode.Failed)
            {
                Localization.SetCulture(result, Culture);
            }
            return result;
        }

        /// <summary>
        /// Անքարտ կանխիկացման հայտի ուղարկում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ApproveCardLessCashOutOrderForApproveOrdes(CardlessCashoutOrder order, short schemaType, string userName)
        {
            ActionResult result = order.ApproveForApproves(schemaType, userName, User);
            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Բորսայում կատարված գործարքին համապատասխան հայտի հաստատում և կատարում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult SaveAndApproveSecuritiesMarketTradingOrder(SecuritiesMarketTradingOrder order)
        {
            ActionResult result = order.SaveAndApprove();
            Localization.SetCulture(result, Culture);
            return result;
        }


        public ActionResult SaveAndApproveSecuritiesTradingOrderCancellationOrder(SecuritiesTradingOrderCancellationOrder order)
        {
            ActionResult result = order.SaveAndApprove();
            Localization.SetCulture(result, Culture);
            return result;
        }

        /// <summary>
        /// Արժեթղթերի առուվաճառքի հայտերի ստացում
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<SecuritiesTradingOrder> GetSecuritiesTradingOrders(short QualityType, DateTime StartDate, DateTime EndDate, Languages Language)
        {
            List<SecuritiesTradingOrder> orders = SecuritiesTradingOrder.GetSecuritiesTradingOrders(this.CustomerNumber, QualityType, StartDate, EndDate, Language);
            Localization.SetCulture(orders, Culture);
            return orders;
        }

        public List<Bond> GetGovernmentBonds(ulong CustomerNumber)
        {
            Bond bond = new Bond();
            return bond.GetGovernmentBonds(CustomerNumber);
        }

        /// <summary>
        /// Պահատուփի տուգանքի մարման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>

        public ActionResult SaveDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Save(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }


        /// <summary>
        /// Պահատուփի տուգանքի մարման հայտի հաստատում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult ApproveDepositCasePenaltyMatureOrder(DepositCasePenaltyMatureOrder order, string userName, short schemaType)
        {

            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = order.Approve(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public ActionResult SaveEventTicketOrder(EventTicketOrder order, string userName)
        {
            order.CustomerNumber = CustomerNumber;
            if (order.OrderNumber == "" || order.OrderNumber == null)
                order.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            ActionResult result = order.Save(userName, Source, User);

            Localization.SetCulture(result, Culture);
            return result;
        }

        public EventTicketOrder GetEventTicketOrder(long ID, Languages Language)
        {
            EventTicketOrder order = new EventTicketOrder();
            order.Id = ID;
            order.CustomerNumber = this.CustomerNumber;
            order.GetEventTicketOrder(Language);
            Localization.SetCulture(order, Culture);
            return order;
        }

        public ActionResult ApproveEventTicketOrder(EventTicketOrder order, short schemaType, string userName)
        {
            ActionResult result = order.Approve(userName, schemaType, User);
            Localization.SetCulture(result, Culture);
            return result;
        }

        //Davit Pos
        public ActionResult SaveAndApproveNewPosLocationOrder(NewPosLocationOrder order, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();
            order.CustomerNumber = this.CustomerNumber;

            result.Errors.AddRange(Validation.CheckOperationAvailability(order, User));
            if (result.Errors.Count > 0)
            {
                Localization.SetCulture(result, Culture);
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            order.user = User;
            result = order.SaveAndApprove(userName, Source, User, schemaType);

            Localization.SetCulture(result, Culture);

            return result;
        }
        //Davit Pos
        public NewPosLocationOrder NewPosApplicationOrderDetails(long orderId)
        {
            NewPosLocationOrder OrderDetail = new NewPosLocationOrder();

            OrderDetail = OrderDetail.NewPosApplicationOrderDetails(orderId);

            return OrderDetail;
        }

        //Davit Pos
        public List<string> GetPosTerminalActivitySphere()
        {
            NewPosLocationOrder OrderDetail = new NewPosLocationOrder();
            return OrderDetail.GetPosTerminalActivitySphere();
        }
    }
}
