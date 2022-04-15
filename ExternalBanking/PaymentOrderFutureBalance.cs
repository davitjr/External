using System;

namespace ExternalBanking
{
    /// <summary>
    ///Վճարման հանձնարարականում օգտագործվող հաշիվների ապագա մնացորդներ
    /// </summary>
    public class PaymentOrderFutureBalance
    {
        //Դեբետ հաշվի ապագա մնացորդ
        public FutureBalance DebitAccountFutureBalance { get; set; }

        //Կրեդիտ հաշվի ապագա մնացորդ (Փոխանցում սեփական հաշիվների մեջ,Փոխարկում)
        public FutureBalance CreditAccountFutureBalance { get; set; }

        //Միջնորդավճարի հաշվի ապագա մնացորդ (Շտապ փոխանցում,Արժույթով փոխանցում ՀՀ տարածքում)
        public FutureBalance FeeAccountFutureBalance { get; set; }


        public PaymentOrderFutureBalance()
        {
            DebitAccountFutureBalance = new FutureBalance();
            CreditAccountFutureBalance = new FutureBalance();
            FeeAccountFutureBalance = new FutureBalance();
        }

        internal void GetPaymentOrderFutureBalance(PaymentOrder paymentOrder)
        {


            if (paymentOrder.Type != OrderType.Convertation)
            {

                if (paymentOrder.Type == OrderType.CommunalPayment)
                {
                    UtilityPaymentOrder order = new UtilityPaymentOrder();
                    order.Id = paymentOrder.Id;
                    order.Get();
                    if (order.CommunalType == CommunalTypes.Gas)
                    {
                        double serviceAmount = order.ServiceAmount;
                        this.DebitAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.DebitAccount.AccountNumber);
                        this.DebitAccountFutureBalance.BalanceAfter = this.DebitAccountFutureBalance.BalanceBefore - paymentOrder.Amount - order.ServiceAmount;
                        this.DebitAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.DebitAccount.AccountNumber, paymentOrder.Source) + this.DebitAccountFutureBalance.BalanceAfter;


                    }
                    else
                    {
                        this.DebitAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.DebitAccount.AccountNumber);
                        this.DebitAccountFutureBalance.BalanceAfter = this.DebitAccountFutureBalance.BalanceBefore - paymentOrder.Amount;
                        this.DebitAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.DebitAccount.AccountNumber, paymentOrder.Source) + this.DebitAccountFutureBalance.BalanceAfter;

                    }
                }
                else
                {
                    //Դեբետ հաշվի ապագա մնացորդ
                    if (paymentOrder.DebitAccount != null && paymentOrder.DebitAccount.AccountNumber != "0")
                    {
                        this.DebitAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.DebitAccount.AccountNumber);
                        this.DebitAccountFutureBalance.BalanceAfter = this.DebitAccountFutureBalance.BalanceBefore - paymentOrder.Amount;
                        this.DebitAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.DebitAccount.AccountNumber, paymentOrder.Source) + this.DebitAccountFutureBalance.BalanceAfter;
                    }

                    if (paymentOrder.SubType == 3)
                    {
                        //Կրեդիտ հաշվի ապագա մնացորդ
                        if (paymentOrder.ReceiverAccount != null && paymentOrder.ReceiverAccount.AccountNumber != "0")
                        {
                            this.CreditAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.ReceiverAccount.AccountNumber);
                            this.CreditAccountFutureBalance.BalanceAfter = this.CreditAccountFutureBalance.BalanceBefore + paymentOrder.Amount;
                            this.CreditAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.ReceiverAccount.AccountNumber, paymentOrder.Source) + this.CreditAccountFutureBalance.BalanceAfter;
                        }
                    }

                    //Միջնորդավճարի հաշվի ապագա մնացորդ
                    if (paymentOrder.FeeAccount != null && paymentOrder.FeeAccount.AccountNumber != "0")
                    {
                        this.FeeAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.FeeAccount.AccountNumber);
                        this.FeeAccountFutureBalance.BalanceAfter = this.FeeAccountFutureBalance.BalanceBefore - paymentOrder.TransferFee;
                        this.FeeAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.FeeAccount.AccountNumber, paymentOrder.Source) + this.FeeAccountFutureBalance.BalanceAfter;
                    }

                }
            }
            else
            {
                paymentOrder.DebitAccount = Account.GetAccount(paymentOrder.DebitAccount.AccountNumber);
                paymentOrder.ReceiverAccount = Account.GetAccount(paymentOrder.ReceiverAccount.AccountNumber);
                double convAmount = 0;

                this.DebitAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.DebitAccount.AccountNumber);
                this.CreditAccountFutureBalance.BalanceBefore = Account.GetAccountBalance(paymentOrder.ReceiverAccount.AccountNumber);

                if (paymentOrder.DebitAccount.Currency == "AMD" && paymentOrder.ReceiverAccount.Currency != "AMD")
                {
                    //Փոխարկման գումար
                    convAmount = paymentOrder.Amount * paymentOrder.ConvertationRate;

                    this.DebitAccountFutureBalance.BalanceAfter = this.DebitAccountFutureBalance.BalanceBefore - convAmount;

                    this.CreditAccountFutureBalance.BalanceAfter = this.CreditAccountFutureBalance.BalanceBefore + paymentOrder.Amount;
                }
                else if (paymentOrder.DebitAccount.Currency != "AMD" && paymentOrder.ReceiverAccount.Currency == "AMD")
                {
                    //Փոխարկման գումար
                    convAmount = paymentOrder.Amount * paymentOrder.ConvertationRate;

                    this.DebitAccountFutureBalance.BalanceAfter = this.DebitAccountFutureBalance.BalanceBefore - paymentOrder.Amount;

                    this.CreditAccountFutureBalance.BalanceAfter = this.CreditAccountFutureBalance.BalanceBefore + convAmount;
                }
                else if (paymentOrder.DebitAccount.Currency != "AMD" && paymentOrder.ReceiverAccount.Currency != "AMD")
                {
                    //Փոխարկման գումար Քրոս փոխարկման դեպքում
                    convAmount = paymentOrder.Amount * Math.Round(paymentOrder.ConvertationRate / paymentOrder.ConvertationRate1, 5);

                    this.DebitAccountFutureBalance.BalanceAfter = this.DebitAccountFutureBalance.BalanceBefore - paymentOrder.Amount;

                    this.CreditAccountFutureBalance.BalanceAfter = this.CreditAccountFutureBalance.BalanceBefore + convAmount;
                }

                this.DebitAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.DebitAccount.AccountNumber, paymentOrder.Source) + this.DebitAccountFutureBalance.BalanceAfter;
                this.CreditAccountFutureBalance.BalanceAfterFull = Order.GetSentOrdersAmount(paymentOrder.ReceiverAccount.AccountNumber, paymentOrder.Source) + this.CreditAccountFutureBalance.BalanceAfter;
            }


        }
    }
}
