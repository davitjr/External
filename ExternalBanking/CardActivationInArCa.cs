using System;
using System.Collections.Generic;
using System.Linq;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Ակտիվացումներ ԱՐՔԱ-ում
    /// </summary>
    public class CardActivationInArCa
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Քարտի համարը
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Մուտքի/ելքի ակտիվացման եղանակ
        /// </summary>
        public string ActivationType { get; set; }
        /// <summary>
        /// Գումար
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Կրեդիտ/դեբետ
        /// </summary>
        public string DebitCredit { get; set; }
        /// <summary>
        /// Ուղարկման ամսաթիվը և ժամը
        /// </summary>
        public DateTime SendDate { get; set; }

        /// <summary>
        /// ԱրՔա ուղարկման ենթակա արժեք
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        /// ԱրՔա ուղարկման ենթակա արժեքի նկարագրություն
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Apigate-ի տվյալներ
        /// </summary>
        public CardActivationInArCaApigateDetails CardActivationInArCaApigateDetails { get; set; }

        /// <summary>
        /// Payment-ի տվյլաներ
        /// </summary>
        public CardActivationInArCaPaymentDetails CardActivationInArCaPaymentDetails { get; set; }

        /// <summary>
        /// Ակտիվացումներ ԱրՔայում
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<CardActivationInArCa> GetCardActivationInArCa(string cardNumber, DateTime startDate, DateTime endDate)
        {
            List<CardActivationInArCa> list = new List<CardActivationInArCa>();

            list.AddRange(GetCardActivationInArCaPaymentDetails(cardNumber, startDate, endDate));
            list.AddRange(GetCardActivationInArCaApigateDetails(cardNumber, startDate, endDate));
            list = list.OrderByDescending(m => m.SendDate).ToList();

            return list;
        }

        /// <summary>
        /// Payment
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<CardActivationInArCa> GetCardActivationInArCaPaymentDetails(string cardNumber, DateTime startDate, DateTime endDate)
        {
            return CardActivationInArCaDB.GetCardActivationInArCaPaymentDetails(cardNumber, startDate, endDate);
        }

        /// <summary>
        /// Apigate
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<CardActivationInArCa> GetCardActivationInArCaApigateDetails(string cardNumber, DateTime startDate, DateTime endDate)
        {
            return CardActivationInArCaDB.GetCardActivationInArCaApigateDetails(cardNumber, startDate, endDate);
        }

        /// <summary>
        /// Վերջին ուղարկման ֆայլի ա/թ և ժամ
        /// </summary>
        /// <returns></returns>
        public static DateTime?  GetLastSendedPaymentFileDate()
        {
            return CardActivationInArCaDB.GetLastSendedPaymentFileDate();
        }


        public static List<CardActivationInArCaApigateDetails> GetCardActivationInArCaApigateDetail(ulong Id)
        {
            List<CardActivationInArCaApigateDetails> list = new List<CardActivationInArCaApigateDetails>();
            list = CardActivationInArCaDB.GetCardActivationInArCaApigateDetail(Id);
            return list;
        }


    }
}
