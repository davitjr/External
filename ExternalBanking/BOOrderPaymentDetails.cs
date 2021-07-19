using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.XBManagement;

namespace ExternalBanking.DBManager
{
    public static class BOOrderPaymentDetails
    {
        public static ActionResult Save(PaymentOrder paymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            if (paymentOrder.Type == OrderType.RATransfer && paymentOrder.SubType == 2)
            {
                result = PaymentOrderDB.SaveOrderPaymentDetails(paymentOrder, orderid);
            }

            return result;
        }

        public static ActionResult Save(BudgetPaymentOrder budgetPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            if (budgetPaymentOrder.Type == OrderType.RATransfer && budgetPaymentOrder.SubType == 5)
            {
                result = BudgetPaymentOrderDB.SaveBudgetOrderPaymentDetails(budgetPaymentOrder, orderid);
            }

            return result;
        }

        public static ActionResult Save(UtilityPaymentOrder utilityPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = UtilityPaymentOrderDB.SaveUPOrderPaymentDetails(utilityPaymentOrder, orderid);

            return result;
        }

        public static ActionResult Save(MatureOrder matureOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = MatureOrderDB.SaveMatureOrderDetails(matureOrder, orderid);

            return result;
        }

        public static ActionResult Save(DepositOrder depositOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = DepositDB.SaveDepositOrderDetails(depositOrder, orderid);

            return result;
        }

        public static ActionResult Save(AccountOrder accountOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = AccountOrderDB.SaveAccountOrderDetails(accountOrder, orderid);

            return result;
        }

        public static ActionResult Save(AccountClosingOrder accountClosingOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = AccountClosingOrderDB.SaveAccountClosingOrderDetails(accountClosingOrder, orderid);

            return result;
        }

        public static ActionResult Save(PeriodicUtilityPaymentOrder periodicUtilityPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = PeriodicTransferOrderDB.SavePeriodicUtilityPaymentOrderDetails(periodicUtilityPaymentOrder, orderid);

            return result;
        }

        public static ActionResult Save(PeriodicBudgetPaymentOrder periodicBudgetPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = PeriodicTransferOrderDB.SavePeriodicBudgetPaymentOrderDetails(periodicBudgetPaymentOrder, orderid);

            return result;
        }

        public static ActionResult Save(PeriodicPaymentOrder periodicPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = PeriodicTransferOrderDB.SavePeriodicPaymentOrderDetails(periodicPaymentOrder, orderid);

            return result;
        }

        public static ActionResult Save(InternationalPaymentOrder internationalPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = InternationalPaymentOrderDB.SaveInternationalPaymentOrderDetails(internationalPaymentOrder, orderid);            

            return result;
        }

        public static ActionResult Save(FastTransferPaymentOrder fastTransferPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = FastTransferPaymentOrderDB.SaveFastTransferPaymentOrderDetails(fastTransferPaymentOrder, orderid);

            return result;
        }


        public static ActionResult Save(ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = ReceivedFastTransferPaymentOrderDB.SaveReceivedFastTransferPaymentOrderDetails(receivedFastTransferPaymentOrder, orderid);

            return result;
        }


        public static ActionResult Save(TransferByCallChangeOrder transferByCallChangeOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = TransferByCallDB.SaveTransferByCallChangeOrderDetails(transferByCallChangeOrder, orderid);

            return result;
        }


        public static ActionResult Save(TransitPaymentOrder transitPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = TransitPaymentOrderDB.SaveTranisPaymentOrderDetails(transitPaymentOrder, orderid);            

            return result;
        }

        public static ActionResult Save(CardClosingOrder cardClosingOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = CardClosingOrderDB.SaveCardClosingOrderDetails(cardClosingOrder, orderid);

            return result;
        }

        public static ActionResult Save(CreditLineTerminationOrder creditLineTerminationOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = CreditLineDB.SaveCreditLineTerminationOrderDetails(creditLineTerminationOrder, orderid);

            return result;
        }

        public static ActionResult Save(CurrencyExchangeOrder currencyExchangeOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = CurrencyExchangeOrderDB.SaveCurrencyExchangeOrderDetails(currencyExchangeOrder, orderid);

            return result;
        }

        public static ActionResult Save(DepositTerminationOrder depositTerminationOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = DepositDB.SaveDepositTerminationOrderDetails(depositTerminationOrder, orderid);

            return result;
        }

        public static ActionResult Save(CredentialTerminationOrder credentialTerminationOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = CredentialOrderDB.SaveCredentialTerminationOrderDetails(credentialTerminationOrder, orderid);

            return result;
        }

        public static ActionResult Save(PeriodicTerminationOrder periodicTerminationOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = PeriodicTerminationOrderDB.SavePeriodicTerminationOrderDetails(periodicTerminationOrder, orderid);

            return result;
        }

        public static ActionResult Save(CashPosPaymentOrder cashPosPaymentOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = CashPosPaymentOrderDB.SaveCashPosPaymentOrderDetails(cashPosPaymentOrder, orderid);

            return result;
        }

        public static ActionResult Save(AccountFreezeOrder accountFreezeOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = AccountFreezeOrderDB.SaveAccountFreezeOrderDetails(accountFreezeOrder, orderid);

            return result;
        }

        public static ActionResult Save(AccountUnfreezeOrder accountUnfreezeOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = AccountUnfreezeOrderDB.SaveAccountUnfreezeOrderDetails(accountUnfreezeOrder, orderid);

            return result;
        }

        public static ActionResult Save(HBActivationOrder activationHBOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = HBActivationOrderDB.SaveHBActivationOrderDetails(activationHBOrder, orderid);

            return result;
        }

        public static ActionResult Save(FeeForServiceProvidedOrder feeForServiceProvidedOrder, ulong orderid)
        {
            ActionResult result = new ActionResult();

            result = FeeForServiceProvidedOrderDB.SaveFeeForServiceOrderDetails(feeForServiceProvidedOrder, orderid);

            return result;
        }

        public static ActionResult Save(CredentialOrder credentialOrder, ulong orderId, Action actionType = Action.Add)
        {
            ActionResult result = new ActionResult();
            result = CredentialOrderDB.SaveCredentialOrderDetails(credentialOrder, orderId, actionType);
            credentialOrder.Credential.Id = (uint)result.Id;

            if (credentialOrder.Credential.AssigneeList != null)
            { 
                credentialOrder.Credential.AssigneeList.ForEach(oneAssignee =>
                {
                    oneAssignee.Save(orderId,actionType);
                });
            }

            return result;
        }


        //public static ActionResult Save(PensionApplicationOrder pensionApplicationOrder, ulong orderid)
        //{
        //    ActionResult result = new ActionResult();

        //    result = PensionApplicationOrderDB.SavePensionApplicationOrderDetails(pensionApplicationOrder, orderid);

        //    return result;
        //}
      
        public static ulong GetOrderId(long id)
        {
            return CredentialOrderDB.GetOrderId(id);
        }
    }
}
