using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;


namespace ExternalBanking
{
    /// <summary>
    /// Կենսաթոշակ
    /// </summary>
    public class PensionApplication
    {
        /// <summary>
        /// Հերթական համար
        /// </summary>
        public ulong ContractId { get; set; }

        /// <summary>
        /// Համար
        /// </summary>
        public ulong ContractNumber { get; set; }

        /// <summary>
        /// Անուն ազգանուն
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Գրանցման ա/թ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Պայմանագրի սկիզբ
        /// </summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>
        /// Պայմանագրի վերջ
        /// </summary>
        public DateTime? DateOfNormalEnd { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Կենսաթոշակի ստացման եղանակի ծառայության տեսակ
        /// </summary>
        public short ServiceType { get; set; }

        /// <summary>
        /// Կենսաթոշակի ստացման եղանակի ծառայության տեսակի նկարագրություն
        /// </summary>
        public string ServiceTypeDescription { get; set; }


        /// <summary>
        /// Քարտի տեսակ
        /// </summary>
        public short CardType { get; set; }

        /// <summary>
        /// Քարտի տեսակի նկարագրություն
        /// </summary>
        public string CardTypeDescription { get; set; }

        /// <summary>
        /// Կենսախոշակի ստացման մասնաճյուղ
        /// </summary>
        public int FillialCode { get; set; }

        /// <summary>
        /// Դադարեցման նշում
        /// </summary>
        public bool Deleted { get; set; }


        /// <summary>
        /// Ստուգում է հաճախորդը ունի մուտքագրված դիմում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static bool HasPensionApplication(ulong customerNumber)
        {
            return PensionApplicationDB.HasPensionApplication(customerNumber);
        }

        /// <summary>
        /// Վերդարաձնում է հաճախորդի կենսաթոշակի դիմումների պատմությունը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<PensionApplication> GetPensionApplicationHistory(ulong customerNumber, ProductQualityFilter filter)
        {
            List<PensionApplication> pensionApplications = new List<PensionApplication>();
            if (filter == ProductQualityFilter.Opened || filter == ProductQualityFilter.NotSet)
            {
                pensionApplications.AddRange(PensionApplicationDB.GetPensionApplicationHistory(customerNumber));
            }
            if (filter == ProductQualityFilter.Closed)
            {
                pensionApplications.AddRange(PensionApplicationDB.GetClosedPensionApplicationHistory(customerNumber));
            }
            if (filter == ProductQualityFilter.All)
            {
                pensionApplications.AddRange(PensionApplicationDB.GetPensionApplicationHistory(customerNumber));
                pensionApplications.AddRange(PensionApplicationDB.GetClosedPensionApplicationHistory(customerNumber));
            }

            return pensionApplications;
        }


        /// <summary>
        /// Վերդարաձնում է հաճախորդի կենսաթոշակի տվյալները
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static PensionApplication GetPensionApplication(ulong customerNumber, ulong contractId)
        {
            return PensionApplicationDB.GetPensionApplication(customerNumber, contractId);
        }


    }
}
