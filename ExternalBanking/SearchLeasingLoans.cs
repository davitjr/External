using ExternalBanking.DBManager;
using ExternalBanking.Leasing;
using System;
using System.Collections.Generic;

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

        public static List<CustomerLeasingLoans> GetLeasings(ulong customerNumber)
        {
            return LeasingDB.GetLeasings(customerNumber);
        }

        public static LeasingLoanDetails GetLeasing(ulong productId)
        {
            return LeasingDB.GetLeasing(productId);
        }

        public static List<LeasingLoanRepayments> GetLeasingRepayments(ulong productId, byte firstReschedule = 0)
        {
            return LeasingDB.GetLeasingRepayments(productId, firstReschedule);
        }       

        public static List<LeasingLoanStatements> GetLeasingLoanStatements(ulong productId, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, int pageNumber = 1, int pageRowCount = 15, short orderByAscDesc = 0)
        {
            return LeasingDB.GetLeasingLoanStatements(productId, dateFrom, dateTo, minAmount, maxAmount, pageNumber, pageRowCount, orderByAscDesc);
        }

        public static List<LeasingPaymentsType> GetLeasingPaymentsType()
        {
            return LeasingDB.GetLeasingPaymentsType();
        }

        public static List<AdditionalDetails> GetLeasingDetailsByAppID(ulong productId, int leasingInsuranceId = 0)
        {
            return LeasingDB.GetLeasingDetailsByAppID(productId, leasingInsuranceId);
        }

        public static Account SetLeasingReceiver()
        {
            return LeasingDB.SetLeasingReceiver();
        }

        public static string GetLeasingPaymentDescription(short paymentType, short paymentSubType)
        {
            return LeasingDB.GetLeasingPaymentDescription(paymentType, paymentSubType);
        }

        public static LeasingLoanRepayments GetLeasingPaymentDetails(ulong productId)
        {
            return LeasingDB.GetLeasingPaymentDetails(productId);
        }
        
        public static List<LeasingOverdueDetail> GetLeasingOverdueDetails(ulong productId)
        {
            return LeasingDB.GetLeasingOverdueDetails(productId);
        }

        public static ulong GetManagerCustomerNumber(ulong customerNumber)
        {
            return LeasingDB.GetManagerCustomerNumber(customerNumber);
        }

        public static List<LeasingInsurance> GetLeasingInsurances(ulong productId)
        {
            return LeasingDB.GetLeasingInsurances(productId);
        }

    }
}
