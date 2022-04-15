using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
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
                {
                    //Լրացրեք բոլոր տվյալները:
                    result.Errors.Add(new ActionError(1812));
                }

                if (CardAdditionalData.AdditionID == 3 || CardAdditionalData.AdditionID == 4 || CardAdditionalData.AdditionID == 14)
                {

                    if (CardAdditionalData.AdditionValue.Length != 8)
                    {
                        //Սխալ ամասթիվ:
                        result.Errors.Add(new ActionError(134));
                    }
                }

                if (CardAdditionalData.AdditionID == 9 || CardAdditionalData.AdditionID == 10)
                {
                    string operday = String.Format("{0}/{1}/{2}", this.OperationDate.Value.Day.ToString().Length < 2 ? "0" + this.OperationDate.Value.Day.ToString() : this.OperationDate.Value.Day.ToString(),
                                                                  this.OperationDate.Value.Month.ToString().Length < 2 ? "0" + this.OperationDate.Value.Month.ToString() : this.OperationDate.Value.Month.ToString(),
                                                                  this.OperationDate.Value.Year.ToString().Substring(2));
                    if (operday != CardAdditionalData.AdditionValue)
                    {
                        //Անհրաժեշտ է մուտքագրել տվյալ գործառնական օրը:
                        result.Errors.Add(new ActionError(1813));
                    }
                }
            }

            if (SubType == 1)
            {
                if (CardAdditionalDataOrderDB.IsExistAdditionalData(PlasticCard.ProductId, CardAdditionalData.AdditionID))
                {
                    switch (CardAdditionalData.AdditionID)
                    {
                        case 3:
                            result.Errors.Add(new ActionError(2057, new string[] { "Ակտի ա/թ" }));
                            break;
                        case 4:
                            result.Errors.Add(new ActionError(2057, new string[] { "Չստացված քարտը Բանկ հետ վերադարձման ա/թ" }));
                            break;
                        case 9:
                            result.Errors.Add(new ActionError(2057, new string[] { "Մասնաճյուղում քարտի առկայություն" }));
                            break;
                        case 10:
                            result.Errors.Add(new ActionError(2057, new string[] { "Մասնաճյուղում PIN ծրարի առկայություն" }));
                            break;
                        case 14:
                            result.Errors.Add(new ActionError(2057, new string[] { "Ստացված քարտի փ/թղթերի վերադարձման ա/թ" }));
                            break;
                        default:
                            break;
                    }
                }
                if (Card.GetCardNumber((long)PlasticCard.ProductId).Equals(""))
                {
                    //Քարտի համարը բացակայում է: Հնարավոր չէ կատարել ընտրված գործողությունը։
                    result.Errors.Add(new ActionError(2058));
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
