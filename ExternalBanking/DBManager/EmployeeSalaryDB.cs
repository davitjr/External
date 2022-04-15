using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class EmployeeSalaryDB
    {
        public static List<EmployeeSalary> GetEmployeeSalaryList(DateTime startDate, DateTime endDate, ulong customerNumber, Languages languageID)
        {
            List<EmployeeSalary> EmployeeSalaryList = new List<EmployeeSalary>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SalaryBaseConn"].ToString()))
            {
                string sql = @"List_of_Calculationts_For_HB";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    DataTable dt = new DataTable();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = startDate;
                    cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = endDate;
                    cmd.Parameters.Add("@Language_ID", SqlDbType.Int).Value = languageID == Languages.hy ? 0 : 1;

                    conn.Open();

                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            EmployeeSalary employeeSalary = new EmployeeSalary();
                            employeeSalary.ID = int.Parse(row["Id"].ToString());
                            employeeSalary.CalculationDate = DateTime.Parse(row["date_of_calculation"].ToString());
                            employeeSalary.GrossSalary = double.Parse(row["AllMoney"].ToString());
                            employeeSalary.NetSalary = double.Parse(row["Clearmoney"].ToString());
                            employeeSalary.IncomeTax = double.Parse(row["Tax"].ToString());

                            EmployeeSalaryList.Add(employeeSalary);
                        }
                    }
                }
            }
            return EmployeeSalaryList;
        }
    }
}
