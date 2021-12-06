using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions;
using System.Data;
using System.Configuration;

namespace ExternalBanking
{
    public class LoanProductOrder : Order
    {
        /// <summary>
        /// Վարկային պրոդուկտի տեսակ
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Վարկի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վարկի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Վարկի տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Գրավի հաշիվ
        /// </summary>
        public Account ProvisionAccount { get; set; }

        /// <summary>
        /// Գրավադրված գումար
        /// </summary>
        public double ProvisionAmount { get; set; }

        /// <summary>
        /// Գրավադրված գումարի արժույթ
        /// </summary>
        public string ProvisionCurrency { get; set; }

        /// <summary>
        /// Առաջին վճարման օր
        /// </summary>
        public DateTime FirstRepaymentDate { get; set; }

        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }

        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի հաշիվ
        /// </summary>
        public Account ProductAccount { get; set; }

        /// <summary>
        /// Վեճերի լուծման եղանակ
        /// </summary>
        public short DisputeResolution { get; set; }

        /// <summary>
        /// Վեճերի լուծման եղանակի նկարագրություն
        /// </summary>
        public string DisputeResolutionDescription { get; set; }

        /// <summary>
        /// Հաճախորդի հետ հաղորդակցման եղանակ
        /// </summary>
        public short CommunicationType { get; set; }

        /// <summary>
        /// Հաղորդակցման եղանակի նկարագրություն
        /// </summary>
        public string CommunicationTypeDescription { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի օգտագործման երկիր
        /// </summary>
        public int LoanUseCountry { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի օգտագործման մարզ
        /// </summary>
        public int LoanUseRegion { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի օգտագործման շրջան
        /// </summary>
        public int LoanUseLocality { get; set; }

        /// <summary>
        /// Դրամային գումար
        /// </summary>
        public double AmountInAMD { get; set; }

        /// <summary>
        /// Վարկային գծի պարտադիր մուտքեր
        /// </summary>
        public bool MandatoryPayment { get; set; }


        /// <summary>
        /// Վարկային գծի (քարտի) վերջ
        /// </summary>
        public DateTime? ValidationDate { get; set; }

        /// <summary>
        /// Գրավադրվող արժույթ
        /// </summary>
        public string PledgeCurrency { get; set; }

        /// <summary>
        /// true` եթե հաճախորդը նշել է checkbox-ը, false հակառակ դեպքում
        /// </summary>
        public bool AcknowledgedByCheckBox { get; set; }

        /// <summary>
        /// Եթե checkbox-ը նշված է ապա checkbox-ի դիմաց ցուցադրված տեքստը, հակառակ դեպքում՝ հաճախորդի կողմից մուտքագրված տեքստը 
        /// </summary>
        public string AcknowledgementText { get; set; }

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

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                if (this.Type == OrderType.CreditSecureDeposit)
                {
                    result = LoanProductOrderDB.SaveLoanOrder(this, userName, source);
                }
                else if (this.Type == OrderType.CreditLineSecureDeposit || this.Type == OrderType.FastOverdraftApplication
                    || this.Type == OrderType.LoanApplicationConfirmation)
                {
                    result = LoanProductOrderDB.SaveCreditLineOrder(this, userName, source);
                }
                else if (this.Type == OrderType.LoanApplicationAnalysis || this.Type == OrderType.CancelLoanApplication
                    || this.Type == OrderType.DeleteLoanApplication)
                {
                    result = LoanProductOrderDB.SaveLoanApplicationQualityChangeOrder(this, userName);
                }
                if ((this.Type == OrderType.FastOverdraftApplication || this.Type == OrderType.LoanApplicationAnalysis
                    || this.Type == OrderType.CancelLoanApplication || this.Type == OrderType.DeleteLoanApplication
                    || this.Type == OrderType.LoanApplicationConfirmation || this.Type == OrderType.CreditLineSecureDeposit) && action == Action.Add)
                {
                    Order.SaveOrderProductId(this.ProductId, this.Id);
                }

                if (action == Action.Add)
                {
                    base.SaveOrderFee();
                }
                else
                {
                    base.UpdateOrderFee();
                }

                //**********
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (this.Type == OrderType.FastOverdraftApplication || this.Type == OrderType.LoanApplicationConfirmation)
            {
                Card card = null;
                string cardNumber = "";

                cardNumber = Card.GetCardNumber(Convert.ToInt64(this.ProductId));

                if (!string.IsNullOrEmpty(cardNumber))
                {
                    card = Card.GetCard(cardNumber);
                    if (card == null)
                    {
                        //Քարտը գտնված չէ
                        result.Errors.Add(new ActionError(534));
                        return result;
                    }

                    if (card.Type == 52 && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft))
                    {
                        if (this.Type == OrderType.FastOverdraftApplication || this.Type == OrderType.CreditLineActivation)
                        {
                            //Visa Signature քարտով հնարավոր չէ ստանալ Արագ օվերդրաֆտ։
                            result.Errors.Add(new ActionError(1901));
                            return result;
                        }
                        if (this.Type == OrderType.CreditLineSecureDeposit && (this.ProductType == 50 || this.ProductType == 51))
                        {
                            //Հնարավոր չէ պահպանել հայտ տվյալ քարտատեսակի դեպքում։
                            result.Errors.Add(new ActionError(1900));
                            return result;
                        }
                    }

                    if (card.Type == 21)
                    {
                        //Հնարավոր չէ պահպանել հայտ տվյալ քարտատեսակի դեպքում։
                        result.Errors.Add(new ActionError(1900));
                        return result;
                    }
                }

                result.Errors.AddRange(Validation.ValidateFastOverdraftApplication(this));
            }
            else if (this.Type == OrderType.LoanApplicationAnalysis)
            {
                result.Errors.AddRange(Validation.ValidateLoanApplicationAnalysis(this));
            }
            else if (this.Type == OrderType.CancelLoanApplication)
            {
                result.Errors.AddRange(Validation.ValidateCancelLoanApplication(this));
            }
            else if (this.Type == OrderType.DeleteLoanApplication)
            {
                result.Errors.AddRange(Validation.ValidateDeleteLoanApplication(this));
            }
            else
            {
                result.Errors.AddRange(Validation.ValidateLoanProductOrderDocument(this));
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if (Type == OrderType.CreditSecureDeposit)
            {
                List<Account> accounts = Account.GetCurrentAccounts(CustomerNumber, ProductQualityFilter.Opened);
                if (!accounts.Any(x => x.Currency == "AMD" && x.AccountType == 10 && x.JointType == 0)) // միայն ընթացիկ AMD հաշիվներ (առանց հօգուտ և համատեղ հաշիվների) չլինելու դեպքում true
                {
                    //Վարկի ստացման նպատակով անհրաժեշտ է բացել ՀՀ դրամով ընթացիկ հաշիվ։
                    result.Errors.Add(new ActionError(1894));
                }
            }

            if (this.Type == OrderType.FastOverdraftApplication && (this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline))
            {
                if (Account.CheckAccessToThisAccounts(this.ProductAccount?.AccountNumber) > 0)
                {
                    //Նշված հաշվին մուտքերն արգելված են
                    result.Errors.Add(new ActionError(1891));
                    return result;
                }
            }


            return result;
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void GetLoanOrder()
        {
            LoanProductOrderDB.GetLoanOrder(this);
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void GetCreditLineOrder()
        {
            LoanProductOrderDB.GetCreditLineOrder(this);
        }

        /// <summary>
        /// Ավանդի գրավով վարկի(վարկային գծի) հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            //Ավանդի գրավով վարկ
            if ((this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking) && this.Type == OrderType.CreditSecureDeposit)
            {
                UpdateLoanProductOrderContractDate(this.Id);
            }

            //Ավանդի գրավով վարկային գիծ
            if ((this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking) && (this.Type == OrderType.CreditLineSecureDeposit || this.Type == OrderType.FastOverdraftApplication))
            {
                UpdateCreditLineProductOrderContractDate(this.Id);
            }

            ActionResult result = ValidateForSend();


            if (result.ResultCode == ResultCode.Normal)
            {
                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnlineXML || this.Source == SourceType.ArmSoft)
                {
                    if (this.ProductType == 29)
                    {
                        PrintDepositLoanContract(this.Id, this.CustomerNumber, true);
                        CreditLine.GetLoansDramContract(this.Id, 1, true, this.CustomerNumber);
                    }
                    if (this.ProductType == 30)
                    {
                        PrintDepositCreditLineContract(this.Id, this.CustomerNumber, true);
                        CreditLine.GetLoansDramContract(this.Id, 2, true, this.CustomerNumber);
                    }
                    if (this.ProductType == 54)
                    {
                        PrintFastOverdraftContract(this.Id, this.CustomerNumber, true);
                    }
                }

                Action action = this.Id == 0 ? Action.Add : Action.Update;



                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {

                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking || this.Source == SourceType.EContract)
                        SaveLoanAcknowledgementText(this.AcknowledgedByCheckBox, this.AcknowledgementText, this.Id, this.Type);

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
        /// <summary>
        /// Ավանդի գրավով վարկի(վարկային գծի) հայտի հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            DateTime nextOperDay = Utility.GetNextOperDay().Date;
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
            else if (this.StartDate != nextOperDay)
            {
                //Սկիզբ դաշտն ուղարկման պահին պետք է լինի @nextOperDay:           
                result.Errors.Add(new ActionError(1220, new string[] { nextOperDay.ToString("dd/MM/yyyy") }));
            }
            else if (this.Type == OrderType.FastOverdraftApplication && this.Source == SourceType.AcbaOnline)
            {
                string currency = this.ProductAccount.Currency;
                if (currency != "AMD")
                {
                    if (Utility.GetCBKursForDate(this.RegistrationDate, currency) != Utility.GetCBKursForDate(DateTime.Now.Date, currency))
                    {
                        //ԿԲ փոխարժեքը չի համապատասխանում ներկա փոխարժեքին։
                        result.Errors.Add(new ActionError(1267, new string[] { nextOperDay.ToString("dd/MM/yyyy") }));
                    }
                }
            }

            if (this.Type == OrderType.CreditSecureDeposit || this.Type == OrderType.CreditLineSecureDeposit || this.Type == OrderType.FastOverdraftApplication)
            {
                result.Errors.AddRange(Validation.CheckForRegisterRequestData(0, this.CustomerNumber, 4, Source));
            }


            if (this.Type == OrderType.FastOverdraftApplication)
            {
                if (CreditLine.GetCreditLines(CustomerNumber, ProductQualityFilter.Opened).Where(x => x.Type == 54).Count() >= 3)
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                    {
                        result.Errors.Add(new ActionError(1627));
                    }
                    else
                        result.Errors.Add(new ActionError(1626));
                }
            }
            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                if (this.Type == OrderType.CreditLineSecureDeposit)
                {
                    if (CheckLoanRequest(this))
                    {
                        //Գոյությու ունի չտրված վարկ/վարկային գիծ
                        result.Errors.Add(new ActionError(302));
                    }
                    if (CheckActiveCreditLine(this.ProductAccount.AccountNumber, this.CustomerNumber))
                    {
                        //Տվյալ քարտին արդեն իսկ առկա է Վարկային գիծ / Օվերդրաֆտ:
                        result.Errors.Add(new ActionError(1713));
                    }
                    if (CheckCreditLineOrder(this.ProductAccount.AccountNumber))
                    {
                        //Տվյալ քարտի համար արդեն իսկ առկա է բանկ ուղարկված սակայն դեռևս չկատարված Վարկային գծի / Օվերդրաֆտի դիմում։
                        result.Errors.Add(new ActionError(1714));
                    }

                    double rate = GetInterestRate(this, this.ProductAccount.ProductNumber);

                    if (rate != InterestRate)
                    {
                        //Տոկոսադրույքը սխալ է:
                        result.Errors.Add(new ActionError(719));
                    }

                    double percent = LoanProductOrder.GetDepositLoanCreditLineAndProfisionCoefficent(Currency, PledgeCurrency, MandatoryPayment, ProductType);

                    if (Amount * Utility.GetCBKursForDate(DateTime.Now.Date, Currency) / Utility.GetCBKursForDate(DateTime.Now.Date, PledgeCurrency) > Customer.GetCustomerAvailableAmount(CustomerNumber, PledgeCurrency) * percent)
                    {
                        result.Errors.Add(new ActionError(307, new string[] { (InterestRate * 100).ToString() }));
                    }

                    double provisionAmountNew = 0;
                    DateTime currentOperDay = UtilityDB.GetCurrentOperDay();
                    provisionAmountNew = Math.Round(this.Amount / LoanProductOrderDB.GetDepositLoanCreditLineAndProfisionCoefficent(this.Currency, this.PledgeCurrency, this.MandatoryPayment, this.ProductType)
                    * UtilityDB.GetCBKursForDate(currentOperDay, this.Currency) / UtilityDB.GetCBKursForDate(currentOperDay, this.PledgeCurrency), 0);
                    if (provisionAmountNew != this.ProvisionAmount)
                    {
                        //Գրավադրվող գումարը սխալ է
                        result.Errors.Add(new ActionError(1834));
                        return result;
                    }

                }
                if (this.Type == OrderType.CreditSecureDeposit)
                {
                    double rate = GetInterestRate(this, null);

                    if (rate != InterestRate)
                    {
                        //Տոկոսադրույքը սխալ է:
                        result.Errors.Add(new ActionError(719));
                    }


                    double feeAccountBalance = Account.GetAcccountAvailableBalance(this.FeeAccount.AccountNumber);
                    if (feeAccountBalance < this.FeeAmount)
                    {
                        //հաշվի մնացորդը չի բավարարում գործարքը կատարելու համար
                        result.Errors.Add(new ActionError(788, new string[] { FeeAccount.AccountNumber }));
                    }

                    if (this.FirstRepaymentDate <= this.StartDate)
                    {
                        //Առաջին մարման ամսաթիվը փոքր է վարկի սկզբից
                        result.Errors.Add(new ActionError(305));
                    }

                    double percent = LoanProductOrder.GetDepositLoanAndProvisionCoefficent(Currency, PledgeCurrency);

                    if (Amount * Utility.GetCBKursForDate(DateTime.Now.Date, Currency) / Utility.GetCBKursForDate(DateTime.Now.Date, PledgeCurrency) > Customer.GetCustomerAvailableAmount(CustomerNumber, PledgeCurrency) * percent)
                    {
                        result.Errors.Add(new ActionError(307, new string[] { (InterestRate * 100).ToString() }));
                    }
                }
                if(Type == OrderType.CreditLineSecureDeposit || Type == OrderType.FastOverdraftApplication)
                {
                    if (LoanProductOrderDB.IsSecondTime(ProductAccount.AccountNumber,StartDate))
                    {
                        result.Errors.Add(new ActionError(1887));
                    }
                }
            }

            if (this.Type == OrderType.FastOverdraftApplication || this.Type == OrderType.LoanApplicationConfirmation)
            {
                string cardNumber = "";
                Card card = null;
                cardNumber = Card.GetCardNumber(Convert.ToInt64(this.ProductId));

                if (!string.IsNullOrEmpty(cardNumber))
                {
                    card = Card.GetCardMainData(cardNumber);
                    if (card == null)
                    {
                        //Քարտը գտնված չէ
                        result.Errors.Add(new ActionError(534));
                        return result;
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
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            if (this.Type == OrderType.LoanApplicationConfirmation)
            {
                LoanApplication application = LoanApplication.GetLoanApplication(this.ProductId, this.CustomerNumber);
                if (!string.IsNullOrEmpty(application.CardNumber))
                {
                    Card card = Card.GetCard(application.CardNumber);
                    if (card != null)
                        this.ProductAccount = card.CardAccount;
                }


                this.StartDate = application.StartDate;
                this.EndDate = application.StartDate.AddMonths(application.Duration);
                this.Amount = application.Amount;
                this.Currency = application.Currency;
                if (!Utility.IsWorkingDay(this.EndDate))
                {
                    this.EndDate = this.EndDate.AddDays(1);
                    while (!Utility.IsWorkingDay(this.EndDate))
                    {
                        this.EndDate = this.EndDate.AddDays(1);
                    }
                }
            }


            if (this.ProductType == 51 || this.ProductType == 50)
            {
                Card card = Card.GetCard(this.ProductId, this.CustomerNumber);
                this.ProductAccount = card.CardAccount;
                this.Currency = card.Currency;

                short cardStatementCommunicationType = card.GetCardStatementReceivingType();

                if (cardStatementCommunicationType == 1)
                {
                    this.CommunicationType = 3;
                }
                else if (cardStatementCommunicationType == 3)
                {
                    this.CommunicationType = 1;
                }
                else if (cardStatementCommunicationType == 4)
                {
                    this.CommunicationType = 2;
                }



                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                    this.ValidationDate = this.EndDate;
            }



            else if (this.Type == OrderType.FastOverdraftApplication && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking))
            {
                Card card;
                if (this.ProductAccount != null && this.ProductId == 0)
                {
                    card = Card.GetCardWithOutBallance(this.ProductAccount.AccountNumber);
                    this.ProductId = (ulong)card.ProductId;
                }
                else
                {
                    card = Card.GetCard(this.ProductId, this.CustomerNumber);
                    this.ProductAccount = card.CardAccount;
                    this.Currency = card.Currency;
                }

                short cardStatementCommunicationType = card.GetCardStatementReceivingType();

                if (cardStatementCommunicationType == 1)
                {
                    this.CommunicationType = 3;
                }
                else if (cardStatementCommunicationType == 3)
                {
                    this.CommunicationType = 1;
                }
                else if (cardStatementCommunicationType == 4)
                {
                    this.CommunicationType = 2;
                }

                this.StartDate = Utility.GetNextOperDay();
                this.EndDate = Utility.GetFastOverdrafEndDate(this.StartDate);
                if (this.Currency != "AMD")
                {

                    double exchangeRate = Utility.GetCBKursForDate(this.StartDate, this.Currency);
                    this.Amount = Convert.ToDouble((int)(this.AmountInAMD / exchangeRate));

                }
                else if (this.AmountInAMD > 0)
                {
                    this.Amount = this.AmountInAMD;
                }

                this.FeeAmount = GetFastOverdraftFeeAmount(this.AmountInAMD);

                this.Fees = new List<OrderFee>();
                OrderFee fee = new OrderFee();
                fee.Amount = this.FeeAmount;
                fee.Account = card.CardAccount;
                fee.Type = 21;
                fee.Currency = "AMD";
                this.Fees.Add(fee);
            }

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (this.Type == OrderType.CreditSecureDeposit)
                {
                    this.MandatoryPayment = true;
                }

                this.StartDate = Utility.GetNextOperDay();

                if (this.Type == OrderType.CreditSecureDeposit || this.Type == OrderType.FastOverdraftApplication || this.Type == OrderType.CreditLineSecureDeposit)
                {
                    Tuple<int, int> countryAndLocality = SetCustomerCountryAndLocality(this.CustomerNumber);
                    this.LoanUseCountry = countryAndLocality.Item1;
                    this.LoanUseLocality = countryAndLocality.Item2;

                    this.CommunicationType = 2;
                    this.DisputeResolution = 1;
                }
            }

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        ///Ավանդի գրավով վարկի(վարկային գծի) հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete(source);
            ActionResult result = new ActionResult();
            result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            if (this.Type != OrderType.LoanApplicationAnalysis && this.Type != OrderType.CancelLoanApplication && this.Type != OrderType.DeleteLoanApplication)
            {
                result = this.ValidateForSend();
            }
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                if (this.Type == OrderType.CreditSecureDeposit)
                {
                    result = LoanProductOrderDB.SaveLoanOrder(this, userName, source);
                }
                else if (this.Type == OrderType.CreditLineSecureDeposit || this.Type == OrderType.FastOverdraftApplication
                    || this.Type == OrderType.LoanApplicationConfirmation)
                {
                    result = LoanProductOrderDB.SaveCreditLineOrder(this, userName, source);
                }
                else if (this.Type == OrderType.LoanApplicationAnalysis || this.Type == OrderType.CancelLoanApplication
                    || this.Type == OrderType.DeleteLoanApplication)
                {
                    result = LoanProductOrderDB.SaveLoanApplicationQualityChangeOrder(this, userName);
                }
                if (this.Type == OrderType.FastOverdraftApplication || this.Type == OrderType.LoanApplicationAnalysis
                    || this.Type == OrderType.CancelLoanApplication || this.Type == OrderType.DeleteLoanApplication
                    || this.Type == OrderType.LoanApplicationConfirmation)
                {
                    Order.SaveOrderProductId(this.ProductId, this.Id);
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
                else
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }



            if (this.Source != SourceType.AcbaOnline && this.Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }




            return result;
        }



        public static double GetInterestRate(LoanProductOrder order, string cardNumber)
        {
            double interestRate = 0;

            if (order.Type == OrderType.CreditSecureDeposit)
            {
                interestRate = LoanProductOrder.GetInterestRateForDepositLoan(order.StartDate, order.EndDate);
            }
            else if (order.Type == OrderType.CreditLineSecureDeposit)
            {
                interestRate = LoanProductOrderDB.GetInterestRateForCreditLine(cardNumber);
            }
            else if (order.Type == OrderType.FastOverdraftApplication)
            {
                interestRate = 0;
            }


            return interestRate;

        }

        public static bool CheckLoanRequest(LoanProductOrder order)
        {
            return LoanProductOrderDB.CheckLoanRequest(order);
        }
        public static ActionError ValidateLoanProduct(LoanProductOrder order)
        {
            return LoanProductOrderDB.ValidateLoanProduct(order);
        }

        /// <summary>
        /// Վերադարձնում է Ավանդի գրավով վարկի գումարի և գրավի գումարի հարաբերակցության առավելագույն տոկոսը
        /// </summary>
        /// <param name="loanCurrency"></param>
        /// <param name="provisionCurrency"></param>
        /// <returns></returns>
        public static double GetDepositLoanAndProvisionCoefficent(string loanCurrency, string provisionCurrency)
        {
            return LoanProductOrderDB.GetDepositLoanAndProvisionCoefficent(loanCurrency, provisionCurrency);
        }



        /// <summary>
        /// Վերադարձնում է Ավանդի գրավով վարկային գծի գումարի և գրավի գումարի հարաբերակցության առավելագույն տոկոսը
        /// </summary>
        /// <param name="loanCurrency"></param>
        /// <param name="provisionCurrency"></param>
        /// <param name="mandatoryPayment"></param>
        /// <returns></returns>
        public static double GetDepositLoanCreditLineAndProfisionCoefficent(string loanCurrency, string provisionCurrency, bool mandatoryPayment, int creditLineType)
        {
            return LoanProductOrderDB.GetDepositLoanCreditLineAndProfisionCoefficent(loanCurrency, provisionCurrency, mandatoryPayment, creditLineType);
        }


        public static double GetInterestRateForDepositLoan(DateTime startDate, DateTime endDate)
        {
            return LoanProductOrderDB.GetInterestRateForDepositLoan(startDate, endDate);
        }

        /// <summary>
        /// Արագ օվերդրաֆտի ստուգումներ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="source"></param>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static List<ActionError> FastOverdraftValidations(ulong customerNumber, SourceType source, string cardNumber = null)
        {
            return LoanProductOrderDB.FastOverdraftValidations(customerNumber, source, cardNumber);
        }

        /// <summary>
        /// Ստուգում է որ նույն քարտի համար նորից չմուտքագրվի վարկային դիմում եթե ունի արդեն դիմում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <returns></returns>
        public static bool CheckLoanApplication(string cardNumber)
        {
            return LoanProductOrderDB.CheckLoanApplication(cardNumber);
        }

        /// <summary>
        /// Ստուգում է որ նույն քարտի համար գոյություն ունի ուղարկված և չհաստատված հայտ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool IsSecontLoanApplication(ulong productId, ushort orderType, long docId)
        {
            return LoanProductOrderDB.IsSecontLoanApplication(productId, orderType, docId);
        }

        /// <summary>
        /// Վերադարձնում է արագ օվերդրաֆտի միջնորդավճարը
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double GetFastOverdraftFeeAmount(double amount)
        {
            double feeAmount = 0;
            if (amount == 100000)
            {
                feeAmount = 5800;
            }
            else if (amount == 50000)
            {
                feeAmount = 2900;
            }
            return feeAmount;
        }

        /// <summary>
        /// Ավտոմատ լրացնում է AcbaOnline կամ AcbaMobile համար ավանդի գրավով վարկի/վարկային գծի/ օվերդրաֆտի օգտագործման վայրը 
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static Tuple<int, int> SetCustomerCountryAndLocality(ulong customerNumber)
        {
            return LoanProductOrderDB.SetCustomerCountryAndLocality(customerNumber);
        }



        public static double GetRedemptionAmountForDepositLoan(double startCapital, double interestRate, DateTime dateOfBeginning, DateTime dateOfNormalEnd, DateTime firstRepaymentDate)
        {
            return LoanProductOrderDB.GetRedemptionAmountForDepositLoan(startCapital, interestRate, dateOfBeginning, dateOfNormalEnd, firstRepaymentDate);
        }


        public static double GetCommisionAmountForDepositLoan(double startCapital, DateTime dateOfBeginning, DateTime dateofNormalEnd, string currency, ulong customerNumber)
        {
            return LoanProductOrderDB.GetCommisionAmountForDepositLoan(startCapital, dateOfBeginning, dateofNormalEnd, currency, customerNumber);
        }

        public static double GetCreditLineDecreasingAmount(double startCapital, string currency, DateTime startDate, DateTime endDate)
        {
            return LoanProductOrderDB.GetCreditLineDecreasingAmount(startCapital, currency, startDate, endDate);
        }

        public static Dictionary<string, string> GetDepositCreditLineContractInfo(int docId)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            DataTable dt = new DataTable();
            dt = LoanProductOrderDB.GetDepositCreditLineContractInfo(docId);
            if (dt.Rows.Count > 0)
            {
                dictionary.Add("security_code_2", dt.Rows[0][0].ToString());
                dictionary.Add("repayment_percent", dt.Rows[0][1].ToString());
                dictionary.Add("repayment_kurs", dt.Rows[0][2].ToString());
            }
            return dictionary;
        }

        public static DataTable GetDepositLoanContractInfo(int docId)
        {
            return LoanProductOrderDB.GetDepositLoanContractInfo(docId);
        }


        public static string GetConnectAccountFullNumber(ulong customerNumber, string currency)
        {
            return LoanProductOrderDB.GetConnectAccountFullNumber(customerNumber, currency);

        }

        public static byte[] GetDepositLoanOrDepositCreditLineContract(string loanNumber, byte type)
        {
            return LoanProductOrderDB.GetDepositLoanOrDepositCreditLineContract(loanNumber, type);
        }

        public static byte[] PrintDepositLoanContract(long docId, ulong customerNumber, bool fromApprove = false)
        {
            return LoanProductOrderDB.PrintDepositLoanContract(docId, customerNumber, fromApprove);
        }
        public static bool CheckActiveCreditLine(string loanAccountNumber, ulong customerNumber)
        {
            return LoanProductOrderDB.CheckActiveCreditLine(loanAccountNumber, customerNumber);
        }
        public static bool CheckCreditLineOrder(string loanAccountNumber)
        {
            return LoanProductOrderDB.CheckCreditLineOrder(loanAccountNumber);
        }
        public static void UpdateLoanProductOrderContractDate(long orderId)
        {
            LoanProductOrderDB.UpdateLoanProductOrderContractDate(orderId);
        }

        public static byte[] PrintDepositCreditLineContract(long docID, ulong cusotmerNumber, bool fromApprove = false)
        {
            return LoanProductOrderDB.PrintDepositCreditLineContract(docID, cusotmerNumber, fromApprove);
        }

        public static byte[] PrintFastOverdraftContract(long docID, ulong customerNumber, bool fromApprove = false)
        {
            return LoanProductOrderDB.PrintFastOverdraftContract(docID, customerNumber, fromApprove);
        }

        public static void SaveLoanAcknowledgementText(bool acknowledgedByCheckBox, string acknowledgementText, long id, OrderType orderType) => LoanProductOrderDB.SaveLoanAcknowledgementText(acknowledgedByCheckBox, acknowledgementText, id, orderType);

        public static void UpdateCreditLineProductOrderContractDate(long orderId)
        {
            LoanProductOrderDB.UpdateCreditLineProductOrderContractDate(orderId);
        }

        public static ulong GetAmexGoldProductId(string account) => LoanProductOrderDB.GetAmexGoldProductId(account);
    }
}
