using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class ServicePaymentNoteOrder:Order
    {
        /// <summary>
        /// Սպասարկման վարձի գանձման նշում
        /// </summary>
        public ServicePaymentNote Note { get; set; }
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user,short schemaType) {
            this.Complete(source);
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();


            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                if (this.Type == OrderType.ServicePaymentNote)
                {
                    result = ServicePaymentNoteDB.Save(this, userName, source);
                }
                else if(this.Type == OrderType.DeleteServicePaymentNote)
                {
                    result = ServicePaymentNoteDB.Delete(this, userName, source);
                }               
               
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
                else
                {
                    return result;
                }
            }
            ActionResult resultConfirm = base.Confirm(user);

            if (resultConfirm.ResultCode == ResultCode.Normal)
            {
                resultConfirm.Errors = warnings;
            }

            return resultConfirm;

        }

        /// <summary>
        /// Վերադարձնում է սպասարկամ վարձի գանձման նշման հեռացման հայտի տվյալները
        /// </summary>
        public void GetDelatedServicePaymentNoteOrder()
        {
            ServicePaymentNoteDB.GetDelatedServicePaymentNoteOrder(this);
        }

        /// <summary>
        /// Վերադարձնում է սպասարկամ վարձի գանձման նշման հայտի տվյալները
        /// </summary>
        public void GetServicePaymentNoteOrder()
        {
            ServicePaymentNoteDB.GetServicePaymentNoteOrder(this);
        }
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }
        /// <summary>
        /// Սպասարկման վարձի գանձման նշման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateServicePaymentNoteOrder(this));
            return result;
        }

    }
}
