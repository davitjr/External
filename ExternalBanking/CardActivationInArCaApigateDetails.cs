using System;
namespace ExternalBanking
{
    public class CardActivationInArCaApigateDetails
    {

        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Պատասխանի հաղորդագրություն
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Փոփոխման ա/թ
        /// </summary>
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// ԱրՔա ուղարկման ենթակա արժեք
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        /// ԱրՔա ուղարկման ենթակա արժեքի նկարագրություն
        /// </summary>
        public string StatusDescription { get; set; }


    }
}
