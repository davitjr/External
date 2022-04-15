using ExternalBanking.DBManager;
using System;
using System.Transactions;

namespace ExternalBanking
{
    public class MRDataChangeOrder : Order
    {
        /// <summary>
        /// MR ծառայության նույնականացման համար
        /// </summary>
        public int MRId { get; set; }


        /// <summary>
        /// Խմբագրման ենթակա հաշվեհամար
        /// </summary>
        public Account DataChangeAccount { get; set; }

        /// <summary>
        /// Խմբագրման ենթակա քարտ
        /// </summary>
        public Card DataChangeCard { get; set; }

        /// <summary>
        /// Վճարման ենթակա գումար	
        /// </summary>
        public double ServiceFee { get; set; }

        /// <summary>
        /// Լրացնում է հաշվի տվյալների խմբագրման հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.DataChangeAccount = Account.GetAccount(this.DataChangeAccount.AccountNumber);
            this.SubType = 1;

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        /// <summary>
        /// MR ծառայության տվյալների խմբագրման հայտի պահպանում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            int filialCode = user.filialCode;

            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }
            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = MRDataChangeOrderDB.Save(this, user.userName, source, filialCode);
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
        /// MR ծառայության տվյալների խմբագրման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateMRDataChangeOrder(this));
            return result;
        }

        public static bool GetMRDataChangeAvailability(int mrID)
        {
            bool available = false;

            MRStatus status = MRDataChangeOrderDB.GetCardMRStatus(mrID);

            if (status == MRStatus.NORM)
                available = true;

            return available;
        }

    }
}
