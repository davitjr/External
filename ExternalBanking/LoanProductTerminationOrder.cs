using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    public class LoanProductTerminationOrder : Order
    {
        /// <summary>
        /// Դադարեցվող երաշխիքի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }


        /// <summary>
        /// Երաշխիքի դադարեցման հայտ
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
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

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = LoanProductTerminationOrderDB.SaveLoanProductTerminationOrder(this, userName, source, user.filialCode);

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
        /// Վերադարձնում է Երաշխիքի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            LoanProductTerminationOrderDB.GetLoanProductTerminationOrder(this);
        }



        /// <summary>
        /// Երաշխիքի դադարեցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if (this.Type == OrderType.GuaranteeTermination)
            {
                result.Errors.AddRange(Validation.ValidateGuaranteeTerminationOrder(this));
            }
            else if (this.Type == OrderType.AccreditiveTerminationOrder)
            {
                result.Errors.AddRange(Validation.ValidateAccreditiveTerminationOrder(this));
            }
            return result;
        }

        /// <summary>
        /// Լրացնում է երաշխիքի դադարեցման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.SubType = 1;
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }


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
                result = LoanProductTerminationOrderDB.SaveLoanProductTerminationOrder(this, userName, source, user.filialCode);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Երաշխիքի դադարեցման հայտի հաստատում
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
        /// Երաշխիքի դադարեցման հայտի ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();
            DateTime nextOperDay = Utility.GetNextOperDay().Date;
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(785));
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


        public static bool IsSecondTermination(LoanProductTerminationOrder order)
        {
            return LoanProductTerminationOrderDB.IsSecondTermination(order);
        }
    }
}
