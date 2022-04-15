using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class LoanProductMakeOutOrder : Order
    {
        /// <summary>
        /// Վարկային պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }
        /// <summary>
        /// Պրոդուկտի տեսակ
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Դասակարգված վարկի դուրսգրման հայտի պահպանում
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
                result = LoanProductMakeOutOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateLoanProductMakeOutOrder(this));
            return result;
        }

        /// <summary>
        /// Դասակարգված վարկի դուրսգրման հայտի հաստատում
        /// </summary>
        /// <param name="schemaType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult Approve(short schemaType, string userName, ACBAServiceReference.User user)
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
        /// Դասակարգված վարկի դուրսգրման հայտի հաստատաման ստուգումներ
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
            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));


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
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        /// <summary>
        ///Դասակարգված վարկի դուրսգրման հայտի պահպանում և ուղարկում
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
                result = LoanProductMakeOutOrderDB.Save(this, userName, source);

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

                if (this.Fees != null)
                {
                    result = base.SaveOrderFee();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
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
        /// Դասակարգված վարկերի դուրսգրման հայտերի ավտոմատ ստեղծում նախնական հայտերի հիման վրա
        /// </summary>
        /// <param name="preOrderId"></param>
        /// <returns></returns>
        public static List<LoanProductMakeOutOrder> GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(long preOrderId)
        {

            return LoanProductMakeOutOrderDB.GenerateOrdersFromAutomaticGenaratedPreOrdersAsync(preOrderId).Result;
        }
        /// <summary>
        /// Ստուգում է գոյություն ունի՞ արդեն մուտքագրված հայտ, թե ոչ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool IsExistsNotConfirmedOrder()
        {
            return LoanProductMakeOutOrderDB.IsExistsNotConfirmedOrder(this);
        }

        /// <summary>
        ///  Ստուգվում է տվյալ կարգավիճակով վարկը հնարավոր է դուրս գրել, թե ոչ
        /// </summary>
        /// <returns></returns>
        public bool IsQualityWrong()
        {
            return LoanProductMakeOutOrderDB.IsQualityWrong(this);
        }

    }
}
