using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.XBManagement;
using System.Transactions;

namespace ExternalBanking
{
    public enum OrderPaymentType : short
    {
        /// <summary>
        /// 1 - Հիմնական գործարք
        /// </summary>
        MainTransaction = 1,

        /// <summary>
        /// 2 - Տոկոսագումարի մարում
        /// </summary>
        InterestRepayment = 2,

        /// <summary>
        /// 3 - Միջնորդավճար
        /// </summary>
        Fee = 3,

        /// <summary>
        /// 4 - Գազի սպասարկման վճար
        /// </summary>
        GasServiceFee = 4

        
    }

    public class BOOrderPayment
    {
        /// <summary>
        /// Հայտի համար
        /// </summary>
        public ulong OrderId { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public OrderPaymentType Type { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
        public string MainDebitAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public string MainCreditAccount { get; set; }

        /// <summary>
        /// Փոխանակման փոխարժեք
        /// </summary>
        public double ExchangeRate { get; set; }

        /// <summary>
        /// Խաչաձև (Կրկնակի, Cross) փոխանակման դեպքում 2-րդ փոխարժեք
        /// </summary>
        public double ExchangeRate1 { get; set; }
        
        /// <summary>
        /// Հանձնարարականի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Միջնորդավճարի տեսակ
        /// </summary>
        public short FeeType { get; set; }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public ulong Save(BOOrderPayment orderPayment, ACBAServiceReference.User user)
        {
            ulong result = 0;
            
            result = BOOrderPaymentDB.Save(orderPayment, user.userID);

            return result;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(PaymentOrder paymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;
            
            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = paymentOrder.Amount;                
            oneBOOrderPayment.Currency = paymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = paymentOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = paymentOrder.ReceiverAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = paymentOrder.Description;
            oneBOOrderPayment.ExchangeRate = paymentOrder.ConvertationRate;
            oneBOOrderPayment.ExchangeRate1 = paymentOrder.ConvertationRate1;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, paymentOrder.Type, paymentOrder.SubType);

            if (paymentOrder.Type == OrderType.CashConvertation ||
                    paymentOrder.Type == OrderType.CashCredit ||
                    paymentOrder.Type == OrderType.CashCreditConvertation ||
                    paymentOrder.Type == OrderType.CashDebit ||
                    paymentOrder.Type == OrderType.CashDebitConvertation ||
                    paymentOrder.Type == OrderType.CashForRATransfer ||
                    paymentOrder.Type == OrderType.TransitCashOutCurrencyExchangeOrder  ||
                    paymentOrder.Type == OrderType.TransitNonCashOutCurrencyExchangeOrder ||
                    paymentOrder.Type==OrderType.TransitCashOut  ||
                    paymentOrder.Type==OrderType.TransitNonCashOut 

                )
            {
                if (paymentOrder.Fees != null)
                {
                    oneBOOrderPayment = new BOOrderPayment();

                
                    paymentOrder.Fees.ForEach(m =>
                        {
                            if (m.Amount != 0)
                            {
                                oneBOOrderPayment.OrderId = orderId;
                                oneBOOrderPayment.Type = OrderPaymentType.Fee;
                                oneBOOrderPayment.Amount = m.Amount;
                                oneBOOrderPayment.Currency = m.Currency;
                                oneBOOrderPayment.MainDebitAccount = m.Account.AccountNumber.ToString();
                                oneBOOrderPayment.MainCreditAccount = "0";
                                oneBOOrderPayment.Description = m.TypeDescription;
                                oneBOOrderPayment.FeeType = m.Type;

                                orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                                BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, paymentOrder.Type, paymentOrder.SubType);
                            }
                        });
                }
                

            }
            else
            {
                if (paymentOrder.CardFee > 0)
                {
                    oneBOOrderPayment = new BOOrderPayment();
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = paymentOrder.CardFee;                    
                    oneBOOrderPayment.Currency = paymentOrder.CardFeeCurrency != null ? paymentOrder.CardFeeCurrency : paymentOrder.Currency;
                    oneBOOrderPayment.MainDebitAccount = paymentOrder.DebitAccount.AccountNumber.ToString();                    
                    oneBOOrderPayment.MainCreditAccount = "0";                    
                    oneBOOrderPayment.Description = "Քարտային ելքագրման միջնորդավճար";
                    oneBOOrderPayment.FeeType = 7;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, paymentOrder.Type, paymentOrder.SubType);
                }

                if (paymentOrder.TransferFee > 0)
                {
                    oneBOOrderPayment = new BOOrderPayment();
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = paymentOrder.TransferFee;
                    oneBOOrderPayment.Currency = "AMD";
                    oneBOOrderPayment.MainDebitAccount = paymentOrder.FeeAccount.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";                    
                    oneBOOrderPayment.Description = "Փոխանցման միջնորդավճար";
                    oneBOOrderPayment.FeeType = 5;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, paymentOrder.Type, paymentOrder.SubType);
                }
            }
            
            
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(BudgetPaymentOrder budgetPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;
            
            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = budgetPaymentOrder.Amount;
            oneBOOrderPayment.Currency = budgetPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = budgetPaymentOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = budgetPaymentOrder.ReceiverAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = budgetPaymentOrder.Description;
            oneBOOrderPayment.ExchangeRate = budgetPaymentOrder.ConvertationRate;
            oneBOOrderPayment.ExchangeRate1 = budgetPaymentOrder.ConvertationRate1;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, budgetPaymentOrder.Type, budgetPaymentOrder.SubType);

            if (budgetPaymentOrder.Type == OrderType.CashForRATransfer)
            {
                if (budgetPaymentOrder.Fees != null)
                {
                oneBOOrderPayment = new BOOrderPayment();
                budgetPaymentOrder.Fees.ForEach(m =>
                {
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = m.Amount;
                    oneBOOrderPayment.Currency = m.Currency;
                    oneBOOrderPayment.MainDebitAccount = m.Account.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = m.TypeDescription;
                    oneBOOrderPayment.FeeType = m.Type;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, budgetPaymentOrder.Type, budgetPaymentOrder.SubType);
                });
            }
            }
            else
            {
                if (budgetPaymentOrder.CardFee > 0)
                {
                    oneBOOrderPayment = new BOOrderPayment();
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = budgetPaymentOrder.CardFee;
                    oneBOOrderPayment.Currency = budgetPaymentOrder.CardFeeCurrency != null ? budgetPaymentOrder.CardFeeCurrency : budgetPaymentOrder.Currency;
                    oneBOOrderPayment.MainDebitAccount = budgetPaymentOrder.DebitAccount.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = "Քարտային ելքագրման միջնորդավճար";
                    oneBOOrderPayment.FeeType = 7;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, budgetPaymentOrder.Type, budgetPaymentOrder.SubType);
                }

                if (budgetPaymentOrder.TransferFee > 0)
                {
                    oneBOOrderPayment = new BOOrderPayment();
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = budgetPaymentOrder.TransferFee;
                    oneBOOrderPayment.Currency = "AMD";
                    oneBOOrderPayment.MainDebitAccount = budgetPaymentOrder.FeeAccount.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = "Փոխանցման միջնորդավճար";
                    oneBOOrderPayment.FeeType = 5;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, budgetPaymentOrder.Type, budgetPaymentOrder.SubType);
                }
            }            

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(UtilityPaymentOrder utilityPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;
            
            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = utilityPaymentOrder.Amount;
            oneBOOrderPayment.Currency = utilityPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = utilityPaymentOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = "0";
            oneBOOrderPayment.Description = utilityPaymentOrder.Description;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, utilityPaymentOrder.Type, utilityPaymentOrder.SubType);

            if (utilityPaymentOrder.ServiceAmount > 0)
            {
                oneBOOrderPayment = new BOOrderPayment();
                oneBOOrderPayment.OrderId = orderId;
                oneBOOrderPayment.Type = OrderPaymentType.GasServiceFee;
                oneBOOrderPayment.Amount = utilityPaymentOrder.ServiceAmount;
                oneBOOrderPayment.Currency = "AMD";
                oneBOOrderPayment.MainDebitAccount = utilityPaymentOrder.DebitAccount.AccountNumber.ToString();
                oneBOOrderPayment.MainCreditAccount = "0";
                oneBOOrderPayment.Description = "Գազի սպասարկման վճար";
                    
                orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, utilityPaymentOrder.Type, utilityPaymentOrder.SubType);
            }
            
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(MatureOrder maturePaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;
            
            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            if (maturePaymentOrder.Amount > 0)
            { 
                oneBOOrderPayment.OrderId = orderId;
                oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
                oneBOOrderPayment.Amount = maturePaymentOrder.Amount;
                oneBOOrderPayment.Currency = maturePaymentOrder.ProductCurrency;
                oneBOOrderPayment.MainDebitAccount = maturePaymentOrder.Account.AccountNumber.ToString();
                oneBOOrderPayment.MainCreditAccount = "0";
                oneBOOrderPayment.Description = maturePaymentOrder.Description;

                orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, maturePaymentOrder.Type, maturePaymentOrder.SubType);
            }

            if (maturePaymentOrder.PercentAmount > 0)
            {
                oneBOOrderPayment = new BOOrderPayment();
                oneBOOrderPayment.OrderId = orderId;
                oneBOOrderPayment.Type = OrderPaymentType.InterestRepayment;
                oneBOOrderPayment.Amount = maturePaymentOrder.PercentAmount;
                oneBOOrderPayment.Currency = maturePaymentOrder.ProductCurrency;
                oneBOOrderPayment.MainDebitAccount = maturePaymentOrder.PercentAccount.AccountNumber.ToString();
                oneBOOrderPayment.MainCreditAccount = "0";
                oneBOOrderPayment.Description = "Տոկոսագումարի մարում";

                orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, maturePaymentOrder.Type, maturePaymentOrder.SubType);
            }

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(DepositOrder depositOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;
            
            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            if (depositOrder.Amount > 0)
            {
                oneBOOrderPayment.OrderId = orderId;
                oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
                oneBOOrderPayment.Amount = depositOrder.Amount;
                oneBOOrderPayment.Currency = depositOrder.Currency;
                oneBOOrderPayment.MainDebitAccount = depositOrder.DebitAccount.AccountNumber.ToString();
                oneBOOrderPayment.MainCreditAccount = "0";
                oneBOOrderPayment.Description = depositOrder.Description;

                orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, depositOrder.Type, depositOrder.SubType);
            }
            
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(AccountOrder accountOrder, ulong orderId, ACBAServiceReference.User user)
        {
            AccountReOpenOrder reOpenOrder = (AccountReOpenOrder)accountOrder;
            ulong orderPaymentId = 0;
            
            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            if (reOpenOrder.FeeChargeType > 0)
            {
                oneBOOrderPayment.OrderId = orderId;
                oneBOOrderPayment.Type = OrderPaymentType.Fee;
                oneBOOrderPayment.Amount = reOpenOrder.Amount;
                oneBOOrderPayment.Currency = accountOrder.Currency;
                oneBOOrderPayment.MainDebitAccount = reOpenOrder.ReOpeningAccounts[0].AccountNumber.ToString();
                oneBOOrderPayment.MainCreditAccount = "0";
                oneBOOrderPayment.Description = accountOrder.Description;
                oneBOOrderPayment.FeeType = 13;

                orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, accountOrder.Type, accountOrder.SubType);
            }

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(InternationalPaymentOrder internationalPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = internationalPaymentOrder.Amount;
            oneBOOrderPayment.Currency = internationalPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = internationalPaymentOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = internationalPaymentOrder.ReceiverAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = internationalPaymentOrder.Description;
            oneBOOrderPayment.ExchangeRate = internationalPaymentOrder.ConvertationRate;
            oneBOOrderPayment.ExchangeRate1 = internationalPaymentOrder.ConvertationRate1;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, internationalPaymentOrder.Type, internationalPaymentOrder.SubType);

            if (internationalPaymentOrder.Type == OrderType.CashInternationalTransfer)
            {
                if (internationalPaymentOrder.Fees != null)
                {
                oneBOOrderPayment = new BOOrderPayment();
                internationalPaymentOrder.Fees.ForEach(m =>
                {
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = m.Amount;
                    oneBOOrderPayment.Currency = m.Currency;
                    oneBOOrderPayment.MainDebitAccount = m.Account.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = m.TypeDescription;
                    oneBOOrderPayment.FeeType = m.Type;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, internationalPaymentOrder.Type, internationalPaymentOrder.SubType);
                });
            }
            }
            else
            {
                if (internationalPaymentOrder.CardFee > 0)
                {
                    oneBOOrderPayment = new BOOrderPayment();
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = internationalPaymentOrder.CardFee;
                    oneBOOrderPayment.Currency = internationalPaymentOrder.CardFeeCurrency != null ? internationalPaymentOrder.CardFeeCurrency : internationalPaymentOrder.Currency;
                    oneBOOrderPayment.MainDebitAccount = internationalPaymentOrder.DebitAccount.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = "Քարտային ելքագրման միջնորդավճար";
                    oneBOOrderPayment.ExchangeRate = 0;
                    oneBOOrderPayment.ExchangeRate1 = 0;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, internationalPaymentOrder.Type, internationalPaymentOrder.SubType);
                }

                if (internationalPaymentOrder.TransferFee > 0)
                {
                    oneBOOrderPayment = new BOOrderPayment();
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = internationalPaymentOrder.TransferFee;
                    oneBOOrderPayment.Currency = "AMD";
                    oneBOOrderPayment.MainDebitAccount = internationalPaymentOrder.FeeAccount.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = "Փոխանցման միջնորդավճար";
                    oneBOOrderPayment.ExchangeRate = 0;
                    oneBOOrderPayment.ExchangeRate1 = 0;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, internationalPaymentOrder.Type, internationalPaymentOrder.SubType);
                }
            }
            
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(FastTransferPaymentOrder fastTransferPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = fastTransferPaymentOrder.Amount;
            oneBOOrderPayment.Currency = fastTransferPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = fastTransferPaymentOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount ="0";
            oneBOOrderPayment.Description = fastTransferPaymentOrder.Description;
            oneBOOrderPayment.ExchangeRate = fastTransferPaymentOrder.ConvertationRate;
            oneBOOrderPayment.ExchangeRate1 = fastTransferPaymentOrder.ConvertationRate1;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, fastTransferPaymentOrder.Type, fastTransferPaymentOrder.SubType);

     
            if (fastTransferPaymentOrder.Fees != null)
            {
                oneBOOrderPayment = new BOOrderPayment();
                fastTransferPaymentOrder.Fees.ForEach(m =>
                {
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = m.Amount;
                    oneBOOrderPayment.Currency = m.Currency;
                    oneBOOrderPayment.MainDebitAccount = m.Account.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = m.TypeDescription;
                    oneBOOrderPayment.FeeType = m.Type;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, fastTransferPaymentOrder.Type, fastTransferPaymentOrder.SubType);
                });
            }
 
  
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(ReceivedFastTransferPaymentOrder receivedFastTransferPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = receivedFastTransferPaymentOrder.Amount;
            oneBOOrderPayment.Currency = receivedFastTransferPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = "0";
            oneBOOrderPayment.MainCreditAccount = receivedFastTransferPaymentOrder.ReceiverAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = receivedFastTransferPaymentOrder.Description;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, receivedFastTransferPaymentOrder.Type, receivedFastTransferPaymentOrder.SubType);

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(TransferByCallChangeOrder tansferByCallChangeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            if (tansferByCallChangeOrder.SubType==5)
            {
                oneBOOrderPayment.Amount = tansferByCallChangeOrder.ReceivedFastTransfer.Amount;
                oneBOOrderPayment.Currency = tansferByCallChangeOrder.ReceivedFastTransfer.Currency;
                oneBOOrderPayment.MainCreditAccount = tansferByCallChangeOrder.ReceivedFastTransfer.ReceiverAccount.AccountNumber.ToString();
            }
            else
            {
                oneBOOrderPayment.Amount = 0;
                oneBOOrderPayment.Currency = "";
                oneBOOrderPayment.MainCreditAccount ="0";
            }
            oneBOOrderPayment.MainDebitAccount = "0";
            oneBOOrderPayment.Description = tansferByCallChangeOrder.Description;
            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, tansferByCallChangeOrder.Type, tansferByCallChangeOrder.SubType);

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(TransitPaymentOrder transitPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = transitPaymentOrder.Amount;
            oneBOOrderPayment.Currency = transitPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = transitPaymentOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = transitPaymentOrder.TransitAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = transitPaymentOrder.Description;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, transitPaymentOrder.Type, transitPaymentOrder.SubType);
            
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(CurrencyExchangeOrder currencyExchangeOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = currencyExchangeOrder.Amount;
            oneBOOrderPayment.Currency = currencyExchangeOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = currencyExchangeOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = currencyExchangeOrder.ReceiverAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = currencyExchangeOrder.Description;
            oneBOOrderPayment.ExchangeRate = currencyExchangeOrder.ConvertationRate;
            oneBOOrderPayment.ExchangeRate1 = currencyExchangeOrder.ConvertationRate1;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, currencyExchangeOrder.Type, currencyExchangeOrder.SubType);
                        
            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(CashPosPaymentOrder cashPosPaymentOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = cashPosPaymentOrder.Amount;
            oneBOOrderPayment.Currency = cashPosPaymentOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = cashPosPaymentOrder.PosAccount.AccountNumber.ToString();
            if (cashPosPaymentOrder.CreditAccount != null)
            {
                oneBOOrderPayment.MainCreditAccount = cashPosPaymentOrder.CreditAccount.AccountNumber.ToString();
            }            
            else
            {
                oneBOOrderPayment.MainCreditAccount = "0";
            }
            oneBOOrderPayment.Description = cashPosPaymentOrder.Description;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, cashPosPaymentOrder.Type, cashPosPaymentOrder.SubType);

            if (cashPosPaymentOrder.Fees != null)
            {
                oneBOOrderPayment = new BOOrderPayment();
                cashPosPaymentOrder.Fees.ForEach(m =>
                {
                    oneBOOrderPayment.OrderId = orderId;
                    oneBOOrderPayment.Type = OrderPaymentType.Fee;
                    oneBOOrderPayment.Amount = m.Amount;
                    oneBOOrderPayment.Currency = m.Currency;
                    oneBOOrderPayment.MainDebitAccount = m.Account.AccountNumber.ToString();
                    oneBOOrderPayment.MainCreditAccount = "0";
                    oneBOOrderPayment.Description = m.TypeDescription;
                    oneBOOrderPayment.FeeType = m.Type;

                    orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
                    BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, cashPosPaymentOrder.Type, cashPosPaymentOrder.SubType);
                });
            }

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(HBActivationOrder activationHBOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = activationHBOrder.Amount;
            oneBOOrderPayment.Currency = activationHBOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = activationHBOrder.DebitAccount.AccountNumber.ToString();
            oneBOOrderPayment.MainCreditAccount = activationHBOrder.CreditAccount.AccountNumber.ToString();
            oneBOOrderPayment.Description = activationHBOrder.Description;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, activationHBOrder.Type, activationHBOrder.SubType);

            return orderPaymentId;
        }

        /// <summary>
        /// Վճարման հանձնարարականի պահպանում:
        /// </summary>                
        /// <param name="orderId">Հայտի համար</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns></returns>
        public static ulong Save(FeeForServiceProvidedOrder feeForServiceProvidedOrder, ulong orderId, ACBAServiceReference.User user)
        {
            ulong orderPaymentId = 0;

            BOOrderPayment oneBOOrderPayment = new BOOrderPayment();

            oneBOOrderPayment.OrderId = orderId;
            oneBOOrderPayment.Type = OrderPaymentType.MainTransaction;
            oneBOOrderPayment.Amount = feeForServiceProvidedOrder.Amount;
            oneBOOrderPayment.Currency = feeForServiceProvidedOrder.Currency;
            oneBOOrderPayment.MainDebitAccount = feeForServiceProvidedOrder.DebitAccount.AccountNumber.ToString();
            if (feeForServiceProvidedOrder.ReceiverAccount != null)
            {
                oneBOOrderPayment.MainCreditAccount = feeForServiceProvidedOrder.ReceiverAccount.AccountNumber.ToString();
            }
            else
            {
                oneBOOrderPayment.MainCreditAccount = "0";
            }
            
            oneBOOrderPayment.Description = feeForServiceProvidedOrder.Description;

            orderPaymentId = oneBOOrderPayment.Save(oneBOOrderPayment, user);
            BOOrderPaymentAddition.Save(oneBOOrderPayment, orderPaymentId, feeForServiceProvidedOrder.Type, feeForServiceProvidedOrder.SubType);

            return orderPaymentId;
        }
    }
}
