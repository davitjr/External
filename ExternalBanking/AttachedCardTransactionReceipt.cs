using ExternalBanking.DBManager;
using System;

namespace ExternalBanking
{
    public class AttachedCardTransactionReceipt
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Ելքագրվող քարտի համար
        /// </summary>
        public string DebitCardNumber { get; set; }

        /// <summary>
        /// Մուտքագրվող քարտի համար
        /// </summary>
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Ելքագրվողի անուն, ազգանուն
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Մուտքագրվողի անուն, ազգանուն
        /// </summary>
        public string RecieverName { get; set; }
        /// <summary>
        /// Միջնորդավճարի գումար
        /// </summary>
        public double FeeAmount { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Հանձնարարականի ունիկալ համար (Id)
        /// </summary>
        public long Doc_Id { get; set; }
        /// <summary>
        /// Տեսակ 
        /// </summary>
        public OrderType Type { get; set; }
        /// <summary>
        /// Գործարքի Տեսակ 
        /// </summary>
        public int TransactionType { get; set; }
        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        /// <summary>
        /// Տերմինալ ID
        /// </summary>
        public string TerminalId { get; set; }
        /// <summary>
        /// Գործարքի անվանում
        /// </summary>
        public string TransactionName { get; set; }
        /// <summary>
        /// Գործարքի նկարագրություն
        /// </summary>
        public string TransactionPurpose { get; set; }
        public AttachedCardTransactionReceipt GetAttachedCardTransactionDetails()
        {
            AttachedCardTransactionReceipt order = new AttachedCardTransactionReceipt();
            order = CardToCardOrderDB.GetAttachedCardTransactionDetails(this);
            return order;
        }
    }
}
