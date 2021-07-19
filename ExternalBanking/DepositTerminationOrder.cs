using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Ավանդի դադարեցում
    /// </summary>
    public class DepositTerminationOrder:Order
    {
        /// <summary>
        /// Ավանդի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }
        /// <summary>
        /// Ավանդի համար
        /// </summary>
        public long DepositNumber { get; set; }
        /// <summary>
        /// Ավանդի մնացորդ
        /// </summary>
        public double Balance { get; set; }
        /// <summary>
        /// Ավանդի արժույթ
        /// </summary>
       //public new string Currency { get; set; }

        /// <summary>
        /// Ավանդային հաշիվ
        /// </summary>
        public Account DepositAccount { get; set; }

        /// <summary>
        /// Ավանդի փակման պատճառի տեսակ
        /// </summary>
        public ushort ClosingReasonType { get; set; }
        /// <summary>
        /// Ավանդի փակման պատճառի նկարագրություն
        /// </summary>
        public string ClosingReasonTypeDescription { get; set; }

        /// <summary>
        /// Ավանդի տեսակներ
        /// </summary>
        public DepositType DepositType { get; set; }

        /// <summary>
        /// Ավանդի դադարեցման հայտ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult SaveDepositTermination(string userName, SourceType source, ACBAServiceReference.User user)
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
                result = DepositDB.TerminateDepositOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Ավանդի դադարեցման հայտի հաստատում
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
        /// Վերադարձնում է ավանդի  դադարեցման հայտի տվյալները
        /// </summary>
        public void Get()
        {
            DepositDB.GetDepositTerminationOrder(this);
        }

        /// <summary>
        /// Ավանդի դադարեցման հաստատաման ստուգումներ
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
        /// Ստուգում է ProductID-ի և CustomerNumner-համատեղելիությունը
        /// </summary>
        public bool CheckDepositProducdID()
        {
            bool check = false;
            List<ulong> depositJointCustomerNumbers = Deposit.GetDepositJointCustomers(this.ProductId, this.CustomerNumber);
            depositJointCustomerNumbers.Add(this.CustomerNumber);

            foreach (ulong customerNumber in depositJointCustomerNumbers)
            {
                if (DepositDB.CheckDepositProducdID(this.DepositNumber, customerNumber))
                {
                    check = true;
                }

            }
            return check;

        }
        /// <summary>
        /// Ավանդի դադարեցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateDepositOrderTermination(this));
            return result;
        }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.SubType = 1;
            //Հայտի համար
            this.OrderNumber = this.DepositNumber.ToString();
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.Type = OrderType.DepositTermination;

            if(this.Source == SourceType.MobileBanking || this.Source == SourceType.AcbaOnline)
            {
                Deposit deposit = Deposit.GetDeposit(this.ProductId, this.CustomerNumber);
                if(deposit != null)
                {
                    this.DepositNumber = deposit.DepositNumber;
                    this.Balance = deposit.Balance;
                    this.Currency = deposit.Currency;
                    this.DepositAccount = new Account();
                    this.DepositAccount = deposit.DepositAccount;
                    
                }
            }
        }

        /// <summary>
        ///Ավանդի հայտի պահպանում և ուղղարկում
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
                result = DepositDB.TerminateDepositOrder(this, userName, source);
               
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
                    this.Quality = OrderQuality.Sent3 ;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }

            result = base.Confirm(user);
          
            return result;
        }
    }
}
