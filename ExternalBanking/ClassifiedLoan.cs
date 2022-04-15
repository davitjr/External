using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class ClassifiedLoan : LoanProduct
    {
        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public string Filial { get; set; }
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Վարկի տեսակ
        /// </summary>
        public string LoanTypeDescription { get; set; }

        /// <summary>
        /// Դասակարգման ամսաթիվ
        /// </summary>
        public DateTime ClassificationDate { get; set; }

        /// <summary>
        /// Դասակարգման դաս
        /// </summary>
        public RiskClassCode LoanClassType { get; set; }

        /// <summary>
        /// Դասակարգման դասի նկարագրություն
        /// </summary>
        public String LoanClassTypeDescription { get; set; }

        /// <summary>
        /// Դասակարգված վարեր
        /// </summary>
        public static List<ClassifiedLoan> GetClassifiedLoans(SearchClassifiedLoan searchParams, out int RowCount)
        {
            List<ClassifiedLoan> credits = new List<ClassifiedLoan>();
            credits = ClassifiedLoanDB.GetClassifiedLoans(searchParams, out RowCount);

            return credits;
        }
        public static void CustomersClassification(ACBAServiceReference.User user, SourceType source)
        {
            ClassifiedLoanDB.CustomersClassification(user, source);

        }


    }

}
