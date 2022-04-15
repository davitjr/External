using System;

namespace ExternalBanking.XBManagement
{
    /// <summary>
    /// Հեռախոսային բանկինգի ակտիվացման դիմում
    /// </summary>
    public class PhoneBankingContractActivationRequest
    {
        public long Id { get; set; }

        /// <summary>
        /// Փոփոխման տեսակ
        /// </summary>
        public RequestType RequestType { get; set; }

        /// <summary>
        /// Նկարագրություն
        /// </summary>
        public string RequestTypeDescription { get; set; }

        /// <summary>
        /// Փոփոխման ա/թ
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Սպասարկման գումար
        /// </summary>
        public double ServiceFee { get; set; }

        /// <summary>
        /// Հեռախոսային կամ հեռահար բանկինգի պայմանգրի ID
        /// </summary>
        public int GlobalID { get; set; }

        /// <summary>
        /// Անվճար է թե ոչ
        /// </summary>
        public bool IsFree { get; set; }
    }
}
