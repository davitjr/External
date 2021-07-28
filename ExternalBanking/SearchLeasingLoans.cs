using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.Leasing;

namespace ExternalBanking
{
    public class SearchLeasingLoan
    {
        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public string CustomerNumber { get; set; }
        /// <summary>
        /// Լիզինգի հաճախորդի համար
        /// </summary>
        public short LeasingCustomerNumber { get; set; }
        /// <summary>
        /// Անուն
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Անվանում
        /// </summary>
        public string OrganizationName { get; set; }
        /// <summary>
        /// ՀՀՎՀ
        /// </summary>
        public string TaxCode { get; set; }
        /// <summary>
        /// Անձնագրի համար
        /// </summary>
        public string PassportNumber { get; set; }


        public static List<LeasingLoan> Search(SearchLeasingLoan searchParams)
        {
            List<LeasingLoan> leasingLoans = LeasingDB.GetLeasingLoans(searchParams);
            return leasingLoans;
        }

        public static LeasingDetailedInformation GetLeasingDetailedInformation(long loanFullName, DateTime dateOfBeginning)
        {
            return LeasingDB.GetLeasingDetailedInformation(loanFullName, dateOfBeginning);
        }

        public static List<LeasingInsuranceDetails> GetLeasingInsuranceInformation(long loanFullName, DateTime dateOfBeginning)
        {
            return LeasingDB.GetLeasingInsuranceInformation(loanFullName, dateOfBeginning);
        }

        public static double GetPartlyMatureAmount(string contractNumber)
        {
            return LeasingDB.GetPartlyMatureAmount(contractNumber);
        }
        public static List<CustomerLeasingLoans> GetHBLeasingLoans(ulong customerNumber)
        {
            return LeasingDB.GetHBLeasingLoans(customerNumber);
        }

        public static LeasingLoanDetails GetHBLeasingLoanDetails(ulong productId)
        {
            return LeasingDB.GetHBLeasingLoanDetails(productId);
        }

        public static List<LeasingLoanRepayments> GetHBLeasingLoanRepayments(ulong productId)
        {
            return LeasingDB.GetHBLeasingLoanRepayments(productId);
        }

        public static List<LeasingLoanStatements> GetHBLeasingLoanStatements(ulong productId, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, int pageNumber = 1, int pageRowCount = 15, short orderByAscDesc = 0)
        {
            return LeasingDB.GetHBLeasingLoanStatements(productId, dateFrom, dateTo, minAmount, maxAmount, pageNumber, pageRowCount, orderByAscDesc);
        }

        public static List<AdditionalDetails> GetHBLeasingDetailsByAppID(ulong productId, int leasingInsuranceId = 0)
        {
            return LeasingDB.GetHBLeasingDetailsByAppID(productId, leasingInsuranceId);
        }

        public static List<LeasingPaymentsType> GetHBLeasingPaymentsType()
        {
            return LeasingDB.GetHBLeasingPaymentsType();
        }

        public static Account SetHBLeasingReceiver()
        {
            return LeasingDB.SetHBLeasingReceiver();
        }

        public static string GetHBLeasingPaymentDescription(short paymentType, short paymentSubType)
        {
            return LeasingDB.GetHBLeasingPaymentDescription(paymentType, paymentSubType);
        }

        public static LeasingLoanRepayments GetHBLeasingPaymentDetails(ulong productId)
        {
            return LeasingDB.GetHBLeasingPaymentDetails(productId);
        }
    }
}
