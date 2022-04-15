using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LoanProductClassification
    {
        /// <summary>
        /// Դասակարգման ա/թ
        /// </summary>
        public DateTime ClassificationDate { get; set; }

        /// <summary>
        /// Պահուստավորվող գումար
        /// </summary>
        public double StoreAmount { get; set; }

        /// <summary>
        /// Օրեր
        /// </summary>
        public int DaysCount { get; set; }

        /// <summary>
        /// Դասակարգման տեսակ , 0- Օբյեկտիվ , 1- Սուբյեկտիվ
        /// </summary>
        public byte StoreType { get; set; }

        /// <summary>
        /// Դաս
        /// </summary>
        public float StorePercent { get; set; }

        /// <summary>
        /// Տեսակ (Օգտ․ մաս,Չօգտ․ մաս, Օգտ․ մաս գեր․)
        /// </summary>
        public string StoreAmountTypeDescription { get; set; }

        /// <summary>
        /// Դասակարգված վարկերի ցանկ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        public static List<LoanProductClassification> GetLoanProductClassifications(ulong productId, DateTime dateFrom)
        {
            List<LoanProductClassification> result = LoanProductClassificationDB.GetLoanProductClassifications(productId, dateFrom);
            return result;
        }

    }
}
