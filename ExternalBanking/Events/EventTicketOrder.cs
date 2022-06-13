using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager.EventsDB;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking.Events
{
    public class EventTicketOrder : PaymentOrder
    {
        /// <summary>
        /// Քանակ
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Միջոցառում
        /// </summary>
        public Event Event { get; set; }

        /// <summary>
        /// Հայտի չզեղչված գումար
        /// </summary>
        public double NonDiscountedAmount { get; set; }

        /// <summary>
        /// Հաճախորդի էլ.հասցե
        /// </summary>
        public string EMailAddress { get; set; }

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

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = EventTicketOrderDB.Save(this, userName, source);

                ActionResult resultOpPerson = base.SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }



                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }


        internal void Complete()
        {

            if (this.Source != SourceType.Bank)
                this.RegistrationDate = DateTime.Now.Date;

            if (this.DebitAccount != null && (!this.ValidateForTransit && this.ValidateDebitAccount))
            {
                this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
            }
            else
            {
                //ԱԳՍ-ով փոխանցումների դեպք
                //Նշված դեպքում դրամարկղի հաշիվ պետք չէ որոշել
                if (this.DebitAccount != null && this.DebitAccount.Status == 7)
                {
                    this.DebitAccount = Account.GetSystemAccount(this.DebitAccount.AccountNumber);
                }
                else
                    this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.DebitAccount.Currency, user.filialCode);
            }


            this.Event = Events.GetEventSubTypes((EventTypes)Event.Id, Languages.hy).Where(e => e.SubTypeId == Event.SubTypeId).First();

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source != SourceType.Bank || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }

            Customer customer = new Customer(this.CustomerNumber, Languages.hy);
            customer.Source = Source;
            if (this.SubType == 1)
            {
                this.ReceiverAccount = new Account();
                this.ReceiverAccount.AccountNumber = this.Event.ReceiverAccount;
                this.ReceiverBankCode = Convert.ToInt32(ReceiverAccount.AccountNumber.Substring(0, 5));
            }


            this.ReceiverAccount = Account.GetAccount(this.ReceiverAccount.AccountNumber);

            this.Receiver = this.ReceiverAccount.AccountDescription;
            this.Description = this.Event.EventSubTypeName?.ToString() + " տոմսի գնում";
        }


        /// <summary>
        /// Վճարման հանձնարարականի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user, TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (GetCustomerEventTicketsCount(Event.SubTypeId, this.CustomerNumber, Event.StartDate.Date) + this.Quantity > this.Event.MaxQuantityPerUser)
            {
                //«acba digital-ով կարող եք գնել առավելագույնը 20 տոմս»
                result.Errors.Add(new ActionError(2073));
            }

            if (this.OnlySaveAndApprove == false)
            {
                string developerSpecialAccountsNonCash = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccountsNonCash"];
                string developerSpecialAccounts = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccounts"];

                if (Account.CheckAccessToThisAccounts(this.ReceiverAccount?.AccountNumber) == 118 && this.DebitAccount?.AccountNumber != "220004130948000")
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if (this.ReceiverAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if (this.DebitAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }

            }

            if (Source == SourceType.Bank)
            {
                if (this.ReasonId == 0 && this.DebitAccount.IsCardAccount())
                {
                    //Պատճառ դաշտը ընտրված չէ։
                    result.Errors.Add(new ActionError(1523));
                }

                if (this.ReasonId == 99 && string.IsNullOrEmpty(this.ReasonIdDescription) && this.DebitAccount.IsCardAccount())
                {
                    //Պատճառի այլ դաշտը լրացված չէ։
                    result.Errors.Add(new ActionError(1524));
                }
            }

            if (this.DebitAccount.IsCardAccount())
            {
                Card card = Card.GetCardWithOutBallance(DebitAccount.AccountNumber);
                if (card.RelatedOfficeNumber == 2405)
                {
                    result.Errors.Add(new ActionError(1525));
                }
                else if ((card.RelatedOfficeNumber == 2572 || card.RelatedOfficeNumber == 2573 || card.RelatedOfficeNumber == 2574) && (this.Type == OrderType.RATransfer && this.SubType != 3))
                {
                    result.Errors.Add(new ActionError(1525));
                }
                else if ((card.RelatedOfficeNumber == 2572 || card.RelatedOfficeNumber == 2573 || card.RelatedOfficeNumber == 2574) && (this.Type == OrderType.RATransfer && this.SubType == 3) && this.UseCreditLine == true)
                {
                    result.Errors.Add(new ActionError(1526));
                }
            }



            result.Errors.AddRange(Validation.ValidateCashOperationAvailability(this, user));

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            if (templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));


                result.Errors.AddRange(Validation.ValidateOPPerson(this, this.ReceiverAccount, this.DebitAccount.AccountNumber));
            }


            if (this.ReceiverAccount != null && this.DebitAccount != null && this.ReceiverAccount.AccountNumber == this.DebitAccount.AccountNumber)
            {
                //Կրեդիտ և դեբիտ հաշիվները նույնն են:
                result.Errors.Add(new ActionError(11));
            }
            else
            {

                //Դեբետ հաշվի ստուգում
                result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));


                //Կրեդիտ հաշվի ստուգում
                result.Errors.AddRange(Validation.ValidateReceiverAccount(this));


                if (result.Errors.Count > 0)
                {
                    return result;
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
                    return result;
                }


            }



            if (string.IsNullOrEmpty(this.Currency))
            {
                result.Errors.Add(new ActionError(254));
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

            //Նկարագրություն և Ստացող դաշտերի ստուգում միայն Փոխանցում ՀՀ տարածքում տեսակի համար
            if ((this.Type == OrderType.RATransfer && this.SubType != 3 && this.SubType != 1 && this.SubType != 4) || this.ValidateForConvertation || this.ValidateForCash)
            {
                result.Errors.AddRange(ValidateTextData());

                if ((this.Type == OrderType.RATransfer && this.SubType != 3 && this.SubType != 1 && this.SubType != 4 && this.Source == SourceType.Bank) || this.Type == OrderType.CashForRATransfer)
                {
                    //6 մլն-ից բարձր փոխանցումներ ֆայլի առկայություն: 
                    result.Errors.AddRange(Validation.ValidateAttachmentDocument(this));
                }
            }


            if ((this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer) && this.SubType != 5)
            {
                if (this.DebitAccount?.IsAttachedCard != true)
                {

                    if (!Validation.CheckReciverBankStatus(this.ReceiverBankCode))
                    {
                        //Ստացողի բանկը փակ է:
                        result.Errors.Add(new ActionError(786));
                    }

                }
            }
            if (Source != SourceType.AcbaOnline && Source != SourceType.AcbaOnlineXML && Source != SourceType.ArmSoft && Source != SourceType.MobileBanking && Source != SourceType.AcbaMat)
            {
                if (Account.IsUserAccounts(user.userCustomerNumber, this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber))
                {
                    //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                    result.Errors.Add(new ActionError(544));
                }
            }



            if (this.Source != SourceType.SSTerminal && Source != SourceType.CashInTerminal)
            {
                if (this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.CashForRATransfer && this.ValidateForCash && !this.ValidateForConvertation && this.Currency != "AMD" && !Validation.IsCurrencyAmountCorrect(this.Amount, this.Currency) || ((this.Type == OrderType.ReestrTransferOrder || this.Type == OrderType.CashOutFromTransitAccountsOrder) && this.Currency != "AMD" && !Validation.IsCurrencyAmountCorrect(this.Amount, this.Currency)))
                {
                    //Գործարքի {0} գումարը պետք է լինի արժույթի նվազագույն անվանական արժեքի և ամբողջ թվի արտադրյալ: Շարունակելու համար ուղղեք գործարքի գումարը:
                    result.Errors.Add(new ActionError(1053, new string[] { this.Currency }));
                }
            }


            if (this.DebitAccount.AccountType == 115)
            {

                Card card = Card.GetCardWithOutBallance(this.ReceiverAccount.AccountNumber);
                if (card == null || (card.CardNumber != this.DebitAccount.ProductNumber))
                {
                    //ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվից փոխանցում հնարավոր է կատարել միայն տվյալ քարտին
                    result.Errors.Add(new ActionError(1064));
                }

            }



            if (this.UseCreditLine)
            {

               if (this.ReceiverAccount.AccountType != 10)
                {

                    //Վարկային գծի միջոցներից հնարավոր է փոխանցում կատարել միայն ընթացիկ հաշվին
                    result.Errors.Add(new ActionError(1222));
                }
                else if (Validation.IsDAHKAvailability(this.CustomerNumber))
                {
                    //{0} Հաճախորդը գտնվում է ԴԱՀԿ արգելանքի տակ:
                    result.Errors.Add(new ActionError(516, new string[] { this.CustomerNumber.ToString() }));
                }
                else if (this.DebitAccount.IsCardAccount())
                {
                    Card card = Card.GetCard(this.DebitAccount);
                    if (card.CreditLine != null && (card.CreditLine.Quality == 5 || card.CreditLine.Quality == 11))
                    {
                        //Տվյալ կարգավիճակով վարկային գծից չի թույատրվում փոխանցում կատարել
                        result.Errors.Add(new ActionError(1223));
                    }
                }
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }


            CustomerMainData mainData = ACBAOperationService.GetCustomerMainData(this.CustomerNumber);
            if (mainData.Emails == null || !mainData.Emails.Exists(dt => dt.priority.key == 1)) 
            {
                //Տոմսերի վաճառքը հնարավոր չէ, քանի որ Դուք բանկում չունեք գրանցված էլ. հասցե։
                result.Errors.Add(new ActionError(2075));
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

        public void GetEventTicketOrder(Languages Language)
        {
            EventTicketOrderDB.GetEventTicketOrder(this, Language);
        }

        /// <summary>
        /// Վերադարձնում է տվյալ հաճախորդի ուղարկած հայտերի քանակը տվյալ միջոցառման ենթատեսակի համար
        /// </summary>
        /// <param name="eventSubTypeId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static int GetCustomerEventTicketsCount(int eventSubTypeId, ulong customerNumber, DateTime eventStartDate)
        {
            return EventTicketOrderDB.GetCustomerEventTicketsCount(eventSubTypeId, customerNumber, eventStartDate);
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

            if (GetCustomerEventTicketsCount(Event.SubTypeId, this.CustomerNumber, Event.StartDate.Date) + this.Quantity > this.Event.MaxQuantityPerUser)
            {
                //«acba digital-ով կարող եք գնել առավելագույնը 20 տոմս»
                result.Errors.Add(new ActionError(2073));
            }


            if (this.OnlySaveAndApprove == false)
            {
                string developerSpecialAccountsNonCash = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccountsNonCash"];
                string developerSpecialAccounts = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccounts"];

                if (Account.CheckAccessToThisAccounts(this.ReceiverAccount?.AccountNumber) == 118 && this.DebitAccount?.AccountNumber != "220004130948000"
                    || this.ReceiverAccount?.AccountType == 119)
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if (this.ReceiverAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if ((this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation)
                    && this.ReceiverAccount?.AccountType == 119 && developerSpecialAccountsNonCash == "1")
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }


                if (!(this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation) &&
                    this.DebitAccount?.AccountType == 119 || this.FeeAccount?.AccountType == 119)
                {
                    //Նշված հաշվից ելքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }

                if (this.DebitAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }

            }

            if (this.Quality != OrderQuality.Draft && this.Quality != OrderQuality.Approved)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }



            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
            {

                ///Մուտքային հաշվի ստուգում:
                string recAcc = this.ReceiverAccount.AccountNumber.ToString();

                if (recAcc.Length != 15 || !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12)))
                {
                    //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                    result.Errors.Add(new ActionError(19));
                }
                else if (this.ReceiverBankCode == 10300 && recAcc[5] == '9')
                {
                    if (recAcc.Length != 17 ||
                        !Utility.CheckAccountNumberControlDigit(recAcc.Substring(5)))
                    {
                        //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                        result.Errors.Add(new ActionError(19));
                    }
                }
                else if (this.ReceiverBankCode.ToString()[0] == '9')
                {
                    if (recAcc.Length != 12 || !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12)))
                    {
                        //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                        result.Errors.Add(new ActionError(19));
                    }
                }



                if (Validation.HasOverdueLoan(this.DebitAccount, 1, 1))
                {
                    //Գոյություն ունի ժամկետանց/դուրսգրված պրոդուկտ
                    result.Errors.Add(new ActionError(1247));
                    return result;
                }

                if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
                {
                    //Մուտքագրված գումարը սխալ է:
                    result.Errors.Add(new ActionError(25));
                }



                if (this.ReceiverAccount != null)
                {
                    if (Account.IsAccountForbiddenForTransfer(this.ReceiverAccount))
                    {
                        //...հաշիվը ժամանակավոր է, խնդրում ենք դիմել Բանկ
                        result.Errors.Add(new ActionError(1548, new string[] { this.ReceiverAccount.AccountNumber.ToString() }));
                    }

                }

            }

            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.ArmSoft || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML)
            {
                if (!Validation.IsBankOpen(this.ReceiverBankCode))
                {
                    result.Errors.Add(new ActionError(739));
                }
            }

            CustomerMainData mainData = ACBAOperationService.GetCustomerMainData(this.CustomerNumber);
            if (mainData.Emails == null || !mainData.Emails.Exists(dt => dt.priority.key == 1)) 
            {
                //Տոմսերի վաճառքը հնարավոր չէ, քանի որ Դուք բանկում չունեք գրանցված էլ. հասցե։
                result.Errors.Add(new ActionError(2075));
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

    }
}
