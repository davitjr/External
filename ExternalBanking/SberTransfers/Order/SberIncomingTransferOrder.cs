using ExternalBanking.DBManager.SberTransfers.Order;
using System;
using System.Transactions;
using ext = ExternalBanking;

namespace ExternalBanking.SberTransfers.Order
{
    public class SberIncomingTransferOrder : ext.Order
    {

        /// <summary>
        /// Incoiming transfer id
        /// </summary>
        public int TransferId { get; set; }

        public string ReceiverFIO { get; set; }

        public string SenderFIO { get; set; }

        public Account CreditAccount { get; set; }

        public string CreditCurrency { get; set; }
        /// <summary>
        /// RUB convert to AMD
        /// </summary>
        public decimal CurrencyRateCrossBuy { get; set; }
        /// <summary>
        /// AMD convert to currency (USD/EUR)
        /// </summary>
        public decimal CurrencyRateCrossSell { get; set; }
        /// <summary>
        /// RUR -> USD/EUR
        /// </summary>
        public decimal CurrencyCrossRateFull { get; set; }
        /// <summary>
        /// Amount which should customer get
        /// </summary>
        public decimal CreditAmount { get; set; }

        public decimal AmountAMD { get; set; }

        public ulong PayId { get; set; }

        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.SberBankTransferOrder;
            this.SubType = 1;
            this.OrderNumber = GenerateNextOrderNumber(this.CustomerNumber);
        }

        public ActionResult SaveAndApproveWithoutConfirm(string userName, short schemaType)
        {
            this.Complete();

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = SberIncomingTransferOrderDB.SaveSberIncomingTransferOrder(this);

                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);

                    scope.Complete();
                }
                else
                {
                    result.ResultCode = ResultCode.Failed;
                    return result;
                }

            }

            result.Id = this.Id;
            return result;
        }
    }
}
