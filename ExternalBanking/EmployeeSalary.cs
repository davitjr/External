using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class EmployeeSalary
    {
        /// <summary>
        /// Հերթական համար
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Հաշվարկի ամսաթիվ
        /// </summary>
        public DateTime CalculationDate { get; set; }
        /// <summary>
        /// Վճարված գումար
        /// </summary>
        public double NetSalary { get; set; }
        /// <summary>
        /// Հաշվարկված գումար
        /// </summary>
        public double GrossSalary { get; set; }
        /// <summary>
        /// Հարկ
        /// </summary>
        public double IncomeTax { get; set; }

        public List<EmployeeSalary> GetEmployeeSalaryList(DateTime startDate, DateTime endDate, ulong customerNumber, Languages languageID)
        {
            return EmployeeSalaryDB.GetEmployeeSalaryList(startDate, endDate, customerNumber, languageID);
        }

    }
}
