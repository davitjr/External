using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;


namespace ExternalBanking
{
    /// <summary>
    /// Ցպահանջ ավանդի տոկոսադրույք
    /// </summary>
    public class DemandDepositRate
    {
        /// <summary>
        /// Ունիկալ համար
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public Account DemandDepositAccount { get; set; }

        /// <summary>
        /// Սակագնի տեսակ
        /// </summary>
        public int TariffGroup { get; set; }

        /// <summary>
        /// Սակագնի տեսակի նկարագրություն
        /// </summary>
        public string TariffGroupDescription { get; set; }

        /// <summary>
        /// Հրամանի համար
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Հրամանի ամսաթիվ
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Սակագնի ենթատեսակը ըստ արժույթի և հաճախորդի տեսակի
        /// </summary>
        public int TariffTypeId { get; set; }

        /// <summary>
        /// Կրեդիտագրվող հաշվեհամար ցպահանջ ավանդի համար
        /// </summary>
        public double PercentCreditAccount { get; set; }


        public List<DemandDepositRateTariffDetail> DemandDepositRateTariffDetails { get; set; } = new List<DemandDepositRateTariffDetail>();



        /// <summary>
        /// Հաշվի սակագին
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public static DemandDepositRate GetDemandDepositRate(string accountNumber)
        {
            return DemandDepositRateDB.GetDemandDepositRate(accountNumber);
        }

        /// <summary>
        /// Ստանդարտ սակագներ
        /// </summary>
        /// <returns></returns>
        public static List<DemandDepositRate> GetDemandDepositRateTariffs()
        {
            return DemandDepositRateDB.GetDemandDepositRateTariffs();
        }

        /// <summary>
        /// Հատուկ սակագնի հրամանի ա/թ և հրամանի համար
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, DateTime>? GetDemandDepositRateTariffDocument(byte documentType)
        {
            KeyValuePair<string, DateTime>? document = new KeyValuePair<string, DateTime>?();
            document = DemandDepositRateDB.GetDemandDepositRateTariffDocument(documentType);
            return document;
        }

    }
}
