using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{
    public class PeriodicBudgetPaymentOrder:PeriodicOrder
    {

        public BudgetPaymentOrder BudgetPaymentOrder { get; set; }
        /// <summary>
        /// Հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult SavePeriodicBudgetPaymentOrder(string userName, SourceType source, ACBAServiceReference.User user)
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
                result = PeriodicTransferOrderDB.SavePeriodicBudgetPaymentOrder(this, userName, source);

                if (this.FeeAccount != null || (this.ChargeType == 2 && this.Fee == -1 && this.FeeAccount != null))
                {
                    this.Fees = new List<OrderFee>();
                    OrderFee fee = new OrderFee();
                    fee.Amount = this.Fee;
                    fee.Account = this.FeeAccount;
                    fee.Currency = this.FeeAccount.Currency;
                    fee.Type = 20;
                    this.Fees.Add(fee);
                }
                
                base.SaveOrderFee();

                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;

        }
        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.BudgetPaymentOrder.OPPerson = this.OPPerson;
            this.DebitAccount = this.BudgetPaymentOrder.DebitAccount;
            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking)
            {
                this.StartDate = Utility.GetCurrentOperDay();
            }

            if(Source == SourceType.AcbaOnline || Source == SourceType.MobileBanking || Source == SourceType.AcbaOnlineXML || Source == SourceType.ArmSoft)
            {
                Customer customer = new Customer();
                customer.CustomerNumber = this.CustomerNumber;
                Order paymentOrder = this.BudgetPaymentOrder;
                paymentOrder.Type = OrderType.PeriodicTransfer;
                paymentOrder.OperationDate = Utility.GetNextOperDay();

                this.Fee = customer.GetPaymentOrderFee(this.BudgetPaymentOrder);
            }

            

        }
        /// <summary>
        /// Հայտի պահմապնման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrder());

            this.BudgetPaymentOrder.OrderNumber = this.OrderNumber;
            this.BudgetPaymentOrder.CustomerNumber = this.CustomerNumber;
            this.BudgetPaymentOrder.RegistrationDate = this.RegistrationDate;
            this.BudgetPaymentOrder.Amount = this.Amount;
            this.BudgetPaymentOrder.Id = this.Id;
            this.BudgetPaymentOrder.Source = this.Source;

            if (this.BudgetPaymentOrder.ReceiverAccount == null && String.IsNullOrEmpty(this.BudgetPaymentOrder.ReceiverAccount.AccountNumber))
            {
                // Մուտքագրվող (կրեդիտ) հաշիվը մուտքագրված չէ:
                result.Errors.Add(new ActionError(18));
            }

            result.Errors.AddRange(BudgetPaymentOrder.Validate().Errors);
            ActionError err1 = new ActionError();
            err1 = result.Errors.Find(m => m.Code == 405);
            if (err1 != null)
            {
                result.Errors.Remove(err1);
            }
            ActionError err2= new ActionError();
            err2 = result.Errors.Find(m => m.Code == 472);
            if (err2 != null)
            {
                result.Errors.Remove(err2);
            }
            ActionError err3 = new ActionError();
            err3 = result.Errors.Find(m => m.Code == 471);
            if (err3 != null)
            {
                result.Errors.Remove(err3);
            }
            ActionError err4 = new ActionError();
            err4 = result.Errors.Find(m => m.Code == 470);
            if (err4 != null)
            {
                result.Errors.Remove(err4);
            }

            //ActionError err5 = new ActionError();
            //err5 = result.Errors.Find(m => m.Code == 470);
            //if (err5 != null)
            //{
            //    result.Errors.Remove(err5);
            //}

            //ActionError err6 = new ActionError();
            //err6 = result.Errors.Find(m => m.Code == 470);
            //if (err6 != null)
            //{
            //    result.Errors.Remove(err6);
            //}





            //Եթե ընտրված է ամբողջ պարտքը տեսակը գումարի ստուգման մասը ջնջում ենք 
            if (this.ChargeType == 2)
            {
                ActionError err = result.Errors.Find(m => m.Code == 22);
                result.Errors.Remove(err);
            }


            //Եթե գործարքը կատարվում է տարանցիկ հաշվից
            //հաճախորդի հետ կապված ստուգումները
            //չեն կատարվում և հեռացվում են
            if (BudgetPaymentOrder.DebitAccount.AccountType == 21)
            {
                result.Errors.RemoveAll(m => m.Code == 584);
                result.Errors.RemoveAll(m => m.Code == 765);
            }

            if (result.Errors.Count > 0)
            {
                return result;
            }
            if (this.FirstTransferDate == Utility.GetNextOperDay())
            {
                //Տվյալ փոխանցման առաջին օրը չպետք է համընկնի պարբերական փոխանցման սկզբի ամսաթվի հետ:
                result.Errors.Add(new ActionError(389));
            }
            //if (this.PaymentOrder.ReceiverBankCode == 10300 && this.PaymentOrder.ReceiverAccount.AccountNumber.ToString()[5] == '9' && this.PaymentOrder.LTACode == 0)
            //{
            //    //ՏՀՏ կոդը լրացված չէ: Հարկային և մաքսային ծառայություններ փոխանցումներ կատարելու համար անհրաժեշտ է պարտադիր նշել տեսչության կոդը: Այլ փոխանցումների դեպքում պետք է լրացնել «99 Այլ»:
            //    result.Errors.Add(new ActionError(356));
            //}
            if (Info.GetBank(Convert.ToInt32(this.BudgetPaymentOrder.ReceiverBankCode), Languages.hy) == "")
            {
                //Ստացողի բանկը գտնված չէ:
                result.Errors.Add(new ActionError(252));
            }
            if (this.ChargeType == 2 && this.BudgetPaymentOrder.DebitAccount.Currency != this.BudgetPaymentOrder.Currency)
            {
                //Ամբողջ մնացորդի փոխանցման դեպքում ընտրված արժույքը չի կարող տարբերվել դեբետագրվող հաշվի արժույթից:
                result.Errors.Add(new ActionError(268));
            }
            if (this.BudgetPaymentOrder.ReceiverBankCode < 22000 && this.BudgetPaymentOrder.ReceiverBankCode >= 22300)
            {
                if (this.BudgetPaymentOrder.DebitAccount.Currency != "AMD" || this.BudgetPaymentOrder.DebitAccount.Currency != "USD" || this.BudgetPaymentOrder.DebitAccount.Currency != "EUR")
                {
                    //ՀՀ տարածքում այլ բանկեր փոխանցման դեպքում դեբետ հաշվի արժույթը պետք է լինի «AMD», «USD» կամ «EUR» 
                    result.Errors.Add(new ActionError(262));
                }
            }
            if (this.BudgetPaymentOrder.ReceiverBankCode >= 22000 && this.BudgetPaymentOrder.ReceiverBankCode < 22300)
            {
                if (this.BudgetPaymentOrder.DebitAccount.Currency != this.BudgetPaymentOrder.ReceiverAccount.Currency && this.BudgetPaymentOrder.Currency != this.BudgetPaymentOrder.DebitAccount.Currency && this.BudgetPaymentOrder.Currency != this.BudgetPaymentOrder.ReceiverAccount.Currency)
                {
                    //Արժույթը պետք է համապատասխանի դեբետ կամ կրեդիտ հաշվին:
                    result.Errors.Add(new ActionError(263));
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
        /// Պարբեարականի հանձնարարականի ուղարկում բանկ
        /// </summary>
        /// <param name="schemaType">Հաստատման կարգ (2 հաստատող,3 հաստատող)</param>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
        {
            ActionResult result = ValidateForSend();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            
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
        /// Հայտի հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(base.ValidatePeriodicOrderForSend().Errors);
            return result;
        }
        /// <summary>
        /// Նոր պարբերականի հայտի հաստատում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                result = PeriodicTransferOrderDB.SavePeriodicBudgetPaymentOrder(this, userName, source);
               
                if (this.FeeAccount != null || (this.ChargeType == 2 && this.Fee == -1 && this.FeeAccount != null))
                {
                    this.Fees = new List<OrderFee>();
                    OrderFee fee = new OrderFee();
                    fee.Amount = this.Fee;
                    fee.Account = this.FeeAccount;
                    fee.Currency = this.FeeAccount.Currency;
                    fee.Type = 20;
                    this.Fees.Add(fee);
                }
                base.SaveOrderFee();


                //**********
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
            }
            if (Source != SourceType.AcbaOnline && Source != SourceType.MobileBanking)
            {
                result = base.Confirm(user);
            }


            return result;
        }
        /// <summary>
        /// Բյուջե պարբերական հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            PeriodicTransferOrderDB.GetPeriodicBudgetPaymentOrder(this);
        }
    }
}
