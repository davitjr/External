using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class EmployeeSalaryDetailsDB
    {
        public static EmployeeSalaryDetails GetEmployeeSalaryDetails(int ID, ulong customerNumber, Languages langID)
        {
            EmployeeSalaryDetails employeeSalaryDetails = new EmployeeSalaryDetails();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SalaryBaseConn"].ToString()))
            {
                string sql = @"One_Calculationts_details_For_HB";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@row_Id", SqlDbType.BigInt).Value = ID;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@Language_ID", SqlDbType.Int).Value = langID == Languages.hy ? 0 : 1;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            employeeSalaryDetails.ID = ID;
                            employeeSalaryDetails.Description = dr["description"].ToString();
                            employeeSalaryDetails.Month = int.Parse(dr["Month"].ToString());
                            employeeSalaryDetails.Year = int.Parse(dr["Year"].ToString());
                            employeeSalaryDetails.MonthWorkingDays = int.Parse(dr["MonthWDays"].ToString());
                            employeeSalaryDetails.WorkedDays = int.Parse(dr["WD"].ToString());
                            employeeSalaryDetails.Salary = double.Parse(dr["Money"].ToString()) + double.Parse(dr["add_money_for_accountant"].ToString());
                            employeeSalaryDetails.Bonus = double.Parse(dr["Bonus"].ToString());
                            employeeSalaryDetails.AdditionalDaysSalary = double.Parse(dr["Over_money"].ToString());
                            employeeSalaryDetails.IncomeTax = double.Parse(dr["Tax1"].ToString());  //Եկամտային հարկ
                            employeeSalaryDetails.PensionFundPayment = double.Parse(dr["Tax2"].ToString());
                            employeeSalaryDetails.StampPayment = double.Parse(dr["Tax3"].ToString());
                            employeeSalaryDetails.CalculatedAmount = double.Parse(dr["TotalAmount"].ToString());
                            employeeSalaryDetails.TotalTaxes = double.Parse(dr["Tax1"].ToString()) + double.Parse(dr["Tax2"].ToString()) + double.Parse(dr["Tax3"].ToString());
                            employeeSalaryDetails.NetSalary = double.Parse(dr["Clearmoney"].ToString());
                            employeeSalaryDetails.VacationFisrtsDay = dr["Holiday_beginning"] != DBNull.Value ? Convert.ToDateTime(dr["Holiday_beginning"]) : (DateTime?)null;
                            employeeSalaryDetails.VacationLastDay = dr["Holiday_end"] != DBNull.Value ? Convert.ToDateTime(dr["Holiday_end"]) : (DateTime?)null;
                            employeeSalaryDetails.VacationDays = dr["HolidayDays"] != DBNull.Value ? Convert.ToInt16(dr["HolidayDays"]) : default;
                            employeeSalaryDetails.VacationAmount = dr["HolidayAmount"] != DBNull.Value ? double.Parse(dr["HolidayAmount"].ToString()) : default;
                        }
                    }
                }
            }
            return employeeSalaryDetails;
        }
    }
}
