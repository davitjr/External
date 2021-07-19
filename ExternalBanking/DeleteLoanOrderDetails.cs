using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class DeleteLoanOrderDetails
    {
        /// <summary>
        /// Հայտի ունիկալ համար
        /// </summary>
        public uint OrderId { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Գործառնական օր
        /// </summary>
        public DateTime OperationDate { get; set; }

        /// <summary>
        /// Պայմանագրի համար
        /// </summary>
        public string GeneralNumber { get; set; }

        /// <summary>
        /// Հեռացման պատճառ
        /// </summary>
        public string DeleteReason { get; set; }

        /// <summary>
        /// Կարգավիճակի կոդ
        /// </summary>
        public byte Quality { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public string QualityDescription { get; set; }

        public static DeleteLoanOrderDetails GetLoanDeleteOrderDetails(uint orderId)
        {
            return DeleteLoanOrderDB.GetLoanDeleteOrderDetails(orderId);
        }

    }
}
