using ExternalBanking.DBManager;
using System;

namespace ExternalBanking
{
    public class EmployeeSalaryDetails
    {
        /// <summary>
        /// Հերթական ունիկալ համար
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Հաշվարկի նկարագրություն
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Ամիս
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// Տարի
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Ամսվա աշխատանքային օրեր
        /// </summary>
        public int MonthWorkingDays { get; set; }
        /// <summary>
        /// Աշխատած օրեր
        /// </summary>
        public int WorkedDays { get; set; }
        /// <summary>
        /// Աշխատավարձ
        /// </summary>
        public double Salary { get; set; }
        /// <summary>
        /// Պարգևատրում
        /// </summary>
        public double Bonus { get; set; }
        /// <summary>
        /// Լրացուցիչ օրերի աշխատավարձ
        /// </summary>
        public double AdditionalDaysSalary { get; set; }
        /// <summary>
        /// Եկամտային հարկ
        /// </summary>
        public double IncomeTax { get; set; }
        /// <summary>
        /// Կենսաթոշակային ֆոնդի վճար
        /// </summary>
        public double PensionFundPayment { get; set; }
        /// <summary>
        /// Դրոշմանիշային վճար
        /// </summary>
        public double StampPayment { get; set; }
        /// <summary>
        /// Հաշվարկված գումար
        /// </summary>
        public double CalculatedAmount { get; set; }
        /// <summary>
        /// Ընդհանուր հարկեր
        /// </summary>
        public double TotalTaxes { get; set; }
        /// <summary>
        /// Առձեռն վճարված գումար
        /// </summary>
        public double NetSalary { get; set; }

        /// <summary>
        /// Արձակուրդի սկիզբ
        /// </summary>
        public DateTime? VacationFisrtsDay { get; set; }

        /// <summary>
        /// Արձակուրդի վերջ
        /// </summary>
        public DateTime? VacationLastDay { get; set; }

        /// <summary>
        /// Արձակուրդի օրերի քանակ
        /// </summary>
        public short VacationDays { get; set; }

        /// <summary>
        /// Արձակուրդային գումար
        /// </summary>
        public double VacationAmount { get; set; }


        public EmployeeSalaryDetails GetEmployeeSalaryDetails(int ID, ulong customerNumber, Languages langID)
        {
            return EmployeeSalaryDetailsDB.GetEmployeeSalaryDetails(ID, customerNumber, langID);
        }

    }
}
