using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Տարանցիկ հաշվին մուտք փոխարկումով
    /// </summary>
    public class TransitCurrencyExchangeOrder:CurrencyExchangeOrder
    {
        /// <summary>
        /// Հաշվի տեսակ
        /// </summary>
        public short AccountType { get; set; }
        /// <summary>
        /// Լրացուցիչ տվյալներ
        /// </summary>
        public List<AdditionalDetails> AdditioanalParameters { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Պրոդուկտի արժույթ
        /// </summary>
        public string ProductCurrency { get; set; }


        public new void Complete()
        {

            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            //Տարանցիկ հաշվի լրացում
            this.ReceiverAccount = GetTransitCurrencyExchangeOrderSystemAccount(this, OrderAccountType.CreditAccount, this.ReceiverAccount.Currency);

            base.Complete();
        }

        public static Account GetTransitCurrencyExchangeOrderSystemAccount(TransitCurrencyExchangeOrder order, OrderAccountType accountType, string operationCurrency)
        {

            Account account = null;
            if (order.AccountType == 5)
            {
                ushort accType = 0;
                if ((operationCurrency == "AMD" && order.ProductCurrency == "AMD") || operationCurrency != "AMD" && order.ProductCurrency != "AMD")
                {
                    accType = 224;
                }
                else if (operationCurrency == "AMD" && order.ProductCurrency != "AMD")
                {
                    accType = 279;
                }
                account= Account.GetProductAccount(order.ProductId, 18, accType);
            }
            else
            {
                account = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, accountType), operationCurrency, order.user.filialCode);
            }
            return account;



        }

        public new ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            Complete();
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
                if (this.ValidateForCash==true)
                {
                    result = PaymentOrderDB.SaveCash(this, userName, source);
                }
                else
                {
                    result = PaymentOrderDB.Save(this, userName, source);
                }

                CurrencyExchangeOrderDB.Save(this, userName, Source);

                if (this.AdditionalParametrs != null && this.AdditionalParametrs.Exists(m => m.AdditionValue == "LeasingAccount"))
                {
                    LeasingDB.SaveLeasingPaymentDetails(this);
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

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                result = base.SaveOrderFee();

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
                    //Տերմինալներից քարտից ելքագրման դեպքում նախքան հաճախորդին գումար տրամադրելը անհրաժեշտ է գործարքի հայտը գրանցելիս նաև գործարքը ուղարկել ARCA համակարգ
                    if (this.Source == SourceType.SSTerminal && this.Type == OrderType.CashCreditConvertation && this.SubType == 1 && Utility.IsCardAccount(this.DebitAccount.AccountNumber))
                    {
                        OrderDB.ChangeQuality(this.Id, OrderQuality.SBQprocessed, user.userID.ToString());
                    }
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }



            result = base.Confirm(user);
            warnings.AddRange(base.GetActionResultWarnings(result));
            result.Errors = warnings;

            return result;
        }

    }
}
