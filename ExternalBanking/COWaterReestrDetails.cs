using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// ՋՕԸ-ի ռեեստրի տվյալներ
    /// </summary>
    public class COWaterReestrDetails
    {
        /// <summary>
        /// Հերթական համար
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// Աբոնենտի համար
        /// </summary>
        public string AbonentNumber { get; set; }

        /// <summary>
        /// Անուն Ազգանուն Հայրանուն
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Համայնք
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Ընդհամենը գանձում
        /// </summary>
        public double TotalCharge { get; set; }

        /// <summary>
        /// Ոռոգման ջրի վարձ
        /// </summary>
        public double WaterPayment { get; set; }

        /// <summary>
        /// Անդամավճար
        /// </summary>
        public double MembershipFee { get; set; }

        /// <summary>
        /// Ֆայլի անունը
        /// </summary>
        public string FileName { get; set; }

    }
}
