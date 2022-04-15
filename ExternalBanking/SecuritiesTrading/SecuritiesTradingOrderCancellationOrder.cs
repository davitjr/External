using ExternalBanking.DBManager.SecuritiesTrading;
using System;
using System.Transactions;

namespace ExternalBanking.SecuritiesTrading
{
    public class SecuritiesTradingOrderCancellationOrder : SecuritiesTradingOrder
    {
        /// <summary>
        /// Չեղարկվող գործարքի կոդ
        /// </summary>
        public long SecuritiesTradingOrderId { get; set; }


        public ActionResult SaveAndApprove()
        {
            Complete();
            ActionResult result = Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = SecuritiesTradingOrderCancellationOrderDB.Save(this);

                if (result.ResultCode != ResultCode.Normal)
                    return result;
                else
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                result = base.Approve(3, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
                else
                    return result;
            }

            if (Order.GetOrderQualityByDocID(SecuritiesTradingOrderId) == OrderQuality.Sent3)
                Confirm();

            return result;
        }

        private ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            OrderQuality orderQuality = Order.GetOrderQualityByDocID(this.SecuritiesTradingOrderId);

            if (orderQuality != OrderQuality.Draft && orderQuality != OrderQuality.Sent3 && orderQuality != OrderQuality.ReviewingByEmployee && orderQuality != OrderQuality.SBQprocessed)
            {
                //Տվյալ կարգավիճակով հայտը հնարավոր չէ չեղարկել։
                result.Errors.Add(new ActionError(2065));
            }

            if (CheckCancellationOrder())
            {
                //Տվյալ ձևակերպման համար հեռացման հայտն արդեն մուտագրված է։
                result.Errors.Add(new ActionError(691));
            }

            return result;
        }

        private bool CheckCancellationOrder() => SecuritiesTradingOrderCancellationOrderDB.CheckCancellationOrder(SecuritiesTradingOrderId);

        private void Complete()
        {
            Quality = OrderQuality.Draft;
            Type = OrderType.SecuritiesTradingOrderCancellationOrder;
            RegistrationDate = DateTime.Now.Date;
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
                OrderNumber = Order.GenerateNextOrderNumber(CustomerNumber);
            OPPerson = Order.SetOrderOPPerson(CustomerNumber);
            Description = "chegharkum";
        }


        public static SecuritiesTradingOrderCancellationOrder Get(long id) => SecuritiesTradingOrderCancellationOrderDB.Get(id);

        public override ActionResult Confirm()
        {
            ActionResult result = ValidateForConfirm();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }

            base.Confirm(user);
            result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            return result;
        }


        public override ActionResult Reject()
        {
            ActionResult result = ValidateForReject();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                Culture culture = new Culture(Languages.hy);
                Localization.SetCulture(result, culture);
                return result;
            }

            RejectOrder(Id, RejectReasonDescription, Type, user.userID);
            result = new ActionResult();
            result.ResultCode = ResultCode.Normal;
            return result;
        }

        /// <summary>
        /// Հայտի հաստատման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public override ActionResult ValidateForConfirm()
        {
            ActionResult result = new ActionResult();

            OrderQuality orderQuality = Order.GetOrderQualityByDocID(this.SecuritiesTradingOrderId);

            if (orderQuality != OrderQuality.Draft || orderQuality != OrderQuality.Sent3 || orderQuality != OrderQuality.ReviewingByEmployee || orderQuality != OrderQuality.SBQprocessed || orderQuality != OrderQuality.TransactionLimitApprovement)
            {
                //Տվյալ կարգավիճակով հայտը հնարավոր չէ չեղարկել։
                result.Errors.Add(new ActionError(2065));
            }

            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

        /// <summary>
        /// Հայտի մերժման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public override ActionResult ValidateForReject()
        {
            ActionResult result = new ActionResult();


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

    }
}
