using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class BOOrderPaymentAddition
    {
        /// <summary>
        /// BOOrderPayment Id
        /// </summary>
        public ulong BOOrderPaymentId { get; set; }
        
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
        public Account DebitAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account CreditAccount { get; set; }

        public static ActionResult Save(BOOrderPayment orderPayment, ulong BOOrderPaymentId, OrderType orderType, int orderSubType)
        {
            ActionResult result = new ActionResult();
            BOOrderPaymentAddition orderPaymentAddition = new BOOrderPaymentAddition();
            orderPaymentAddition.BOOrderPaymentId = BOOrderPaymentId;
            Random random = new Random();
            orderPaymentAddition.DebitAccount = new Account();
            orderPaymentAddition.CreditAccount = new Account();
            if (orderType == OrderType.RATransfer && orderSubType ==3) // Փոխանցում սեփական հաշիվների մեջ
            {
                orderPaymentAddition.DebitAccount.AccountNumber = orderPayment.MainDebitAccount;
                orderPaymentAddition.CreditAccount.AccountNumber = orderPayment.MainCreditAccount;
            }
            else
            {
                orderPaymentAddition.DebitAccount.AccountNumber = "220000000011111";
                orderPaymentAddition.CreditAccount.AccountNumber = "220000000011111";
            }

            BOOrderPaymentAdditionDB.Save(orderPaymentAddition);
            return result;
        }
    }
}
