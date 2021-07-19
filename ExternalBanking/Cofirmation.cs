using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking
{

    /// <summary>
    /// Հաստատման գրառում
    /// </summary>
    public class Confirmation
    {

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; }
        /// <summary>
        /// Օգտագործողի համար
        /// </summary>
        public short UserId { get; set; }
        /// <summary>
        /// Գործարքի ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        /// Ելքագրվող հաշիվ
        /// </summary>
        public string DebetAccountNumber { get; set; }
        /// <summary>
        /// Մուտքագրվող հաշվի
        /// </summary>
        public string CreditAccountNumber { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Գումարը արժույթով
        /// </summary>
        public double AmountCurrency { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string Wording { get; set; }
        /// <summary>
        /// Լրացուցիչ նկարագրություն
        /// </summary>
        public string AddInf { get; set; }

        /// <summary>
        /// Գործարքի տեսակ
        /// </summary>
        public ConfirmationOperationType OperationType { get; set; }

        /// <summary>
        /// Շեղված փոխարժեք
        /// </summary>
        public double ChangedRate { get; set; }

        /// <summary>
        /// Գրառման ունիկալ համար
        /// </summary>
        public ulong UniqueNumber { get; set; }

        /// <summary>
        /// Շեղված փոխարկման հաստաման եղանակ(0-Դիլինգ չի գնում,1-Գնում է դիլինգ)
        /// </summary>
        public byte ApproveType { get; set; }

        /// <summary>
        /// ԳԳՀՄ/ՆՀՊ-ի հաստատումը պետք է , թե ոչ (0-պետք չէ, 1-պետք է)
        /// </summary>
        public bool IsNeedBranchAccountantApprovement { get; set; }

        /// <summary>
        /// Հայտի հերթական համար
        /// </summary>
        public long OrderId { get; set; }

        public int Result { get; set; }

        /// <summary>
        /// Հաստատման ա/թ
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>
        /// Դրամարկղի կատարմանը ուղարկվող հայտերի դեպքում միայն հաստատել 2
        /// Կատարել կոճակի դեպքում հաստատել և կատարել 1
        /// </summary>
        public byte ConfirmationType { get; set; }


        public void WriteToJournal()
        {
            ConfirmationDB.WriteToJournal(this);
        }

        public void InsertCurrencyMarket(CurrencyExchangeOrder order, int userFilialCode, int status, short direction)
        {
            ConfirmationDB.InsertIntoCurrencyMarket(order, userFilialCode, status, direction);
            if (order.IsDoulbeExchange)
            {
                bool isCross = true;
                ConfirmationDB.InsertIntoCurrencyMarket(order, userFilialCode, status, 2, isCross);
            }
        }

        public void AutomaticallyConfirmJournal(long orderID, short userID)
        {
            DateTime OperationDate = Utility.GetCurrentOperDay();
            ConfirmationDB.AutomaticallyConfirmJournal(orderID, userID, OperationDate);
        }
        public void InsertCurrencyMarketForInternational(CurrencyExchangeOrder order, int userFilialCode, int status, short direction)
        {
            ConfirmationDB.InsertIntoCurrencyMarketForInternational(order, userFilialCode, status, direction);
            if (order.IsDoulbeExchange)
            {
                bool isCross = true;
                ConfirmationDB.InsertIntoCurrencyMarketForInternational(order, userFilialCode, status, 2, isCross);
            }
        }

        public Confirmation()
        { }

        public Confirmation(PaymentOrder order)
        {
            FilialCode = order.FilialCode;
            UserId = order.user.userID;
            RegistrationDate = Convert.ToDateTime(order.OperationDate);
            DebetAccountNumber = order.DebitAccount.AccountNumber.ToString();
            CreditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
            Currency = order.Currency;
            ConfirmationType = (byte)(order.OnlySaveAndApprove ? 2 : 1);

            if (order.Currency == "AMD")
            {
                Amount = order.Amount;
                AmountCurrency = 0;
            }
            else
            {
                Amount = Utility.RoundAmount(order.Amount * Utility.GetCBKursForDate(Convert.ToDateTime(order.OperationDate), order.Currency), order.Currency);
                AmountCurrency = order.Amount;
            }
            if (order.Type == OrderType.FastTransferPaymentOrder)
                Wording = "öáË³ÝóáõÙ ³ñ³· Ñ³Ù³Ï³ñ·»ñáí";
            else
                Wording = Utility.ConvertUnicodeToAnsi(order.Description);

            if (order.Type == OrderType.RATransfer && order.SubType == 2)
                OperationType = ConfirmationOperationType.ExternalTransfer;
            else if (order.Type == OrderType.RATransfer && (order.SubType == 1 || order.SubType == 3 || order.SubType == 4))
                OperationType = ConfirmationOperationType.InternalTransfer;
            else if (order.Type == OrderType.CashCredit ||
                    order.Type == OrderType.CashDebit ||
                    order.Type == OrderType.CashOrder ||
                    order.Type == OrderType.CashForRATransfer ||
                    order.Type == OrderType.CashCommunalPayment ||
                    order.Type == OrderType.FastTransferPaymentOrder ||
                    order.Type == OrderType.TransitCashOut ||
                    order.Type == OrderType.TransitNonCashOut
                    )
                OperationType = ConfirmationOperationType.Cash;

            OrderId = order.Id;


        }

        public Confirmation(TransitPaymentOrder order)
        {
            FilialCode = order.FilialCode;
            UserId = order.user.userID;
            RegistrationDate = Convert.ToDateTime(order.OperationDate);
            DebetAccountNumber = order.DebitAccount.AccountNumber.ToString();
            CreditAccountNumber = order.TransitAccount.AccountNumber.ToString();
            Currency = order.Currency;
            ConfirmationType = (byte)(order.OnlySaveAndApprove ? 2 : 1);

            if (order.Currency == "AMD")
            {
                Amount = order.Amount;
                AmountCurrency = 0;
            }
            else
            {
                Amount = Utility.RoundAmount(order.Amount * Utility.GetCBKursForDate(Convert.ToDateTime(order.OperationDate), order.Currency), order.Currency);
                AmountCurrency = order.Amount;
            }

            Wording = Utility.ConvertUnicodeToAnsi(order.Description);

            OperationType = ConfirmationOperationType.Cash;

            OrderId = order.Id;


        }

        public Confirmation(InternationalPaymentOrder order, CurrencyExchangeOrder orderCurrencyExchange)
        {
            FilialCode = order.FilialCode;
            UserId = order.user.userID;
            RegistrationDate = Convert.ToDateTime(order.OperationDate);
            DebetAccountNumber = order.DebitAccount.AccountNumber.ToString();
            CreditAccountNumber = "0";
            Currency = order.Currency;
            AmountCurrency = order.Amount;
            ConfirmationType = (byte)(order.OnlySaveAndApprove ? 2 : 1);


            if (order.DebitAccount.Currency == "AMD")
            {
                ChangedRate = orderCurrencyExchange.IsVariation == ExchangeRateVariationType.None ? 0 : order.ConvertationRate;
                Amount = Utility.RoundAmount(order.Amount * order.ConvertationRate, "AMD");
            }
            else if (order.DebitAccount.Currency != order.Currency)
            {
                ChangedRate = orderCurrencyExchange.IsVariation == ExchangeRateVariationType.None ? 0 : order.ConvertationRate / order.ConvertationRate1;
                Amount = Utility.RoundAmount(order.Amount * (order.ConvertationRate1 / order.ConvertationRate), "AMD");
            }
            else
            {
                ChangedRate = 0;
                Amount = Utility.RoundAmount(order.Amount * Utility.GetCBKursForDate(Convert.ToDateTime(order.OperationDate), order.Currency), "AMD");
            }
            Wording = "ØÇç³½·³ÛÇÝ ÷áË³ÝóáõÙ" + (orderCurrencyExchange.IsVariation == ExchangeRateVariationType.None ? "" : " ³ñÅáõÛÃÇ ß»ÕáõÙáí");

            OperationType = ConfirmationOperationType.ExternalTransfer;

            if (orderCurrencyExchange.IsVariation == ExchangeRateVariationType.BranchVariation || orderCurrencyExchange.IsVariation == ExchangeRateVariationType.None)
                ApproveType = 0;
            else
                ApproveType = 1;
            UniqueNumber = order.UniqueNumber;
            OrderId = order.Id;
            if (order.user.TransactionLimit == -1)
            {
                Result = 1;
                ConfirmationDate = order.OperationDate;
            }
        }

        public Confirmation(CurrencyExchangeOrder order)
        {
            FilialCode = order.FilialCode;
            UserId = order.user.userID;
            RegistrationDate = Convert.ToDateTime(order.OperationDate);
            DebetAccountNumber = order.DebitAccount.AccountNumber.ToString();
            CreditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
            Currency = order.Currency;
            Amount = Convert.ToDouble(order.AmountInAmd);
            AmountCurrency = order.Amount;
            OperationType = ConfirmationOperationType.Convertation;
            ConfirmationType = (byte)(order.OnlySaveAndApprove ? 2 : 1);
            if (order.IsVariation != ExchangeRateVariationType.None)
            {
                if (order.IsDoulbeExchange)
                {
                    ChangedRate = order.ConvertationCrossRate;
                    Wording = Utility.ConvertUnicodeToAnsi("Արժույթի շեղումով փոխանակում, " + order.ConvertationCrossRate.ToString("#,0.0000000") + " կուրսով");
                }
                else
                {
                    ChangedRate = order.ConvertationRate;
                    Wording = Utility.ConvertUnicodeToAnsi("Արժույթի շեղումով փոխանակում, " + order.ConvertationRate.ToString() + " կուրսով");
                }
            }
            else
            {
                Wording = Utility.ConvertUnicodeToAnsi("Արժույթի փոխանակում");
            }


            UniqueNumber = order.UniqueNumber;

            if ((order.IsVariation == ExchangeRateVariationType.BranchVariation || order.IsVariation == ExchangeRateVariationType.None) && !order.SendDillingConfirm)
            {
                ApproveType = 0;
                IsNeedBranchAccountantApprovement = true;
            }
            else
            {
                ApproveType = 1;

                if (
                    ((order.Type == OrderType.CashCreditConvertation || order.IsVariation == ExchangeRateVariationType.BranchVariation || order.Type == OrderType.CashConvertation) && Amount >= 6000000) 
                    )
                    IsNeedBranchAccountantApprovement = true;
                else
                    IsNeedBranchAccountantApprovement = false;
            }


            OrderId = order.Id;

            if (order.user.TransactionLimit == -1)
            {
                Result = 1;
                ConfirmationDate = order.OperationDate;
            }

        }



        public Confirmation(UtilityPaymentOrder order)
        {
            FilialCode = order.FilialCode;
            UserId = order.user.userID;
            RegistrationDate = Convert.ToDateTime(order.OperationDate);
            DebetAccountNumber = order.DebitAccount.AccountNumber.ToString();
            CreditAccountNumber = order.ReceiverAccount.AccountNumber.ToString();
            Currency = order.Currency;
            Amount = order.Amount;
            AmountCurrency = 0;
            Wording = Utility.ConvertUnicodeToAnsi(order.Description);
            OperationType = ConfirmationOperationType.Cash;
            OrderId = order.Id;
            ConfirmationType = (byte)(order.OnlySaveAndApprove ? 2 : 1);

        }




        /// <summary>
        /// Քարտային հաշվից ելքագրման դեպքում, ստուգում է հաստաման անհրաժեշտությունը:
        /// </summary>
        /// <param name="order">Ստուգման ենթակա հայտ</param>
        /// <param name="user">Օգտագործող</param>
        /// <returns>true` եթե հաստատման անհրաժեշտությունը կա, 
        /// false՝ եթե չկա: </returns>
        public static bool IsConfirmatioRequiredForDebitFromCard(Order order, User user)
        {
            bool result = false;

            if ((order.Type == OrderType.RATransfer || order.Type == OrderType.CashCredit) && order.DebitAccount != null && order.DebitAccount.IsCardAccount())
            {
                if (user.TransactionLimit > 0)
                {
                    Card card = Card.GetCardWithOutBallance(order.DebitAccount.AccountNumber);
                    CardStatus cardStatus = Card.GetCardStatus((ulong)card.ProductId, order.CustomerNumber);

                    if (cardStatus.Status != 1)
                    {
                        result = true;
                    }
                }
            }

            return result;

        }
    }
}
