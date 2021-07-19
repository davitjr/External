using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.ARUSDataService;
using System.Globalization;
using System.Data;


namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class ReceivedFastTransferPaymentOrder : PaymentOrder
    {

        /// <summary>
        /// Ուղարկողի անուն/անվանում
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Ուղարկողի անուն/անվանում
        /// </summary>
        public string NATSender { get; set; }

        /// <summary>
        /// Ստացողի լրացուցիչ տվյալներ
        /// </summary>
        public string ReceiverPassport { get; set; }

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
        public double Fee { get; set; }

        /// <summary>
        /// Միջնորդավճար ACBA արժույթով
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

        /// Համաձայնագրի ունիկալ համար        
        public long ContractId { get; set; }

        ///Հեռախոսազանգով փոխանցման id  
        public long TransferByCallID { get; set; }

        ///Ստացողի հեռախոսահամար  
        public string ReceiverPhone { get; set; }



        ///// <summary>
        ///// Գումարի ստացման եղանակ
        ///// </summary>
        //public byte IsCash { get; set; }

        //**************************************************************************************************
        //ARUS համակարգի համար պահանջվող դաշտեր, որոնք առկա չէին նախքան ARUS համակարգին անցումը

        /// <summary>
        ///Ստացողի ազգանուն
        /// </summary>
        public string BeneficiaryLastName { get; set; }

        /// <summary>
        ///Ստացողի հայրանուն
        /// </summary>
        public string BeneficiaryMiddleName { get; set; }

        /// <summary>
        ///Ստացողի անուն
        /// </summary>
        public string BeneficiaryFirstName { get; set; }

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
        ///Ստացողի սեռի կոդ
        /// </summary>
        public string BeneficiarySexCode { get; set; }

        /// <summary>
        ///Ստացողի երկրի կոդ
        /// </summary>
        public string BeneficiaryCountryCode { get; set; }

        /// <summary>
        ///Ստացողի մարզի կոդ
        /// </summary>
        public string BeneficiaryStateCode { get; set; }

        /// <summary>
        ///Ստացողի քաղաքի կոդ
        /// </summary>
        public string BeneficiaryCityCode { get; set; }

        /// <summary>
        ///Ստացողի հասցե
        /// </summary>
        public string BeneficiaryAddressName { get; set; }

        /// <summary>
        ///Ստացողի փոստային կոդ
        /// </summary>
        public string BeneficiaryZipCode { get; set; }

        /// <summary>
        ///Ստացողի փաստաթղթի տեսակի կոդ
        /// </summary>
        public string BeneficiaryDocumentTypeCode { get; set; }

        /// <summary>
        ///Ստացողի զբաղվածություն
        /// </summary>
        public string BeneficiaryOccupationName { get; set; }

        /// <summary>
        ///Երկրի կոդը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCountryCode { get; set; }

        /// <summary>
        ///Քաղաքի կոդը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCityCode { get; set; }

        /// <summary>
        ///Ստացողի ID համարը
        /// </summary>
        public string BeneficiaryIssueIDNo { get; set; }

        /// <summary>
        ///Ստացողի ID համարի տրման ամսաթիվ
        /// </summary>
        public string BeneficiaryIssueDate { get; set; }

        /// <summary>
        ///Ամսաթիվ՝ ցույց տվող, թե մինչև երբ է ուժի մեջ ստացողի ID համարը
        /// </summary>
        public string BeneficiaryExpirationDate { get; set; }

        /// <summary>
        ///Ստացողի ծննդյան ամսաթիվ
        /// </summary>
        public string BeneficiaryBirthDate { get; set; }

        /// <summary>
        ///Ստացողի էլեկտրոնային հասցե
        /// </summary>
        public string BeneficiaryEMailName { get; set; }

        /// <summary>
        ///Ստացողի բջջային հեռախոսահամար
        /// </summary>
        public string BeneficiaryMobileNo { get; set; }

        /// <summary>
        /// Ստացողի ռեզիդենտության կոդ
        /// </summary>
        public string BeneficiaryResidencyCode { get; set; }

        /// <summary>
        /// Ստացողի ֆիսկալ կոդ
        /// </summary>
        public string BeneficiaryFiscalCode { get; set; }

        /// <summary>
        /// Ստացողի ծննդավայր
        /// </summary>
        public string BeneficiaryBirthPlaceName { get; set; }

        //**********************************************************************************************************************
        //ACBA-ի կողմից ավելացրած դաշտեր ARUS համակարգի համար

        /// <summary>
        /// Դրամական փոխանցման օպերատորի գործակալի կոդ
        /// </summary>
        public string MTOAgentCode { get; set; }

        /// <summary>
        /// Դրամական փոխանցման օպերատորի գործակալի կոդ
        /// </summary>
        public string MTOAgentName { get; set; }

        /// <summary>
        /// Ցույց է տալիս՝ արդյոք փոխանցման ստացումը հաջողությամբ կատարվել է ARUS համակարգի կողմից, թե ոչ (1` բարեհաջող կատարում, 0՝ անհաջող ավարտ)
        /// </summary>
        public short ARUSSuccess { get; set; }

        /// <summary>
        /// ARUS համակարգի կողմից ստացված սխալի հաղորդագրություն
        /// </summary>
        public string ARUSErrorMessage { get; set; }

        /// <summary>
        /// Ստացողի սեռի անվանում
        /// </summary>
        public string BeneficiarySexName { get; set; }

        /// <summary>
        /// Ստացողի ռեզիդենտության անվանում (Այո / Ոչ)
        /// </summary>
        public string BeneficiaryResidencyName { get; set; }

        /// <summary>
        ///Ստացողի երկրի անվանում
        /// </summary>
        public string BeneficiaryCountryName { get; set; }

        /// <summary>
        ///Ստացողի մարզի անվանում
        /// </summary>
        public string BeneficiaryStateName { get; set; }

        /// <summary>
        ///Ստացողի քաղաքի անվանում
        /// </summary>
        public string BeneficiaryCityName { get; set; }

        /// <summary>
        ///Ստացողի փաստաթղթի տեսակի անվանում
        /// </summary>
        public string BeneficiaryDocumentTypeName { get; set; }

        /// <summary>
        ///Երկրի անվանումը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCountryName { get; set; }

        /// <summary>
        ///Քաղաքի անվանումը, որի կողմից որ թողարկվել է ստացողի փաստաթուղթը
        /// </summary>
        public string BeneficiaryIssueCityName { get; set; }

        /// <summary>
        /// Միջնորդավճարի արժույթ
        /// </summary>
        public string FeeCurrency { get; set; }


        //**********************************************************************************************************************

        /// <summary>
        ///Փոխարկված գումար
        /// </summary>
        public double ExchangedAmount { get; set; }

        ///Ստացողի հեռախոսահամար
        public string SenderPhone { get; set; }
        ///Ստացողի հեռախոսահամար
        public string SenderAgentName { get; set; }


        public new void Get(string userName, string authorizedUserSessionToken, string clientIP, SourceType source)
        {
            ReceivedFastTransferPaymentOrderDB.Get(this);
            this.Fees = Order.GetOrderFees(this.Id);

            if (this.Fees.Exists(m => m.Type == 5))
            {
                this.TransferFee = this.Fees.Find(m => m.Type == 5).Amount;
                this.FeeAccount = this.Fees.Find(m => m.Type == 5).Account;
            }

            //ARUS համակարգի կողմից ստացված փոխանցումներ
            if (this.SubType == 23)
            {
                GetCodeNames(authorizedUserSessionToken, userName, clientIP);
            }

            if(source == SourceType.AcbaOnline || source == SourceType.MobileBanking ||
                source == SourceType.AcbaOnlineXML || source == SourceType.ArmSoft)
            {
                double coeficent = 0;
                this.ExchangedAmount = 0;

                if (this.ReceiverAccount != null)
                {
                    if ((this.Currency != this.ReceiverAccount.Currency) && this.ReceiverAccount.Currency == "AMD")
                    {
                        this.ExchangedAmount = Math.Round(this.ConvertationRate1 * this.Amount, 2);
                    }
                    else if ((this.Currency != this.ReceiverAccount.Currency) && this.ReceiverAccount.Currency != "AMD")
                    {

                        if (this.ConvertationRate != 0 && this.ConvertationRate1 != 0)
                        {
                            coeficent = Math.Round(this.ConvertationRate / this.ConvertationRate1, 3);
                            this.ExchangedAmount = Math.Round(this.Amount * coeficent, 2);
                        }
                        else
                        {
                            double rate = Utility.GetLastExchangeRate(this.ReceiverAccount.Currency, RateType.Transfer, ExchangeDirection.Sell);
                            this.ExchangedAmount = Math.Round(this.Amount / rate, 2);
                        }

                    }
                }



            }


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

                if (!String.IsNullOrEmpty(BeneficiaryCountryCode))
                {
                    BeneficiaryCountryName = countries.Select("code = '" + BeneficiaryCountryCode + "'")[0]["name"].ToString();

                    if (!String.IsNullOrEmpty(BeneficiaryStateCode))
                    {
                        DataTable states = ARUSInfo.GetStates(MTOAgentCode, BeneficiaryCountryCode, authorizedUserSessionToken, userName, clientIP);
                        BeneficiaryCityName = states.Select("code = '" + BeneficiaryStateCode + "'")[0]["name"].ToString();
                    }

                    if (!String.IsNullOrEmpty(BeneficiaryCityCode))
                    {
                        DataTable cities = ARUSInfo.GetCitiesByCountry(MTOAgentCode, BeneficiaryCountryCode, authorizedUserSessionToken, userName, clientIP);
                        BeneficiaryCityName = cities.Select("code = '" + BeneficiaryCityCode + "'")[0]["name"].ToString();
                    }

                }

                if (!String.IsNullOrEmpty(BeneficiaryIssueCountryCode))
                {
                    BeneficiaryIssueCountryName = countries.Select("code = '" + BeneficiaryIssueCountryCode + "'")[0]["name"].ToString();

                    if (!String.IsNullOrEmpty(BeneficiaryIssueCityCode))
                    {
                        DataTable cities = ARUSInfo.GetCitiesByCountry(MTOAgentCode, BeneficiaryIssueCountryCode, authorizedUserSessionToken, userName, clientIP);
                        BeneficiaryIssueCityName = cities.Select("code = '" + BeneficiaryIssueCityCode + "'")[0]["name"].ToString();
                    }
                }

                if (!String.IsNullOrEmpty(BeneficiaryDocumentTypeCode))
                {
                    DataTable documentTypes = ARUSInfo.GetDocumentTypes(MTOAgentCode, authorizedUserSessionToken, userName, clientIP);

                    BeneficiaryDocumentTypeName = documentTypes.Select("code = '" + BeneficiaryDocumentTypeCode + "'")[0]["name"].ToString();
                }
            }

            if (!String.IsNullOrEmpty(BeneficiarySexCode))
            {
                DataTable sexes = ARUSInfo.GetSexes(authorizedUserSessionToken, userName, clientIP);

                BeneficiarySexName = sexes.Select("code = '" + BeneficiarySexCode + "'")[0]["name"].ToString();
                BeneficiarySexName = BeneficiarySexName == "Female" ? "Female (Իգական)" : (BeneficiarySexName == "Male" ? "Male (Արական)" : BeneficiarySexName);
            }

            if (!String.IsNullOrEmpty(BeneficiaryResidencyCode))
            {
                DataTable YesNoList = ARUSInfo.GetYesNo(authorizedUserSessionToken, userName, clientIP);
                if (!String.IsNullOrEmpty(BeneficiaryResidencyCode))
                {
                    BeneficiaryResidencyName = YesNoList.Select("code = '" + BeneficiaryResidencyCode + "'")[0]["name"].ToString();
                    BeneficiaryResidencyName = BeneficiaryResidencyName == "Yes" ? "Այո" : (BeneficiaryResidencyName == "No" ? "Ոչ" : BeneficiaryResidencyName);
                }
            }

        }

        private void Get(ReceivedFastTransferPaymentOrder order)
        {
            ReceivedFastTransferPaymentOrderDB.Get(order);
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
            if (this.ReceiverAccount == null)
            {
                //Տարանցիկ հաշվի լրացում
                this.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), this.Currency, user.filialCode);
                //this.IsCash = 1;
            }

            this.FilialCode = user.filialCode;

            if(Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
            {
                this.RegistrationDate = DateTime.Now.Date;
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
                if (user.AdvancedOptions == null)
                {
                    user.AdvancedOptions = new Dictionary<string, string>();
                }

                user.AdvancedOptions.Add("isCallCenter", "1");
            }


            List<CustomerDocument> customerDocumentsList = Customer.GetCustomerDocumentList(this.CustomerNumber);
            if (customerDocumentsList.Find(m => m.defaultSign == true) != null)
            {
                this.ReceiverPassport = customerDocumentsList.Find(m => m.defaultSign == true).documentNumber;
            }


            //this.Complete();
            string isCallCenter = null;
            if (user.AdvancedOptions != null)
            {
                user.AdvancedOptions.TryGetValue("isCallCenter", out isCallCenter);
            }


            ActionResult result = this.Validate(Convert.ToUInt16(isCallCenter));
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = ReceivedFastTransferPaymentOrderDB.Save(this, userName, source, Convert.ToUInt16(isCallCenter));
                LogOrderChange(user, action);
                scope.Complete();
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
        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, string authorizedUserSessionToken, string clientIP)
        {
            this.Complete();

            string isCallCenter = null;
            user.AdvancedOptions.TryGetValue("isCallCenter", out isCallCenter);

            ActionResult result = this.Validate(Convert.ToUInt16(isCallCenter));

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

                result = ReceivedFastTransferPaymentOrderDB.Save(this, userName, source, Convert.ToUInt16(isCallCenter));
             
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

            if (this.SubType != 23)
            {
                result = base.Confirm(user);
            }
            //ARUS համակարգով փոխանցումներ
            else
            {
                ARUSDataService.PayoutRequestResponse ARUSRequestResponse = new ARUSDataService.PayoutRequestResponse();

                PayOutInput payoutInput = new PayOutInput();
                ARUSHelper.ConvertObject(this, ref payoutInput);
                payoutInput.BeneficiaryPhoneNo = this.ReceiverPhone;
                payoutInput.PayoutCurrencyCode = this.Currency;
                payoutInput.PayoutAmount = (decimal)this.Amount;
                payoutInput.URN = this.Code;

                //ARUS հարցում
                ARUSHelper.Use(client =>
                {
                    ARUSRequestResponse = client.PayOutOperation(payoutInput, ARUSHelper.GenerateMessageUniqueNo().ToString(), this.Id.ToString());
                }, authorizedUserSessionToken, userName, "clientIP");

                //ARUS համակարգից ստացվել է սխալի հաղորդագրություն
                if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.NoneAutoConfirm)
                {
                    result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                    ReceivedFastTransferPaymentOrderDB.UpdateARUSMessage(this.Id, result.Errors[0].Description);
                    result.ResultCode = ResultCode.SavedNotConfirmed;
                }
                //ARUS համակարգին դիմելու պահին տեղի է ունեցել սխալ
                else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Failed)
                {
                    result = ARUSHelper.ConvertARUSActionResultToXBActionResult(ARUSRequestResponse.ActionResult);
                }
                //ARUS համակարգի հարցումն ունեցել է հաջող ավարտ
                else if (ARUSRequestResponse.ActionResult.ResultCode == ARUSDataService.ResultCode.Normal)
                {
                    ReceivedFastTransferPaymentOrderDB.UpdateARUSSuccess(this.Id, 1);
                    result = base.Confirm(user);

                    string infoDescription = Environment.NewLine + "Փոխանցման կարգավիճակ՝ " + ARUSRequestResponse.PayOutOutput.StatusCodeName;

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
            }

            return result;
        }



        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected new void Complete()
        {

            if (this.ReceiverAccount == null)
            {
                //Տարանցիկ հաշվի լրացում
                this.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), this.Currency, user.filialCode);
                //this.IsCash = 1;
            }
            this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source == SourceType.MobileBanking || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }
            this.Fee = Utility.RoundAmount(this.Fee, this.Currency);
            //ARUS համակարգով փոխանցումներ
            if (this.SubType == 23)
            {
                //Եթե փոխանցումը ուղարկվել է Հայաստանից Հայաստան
                if (this.Country == "ARM" && this.Country == this.BeneficiaryCountryCode)
                {
                    this.FeeCurrency = "AMD";
                }

                DateTime outputDate;
                if (DateTime.TryParseExact(BeneficiaryIssueDate, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate))
                {
                    BeneficiaryIssueDate = String.Format("{0:yyyyMMdd}", outputDate);
                }


                if (DateTime.TryParseExact(BeneficiaryExpirationDate, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate))
                {
                    BeneficiaryExpirationDate = String.Format("{0:yyyyMMdd}", outputDate);
                }

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
            }
            else
            {
                this.FeeAcba = Utility.RoundAmount(this.FeeAcba, this.Currency);
            }
        }

        public static double GetReceivedFastTransferFeePercent(byte transferType, string code = "", string countryCode = "", double amount = 0, string currency = "", DateTime date = new DateTime())
        {
            return ReceivedFastTransferPaymentOrderDB.GetFastTransferFeePercent(transferType, code, countryCode, amount, currency, date);
        }

        public static byte GetFastTransferAcbaCommisionType(byte transferType, string code)
        {
            return ReceivedFastTransferPaymentOrderDB.GetFastTransferAcbaCommisionType(transferType, code);
        }

        /// <summary>
        /// Միջազգային վճարման հանձնարարականի ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ushort isCallCenter)
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if(this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (string.IsNullOrEmpty(this.Code))
                {
                    result.Errors.Add(new ActionError(1631));
                }

            }

            //if (isCallCenter!=1 )
            result.Errors.AddRange(Validation.ValidateCustomerSignature(this.CustomerNumber));

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));
            result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));
            // result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber));
            result.Errors.AddRange(ValidateData(isCallCenter));
            
            if (this.TransferAdditionalData != null)
            {
                result.Errors.AddRange(Validation.ValidateTransferAdditionalData(this.TransferAdditionalData, this.Amount));
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if (this.SubType == 23)
            {

                PayOutInput payoutInput = new PayOutInput();
                ARUSHelper.ConvertObject(this, ref payoutInput);
                payoutInput.BeneficiaryPhoneNo = this.ReceiverPhone;
                payoutInput.PayoutCurrencyCode = this.Currency;
                payoutInput.PayoutAmount = (decimal)this.Amount;
                payoutInput.URN = this.Code;

                //Մուտքային դաշտերի ստուգումներ
                result.Errors.AddRange(Validation.ValidatePayOutOperation(payoutInput));
            }

            //9772
            if (ContractId != 0 || Source != SourceType.Bank)
            {
                Account transferCallContractAccount = new Account();

                if (ContractId != 0)
                    transferCallContractAccount = TransferCallContractDetailsDB.GetTransferCallContractDetails(ContractId, CustomerNumber).Account;
                else
                    transferCallContractAccount = ReceiverAccount;

                if (TransferByCallDB.IsPensionAccount(transferCallContractAccount.AccountNumber))
                {
                    if (Source == SourceType.Bank)
                    {
                        //Սոցիալական ապահովության հաշվին կամ ACBA Pension կենսաթոշակային քարտի հաշվեհամարին հնարավոր չէ իրականացնել հայտի մուտքագրում:
                        result.Errors.Add(new ActionError(1952));
                    }
                    else
                    {
                        //Հարգելի հաճախորդ, Ձեր նախընտրած հաշվին բացի կենսաթոշակի և նպաստի գումարներից այլ տեսակի փոխանցումները հնարավոր չէ մուտքագրել։ Առկայության դեպքում, խնդրում ենք ընտրել մեկ այլ հաշիվ:​
                        result.Errors.Add(new ActionError(1953));
                    }
                }
                if (TransferByCallDB.IsPrivateEntrepreneurAccount(transferCallContractAccount.AccountNumber))
                {
                    if (Source == SourceType.Bank)
                    {
                        //Ձեր ընտրած հաշվին հնարավոր չէ մուտքագրել արագ դրամական համակարգով ստացված փոխանցումը։ Խնդրում ենք ընտրել մեկ այլ հաշիվ
                        result.Errors.Add(new ActionError(1954));
                    }
                    else
                    {
                        //Հարգելի հաճախորդ, Ձեր նախընտրած հաշվին հնարավոր չէ մուտքագրել արագ դրամական համակարգով ստացված փոխանցումը։ Խնդրում ենք ընտրել մեկ այլ հաշիվ
                        result.Errors.Add(new ActionError(1955));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգում ուղարկելուց
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
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

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                if (!(this.ConvertationRate == 0 && this.ConvertationRate1 == 0))
                    result.Errors.AddRange(this.ValidateRate());
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
        public List<ActionError> ValidateData(ushort isCallCenter)
        {
            List<ActionError> result = new List<ActionError>();

            if (this.Source != SourceType.MobileBanking && this.Source != SourceType.AcbaOnline)
            {
                ReceivedFastTransferPaymentOrder order = (ReceivedFastTransferPaymentOrder)this.MemberwiseClone();
                order.SubType = 1;
                result.AddRange(Validation.CheckOperationAvailability(order, user));
            }

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

            if (this.ContractId != 0)
            {
                TransferCallContract contract = TransferCallContract.GetContractDetails(this.ContractId);

                string receiverAccountCurrency = Account.GetAccount(contract.AccountNumber).Currency;
                if (receiverAccountCurrency != this.Currency && this.ConvertationRate == 0 && this.ConvertationRate1 == 0)
                {
                    //Փոխարժեքը մուտքագրված չէ:
                    result.Add(new ActionError(59));
                }
            }

            //Ð³×³Ëáñ¹Ç ³Ý·É»ñ»Ý ³ÝáõÝÁ µ³ó³Ï³ÛáõÙ ¿
            if (string.IsNullOrEmpty(this.Receiver) && this.Type == OrderType.ReceivedFastTransferPaymentOrder && isCallCenter != 1)
            {
                //Ստացողի տվյալները բացակայում են:
                result.Add(new ActionError(779));
            }


            if (string.IsNullOrEmpty(this.ReceiverPassport) && isCallCenter != 2)
            {
                //Հաճախորդի անձը հաստատող փաստաթուղթը գտնված չէ
                result.Add(new ActionError(751));
            }

            if (this.ContractId == 0)
            {
                if (Customer.IsCustomerUpdateExpired(this.CustomerNumber))
                {
                    //Հաճախորդի տվյալները չեն թարմացվել մեկ տարվա ընթացքում
                    result.Add(new ActionError(765, new string[] { this.CustomerNumber.ToString() }));
                    return result;
                }
            }

            if (this.Source != SourceType.AcbaOnline && this.Source != SourceType.MobileBanking)
            {
                byte codeMaxLength = FastTransferPaymentOrderDB.GetFastTransferCodeLength(this.SubType, 2);
                byte codeMinLength = FastTransferPaymentOrderDB.GetFastTransferCodeLength(this.SubType, 1);

                if (this.SubType == 0)
                {
                    //Փոխանցման համակարգը ընտրված չէ
                    result.Add(new ActionError(778));
                }

                if (string.IsNullOrEmpty(this.Code))
                {
                    //Փոխանցման կոդի երկարությունը պետք է լինի ՝ 
                    result.Add(new ActionError(708, new string[] { codeMaxLength == codeMinLength ? codeMinLength.ToString() : codeMinLength.ToString() + "-" + codeMaxLength.ToString() }));
                }
                else if (this.Code.Length > codeMaxLength || this.Code.Length < codeMinLength)
                {
                    //Փոխանցման կոդի երկարությունը պետք է լինի ՝ 
                    result.Add(new ActionError(708, new string[] { codeMaxLength == codeMinLength ? codeMinLength.ToString() : codeMinLength.ToString() + "-" + codeMaxLength.ToString() }));
                }

                if ((string.IsNullOrEmpty(this.Country) || this.Country == "0") && this.Type == OrderType.ReceivedFastTransferPaymentOrder && isCallCenter != 1)
                {
                    //Երկիր դաշտը ընտրված չէ:
                    result.Add(new ActionError(81));
                }

                if (string.IsNullOrEmpty(this.Sender) && this.Type == OrderType.ReceivedFastTransferPaymentOrder && isCallCenter != 1)
                {
                    //Ուղարկողի անվանումը բացակայում է:
                    result.Add(new ActionError(104));
                }
            }

            


            if (this.FeeAcba == 0 && (this.SubType == 12 || (!(isCallCenter == 1 && this.Type == OrderType.ReceivedFastTransferPaymentOrder) && (this.SubType == 21 || this.SubType == 17))))
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
            if (this.SubType == 21 && this.Currency == "AMD" && this.Country != "51" && isCallCenter != 1)
            {
                //AMD արժույթի դեպքում երկիրը պետք է լինի ARMENIA
                result.Add(new ActionError(780));
            }
            if (this.ContractId != 0 && !Utility.CanPayTransferByCall(this.SubType))
            {
                //Հնարավոր չէ կատարել հեռախոսազանգով փոխանցում MONEYGRAM համակարգով
                result.Add(new ActionError(1049));
            }

            return result;
        }


        /// <summary>
        /// Նույն տվյալներով փոխանցման առկայության ստուգում
        /// </summary>
        public static bool IsExistingTransferByCall(short transferSystem, string code, long transferId)
        {
            return ReceivedFastTransferPaymentOrderDB.IsExistingTransferByCall(transferSystem, code, transferId);
        }
        public static void SetTransferByCallType(short type, long id)
        {
            ReceivedFastTransferPaymentOrderDB.SetTransferByCallType(type, id);
        }
        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult ApproveReceivedFastTransferOrder(string userName, SourceType source, ACBAServiceReference.User user, short schemaType, ReceivedFastTransferPaymentOrder order)
        {


            ActionResult result = new ActionResult();


            result = this.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            //todo bacel hetagayum stugeluc heto COMMENT E ARVEL 07/08/18  Babken Makaryan
            Account receiverAccount = Account.GetAccount(order.ReceiverAccount.AccountNumber);
            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                double rate;
                if (order.Currency == receiverAccount.Currency)
                {
                    ReceivedFastTransferPaymentOrder.UpdateOrderCurrencyFastTransfer(0, 0, order.OrderId);
                }
                else
                {
                    if (order.Currency == "AMD" || receiverAccount.Currency == "AMD")
                    {
                        if (order.Currency != "AMD")
                        {
                            rate = Utility.GetLastExchangeRate(order.Currency, RateType.Transfer, ExchangeDirection.Buy);
                            ReceivedFastTransferPaymentOrder.UpdateOrderCurrencyFastTransfer(rate, 0, order.Id);

                        }
                        else
                        {
                            rate = Utility.GetLastExchangeRate(receiverAccount.Currency, RateType.Transfer, ExchangeDirection.Sell);
                            ReceivedFastTransferPaymentOrder.UpdateOrderCurrencyFastTransfer(0, rate, order.Id);
                        }
                    }
                    else
                    {
                        double rateDebit;
                        double rateCredit;

                        rateDebit = Utility.GetLastExchangeRate(order.Currency, RateType.Cross, ExchangeDirection.Buy);
                        rateCredit = Utility.GetLastExchangeRate(receiverAccount.Currency, RateType.Cross, ExchangeDirection.Sell);
                        ReceivedFastTransferPaymentOrder.UpdateOrderCurrencyFastTransfer(rateDebit, rateCredit, order.Id);

                    }
                }

            }


            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = base.Approve(schemaType, userName);
                this.Quality = OrderQuality.Sent3;
                if (source != SourceType.MobileBanking && source != SourceType.AcbaOnline)
                {
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                        base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                        LogOrderChange(user, Action.Update);
                    }

                }
                result = base.Confirm(user);
                scope.Complete();
            }



            return result;
        }

        /// <summary>
        /// Վերադարձնում է ստացված արագ փոխանցման հայտի մերժման պատճառը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string GetReceivedFastTransferOrderRejectReason(Languages language)
        {
            return ReceivedFastTransferPaymentOrderDB.GetReceivedFastTransferOrderRejectReason(this.Id, language);
        }



        /// <summary>
        /// Փոխարժեքի արժեքի ստուգում
        /// </summary>
        /// <param name="paymentOrder"></param>
        /// <returns></returns>
        internal List<ActionError> ValidateRate()
        {
            List<ActionError> result = new List<ActionError>();
            string receiverAccountCurrency = Account.GetAccount(this.ReceiverAccount.AccountNumber).Currency;
            Double rate = 0;
            Double rate1 = 0;


            if (this.ConvertationRate == 0 || this.ConvertationRate1 == 0)
            {
                RateType rateType = new RateType();
                rateType = RateType.Transfer;

                if (receiverAccountCurrency != "AMD")
                {
                    rate = Utility.GetLastExchangeRate(receiverAccountCurrency, rateType, ExchangeDirection.Sell);
                    if (rate != this.ConvertationRate)
                    {
                        //Հնարավոր չէ կատարել:Տեղի է ունեցել փոխարժեքի փոփոխություն:
                        result.Add(new ActionError(121));
                    }
                }
                else
                {
                    rate = Utility.GetLastExchangeRate(this.Currency, rateType, ExchangeDirection.Buy);
                    if (rate != this.ConvertationRate1)
                    {
                        //Հնարավոր չէ կատարել:Տեղի է ունեցել փոխարժեքի փոփոխություն:
                        result.Add(new ActionError(121));
                    }
                }

            }

            else
            {
                rate = Utility.GetLastExchangeRate(this.Currency, RateType.Cross, ExchangeDirection.Buy);
                rate1 = Utility.GetLastExchangeRate(receiverAccountCurrency, RateType.Cross, ExchangeDirection.Sell);

                if (rate != this.ConvertationRate || rate1 != this.ConvertationRate1)
                {
                    //Հնարավոր չէ կատարել:Տեղի է ունեցել փոխարժեքի փոփոխություն:
                    result.Add(new ActionError(121));
                }
            }

            return result;
        }









        internal static void UpdateOrderCurrencyFastTransfer(double convertationRate, double convertationRate1, long docID)
        {
            ReceivedFastTransferPaymentOrderDB.UpdateOrderCurrencyFastTransfer(convertationRate, convertationRate1, docID);
        }

    }





}
