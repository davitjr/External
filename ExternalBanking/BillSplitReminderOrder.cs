using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class BillSplitReminderOrder : Order
    {
        /// <summary>
        /// Փոխանցողի (ում պետք է ուղարկվի ծանուցումը) ունիկալ համար 
        /// </summary>
        public int SenderId { get; set; }


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


                result = BillSplitReminderOrderDB.Save(this, userName, source);

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
            Type = OrderType.BillSplitReminder;

        }

        public ActionResult Validate(User user, TemplateType templateType = TemplateType.None)
        {
            ActionResult result = new ActionResult();

            if (GetBillSplitSenderReminderOrdersCount(this.SenderId) > 1)
            {
                //Դուք արդեն ուղարկել եք հիշեցում տվյալ փոխանցողին։ Յուրաքանչյուր փոխանցողի օրական հնարավոր է ուղարկել մեկ հիշեցում։
                result.Errors.Add(new ActionError(1962));
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
            BillSplitReminderOrderDB.GetBillSplitReminderOrder(this);
            OPPerson = OrderDB.GetOrderOPPerson(Id);
        }

        public static int GetBillSplitSenderReminderOrdersCount(int senderId)
        {
            return BillSplitReminderOrderDB.GetBillSplitSenderReminderOrdersCount(senderId);
        }

    }
}
