using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class ThirdPersonAccountRightsTransferOrder : Order
    {

        ///<summary>
        ///Երրորդ անձի հաճախորդ համար
        /// </summary>
        public ulong ThirdPersonCustomerNumber { get; set; }


        ///<summary>
        ///Երրորդ անձի հաճախորդ համար
        /// </summary>
        public Account JointAccount { get; set; }


        ///<summary>
        /// Հայտի ավտոմատ լրացվող դաշտերի ստացում 
        ///</summary>
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }


        /// <summary>
        /// Իրավունքի փոխանցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateThirdPersonAccountRightsTransfer(this));
            return result;
        }

        public static bool CheckRightsWereTransferred(string accountNumber)
        {
            return ThirdPersonAccountRightsTransferOrderDB.CheckRightsWereTransferred(accountNumber);
        }

        public static bool CheckAccountHasArrest(ulong customerNumber)
        {
            return ThirdPersonAccountRightsTransferOrderDB.CheckAccountHasArrest(customerNumber);
        }

        public static bool CheckThirdPersonIsCustomer(ulong customerNumber)
        {
            return ThirdPersonAccountRightsTransferOrderDB.CheckThirdPersonIsCustomer(customerNumber);
        }

        /// <summary>
        /// Իրավունքի փոխանցման հայտի համար
        /// </summary>
        internal static bool IsSecondClosing(ulong customerNumber, string accountNumber, SourceType sourceType)
        {
            bool secondClosing = ThirdPersonAccountRightsTransferOrderDB.IsSecondClosing(customerNumber, accountNumber, sourceType);
            return secondClosing;
        }

        /// <summary>
        /// Իրավունքի փոխանցման հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = ThirdPersonAccountRightsTransferOrderDB.RightsTransferOrder(this, userName, source);

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

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
    }
}
