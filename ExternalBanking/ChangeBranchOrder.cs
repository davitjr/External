using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի մասնաճյուղի փոփոխման հայտ
    /// </summary>
    public class ChangeBranchOrder : Order
    {
        /// <summary>
        /// 
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Քարտի ընթացիկ մասնաճյուղ
        /// </summary>
        public int Filial { get; set; }

        /// <summary>
        /// Քարտի Նոր մասնաճյուղ
        /// </summary>
        public int MovedFilial { get; set; }
        /// <summary>
        /// Քարտի համար
        /// </summary>
        public long CardNumber { get; set; }

        /// <summary>
        /// AAP_ID
        /// </summary>
        public double ProductId { get; set; }


        /// <summary>
        /// Հանձնարարականի պահպանում
        /// </summary>
        /// <param name="userName">Հաճախորդի մուտքանուն(login)Հաճախորդի մուտքանուն(login)</param>
        /// <param name="source">Հայտի ստացման աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user = null)
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
                result = ChangeBranchOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns>Վերադարձնում է ստուգման արդյունքը</returns>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            short typeID = ChangeBranchOrderDB.CheckCardReplacedOrReissued(this);
            if (typeID != 0)
            {
                if (typeID == 3)
                {
                    result.Errors = new List<ActionError>();
                    result.Errors.Add(new ActionError(1817));
                    return result;
                }
                else if (typeID == 1)
                {
                    result.Errors = new List<ActionError>();
                    result.Errors.Add(new ActionError(1818));
                    return result;
                }
            }
            result.Errors.AddRange(Validation.ValidateChangeBranchOrder(this));
            return result;
        }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.OperationDate = Utility.GetCurrentOperDay();


            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            //this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.SubType = 1;
        }

        /// <summary>
        /// Հայտի պահպանում և հաստատում
        /// </summary>
        /// <param name="userName">Հաճախորդի մուտքանուն(login)</param>
        /// <param name="source">Հայտի ստացման աղբյուր</param>
        /// <param name="user">Հայտն ընդունող աշխատակից(օգտագործող)</param>
        /// <param name="schemaType">Հայտի հաստատման սխեմա</param>
        /// <returns>Գործողության արդյունք</returns>
        internal ActionResult SaveAndApprove(string userName, SourceType source, User user, short schemaType)
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
                result = ChangeBranchOrderDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
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
        /// Քարտային հաշվի չվճարված դրական տոկոսագումարի վճարման հայտի ստացում
        /// </summary>
        public void Get()
        {
            ChangeBranchOrderDB.getChangeBranchOrder(this);
        }

        internal void GetFilialCode()
        {
            ChangeBranchOrderDB.GetFilialCode(this);
        }
    }
}
