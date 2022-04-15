using ExternalBanking.ACBAServiceReference;
using ExternalBanking.ARUSDataService;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// ՍՏԱԿ վճարման հանձնարարական
    /// </summary>
    public class STAKPaymentOrder : PaymentOrder
    {
        #region Properties

        public R2ARequestDetails R2ADetails { get; set; }

        public Card ReceiverCardNumber { get; set; }

        /// <summary>
        /// Ուղարկողի անուն ազգանուն/անվանում
        /// </summary>
        public string Sender { get; set; }

        ///// <summary>  STAK-y chi talis  -- datark toxnel!!!
        ///// Ստացողի լրացուցիչ տվյալներ
        ///// </summary>
        //public string ReceiverPassport { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Թվային կոդ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Ուղարկողի երկրի անվանում
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Միջնորդավճար արժույթով
        /// </summary>
        public double Fee { get; set; }

        /// <summary>
        /// Միջնորդավճար ACBA արժույթով
        /// </summary>
        public double FeeAcba { get; set; }

        /// <summary>
        /// Փոխանցման համակարգի անունը
        /// </summary>
        public string TransferSystemDescription { get; set; }

        /// <summary>
        /// Փոխանցման լրացուցիչ տվյալներ
        /// </summary>
        public TransferAdditionalData TransferAdditionalData { get; set; }

        /// <summary>
        ///Ստացողի հեռախոսահամար
        /// </summary>
        public string ReceiverPhone { get; set; }

        /// <summary>
        /// Ստացողի անձնագիր
        /// </summary>
        public string ReceiverPassport { get; set; }

        /// <summary>
        /// Փոխանցման (Ուղարկողի) արժույթի կոդ 
        /// </summary>
        public string SendCurrencyCode { get; set; }


        public double Rate { get; set; }

        /// <summary>
        /// Փոխարկման տեսակի համար
        /// </summary>
        public short ConvertationTypeNum { get; set; }

        /// <summary>
        /// Փոխարկման տեսակ
        /// </summary>
        public string ConvertationType { get; set; }

        public ushort CrossVariant { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճարի արժույթ՝ որոշված ԱԿԲԱ-ում
        /// </summary>
        public string ACBACommissionCurrency { get; set; }

        /// <summary>
        /// Փոխանցումը ստացող հաճախորդի տեսակը
        /// </summary>
        public int ReceiverCustomerType { get; set; }

        /// <summary>
        /// R2A փոխանցման տվյալների պահպանման հայտի համար
        /// </summary>
        public long R2ATransferDocID { get; set; }

        /// <summary>
        /// R2A փոխանցման վճարման հայտի համար
        /// </summary>
        public long R2APaymentDocID { get; set; }

        /// <summary>
        /// R2A փոխանցման արժույթի փոխարկման հայտի համար
        /// </summary>
        public long R2ACurrencyExchangeDocID { get; set; }

        #endregion

        public STAKPaymentOrder()
        {
            R2ADetails = new R2ARequestDetails();
        }

        public STAKPaymentOrder(R2ARequest r2a)
        {
            R2ADetails = (R2ARequestDetails)r2a;

            Sender = r2a.SenderFirstName + " " + r2a.SenderLastName;
            Code = r2a.URN;
            ReceiverPhone = String.IsNullOrEmpty(r2a.BeneficiaryPhoneNo) ? r2a.BeneficiaryMobileNo : r2a.BeneficiaryMobileNo + ", " + r2a.BeneficiaryPhoneNo;
            Receiver = r2a.BeneficiaryFirstName + " " + r2a.BeneficiaryLastName;
            // 79 համարի հայտի դեպքում հայտի արժույթը համընկնում է փոխանցման արժույթի հետ` Currency = SendCurrencyCode, 
            // սակայն 82 տեսակի՝ փոխարկման հայտի դեպքում կարող է տարբերվել, և հայտի արժույթը որոշվում է  SetCurrency()-ի միջոցով
            SendCurrencyCode = r2a.SendCurrencyCode;
            Currency = r2a.SendCurrencyCode;
            Amount = Convert.ToDouble(r2a.SendAmount);
            RegistrationDate = DateTime.Now;
            // $scope.order.DescriptionForPayment = 'Non-commercial transfer for personal needs';   ??????????????????????
        }


        /// <summary>
        /// Հայտի պահպանում և ուղղարկում
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(ՍՏԱԿ, HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        internal new R2ARequestOutputResponse SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            R2ARequestOutputResponse response = new R2ARequestOutputResponse();

            //R2ARequestOutput r2ARequestOutput = new R2ARequestOutput();
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            //long STAKDetailsOrderID;

            Type = OrderType.ReceivedFastTransferPaymentOrder;
            SubType = 23;
            Quality = OrderQuality.Draft;
            this.Id = 0;


            result.Errors = this.ValidateBeforeComplete();

            if (result.Errors.Count > 0)
            {
                response.ActionResult.Errors = result.Errors;
                response.ActionResult.ResultCode = ResultCode.Failed;

                return response;
            }

            this.Complete();

            result = this.Validate();


            if (result.Errors.Count > 0)
            {
                response.ActionResult.Errors = result.Errors;
                response.ActionResult.ResultCode = ResultCode.Failed;

                return response;
            }


            result = this.ValidateForSend();

            if (result.Errors.Count > 0)
            {
                response.ActionResult.Errors = result.Errors;
                response.ActionResult.ResultCode = ResultCode.Failed;

                return response;
            }




            Action action = this.Id == 0 ? Action.Add : Action.Update;


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                /* ///  ՓՈԽԱՆՑՄԱՆ ՀԱՅՏԻ ՊԱՀՊԱՆՈւՄ  ///  */

                //result = STAKPaymentOrderDB.Save(this, userName, source, ref response.R2ARequestOutput);
                result = STAKPaymentOrderDB.Save(this, userName, source);

                this.R2ATransferDocID = result.Id;

                if (result.ResultCode != ResultCode.Normal)
                {
                    response.ActionResult.Errors = result.Errors;
                    response.ActionResult.ResultCode = ResultCode.Failed;

                    return response;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        response.ActionResult.Errors = result.Errors;
                        response.ActionResult.ResultCode = ResultCode.Failed;

                        return response;
                    }
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    response.ActionResult.Errors = result.Errors;
                    response.ActionResult.ResultCode = ResultCode.Failed;

                    return response;
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


            if ((this.SubType != 23) || (this.SubType == 23 && (int)source == 13))
            {
                result = base.Confirm(user);
            }

            /* ///  ՓՈԽԱՆՑՄԱՆ ՀԱՅՏԻ ԿԱՏԱՐՈւՄ  ///  */

            if (result.ResultCode == ResultCode.Normal)
            {
                ulong bankMailID = 0;
                string transferDebitAccount = null;

                STAKPaymentOrderDB.GetBankMailID(this.Id, ref bankMailID, ref transferDebitAccount);

                if (Currency == ReceiverAccount.Currency)   //   ՎՃԱՐՈւՄ
                {
                    /*PaymentOrder paymentOrder = new PaymentOrder();

                    paymentOrder = (PaymentOrder)this;
                    paymentOrder.Id = 0;
                    paymentOrder.RegistrationDate = DateTime.Now;
                    //paymentOrder.DebitAccount = new Account();   // ??????????????????????????????
                    //paymentOrder.DebitAccount.Currency = Currency;    //   ??????????????????????????????

                    paymentOrder.Type = OrderType.TransitNonCashOut;    //   TransitNonCashOut =   85  =  Տարանցիկ հաշվից անկանխիկ ելք   
                    paymentOrder.TransferID = bankMailID;
                    paymentOrder.ReceiverBankCode = ReceiverAccount.FilialCode;
                    paymentOrder.Quality = OrderQuality.Draft;
                    paymentOrder.OrderNumber = "";
                    paymentOrder.Description = "Փոխանցման հաստատում " + '(' + TransferSystemDescription + ' ' + Code + ')';

                    result = paymentOrder.SaveAndApprove(userName, source, user, schemaType);
                    R2APaymentDocID = paymentOrder.Id;
                    */

                    this.Id = 0;
                    this.RegistrationDate = DateTime.Now;

                    this.Type = OrderType.TransitNonCashOut;    //   TransitNonCashOut =   85  =  Տարանցիկ հաշվից անկանխիկ ելք   
                    this.TransferID = bankMailID;
                    this.ReceiverBankCode = ReceiverAccount.FilialCode;
                    this.Quality = OrderQuality.Draft;
                    this.OrderNumber = "";
                    this.Description = "Փոխանցման հաստատում " + '(' + TransferSystemDescription + ' ' + Code + ')';

                    result = base.SaveAndApprove(userName, source, user, schemaType);
                    R2APaymentDocID = result.Id;
                }
                else            //    ՓՈԽԱՐԿՈւՄ
                {
                    this.Id = 0;
                    this.RegistrationDate = DateTime.Now;
                    this.Type = OrderType.TransitNonCashOutCurrencyExchangeOrder;    //   TransitNonCashOutCurrencyExchangeOrder =   82  =  Տարանցիկ հաշվից անկանխիկ ելք փոխարկումով    
                    this.DebitAccount = Account.GetSystemAccount(transferDebitAccount);
                    this.OrderNumber = GenerateNewOrderNumber(OrderNumberTypes.Convertation, FilialCode).ToString();    //  22000 ?????????????????????
                    this.TransferID = bankMailID;
                    this.ReceiverBankCode = ReceiverAccount.FilialCode;
                    this.Quality = OrderQuality.Draft;

                    if (SendCurrencyCode == ReceiverAccount.Currency)
                    {
                        Currency = SendCurrencyCode;
                    }
                    else
                    {
                        SetCurrency();
                    }

                    SetConvertationDescription();
                    SetOrderSubType();


                    CurrencyExchangeOrder currencyExchangeOrder = new CurrencyExchangeOrder(this);

                    SetConvertationRates(currencyExchangeOrder);

                    currencyExchangeOrder.ConvertationRate = this.ConvertationRate;
                    currencyExchangeOrder.ConvertationRate1 = this.ConvertationRate1;

                    if (ConvertationTypeNum == 3)
                    {
                        currencyExchangeOrder.ConvertationCrossRate = Rate;
                    }


                    result = currencyExchangeOrder.SaveAndApprove(userName, source, user, schemaType);
                    R2ACurrencyExchangeDocID = currencyExchangeOrder.Id;
                }

                /* ///  ՎՃԱՐՎԱԾ ՓՈԽԱՆՑՄԱՆ ՏՎՅԱԼՆԵՐԻ ՎԵՐԱԴԱՐՁ ՍՏԱԿ ՀԱՄԱԿԱՐԳԻՆ  ///  */

                if (result.ResultCode == ResultCode.Normal)
                {
                    response.R2ARequestOutput = STAKPaymentOrderDB.GetR2ARequestOutput(this);

                    response.ActionResult.ResultCode = ResultCode.Normal;
                }
                else
                {
                    response.ActionResult.Errors = result.Errors;
                    response.ActionResult.ResultCode = ResultCode.Failed;

                    return response;
                }
            }

            return response;
        }


        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected new void Complete()
        {

            if (ushort.Parse(R2ADetails.PayoutDeliveryCode) == (ushort)PayoutDeliveryCodeForR2A.Card)
            {
                ReceiverCardNumber = Card.GetCard(this.R2ADetails.AccountNo);   // ToDoSTAK  poxel, GetCardWithoutBallance -ov stugel 

                if (ReceiverCardNumber != null)
                {
                    ReceiverAccount = ReceiverCardNumber.CardAccount;
                    CustomerNumber = ReceiverAccount.GetAccountCustomerNumber();
                }
            }
            else if (ushort.Parse(R2ADetails.PayoutDeliveryCode) == (ushort)PayoutDeliveryCodeForR2A.Account)
            {
                ReceiverAccount = Account.GetAccount(this.R2ADetails.AccountNo);

                if (ReceiverAccount != null)
                {
                    CustomerNumber = ReceiverAccount.GetAccountCustomerNumber();
                }
            }


            if (CustomerNumber != 0)
            {
                //if customerType == 6
                PhysicalCustomer physicalCustomer = (PhysicalCustomer)ACBAOperationService.GetCustomer(CustomerNumber);
                ReceiverPassport = Utility.ConvertAnsiToUnicode(physicalCustomer.DefaultDocument);

                FilialCode = Convert.ToUInt16(physicalCustomer.filial.key);

                ReceiverCustomerType = 6;

                //if (SendCurrencyCode == ReceiverAccount.Currency)
                //{
                //    Currency = SendCurrencyCode;
                //}
                //else
                //{
                //    SetCurrency();
                //}

                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            }

            //this.FeeAcba = Utility.RoundAmount(Convert.ToDouble(R2A.BeneficiaryFee), R2A.BeneficiaryFeeCurrencyCode);
            if (this.Country == "51")   // Եթե փոխանցումը ՀՀ տարածքից է
            {
                if (SendCurrencyCode == "AMD")
                {
                    this.FeeAcba = Utility.RoundAmount(Convert.ToDouble(R2ADetails.BeneficiaryFee), "AMD");
                    ACBACommissionCurrency = "AMD";
                }
                else
                {
                    this.FeeAcba = Utility.RoundAmount(Convert.ToDouble(R2ADetails.BeneficiaryFee), "AMD");
                    ACBACommissionCurrency = "AMD";
                }
            }
            else
            {
                //  Քանի որ 9211 ԾԱ-ի մեջ գրված էր միջնորդավճարը գանձել փոխանցման արժույթից կախված՝ 
                // գումարը որոշել եմ ըստ SendCurrencyCode, ոչ թե ըստ R2ADetails.BeneficiaryFeeCurrencyCode-ի
                this.FeeAcba = Utility.RoundAmount(Convert.ToDouble(R2ADetails.BeneficiaryFee), this.SendCurrencyCode);
                ACBACommissionCurrency = this.SendCurrencyCode;
            }


            Description = "Փոխանցում STAK համակարգով";
            TransferSystemDescription = "STAK";

            //TransferAdditionalData
        }


        /// <summary>
        /// Վճարման հանձնարարականի ստուգում ուղարկելուց
        /// </summary>
        /// <returns></returns>
        private ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            short notStrictFreezeChecking = 0;
            short notStrictDAHKChecking = 0;
            short notStrictDebtType = 1;

            result = this.ReceiverAccount.CreditAccountValidation(notStrictFreezeChecking, notStrictDAHKChecking, notStrictDebtType);

            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }

            if (this.ReceiverAccount.CheckStateRevenueCommitteeArrest())
            {
                //Հաշվին առկա է ՊԵԿ գրություն` արգելանք։
                result.Errors.Add(new ActionError(1875));
                return result;
            }

            result = CheckWithCriminalLists(this);

            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }


            return result;
        }


        private List<ActionError> ValidateBeforeComplete()
        {
            List<ActionError> result = new List<ActionError>();

            if (string.IsNullOrEmpty(this.R2ADetails.PayoutDeliveryCode))
            {
                //Փոխանցման ստացման տարբերակը մուտքագրված չէ։
                result.Add(new ActionError(1876));
            }

            if (string.IsNullOrEmpty(R2ADetails.AccountNo) || R2ADetails.AccountNo == "0")
            {
                /// Ստացված փոխանցման մեջ բացակայում է ստացողի հաշիվը։
                result.Add(new ActionError(1746));
            }

            //if (R2ADetails.AccountNo.Length > 34)
            //{
            //    /// ՍՏԱԿ համակարգով փոխանցված գումարը ստացողի հաշիվը մուտքագրված չէ:
            //    result.Add(new ActionError(1746));
            //}

            if (string.IsNullOrEmpty(R2ADetails.SenderCountryCode))
            {
                /// R2A փոխանցման ուղարկողի երկրի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1761));
            }

            if (string.IsNullOrEmpty(this.Country))
            {
                /// R2A փոխանցման ուղարկողի երկրի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1761));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryFee))
            {
                /// R2A փոխանցման ստացողի միջնորդավճարը մուտքագրված չէ։
                result.Add(new ActionError(1757));
            }

            if (string.IsNullOrEmpty(this.SendCurrencyCode))
            {
                //R2A փոխանցման արժույթը մուտքագրված չէ:
                result.Add(new ActionError(1878));
            }

            return result;
        }


        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            result.Errors.AddRange(Validation.ValidateCustomerSignature(this.CustomerNumber));
            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            //result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));

            result.Errors.AddRange(ValidateR2AAccount());

            if (result.Errors.Count > 0)
            {
                return result;
            }


            result.Errors.AddRange(ValidateData());

            result.Errors.AddRange(ValidateBeneficiary());

            if (this.Quality != OrderQuality.Draft)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }

            if (this.TransferAdditionalData != null)
            {
                result.Errors.AddRange(Validation.ValidateTransferAdditionalData(this.TransferAdditionalData, this.Amount));
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }


            result.Errors.AddRange(Validation.ValidateURN(this.Code));

            // Էլեն Բակլաչևան ասեց այս փուլում ստուգման կարիք չկա
            // Ստացողի փաստաթղթերի ստուգում
            //result.Errors.AddRange(Validation.ValidateCustomerDocument(CustomerNumber));

            //if (!Validation.ValidateR2AAccount(this.R2A.AccountNo))
            //{
            //    /// ՍՏԱԿ համակարգով փոխանցված գումարը ստացողի հաշիվը ԱԿԲԱ բանկի հաշիվ/քարտի համար չէ։
            //    result.Errors.Add(new ActionError(1747));
            //}


            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            return result;
        }

        private List<ActionError> ValidateBeneficiary()
        {
            List<ActionError> result = new List<ActionError>();
            short customerType;
            string nameENG;
            string lastNameENG;

            ACBAServiceReference.Customer customer = ACBAOperationService.GetCustomer(CustomerNumber);

            customerType = customer.customerType.key;


            if (customerType == 6)
            {
                PhysicalCustomer physicalCustomer = (PhysicalCustomer)customer;
                nameENG = physicalCustomer.person.fullName.firstNameEng;
                lastNameENG = physicalCustomer.person.fullName.lastNameEng;

                if (R2ADetails.BeneficiaryFirstName != nameENG || R2ADetails.BeneficiaryLastName != lastNameENG)
                {
                    //Փոխանցման տվյալներում ստացողի անգլերեն անունը կամ ազգանունը չի համապատասխանում հաճախորդի՝ բանկում գրանցած անվան կամ ազգանվան հետ:
                    result.Add(new ActionError(1831));
                }
            }

            return result;
        }


        /// Ստուգում է մուտքային տվյալները
        /// <returns></returns>
        private List<ActionError> ValidateData()
        {
            List<ActionError> result = new List<ActionError>();

            if (ReceiverCustomerType != 6)
            {
                //Փոխանցման ստացողը ֆիզիկական անձ չէ։
                result.Add(new ActionError(1877));
            }

            if (string.IsNullOrEmpty(this.Currency))
            {
                //Արժույթը ընտրված չէ:
                result.Add(new ActionError(254));
            }

            if (this.Currency != "RUR" && this.Currency != "USD" && this.Currency != "EUR" && this.Currency != "AMD")
            {
                //ՍՏԱԿ համակարգի միջոցով նշված արժույթով փոխանցումներ չեն իրականացվում։
                result.Add(new ActionError(1860));
            }

            if (this.Amount <= 0.01)
            {
                //Մուտքագրված գումարը սխալ է:
                result.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                //Գումարը սխալ է մուտքագրած:
                result.Add(new ActionError(25));
            }

            if (this.Currency == "RUR")
            {
                if (this.Amount < 100 || this.Amount > 650000)
                {
                    //ՍՏԱԿ համակարգով փոխանցված գումարը պետք է լինի 100 RUR-ից 650․000 RUR միջակայքում։
                    result.Add(new ActionError(1861));
                }
            }
            else if (this.Currency == "USD")
            {
                if (this.Amount < 2 || this.Amount > 10000)
                {
                    //ՍՏԱԿ համակարգով փոխանցված գումարը պետք է լինի 2 USD-ից 10․000 USD միջակայքում։
                    result.Add(new ActionError(1862));
                }
            }
            else if (this.Currency == "EUR")
            {
                if (this.Amount < 2 || this.Amount > 10000)
                {
                    //ՍՏԱԿ համակարգով փոխանցված գումարը պետք է լինի 2 EUR-ից 10․000 EUR միջակայքում։
                    result.Add(new ActionError(1863));
                }
            }
            else if (this.Currency == "AMD")
            {
                if (this.Amount < 1000 || this.Amount > 5000000)
                {
                    //ՍՏԱԿ համակարգով փոխանցված գումարը պետք է լինի 1․000 AMD-ից 5․000․000 AMD միջակայքում։
                    result.Add(new ActionError(1864));
                }
            }

            if (this.Description == null || this.Description == "")
            {
                //Վճարման նպատակը մուտքագրված չէ:
                result.Add(new ActionError(645));
            }
            else if (Description.Length > 150)
            {
                //«Վճարման նպատակ» դաշտի առավելագույն նիշերի քանակն է 150:
                result.Add(new ActionError(646));
            }

            if (CustomerNumber == 0)
            {
                //Հաճախորդը գտնված չէ:
                result.Add(new ActionError(771));
            }

            if (FilialCode == 0)
            {
                //Հաճախորդի մասնաճյուղը որոշված չէ:
                result.Add(new ActionError(1874));
            }



            //Todo STAK-i R2A - i depqum petq e ??????? Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
            //if (Customer.IsCustomerUpdateExpired(this.CustomerNumber))
            //{
            //    //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
            //    result.Add(new ActionError(765, new string[] { this.CustomerNumber.ToString() }));
            //    return result;
            //}



            if (this.FeeAcba == 0)
            {
                //ACBA միջնորդավճարը բացակայում է
                result.Add(new ActionError(709));
            }


            if (!String.IsNullOrEmpty(this.Country))
            {
                //Ռիսկային երկրների ստուգում
                if (this.Country == "364" || this.Country == "760" || this.Country == "729" || this.Country == "728")
                {
                    //Ստացողի երկիրը ռիսկային է (ԻՐԱՆ, ՍԻՐԻԱ, ՍՈՒԴԱՆ, ՀԱՐԱՎԱՅԻՆ ՍՈՒԴԱՆ)
                    result.Add(new ActionError(626));
                }
            }


            if (this.SubType == 0)
            {
                //Փոխանցման համակարգը ընտրված չէ
                result.Add(new ActionError(778));
            }


            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryFirstName))
            {
                /// R2A փոխանցման ստացողի անունը մուտքագրված չէ։
                result.Add(new ActionError(1758));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryLastName))
            {
                /// R2A փոխանցման ստացողի ազգանունը մուտքագրված չէ։
                result.Add(new ActionError(1760));
            }


            if (string.IsNullOrEmpty(this.Receiver))
            {
                //Ստացողի տվյալները բացակայում են:
                result.Add(new ActionError(87));
            }
            else if (Utility.IsExistForbiddenCharacter(this.Receiver))
            {
                //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                result.Add(new ActionError(433));
            }



            //*********STAK Mandatory Fields*********//

            if (string.IsNullOrEmpty(R2ADetails.SendAgentCode))
            {
                /// R2A փոխանցման ուղարկողի Agent-ի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1749));
            }

            if (string.IsNullOrEmpty(this.Code))
            {
                /// R2A փոխանցման վճարման ունիկալ համարը մուտքագրված չէ։
                result.Add(new ActionError(1750));
            }

            if (string.IsNullOrEmpty(R2ADetails.BankCode))
            {
                /// R2A փոխանցման ստացողի Agent-ի կոդը(բանկը) մուտքագրված չէ։
                result.Add(new ActionError(1751));
            }

            // Ստացողի Agent-ի կոդ (պահանջվում է, երբ փոխանցման ստացման տեսակը` PayoutDeliveryCode - ը, կանխիկ (Cash) չէ)
            if (R2ADetails.PayoutDeliveryCode != PayoutDeliveryCodeForR2A.Cash.ToString() && string.IsNullOrEmpty(R2ADetails.BeneficiaryAgentCode))
            {
                /// R2A փոխանցման ստացողի Agent-ի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1754));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryCountryCode))
            {
                /// R2A փոխանցման ստացողի երկրի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1752));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryFeeCurrencyCode))
            {
                /// R2A փոխանցման միջնորդավճարի արժույթի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1753));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderLastName))
            {
                /// R2A փոխանցման ուղարկողի անգլերեն ազգանունը մուտքագրված չէ։
                result.Add(new ActionError(1756));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderFirstName))
            {
                /// R2A փոխանցման ուղարկողի անգլերեն անունը մուտքագրված չէ։
                result.Add(new ActionError(1759));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderDocumentTypeCode))
            {
                /// R2A փոխանցման ուղարկողի փաստաթղթի տեսակի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1762));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderIssueDate))
            {
                /// R2A փոխանցման ուղարկողի ID համարի տրման ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1763));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderExpirationDate))
            {
                /// R2A փոխանցման ուղարկողի ID համարի վավերականության ժամկետը մուտքագրված չէ։
                result.Add(new ActionError(1764));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderIssueCountryCode))
            {
                /// R2A փոխանցման ուղարկողի փաստաթղթի թողարկման երկրի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1765));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderIssueIDNo))
            {
                /// R2A փոխանցման ուղարկողի ID-ի համարը մուտքագրված չէ։
                result.Add(new ActionError(1766));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderBirthDate))
            {
                /// R2A փոխանցման ուղարկողի ծննդյան ամսաթիվը մուտքագրված չէ։
                result.Add(new ActionError(1767));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderSexCode))
            {
                /// R2A փոխանցման ուղարկողի սեռի կոդը մուտքագրված չէ։
                result.Add(new ActionError(1768));
            }

            if (string.IsNullOrEmpty(R2ADetails.SenderMobileNo))
            {
                /// R2A փոխանցման ուղարկողի բջջային հեռախոսահամարը մուտքագրված չէ։
                result.Add(new ActionError(1769));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryLastName))
            {
                /// R2A փոխանցման ստացողի անգլերեն ազգանունը մուտքագրված չէ։
                result.Add(new ActionError(1772));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryFirstName))
            {
                /// R2A փոխանցման ստացողի անգլերեն անունը մուտքագրված չէ։
                result.Add(new ActionError(1773));
            }

            if (string.IsNullOrEmpty(R2ADetails.BeneficiaryMobileNo))
            {
                /// R2A փոխանցման ստացողի բջջային հեռախոսահամարը մուտքագրված չէ։
                result.Add(new ActionError(1748));
            }


            return result;
        }

        private List<string> GetCustomerPassport(ulong customerNumber)
        {
            List<string> result = new List<string>();
            DataTable dt = Info.GetCustomerAllPassports(customerNumber);

            foreach (DataRow item in dt.Rows)
            {
                string documentGivenDate = Convert.ToDateTime(item["document_given_date"]).ToString("dd/MM/yyyy");
                result.Add(item["document_number"].ToString() + ", " + item["document_given_by"].ToString() + ", " + documentGivenDate);
            }
            return result;
        }

        private List<ActionError> ValidateR2AAccount()
        {
            List<ActionError> result = new List<ActionError>();

            if (ushort.Parse(R2ADetails.PayoutDeliveryCode) == (ushort)PayoutDeliveryCodeForR2A.Card && ReceiverCardNumber == null)
            {
                // 1747 = "ԱԿԲԱ բանկում նշված համարի քարտ չկա:"
                result.Add(new ActionError(1747));
            }
            else if (ushort.Parse(R2ADetails.PayoutDeliveryCode) == (ushort)PayoutDeliveryCodeForR2A.Account && (ReceiverAccount == null || ReceiverAccount.ClosingDate != null))
            {
                // 1836 = "ԱԿԲԱ բանկում նման բանկային հաշիվ առկա չէ։"
                result.Add(new ActionError(1836));
            }

            return result;
        }

        private void SetConvertationDescription()
        {
            //string strForTransfer = ' ' + Receiver + " (STAK " + Code + ')';  // Front-ից այսպես է որոշում
            //string strForTransfer = " ուղարկող` " + this.Sender;  // + " (STAK code " + Code + ')';

            /*  Dictionary<string, string> operationsListDescriptions = Info.GetOperationsList(Languages.hy);
              string description = "";
              string additional = "";

              if (Currency != "AMD")
              {
                  if (ReceiverAccount.Currency != "AMD")
                  {
                      //Արտարժույթի առք ու վաճառք
                      description = operationsListDescriptions["17"];
                      additional = Currency + "/" + ReceiverAccount.Currency + strForTransfer;
                  }
                  else
                  {
                      //Արտարժույթի առք (Բանկի տեսանկյունից)                          
                      description = operationsListDescriptions["16"];
                      additional = Currency + strForTransfer;
                  }
              }
              else
              {
                  if (ReceiverAccount.Currency != "AMD")
                  {
                      //Արտարժույթի վաճառք (Բանկի տեսանկյունից)
                      description = operationsListDescriptions["17"];
                      additional = ReceiverAccount.Currency + strForTransfer;
                  }
              }

              Description = description + additional != "" ? " " + additional : "";  */

            int length = this.Sender.Length;

            if (length > 20)
            {
                length = 20;
            }

            Description = " ՍՏԱԿ համ, " + this.Amount.ToString() + this.SendCurrencyCode + ", " + this.Sender.Substring(0, length) + ", ";  // " ՍՏԱԿ համ, " + 

            //Description = strForTransfer + " ՍՏԱԿ-ով ստ. գումար " + this.Amount.ToString() + ' ' + this.SendCurrencyCode
            //                                + ", փոխարժեք ";

        }

        private void SetOrderSubType()
        {
            if (Currency == "AMD" && ReceiverAccount.Currency != "AMD")
            {
                SubType = 2;
            }
            else if (Currency != "AMD" && ReceiverAccount.Currency == "AMD")
            {
                SubType = 1;
            }
            else if (Currency != "AMD" && ReceiverAccount.Currency != "AMD")
            {
                SubType = 3;
            }
        }


        /// <summary>
        /// Դեբետ և կրեդիտ հաշիվներից կախված որոշվում է հայտի արժույթը
        /// </summary>
        private void SetCurrency()
        {
            if (SendCurrencyCode != null && ReceiverAccount.Currency != null)
            {
                if (SendCurrencyCode != ReceiverAccount.Currency)
                {
                    if (SendCurrencyCode == "AMD")
                    {
                        if (ReceiverAccount.Currency != "AMD")
                        {
                            Currency = ReceiverAccount.Currency;
                        }
                        else
                        {
                            Currency = "";      //  SendCurrencyCode  և  ReceiverAccount.Currency = "AMD"
                        }
                    }
                    else
                    {
                        Currency = SendCurrencyCode;
                    }
                }
                else
                {
                    Currency = SendCurrencyCode;
                }
            }

            //SetConvertationRates();

        }

        /// <summary>
        /// Նշվում է արժույթի/ների կուրսերը և փոխարկման տեսակը
        /// </summary>
        private void SetConvertationRates(CurrencyExchangeOrder currencyExchangeOrder)
        {
            if (SendCurrencyCode != null && ReceiverAccount.Currency != null && SendCurrencyCode != ReceiverAccount.Currency)
            {
                if (SendCurrencyCode == "AMD" || ReceiverAccount.Currency == "AMD")
                {
                    if (SendCurrencyCode != "AMD")
                    {
                        //NonCash Buy,  RateType.NonCash = 2,   ExchangeDirection.Buy = 2
                        GetLastRates(currencyExchangeOrder, SendCurrencyCode, RateType.NonCash, ExchangeDirection.Buy, FilialCode);
                    }
                    else
                    {
                        // NonCash Sale,  RateType.NonCash = 2,   ExchangeDirection.Sell = 1
                        GetLastRates(currencyExchangeOrder, ReceiverAccount.Currency, RateType.NonCash, ExchangeDirection.Sell, FilialCode);
                    }
                }
                else
                    if (SendCurrencyCode != ReceiverAccount.Currency)
                {
                    CalculateCrossRate(currencyExchangeOrder, SendCurrencyCode, ReceiverAccount.Currency);
                }
            }
        }

        //  Հաշվարկում է գործող կուրսը և թարմացվում գումարը
        private void GetLastRates(CurrencyExchangeOrder currencyExchangeOrder, string currency, RateType rateType, ExchangeDirection direction, ushort filialCode)
        {
            //result = utilityService.getLastRates(currency, rateType, direction);
            double result = Utility.GetLastExchangeRate(currency, rateType, direction, filialCode);

            //if (result == 0)
            //{
            //    return ShowMessage('Տվյալ արտարժույթով փոխարկում հնարավոր չէ:', 'error');
            //}

            this.Rate = Math.Round(result * 100) / 100;
            ConvertationRate = Math.Round(result * 100) / 100;

            ConvertationTypeNum = (short)direction;

            if ((short)direction == 1)
                ConvertationType = "Վաճառք";  //2
            else if ((short)direction == 2)
                ConvertationType = "Գնում";  //1

            //$scope.ConvertationTypeNum = direction;

            currencyExchangeOrder.CalculateConvertationAmount(Rate, ConvertationTypeNum, ConvertationType, CrossVariant);
        }

        private void CalculateCrossRate(CurrencyExchangeOrder currencyExchangeOrder, string debitCurrency, string creditCurrency)
        {
            double dRate;
            double cRate;

            CrossVariant = CurrencyExchangeOrder.GetCrossConvertationVariant(debitCurrency, creditCurrency);

            // Cross Buy,  RateType.Cross = 5, ExchangeDirection.Buy = 2
            dRate = Utility.GetLastExchangeRate(debitCurrency, RateType.Cross, ExchangeDirection.Buy, this.FilialCode);

            ConvertationRate = Math.Round(dRate * 100) / 100;

            // Cross Sell,  RateType.Cross = 5, ExchangeDirection.Sell = 1
            cRate = Utility.GetLastExchangeRate(creditCurrency, RateType.Cross, ExchangeDirection.Sell, this.FilialCode);

            ConvertationRate1 = Math.Round(cRate * 100) / 100;

            if (CrossVariant == 1)
            {
                //$scope.order.Rate = dRate / cRate;
                this.Rate = dRate / cRate;
            }
            else
            {
                //$scope.order.Rate = parseFloat(cRate / dRate);
                this.Rate = cRate / dRate;
            }

            this.Rate = Math.Round((this.Rate + 0.00000001) * 10000000) / 10000000;

            ConvertationType = "Կրկնակի փոխարկում";  //3
            ConvertationTypeNum = 3;

            currencyExchangeOrder.Amount = this.Amount;
            currencyExchangeOrder.ConvertationRate = this.ConvertationRate;
            currencyExchangeOrder.ConvertationRate1 = this.ConvertationRate1;

            currencyExchangeOrder.CalculateConvertationAmount(Rate, ConvertationTypeNum, ConvertationType, CrossVariant);
        }

        private ActionResult CheckWithCriminalLists(STAKPaymentOrder order)
        {
            return STAKPaymentOrderDB.CheckWithCriminalLists(order);
        }

        private List<ActionError> ValidateDataLength()
        {
            List<ActionError> result = new List<ActionError>();

            if (ReceiverCustomerType != 6)
            {
                //Փոխանցման ստացողը ֆիզիկական անձ չէ։
                result.Add(new ActionError(1877));
            }




            return result;
        }

        public void SetCountry(string senderCountryCode)
        {
            if (!String.IsNullOrEmpty(senderCountryCode))
            {
                DataTable dt = Info.GetCountries();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["CountryCodeA3"].ToString() == senderCountryCode)
                    {
                        this.Country = dt.Rows[i]["CountryCodeN"].ToString();
                        break;
                    }
                }
            }
        }

        internal void SaveValidationErrors(short errorCode, string errorDescription, string guid)
        {
            STAKPaymentOrderDB.SaveValidationErrors(this, errorCode, errorDescription, guid);
        }


    }
}

