using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class CardAdditionalDataOrder : Order
    {
        public CardAdditionalData CardAdditionalData { get; set; }
        public PlasticCard PlasticCard { get; set; }

        public CardAdditionalDataOrder()
        {
            CardAdditionalData = new CardAdditionalData();
            PlasticCard = new PlasticCard();
        }

        public ActionResult SaveAndApprove(User user, SourceType source, ulong customerNumber, short approvementScheme)
        {
            this.Complete();

            ActionResult result = this.Validate(customerNumber);
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardAdditionalDataOrderDB.Save(this, user, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                LogOrderChange(user, action);

                ActionResult res = base.Approve(approvementScheme, user.userName);

                if (res.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    this.SubType = 1;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);

                }
                else
                {
                    return res;
                }
                scope.Complete();
            }

            result = base.Confirm(user);

            return result;
        }

        private ActionResult Validate(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            result.Errors = new List<ActionError>();

            if (this.SubType == 1 || this.SubType == 2)
            {
                if (CardAdditionalData.AdditionID == 0 || CardAdditionalData.AdditionValue == "")
                {//Լրացրեք բոլոր տվյալները:
                    result.Errors.Add(new ActionError(1812));
                }

                if (CardAdditionalData.AdditionID == 3 || CardAdditionalData.AdditionID == 4 || CardAdditionalData.AdditionID == 14)
                {

                    if (CardAdditionalData.AdditionValue.Length != 8)
                    {//Սխալ ամասթիվ:
                        result.Errors.Add(new ActionError(134));
                    }
                }

                if (CardAdditionalData.AdditionID == 9 || CardAdditionalData.AdditionID == 10)
                {
                    string operday = String.Format("{0}/{1}/{2}", this.OperationDate.Value.Day.ToString().Length < 2 ? "0" + this.OperationDate.Value.Day.ToString() : this.OperationDate.Value.Day.ToString(),
                                                                  this.OperationDate.Value.Month.ToString().Length < 2 ? "0" + this.OperationDate.Value.Month.ToString() : this.OperationDate.Value.Month.ToString(),
                                                                  this.OperationDate.Value.Year.ToString().Substring(2));
                    if (operday != CardAdditionalData.AdditionValue)
                    {//Սխալ ամասթիվ:
                        result.Errors.Add(new ActionError(1813));
                    }
                }
            }

            return result;
        }

        private void Complete()
        {
            this.Type = OrderType.CardAdditionalDataOrder;
            this.RegistrationDate = DateTime.Now.Date;
            this.OperationDate = Utility.GetCurrentOperDay();

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

        }

        public CardAdditionalDataOrder Get()
        {
            return CardAdditionalDataOrderDB.GetCardAdditionalDataOrder(this);
        }
    }
}
