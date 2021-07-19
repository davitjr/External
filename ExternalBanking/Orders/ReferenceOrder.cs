using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    ///  Տեղեկանքի ստացման հայտ
    /// </summary>
    public class ReferenceOrder:Order
    {
        /// <summary>
        /// Տեղեկանքի տեսակ
        /// </summary>
        public List<ushort> ReferenceTypes { get; set; }
        public ushort ReferenceType { get; set; }
        /// <summary>
        /// Շարժի սկզբնաժամկետ
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// Շարժի վերջնաժամկետ
        /// </summary>
        public DateTime? DateTo { get; set; }
        /// <summary>
        /// Հաշվեհամարներ
        /// </summary>
        public List<Account> Accounts { get; set; }
        /// <summary>
        /// Տեղեկանքը նեկայացվելու է
        /// </summary>
        public ushort ReferenceEmbasy { get; set; }
        /// <summary>
        /// Եթե ընտրվում ե տեղեկանքը ներկայացվելու է "ԱՅԼ" տարբերակը
        /// </summary>
        public string ReferenceFor { get; set; }
        /// <summary>
        /// Տեղեկանքի ստացման մասնաճյուղ
        /// </summary>
        public int ReferenceFilial { get; set; }
        /// <summary>
        /// Նույն օրվա ընթացքում
        /// </summary>
        public ushort Urgent { get; set; }
        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public double FeeAmount { get; set; }
        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }
        /// <summary>
        /// Տեղեկանքի լեզու
        /// </summary>
        public ushort ReferenceLanguage { get; set; }
        /// <summary>
        /// Վարկային գծի վերաբերյալ տեղեկատվություն
        /// </summary>
        public bool IncludeCreditLine { get; set; }
        /// <summary>
        /// Տեղեկանքի տեսակի նկարագրություն
        /// </summary>
        public string OtherTypeDescription { get; set; }
        /// <summary>
        /// Տեղեկանքի ստացման օր
        /// </summary>
        public DateTime? ReceiveDate { get; set; }
        /// <summary>
        /// Այլ վավերապայմաններ
        /// </summary>
        public string OtherRequisites { get; set; }

        public string ReferenceTypeDescription { get; set; }
        public string ReferenceLanguageDescription { get; set; }
        public string ReferenceEmbasyDescription { get; set; }
        public string ReferenceFilialDescription { get; set; }

        /// <summary>
        /// Տեղեկանքի ստացման եղանակ
        /// </summary>
        public ReferenceReceiptTypes ReferenceReceiptType { get; set; }

        /// <summary>
        /// Առաքման ամբողջական հասցե
        /// </summary>
        public string FullDeliveryAddress { get; set; }

        /// <summary>
        /// Անձը հաստատող փաստաթուղթ
        /// </summary>
        public string PassportDetails { get; set; }

        /// <summary>
        /// Տեղեկանքի ստացման եղանակի նկարագրություն
        /// </summary>
        public string ReferenceReceiptTypeDescription { get; set; }

        /// <summary>
        /// Տեղեկանքի ստացման հայտի պահպանւմ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        /// 
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result =this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

             Action action = this.Id == 0 ? Action.Add : Action.Update;

             using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
             {
                 result = ReferenceOrderDB.Save(this, userName, source);

                 ActionResult resultOPPerson = base.SaveOrderOPPerson();

                 if (resultOPPerson.ResultCode != ResultCode.Normal)
                 {
                     return resultOPPerson;
                 }

                 ActionResult resultFee = base.SaveOrderFee();

                 if (resultFee.ResultCode != ResultCode.Normal)
                 {
                     return resultFee;
                 }

                 LogOrderChange(user, action);
                 scope.Complete();
             }

            return result;
        }


        /// <summary>
        /// Տեղեկանքի ստացման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateReferenceOrder(this));
            return result;
        }
        /// <summary>
        /// Տեղեկանքի ստացման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
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

           

            return result;
        }


        /// <summary>
        /// Տեղեկանքի ստացման հայտի հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
            }
            if (this.Fees[0].Type == 15)
            {

                result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

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
        /// Վերադարձնում է տեղեկանքի ստացման հայտի տվյալները
        /// </summary>
        public void Get()
        {
            ReferenceOrderDB.Get(this);
            this.Fees = Order.GetOrderFees(this.Id);
            
        }


          /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.SubType = 1;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if (Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                double referenceOrderFee = ChequeBookOrder.GetOrderServiceFee(this.CustomerNumber, OrderType.ReferenceOrder, Convert.ToInt32(this.Urgent));
                this.Fees = new List<OrderFee>();
                OrderFee orderFee = new OrderFee();
                orderFee.Amount = referenceOrderFee;
                orderFee.Type = 15;
                orderFee.Account = this.FeeAccount;
                orderFee.Currency = "AMD";
                this.Fees.Add(orderFee);
                FeeAmount = referenceOrderFee;

                RegistrationDate = DateTime.Now;
                this.ReferenceTypes = new List<ushort>();
                this.ReferenceTypes.Add(this.ReferenceType);

                if (this.Urgent == 1)
                {
                    this.ReceiveDate = this.RegistrationDate;
                }
            }


            this.Fees.ForEach(m =>
            {
                if (m.Type == 14)
                {
                    m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", this.FilialCode);
                    m.OrderNumber = this.OrderNumber;
                    m.Description = Urgent == 1 ? "Նույն օրվա ընթացքում տեղեկանքի համար (" + CustomerNumber + ")" : "Տեղեկանքի համար (" + CustomerNumber + ")";
                }

            });

            if(ReferenceTypes.Count == 1 && ReferenceTypes.Contains(9))
            {
                ReferenceLanguage = 1;
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
            ActionResult result = this.Validate();
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
                result = ReferenceOrderDB.Save(this, userName, source);
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

                result = base.SaveOrderFee();

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
                if (result.ResultCode == ResultCode.Normal && this.Fees[0].Type == 14)
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


    }
}
