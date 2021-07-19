using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForREST
{
  public class PaymentOrder
    {

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Վճարման հանձնարարականի ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }
        /// <summary>
        /// Վճարման հանձնարարականի գրանցման ամսաթիվ
        /// </summary>
        public string RegistrationDate { get; set; }
        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
        public Account DebitAccount { get; set; }
        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account ReceiverAccount { get; set; }
        /// <summary>
        /// Ստացող
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Ստացողի Բանկ
        /// </summary>
        public int ReceiverBankCode { get; set; }
        /// <summary>
        /// Գործարքի Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Գործարքի Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Վճարման հանձնարարականի տեսակ
        /// </summary>
        public short Type { get; set; }
        /// <summary>
        /// Վճարման հանձնարարականի ենթատեսակ
        /// </summary>
        public byte SubType { get; set; }
        /// <summary>
        /// Վճարման հանձնարարականի հերթական համար
        /// </summary>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Գործարքի նկարագրություն
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Փոխանակման փոխարժեք
        /// </summary>
        public double ConvertationRate { get; set; }
        /// <summary>
        /// Խաչաձև (Կրկնակի, Cross) փոխանակման դեպքում 2-րդ փոխարժեք
        /// </summary>
        public double ConvertationRate1 { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public double TransferFee { get; set; }
        /// <summary>
        /// Փոխանցման միջնորդավճարի հաշվեհամար
        /// </summary>
        public Account FeeAccount { get; set; }

        /// <summary>
        ///Շտապ փոխանցում
        /// </summary>
        public bool UrgentSign { get; set; }
    }
}
