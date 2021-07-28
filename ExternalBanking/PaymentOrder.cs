using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.UtilityPaymentsManagment;
using ExternalBanking.ServiceClient;

namespace ExternalBanking
{

    /// <summary>
    /// Վճարման հանձնարարական
    /// </summary>
    public class PaymentOrder : Order
    {
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
        //public override Account DebitAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Ստացող
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Ստացողի Բանկ
        /// </summary>
        public int ReceiverBankCode { get; set; }

        /// <summary>
        /// Փոխանակման փոխարժեք
        /// </summary>
        public double ConvertationRate { get; set; }

        /// <summary>
        /// Խաչաձև (Կրկնակի, Cross) փոխանակման դեպքում 2-րդ փոխարժեք
        /// </summary>
        public double ConvertationRate1 { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public double TransferFee { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճարի հաշվեհամար
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        ///Շտապ փոխանցում
        /// </summary>
        public bool UrgentSign { get; set; }

        /// <summary>
        ///Քարտային ելքագրման միջնորդավճար
        /// </summary>
        public double CardFee { get; set; }


        /// <summary>
        ///Քարտային ելքագրման միջնորդավճարի արժույթ
        /// </summary>
        public string CardFeeCurrency { get; set; }


        /// <summary>
        /// Փոխանակման խաչաձև փոխարժեք
        /// </summary>
        public double ConvertationCrossRate { get; set; }

        /// <summary>
        /// Փոխանցման ունիկալ համար 
        /// </summary>
        public ulong TransferID { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) փաստաթղթի տեսակ 
        /// </summary>
        public byte CreditorDocumentType { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) փաստաթղթի համար  
        /// </summary>
        public string CreditorDocumentNumber { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) անուն ազգանուն / անվանում  
        /// </summary>
        public string CreditorDescription { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) կարգավիճակ 
        /// </summary>
        public int CreditorStatus { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) կարգավիճակի նկարագրություն 
        /// </summary>
        public string CreditorStatusDescription { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) հաճախորդի համար
        /// </summary>
        public ulong CreditorCustomerNumber { get; set; }

        /// <summary>
        /// Այլ անձի (պարտատիրոջ) մահվան վկայական
        /// </summary>
        public string CreditorDeathDocument { get; set; }

        /// <summary>
        /// վարկային/կրեդիտ կոդը
        /// </summary>
        public string CreditCode { get; set; }

        /// <summary>
        ///վարկառուի անվանումը
        /// </summary>
        public string Borrower { get; set; }

        /// <summary>
        /// վարկի մարման տեսակ
        /// </summary>
        public string MatureType { get; set; }

        /// <summary>
        /// վարկի մարման տեսակի նկարագրություն
        /// </summary>
        public string MatureTypeDescription { get; set; }

        /// <summary>
        /// Նվիրաբերողի հրապարակում
        /// </summary>
        public byte PublicContributor { get; set; }

        /// <summary>
        /// Օգտագործել վարկային գծերի միջոցները
        /// </summary>
        public bool UseCreditLine { get; set; }

        /// <summary>
        /// SWIFT հաղորդագրության ունիկալ համար
        /// </summary>
        public int SwiftMessageID { get; set; }

        /// <summary>
        /// Քարտային հաշվից ելքագրման պատճառի  Id
        /// </summary>
        public int ReasonId { get; set; }

        /// <summary>
        /// Քարտային հաշվից ելքագրման պատճառի նկարագրություն
        /// </summary>
        public string ReasonIdDescription { get; set; }

        /// <summary>     
        /// Գրանցման ժամ 
        /// </summary>
        public string RegistrationTime { get; set; }

        /// <summary>
        /// ՀԲ ում թղթակցային հաշիվներ
        /// </summary>
        public ulong? CorrespondentAccounts { get; set; }

        /// <summary>
        /// ՀԲ ում թղթակցային հաշիվներ(Edit)
        /// </summary>
        public ulong? EditedCorrespondentAccounts { get; set; }

        /// <summary>
        /// Շեղված փոխարժեքով գործարքի նշում: 
        /// </summary>
        public bool ForDillingApprovemnt { get; set; } = false;

        /// <summary>
        /// ՀԲ-ի "փոխանցում բանկի ներսում" գործարքի ԴԱՀԿ արգելադրման նպատակ
        /// </summary>
        public int ArmPaymentType { get; set; } = 0;

        /// <summary>
        /// ՀԲ-ի "փոխանցում բանկի ներսում" գործարքի կրեդիտոր հաճախորդը ունի ԴԱՀԿ թե ոչ
        /// </summary>
        public bool CreditorHasDAHK { get; set; } = false;



        public void Get()
        {
            PaymentOrderDB.GetPaymentOrder(this);
            OPPerson = OrderDB.GetOrderOPPerson(Id);
            Fees = GetOrderFees(Id);

            if (Fees.Exists(m => m.Type == 7))
            {
                CardFee = this.Fees.Find(m => m.Type == 7).Amount;
                Currency = this.Fees.Find(m => m.Type == 7).Currency;
                Fees.Find(m => m.Type == 7).Account = this.DebitAccount;
            }
            if (Fees.Exists(m => m.Type == 20))
            {
                TransferFee = this.Fees.Find(m => m.Type == 20).Amount;
                FeeAccount = this.Fees.Find(m => m.Type == 20).Account;
            }


        }

        private void Get(PaymentOrder order)
        {
            PaymentOrderDB.GetPaymentOrder(order);
        }

        /// <summary>
        /// Վճարման հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend(user);

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

                    //Վիվասել ՄՏՍ ընթացիկ հաշվին կատարված փոխանցում
                    if (((this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3)) || this.Type == OrderType.Convertation ||
                    this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation)
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

                    short amlResult = 0;
                    //Բանկի ներսում հաշիվների միջև փոխանցման և կոնվերտացիայի դեպքում ստուգում ենք ԱՄԼ ստուգման անհրաժեշտությունը
                    if (this.Type == OrderType.RATransfer && this.SubType == 1 || this.Type == OrderType.InBankConvertation)
                    {

                        if (nextQuality != OrderQuality.Approved)
                        {
                            ulong receiver_customer_number = this.ReceiverAccount.GetAccountCustomerNumber();
                            if (this.CustomerNumber != receiver_customer_number)
                            {
                                amlResult = PaymentOrderDB.MatchAMLConditions(this);
                                if (amlResult != 1)
                                {


                                    amlResult = PaymentOrderDB.MatchAMLConditions(this, receiver_customer_number);
                                }
                                //եթե կա ԱՄԼ ստուգման անհրաժեշտություն, պահպանում ենք փոխանցման տվյալները ԱՄԼ ծրագրում ցուցադրելու համար
                                if (amlResult == 1)
                                {
                                    PaymentOrderDB.AddInBankTransferDetailsForAML(this, amlResult);

                                }
                            }
                        }
                    }

                    if (this.Type == OrderType.Convertation && nextQuality != OrderQuality.Approved)
                    {
                        this.DebitAccount.Currency = Account.GetAccountCurrency(this.DebitAccount.AccountNumber);
                        this.ReceiverAccount.Currency = Account.GetAccountCurrency(this.ReceiverAccount.AccountNumber);
                        if (Source != SourceType.SberBankTransfer)
                            this.CheckDillingConfirm();
                    }



                    result = base.Approve(schemaType, userName, amlResult);

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

        /// <summary>
        /// Վճարման հանձնարարականի հեռացում, միայն խմբագրվող կարգավիճակ 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Delete(string userName, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = base.Delete(userName);
                LogOrderChange(user, Action.Delete);
                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
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

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = PaymentOrderDB.Save(this, userName, source);
                if (this.AdditionalParametrs != null && this.AdditionalParametrs.Exists(m => m.AdditionValue == "LeasingAccount"))
                {
                    LeasingDB.SaveLeasingPaymentOrderDetails(this);
                }

                ActionResult resultOrderFee = SaveOrderFee();
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

        /// <summary>
        /// Ստուգում է տեքստային տվյլաները
        /// </summary>
        /// <returns></returns>
        public List<ActionError> ValidateTextData()
        {
            List<ActionError> result = new List<ActionError>();
            result.AddRange(base.ValidateOrderDescription());
            if ((this.Type == OrderType.RATransfer && this.SubType != 3 && this.SubType != 1 && this.SubType != 4) || this.Type == OrderType.CashForRATransfer)
            {
                //Partapani unicode stugum
                //Soci hamari unicode stugum
                //socnosoc-i unicode stugum
                //andznagri unicode stugum
                //Partapani andznagri unicode stugum
                if (this.Receiver == null || this.Receiver == "")
                {
                    //Ստացողի անունը մուտքագրված չէ:
                    result.Add(new ActionError(24));
                }
                else if (Receiver.Length > 255)
                {
                    //«Ստացող» դաշտի առավելագույն նիշերի քանակն է 255:
                    result.Add(new ActionError(123));
                }
                else if (Utility.IsExistForbiddenCharacter(Receiver))
                {
                    //«Ստացող» դաշտի  մեջ կա անթույլատրելի նշան`
                    result.Add(new ActionError(79));
                }
            }
            return result;
        }
        /// <summary>
        /// Վճարման հանձնարարականի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(User user, TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (this.OnlySaveAndApprove == false)
            {
                string developerSpecialAccountsNonCash = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccountsNonCash"];
                string developerSpecialAccounts = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccounts"];

                if (Account.CheckAccessToThisAccounts(this.ReceiverAccount?.AccountNumber) == 118 && this.DebitAccount?.AccountNumber != "220004130948000"
                    || (!(this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation) && this.ReceiverAccount?.AccountType == 119))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if ((this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation)
                    && this.ReceiverAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
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

                if ((this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation)
                    && this.DebitAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }

                if ((this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation)
                    && this.DebitAccount?.AccountType == 119 && developerSpecialAccountsNonCash == "1")
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }
            }

            if (Source == SourceType.Bank && (this.Type == OrderType.RATransfer || this.Type == OrderType.Convertation || this.Type == OrderType.InternationalTransfer || this.Type == OrderType.CashCredit))
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

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
            {

                if (this.SubType == 2 && this.Type == OrderType.RATransfer)
                {
                    result.Errors.AddRange(Validation.CheckSymbolsExistanceForRATransfer(this.CreditorDescription, this.CreditorDocumentNumber,
                        this.Receiver, this.Description));
                }

                if (this.Type == OrderType.RATransfer && this.SubType == 4 && this.DebitAccount?.AccountType == 46)
                    result.Errors.Add(new ActionError(1922));

            }


            if ((this.Type == OrderType.RATransfer || this.Type == OrderType.Convertation || this.Type == OrderType.InternationalTransfer) && this.DebitAccount.IsCardAccount())
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

            if (Source == SourceType.Bank && this.Type == OrderType.RATransfer)
            {
                if (this.UrgentSign)
                {
                    TimeSpan start = new TimeSpan(14, 30, 0);
                    TimeSpan end = new TimeSpan(15, 30, 0);
                    TimeSpan now = DateTime.Now.TimeOfDay;

                    if (!((now >= start) && (now <= end)))
                    {
                        //ՀՀ դրամով փոխանցումը կարող է ընդունվել Շտապ պայմանով ժամը 14:30-ից մինչև 15:30-ը:
                        result.Errors.Add(new ActionError(1538));
                    }
                }
            }


            if (this.Type != OrderType.TransitNonCashOutCurrencyExchangeOrder && this.Type != OrderType.TransitNonCashOut)
                result.Errors.AddRange(Validation.ValidateCashOperationAvailability(this, user));

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            if (templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));


                result.Errors.AddRange(Validation.ValidateOPPerson(this, this.ReceiverAccount, this.DebitAccount.AccountNumber));
            }
            if (this.TransferID != 0)
                result.Errors.AddRange(Validation.ValidateForTransfer(this, user));

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

                if (this.ValidateForConvertation && this.DebitAccount.Currency != "AMD" && this.ReceiverAccount.Currency != "AMD")
                {
                    ushort crossVariant = CurrencyExchangeOrder.GetCrossConvertationVariant(this.DebitAccount.Currency, this.ReceiverAccount.Currency);
                    if (crossVariant == 0)
                    {
                        //Տվյալ զույգ արժույթների համար փոխարկում չի նախատեսված:
                        result.Errors.Add(new ActionError(658));
                        return result;
                    }

                }


                if (this.ValidateDebitAccount && this.Type != OrderType.CashOutFromTransitAccountsOrder && this.Type != OrderType.SSTerminalCashInOrder && this.Type != OrderType.SSTerminalCashOutOrder)

                {
                    //Դեբետ հաշվի ստուգում
                    result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
                }
                //else if (this.Type != OrderType.CashDebit && this.Type != OrderType.CashDebitConvertation)
                //{
                //    result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber ));
                //}

                if (this.Type != OrderType.CashCredit && this.Type != OrderType.CashConvertation
                    && this.Type != OrderType.CashCreditConvertation && this.Type != OrderType.CashOutFromTransitAccountsOrder
                    && this.Type != OrderType.TransitCashOutCurrencyExchangeOrder && this.Type != OrderType.CardServiceFeePayment && this.Type != OrderType.TransitCashOut
                    && this.Type != OrderType.InterBankTransferCash && this.Type != OrderType.InterBankTransferNonCash
                    && this.Type != OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount && this.Type != OrderType.ReestrPaymentOrder)
                {
                    //Կրեդիտ հաշվի ստուգում
                    result.Errors.AddRange(Validation.ValidateReceiverAccount(this));

                    if (this.Type == OrderType.RATransfer && this.SubType == 1 || this.Type == OrderType.InBankConvertation)
                    {
                        ulong reciverCustomerNumber = this.ReceiverAccount.GetAccountCustomerNumber();
                        if (reciverCustomerNumber == this.CustomerNumber)
                        {
                            if (this.Type == OrderType.InBankConvertation)
                            {
                                //Տվյալ փոխանցումը անհրաժեշտ է կատարել Փոխարկում բաժնի միջոցով
                                result.Errors.Add(new ActionError(748));
                            }
                            else
                            {
                                //Տվյալ փոխանցումը անհրաժեշտ է կատարել Սեփական հաշիվների մեջ բաժնի միջոցով:
                                result.Errors.Add(new ActionError(747));
                            }
                        }

                    }

                    if ((this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer) && this.SubType == 2)
                    {
                        if (this.ReceiverAccount.AccountNumber.Length == 5)
                        {
                            if (String.IsNullOrEmpty(this.CreditCode))
                            {
                                //Վարկային/կրեդիտ կոդը լրացված չէ
                                result.Errors.Add(new ActionError(1103));
                            }
                        }
                        else if (this.ReceiverAccount.AccountNumber.Length < 12)
                        {
                            //Ստացողի հաշվեհամարի նվազագուն երկարությունը 12 է
                            result.Errors.Add(new ActionError(1109));
                        }
                        else if (this.ReceiverAccount.AccountNumber.Length > 12)
                        {
                            if (this.ReceiverAccount.AccountNumber.Substring(12, 1) == "L")
                            {
                                //Կրեդիտային հաշիվը սխալ է մուտքագրված:
                                result.Errors.Add(new ActionError(6));
                            }
                        }
                        if (!String.IsNullOrEmpty(this.CreditCode))
                        {


                            if (this.CreditCode.Length != 16)
                            {
                                //Վարկային/կրեդիտ կոդը սխալ է
                                result.Errors.Add(new ActionError(1106));
                            }
                            else if (this.CreditCode.Substring(12, 1) != "L")
                            {
                                //Վարկային/կրեդիտ կոդը սխալ է
                                result.Errors.Add(new ActionError(1106));
                            }
                            else if (!Utility.CheckAccountNumberControlDigit(this.CreditCode.Substring(0, 12)))
                            {
                                //Վարկային/կրեդիտ կոդի ստուգիչ նիշը սխալ է
                                result.Errors.Add(new ActionError(1107));
                            }
                            if (String.IsNullOrEmpty(this.Borrower))
                            {
                                //Վարկառուի անվանումը լրացված չէ
                                result.Errors.Add(new ActionError(1104));
                            }
                            else if (this.Borrower.Length > 31)
                            {
                                //Վարկառուի անվանում դաշտի առավելագույն երկարությունը 31 է
                                result.Errors.Add(new ActionError(1108));
                            }

                            if (String.IsNullOrEmpty(this.MatureType))
                            {
                                //Վարկի մարման տեսակը լրացված չէ
                                result.Errors.Add(new ActionError(1105));
                            }
                        }
                    }

                    if (ForThirdPerson)
                    {
                        if (String.IsNullOrEmpty(this.CreditorDescription))
                        {//§Պարտատիրոջ¦ անուն ազգանուն¦ դաշտը լրացված չէ
                            result.Errors.Add(new ActionError(400));
                        }

                        if (String.IsNullOrEmpty(this.CreditorDocumentNumber))
                        {//Պարտատիրոջ ՀԾՀ-ն լրացված չէ
                            result.Errors.Add(new ActionError(403));
                        }
                    }

                    //Հօգուտ 3-րդ անձի փոխանցում
                    if (ForThirdPerson && CreditorStatus == 0)
                    {
                        result.Errors.Add(new ActionError(408));
                    }

                    else
                    {
                        if (CreditorStatus != 0 || ForThirdPerson)
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



                }

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

                //Փոխանցում սեփական հաշիվների մեջ կամ բանկի ներսում կամ փոխարկում
                if (this.Type == OrderType.Convertation || (this.Type == OrderType.RATransfer && (this.SubType != 2 && this.SubType != 6 && this.SubType != 5)))
                {
                    string debitAccountCurrency = Account.GetAccountCurrency(this.DebitAccount.AccountNumber);
                    string creditAccountCurrency = Account.GetAccountCurrency(this.ReceiverAccount.AccountNumber);


                    if (this.Type == OrderType.Convertation)
                    {
                        if (debitAccountCurrency == creditAccountCurrency)
                        {
                            //Ընտրված են նույն արժույթներով հաշիվներ:
                            result.Errors.Add(new ActionError(56));
                        }

                        if (this.ConvertationRate < 0)
                        {
                            //Սխալ փոխարժեք:
                            result.Errors.Add(new ActionError(58));
                        }
                    }
                    else
                    {
                        if (debitAccountCurrency != creditAccountCurrency)
                        {
                            //Տարբեր արժույթներով հաշիվների միջև փոխանցումն անհրաժեշտ է կատարել «Փոխարկում» բաժնի միջոցով:
                            result.Errors.Add(new ActionError(328));
                        }
                    }

                }
            }


            if (this.Type != OrderType.CashCredit && this.Type != OrderType.CashConvertation
                && this.Type != OrderType.TransitCashOutCurrencyExchangeOrder
                && this.Type != OrderType.TransitCashOut
                && this.Type != OrderType.CashCreditConvertation
                && !this.ValidateForTransit
                && this.Type != OrderType.InterBankTransferCash
                && this.Type != OrderType.InterBankTransferNonCash
                && this.Type != OrderType.ReestrTransferOrder
                && this.Type != OrderType.ReestrPaymentOrder
                && this.Type != OrderType.CashOutFromTransitAccountsOrder)
            {
                if (this.DebitAccount?.IsAttachedCard != true)
                {
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
            if ((this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer) && this.SubType != 3)
            {

                Customer customer = new Customer();

                if (customer.GetPaymentOrderFee(this) == -1)
                {
                    //Սակագին նախատեսված չէ:Ստուգեք փոխանցման տվյալները
                    result.Errors.Add(new ActionError(659));
                }
                if (this.Currency != "AMD" && this.UrgentSign == true)
                {
                    //Շտապ փոխանցումներ իրականացնել հնարավոր է միայն ՀՀ դրամով
                    result.Errors.Add(new ActionError(503));
                }
                else if (templateType != TemplateType.CreatedByCustomer && ((this.Currency != "AMD" && this.TransferFee != 0) || (this.Currency == "AMD" && this.TransferFee != 0) || (this.Currency == "AMD" && this.UrgentSign == true && this.TransferFee != 0)) && this.Type != OrderType.CashForRATransfer)
                {
                    //Երե պահանջվում է միջնորդավճարի հաշիվ
                    result.Errors.AddRange(Validation.ValidateFeeAccount(this.CustomerNumber, this.FeeAccount));
                }
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
                    if (this.ReceiverBankCode.ToString().Substring(0, 1) == "9")
                    {
                        //Բյուջետային հաշվին փոխ1`` անցումը անհրաժեշտ է կատարել Բյուջե բաժնի միջոցով
                        result.Errors.Add(new ActionError(763));
                    }
                    else
                    {
                        if (!Validation.CheckReciverBankStatus(this.ReceiverBankCode))
                        {
                            //Ստացողի բանկը փակ է:
                            result.Errors.Add(new ActionError(786));
                        }
                    }

                }
            }
            if (Source != SourceType.AcbaOnline && Source != SourceType.AcbaOnlineXML && Source != SourceType.ArmSoft && Source != SourceType.MobileBanking)
            {
                if (Account.IsUserAccounts(user.userCustomerNumber, this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber))
                {
                    //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                    result.Errors.Add(new ActionError(544));
                }
            }


            byte customerType = 0;
            if (this.CustomerNumber != 0)
            {
                customerType = (byte)ACBAOperationService.GetCustomerType(this.CustomerNumber);
            }
            else
            {
                customerType = 6;
            }

            if (this.Type == OrderType.CashConvertation && customerType != (short)CustomerTypes.physical)
            {
                //Տվյալ գործարքը կարելի է կատարել միայն ֆիզ. անձանց համար
                result.Errors.Add(new ActionError(726));

            }

            if (this.ReceiverAccount.AccountNumber == Constants.LEASING_ACCOUNT_NUMBER && this.AdditionalParametrs == null && this.Source == SourceType.Bank)
            {
                //Ընտրված է լիզինգային հաշիվ սակայն վարկի տվյալները լրացված չէ
                result.Errors.Add(new ActionError(790));
            }


            if (this.Type == OrderType.CardServiceFeePayment || this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount)
            {
                ulong productId;


                if (this.AdditionalParametrs == null || this.AdditionalParametrs.Count == 0)
                {
                    //Քարտը գտնված չէ
                    result.Errors.Add(new ActionError(534));
                }
                else
                {
                    productId = Convert.ToUInt64(this.AdditionalParametrs.Find(m => m.Id == Constants.CARD_SERVICE_FEE_PAYMENT_ADDITIONAL_ID).AdditionValue);
                    Card card = Card.GetCardWithOutBallance(productId);


                    if (card == null)
                    {
                        //Քարտը գտնված չէ
                        result.Errors.Add(new ActionError(534));
                    }
                    else
                    {
                        CardServiceFee cardServiceFee = Card.GetCardServiceFee((ulong)card.ProductId);
                        DateTime operDay = Utility.GetCurrentOperDay();
                        if (cardServiceFee.NextDayOfServiceFeePayment > operDay && cardServiceFee.Debt == 0)
                        {
                            //Սպասարկման վարձի գանձման ա/թ մեծ է գործառնական օրվա ա/թ-ից
                            result.Errors.Add(new ActionError(816));
                        }
                        if (cardServiceFee.Debt != 0 && cardServiceFee.Debt < this.Amount && cardServiceFee.NextDayOfServiceFeePayment > operDay)
                        {
                            //Գանձվող գումարը մեծ է պարտքի գումարից
                            result.Errors.Add(new ActionError(817));
                        }

                        if (Utility.RoundAmount(cardServiceFee.ServiceFeePayed + this.Amount, "AMD") > cardServiceFee.ServiceFeeTotal)
                        {
                            //Տվյալ գանձումը կգերազանցի ամբողջ սպասարկման վարձի պարտավորությունը՝ հաշվի առած ընդհանուր գանձված սպասարկման վարձի գումարը
                            result.Errors.Add(new ActionError(818));
                        }

                        if (this.DebitAccount.Currency != "AMD")
                        {
                            //Տվյալ գործարքը կարելի է կատարել միայն AMD հաշվից:
                            result.Errors.Add(new ActionError(669));
                        }


                    }



                }
            }

            if (this.Type == OrderType.ReestrTransferOrder || this.Type == OrderType.ReestrPaymentOrder)
            {
                ReestrTransferOrder reestrTransferOrder = (ReestrTransferOrder)this;
                int index = 1;

                if (this.Type != OrderType.ReestrPaymentOrder)
                {
                    string currency = Validation.CashOperationCurrency(this);
                    decimal cashLimit = this.user.CashLimit(currency);
                    decimal cashRest = this.user.CashRest(currency);

                    if ((decimal)this.Amount + cashRest > cashLimit)
                    {
                        //Գործարքի գումարը գերազանցում է դրամարկղի մնացորդի սահմանաչափը:
                        result.Errors.Add(new ActionError(1094));
                    }

                }
                else
                {
                    if (this.DebitAccount?.Currency != "AMD")
                    {
                        //Տվյալ գործարքը կարելի է կատարել միայն AMD հաշվից:
                        result.Errors.Add(new ActionError(669));
                    }
                }
                if (reestrTransferOrder.ReestrTransferAdditionalDetails == null || reestrTransferOrder.ReestrTransferAdditionalDetails.Count == 0)
                {
                    //Մուտքագրեք փոխանցման տվյալները
                    result.Errors.Add(new ActionError(847));
                }
                else
                {
                    if (this.Type != OrderType.ReestrPaymentOrder && reestrTransferOrder.ReestrTransferAdditionalDetails.Count > 300)
                    {
                        if (this.Source == SourceType.Bank)
                        {
                            //Գործարքների քանակը մեծ է 300-ից:Առավելագույն գործարքների քանակը պետք է լինի 300:
                            result.Errors.Add(new ActionError(1266));
                        }


                    }
                    else
                    {


                        foreach (ReestrTransferAdditionalDetails details in reestrTransferOrder.ReestrTransferAdditionalDetails)
                        {
                            details.Index = index;
                            index++;
                            if (this.Type == OrderType.ReestrTransferOrder)
                            {
                                if (string.IsNullOrEmpty(details.Description))
                                {
                                    //{0} փոխանցման վճարման նպատակը մուտքագրված չէ
                                    result.Errors.Add(new ActionError(848, new string[] { "«" + details.Index.ToString() + "»" }));
                                }
                                else if (details.Description.Length > 130)
                                {
                                    //{0} փոխանցման «վճարման նպատակ» դաշտի առավելագույն նիշերի քանակն է 125
                                    result.Errors.Add(new ActionError(849, new string[] { "«" + details.Index.ToString() + "»" }));
                                }
                                else if (Utility.IsExistForbiddenCharacter(details.Description))
                                {
                                    //{0} փոխանցման «Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                                    result.Errors.Add(new ActionError(850, new string[] { "«" + details.Index.ToString() + "»" }));
                                }
                            }
                            else if (this.Type == OrderType.ReestrPaymentOrder)
                            {


                                result.Errors.AddRange(base.ValidateOrderDescription());
                                if (string.IsNullOrEmpty(details.Description))
                                {
                                    //{0} տողի ստացողը բացակայում է:
                                    result.Errors.Add(new ActionError(1013, new string[] { "«" + details.Index.ToString() + "»" }));
                                }

                                if (details.CreditAccount == null || details.CreditAccount.AccountNumber == "0")
                                {
                                    // {0} հաշիվը բացակայում է
                                    result.Errors.Add(new ActionError(891, new string[] { "«" + details.Index.ToString() + "» տողի" }));
                                }
                                else
                                {
                                    if (details.CreditAccount.AccountNumber == this.DebitAccount.AccountNumber)
                                    {
                                        // {0} տողի կրեդիտ հաշիվը և դեբետ հաշիվը նույնն են
                                        result.Errors.Add(new ActionError(971, new string[] { "«" + details.Index.ToString() + "»" }));
                                    }
                                    else
                                    {

                                        if (details.CreditAccount.IsCardAccount())
                                        {

                                            Card card = Card.GetCardWithOutBallance(details.CreditAccount.AccountNumber);
                                            if (card == null)
                                            {
                                                // {0} տողի քարտը գտնված չէ:
                                                result.Errors.Add(new ActionError(969, new string[] { "«" + details.Index.ToString() + "»" }));
                                            }
                                            else if (Account.CheckAccountIsClosed(details.CreditAccount.AccountNumber))
                                            {
                                                // {0} տողի քարտը փակ է:
                                                result.Errors.Add(new ActionError(970, new string[] { "«" + details.Index.ToString() + "»" }));
                                            }
                                            //else if (card.Currency != this.DebitAccount.Currency)
                                            //{
                                            //    // {0} տողի կրեդիտ հաշվի արժույթը` {1} է,իսկ գործարքի արժույթը ՝ {2}:
                                            //    result.Errors.Add(new ActionError(973, new string[] { "«" + details.Index.ToString() + "»", card.Currency, this.DebitAccount.Currency }));
                                            //}

                                        }

                                        ulong customerNumber = details.CreditAccount.GetAccountCustomerNumber();
                                        bool hasDahk = Validation.IsDAHKAvailability(customerNumber);
                                        if (hasDahk && details.PaymentType == 0)
                                        {
                                            // {0} տողի արգելադրման նպատակը ընտրված չէ:
                                            result.Errors.Add(new ActionError(1519, new string[] { "«" + details.Index.ToString() + "»" }));
                                        }


                                        //***Diana 1548   Save Validate
                                        if (details.CreditAccount != null && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft))
                                        {
                                            if (Convert.ToUInt32(details.CreditAccount.AccountNumber.Substring(0, 5)) >= 22000
                                            && Convert.ToUInt32(details.CreditAccount.AccountNumber.Substring(0, 5)) <= 22300)
                                            {
                                                Account account;
                                                account = Account.GetAccount(details.CreditAccount.AccountNumber);

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


                                }



                            }

                            if (details.Amount < 0.01)
                            {
                                //{0} փոխանցման մուտքագրված գումարը սխալ է
                                result.Errors.Add(new ActionError(851, new string[] { "«" + details.Index.ToString() + "»" }));
                            }
                            else if (!Utility.IsCorrectAmount(details.Amount, this.Currency))
                            {
                                //{0} փոխանցման մուտքագրված գումարը սխալ է
                                result.Errors.Add(new ActionError(851, new string[] { "«" + details.Index.ToString() + "»" }));
                            }

                        }
                        if (this.Type == OrderType.ReestrPaymentOrder)
                        {
                            List<int> nonValidateRows = ValidationDB.CheckReestrPaymentOrderReciverName(reestrTransferOrder.ReestrTransferAdditionalDetails);
                            if (nonValidateRows.Count > 0)
                            {
                                string nonValidateRowsString = "";
                                nonValidateRows.ForEach(m =>
                                    nonValidateRowsString = nonValidateRowsString + "«" + m.ToString() + "»,"
                                    );
                                nonValidateRowsString = nonValidateRowsString.Substring(0, nonValidateRowsString.Length - 1);
                                result.Errors.Add(new ActionError(169, new string[] { nonValidateRowsString }));
                            }
                        }

                        if (Math.Round(reestrTransferOrder.ReestrTransferAdditionalDetails.Sum(m => m.Amount), 2) != reestrTransferOrder.Amount)
                        {
                            //Փոխանցման տվյալների գումարը չի համապասխանում գործարքի գումարին
                            result.Errors.Add(new ActionError(853));
                        }
                        if (IsReestrOrderDonePartially())
                        {
                            //Գոյություն ունի Կատարված է մասնակի կարգավիճակով հայտ,սկզբում անհրաժեշտ է կատարել տվյալ գործարքը։
                            result.Errors.Add(new ActionError(1458));
                        }

                    }
                }

            }

            if (this.SubType == 4 && this.Type == OrderType.RATransfer)
            {
                CreditLine creditLine = CreditLine.GetCreditLine(this.DebitAccount.AccountNumber);
                if (this.ReceiverAccount.AccountNumber != creditLine.ConnectAccount.AccountNumber)
                {
                    //Սխալ կրեդիտ հաշիվ:(Օվերդրաֆտ/Առևտրային վարկային գիծ)-ից փոխանցում հնարավոր է կատարել միայն պրոդուկտին կցված հաշվին'
                    result.Errors.Add(new ActionError(868));
                }
            }



            if (this.SubType == 2 && this.Type == OrderType.CashForRATransfer)
            {
                if (this.Currency != "AMD")
                {
                    if ((this.Currency == "USD" && this.Amount > 3000) ||
                                                               (this.Currency == "EUR" && (this.Amount * Utility.GetLastCBExchangeRate(this.Currency)) / Utility.GetLastCBExchangeRate("USD") > 3000))
                    {
                        //Կանխիկ փոխանցման գումարը չի կարող գերազանցել 3000USD
                        result.Errors.Add(new ActionError(713));
                    }
                }
                if (this.Currency == "AMD" && this.Amount > 5000000)
                {
                    //Առանց հաշիվ բացելու թույլատրվում է փոխանցել մինչև 5 000 000 (ներառյալ) ՀՀ դրամ
                    result.Errors.Add(new ActionError(1385));
                }
            }

            if (this.CustomerNumber != 0 && (this.Type == OrderType.CashForRATransfer || this.Type == OrderType.CashConvertation || this.Type == OrderType.InterBankTransferCash))
            {
                result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber));

            }

            if (this.Type == OrderType.InterBankTransferCash || this.Type == OrderType.InterBankTransferNonCash)
            {
                if (this.AdditionalParametrs == null || !this.AdditionalParametrs.Exists(m => m.AdditionTypeDescription == "InterBankTransfer"))
                {
                    //Ստացողի տեսակ դաշտը ընտրված չէ:
                    result.Errors.Add(new ActionError(974));
                }
                else if (string.IsNullOrEmpty(this.AdditionalParametrs.Find(m => m.AdditionTypeDescription == "InterBankTransfer").AdditionValue))
                {
                    //Ստացողի տեսակ դաշտը ընտրված չէ:
                    result.Errors.Add(new ActionError(974));
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
            if ((this.ValidateForCash || this.Type == OrderType.ReestrTransferOrder) && this.Type != OrderType.CashConvertation && this.Type != OrderType.CashCreditConvertation)
            {
                if (this.Currency == "GEL")
                {
                    //{0} արժույթով գործարք կատարել հնարավոր չէ:
                    result.Errors.Add(new ActionError(1478, new string[] { this.Currency }));
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


                ulong creditAccountCustomerNumber = this.ReceiverAccount.GetAccountCustomerNumber();

                if (this.CustomerNumber != creditAccountCustomerNumber)
                {
                    //Վարկային գծի միջոցներից հնարավոր է փոխանցում կատարել միայն սեփական հաշիվների միջև:
                    result.Errors.Add(new ActionError(1116));
                }
                else if (this.ReceiverAccount.AccountType != 10)
                {

                    //Վարկային գծի միջոցներից հնարավոր է փոխանցում կատարել միայն ընթացիք հաշվին
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

            if (this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer)
            {
                if (this.ReceiverAccount.AccountNumber == "103003240159" || this.ReceiverAccount.AccountNumber == "103003249150"
                    || this.ReceiverAccount.AccountNumber == "103003241157" || this.ReceiverAccount.AccountNumber == "103003635671")
                {
                    switch (this.ReceiverAccount.AccountNumber)
                    {
                        case "103003240159":
                            if (this.DebitAccount.Currency != "AMD")
                                result.Errors.Add(new ActionError(1824));
                            break;
                        case "103003241157":
                            if (this.DebitAccount.Currency != "USD")
                                result.Errors.Add(new ActionError(1825));
                            break;
                        case "103003249150":
                            if (this.DebitAccount.Currency != "EUR")
                                result.Errors.Add(new ActionError(1826));
                            break;
                        case "103003635671":
                            if (this.DebitAccount.Currency != "GBP")
                                result.Errors.Add(new ActionError(1827));
                            break;
                        default:
                            break;
                    }
                }

            }


            if (this.Source == SourceType.STAK && SendDillingConfirm == true)
            {
                // Տվյալ գործարքից հետո հաճախորդի՝ մեկ օրվա ընթացքում կատարած փոխարժեքի առքի լիմիթի սահմանը գերազանցում է բանկում սահմանաված սահմանը։  
                // Գործարքը մերժված է, քանի որ տվյալ գործարքից հետո կգերազանցվի բանկում սահմանաված՝ մեկ օրվա ընթացքում հաճախորդի համար թույլատրելի փոխարժեքի առքի լիմիթի սահմանը։  
                result.Errors.Add(new ActionError(25));
            }


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            if (Source == SourceType.SSTerminal && base.IsPaymentIdUnique())
            {
                result.Errors.Add(new ActionError(1498));
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }



            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգում բանկ ուղարկելուց
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();


            if (this.OnlySaveAndApprove == false)
            {
                string developerSpecialAccountsNonCash = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccountsNonCash"];
                string developerSpecialAccounts = user?.AdvancedOptions?["checkAccessToDeveloperSpecialAccounts"];

                if (Account.CheckAccessToThisAccounts(this.ReceiverAccount?.AccountNumber) == 118 && this.DebitAccount?.AccountNumber != "220004130948000"
                    || (!(this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation) && this.ReceiverAccount?.AccountType == 119))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }

                if ((this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation)
                    && this.ReceiverAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
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

                if ((this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3) || this.Type == OrderType.Convertation || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.InBankConvertation)
                    && this.DebitAccount?.AccountType == 119 && !(developerSpecialAccounts == "1" || developerSpecialAccountsNonCash == "1"))
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1966));
                    return result;
                }

                if ((this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation)
                    && this.DebitAccount?.AccountType == 119 && developerSpecialAccountsNonCash == "1")
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


                if (this.Type == OrderType.RATransfer)
                {
                    ///Մուտքային հաշվի ստուգում:
                    string recAcc = this.ReceiverAccount.AccountNumber.ToString();

                    if (this.SubType == 1 && (recAcc.Length != 15 ||
                                              !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12))
                                             ))
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
                    else if (this.SubType == 2 && !(recAcc.Length == 5 && this.Type == OrderType.RATransfer) && (recAcc.Length < 12 || recAcc.Length > 16 || !Utility.CheckAccountNumberControlDigit(recAcc.Substring(0, 12))))
                    {
                        //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                        result.Errors.Add(new ActionError(19));
                    }

                    if ((this.SubType == 1 || this.SubType == 3) && this.ReceiverAccount.AccountType == 13 && this.ReceiverAccount.TypeOfAccount == 24)
                    {
                        DateTime depositEnd = GetNormalEndOfDeposit(this.ReceiverAccount.ProductNumber);

                        DateTime currentOperDay = Utility.GetNextOperDay().Date;
                        if (currentOperDay.AddMonths(3) > depositEnd)
                        {
                            //Տվյալ ավանդի գումարը ավելացնել հնարավոր չէ (ըստ սակագնի):
                            result.Errors.Add(new ActionError(1069));
                            return result;
                        }
                    }

                    if (this.SubType != 3)
                    {
                        if (Validation.HasOverdueLoan(this.DebitAccount, 1, 1))
                        {
                            //Գոյություն ունի ժամկետանց/դուրսգրված պրոդուկտ
                            result.Errors.Add(new ActionError(1247));
                            return result;
                        }
                    }

                    if (this.SubType == 2 || this.SubType == 5)
                    {
                        if (this.Currency != "AMD" && this.Currency != "USD" && this.Currency != "EUR")
                        {
                            //Փոխանցման արժույթը սխալ է։
                            result.Errors.Add(new ActionError(1425));
                        }

                        double fee = AccountingTools.GetFeeForLocalTransfer(this);
                        if (fee > 0)
                        {
                            if (this.Fees.Find(m => m.Type == 20 && m.Amount == fee) == null)
                            {
                                //Միջնորդավճարի գումարը սխալ է:
                                result.Errors.Add(new ActionError(1423));
                            }
                        }
                        if (this.ReceiverAccount != null)
                        {
                            if ((this.ReceiverBankCode == 10300 && this.ReceiverAccount.AccountNumber.ToString()[5] == '9') || this.ReceiverBankCode.ToString()[0] == '9')
                            {
                                BudgetPaymentOrder budgetPaymentOrder = new BudgetPaymentOrder();
                                budgetPaymentOrder.Id = this.Id;
                                budgetPaymentOrder.Get();
                                if (budgetPaymentOrder.LTACode == 0)
                                {
                                    //ՏՀՏ կոդը լրացված չէ: Հարկային և մաքսային ծառայություններ փոխանցումներ կատարելու համար անհրաժեշտ է պարտադիր նշել տեսչության կոդը: 
                                    //Այլ փոխանցումների դեպքում պետք է լրացնել «99 Այլ»:
                                    result.Errors.Add(new ActionError(356));
                                }
                                string TIN = "";
                                short customerType = 0;
                                int creditorCustomerType = 0;
                                if (this.CustomerNumber != 0)
                                {
                                    var customer = ACBAOperationService.GetCustomer(this.CustomerNumber);
                                    if (customer.customerType.key != (short)CustomerTypes.physical)
                                    {
                                        LegalCustomer legalCustomer = (LegalCustomer)customer;
                                        TIN = legalCustomer.CodeOfTax;
                                    }

                                    customerType = customer.customerType.key;
                                }
                                else
                                {
                                    customerType = (short)CustomerTypes.physical;
                                }

                                if (this.CreditorStatus == 10 || this.CreditorStatus == 20)
                                    creditorCustomerType = 6;
                                else if (this.CreditorStatus == 13 || this.CreditorStatus == 23)
                                    creditorCustomerType = 2;
                                else if (this.CreditorStatus != 10 && this.CreditorStatus != 20 && this.CreditorStatus != 0)
                                    creditorCustomerType = 1;

                                short transferVerification = (short)PaymentOrderDB.CheckTransferVerification(this.ReceiverBankCode == 10300 ? Convert.ToDouble(this.ReceiverAccount.AccountNumber.ToString().Substring(5))
                                    : Convert.ToDouble(this.ReceiverAccount.AccountNumber), budgetPaymentOrder.LTACode, customerType, TIN, creditorCustomerType, this.CreditorDocumentType == 4 ? this.CreditorDocumentNumber : "");
                                if (transferVerification != 1)
                                {
                                    result.Errors.Add(new ActionError(transferVerification));
                                }
                                if (ValidationDB.CheckBudjetRegCode(this.Id))
                                {
                                    if (!ValidationDB.CheckBudgetCodeExistance(this.DebitAccount.AccountNumber, customerType))
                                    {
                                        //Բյուջետային հաշիվը նախատեսված չէ նշված վճարողի տեսակի համար
                                        result.Errors.Add(new ActionError(383));
                                    }
                                }
                            }
                        }

                    }

                    if (this.UrgentSign)
                    {
                        //Տվյալ տեսակի գործարքը չի կարող ընդունվել շտապ պայմանով:
                        result.Errors.Add(new ActionError(1536));
                    }


                    if ((this.SubType == 1 || this.SubType == 2 || this.SubType == 3 || this.SubType == 5 || this.SubType == 6) && this.Currency == "AMD")
                    {
                        if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
                        {
                            //Մուտքագրված գումարը սխալ է:
                            result.Errors.Add(new ActionError(25));
                        }
                    }
                }
                if ((this.Type == OrderType.RATransfer && (this.SubType == 3 || this.SubType == 1)) || (this.Type == OrderType.Convertation) || (this.Type == OrderType.ReceivedFastTransferPaymentOrder))
                {
                    if (this.ReceiverAccount != null)
                    {
                        Account account;

                        account = Account.GetAccount(this.ReceiverAccount.AccountNumber);
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


            if (this.Source != SourceType.Bank && this.Source != SourceType.SSTerminal && this.Source != SourceType.CashInTerminal)
            {
                result.Errors.AddRange(Validation.ValidateRate(this));
            }


            if (this.Type != OrderType.CashDebit && this.Type != OrderType.CashConvertation
                && this.Type != OrderType.CashDebitConvertation && this.Type != OrderType.InterBankTransferCash
                && this.Type != OrderType.CashForRATransfer && this.SubType != 4 && !this.ValidateForTransit
                && this.Type != OrderType.ReestrTransferOrder
                && this.Type != OrderType.SSTerminalCashInOrder
                && this.Type != OrderType.SSTerminalCashOutOrder
                && this.Type != OrderType.CashOutFromTransitAccountsOrder || this.Type == OrderType.CardServiceFeePayment)
            {
                if ((this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder
                        || this.Type == OrderType.TransitCashOut || this.Type == OrderType.TransitNonCashOut) && this.TransferID != 0)
                {
                    Transfer transfer = new Transfer();
                    transfer.Id = this.TransferID;
                    transfer.Get();
                    if (transfer.InstantMoneyTransfer != 1)
                        result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));
                }
                else
                    result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));
            }
            else if (this.Type == OrderType.CashForRATransfer || this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount
                || this.Type == OrderType.InterBankTransferCash || this.Type == OrderType.CashOutFromTransitAccountsOrder)
            {
                //Տարանցիկ հաշվի մնացորդի ստուգումներ:

                double debitAccountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);
                if (OnlySaveAndApprove)
                {
                    debitAccountBalance += this.DebitAccount.Currency == "AMD" ? GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.TransitPaymentOrder) : GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.TransitPaymentOrder, false);
                }

                if (this.DebitAccount.Currency == "AMD")
                {
                    double feeAmount = 0;
                    if (this.Fees != null)
                    {
                        if (this.Fees.FindAll(m => m.Currency == "AMD").Count > 0)
                            feeAmount = this.Fees.FindAll(m => m.Currency == "AMD").Sum(m => m.Amount);
                    }



                    if ((debitAccountBalance < this.Amount + feeAmount) & this.Type != OrderType.CashOutFromTransitAccountsOrder)
                    {
                        result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", debitAccountBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency }));
                    }
                }
                else
                {
                    if (debitAccountBalance < this.Amount)
                    {
                        result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", debitAccountBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency }));
                    }


                    if (this.Type != OrderType.CashOutFromTransitAccountsOrder)
                    {
                        double feeAmount = 0;
                        if (this.Fees?.FindAll(m => m.Currency == "AMD").Count > 0)
                            feeAmount = this.Fees.FindAll(m => m.Currency == "AMD").Sum(m => m.Amount);

                        double feeAccountBalance = Account.GetAccountBalance(this.Fees?.Find(m => m.Currency == "AMD").Account.AccountNumber);

                        if (OnlySaveAndApprove)
                        {
                            feeAccountBalance += Order.GetSentNotConfirmedAmounts(this.Fees?.Find(m => m.Currency == "AMD").Account.AccountNumber, OrderType.TransitPaymentOrder);
                        }

                        if (feeAccountBalance < feeAmount)
                        {
                            result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", feeAccountBalance.ToString("#,0.00") + " " + this.Fees?.Find(m => m.Currency == "AMD").Account.Currency, this.Fees?.Find(m => m.Currency == "AMD").Account.Balance.ToString("#,0.00") + " " + this.Fees?.Find(m => m.Currency == "AMD").Account.Currency }));
                        }
                    }


                }
            }
            else if (this.Type == OrderType.CashDebit || this.Type == OrderType.ReestrTransferOrder || this.Type == OrderType.SSTerminalCashInOrder || this.Type == OrderType.CashDebitConvertation)
            {

                if (this.Fees != null && this.Fees.Count > 0)
                {
                    foreach (OrderFee fee in this.Fees)
                    {
                        if (fee.Type == 9 || fee.Type == 29 || ((Source == SourceType.AcbaOnline || Source == SourceType.AcbaOnlineXML || Source == SourceType.ArmSoft) && fee.Type == 20))
                        {
                            double feeAmount = this.Fees.FindAll(m => m.Account.AccountNumber == fee.Account.AccountNumber).Sum(m => m.Amount);
                            double feeAccountBalance = Account.GetAcccountAvailableBalance(fee.Account.AccountNumber);

                            if (feeAccountBalance < feeAmount)
                            {

                                if (Account.AccountAccessible(fee.Account.AccountNumber, user.AccountGroup))
                                {
                                    //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                                    result.Errors.Add(new ActionError(499, new string[] { fee.Account.AccountNumber, feeAccountBalance.ToString("#,0.00") + " " + this.Fees.Find(m => m.Currency == "AMD").Account.Currency, this.Fees.Find(m => m.Currency == "AMD").Account.Balance.ToString("#,0.00") + " " + this.Fees.Find(m => m.Currency == "AMD").Account.Currency }));
                                }
                                else
                                {
                                    //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                                    result.Errors.Add(new ActionError(788, new string[] { fee.Account.AccountNumber }));
                                }
                            }
                        }

                    }
                }

                //***Diana 1548 
                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                {
                    if (this.Type == OrderType.ReestrTransferOrder || this.Type == OrderType.ReestrPaymentOrder)
                    {
                        ReestrTransferOrder reestrTransferOrder = (ReestrTransferOrder)this;

                        foreach (ReestrTransferAdditionalDetails details in reestrTransferOrder.ReestrTransferAdditionalDetails)
                        {

                            if (this.Type == OrderType.ReestrPaymentOrder)
                            {

                                if (details.CreditAccount != null)
                                {

                                    if (Convert.ToUInt32(details.CreditAccount.AccountNumber.Substring(0, 5)) >= 22000
                                    && Convert.ToUInt32(details.CreditAccount.AccountNumber.Substring(0, 5)) <= 22300)
                                    {
                                        Account account;
                                        account = Account.GetAccount(details.CreditAccount.AccountNumber);

                                        if (Account.IsAccountForbiddenForTransfer(account))
                                        {
                                            //...հաշիվը ժամանակավոր է, խնդրում ենք դիմել Բանկ
                                            result.Errors.Add(new ActionError(1548, new string[] { account.AccountNumber.ToString() }));
                                        }
                                    }
                                }
                            }
                        }

                    }
                }



            }
            else if (this.SubType == 4 && this.Type == OrderType.RATransfer)
            {
                CreditLine creditLine = CreditLine.GetCreditLine(this.DebitAccount.AccountNumber);
                double debitAccountBalance = creditLine.StartCapital - ((creditLine.CurrentCapital * -1) + creditLine.OutCapital);
                if (debitAccountBalance < this.Amount)
                {


                    if (Account.AccountAccessible(this.DebitAccount.AccountNumber, user.AccountGroup))
                    {
                        //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                        result.Errors.Add(new ActionError(499, new string[] { this.DebitAccount.AccountNumber, debitAccountBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency }));
                    }
                    else
                    {
                        //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                        result.Errors.Add(new ActionError(788, new string[] { this.DebitAccount.AccountNumber }));
                    }
                }
            }

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.ArmSoft || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML)
            {
                if (this.Type == OrderType.RATransfer && this.SubType == 1 && !Validation.IsBankOpen(this.ReceiverBankCode))
                {
                    result.Errors.Add(new ActionError(739));
                }
            }

            if (this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer)
            {
                if (this.ReceiverAccount.AccountNumber == "103003240159" || this.ReceiverAccount.AccountNumber == "103003249150"
                    || this.ReceiverAccount.AccountNumber == "103003241157" || this.ReceiverAccount.AccountNumber == "103003635671")
                {
                    switch (this.ReceiverAccount.AccountNumber)
                    {
                        case "103003240159":
                            if (this.DebitAccount.Currency != "AMD")
                                result.Errors.Add(new ActionError(1824));
                            break;
                        case "103003241157":
                            if (this.DebitAccount.Currency != "USD")
                                result.Errors.Add(new ActionError(1825));
                            break;
                        case "103003249150":
                            if (this.DebitAccount.Currency != "EUR")
                                result.Errors.Add(new ActionError(1826));
                            break;
                        case "103003635671":
                            if (this.DebitAccount.Currency != "GBP")
                                result.Errors.Add(new ActionError(1827));
                            break;
                        default:
                            break;
                    }
                }

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


        public PaymentOrderFutureBalance GetFutureBalance()
        {
            PaymentOrderFutureBalance futureBalance = new PaymentOrderFutureBalance();
            futureBalance.GetPaymentOrderFutureBalance(this);
            return futureBalance;
        }

        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        internal void Complete()
        {
            bool IsAttachedCard = default(bool);    // = this.DebitAccount.IsAttachedCard;

            if (this.Source != SourceType.STAK)
            {
                IsAttachedCard = this.DebitAccount.IsAttachedCard;
            }

            Account creditAccount;
            Account debitAccount;

            if (this.Source != SourceType.Bank)
                this.RegistrationDate = DateTime.Now.Date;
            if (this.Source != SourceType.SSTerminal && Source != SourceType.CashInTerminal)
            {
                if (this.DebitAccount != null && ((!this.ValidateForTransit && this.ValidateDebitAccount) || this.Type == OrderType.CardServiceFeePayment))
                {
                    this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
                }
                else
                {
                    if ((this.Type == OrderType.TransitCashOutCurrencyExchangeOrder || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder ||
                                this.Type == OrderType.TransitCashOut || this.Type == OrderType.TransitNonCashOut) && this.TransferID != 0)
                    {
                        Transfer transfer = new Transfer();
                        transfer.Id = this.TransferID;
                        transfer.Get();
                        if (transfer.InstantMoneyTransfer == 1 || transfer.TransferGroup == 5)
                            this.DebitAccount = Account.GetSystemAccount(transfer.DebitAccount.AccountNumber);
                        else if (transfer.TransferGroup == 4)
                            this.DebitAccount = Account.GetSystemAccount(transfer.CreditAccount.AccountNumber);
                        else
                            this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.DebitAccount.Currency, user.filialCode);
                    }
                    //ԱԳՍ-ով փոխանցումների դեպք
                    //Նշված դեպքում դրամարկղի հաշիվ պետք չէ որոշել
                    else if (this.DebitAccount != null && this.DebitAccount.Status == 7)
                    {
                        this.DebitAccount = Account.GetSystemAccount(this.DebitAccount.AccountNumber);
                    }
                    else
                        this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.DebitAccount.Currency, user.filialCode);
                }

                this.DebitAccount.IsAttachedCard = IsAttachedCard;//Babken Makaryan  for attached Card order

            }

            if (this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount)
            {
                this.Fees = new List<OrderFee>();
            }

            if (this.Type == OrderType.InterBankTransferCash)
            {
                if (this.Fees != null && this.Fees.Count > 0)
                {
                    this.Fees.ForEach(m =>
                    {
                        if (m.Type != 0)
                            m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", user.filialCode);
                    });
                }
            }


            //Կանխիկ ՀՀ տարածքում
            if (this.Type == OrderType.CashForRATransfer)
            {
                if (this.Fees != null)
                {
                    this.Fees.ForEach(m =>
                    {
                        m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                        m.CreditAccount = OperationAccountHelper.GetOperationSystemAccountForFee(this, m.Type);
                    });
                }
            }

            //Կանխիկ մուտք հաշվին կամ կանխիկ ելք հաշվից եթե միջնորդավճարը կանխիկ է
            if (this.Type == OrderType.CashDebit ||
                this.Type == OrderType.CashCredit ||
                this.Type == OrderType.ReestrTransferOrder ||
                this.Type == OrderType.CashOutFromTransitAccountsOrder ||
                this.Type == OrderType.CashDebitConvertation ||
                this.Type == OrderType.CashTransitCurrencyExchangeOrder)
            {
                if (this.Fees != null)
                {
                    foreach (OrderFee fee in this.Fees)
                    {
                        if (fee.Type == 8 || fee.Type == 1 || fee.Type == 3 || fee.Type == 5 || fee.Type == 6 || fee.Type == 28)
                        {
                            fee.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", user.filialCode);
                        }
                        fee.CreditAccount = OperationAccountHelper.GetOperationSystemAccountForFee(this, fee.Type);
                    }
                }
            }


            //Փոխանցում ՀՀ տարածքում ,որոշում ենք միջնորդավճարի կրեդիտ հաշիվը
            if (this.Type == OrderType.RATransfer)
            {
                if (this.Fees != null)
                {
                    foreach (OrderFee fee in this.Fees)
                    {
                        fee.CreditAccount = OperationAccountHelper.GetOperationSystemAccountForFee(this, fee.Type);
                    }
                }
            }

            if (this.Type == OrderType.CashCredit || this.Type == OrderType.CashCreditConvertation ||
                this.Type == OrderType.CashConvertation || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder
                || this.Type == OrderType.TransitCashOut || this.Type == OrderType.InterBankTransferCash
                || this.Type == OrderType.InterBankTransferNonCash || this.Type == OrderType.CashOutFromTransitAccountsOrder)
            {
                //Նշված դեպքում դրամարկղի հաշիվ պետք չէ որոշել
                if (this.ReceiverAccount != null && this.ReceiverAccount.Status == 7)
                {
                    this.ReceiverAccount = Account.GetSystemAccount(this.ReceiverAccount.AccountNumber);
                }
                else if (this.Source == SourceType.SSTerminal)
                {
                    //this.ReceiverAccount = SSTerminal.GetOperationSystemAccount(this.TerminalID, this.ReceiverAccount.Currency);
                }
                else
                {
                    this.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), this.ReceiverAccount.Currency, user.filialCode);
                }
            }
            else if (this.Type == OrderType.CardServiceFeePayment || this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount)
            {
                ulong cardAppId = Convert.ToUInt64(this.AdditionalParametrs.Find(m => m.Id == Constants.CARD_SERVICE_FEE_PAYMENT_ADDITIONAL_ID).AdditionValue);
                this.ReceiverAccount = Account.GetCardServiceFeeAccountNew(cardAppId);


            }
            else if (this.Type == OrderType.ReestrPaymentOrder)
            {
                this.ReceiverAccount = new Account();
            }

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            //Եթե փոխանցում է ՀՀ տարացքում,որոշում ենք փոխանցման ենթատեսակը
            if ((this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer) && this.SubType != 3 && this.ReceiverAccount.AccountNumber != "0")
            {
                if (this.ReceiverAccount.AccountNumber.Length > 5)
                {
                    ulong creditBankCode = ulong.Parse(this.ReceiverAccount.AccountNumber.ToString().Substring(0, 5));

                    if (creditBankCode >= 22000 && creditBankCode < 22300)
                        this.SubType = 1; // Փոխանցում բանկի ներսում
                    else if ((this.ReceiverBankCode == 10300 && this.ReceiverAccount.AccountNumber.ToString()[5] == '9') || this.ReceiverBankCode.ToString()[0] == '9')
                    {
                        this.SubType = 5; // Փոխանցում բյուջե


                        if (this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1)
                        {
                            this.SubType = 6;
                        }
                    }
                    else if (this.ReceiverBankCode == 10300 && this.ReceiverAccount.AccountNumber.ToString()[0] == '9' && (this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline))
                    {
                        this.SubType = 5; // Փոխանցում բյուջե


                        if (this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1)
                        {
                            this.SubType = 6;
                        }
                    }

                    else
                        this.SubType = 2; // Փոխանցում ՀՀ տարածքում

                }

            }

            if (this.Type == OrderType.RATransfer && (this.SubType == 3 || this.SubType == 4) && this.DebitAccount.AccountNumber != "0")
            {
                debitAccount = this.DebitAccount;
                if ((debitAccount.AccountType == 18 || debitAccount.AccountType == 60 || debitAccount.AccountType == 46 || debitAccount.AccountType == 5) && debitAccount.TypeOfAccount != 224 && debitAccount.TypeOfAccount != 279 && DebitAccount?.IsAttachedCard != true)
                {
                    if (string.IsNullOrEmpty(this.Description))
                        this.Description = "²é¨ïñ³ÛÇÝ í³ñÏ³ÛÇÝ ·ÍÇ ïñ³Ù³¹ñáõÙ";
                    this.SubType = 4;
                }
                else if (debitAccount.AccountType == 25 || debitAccount.AccountType == 3)
                {
                    if (string.IsNullOrEmpty(this.Description))
                        this.Description = "úí»ñ¹ñ³ýïÇ ïñ³Ù³¹ñáõÙ";
                    this.SubType = 4;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Description))
                        this.Description = "öáË³ÝóáõÙ Ñ³ßíÇÝ";
                    this.SubType = 3;
                }
            }

            if (this.Source != SourceType.Bank || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }

            Customer customer = new Customer(this.CustomerNumber, Languages.hy);
            customer.Source = Source;


            //Երե կատարվում է արտարժույթի փոխարկում, որոշում ենք ենթատեսակը
            if (this.ValidateForConvertation)
            {
                debitAccount = this.DebitAccount;
                if (this.Type == OrderType.Convertation || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                {
                    creditAccount = Account.GetAccount(this.ReceiverAccount.AccountNumber);
                    this.ReceiverAccount = creditAccount;
                }
                else
                {
                    creditAccount = this.ReceiverAccount;
                }

                if (debitAccount.Currency != "AMD")
                {
                    if (creditAccount.Currency != "AMD")
                    {

                        if (this.Type == OrderType.Convertation || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.CashCreditConvertation || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                        {
                            this.SubType = 3;//Արտարժույթի առք ու վաճառք
                            if (string.IsNullOrEmpty(this.Description))
                            {
                                this.Description = "²ñÅ. í³×." + " " + this.DebitAccount.Currency + "/" + this.ReceiverAccount.Currency + " " +
                                    this.OPPerson.PersonName + " " + this.OPPerson.PersonLastName + (this.OPPerson.CustomerNumber != 0 ? " (" + this.OPPerson.CustomerNumber.ToString() + ") " : "");
                                if (Description.Length >= 100)
                                    this.Description = "²ñÅ. í³×." + " " + this.DebitAccount.Currency + "/" + this.ReceiverAccount.Currency + " " +
                                    (this.OPPerson.CustomerNumber != 0 ? " (" + this.OPPerson.CustomerNumber.ToString() + ") " : "");
                            }

                        }

                        else
                        {
                            this.SubType = 3;//Արտարժույթի առք ու վաճառք
                            if (string.IsNullOrEmpty(this.Description))
                                this.Description = "²ñÅ. í³×." + " " + this.DebitAccount.Currency + "/" + this.ReceiverAccount.Currency;
                        }
                    }

                    else
                    {
                        if (this.Type == OrderType.Convertation || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.CashCreditConvertation || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                        {
                            this.SubType = 1; //Արտարժույթի վաճառք
                            if (string.IsNullOrEmpty(this.Description))
                            {
                                this.Description = "²ñÅ. ¶ÝáõÙ" + " " + this.DebitAccount.Currency + " " +
                                                   this.OPPerson.PersonName + " " + this.OPPerson.PersonLastName + " (" + this.OPPerson.CustomerNumber.ToString() + ") ";
                                if (Description.Length >= 100)
                                    this.Description = "²ñÅ. ¶ÝáõÙ" + " " + this.DebitAccount.Currency + " " +
                                                   "(" + this.OPPerson.CustomerNumber.ToString() + ") ";

                            }
                        }
                        else
                        {
                            this.SubType = 1; //Արտարժույթի վաճառք
                            if (string.IsNullOrEmpty(this.Description))
                                this.Description = "²ñÅ. ¶ÝáõÙ" + " " + this.DebitAccount.Currency + "/" + this.ReceiverAccount.Currency;
                        }

                    }

                }
                else
                {
                    if (this.Type == OrderType.Convertation || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.CashCreditConvertation || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder)
                    {
                        if (string.IsNullOrEmpty(this.Description))
                        {
                            this.Description = "²ñÅ. í³×." + " " + this.ReceiverAccount.Currency + " " +
                                               this.OPPerson.PersonName + " " + this.OPPerson.PersonLastName + " (" + this.OPPerson.CustomerNumber.ToString() + ") ";
                            if (Description.Length >= 100)
                                this.Description = "²ñÅ. í³×." + " " + this.ReceiverAccount.Currency + " " +
                                               "(" + this.OPPerson.CustomerNumber.ToString() + ") ";
                        }

                        this.SubType = 2; //Արտարժույթի առք
                    }
                    else
                    {
                        this.SubType = 2; //Արտարժույթի առք
                        if (string.IsNullOrEmpty(this.Description))
                            this.Description = "²ñÅ. í³×." + " " + this.DebitAccount.Currency + "/" + this.ReceiverAccount.Currency;
                    }
                }
            }
            else if (this.Type == OrderType.RATransfer && this.SubType == 3 && (this.Source == SourceType.MobileBanking || Source == SourceType.AcbaOnlineXML || Source == SourceType.AcbaOnline || Source == SourceType.ArmSoft))
            {
                this.ReceiverAccount = Account.GetAccount(this.ReceiverAccount.AccountNumber);
            }

            if (!this.ValidateForConvertation && this.Type != OrderType.CashForRATransfer && this.Type != OrderType.CashDebit &&
                this.Type != OrderType.CashOutFromTransitAccountsOrder && this.Type != OrderType.TransitCashOut &&
                this.Type != OrderType.TransitNonCashOut && this.Type != OrderType.ReestrTransferOrder
                && this.Type != OrderType.InterBankTransferNonCash && this.Type != OrderType.InterBankTransferCash
                // MobileBanking-ի կամ AcbaOnline-ի դեպքում անհրաժեշտ է 
                //միշտ ստուգել առկա է միջնորդաճար թե ոչ
                || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline)
            {
                double transferFee = customer.GetPaymentOrderFee(this);
                this.TransferFee = transferFee;
                double cardFee = customer.GetCardFee(this);
                this.CardFee = cardFee;
            }

            if (this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline || this.Source == SourceType.ArmSoft || this.Source == SourceType.AcbaOnlineXML)
            {
                this.RegistrationDate = DateTime.Now.Date;
                if (this.Fees == null)
                {
                    this.Fees = new List<OrderFee>();
                }
                if (this.TransferFee > 0)
                {
                    short feeType = 20;
                    if (this.SubType == 1 && this.Type == OrderType.RATransfer)
                    {
                        //Ստուգում է Ա/Ձ է թե ոչ
                        if (Customer.GetCustomerType(this.CustomerNumber) == 2)
                        {
                            if (IsTransferFromBusinessmanToOwnerAccount(this.DebitAccount.AccountNumber, this.ReceiverAccount.AccountNumber))
                            {
                                feeType = 11;
                            }
                        }
                    }
                    OrderFee orderFee = new OrderFee();
                    orderFee.Type = feeType;
                    orderFee.Account = this.FeeAccount;
                    orderFee.Currency = this.FeeAccount?.Currency;
                    orderFee.Amount = this.TransferFee;
                    this.Fees.Add(orderFee);
                }
                if (this.CardFee > 0)
                {
                    OrderFee orderFee = new OrderFee();
                    orderFee.Type = 7;
                    orderFee.Account = this.DebitAccount;
                    orderFee.Currency = this.DebitAccount?.Currency;
                    orderFee.Amount = this.CardFee;
                    this.Fees.Add(orderFee);
                }

                if (Type == OrderType.RATransfer && SubType == 1 && string.IsNullOrWhiteSpace(Receiver))
                {
                    Receiver = Account.GetAccountDescription(ReceiverAccount.AccountNumber);
                }

            }

            if (Source != SourceType.SberBankTransfer)
                this.CheckDillingConfirm();

        }



        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            List<ActionError> warnings = new List<ActionError>();

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

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                if (this.ValidateForCash || this.Type == OrderType.TransitNonCashOut || this.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder
                        || this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount || this.Type == OrderType.CashOutFromTransitAccountsOrder
                        || this.Type == OrderType.SSTerminalCashInOrder || this.Type == OrderType.SSTerminalCashOutOrder)
                {
                    result = PaymentOrderDB.SaveCash(this, userName, source);
                }
                else
                {
                    result = PaymentOrderDB.Save(this, userName, source);

                    if (this.AdditionalParametrs != null && this.AdditionalParametrs.Exists(m => m.AdditionValue == "LeasingAccount"))
                    {
                        LeasingDB.SaveLeasingPaymentDetails(this);
                    }
                }

                ActionResult res = new ActionResult();

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

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

                result = base.SaveOrderFee();

                if (this.SwiftMessageID != 0)
                {
                    Order.SaveOrderLinkId(this.SwiftMessageID, this.Id, 1);
                }

                if (this.Type == OrderType.CardServiceFeePayment || this.Type == OrderType.CardServiceFeePaymentFromProblematicLoanTransitAccount)
                {
                    foreach (AdditionalDetails additionalDetail in this.AdditionalParametrs)
                    {
                        if (additionalDetail.Id == Constants.CARD_SERVICE_FEE_PAYMENT_ADDITIONAL_ID)
                        {
                            Order.SaveOrderProductId(Convert.ToUInt64(additionalDetail.AdditionValue), this.Id);
                        }
                    }

                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                //Վիվասել ՄՏՍ ընթացիկ հաշվին կատարված փոխանցում
                if (((this.Type == OrderType.RATransfer && (this.SubType == 1 || this.SubType == 3)) || this.Type == OrderType.Convertation ||
                this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation)
                && this.ReceiverAccount.AccountNumber == Constants.VIVACELL_PAYMENT_BY_TRANSFER_ACCOUNT_NUMBER)
                {
                    DateTime currentOperDay = Utility.GetNextOperDay().Date;
                    if (currentOperDay == DateTime.Now.Date)
                    {
                        DateTime actionStartDate = DateTime.Now;
                        VivaCellBTFCheckDetails vivaCellBTFCheck = new VivaCellBTFCheckDetails();
                        vivaCellBTFCheck.CheckTransferPossibility(Description, Amount);

                        if (vivaCellBTFCheck.PaymentTransactionID != null)
                        {
                            result = SaveUtilityTransferDetails(vivaCellBTFCheck.PaymentTransactionID, this.Id, CommunalTypes.VivaCell, vivaCellBTFCheck.PaymentTransactionDateTime != null ? vivaCellBTFCheck.PaymentTransactionDateTimeFormated : null, actionStartDate);
                        }
                    }
                }

                short amlResult = 0;
                //Բանկի ներսում հաշիվների միջև փոխանցման և կոնվերտացիայի դեպքում ստուգում ենք ԱՄԼ ստուգման անհրաժեշտությունը
                if (this.Type == OrderType.RATransfer && this.SubType == 1 || this.Type == OrderType.InBankConvertation)
                {
                    ulong receiver_customer_number = this.ReceiverAccount.GetAccountCustomerNumber();
                    if (this.CustomerNumber != receiver_customer_number)
                    {
                        amlResult = PaymentOrderDB.MatchAMLConditions(this);
                        if (amlResult != 1)
                        {
                            amlResult = PaymentOrderDB.MatchAMLConditions(this, receiver_customer_number);
                        }
                        //եթե կա ԱՄԼ ստուգման անհրաժեշտություն, պահպանում ենք փոխանցման տվյալները ԱՄԼ ծրագրում ցուցադրելու համար
                        if (amlResult == 1)
                        {
                            PaymentOrderDB.AddInBankTransferDetailsForAML(this, amlResult);
                        }
                    }
                }

                result = base.Approve(schemaType, userName, amlResult);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    //Տերմինալներից քարտից ելքագրման դեպքում նախքան հաճախորդին գումար տրամադրելը անհրաժեշտ է գործարքի հայտը գրանցելիս նաև գործարքը ուղարկել ARCA համակարգ
                    if (this.Source == SourceType.SSTerminal && this.Type == OrderType.CashCredit && this.SubType == 1 && Utility.IsCardAccount(this.DebitAccount.AccountNumber))
                    {
                        OrderDB.ChangeQuality(this.Id, OrderQuality.SBQprocessed, user.userID.ToString());
                    }
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

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));
                if (this.Source != SourceType.SSTerminal && this.Source != SourceType.CashInTerminal)
                {
                    //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                    if (result.ResultCode == ResultCode.Normal && (this.Type == OrderType.CashConvertation || this.Type == OrderType.CashCredit || this.Type == OrderType.CashCreditConvertation
                        || this.Type == OrderType.CashDebit || this.Type == OrderType.CashDebitConvertation || this.Type == OrderType.CashForRATransfer || this.Type == OrderType.TransitCashOutCurrencyExchangeOrder || this.Type == OrderType.TransitCashOut))
                    {
                        warnings.AddRange(user.CheckForNextCashOperation(this));
                    }
                }

            }
            catch
            {

            }

            result.Errors = warnings;
            return result;
        }

        /// <summary>
        /// Վերադարձնում է կրեդիտային հաշվի հետ կապված զգուշացումները
        /// </summary>
        /// <returns></returns>
        public static List<string> GetReceiverAccountWarnings(string accountNumber, Culture culture, ulong opCustomerNumber = 0)
        {
            List<string> warnings = new List<string>();
            ActionResult result = new ActionResult();

            Account account = Account.GetAccount(accountNumber);
            if (account != null)
            {
                ulong customerNumber = account.GetAccountCustomerNumber();

                if (opCustomerNumber != customerNumber && Account.CheckAccountForDAHK(account.AccountNumber))
                {
                    warnings.Add(Info.GetTerm(546, new string[] { }, Languages.hy));
                }

                if (opCustomerNumber != customerNumber)
                {
                    result.Errors.AddRange(Validation.ValidateCustomerDocument(customerNumber));
                }
                Localization.SetCulture(result, culture);

                result.Errors.ForEach(m =>
                {
                    warnings.Add(m.Description);
                }
                );
            }
            return warnings;
        }


        /// <summary>
        /// Անդորրագրի համարի ստեղծում
        /// </summary>
        /// <param name="currencyCode">Արժույթի կոդ</param>
        /// <param name="operationType">Կոնվերտացիայի տեսակ(1` Վաճառք, 2` Առք)</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public string CreateSerialNumber(int currencyCode, byte operationType)
        {
            return PaymentOrderDB.CreateSerialNumber(currencyCode, operationType);
        }

        public static Tuple<bool, string> IsBigAmount(Order order)
        {
            string inOut = "";
            string debitAccountNumber = "";
            string receiverAccountNumber = "";
            ulong customerNumber;
            double amount = 0;
            string currency = "";

            PaymentOrder paymentOrder = new PaymentOrder();
            CurrencyExchangeOrder currencyExchangeOrder = new CurrencyExchangeOrder();

            if (order.Source != SourceType.Bank || (order.Source == SourceType.Bank && order.OPPerson == null))
            {
                order.OPPerson = Order.SetOrderOPPerson(order.CustomerNumber);
            }

            customerNumber = order.OPPerson.CustomerNumber;



            switch (order.Type)
            {
                case OrderType.CashDebit:
                case OrderType.ReestrTransferOrder:
                    inOut = "in";
                    break;
                case OrderType.CashCredit:
                    inOut = "out";
                    break;
                case OrderType.CashConvertation:
                    inOut = "in_out";
                    debitAccountNumber = "";
                    receiverAccountNumber = "";
                    break;
                case OrderType.CashDebitConvertation:
                    inOut = "in";
                    break;
                case OrderType.CashCreditConvertation:
                    inOut = "out";
                    break;
                case OrderType.TransitNonCashOutCurrencyExchangeOrder:
                    inOut = "in";
                    break;
                case OrderType.TransitCashOutCurrencyExchangeOrder:
                    inOut = "in_out";
                    debitAccountNumber = "";
                    receiverAccountNumber = "";
                    break;
                case OrderType.TransitNonCashOut:
                    inOut = "in";
                    break;

            }

            if (order.Type == OrderType.CashCredit || order.Type == OrderType.CashDebit ||
                order.Type == OrderType.TransitNonCashOut || order.Type == OrderType.ReestrTransferOrder)
            {
                paymentOrder = (PaymentOrder)order;
                if (inOut == "in")
                {
                    receiverAccountNumber = paymentOrder.ReceiverAccount.AccountNumber;
                }
                else if (inOut == "out")
                {
                    debitAccountNumber = paymentOrder.DebitAccount.AccountNumber;
                }

                amount = paymentOrder.Amount;
                currency = paymentOrder.Currency;
            }
            else
                if (order.Type == OrderType.CashCreditConvertation || order.Type == OrderType.CashDebitConvertation || order.Type == OrderType.CashConvertation || order.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder || order.Type == OrderType.TransitCashOutCurrencyExchangeOrder)
            {
                currencyExchangeOrder = (CurrencyExchangeOrder)order;
                if (inOut == "in")
                {
                    receiverAccountNumber = currencyExchangeOrder.ReceiverAccount.AccountNumber;
                }
                else if (inOut == "out")
                {
                    debitAccountNumber = currencyExchangeOrder.DebitAccount.AccountNumber;

                }
                amount = currencyExchangeOrder.AmountInAmd;
                currency = "AMD";
            }
            return PaymentOrderDB.IsBigAmount(inOut, (inOut != "in") ? debitAccountNumber : receiverAccountNumber, amount, currency, customerNumber, order.OperationDate.Value);
        }

        /// <summary>
        /// Վերադարձնում է վճարման հանձնարարականի նկարագրությունը
        /// </summary>
        /// <returns></returns>
        public static string GetPaymentOrderDescription(PaymentOrder order, ulong customerNumber)
        {
            return PaymentOrderDB.GetPaymentOrderDescription(order, customerNumber);
        }




        public static bool IsTransferFromBusinessmanToOwnerAccount(string debitAccountNumber, string creditAccountNumber)
        {
            return PaymentOrderDB.IsTransferFromBusinessmanToOwnerAccount(debitAccountNumber, creditAccountNumber);
        }

        /// <summary>
        /// Ստուգում է կանխիկ ելք տարանցիկ հաշվի ժամանակ անհրաժեշտ է գնա հաստատամնա թե ոչ
        /// </summary>
        /// <param name="debitAccountNumber"></param>
        /// <returns></returns>
        public static bool IsRequireApprovalCashOutFromTransitAccounts(string debitAccountNumber)
        {
            return PaymentOrderDB.IsRequireApprovalCashOutFromTransitAccounts(debitAccountNumber);
        }
        /// <summary>
        /// Ցույց է տալիս աշխ․ծրագրով ռեեստր կա մասնակի կատարված օրվա ընթացքում
        /// </summary>
        /// <returns></returns>
        public bool IsReestrOrderDonePartially()
        {
            return PaymentOrderDB.IsReestrOrderDonePartially(this.CustomerNumber, this.RegistrationDate);
        }

        /// <summary>
        /// Պահպանում է բիլինգային համակարգից ստացված տվյալները
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="orderId"></param>
        /// <param name="utilityServiceType"></param>
        /// <param name="externalStartDate"></param>
        /// <returns></returns>
        public static ActionResult SaveUtilityTransferDetails(string transactionId, long orderId, CommunalTypes utilityServiceType, DateTime? externalStartDate, DateTime actionStartDate)
        {
            ActionResult result = new ActionResult();

            result = PaymentOrderDB.SaveUtilityTransferDetails(transactionId, orderId, utilityServiceType, externalStartDate, actionStartDate);

            return result;
        }

        public static void SaveDAHKPaymentType(long orderId, int paymentType, int setNumber)
        {
            PaymentOrderDB.SaveDAHKPaymentType(orderId, paymentType, setNumber);
        }

        public static Tuple<string, string> GetSintAccountForHB(string accountNumber)
        {
            return PaymentOrderDB.GetSintAccountForHB(accountNumber);
        }

        public DateTime GetNormalEndOfDeposit(string productNumber)
        {
            return PaymentOrderDB.GetNormalEndOfDeposit(productNumber);
        }


        public static bool IsDebetExportAndImportCreditLine(string debAccountNumber)
        {
            return PaymentOrderDB.IsDebetExportAndImportCreditLine(debAccountNumber);
        }

        public void InitOrderForPaymentOrder(TransferByCallChangeOrder order, Transfer transfer, Customer customer)
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

            Type = OrderType.TransitNonCashOut; //85 î³ñ³ÝóÇÏ Ñ³ßíÇó ³ÝÏ³ÝËÇÏ »Éù
            SubType = 1; //բանկի ներսում

            OrderNumberTypes orderNumberTypes = OrderNumberTypes.PaymentOrder; //10 Սեփական հաշիվների միջև
            OrderNumber = Convert.ToString(GenerateNewOrderNumber(orderNumberTypes, customer.User.filialCode));

            ReceiverBankCode = Convert.ToInt32(ReceiverAccount.AccountNumber.Substring(0, 5));
            Description = "Փոխանցման հաստատում (" + transfer.TransferSystemDescription + " " + transfer.SenderReferance + ")";

            Currency = DebitAccount.Currency;
        }
    }
}