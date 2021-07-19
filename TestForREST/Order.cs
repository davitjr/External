using System;
using System.Collections.Generic;

namespace TestForREST
{

    public enum OrderType : short
    {
        /// <summary>
        /// Փոխանցում ՀՀ տարածքում
        /// </summary>
        RATransfer = 1,
        /// <summary>
        /// Փոխանակում
        /// </summary>
        Convertation = 2,
        /// <summary>
        /// Միջազգային փոխացում
        /// </summary>
        InternationalTransfer = 3,
        /// <summary>
        /// Ավանդի դադարեցում
        /// </summary>
        DepositTermination = 4,
        /// <summary>
        /// Վարկի մարում
        /// </summary>
        LoanMature = 5,
        /// <summary>
        /// Ավանդի ձևակերպում
        /// </summary>
        Deposit = 9,
        /// <summary>
        /// Պարբերական փոխանցում
        /// </summary>
        PeriodicTransfer = 10,

        CommunalPayment=15,
        /// <summary>
        /// Վարկային գծի դադարեցում
        /// </summary>
        CreditLineMature = 21,
        /// <summary>
        /// Տեղեկանքի ստացման հայտ
        /// </summary>
        ReferenceOrder=20


    }

    public enum OrderQuality : short
    {
        /// <summary>
        /// Խմբագրվում է
        /// </summary>
        Draft = 1,


        /// <summary>
        /// Ուղղարկված է
        /// </summary>
        Sent = 3,
        /// <summary>
        /// Կատարված է
        /// </summary>
        Approved = 30,
        /// <summary>
        /// Մերժված է
        /// </summary>
        Declined = 31,
        /// <summary>
        /// Հեռացված է
        /// </summary>
        Removed = 40
    }

	public class Order
    {
        /// <summary>
        /// Հանձնարարականի ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Տեսակ 
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// Հանձնարարականի ենթատեսակ
        /// </summary>
        public byte SubType { get; set; }

        /// <summary>
        /// Հանձնարարականի հերթական համար
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public string RegistrationDate { get; set; }

        /// <summary>
        /// Ենթատեսակի նկարագրություն
        /// </summary>
        public string SubTypeDescription { get; set; }

        /// <summary>
        /// Հանձնարարականի նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public OrderQuality Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

    }
}
