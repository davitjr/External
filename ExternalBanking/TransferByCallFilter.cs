using ExternalBanking.DBManager;
using System;
namespace ExternalBanking
{
    public class TransferByCallFilter
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public TransferCallQuality Quality { get; set; }
        /// <summary>
        /// Գրանցման Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Գրանցման Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Համակարգ
        /// </summary>
        public short TransferSystem { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        ///  Գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        // Գրանցողի ՊԿ
        /// </summary>
        public long RegistrationSetNumber { get; set; }
        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }
        /// <summary>
        // Գրանցողի ՊԿ
        /// </summary>
        public short RegisteredBy { get; set; }
        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public ushort Filial { get; set; }
        /// <summary>
        /// Վերադարձնում է զանգով կատարված փոխանցումները ըստ ֆիլտրերի
        /// </summary> 
        public TransferByCallList GetList()
        {
            return TransferByCallDB.GetList(this);
        }

    }


}
