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
    /// Swift հաղորդագրության պատճենի ստացման հայտ
    /// </summary>
    public class SwiftCopyOrder:Order
    {
        /// <summary>
        /// Գործարքի համար
        /// </summary>
        public int ContractNumber { get; set; }
        /// <summary>
        /// Միջնորդավճար
        /// </summary>
        public string FeeAmount { get; set; }
        /// <summary>
        /// Միջնորդավճարի հաշիվ
        /// </summary>
        public Account FeeAccount { get; set; }
        /// <summary>
        /// Swift հաղորդագրության պատճենի ստացման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
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
                result = SwiftCopyOrderDB.Save(this, userName, source);
                long id = result.Id;
                result = base.SaveOrderFee();
                result.Id = id;
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }
        /// <summary>
        /// Swift հաղորդագրության պատճենի ստացման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateSwiftCopyOrder(this));
            
            return result;
        }
        /// <summary>
        /// Ստուգում է եղել ե այդպիսի Swift փոխանցում թե ոչ 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="CustomerNumber"></param>
        /// <returns></returns>
        public bool CheckForSwiftCopy(long Id,ulong CustomerNumber)
        {
            return SwiftCopyOrderDB.CheckForSwiftCopy(Id,CustomerNumber);
        }
        /// <summary>
        /// Swift հաղորդագրության պատճենի ստացման հայտի հաստատում
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
        /// Swift հաղորդագրության պատճենի ստացման հայտի հաստատման ստուգումներ
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
            if (!CheckForSwiftCopy(ContractNumber, CustomerNumber))
            {
                //Տվյալ գործարքի համարով միջազգային փոխանցում գոյություն չունի:
                result.Errors.Add(new ActionError(458));
            }

            if (SwiftCopyOrderDB.CheckSwiftCopy(this.ContractNumber) != true)
            {
                //Փոխանցումը դեռ վերջնական հաստատված չէ Բանկի կողմից (թղթակցային հարաբերություններ բաժնի կողմից)։
                result.Errors.Add(new ActionError(1709));
            }

            if (this.Fees[0].Type == 19)
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
        /// Վերադարձնում է Swift հաղորդագրության պատճենի ստացման հայտի տվյալները
        /// </summary>
        public void Get()
        {
            SwiftCopyOrderDB.Get(this);
            this.Fees = Order.GetOrderFees(this.Id);
        }

        /// <summary>
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.SubType = 1;

            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {                
                double swiftCopyOrderFee = ChequeBookOrder.GetOrderServiceFee(this.CustomerNumber, OrderType.SwiftCopyOrder, 0);
                this.Fees = new List<OrderFee>();
                OrderFee orderFee = new OrderFee();
                this.FeeAmount = swiftCopyOrderFee.ToString();
                orderFee.Amount = swiftCopyOrderFee;
                orderFee.Type = 19;
                orderFee.Account = this.FeeAccount;
                orderFee.Currency = "AMD";
                this.Fees.Add(orderFee);
                this.RegistrationDate = DateTime.Now;
            }

            this.Fees.ForEach(m =>
            {
                if (m.Type == 18)
                {
                    m.Account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.FeeAccount), "AMD", this.FilialCode);
                    m.OrderNumber = this.OrderNumber;                    
                }

            });

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
                result = SwiftCopyOrderDB.Save(this, userName, source);
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

            if(Source != SourceType.AcbaOnline || source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);

                try
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    //Կանխիկ գործարքների դեպքում ստուգում է սպասարկողի դրամարկղում կանխիկի սահմանաչափը 
                    if (result.ResultCode == ResultCode.Normal && this.Fees[0].Type == 18)
                    {
                        warnings.AddRange(user.CheckForNextCashOperation(this));
                    }
                }
                catch
                {

                }


                result.Errors = warnings;
            }

            
            return result;
        }

        public static byte[] PrintSwiftCopyOrderFile(long docID)
        {
            return SwiftCopyOrderDB.PrintSwiftCopyOrderFile(docID);
        }
    }
}
