using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.UtilityPaymentsManagment;
using ExternalBanking.UtilityPaymentsServiceReference;

namespace ExternalBanking
{
    public class UtilityPaymentOrder :Order
    {
        /// <summary>
        /// Սպ. վճար (գազի դեպքում)
        /// </summary>
        public double ServiceAmount { get; set; }

        /// <summary>
        /// Պայմանագրի/Աբոնենտի համար
        /// </summary>
		public string Code { get; set; }

        /// <summary>
        /// Աբոնենտի մասնաճյուղ
        /// </summary>
		public string Branch { get; set; }

        /// <summary>
        /// Աբոնենտի տեսակը (ֆիզ.անձ,Իրավաբանական անձ)
        /// </summary>
		public int AbonentType { get; set; }

        /// <summary>
        /// Դեբետ (ելքագրվող) հաշիվ
        /// </summary>
      //  public override Account DebitAccount { get; set; }

        /// <summary>
        /// Կոմունալի տեսակ
        /// </summary>
		public CommunalTypes CommunalType { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account ReceiverAccount { get; set; }

        /// <summary>
        /// Կախավճարային (0 - ոչ, 1 - այո)
        /// </summary>
        public int PrepaidSign { get; set; }

        /// <summary>
        /// Վճարման տեսակ օր՝ անդամավճար կամ ոռոգմնան վճար
        /// </summary>
        public ushort PaymentType { get; set; }

        /// <summary>
        /// Աբոնենտի բանկային մասնաճյուղ
        /// </summary>
        public ushort AbonentFilialCode { get; set; }


        /// <summary>
        /// Վճարման ժամը
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public DateTime PaymentTime { get; set; }

        /// <summary>
        /// Պարտք
        /// </summary>
        public decimal?  Debt { get; set; }

        /// <summary>
        /// Օգտագործել վարկային գծերի միջոցները
        /// </summary>
        public bool UseCreditLine { get; set; }


        public ActionResult Validate(User user, TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (Account.CheckAccessToThisAccounts(this.DebitAccount?.AccountNumber) == 119)
            {
                //Նշված հաշվից ելքերն արգելված են
                result.Errors.Add(new ActionError(1966));
                return result;
            }

            if ((this.CommunalType == CommunalTypes.Gas || this.CommunalType == CommunalTypes.ENA) && string.IsNullOrEmpty(this.Description) && this.Source == SourceType.MobileBanking)
            {
                result.Errors.Add(new ActionError(1819));
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if(CommunalType == CommunalTypes.Orange)
            {
                if (string.IsNullOrEmpty(Code))
                {
                    result.Errors.Add(new ActionError(1805));
                }
            }


            result.Errors.AddRange(Validation.ValidateCashOperationAvailability(this, user));

            if(templateType == TemplateType.None)
            {
                result.Errors.AddRange(Validation.ValidateDocumentNumber(this, this.CustomerNumber));
                result.Errors.AddRange(Validation.ValidateOPPerson(this, debitAccountNumber: this.DebitAccount.AccountNumber));
            }


            if (this.Type != OrderType.CashCommunalPayment && this.Type != OrderType.ReestrCashCommunalPayment)
            {
                result.Errors.AddRange(Validation.ValidateDebitAccount(this, this.DebitAccount));
            }

            //Սահմանափակ հասանելիությամ հաշվիներ
            if (DebitAccount.TypeOfAccount == 283 && Amount >= 400000)
            {
                //Սահամանափակ հասանելիությամբ հաշիվներից գործարքի գումարը չի կարող գերազանցել 400,000 ՀՀ դրամը
                result.Errors.Add(new ActionError(1781));
            }


            result.Errors.AddRange(Validation.ValidateOPPerson(this, debitAccountNumber: this.DebitAccount.AccountNumber));

            if (string.IsNullOrEmpty(this.Currency))
            {
                result.Errors.Add(new ActionError(254));
            }
            if ((Amount + ServiceAmount) <= 0)
                result.Errors.Add(new ActionError(22));
            else if (Amount < 100 && (CommunalType == CommunalTypes.ArmenTel || CommunalType == CommunalTypes.BeelineInternet || CommunalType == CommunalTypes.Orange || CommunalType == CommunalTypes.UCom))
                result.Errors.Add(new ActionError(319));
            else if (Amount < 50 && CommunalType == CommunalTypes.VivaCell)
                result.Errors.Add(new ActionError(334));


            if (CommunalType == 0)
                result.Errors.Add(new ActionError(318));

            if (!Utility.IsCorrectAmount(Amount, Currency))
                result.Errors.Add(new ActionError(25));

            if(ServiceAmount > 0)
            {
                if (!Utility.IsCorrectAmount(ServiceAmount, Currency))
                    result.Errors.Add(new ActionError(1676));
            }

            if (Amount != Math.Truncate(Amount) && Currency == "AMD" && (CommunalType == CommunalTypes.VivaCell || CommunalType == CommunalTypes.Orange))
                result.Errors.Add(new ActionError(337));

            if (this.Type != OrderType.ReestrCashCommunalPayment && this.Type != OrderType.ReestrCommunalPayment && this.CommunalType != CommunalTypes.ArmenTel && this.CommunalType != CommunalTypes.BeelineInternet)
            {
                KeyValuePair<ActionResult, List<string>> res = new KeyValuePair<ActionResult, List<string>>();
                if (CommunalType == CommunalTypes.Gas && AbonentType == 1)
                {
                    ActionResult actionResult = new ActionResult();
                    ActionError actionError = new ActionError();
                    actionResult.Errors.Add(actionError);

                    List<string> resultList = new List<string>();
                    List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();
                    if (Code.Length != 6)
                        result.Errors.Add(new ActionError(315));
                    else
                    {
                        gazList = CommunalDB.SearchFullCommunalGas(Code, Branch);

                        if (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
                        {
                            if ((gazList[0].CurrentServiceFeeDebt > 0 && ServiceAmount > 0) || (ServiceAmount > Math.Abs(gazList[0].CurrentServiceFeeDebt) && gazList[0].CurrentServiceFeeDebt <= 0))
                                result.Errors.Add(new ActionError(1716));
                        }

                        if (gazList.Count == 1)
                        {
                            resultList.Add(gazList[0].Name + gazList[0].Name);
                            resultList.Add(gazList[0].SectionCode);
                            resultList.Add(gazList[0].CurrentGasDebt < 0 ? Math.Abs(gazList[0].CurrentGasDebt).ToString() : "0");
                            resultList.Add(gazList[0].CurrentServiceFeeDebt < 0 ? Math.Abs(gazList[0].CurrentServiceFeeDebt).ToString() : "0");

                            actionError.Code = 1;
                        }
                        else if (gazList.Count > 1)
                        {
                            result.Errors.Add(new ActionError(317));
                        }
                        else
                        {
                            result.Errors.Add(new ActionError(316));
                        }
                    }

                    res = new KeyValuePair<ActionResult, List<string>>(actionResult, resultList);
                }
                else
                {
                    res = UtilityPaymentOrderDB.GetCommunalSearchedName(CommunalType, true, AbonentType, Code, Branch);
                }

                if (res.Key.Errors[0].Code == (short)ResultCode.Normal)
                {

                    if (CommunalType == CommunalTypes.VivaCell && Amount > double.Parse(res.Value[2]) && double.Parse(res.Value[2]) != 0)
                        result.Errors.Add(new ActionError(336, new string[] { res.Value[2] }));
                }
            }

            if (Source == SourceType.SSTerminal && base.IsPaymentIdUnique())
            {
                result.Errors.Add(new ActionError(1498));
            }


            if (CommunalType == CommunalTypes.Gas && AbonentType == 2 && ServiceAmount > 0)
                result.Errors.Add(new ActionError(1715));

            if (this.Id != 0)
            {
                OrderQuality quality = OrderDB.GetOrderQualityByDocID(this.Id);
                if (quality != OrderQuality.Draft)
                {
                    result.Errors.Add(new ActionError(29));
                }
            }

            return result;
        }

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
                result = UtilityPaymentOrderDB.SaveUtilityPaymentOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        internal void Get()
        {
            UtilityPaymentOrderDB.GetUtilityPaymentOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);

        }

        public new ActionResult Approve(short schemaType, string userName,ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
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

        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline)
            {

                if (this.CommunalType == CommunalTypes.ArmenTel)
                {
                    BeelineAbonentSearch beelineAbonentSearch = new BeelineAbonentSearch();
                    string debt = "";
                    if(beelineAbonentSearch != null)
                    {
                        debt = beelineAbonentSearch.GetBeelineAbonentBalance(this.Code)?.Balance?.ToString();
                    }
                   
                    if (string.IsNullOrEmpty(debt))
                    {
                        result.Errors.Add(new ActionError(316));
                    }
                }
            }

            if (this.Type != OrderType.CashCommunalPayment && this.Type != OrderType.ReestrCashCommunalPayment)
            {

                result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

            }


            if (CommunalType == CommunalTypes.Gas && (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline) && AbonentType == 1)
            {
                double validServiceAmount = 0;

                List<GasPromAbonentSearch> gazList = new List<GasPromAbonentSearch>();
                validServiceAmount = CommunalDB.SearchFullCommunalGas(Code, Branch)[0].CurrentServiceFeeDebt;

                if ((validServiceAmount > 0 && ServiceAmount > 0) || (ServiceAmount > Math.Abs(validServiceAmount) && validServiceAmount <= 0))
                    result.Errors.Add(new ActionError(1716));

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
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        public void Complete()
        {
            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                SearchCommunal searchCommunal = new SearchCommunal()
                {
                    AbonentType = (short)AbonentType,
                    CommunalType = (CommunalTypes)CommunalType,
                    AbonentNumber = Code,
                    PhoneNumber = PhoneNumber,
                    Branch = Branch,
                    PaymentType = PaymentType
                };
                Description = searchCommunal.GetCommunalPaymentDescription();
            }

            if (this.Type == OrderType.CashCommunalPayment || this.Type == OrderType.ReestrCashCommunalPayment)
            {
                if (this.Source == SourceType.SSTerminal || this.Source == SourceType.CashInTerminal)
                {
                    //this.DebitAccount = SSTerminal.GetOperationSystemAccount(this.TerminalID, "AMD");
                }
                else
                {
                    this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
                }


            }

            ushort abonentType = ushort.Parse(this.AbonentType.ToString());



            if (this.CommunalType == CommunalTypes.COWater)
            {
                string branchID = SearchCommunal.GetCOWaterBranchID(this.Branch, this.AbonentFilialCode.ToString());
                this.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), "AMD", this.AbonentFilialCode, abonentType, "0", branchID);
            }
            else
            {
                this.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), "AMD", user.filialCode, abonentType, "0", this.Branch);
            }

            if (this.Source == SourceType.MobileBanking || (this.Source == SourceType.Bank && this.OPPerson == null) || Source==SourceType.AcbaOnline)
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }

            if(this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline)
            {
                this.RegistrationDate = DateTime.Now.Date;
            }
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում և ուղղարկում
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

            result = this.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = UtilityPaymentOrderDB.SaveUtilityPaymentOrder(this, userName, source);

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
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }

            //Ժամանակավոր մինչև բոլորի ավտոմատ մասը լինի
            result = base.Confirm(user);

            try
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                if (result.ResultCode == ResultCode.Normal && this.Type == OrderType.CashCommunalPayment)
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
        /// Վերադարձնում է ՋՕԸ ի վճարվող գումարը կախված վճարման տեսակից
        /// </summary>
        /// <param name="abonentNumber"></param>
        /// <param name="branchCode"></param>
        /// <param name="paymentType"></param>
        /// <returns></returns>
        public static double GetCOWaterOrderAmount(string abonentNumber, string branchCode, ushort paymentType)
        {
            return UtilityPaymentOrderDB.GetCOWaterPaymentAmount(abonentNumber, branchCode, paymentType);
        }

        /// <summary>
        /// Վերադարձնում է կոմուալ վճարման աբոնենտի տեսակը 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static int GetOrderAbonentType(long orderId)
        {
            return UtilityPaymentOrderDB.GetOrderAbonentType(orderId);
        }


    }
}
