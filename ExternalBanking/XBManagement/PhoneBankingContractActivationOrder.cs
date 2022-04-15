using ExternalBanking.DBManager;
using System;
using System.Transactions;


namespace ExternalBanking.XBManagement
{
    public class PhoneBankingContractActivationOrder : Order
    {
        /// <summary>
        /// Ակտիվացման դիմումի մանրամասներ
        /// </summary>
        public PhoneBankingContractActivationRequest PBActivationRequest { get; set; }
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ 
        /// </summary>
      //  public override Account DebitAccount { get; set; }
        /// <summary>
        /// Հեռախոսային կամ հեռահար բանկինգի պայմանգրի ID
        /// </summary>
        public int GlobalID { get; set; }
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
                result = this.ValidateForSend();
            }
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = HBActivationOrderDB.SavePhoneBankingContractActivationOrder(this, userName, source);

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
            this.Description = "Հեռախոսային բանկինգի ծառայության ակտիվացում:";
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        ///  Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidatePhoneBankingContractActivationOrder(this));
            return result;
        }

        /// <summary>
        /// Հայտի ուղարկման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (this.Quality != OrderQuality.Draft)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
            }

            if (this.Amount > 0)
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

        public static PhoneBankingContractActivationRequest GetPhoneBankingRequests(ulong customerNumber)
        {
            return HBActivationOrderDB.GetPhoneBankingRequests(customerNumber);
        }

    }
}
