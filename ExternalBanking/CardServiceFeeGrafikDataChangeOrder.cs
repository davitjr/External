using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Քարտի սպասարկման վարձի գրաֆիկի տվյալների փոփոխման հայտ
    /// </summary>
    public class CardServiceFeeGrafikDataChangeOrder : Order
    {
        /// <summary>
        /// քարտի սպասարկման վարձի գրաֆիկ
        /// </summary>
        public List<CardServiceFeeGrafik> CardServiceFeeGrafik { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductAppId { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

        }

        /// <summary>
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        public void Get()
        {
            CardServiceFeeGrafikDataChangeOrderDB.GetCardServiceFeeDataChangeOrder(this);
        }


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCardServiceFeeGrafikDataChangeOrder(this));
            return result;
        }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();

            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardServiceFeeGrafikDataChangeOrderDB.SaveCardServiceFeeGrafikDataChangeOrder(this, userName, source);

                if (this.Type == OrderType.CardServiceFeeGrafikDataChangeOrder)
                {
                    SaveCardServiceFeeGrafikDataChangeOrderNewGrafik();
                }
                if (this.Type == OrderType.CardServiceFeeGrafikRemovableOrder)
                {
                    SaveOrderProductId((ulong)this.ProductAppId, this.Id);
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

            return resultConfirm;
        }



        public void SaveCardServiceFeeGrafikDataChangeOrderNewGrafik()
        {
            CardServiceFeeGrafikDataChangeOrderDB.SaveCardServiceFeeGrafikDataChangeOrderNewGrafik(this);
        }


    }
}
