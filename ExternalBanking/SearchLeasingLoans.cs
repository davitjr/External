using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
