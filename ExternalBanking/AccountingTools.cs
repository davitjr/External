using ExternalBanking.DBManager;
using System;
namespace ExternalBanking
{
    public static class AccountingTools
    {
        /// <summary>
        /// Վերադարձնում է ՀՀ տարածքում կատարվող փոխանցման միջնորդավճարը
        /// </summary>
        internal static double GetFeeForLocalTransfer(PaymentOrder order)
        {
            if (!string.IsNullOrEmpty(order?.ReceiverAccount?.AccountNumber) && Order.CheckAccountForNonFee(order?.ReceiverAccount?.AccountNumber))
            {
                return 0;
            }

            if (order.Type == OrderType.CashForRATransfer)
                order.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(order, OrderAccountType.DebitAccount), order.DebitAccount.Currency, order.FilialCode);

            return AccountingToolsDB.GetFeeForLocalTransfer(order);
        }
        /// <summary>
        /// Վերադարձնում է սեփական հաշիվների միջև կատարվող փոխանցման միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        public static double GetCashPaymentOrderFee(int index, string fieldName, string accountNumber)
        {
            return AccountingToolsDB.GetCashPaymentOrderFee(index, fieldName, accountNumber);
        }

        /// <summary>
        /// Վերադարձնում է միջմասնաճուղային առանց հաշվի բացման փոխանցման ելքի միջնորդավճարի տոկոսը և գումարը:
        /// </summary>
        /// <returns></returns>
        public static void GetPriceForTransferCashOut(double amount, string currency, DateTime? date, short transferType, ulong transferID, out double percent, out double priceAmount)
        {
            AccountingToolsDB.GetPriceForTransferCashOut(amount, currency, date, transferType, transferID, out percent, out priceAmount);
        }



        /// <summary>
        /// Վերադարձնում է միջազգային փոխանցման միջնորդավճարը
        /// </summary>
        /// <param name="tariffGroup">Սակագնային խումբ (Vip)</param>
        /// <param name="order">Միջազգային ոխանցում</param>
        /// <returns></returns>
        internal static double GetFeeForInternationalTransfer(InternationalPaymentOrder order)
        {
            double trasferFee = 0;
            //double transfersTotal =order.Amount ;
            //TransferType transferType;
            //string countryCode = order.Country;
            //if (order.Currency == "USD" && (order.DetailsOfCharges == "OUR" || order.DetailsOfCharges == "OUROUR") && tariffGroup == 1 && order.Type!=OrderType.CashInternationalTransfer )
            //{
            //    transfersTotal = AccountingToolsDB.GetCustomerTransferSum(order.CustomerNumber, (order.RegistrationDate.AddDays(-1)).AddMonths(-3), order.RegistrationDate.AddDays(-1), order.Currency);
            //}

            //switch (order.DetailsOfCharges)
            //{
            //    case "":
            //        transferType=TransferType.Local;
            //        break;
            //    case "OUR":
            //        transferType =TransferType.Our ;
            //        break;
            //    case  "OUROUR":
            //        transferType =TransferType.OurOur ;
            //        break;
            //    case "BEN":
            //        transferType = TransferType.Ben ;
            //        break;                
            //      default:
            //        transferType=0;
            //         break; 
            //}



            //if (!(transferType == TransferType.Local || (transferType == TransferType.Our && order.Currency == "EUR" && (order.Country == "100" || order.Country == "792" || order.Country == "642"))))
            //    countryCode = "0";

            //trasferFee= AccountingToolsDB.GetFeeForTransfer(transferType,
            //                                                     tariffGroup,
            //                                                     order.RegistrationDate,
            //                                                     Convert.ToInt16(countryCode),
            //                                                     order.UrgentSign,
            //                                                     order.Currency,
            //                                                     order.Amount,
            //                                                     transfersTotal);

            trasferFee = AccountingToolsDB.GetFeeForInternationalTransfer(order.DebitAccount.AccountNumber, order.DetailsOfCharges, order.Currency, order.Amount, order.UrgentSign, Convert.ToInt16(order.Country), order.RegistrationDate);


            return trasferFee != -1 ? trasferFee : 0;
        }


        /// <summary>
        /// Վերադարձնում է Ա/Ձ ի հաշվից տնօրենի հաշվին փոխանցման միջնորդավճարը
        /// </summary>
        /// <returns></returns>
        public static double GetFromBusinessmanToOwnerAccountTransferRateAmount(PaymentOrder order)
        {
            return AccountingToolsDB.GetFromBusinessmanToOwnerAccountTransferRateAmount(order);
        }

        /// <summary>
        /// Կախիկ ելքի ժամանակ հաշվարկում է միջնորդավճարը
        /// </summary>
        /// <param name="order"></param>
        /// <param name="feeType"></param>
        /// <returns></returns>
        public static double GetCashoutOrderFee(PaymentOrder order, int feeType)
        {
            return AccountingToolsDB.GetCashoutOrderFee(order, feeType);
        }

        /// <summary>
        /// Վերադարձնում է ելքային գործարքների տարանցիկ հաշվին համապատասխանող միջնորդավճարի տոկոսադրույքը և նվազագույն միջնորդավճարը
        /// </summary>
        /// <param name="account"></param>
        /// <param name="filialCode"></param>
        /// <param name="minFeeAmount"></param>
        /// <returns></returns>
        public static double CashOutFromTransitAccountsOrderFee(Account account, ushort filialCode, ref double minFeeAmount)
        {
            double feeRate = 0;
            TransitAccountForDebitTransactions transitAccount = TransitAccountForDebitTransactions.GetTransitAccountsForDebitTransaction(account.AccountNumber, filialCode);
            feeRate = transitAccount.FeeRate / 100;
            minFeeAmount = transitAccount.MinFeeAmount;
            return feeRate;
        }

        /// <summary>
        /// Վերադարձնում դրամային հաշվին կանխիկ մուտքի միջնորդավճարի չափ
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double GetCashInOperationFeeForAMDAccount(string accountNumber, decimal amount)
        {
            return AccountingToolsDB.GetCashInOperationFeeForAMDAccount(accountNumber, amount);
        }


    }
}
