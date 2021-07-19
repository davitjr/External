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


namespace ExternalBanking.XBManagement
{
    /// <summary>
    /// ՀԲ ակտիվացման/փոփոխման հայտ
    /// </summary>
    public class HBActivationOrder : Order
    {
        /// <summary>
        /// Ակտիվացման/փոփոխման դիմումի մանրամասներ
        /// </summary>
        public List<HBActivationRequest> HBActivationRequests { get; set; }
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ 
        /// </summary>
     //   public override Account DebitAccount { get; set; }
        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Ակտիվացման/փոփոխման դիմումի մանրամասները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<HBActivationRequest> GetHBRequests(ulong customerNumber)
        {
            return HBActivationOrderDB.GetHBRequests(customerNumber);
        }
       


        /// <summary>
        /// Հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = new ActionResult();


            result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            if (this.Amount > 0)
            {
                result = this.ValidateForSend(source);
            }
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = HBActivationOrderDB.SaveHBActivationOrder(this, userName, source);
               
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
                    foreach (var item in this.HBActivationRequests)
                    {
                        Order.SetDefaultRestConfigs(item.UserName);
                    }
                    scope.Complete();
                }

            }
            result = base.Confirm(user);

            return result;
        }

        /// <summary>
        /// Լրացնում է ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            this.Description = "ՀԲ ծառայության ակտիվացում:";
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            if (this.HBActivationRequests != null)
            {
                foreach (HBActivationRequest request in this.HBActivationRequests)
                {
                    if (request.IsFree)
                    {
                        request.ServiceFee = 0;
                    }
                }
                
            }


        }

        /// <summary>
        ///  Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateHBActivationOrder(this));
            return result;
        }

        /// <summary>
        /// Հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
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
                result = HBActivationOrderDB.SaveHBActivationOrder(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }
        /// <summary>
        /// Հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public new ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user, SourceType source)
        {

            ActionResult result = ValidateForSend(source);
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
        /// Հայտի ուղարկման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend(SourceType source)
        {
            ActionResult result = new ActionResult();

            if (this.Quality != OrderQuality.Draft && source != SourceType.MobileBanking && source != SourceType.AcbaOnline)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }

            if (this.Amount>0)
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

       

        public void Get()
        {
            HBActivationOrderDB.Get(this);
        }

        public bool HasHBActivationOrder()
        {
            return HBActivationOrderDB.HasHBActivationOrder(this.CustomerNumber);
        }

        public override bool CheckConditionForMakingTransactionIn24_7Mode()
        {
            var requestsswithfees = from request in HBActivationRequests.ToList()
                                    where request.ServiceFee > 0
                                    select new { request };

            if (requestsswithfees.Count() > 0)
                return false;
            else
                return true;

        }
    }
}
