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
    public class CardAccountClosingOrder : Order
    {
        public string ClosingReasonText { get; set; }
        public string CardAccountNumber { get; set; }
        public ulong ProductId { get; set; }

        public ActionResult SaveAndApprove(SourceType source, ulong customerNumber, short approvementScheme)
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
                result = CardAccountClosingOrderDB.Save(this, user, source);

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
            Account account = new Account(CardAccountNumber);
            Card card = Card.GetCardMainData(this.ProductId, this.CustomerNumber);

            if (card == null)
            {
                //Քարտը գտնված չէ
                result.Errors.Add(new ActionError(534));
                return result;
            }

            if (!String.IsNullOrEmpty(card.MainCardNumber))
            {
                //Տվյալ տեսակի քարտի համար հնարավոր չէ իրականացնել քարտային հաշվի փակում
                result.Errors.Add(new ActionError(1828));
                return result;
            }

            if (card.ClosingDate == null)
            {
                //Քարտային հաշիվը հնարավոր է փակել հաշվին կցված գործող քարտը փակելուց հետո
                result.Errors.Add(new ActionError(1820));
                return result;
            }

            if (String.IsNullOrWhiteSpace(ClosingReasonText))
            {
                //  Պատճառի նկարագրությունը մուտքագրված չէ 
                result.Errors.Add(new ActionError(1819));
                return result;
            }

            if (account.DAHKRestrictionForCardAccount())
            {
                //Տվյալ քարտային հաշվի համար առկա են գործող ԴԱՀԿ արգելադրումներ
                result.Errors.Add(new ActionError(1830));
                return result;
            }

            AccountClosingOrder accountClosingOrder = new AccountClosingOrder();
            accountClosingOrder.ClosingAccounts = new List<Account>();
            accountClosingOrder.Id = 0;
            accountClosingOrder.GroupId = 0;
            accountClosingOrder.ClosingAccounts.Add(account);
            accountClosingOrder.CustomerNumber = customerNumber;

            result.Errors.AddRange(Validation.ValidateAccountClosingOrder(accountClosingOrder));

            return result;
        }

        private void Complete()
        {
            this.Type = OrderType.CardAccountClosingOrder;
            this.SubType = 1;
            this.RegistrationDate = DateTime.Now.Date;
            this.OperationDate = Utility.GetCurrentOperDay();

            if (this.OrderNumber == null || this.OrderNumber == "")
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        public CardAccountClosingOrder Get()
        {
            return CardAccountClosingOrderDB.GetCardAccountClosingOrder(this);
        }
    }
}
