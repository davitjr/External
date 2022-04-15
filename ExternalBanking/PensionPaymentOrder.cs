using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class PensionPaymentOrder : Order
    {
        /// <summary>
        /// ՈՒնիկալ համար
        /// </summary>
        public int PensionPaymentId { get; set; }

        /// <summary>
        /// Ստանալու ամսաթիվ
        /// </summary>
        public DateTime DateGet { get; set; }

        /// <summary>
        /// Տարի
        /// </summary>
        public short Year { get; set; }

        /// <summary>
        /// Ամսաթիվ
        /// </summary>
        public short Month { get; set; }

        /// <summary>
        /// Անուն
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Հայրանուն
        /// </summary>
        public string FatherName { get; set; }

        /// <summary>
        /// Սոցիալական քարտի համար
        /// </summary>
        public string SocialCardNumber { get; set; }

        /// <summary>
        /// Կրեդիտագրվող հաշիվ
        /// </summary>
        public string CreditAccount { get; set; }

        /// <summary>
        /// Նպաստ/կենսաթոշակ
        /// </summary>
        public string FileTypeDescription { get; set; }

        public PensionPaymentOrder()
        {
            this.DebitAccount = new Account();
        }

        /// <summary>
        /// Վերադարձնում ահաճախորդի բոլոր տվյալները
        /// </summary>
        /// <param name="socialCardNumber"></param>
        /// <returns></returns>
        public static List<PensionPaymentOrder> GetAllPensionPayment(string socialCardNumber)
        {
            return PensionPaymentOrderDB.GetAllPensionPayment(socialCardNumber);
        }

        /// <summary>
        /// Վերադարձնում է մանրամասն տվյալներ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PensionPaymentOrder GetPensionPaymentOrderDetails(uint id)
        {
            return PensionPaymentOrderDB.GetPensionPaymentOrderDetails(id);
        }

        public ActionResult SavePensionPaymentOrder(PensionPaymentOrder order, string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = PensionPaymentOrderDB.Save(order, userName);

                result = base.SaveOrderOPPerson();
                this.SaveOrderAttachments();

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

            ActionResult resultConfirm = base.Confirm(user);
            return resultConfirm;
        }

        /// <summary>
        ///  ստուգում
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidatePensionPaymentOrder(this));
            return result;
        }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = (OrderType)237;
            this.Currency = "AMD";
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.CreditAccount = AccountDB.GetPensionPaymentCreditAccount(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);


        }


    }
}
