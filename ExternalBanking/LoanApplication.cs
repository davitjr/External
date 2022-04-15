using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class LoanApplication
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }

        /// <summary>
        /// Դիմումի հերթական համար
        /// </summary>
        public long LoanApplicationNumber { get; set; }

        /// <summary>
        /// Դիմումի ա/թ
        /// </summary>
        public DateTime ApplicationDate { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի Տեսակ
        /// </summary>
        public short ProductType { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի Տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }

        /// <summary>
        /// Դիմումի գումար
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Տևողություն
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Առաջարկվող տոկոսադրույք
        /// </summary>
        public float InterestRateOffered { get; set; }

        /// <summary>
        /// Հաճախորդի հետ հաղորդակցման եղանակ
        /// </summary>
        public short CommunicationType { get; set; }

        /// <summary>
        /// Հաղորդակցման եղանակի նկարագրություն
        /// </summary>
        public string CommunicationTypeDescription { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի օգտագործման երկիր
        /// </summary>
        public int LoanUseCountry { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի օգտագործման մարզ
        /// </summary>
        public int LoanUseRegion { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի օգտագործման շրջան
        /// </summary>
        public int LoanUseLocality { get; set; }

        /// <summary>
        /// Քարտի համար
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public int FilialCode { get; set; }

        // <summary>
        /// Առաջարկվող գումար
        /// </summary>
        public double OfferedAmount { get; set; }


        /// <summary>
        /// Մերժման պատճառ
        /// </summary>
        public string RejectReason { get; set; }

        /// <summary>
        /// Հրաժարման պատճառ
        /// </summary>
        public string RefuseReason { get; set; }


        public static LoanApplication GetLoanApplication(ulong productId, ulong customerNumber)
        {
            return LoanApplicationDB.GetLoanApplication(productId, customerNumber);
        }


        public static List<LoanApplication> GetLoanApplications(ulong customerNumber)
        {
            List<LoanApplication> deposits = new List<LoanApplication>();

            deposits.AddRange(LoanApplicationDB.GetLoanApplications(customerNumber));

            return deposits;
        }


        public static List<FicoScoreResult> GetLoanApplicationFicoScoreResults(ulong customerNumber, ulong productId, DateTime requestDate)
        {
            return LoanApplicationDB.GetLoanApplicationFicoScoreResults(customerNumber, productId, requestDate);
        }

        public static LoanApplication GetLoanApplicationByDocId(long docId, ulong customerNumber)
        {
            ulong productId = LoanApplicationDB.GetLoanApplicationByDocId(docId);
            return LoanApplicationDB.GetLoanApplication(productId, customerNumber);
        }



    }
}
