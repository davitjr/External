using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class BillSplitSenderRejectionOrder : Order
    {
        /// <summary>
        /// BillSplit փոխանցողի ունիկալ համար
        /// </summary>
        public int BillSplitSenderId { get; set; }

        /// <summary>
        /// Հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = this.ValidateForSend(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {


                result = BillSplitOrderRejectionDB.Save(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();
                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }


                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }



                LogOrderChange(user, action);


                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    warnings.AddRange(base.GetActionResultWarnings(result));

                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);


                    LogOrderChange(user, Action.Update);
                    result = base.Confirm(user);
                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }

            }

            return result;
        }

        internal void Complete()
        {

            if (this.Source != SourceType.Bank)
                this.RegistrationDate = DateTime.Now.Date;


            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);

            this.SubType = 1;

            if (this.Source != SourceType.Bank || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }

            Quality = OrderQuality.Draft;
            Type = OrderType.BillSplitSenderRejection;

        }

        public ActionResult Validate(User user, TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            BillSplitSenderInfo sender = BillSplitOrder.GetBillSplitSenders(0, BillSplitSenderId)[0];



            if (sender.Status != 0 || BillSplitOrder.HasBillSplitSenderSentTransfer(BillSplitSenderId))
            {
                //Հնարավոր չէ չեղարկել տվյալ մասնակցի գումարի ստացման հայտը։
                result.Errors.Add(new ActionError(1957));
                return result;
            }

            result.Errors.AddRange(Validation.ValidateDraftOrderQuality(this, this.CustomerNumber));

            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }


        public ActionResult ValidateForSend(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            if (this.Quality != OrderQuality.Draft && this.Quality != OrderQuality.Approved)
            {
                //Տվյալ կարգավիճակով փաստաթուղթը հնարավոր չէ ուղարկել:
                result.Errors.Add(new ActionError(35));
                return result;
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
            BillSplitOrderRejectionDB.GetBillSplitSenderRejectionOrder(this);
            OPPerson = OrderDB.GetOrderOPPerson(Id);
        }


    }
}
