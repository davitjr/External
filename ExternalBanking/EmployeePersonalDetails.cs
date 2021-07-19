using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class EmployeePersonalDetails
    {
        /// <summary>
        /// Բաժնի/ մասնաճյուղի անվանումը
        /// </summary>
        public string DepDescription { get; set; }
        /// <summary>
        /// Պաշտոն
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// Աշխատելու տարիների քանակ
        /// </summary>
        public int WorkingYears { get; set; }

        public EmployeePersonalDetails GetEmployeePersonalDetails(ulong customerNumber)
        {
            return EmployeePersonalDetailsDB.GetEmployeePersonalDetails(customerNumber);
        }
    }
}
