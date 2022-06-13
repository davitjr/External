using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.ServiceClient;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{

    /// <summary>
    /// Փաստաթղթի տեսակներ
    /// </summary>
    public enum DocumentType : byte
    {
        /// <summary>
        /// ՀԾՀ
        /// </summary>
        PublicServiceNumber = 1,

        /// <summary>
        /// ՀԾՀ չստանալու մասին տեղեկանքի համար
        /// </summary>
        ReferenceNotReceivingPSN = 2,

        /// <summary>
        /// Անձնագիր
        /// </summary>
        PassportNumber = 3,

        /// <summary>
        /// ՀՎՀՀ 
        /// </summary>
        TIN = 4
    }

    public class BudgetPaymentOrder : PaymentOrder
    {
        /// <summary>
        /// Ուղարկողի փաստաթղթի տեսակ 
        /// </summary>
        public byte PayerDocumentType { get; set; }

        /// <summary>
        /// Ուղարկողի փաստաթղթի համար  
        /// </summary>
        public string PayerDocumentNumber { get; set; }


        /// <summary>
        /// ՏՀՏ կոդ   
        /// </summary>
        public short LTACode { get; set; }

        /// <summary>
        /// Ոստիկանության կոդ 
        /// </summary>
        public int PoliceCode { get; set; }

        /// <summary>

        /// <summary>
        /// Ոստիկանության հարցման արդյունքում ստացված պատասխանի ունիկալ համար
        /// </summary>
        public long PoliceResponseDetailsId { get; set; }

        /// <summary>
        /// Ոստիկանության որոշման համար
        /// </summary>
        public string ViolationID { get; set; }

        /// <summary>
        /// Ոստիկանության որոշման ամսաթիվ
        /// </summary>
        public DateTime? ViolationDate { get; set; }

        /// <summary>
        /// Բռնագանձում է թե ոչ
        /// </summary>
        public bool Exaction { get; set; }

        /// <summary>
        /// ՀՏՀ կոդի նկարագրություն
        /// </summary>
        public string LTACodeDescription { get; set; }

        /// <summary>
        /// Ոստիկանության կոդի նկարագորություն
        /// </summary>
        public string PoliceCodeDescription { get; set; }

        /// <summary>
        /// մուտքագրվելու են թվեր և տառեր, առավելագույնը 15 նիշ։
        /// </summary>
        public string BadgeNumber { get; set; }

        /// <summary>
        /// մուտքագրվելու են թվեր և տառեր, առավելագույնը 15 նիշ։
        /// </summary>
        public bool IsPoliceInspectorAct { get; set; }

        public new void Get()
        {
            BudgetPaymentOrderDB.GetPaymentOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
            this.Fees = Order.GetOrderFees(this.Id);

            if (this.Fees.Exists(m => m.Type == 7))
            {
                this.CardFee = this.Fees.Find(m => m.Type == 7).Amount;
                this.Currency = this.Fees.Find(m => m.Type == 7).Currency;
                this.Fees.Find(m => m.Type == 7).Account = this.DebitAccount;
            }
            if (this.Fees.Exists(m => m.Type == 20))
            {
                this.TransferFee = this.Fees.Find(m => m.Type == 20).Amount;
                this.FeeAccount = this.Fees.Find(m => m.Type == 20).Account;
            }
        }

        private void Get(BudgetPaymentOrder order)
        {
            BudgetPaymentOrderDB.GetPaymentOrder(order);
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
            if (this.IsPoliceInspectorAct)
            {
                this.ReceiverAccount = new Account() { AccountNumber = Constants.POLICE_ACCOUNT_NUMBER_1 };
                this.ReceiverBankCode = 90001;
                string date = this.ViolationDate.HasValue ? this.ViolationDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                this.Description = "կրծքանշանի համար՝ " + this.BadgeNumber + ", որոշում` " + this.ViolationID + " " + date;
                this.Receiver = "Կենտրոնական գանձապետարան";
                this.LTACode = 99;
            }

            base.Complete();

            if (this.SubType == 6 && this.PoliceResponseDetailsId == 0)
            {
                this.SubType = 5;
            }

            if (this.Exaction)
            {
                this.TransferFee = 0;
                this.CardFee = 0;
            }
            this.GetPayerDocumentNumber();

            ActionResult result = this.Validate();


            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                ActionResult resultOrder = BudgetPaymentOrderDB.Save(this, userName, source);
                if (result.ResultCode == ResultCode.Normal)
                {
                    result = base.SaveOrderFee();
                }
                else
                {
                    return result;
                }

                LogOrderChange(user, action);
                scope.Complete();

                result.Id = resultOrder.Id;
            }
            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի ստուգում
        /// </summary>
        /// <returns></returns>
        public new ActionResult Validate(TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));

                result.Errors.AddRange(Validation.ValidateOPPerson(this, this.ReceiverAccount, this.DebitAccount.AccountNumber));
            }


            if (this.CreditorStatus == 0 || this.CreditorStatus == 10)
            {
                if (this.ReceiverAccount.AccountNumber == "900008000490")
                {
                    string taxCode = null;

                    var cust = ACBAOperationService.GetCustomer(this.CustomerNumber);

                    if (cust.customerType.key == 6)
                    {
                        PhysicalCustomer physicalCustomer = (PhysicalCustomer)cust;
                        Person person = physicalCustomer.person;
                        if (person.documentList.Find(cd => cd.documentType.key == 19) != null)
                            taxCode = Utility.ConvertAnsiToUnicode(person.documentList.Find(cd => cd.documentType.key == 19).documentNumber);
                    }

                    if (taxCode == null && cust.customerType.key == 6)
                    {
                        result.Errors.Add(new ActionError(1575, new string[] { "900008000490" }));
                    }

                    if (CreditorCustomerNumber != default)
                    {
                        var customerType = ACBAOperationService.GetCustomerType(CreditorCustomerNumber);
                        var creditorCustomerDocuments = ACBAOperationService.GetCustomerDocumentList(CreditorCustomerNumber);

                        if (customerType == 6)
                        {
                            if (creditorCustomerDocuments.Find(cd => cd.documentType.key == 19) != null)
                                taxCode = Utility.ConvertAnsiToUnicode(creditorCustomerDocuments.Find(cd => cd.documentType.key == 19).documentNumber);
                        }

                        if (taxCode == null && customerType == 6)
                        {
                            result.Errors.Add(new ActionError(1575, new string[] { "900008000490" }));
                        }
                    }
                }

            }

            if (this.ReceiverAccount != null && this.DebitAccount != null && this.ReceiverAccount.AccountNumber == this.DebitAccount.AccountNumber)
            {
                //Կրեդիտ և դեբիտ հաշիվները նույնն են:
                result.Errors.Add(new ActionError(11));
            }
            else
            {

                if (this.Type != OrderType.CashForRATransfer)
                {
                    //Դեբետ հաշվի ստուգում
                    result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
                }
                //else
                //{
                //    result.Errors.AddRange(Validation.CheckCustomerDebts(this.CustomerNumber));
                //}

                //Կրեդիտ հաշվի ստուգում
                result.Errors.AddRange(Validation.ValidateReceiverAccount(this));

                if (result.Errors.Count > 0)
                {
                    return result;
                }

            }

            //Սահմանափակ հասանելիությամ հաշվիներ
            if (this.DebitAccount.TypeOfAccount == 283 && this.Amount >= 400000)
            {   //Սահամանափակ հասանելիությամբ հաշիվներից գործարքի գումարը չի կարող գերազանցել 400,000 ՀՀ դրամը
                result.Errors.Add(new ActionError(1781));
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

            if (string.IsNullOrEmpty(this.Currency))
            {
                result.Errors.Add(new ActionError(254));
            }

            if (this.Currency != "AMD")
            {
                result.Errors.Add(new ActionError(723));
            }

            if (this.Amount <= 0 && templateType != TemplateType.CreatedByCustomer)
            {
                result.Errors.Add(new ActionError(22));
            }
            else if (!Utility.IsCorrectAmount(this.Amount, this.Currency))
            {
                result.Errors.Add(new ActionError(25));
            }


            if ((this.Type == OrderType.RATransfer || this.Type != OrderType.CashForRATransfer) && this.SubType != 3)
            {

                Customer customer = new Customer();

                if (customer.GetPaymentOrderFee(this) == -1)
                {
                    //Սակագին նախատեսված չէ:Ստուգեք փոխանցման տվյալները
                    result.Errors.Add(new ActionError(659));
                    return result;
                }


                if (this.Currency != "AMD" && this.UrgentSign)
                {//Շտապ փոխանցումներ իրականացնել հնարավոր է միայն ՀՀ դրամով

                    result.Errors.Add(new ActionError(503));
                }
                else if (templateType != TemplateType.CreatedByCustomer && ((this.Currency != "AMD" && this.TransferFee != 0) || (this.Currency == "AMD" && this.TransferFee != 0) || (this.Source == SourceType.Bank && this.TransferFee != 0)))
                {//Երե պահանջվում է միջնորդավճարի հաշիվ
                    result.Errors.AddRange(Validation.ValidateFeeAccount(this.CustomerNumber, this.FeeAccount));
                }

            }



            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            //Նկարագրություն և Ստացող դաշտերի ստուգում միայն Փոխանցում ՀՀ տարածքում տեսակի համար
            if ((this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer) && this.SubType != 3)
                result.Errors.AddRange(ValidateTextData());

            //6 մլն-ից բարձր փոխանցումներ ֆայլի առկայություն: 
            result.Errors.AddRange(Validation.ValidateAttachmentDocument(this));

            //Բյուջետային փոխանցման համար անհրաժեշտ դաշտերի ստուգում (աշխատում է նաև ՃՈ փոխանցումների համար)
            if ((this.Type == OrderType.RATransfer || this.Type == OrderType.CashForRATransfer) && (this.SubType == 5 || this.SubType == 6) && ((this.ReceiverBankCode == 10300 && this.ReceiverAccount.AccountNumber.ToString()[5] == '9') || this.ReceiverBankCode.ToString()[0] == '9'))  //Karen
            {
                string TIN = "";
                long output;
                short customerType = 0;
                short customerResidence = 0;
                int creditorCustomerType = 0;

                if (this.LTACode == 0)
                {
                    //ՏՀՏ կոդը լրացված չէ: Հարկային և մաքսային ծառայություններ փոխանցումներ կատարելու համար անհրաժեշտ է պարտադիր նշել տեսչության կոդը: Այլ փոխանցումների դեպքում պետք է լրացնել «99 Այլ»:
                    result.Errors.Add(new ActionError(356));
                }

                if (this.SubType == 6 && this.PoliceResponseDetailsId != 0)
                {


                    VehicleViolationResponse violationResponse = VehicleViolationResponse.GetVehicleViolationResponseDetails(this.PoliceResponseDetailsId);

                    if (violationResponse != null)
                    {

                        if (this.Amount > violationResponse.RequestedAmount && violationResponse.ResponseId != -1)
                        {
                            result.Errors.Add(new ActionError(25));
                        }
                        if ((this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1) && (String.IsNullOrEmpty(this.ViolationID) || this.ViolationDate == null) && violationResponse.ResponseId == -1)
                        {
                            //Որոշման համարը և/կամ ամսաթիվը մուտքագրված չեն
                            result.Errors.Add(new ActionError(1008));
                        }
                    }
                    else // TODO եթե չի գտնվել ոստիկանության հարցումը
                    {

                    }

                    if (this.ReceiverAccount.AccountNumber != Constants.POLICE_ACCOUNT_NUMBER && this.ReceiverAccount.AccountNumber != Constants.POLICE_ACCOUNT_NUMBER_1)
                    {
                        //Մուտքագրվող (կրեդիտ)  հաշիվը սխալ է մուտքագրված:
                        result.Errors.Add(new ActionError(19));
                    }


                    short errorId = VehicleViolationResponseDB.CheckVehicleViloationOperation(this.PoliceResponseDetailsId);
                    if (errorId != 0 && violationResponse.ResponseId != -1)
                        result.Errors.Add(new ActionError(errorId));

                }
                else
                {

                    if ((this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1) && (String.IsNullOrEmpty(this.ViolationID) || this.ViolationDate == null))
                    {
                        //Որոշման համարը և/կամ ամսաթիվը մուտքագրված չեն
                        result.Errors.Add(new ActionError(1008));
                    }
                }

                if (this.CustomerNumber != 0)
                {
                    var customer = ACBAOperationService.GetCustomer(this.CustomerNumber);
                    if (customer.customerType.key != (short)CustomerTypes.physical)
                    {
                        LegalCustomer legalCustomer = (LegalCustomer)customer;
                        TIN = legalCustomer.CodeOfTax;
                    }

                    customerType = customer.customerType.key;
                    customerResidence = customer.residence.key;

                }
                else
                {
                    customerType = (short)CustomerTypes.physical;
                    customerResidence = this.OPPerson.PersonResidence;
                }


                if (customerType == (short)CustomerTypes.physical)
                {
                    if (customerResidence == 1)
                    {
                        if (this.PayerDocumentType == 1)
                        {
                            if (this.PayerDocumentNumber != null && this.PayerDocumentNumber.Trim().Length != 10)
                            {   //ՀԾՀ դաշտը սխալ է լրացված
                                result.Errors.Add(new ActionError(405));
                            }

                            bool isNumeric = Int64.TryParse(String.IsNullOrEmpty(this.PayerDocumentNumber) ? "" : this.PayerDocumentNumber.Trim(), out output);

                            if (!isNumeric)
                            {//ՀԾՀ-ն մուտքագրեք թվերով:
                                result.Errors.Add(new ActionError(471));
                            }

                        }
                        else if (this.PayerDocumentType == 2 && this.PayerDocumentNumber != null && this.PayerDocumentNumber.Trim().Length != 10)
                        {  //ՀԾՀ չստանալու մասին տեղեկանքի համար դաշտը սխալ է լրացված:
                            result.Errors.Add(new ActionError(472));
                        }


                        if (Account.CheckAccountForPSN(this.ReceiverBankCode == 10300 ? this.ReceiverAccount.AccountNumber.ToString().Substring(5) : this.ReceiverAccount.AccountNumber.ToString()))
                        {
                            if ((this.PayerDocumentType != 1 && this.PayerDocumentType != 2) || String.IsNullOrEmpty(this.PayerDocumentNumber))
                            {//ՀԾՀ / ՀԾՀ չստանալու մասին տեղեկանքի համար դաշտը լրացված չէ:
                                result.Errors.Add(new ActionError(470));
                            }
                        }
                        if (!String.IsNullOrEmpty(this.OPPerson.PersonSocialNumber) && !String.IsNullOrEmpty(this.OPPerson.PersonNoSocialNumber))
                        {
                            //Լրացրեք հաճախորդի միայն ՀԾՀ-ն կամ ՀԾՀ-ից հրաժարվելու տեղեկանքը
                            result.Errors.Add(new ActionError(762));
                        }

                    }
                    else
                    {
                        if (this.Type == OrderType.CashForRATransfer && this.OPPerson.CustomerNumber == 0 && this.CustomerNumber == 0)
                        {//Լրացրեք հաճախորդի համարը :
                            result.Errors.Add(new ActionError(485));
                        }

                        if (this.PayerDocumentType != 3 || String.IsNullOrEmpty(this.PayerDocumentNumber))
                        {//Հաճախորդի անձը հաստատող փաստաթուղթը գտնված չէ
                            result.Errors.Add(new ActionError(751));
                        }
                    }


                }



                if (this.CreditorStatus == 10 || this.CreditorStatus == 20)
                    creditorCustomerType = 6;
                else if (this.CreditorStatus == 13 || this.CreditorStatus == 23)
                    creditorCustomerType = 2;
                else if (this.CreditorStatus != 10 && this.CreditorStatus != 20 && this.CreditorStatus != 0)
                    creditorCustomerType = 1;

                if ((this.ReceiverBankCode == 10300 && this.ReceiverAccount.AccountNumber.ToString()[5] == '9') || this.ReceiverBankCode.ToString().Substring(0, 3) == "900")
                {
                    short transferVerification = (short)PaymentOrderDB.CheckTransferVerification(this.ReceiverBankCode == 10300 ? Convert.ToDouble(this.ReceiverAccount.AccountNumber.ToString().Substring(5)) : Convert.ToDouble(this.ReceiverAccount.AccountNumber), this.LTACode, customerType, TIN, creditorCustomerType, this.CreditorDocumentType == 4 ? this.CreditorDocumentNumber : "");
                    if (transferVerification != 1)
                    {
                        result.Errors.Add(new ActionError(transferVerification));
                    }
                    if (AccountDB.IsPoliceAccount(this.ReceiverBankCode == 10300 ? this.ReceiverAccount.AccountNumber.ToString().Substring(5) : this.ReceiverAccount.AccountNumber.ToString()) && this.PoliceCode == 0)
                    {//Հաշվի լրացուցիչ մասն ընտրված չէ
                        result.Errors.Add(new ActionError(1084));
                    }
                }

                if (this.CreditorStatus == 10)
                {
                    if (String.IsNullOrEmpty(this.CreditorDescription))
                    {//§Պարտատիրոջ¦ անուն ազգանուն¦ դաշտը լրացված չէ
                        result.Errors.Add(new ActionError(400));
                    }

                    if ((this.CreditorDocumentType == 1 || this.CreditorDocumentType == 2) && String.IsNullOrEmpty(this.CreditorDocumentNumber))
                    {//Պարտատիրոջ ՀԾՀ-ն լրացված չէ
                        result.Errors.Add(new ActionError(475));
                    }

                    if (Account.CheckAccountForPSN(this.ReceiverBankCode == 10300 ? this.ReceiverAccount.AccountNumber.ToString().Substring(5) : this.ReceiverAccount.AccountNumber.ToString()) && this.CreditorDocumentType != 1 && this.CreditorDocumentType != 2 && String.IsNullOrEmpty(this.CreditorDeathDocument))
                    {//Պարտատիրոջ ՀԾՀ / Պարտատիրոջ ՀԾՀ չստանալու մասին տեղեկանքի համար դաշտը լրացված չէ:
                        result.Errors.Add(new ActionError(475));
                    }
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

                    if (String.IsNullOrEmpty(this.CreditorDocumentNumber) && this.ReceiverAccount.AccountNumber == "900008000490")
                    {//§Պարտատիրոջ ՀՎՀՀ¦ դաշտը լրացված չէ
                        result.Errors.Add(new ActionError(402));
                    }
                }
                else if (this.CreditorStatus == 20 || ForThirdPerson)
                {
                    if (String.IsNullOrEmpty(this.CreditorDocumentNumber))
                    {//§Պարտատիրոջ անձնագիրը¦ դաշտը լրացված չէ
                        result.Errors.Add(new ActionError(409));
                    }
                    if (String.IsNullOrEmpty(this.CreditorDescription))
                    {//§Պարտատիրոջ¦ անուն ազգանուն¦ դաշտը լրացված չէ
                        result.Errors.Add(new ActionError(400));
                    }
                }
                else if (creditorCustomerType == 2 || creditorCustomerType == 1)
                {
                    if (String.IsNullOrEmpty(this.CreditorDocumentNumber))
                    {//§Պարտատիրոջ ՀՎՀՀ¦ դաշտը լրացված չէ
                        result.Errors.Add(new ActionError(402));
                    }
                    else if (this.CreditorDocumentNumber.Trim().Length != 8)
                    {//Պարտատիրոջ ՀՎՀՀ դաշտը պետք է լինի 8 նիշ
                        result.Errors.Add(new ActionError(443));
                    }
                }

            }


            if (this.DebitAccount.AccountType == 115 && !this.Exaction)
            {

                //ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվից փոխանցում հնարավոր է կատարել միայն բռնագանձման համար
                result.Errors.Add(new ActionError(1065));
            }


            if (this.CustomerNumber != 0 && this.Type != OrderType.RATransfer)
            {
                result.Errors.AddRange(Validation.CheckCustomerDebtsAndDahk(this.CustomerNumber));
                result.Errors.RemoveAll(m => m.Code == 743);
            }



            if (Source == SourceType.Bank && this.DebitAccount.IsCardAccount())
            {

                Card card = Card.GetCardWithOutBallance(DebitAccount.AccountNumber);
                if (card.RelatedOfficeNumber == 2405 || card.RelatedOfficeNumber == 2572 || card.RelatedOfficeNumber == 2573 || card.RelatedOfficeNumber == 2574)
                {
                    result.Errors.Add(new ActionError(1525));
                }

                if (this.ReasonId == 0)
                {
                    //Պատճառ դաշտը ընտրված չէ։
                    result.Errors.Add(new ActionError(1523));
                }

                if (this.ReasonId == 99 && string.IsNullOrEmpty(this.ReasonIdDescription))
                {
                    //Պատճառի այլ դաշտը լրացված չէ։
                    result.Errors.Add(new ActionError(1524));
                }


            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            return result;
        }

        /// <summary>
        /// Փաստաթղթի որոշում
        /// </summary>
        /// <returns></returns>
        public void GetPayerDocumentNumber()
        {
            if (this.CustomerNumber != 0)
            {
                short customerType = ACBAOperationService.GetCustomerType(this.CustomerNumber);

                if (customerType == (short)CustomerTypes.physical)
                {
                    var customer = (ACBAServiceReference.PhysicalCustomer)ACBAOperationService.GetCustomer(this.CustomerNumber);

                    if (customer.residence.key == 1)
                    {
                        if (customer.person.documentList.Exists(cd => cd.documentType.key == 56))
                        {
                            this.PayerDocumentType = (byte)DocumentType.PublicServiceNumber;
                            this.PayerDocumentNumber = Utility.ConvertAnsiToUnicode(customer.person.documentList.Find(cd => cd.documentType.key == 56).documentNumber);
                        }
                        else if (customer.person.documentList.Exists(cd => cd.documentType.key == 57))
                        {
                            this.PayerDocumentType = (byte)DocumentType.ReferenceNotReceivingPSN;
                            this.PayerDocumentNumber = Utility.ConvertAnsiToUnicode(customer.person.documentList.Find(cd => cd.documentType.key == 57).documentNumber);
                        }
                    }
                    else
                    {
                        this.PayerDocumentType = (byte)DocumentType.PassportNumber;
                        this.PayerDocumentNumber = Utility.ConvertAnsiToUnicode(customer.DefaultDocument);
                    }
                }
            }
            else
            {
                if (this.OPPerson != null && this.OPPerson.PersonResidence == 1)
                {
                    if (!string.IsNullOrEmpty(this.OPPerson.PersonSocialNumber))
                    {
                        this.PayerDocumentType = (byte)DocumentType.PublicServiceNumber;
                        this.PayerDocumentNumber = Utility.ConvertAnsiToUnicode(this.OPPerson.PersonSocialNumber);
                    }
                    else if (!string.IsNullOrEmpty(this.OPPerson.PersonNoSocialNumber))
                    {
                        this.PayerDocumentType = (byte)DocumentType.ReferenceNotReceivingPSN;
                        this.PayerDocumentNumber = Utility.ConvertAnsiToUnicode(this.OPPerson.PersonNoSocialNumber);
                    }
                }
                else if (!string.IsNullOrEmpty(this.OPPerson.PersonDocument))
                {
                    this.PayerDocumentType = (byte)DocumentType.PassportNumber;
                    this.PayerDocumentNumber = Utility.ConvertAnsiToUnicode(this.OPPerson.PersonDocument);
                }

            }

        }

        /// <summary>
        /// հանձնարարականի ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public new ActionResult Approve(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.ForPoliceTransferWithoutRequest();

            ActionResult result = this.ValidateForSend(user);

            //ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվներից ելքի ժամանակ երբ ընտրված է բռնագանձումը
            //հեռացնում ենք հաճախորդի պարտավորությունները
            if (this.DebitAccount.AccountType == 115 && this.Exaction)
            {
                result.Errors.RemoveAll(m => m.Code == 744);
                result.Errors.RemoveAll(m => m.Code == 745);
            }

            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
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

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }

            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && this.Type == OrderType.CashForRATransfer)
                {
                    warnings.AddRange(user.CheckForNextCashOperation(this));
                }
            }
            catch
            {

            }

            result.Errors = warnings;

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

            if (this.SubType == 6 && this.PoliceResponseDetailsId == 0)
            {
                this.SubType = 5;
            }
            if (this.Exaction)
            {
                this.TransferFee = 0;
                this.CardFee = 0;
            }
            this.ForPoliceTransferWithoutRequest();

            this.GetPayerDocumentNumber();
            ActionResult result = this.Validate();


            //ԴԱՀԿ արգելանքի տակ գտնվող քարտի տարանցիկ հաշիվներից ելքի ժամանակ երբ ընտրված է բռնագանձումը
            //հեռացնում ենք հաճախորդի պարտավորությունները
            if (this.DebitAccount.AccountType == 115 && this.Exaction)
            {
                result.Errors.RemoveAll(m => m.Code == 744);
                result.Errors.RemoveAll(m => m.Code == 745);
            }

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
                if (this.Type == OrderType.CashForRATransfer)
                {
                    result = BudgetPaymentOrderDB.SaveCash(this, userName, source);
                }
                else
                {
                    result = BudgetPaymentOrderDB.Save(this, userName, source);

                }
                result = base.SaveOrderFee();

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

                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }

            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && this.Type == OrderType.CashForRATransfer)
                {
                    warnings.AddRange(user.CheckForNextCashOperation(this));
                }
            }
            catch
            {

            }

            result.Errors = warnings;

            return result;
        }

        public static int GetPoliceResponseDetailsIDWithoutRequest(string violationID, DateTime? violationDate, string badgeNumber, int responseDetailsID = 0)
        {
            return BudgetPaymentOrderDB.GetPoliceResponseDetailsIDWithoutRequest(violationID, violationDate, badgeNumber, responseDetailsID);
        }


        /// <summary>
        /// Վճարման հանձնարարականի ստուգում
        /// </summary>
        /// <returns></returns>
        internal void ForPoliceTransferWithoutRequest()
        {

            if (this.PoliceResponseDetailsId != 0)
            {
                VehicleViolationResponse violationResponse = VehicleViolationResponse.GetVehicleViolationResponseDetails(this.PoliceResponseDetailsId);
                if (violationResponse != null)
                {

                    if (violationResponse.ResponseId == -1)
                    {
                        this.SubType = 5;
                        this.PoliceResponseDetailsId = Convert.ToInt64(GetPoliceResponseDetailsIDWithoutRequest(this.ViolationID, this.ViolationDate, this.BadgeNumber, Convert.ToInt32(this.PoliceResponseDetailsId)));
                    }

                }
            }
            else if (this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER || this.ReceiverAccount.AccountNumber == Constants.POLICE_ACCOUNT_NUMBER_1)
            {
                this.PoliceResponseDetailsId = Convert.ToInt64(GetPoliceResponseDetailsIDWithoutRequest(this.ViolationID, this.ViolationDate, this.BadgeNumber));
            }


        }

        public static string GetOrderViolationID(long policeResponseId)
        {
            return Utility.ConvertAnsiToUnicode(BudgetPaymentOrderDB.GetOrderViolationId(policeResponseId));
        }
    }

}
