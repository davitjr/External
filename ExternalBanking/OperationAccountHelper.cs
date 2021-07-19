using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Նախատեսված է սիստեմային հաշիվները գտնելու համար
    /// </summary>
    public class OperationAccountHelper
    {
        /// <summary>
        /// Հաշվի տեսակ
        /// </summary>
        public int AccountType { get; set; }

        /// <summary>
        ///Արժույթ 
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Գործարքի կատարման մասնաճյուղ
        /// </summary>
        public int OperationFilialCode { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի տեսակ
        /// </summary>
        public int CustomerType { get; set; }

        /// <summary>
        /// Հաճախորդի ռեզիդենտություն
        /// </summary>
        public int CustomerResidence { get; set; }

        /// <summary>
        /// Կոմունալ ծառայության մասնաճյուղ
        /// </summary>
        public string Utilitybranch { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Դեբետ հաշիվ
        /// </summary>
        public string DebetAccount { get; set; }

        /// <summary>
        /// Կրեդիտ հաշիվ
        /// </summary>
        public string CreditAccount { get; set; }

        /// <summary>
        /// Ծառայության սակագնի հերթական համար
        /// </summary>
        public int PriceIndex { get; set; }

        /// <summary>
        /// Պրոդուկտի համար
        /// </summary>
        public float AppID { get; set; }


        public OperationAccountHelper()
        {

        }

        public OperationAccountHelper(PaymentOrder order, short feeType)
        {
            if (order.Type == OrderType.CashCredit || order.Type == OrderType.CashForRATransfer 
                || order.Type == OrderType.CashDebit || order.Type == OrderType.RATransfer || order.Type==OrderType.TransitCashOut
                || order.Type == OrderType.ReestrTransferOrder || order.Type == OrderType.CashOutFromTransitAccountsOrder
                || order.Type == OrderType.CashDebitConvertation
                || order.Type == OrderType.CashTransitCurrencyExchangeOrder)
            {
                this.AccountType = OperationAccountHelper.GetOperationSystemAccountTypeForFee(order, feeType);
                if (order.Type == OrderType.CashForRATransfer && feeType == 5 
                    || order.Type == OrderType.CashDebitConvertation 
                    || order.Type == OrderType.CashTransitCurrencyExchangeOrder)
                    this.Currency = "AMD";
                else
                    this.Currency = order.Currency;

                if(feeType == 28 || feeType == 29)
                {
                    this.CustomerNumber = order.CustomerNumber;
                }

                this.OperationFilialCode = order.FilialCode;
            }

        }

        public OperationAccountHelper(CashPosPaymentOrder order, short feeType)
        {
            this.AccountType = OperationAccountHelper.GetOperationSystemAccountTypeForFee(order, feeType);
            this.Currency = "AMD";
            this.OperationFilialCode = order.FilialCode;
            this.CardNumber = order.CardNumber;
            if (feeType == 28 || feeType == 29)
            {
                this.CustomerNumber = order.CustomerNumber;
            }

        }

        public OperationAccountHelper(AccountReOpenOrder order, short feeType)
        {
            this.AccountType = OperationAccountHelper.GetOperationSystemAccountTypeForFee(order, feeType);
            this.DebetAccount = order.ReOpeningAccounts[0].AccountNumber;
            this.Currency = "AMD";
            this.OperationFilialCode = order.FilialCode;
            if (feeType == 28 || feeType == 29)
            {
                this.CustomerNumber = order.CustomerNumber;
            }
        }

        public OperationAccountHelper(InternationalPaymentOrder order, short feeType)
        {
            this.AccountType = OperationAccountHelper.GetOperationSystemAccountTypeForFee(order, feeType);
            this.OperationFilialCode = order.FilialCode;
            this.Currency = "AMD";
            if (feeType == 28 || feeType == 29)
            {
                this.CustomerNumber = order.CustomerNumber;
            }

        }

        public OperationAccountHelper(TransitPaymentOrder order, short feeType)
        {
            this.AccountType = OperationAccountHelper.GetOperationSystemAccountTypeForFee(order, feeType);
            this.OperationFilialCode = order.FilialCode;
            this.Currency = order.Currency;
            if (feeType == 28 || feeType == 29)
            {
                this.CustomerNumber = order.CustomerNumber;
            }

        }

        /// <summary>
        /// Վերադարձնում է ներբանկային հաշիվը միջնորդավճարների համար
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static Account GetOperationSystemAccountForFee(Order order, short feeType)
        {
            OperationAccountHelper helper = new OperationAccountHelper();


            if (order.Type == OrderType.CashCredit || order.Type == OrderType.CashForRATransfer 
                || order.Type == OrderType.CashDebit || order.Type == OrderType.RATransfer
                || order.Type == OrderType.TransitCashOut || order.Type == OrderType.ReestrTransferOrder
                || order.Type == OrderType.CashOutFromTransitAccountsOrder
                || order.Type == OrderType.CashDebitConvertation
                || order.Type == OrderType.CashTransitCurrencyExchangeOrder)
            {
                PaymentOrder paymentOrder = (PaymentOrder)order;
                helper = new OperationAccountHelper(paymentOrder, feeType);
            }
            else if
                 (order.Type == OrderType.CashPosPayment)
            {
                CashPosPaymentOrder cashPosPaymentOrder = (CashPosPaymentOrder)order;
                helper = new OperationAccountHelper(cashPosPaymentOrder, feeType);
            }
            else if
                (order.Type == OrderType.CurrentAccountReOpen)
            {
                AccountReOpenOrder accountReOpenOrder = (AccountReOpenOrder)order;
                helper = new OperationAccountHelper(accountReOpenOrder, feeType);
            }
            else if
                (order.Type == OrderType.CashInternationalTransfer)
            {
                InternationalPaymentOrder cashInternationalTransfer = (InternationalPaymentOrder)order;
                helper = new OperationAccountHelper(cashInternationalTransfer, feeType);
            }
            else if
                (order.Type == OrderType.TransitPaymentOrder)
            {
                TransitPaymentOrder tranzitPaymentOrder = (TransitPaymentOrder)order;
                helper = new OperationAccountHelper(tranzitPaymentOrder, feeType);
            }


            return OperationAccountHelperDB.GetOperationSystemAccountForFee(helper);
        }

        /// <summary>
        /// Վերադարձնում է հաշվի տեսակը
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderAccountType"></param>
        /// <returns></returns>
        public static ushort GetOperationSystemAccountTypeForFee(Order order, short feeType)
        {
            return OperationAccountHelperDB.GetOperationSystemAccountTypeForFee(order, feeType);
        }

        /// <summary>
        /// Վերադարձնում է լիզինգի հաշիվը
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static Account GetOperationSystemAccountForLeasing(string operationCurrency, ushort filialCode) {
            return Account.GetOperationSystemAccount(3014, operationCurrency, filialCode);
        }
    }
}
