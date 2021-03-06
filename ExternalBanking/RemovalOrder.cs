using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class RemovalOrder : Order
    {
        /// <summary>
        /// Հեռացվող հայտի ունիկալ համար (Id)
        /// </summary>
        public long RemovingOrderId { get; set; }

        /// <summary>
        /// Հեռացման պատճառ
        /// </summary>
        public short RemovingReason { get; set; }

        /// <summary>
        /// Հեռացման պատճառի նկարագրություն
        /// </summary>
        public string RemovingReasonDescription { get; set; }

        /// <summary>
        /// Հեռացման լրացուցիչ պատճառ ("Այլ" նշված լինելու դեպքում)
        /// </summary>
        public string RemovingReasonAdd { get; set; }

        /// <summary>
        /// Վերլուծության մերժման հիմքեր
        /// </summary>
        public byte RefuseReasonType { get; set; }

        /// <summary>
        /// Պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ushort filialCode)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateRemovalOrder(this, filialCode));
            return result;
        }

        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private OrderType Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.OperationDate = Utility.GetCurrentOperDay();
            OrderType removableOrderType = OrderType.NotDefined;

            this.SubType = 1;
            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);


            if (Source == SourceType.MobileBanking || Source == SourceType.AcbaOnline)
            {
                OrderQuality removableOrderQuality = Order.GetOrderQualityByDocID(this.RemovingOrderId);
                removableOrderType = GetOrderType(this.RemovingOrderId);

                if (removableOrderQuality == OrderQuality.Sent3 || removableOrderType == OrderType.LinkPaymentOrder || removableOrderType == OrderType.ConsumeLoanApplicationOrder || removableOrderType == OrderType.ConsumeLoanSettlementOrder)
                {
                    Type = OrderType.CancelTransaction;
                }
                else
                {
                    Type = OrderType.RemoveTransaction;
                }
            }

            return removableOrderType;

        }

        /// <summary>
        /// Գործարքի հեռացման հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            OrderType removableOrderType = this.Complete();
            ActionResult result = this.Validate(user.filialCode);

            List<ActionError> warnings = new List<ActionError>();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = RemovalOrderDB.Save(this, userName, source);

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

            ConfirmationSourceType confirmationSourceType = ConfirmationSourceType.None;
            if ((removableOrderType == OrderType.LinkPaymentOrder || removableOrderType == OrderType.CardLessCashOrder || removableOrderType == OrderType.ConsumeLoanApplicationOrder) && (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking))
            {
                confirmationSourceType = ConfirmationSourceType.FromACBADigital;
            }

            result = base.Confirm(user, confirmationSourceType);


            return result;
        }

        public void Get()
        {
            RemovalOrderDB.Get(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
        }

        public bool CanRemoveOrder(long removingOrderId, long userId)
        {
            return RemovalOrderDB.CanRemoveOrder(removingOrderId, userId);
        }
    }
}
