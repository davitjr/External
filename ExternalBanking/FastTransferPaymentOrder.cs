using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.ARUSDataService;
using System.Data;
using System.Globalization;



namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class FastTransferPaymentOrder : PaymentOrder
    {

        /// <summary>
        /// Ուղարկողի անուն/անվանում
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Ուղարկողի հասցե
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Ուղարկողի անձնագիր
        /// </summary>
        public string SenderPassport { get; set; }

        /// <summary>
        /// Ուղարկողի ծննդյան ա/թ
        /// </summary>
        public DateTime SenderDateOfBirth { get; set; }

         /// <summary>
        /// Ուղարկողի էլ. հասցե
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Ուղարկողի հեռախոս
        /// </summary>
        public string SenderPhone { get; set; }

        /// <summary>
        /// Ուղարկող հաճախորդի տեսակ
        /// </summary>
        public short SenderType { get; set; }

        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverAddInf { get; set; }
       
        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverPassport  { get; set; }
 
        /// <summary>
        /// Վճարման մանրամասներ
        /// </summary>
        public string DescriptionForPayment { get; set; }

        /// <summary>
        /// Երկիր
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Թվային կոդ
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Երկրի անվանում
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Միջնորդավճար արժույթով
        /// </summary>
        public double  Fee { get; set; }

        /// <summary>
        /// Միջնորդավհար ACBA արժույթով
        /// </summary>
        public double FeeAcba { get; set; }

        /// <summary>
        /// Փոխանցման համակարգ
        /// </summary>
        public string TransferSystemDescription { get; set; }

        /// <summary>
        /// Փոխանցման լրացուցիչ տվյալներ
        /// </summary>
        public TransferAdditionalData TransferAdditionalData { get; set; }

        //***************ARUS համակարգի համար անհրաժեշտ դաշտեր**************************************
        /// <summary>
        /// ARUS-ի կոդ կամ հատուկ MTO(Դրամական Փոխանցման Օպերատոր)-ի կոդ
        /// </summary>
        public string MTOAgentCode { get; set; }

        /// <summary>
        /// Փոխանցման ստացման տեսակի կոդ
        /// </summary>
        public string PayoutDeliveryCode { get; set; }

        /// <summary>
        ///Ստացողի Հաշվեհամար (պահանջվում է, երբ փոխանցման ստացման տեսակը` PayoutDeliveryCode, կանխիկ է՝ Cash)
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// Ստացող Agent-ի կոդ (պահանջվում է, երբ փոխանցման ստացման տեսակը` PayoutDeliveryCode, կանխիկ չէ)
        /// </summary>
        public string BeneficiaryAgentCode { get; set; }


        /// <summary>
        ///Ստացողի մարզի կոդ
        /// </summary>
        public string BeneficiaryStateCode { get; set; }

        /// <summary>
        ///Ստացողի քաղաքի կոդ
        /// </summary>
        public string BeneficiaryCityCode { get; set; }


        /// <summary>
        /// Փոխանցման ստացումն իրականացնող Agent-ի կոդ 
        /// </summary>
        public string PayOutAgentCode { get; set; }

        /// <summary>
        /// Ուղարկողի ազգանուն անգլերենով
        /// </summary>
        public string SenderLastName { get; set; }

        /// <summary>
        /// Ուղարկողի հայրանուն անգլերենով
        /// </summary>
        public string SenderMiddleName { get; set; }

        /// <summary>
        /// Ուղարկողի անուն անգլերենով
        /// </summary>
        public string SenderFirstName { get; set; }

        /// <summary>
        /// Ուղարկողի ազգանուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով 
        /// </summary>
        public string NATSenderLastName { get; set; }

        /// <summary>
        /// Ուղարկողի հայրանուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderMiddleName { get; set; }

        /// <summary>
        /// Ուղարկողի անուն` տվյալ երկրի օրենսդրությամբ սահմանված լեզվով
        /// </summary>
        public string NATSenderFirstName { get; set; }


        /// <summary>
        /// Ուղարկողի երկրի կոդ
        /// </summary>
        public string SenderCountryCode { get; set; }

        /// <summary>
        /// Ուղարկողի մարզի կոդ
        /// </summary>
        public string SenderStateCode { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաքի կոդ
        /// </summary>
        public string SenderCityCode { get; set; }

        /// <summary>
        /// Ուղարկողի փոստային կոդ
        /// </summary>
        public string SenderZipCode { get; set; }

        /// <summary>
        /// Ուղարկողի զբաղվածության անվանում
        /// </summary>
        public string SenderOccupationName { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի տեսակի կոդ
        /// </summary>
        public string SenderDocumentTypeCode { get; set; }

        /// <summary>
        /// Ուղարկողի ID համարի տրման ամսաթիվ
        /// </summary>
        public string SenderIssueDate { get; set; }

        /// <summary>
        /// Ամսաթիվ՝ ցույց տվող, թե մինչև երբ է ուժի մեջ Ուղարկողի ID համարը 
        /// </summary>
        public string SenderExpirationDate { get; set; }

        /// <summary>
        /// Երկրի կոդը, որի կողմից որ թողարկվել է Ուղարկողի փաստաթուղթը
        /// </summary>
        public string SenderIssueCountryCode { get; set; }

        /// <summary>
        /// Քաղաքի կոդը, որի կողմից որ թողարկվել է Ուղարկողի փաստաթուղթը
        /// </summary>
        public string SenderIssueCityCode { get; set; }

        /// <summary>
        ///Ուղարկողի ID համարը
        /// </summary>
        public string SenderIssueIDNo { get; set; }

        /// <summary>
        ///Ուղարկողի ծննդավայր
        /// </summary>
        public string SenderBirthPlaceName { get; set; }

        /// <summary>
        ///Ուղարկողի սեռի կոդ
        /// </summary>
        public string SenderSexCode { get; set; }

        /// <summary>
        ///Ուղարկողի բջջային հեռախոսահամար
        /// </summary>
        public string SenderMobileNo { get; set; }

        /// <summary>
        ///Ստացողի ազգանուն
        /// </summary>
        public string NATBeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն
        /// </summary>
        public string NATBeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն
        /// </summary>
        public string NATBeneficiaryFirstName { get; set; }

        /// <summary>
        ///Ստացողի ազգանուն անգլերենով
        /// </summary>
        public string BeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն անգլերենով
        /// </summary>
        public string BeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն անգլերենով
        /// </summary>
        public string BeneficiaryFirstName { get; set; }

        /// <summary>
        ///Ստացողի հասցե
        /// </summary>
        public string BeneficiaryAddress { get; set; }

        /// <summary>
        /// Ստացողի էլեկտրոնային հասցե
        /// </summary>
        public string BeneficiaryEMailName { get; set; }

        /// <summary>
        ///Ստացողի հեռախոսահամար
        /// </summary>
        public string BeneficiaryPhoneNo { get; set; }

        /// <summary>
        ///Ստացողի բջջային հեռախոսահամար
        /// </summary>
        public string BeneficiaryMobileNo { get; set; }

        /// <summary>
        ///Ստուգիչ հարց
        /// </summary>
        public string ControlQuestionName { get; set; }

        /// <summary>
        ///Ստուգիչ հարցի պատասխան
        /// </summary>
        public string ControlAnswerName { get; set; }

        /// <summary>
        /// Փոխանցման նպատակ
        /// </summary>
        public string PurposeRemittanceCode { get; set; }

        /// <summary>
        /// Promo կոդ 
        /// </summary>
        public string PromotionCode { get; set; }

        /// <summary>
        /// Նշում
        /// </summary>
        public string DestinationText { get; set; }

        /// <summary>
        /// Ուղարկողի ռեզիդենտություն
        /// </summary>
        public string SenderResidencyCode { get; set; }

        /// <summary>
        /// Ուղարկողի ծննդյան երկիր
        /// </summary>
        public string SenderBirthCountryCode { get; set; }

        /// <summary>
        /// Ուղարկողի ֆիսկալ կոդ
        /// </summary>
        public string SenderFiscalCode { get; set; }

        /// <summary>
        /// ԿԲ—ի կողմից հայտարարված փոխարժեք՝ միջնորդավճարը ՀՀ դրամի փոխարկելու համար  (######.######)
        /// ՍՏԱԿ համակարգի հաշվարկային բանկի փոխարժեքը ՀՀ դրամով
        /// </summary>
        public double SettlementExchangeRate { get; set; }

        //***************************************ARUS դաշտեր ավարտ*******************************************************************

        //****************************ARUS-ի հետ կապված դաշտեր՝ ավելացրած ACBA-ի կողմից

        /// <summary>
        /// Դրամական Փոխանցման Օպերատորի անվանում
        /// </summary>
        public string MTOAgentName { get; set; }

        /// <summary>
        /// Փոխանցման ստացման տեսակի անվանում
        /// </summary>
        public string PayoutDeliveryName { get; set; }

        /// <summary>
        /// Փոխանցման նպատակի անվանում
        /// </summary>
        public string PurposeRemittanceName { get; set; }

        /// <summary>
        /// Ուղարկողի երկիր
        /// </summary>
        public string SenderCountryName { get; set; }

        /// <summary>
        /// Ուղարկողի մարզի անվանում
        /// </summary>
        public string SenderStateName { get; set; }

        /// <summary>
        /// Ուղարկողի քաղաքի անվանում
        /// </summary>
        public string SenderCityName { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի տեսակի անվանում
        /// </summary>
        public string SenderDocumentTypeName { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի թողարկման երկրի անվանում
        /// </summary>
        public string SenderIssueCountryName { get; set; }

        /// <summary>
        /// Ուղարկողի Փաստաթղթի թողարկման քաղաքի անվանում
        /// </summary>
        public string SenderIssueCityName { get; set; }

        /// <summary>
        /// Ուղարկողի Ծննդյան երկրի անվանում
        /// </summary>
        public string SenderBirthCountryName { get; set; }

        /// <summary>
        /// Ուղարկողի սեռի անվանում
        /// </summary>
        public string SenderSexName { get; set; }

        /// <summary>
        /// Ուղարկողի ռեզիդենտություն
        /// </summary>
        public string SenderResidencyName { get; set; }

        /// <summary>
        /// Ստացողի մարզի անվանում
        /// </summary>
        public string BeneficiaryStateName { get; set; }

        /// <summary>
        /// Ստացողի քաղաքի անվանում
        /// </summary>
        public string BeneficiaryCityName { get; set; }

        /// <summary>
        /// Ստացողի գործակալի անվանում
        /// </summary>
        public string BeneficiaryAgentName { get; set; }

        /// <summary>
        /// Ցույց է տալիս՝ արդյոք փոխանցման ուղարկումը հաջողությամբ կատարվել է ARUS համակարգի կողմից, թե ոչ (1` բարեհաջող կատարում, 0՝ անհաջող ավարտ)
        /// </summary>
        public short ARUSSuccess { get; set; }

        /// <summary>
        /// ARUS համակարգի կողմից ստացված սխալի հաղորդագրություն
        /// </summary>
        public string ARUSErrorMessage { get; set; }


        //***************************ACBA դաշտեր ավարտ************************************

        public new void Get(string userName, string authorizedUserSessionToken, string clientIP)
        {
            FastTransferPaymentOrderDB.Get(this);
            this.Fees = Order.GetOrderFees(this.Id);
 
            if (this.Fees.Exists(m => m.Type == 5))
            {
                this.TransferFee = this.Fees.Find(m =>  m.Type == 5).Amount;
                this.FeeAccount = this.Fees.Find(m => m.Type == 5).Account;
            }

            if (this.Fees.Exists(m => m.Type == 7))
            {
                this.CardFee = this.Fees.Find(m => m.Type == 7).Amount;
                this.CardFeeCurrency = this.Fees.Find(m => m.Type == 7).Currency;

            }

            //ARUS համակարգի կողմից ստացված փոխանցումներ
            if (this.SubType == 23)
            {
                GetCodeNames(authorizedUserSessionToken, userName, clientIP);
            }

        }

        private void Get(FastTransferPaymentOrder  order)
        {
            FastTransferPaymentOrderDB.Get(order);
        }
        
                
        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public new ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.FilialCode = user.filialCode;
            this.Complete();
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = FastTransferPaymentOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Ուղարկման հայտի կատարում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="clientIP"></param>
        /// <returns></returns>
        public ActionResult Approve(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            ActionResult result = this.ValidateForApprove();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            SendMoneyRequestResponse ARUSRequestResponse = new SendMoneyRequestResponse();

            if (this.ARUSSuccess != 1)
            {

                SendMoneyInput sendMoneyInput = new SendMoneyInput();
                ARUSHelper.ConvertObject(this, ref sendMoneyInput);


                if (!String.IsNullOrEmpty(Country))
                {

                    DataTable dt = Info.GetCountries();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["CountryCodeN"].ToString() == this.Country)
                        {
                            sendMoneyInput.BeneficiaryCountryCode = dt.Rows[i]["CountryCodeA3"].ToString();
                            break;
                        }
                    }
                }

                sendMoneyInput.SendAmount = (decimal)this.Amount;
                sendMoneyInput.SendCurrencyCode = this.Currency;
                sendMoneyInput.SenderAddressName = this.SenderAddress;
                sendMoneyInput.SenderBirthDate = String.Format("{0:yyyyMMdd}", SenderDateOfBirth);
                sendMoneyInput.SenderEMailName = this.SenderEmail;
                sendMoneyInput.SenderPhoneNo = this.SenderPhone;



                ARUSHelper.Use(client =>
                {
                    ARUSRequestResponse = client.SendMoneyOperation(sendMoneyInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), this.Id.ToString());
                }, authorizedUserSessionToken, userName, "clientIP");

            }
            else
            {
                ARUSRequestResponse.ActionResult = new ARUSDataService.ActionResult();
                ARUSRequestResponse.ActionResult.ResultCode = ARUSDataService.ResultCode.Normal;
                ARUSRequestResponse.SendMoneyOutput = new SendMoneyOutput();
                ARUSRequestResponse.SendMoneyOutput.URN = this.Code;
            }




            if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
            {
                result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                FastTransferPaymentOrderDB.UpdateARUSMessage(this.Id, result.Errors[0].Description);
                result.ResultCode = ResultCode.SavedNotConfirmed;
            }
            else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
            {
                FastTransferPaymentOrderDB.UpdateARUSSuccess(this.Id, 1, ARUSRequestResponse.SendMoneyOutput.URN);

                if (this.Source == SourceType.MobileBanking || (this.Source == SourceType.Bank && this.OPPerson == null))
                {
                    this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }


                result = base.Confirm(user);

                string infoDescription = Environment.NewLine + "Փոխանցման հսկիչ համար՝ " + ARUSRequestResponse.SendMoneyOutput.URN +
                                         Environment.NewLine + "Փոխանցման կարգավիճակ՝ " + ARUSRequestResponse.SendMoneyOutput.StatusCodeName;

                if (result.ResultCode == ResultCode.Normal || result.ResultCode == ResultCode.Failed)
                {
                    ActionError information = new ActionError();
                    information.Code = 0;
                    information.Description = infoDescription;

                    result.Errors.Add(information);
                }
                else   //SaveNotConfirmed
                {
                    result.Errors[0].Description += infoDescription;
                }
            }

            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType) 
        {

            this.Complete();
            ActionResult result = this.Validate();
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
                result = FastTransferPaymentOrderDB.Save(this, userName, source);
                if (this.TransferAdditionalData != null)
                {
                    this.TransferAdditionalData.OrderId = result.Id;
                    TransferAdditionalDataDB.Save(this.TransferAdditionalData);
                }

                 if (result.ResultCode != ResultCode.Normal)
                 {
                     return result;
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
                        return result;
                    }
                }

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

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

            result = base.Confirm(user);
           
            return result;
        }

 

        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected new void Complete()
        {
            this.ReceiverAccount = new Account();

            if (this.DebitAccount != null && this.Type == OrderType.FastTransferFromCustomerAccount )
            {
                this.DebitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
            }
            else if (this.Type == OrderType.FastTransferPaymentOrder)
            {
                //Տարանցիկ հաշվի լրացում
                this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), this.Currency, user.filialCode);
            }
            


            if (this.OrderNumber == "" && this.Id == 0)
                this.OrderNumber = GenerateNewOrderNumber(OrderNumberTypes.InternationalOrder,22000).ToString();

            Customer customer = new Customer(this.CustomerNumber, Languages.hy);

            if (this.Source == SourceType.MobileBanking || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }
            if (this.Type == OrderType.FastTransferPaymentOrder)
            {
                //Տարանցիկ հաշվի լրացում (Միջնորդավճարի)
                this.FeeAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", user.filialCode);
            }
           this.Fee = Utility.RoundAmount(this.Fee, this.Currency );
           this.FeeAcba = Utility.RoundAmount(this.FeeAcba, this.Currency);
           this.TransferFee = Utility.RoundAmount(this.TransferFee, "AMD");
           if (this.Type != OrderType.CashInternationalTransfer)
           {
               this.CardFee = customer.GetCardFee(this);
           }

            //ARUS
            if (this.SubType == 23)
            {
                if (!String.IsNullOrEmpty(this.Country))
                {
                    DataTable dt = Info.GetCountries();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["CountryCodeA3"].ToString() == this.Country)
                        {
                            this.Country = dt.Rows[i]["CountryCodeN"].ToString();
                            break;
                        }
                    }
                }

                DateTime outputDate;
                if (DateTime.TryParseExact(SenderIssueDate, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate))
                {
                    SenderIssueDate = String.Format("{0:yyyyMMdd}", outputDate);
                }

                if (DateTime.TryParseExact(SenderExpirationDate, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate))
                {
                    SenderExpirationDate = String.Format("{0:yyyyMMdd}", outputDate);
                }
            }
        }

        public static double GetFastTransferFeeAcbaPercent(byte transferType)
        {
            return FastTransferPaymentOrderDB.GetFastTransferFeeAcbaPercent(transferType);
        }

        
        /// <summary>
        /// Միջազգային վճարման հանձնարարականի ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (this.Type == OrderType.FastTransferFromCustomerAccount)
            {
                result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
            }
            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));
            result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));
            result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber));
            result.Errors.AddRange(ValidateData());
            result.Errors.AddRange(Validation.ValidateAttachmentDocument(this));
            if (this.TransferAdditionalData != null)
            {
                result.Errors.AddRange(Validation.ValidateTransferAdditionalData(this.TransferAdditionalData,this.Amount));
            }

            if (this.SubType == 23)
            {

                SendMoneyInput sendMoneyInput = new SendMoneyInput();
                ARUSHelper.ConvertObject(this, ref sendMoneyInput);

                if (!String.IsNullOrEmpty(this.Country))
                {
                    DataTable dt = Info.GetCountries();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["CountryCodeN"].ToString() == this.Country)
                        {
                            sendMoneyInput.BeneficiaryCountryCode = dt.Rows[i]["CountryCodeA3"].ToString();
                            break;
                        }
                    }
                }

                sendMoneyInput.SendAmount = (decimal)this.Amount;
                sendMoneyInput.SendCurrencyCode = this.Currency;
                sendMoneyInput.SenderAddressName = this.SenderAddress;
                sendMoneyInput.SenderBirthDate = String.Format("{0:yyyyMMdd}", SenderDateOfBirth);
                sendMoneyInput.SenderEMailName = this.SenderEmail;
                sendMoneyInput.SenderPhoneNo = this.SenderPhone;

                //Մուտքային դաշտերի ստուգումներ
                result.Errors.AddRange(Validation.ValidateSendMoneyOperation(sendMoneyInput));
            }
            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգում ուղարկելուց
        /// </summary>
        /// <returns></returns>
        public new ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (this.Quality != OrderQuality.Draft)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }
            if (this.CustomerNumber == 0)
            {
                //Հաճախորդը գտնված չէ:
                result.Errors.Add(new ActionError(771));
            }
            if (this.Type == OrderType.FastTransferFromCustomerAccount)
            {
                Account debitAccount = Account.GetAccount(this.DebitAccount.AccountNumber);
                Account feeAccount = Account.GetAccount(this.FeeAccount.AccountNumber);

                double debitAccountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);
                double feeAccountBalance = Account.GetAcccountAvailableBalance(this.FeeAccount.AccountNumber);

                if (this.OnlySaveAndApprove)
                {
                    debitAccountBalance += Order.GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.CashDebit);
                    feeAccountBalance += Order.GetSentNotConfirmedAmounts(this.FeeAccount.AccountNumber, OrderType.CashDebit);
                }

                string debitCurrency = debitAccount.Currency;
                double cardFee = 0;
                ///Եթե փոխանցումը կատարվում է քարտից, ապա ստուգվում է մնացորդը ԱՐՔԱ-ում
                if (debitAccount.IsCardAccount())
                {
                    Card card = new Card();
                    card = Card.GetCardWithOutBallance(debitAccount.AccountNumber);

                    KeyValuePair<String, double> arcaBalance = card.GetArCaBalance(this.user.userID);

                    if (arcaBalance.Key == "00" && arcaBalance.Value <= debitAccountBalance)
                    {
                        debitAccountBalance = arcaBalance.Value;
                    }
                    if (this.CardFee != 0)
                    {
                        cardFee = this.CardFee;
                    }
                }

                ///Եթե միջնորդավճարը գանձվում է քարտից, ապա ստուգվում է մնացորդը ԱՐՔԱ-ում
                if (feeAccount.IsCardAccount())
                {
                    Card feeAccountCard = new Card();
                    feeAccountCard = Card.GetCardWithOutBallance(feeAccount.AccountNumber);

                    KeyValuePair<String, double> arcaBalanceFeeCard = feeAccountCard.GetArCaBalance(this.user.userID);

                    if (arcaBalanceFeeCard.Key == "00" && arcaBalanceFeeCard.Value <= feeAccountBalance)
                    {
                        feeAccountBalance = arcaBalanceFeeCard.Value;
                    }

                }


                double debitBalance = debitAccountBalance + Order.GetSentOrdersAmount(debitAccount.AccountNumber, this.Source);
                double feeBalance = feeAccountBalance + Order.GetSentOrdersAmount(feeAccount.AccountNumber, this.Source);

                //Եթե փոխանցումն ու միջնորդավճարի գանձումը կատարվում են նույն հաշվից
                if (debitAccount == feeAccount)
                {
                    if (debitBalance < this.Amount * this.ConvertationRate + this.TransferFee + cardFee)
                    {
                        //հաշվի մնացորդը չի բավարարում տվյալ փոխանցումը կատարելու համար:
                        if (Account.AccountAccessible(debitAccount.AccountNumber, user.AccountGroup))
                            result.Errors.Add(new ActionError(499, new string[] { debitAccount.AccountNumber.ToString(), debitBalance.ToString("#,0.00") + " " + debitAccount.Currency, debitAccount.Balance.ToString("#,0.00") + " " + debitAccount.Currency }));
                        else
                            result.Errors.Add(new ActionError(788, new string[] { debitAccount.AccountNumber }));
                    }
                }
                else
                {
                    if (feeBalance < this.TransferFee)
                    {
                        if (Account.AccountAccessible(feeAccount.AccountNumber, user.AccountGroup))
                            result.Errors.Add(new ActionError(499, new string[] { feeAccount.AccountNumber.ToString(), feeBalance.ToString("#,0.00") + " " + feeAccount.Currency, feeAccount.Balance.ToString("#,0.00") + " " + feeAccount.Currency }));
                        else
                            result.Errors.Add(new ActionError(788, new string[] { feeAccount.AccountNumber }));
                    }


                    if (debitBalance < this.Amount + cardFee)
                    {
                        if (Account.AccountAccessible(debitAccount.AccountNumber, user.AccountGroup))
                            result.Errors.Add(new ActionError(499, new string[] { debitAccount.AccountNumber.ToString(), debitBalance.ToString("#,0.00") + " " + debitAccount.Currency, debitAccount.Balance.ToString("#,0.00") + " " + debitAccount.Currency }));
                        else
                            result.Errors.Add(new ActionError(788, new string[] { debitAccount.AccountNumber }));
                    }

                }

              
            }
            else if (this.Type == OrderType.FastTransferPaymentOrder)
            {

                double debitAccountBalance = Account.GetAcccountAvailableBalance(this.DebitAccount.AccountNumber);// + GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.TransitPaymentOrder);
                if (this.OnlySaveAndApprove)
                {
                    debitAccountBalance += GetSentNotConfirmedAmounts(this.DebitAccount.AccountNumber, OrderType.TransitPaymentOrder);
                }
                if (debitAccountBalance < this.Amount)
                {
                    result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ", debitAccountBalance.ToString("#,0.00") + " " + this.DebitAccount.Currency, this.DebitAccount.Balance.ToString("#,0.00") + " " + this.DebitAccount.Currency }));

                }


                double feeAccountBalance = Account.GetAcccountAvailableBalance(this.FeeAccount.AccountNumber);
                if (this.OnlySaveAndApprove)
                {
                    feeAccountBalance += GetSentNotConfirmedAmounts(this.FeeAccount.AccountNumber, OrderType.TransitPaymentOrder);
                }
                if (feeAccountBalance < this.TransferFee)
                {
                    result.Errors.Add(new ActionError(499, new string[] { "Տարանցիկ դրամային", feeAccountBalance.ToString("#,0.00") + " " + this.FeeAccount.Currency, this.FeeAccount.Balance.ToString("#,0.00") + " " + this.FeeAccount.Currency }));
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
        /// Ստուգում է տեքստային տվյլաները
        /// </summary>
        /// <returns></returns>
        public new List<ActionError> ValidateData()
        {
            List<ActionError> result = new List<ActionError>();

            if (string.IsNullOrEmpty(this.Currency))
            {
                //Արժույթը ընտրված չէ:
                result.Add(new ActionError(254));
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
            else if (Utility.IsExistForbiddenCharacter(Description))
            {
                //«Վճարման նպատակ» դաշտի մեջ կա անթույլատրելի նշան`
                result.Add(new ActionError(647));
            }

            if (this.SubType == 0)
            {
                //Փոխանցման համակարգը ընտրված չէ
                result.Add(new ActionError(778));
            }
            if (string.IsNullOrEmpty(this.Receiver ))
            {
                //Ստացողի տվյալները բացակայում են:
                result.Add(new ActionError(87));
            }
            else if (this.Receiver.Length > 140 || (!string.IsNullOrEmpty(this.ReceiverAddInf) && this.Receiver.Length + this.ReceiverAddInf.Length > 140))
            {
                //Ստացողի տվյալները առավելագույնը 140 նիշ է:
                result.Add(new ActionError(100));
            }
            else if (Utility.IsExistForbiddenCharacter(this.Receiver))
            {
                //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                result.Add(new ActionError(433));
            }

            byte codeMaxLength = FastTransferPaymentOrderDB.GetFastTransferCodeLength(this.SubType, 2);
            byte codeMinLength = FastTransferPaymentOrderDB.GetFastTransferCodeLength(this.SubType, 1);
            

            if (string.IsNullOrEmpty(this.Code ))
            {
                if (this.SubType != 23)
                {
                    //Փոխանցման կոդի երկարությունը պետք է լինի ՝ 
                    result.Add(new ActionError(708, new string[] { codeMaxLength == codeMinLength ? codeMinLength.ToString() : codeMinLength.ToString() + "-" + codeMaxLength.ToString() }));
                }
            }
            else if (this.Code.Length > codeMaxLength || this.Code.Length <codeMinLength)
            {
                //Փոխանցման կոդի երկարությունը պետք է լինի ՝ 
                result.Add(new ActionError(708, new string[] { codeMaxLength == codeMinLength ? codeMinLength.ToString() : codeMinLength.ToString() + "-" + codeMaxLength.ToString() }));
            }
 
            if (string.IsNullOrEmpty(this.DescriptionForPayment))
            {
                //Վճարման մանրամասները բացակայում են:
                result.Add(new ActionError(88));
            }
            else if (this.DescriptionForPayment.Length > 135)
            {
                //Վճարման մանրամասները առավելագույնը 135 նիշ է:
                result.Add(new ActionError(101));
            }
            else if (Utility.IsExistForbiddenCharacter(this.DescriptionForPayment))
            {
                //«Վճարման նպատակ դաշտը պարունակում է անթույլատրելի նիշ/նիշեր: 
                result.Add(new ActionError(988));
            }
            string symbol = "";
            if (this.Receiver != null)
            {
                symbol = Utility.IsExistSwiftForbiddenCharacter(this.Receiver);
            }
            if (symbol!="")
            {
                //Ստացողի տվյալներ դաշտի մեջ կա անթույլատրելի նշան` 
                result.Add(new ActionError(591, new string[] { symbol }));
            }

            if (!string.IsNullOrEmpty(this.ReceiverAddInf))
            {
                symbol = Utility.IsExistSwiftForbiddenCharacter(this.ReceiverAddInf);
                if (symbol != "")
                {
                    //Ստացողի տվյալներ դաշտի մեջ կա անթույլատրելի նշան`
                    result.Add(new ActionError(591, new string[] { symbol }));
                }
                if (Utility.IsExistForbiddenCharacter(this.ReceiverAddInf))
                {
                    //«Ստացողի անվանում»  դաշտի  մեջ կա անթույլատրելի նշան`
                    result.Add(new ActionError(1101));
                }
            }

    
            symbol = Utility.IsExistSwiftForbiddenCharacter(this.DescriptionForPayment );
            if(symbol!="")
            {
                //Վճարման մանրամասներ դաշտի մեջ կա անթույլատրելի նշան`  
                result.Add(new ActionError(592, new string[] { symbol }));
            }

            symbol = Utility.IsExistSwiftForbiddenCharacter(this.Sender  );
            if(symbol!="")
            {
                //Ուղարկողի անվանում դաշտի մեջ կա անթույլատրելի նշան`  
                result.Add(new ActionError(593, new string[] { symbol }));
            }
            else if (Utility.IsExistForbiddenCharacter(this.Sender))
            {
                //Ուղարկողի անվանում դաշտի մեջ կա անթույլատրելի նշան`  
                result.Add(new ActionError(593, new string[] { "" }));
            }


            symbol = Utility.IsExistSwiftForbiddenCharacter(this.SenderAddress  );
            if(symbol!="")
            {
                //Ուղարկողի հասցե դաշտի մեջ կա անթույլատրելի նշան`  
                result.Add(new ActionError(594, new string[] { symbol }));
            }
            else if (Utility.IsExistForbiddenCharacter(this.SenderAddress))
            {
                //Ուղարկողի հասցե դաշտի մեջ կա անթույլատրելի նշան`  
                result.Add(new ActionError(594, new string[] { symbol }));
            }

            if (!string.IsNullOrEmpty(this.SenderPassport) && this.SenderType == (short)CustomerTypes.physical)
            {
                symbol = Utility.IsExistSwiftForbiddenCharacter(this.SenderPassport);
                if (symbol != "" && this.SenderType == (short)CustomerTypes.physical)
                {
                    //Ուղարկողի անձնագիր դաշտի մեջ կա անթույլատրելի նշան` (  ) 
                    result.Add(new ActionError(595, new string[] { symbol }));
                }
            }

            if (!string.IsNullOrEmpty(this.ReceiverPassport))
            {

                if (Utility.IsExistForbiddenCharacter(this.ReceiverPassport))
                {
                    //Ստացողի անձնագրի մեջ կա անթույլատրելի նշան
                    result.Add(new ActionError(1100));
                }
              
            }

            if (string.IsNullOrEmpty(this.Country ) || this.Country=="0")
            {
                //Երկիր դաշտը ընտրված չէ:
                result.Add(new ActionError(81));
            }
            
            if (string.IsNullOrEmpty(this.Sender))
            {
                //Ուղարկողի անվանումը բացակայում է:
                result.Add(new ActionError(104));
            }
            if (string.IsNullOrEmpty(this.SenderAddress))
            {
                //Ուղարկողի հասցեն բացակայում է:
                result.Add(new ActionError(105));
            }
            if (string.IsNullOrEmpty(this.SenderPhone ))
            {
                //Ուղարկողի հեռախոսը բացակայում է:
                result.Add(new ActionError(136));
            }

            if (string.IsNullOrEmpty(this.SenderDateOfBirth.ToString()))
            {
                //Ուղարկողի ծննդյան ամսաթիվը բացակայում է:
                result.Add(new ActionError(108));
            }
            if (string.IsNullOrEmpty(this.SenderPassport) )
            {
                //Ուղարկողի անձնագիրը բացակայում է:
                result.Add(new ActionError(107));
            }
 

            //Ռիսկային երկրների ստուգում
            if (this.Country=="364" || this.Country=="760" || this.Country=="729" || this.Country=="728")
            {
                //Ստացողի երկիրը ռիսկային է (ԻՐԱՆ, ՍԻՐԻԱ, ՍՈՒԴԱՆ, ՀԱՐԱՎԱՅԻՆ ՍՈՒԴԱՆ)
                result.Add(new ActionError(626));
            }

            if (this.DebitAccount.Currency != this.Currency)
            {
                //Դեբետագրվող արժույթը տարբերվում է գործարքի արժույթից
                result.Add(new ActionError(640));
            }

            if (this.FeeAcba==0)
            {
                //ACBA միջնորդավճարը բացակայում է
                result.Add(new ActionError(709));
            }


            if (this.Fee == 0)
            {
                //Համակարգի միջնորդավճարը բացակայում է
                result.Add(new ActionError(710));
            }

            if (this.OPPerson != null)
            {
                if (this.OPPerson.CustomerNumber != this.CustomerNumber)
                {
                    //Որպես գործարք կարատող անձ ընտրված է այլ հաճախորդ
                    result.Add(new ActionError(714));
                }
            }
            if (Account.IsUserAccounts(user.userCustomerNumber, this.DebitAccount.AccountNumber, "0"))
            {
                //Հնարավոր չէ կատարել ձևակերպումներ օգտագործողի սեփական հաշիվների միջև
                result.Add(new ActionError(544));
            }

          return result;
        }

        /// <summary>
        /// Ուղարկման հայտի կատարման ստուգումներ 
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForApprove()
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            return result;
        }

        /// <summary>
        /// Ստանում է դաշտերի կոդերին համապատասխան անվանումները
        /// </summary>
        /// <param name="authorizedUserSessionToken"></param>
        /// <param name="userName"></param>
        /// <param name="clientIP"></param>
        void GetCodeNames(string authorizedUserSessionToken, string userName, string clientIP)
        {
            if (!String.IsNullOrEmpty(MTOAgentCode))
            {
                DataTable MTOList = ARUSInfo.GetMTOList(authorizedUserSessionToken, userName, clientIP);

                MTOAgentName = MTOList.Select("code = '" + MTOAgentCode + "'")[0]["name"].ToString();

                DataTable countries = ARUSInfo.GetCountriesByMTO(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
                string beneficiaryCountryCode = "";
                if (!String.IsNullOrEmpty(Country))
                {

                    DataTable dt = Info.GetCountries();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["CountryCodeN"].ToString() == this.Country)
                        {
                            beneficiaryCountryCode = dt.Rows[i]["CountryCodeA3"].ToString();
                            break;
                        }
                    }


                    if (!String.IsNullOrEmpty(BeneficiaryStateCode))
                    {
                        DataTable states = ARUSInfo.GetStates(MTOAgentCode, beneficiaryCountryCode, authorizedUserSessionToken, userName, clientIP);
                        BeneficiaryCityName = states.Select("code = '" + BeneficiaryStateCode + "'")[0]["name"].ToString();
                    }

                    if (!String.IsNullOrEmpty(BeneficiaryCityCode))
                    {
                        DataTable cities = ARUSInfo.GetCitiesByCountry(MTOAgentCode, beneficiaryCountryCode, authorizedUserSessionToken, userName, clientIP);
                        BeneficiaryCityName = cities.Select("code = '" + BeneficiaryCityCode + "'")[0]["name"].ToString();
                    }

                }

                if (!String.IsNullOrEmpty(SenderCountryCode))
                {
                    SenderCountryName = countries.Select("code = '" + SenderCountryCode + "'")[0]["name"].ToString();

                    if (!String.IsNullOrEmpty(SenderCityCode))
                    {
                        DataTable cities = ARUSInfo.GetCitiesByCountry(MTOAgentCode, SenderCountryCode, authorizedUserSessionToken, userName, clientIP);
                        SenderCityName = cities.Select("code = '" + SenderCityCode + "'")[0]["name"].ToString();
                    }

                    if (!String.IsNullOrEmpty(SenderStateCode))
                    {
                        DataTable states = ARUSInfo.GetStates(MTOAgentCode, SenderCountryCode, authorizedUserSessionToken, userName, clientIP);
                        SenderStateName = states.Select("code = '" + SenderStateCode + "'")[0]["name"].ToString();
                    }
                }

                if (!String.IsNullOrEmpty(SenderIssueCountryCode))
                {
                    SenderIssueCountryName = countries.Select("code = '" + SenderIssueCountryCode + "'")[0]["name"].ToString();

                    if (!String.IsNullOrEmpty(SenderIssueCityCode))
                    {
                        DataTable cities = ARUSInfo.GetCitiesByCountry(MTOAgentCode, SenderIssueCountryCode, authorizedUserSessionToken, userName, clientIP);
                        SenderIssueCityName = cities.Select("code = '" + SenderIssueCityCode + "'")[0]["name"].ToString();
                    }
                }

                if (!String.IsNullOrEmpty(SenderBirthCountryCode))
                {
                    SenderBirthCountryName = countries.Select("code = '" + SenderBirthCountryCode + "'")[0]["name"].ToString();
                }

                if (!String.IsNullOrEmpty(SenderDocumentTypeCode))
                {
                    DataTable documentTypes = ARUSInfo.GetDocumentTypes(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);

                    SenderDocumentTypeName = documentTypes.Select("code = '" + SenderDocumentTypeCode + "'")[0]["name"].ToString();
                }

                if (!String.IsNullOrEmpty(PurposeRemittanceCode))
                {
                    DataTable purposes = ARUSInfo.GetRemittancePurposes(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
                    PurposeRemittanceName = purposes.Select("code = '" + PurposeRemittanceCode + "'")[0]["name"].ToString();
                }

                if (!String.IsNullOrEmpty(PayoutDeliveryCode))
                {
                    DataTable deliveryCodes = ARUSInfo.GetPayoutDeliveryCodes(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);
                    PayoutDeliveryName = deliveryCodes.Select("code = '" + PayoutDeliveryCode + "'")[0]["name"].ToString();
                }

                if (!String.IsNullOrEmpty(BeneficiaryAgentCode))
                {
                    if (!String.IsNullOrEmpty(beneficiaryCountryCode) && !String.IsNullOrEmpty(BeneficiaryCityCode) && !String.IsNullOrEmpty(Currency))
                    {
                        DataTable agentCodes = ARUSInfo.GetMTOAgencies(MTOAgentCode, beneficiaryCountryCode, BeneficiaryCityCode, Currency, BeneficiaryStateCode, authorizedUserSessionToken, userName, clientIP);
                        BeneficiaryAgentName = agentCodes.Select("code = '" + BeneficiaryAgentCode + "'")[0]["name"].ToString();
                    }

                }
            }

            if (!String.IsNullOrEmpty(SenderSexCode))
            {
                DataTable sexes = ARUSInfo.GetSexes(authorizedUserSessionToken, userName, clientIP);

                SenderSexName = sexes.Select("code = '" + SenderSexCode + "'")[0]["name"].ToString();
                SenderSexName = SenderSexName == "Female" ? "Female (Իգական)" : (SenderSexName == "Male" ? "Male (Արական)" : SenderSexName);
            }

            if (!String.IsNullOrEmpty(SenderResidencyCode))
            {
                DataTable YesNoList = ARUSInfo.GetYesNo(authorizedUserSessionToken, userName, clientIP);

                SenderResidencyName = YesNoList.Select("code = '" + SenderResidencyCode + "'")[0]["name"].ToString();
                SenderResidencyName = SenderResidencyName == "Yes" ? "Այո" : (SenderResidencyName == "No" ? "Ոչ" : SenderResidencyName);

            }

        }

        public static Dictionary<string, string> GetRemittanceContractDetails(string authorizedUserSessionToken, string userName, string clientIP, ulong docId) 
        {
            Dictionary<string, string> detailsResult = new Dictionary<string, string>();
            detailsResult = FastTransferPaymentOrderDB.GetRemittanceContractDetails(docId);

            DataTable countries = ARUSInfo.GetCountriesByMTO(detailsResult["MTOAgentCode"], authorizedUserSessionToken, userName, clientIP);
            detailsResult.Remove("MTOAgentCode");

            detailsResult["countryName"] = countries.Select("code = '" + detailsResult["countryName"] + "'")[0]["name"].ToString();

            return detailsResult;
        }

    }

}
