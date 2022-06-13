using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking.SecuritiesTrading
{
    public class SecuritiesMarketTradingOrder : SecuritiesTradingOrder
    {
        /// <summary>
        /// Հաճախորդի ԱԱՀ
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Գործարքի կատարման ամսաթիվ և ժամ
        /// </summary>
        public DateTime ConfirmDate { get; set; }

        /// <summary>
        /// Փաստացի կատարված արժեթղթերի քանակ
        /// </summary>
        public int ActuallyQuantity { get; set; }

        /// <summary>
        /// Միավոր գին
        /// </summary>
        public decimal UnitAmount { get; set; }

        /// <summary>
        /// Ընդհանուր ծավալ → Փաստացի կատարված արժեթղթերի քանակ * Միավոր գին
        /// </summary>
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Գործարքի մյուս կողմ
        /// </summary>
        public string OrderDealOtherSide { get; set; }

        /// <summary>
        /// Գործարք կատարող աշխատակից
        /// </summary>
        public string ConfirmOrderUserName { get; set; }

        /// <summary>
        /// Հանձրարականի տրման ա/թ
        /// </summary>
        public DateTime SecuritiTrandingOrderDate { get; set; }

        /// <summary>
        /// Գործարքի կնքման վայր
        /// </summary>
        public string TransactionPlace { get; set; }

        /// <summary>
        /// Մնացորդային քանակ
        /// </summary>
        public decimal ResidualQuantity { get; set; }


        /// <summary>
        /// ACBA միջնորդավճար
        /// </summary>
        public double AcbaFee { get; set; }

        /// <summary>
        /// Հայտի տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }


        /// <summary>
        /// Բորսայում կատարված գործարքին համապատասխան հայտի պահպանում և կատարում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
                result = SecuritiesMarketTradingOrderDB.Save(this);

                if (result.ResultCode != ResultCode.Normal)
                    return result;
                else
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                if (result.ResultCode != ResultCode.Normal)
                    return result;

                result = base.Approve(3, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);


                    result = base.SaveOrderOPPerson();
                    LogOrderChange(user, Action.Update);
                    result.Id = this.Id;
                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }
            }
            return base.Confirm(user);
        }

        private void Complete()
        {
            user = new ACBAServiceReference.User() { userID = 88, userName = "Auto" };

            RegistrationDate = DateTime.Now.Date;
            Type = OrderType.SecuritiesMarketTradingOrder;
            SubType = 1;
            Description = "Հայաստանի ֆոնդային բորսա ԲԲԸ";

            //Հայտի համար   
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
                OrderNumber = Order.GenerateNextOrderNumber(CustomerNumber);
            OPPerson = Order.SetOrderOPPerson(CustomerNumber);

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            if ((GroupId != 0) ? !OrderGroup.CheckGroupId(GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }

            if (ActuallyQuantity > Quantity - GetMarketTradedQuantity(this.OrderId))
            {
                //Կատարված քանակ դաշտը չի կարող լինել ավելին քան հայտում նշված քանակը:
                result.Errors.Add(new ActionError(2070));
            }

            return result;
        }

        public static List<SecuritiesMarketTradingOrder> Get(long orderId) => SecuritiesMarketTradingOrderDB.Get(orderId);

        public static int GetMarketTradedQuantity(long orderId) => SecuritiesMarketTradingOrderDB.GetMarketTradedQuantity(orderId);

    }
}
