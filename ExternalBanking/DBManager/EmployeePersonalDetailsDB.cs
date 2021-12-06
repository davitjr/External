using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    public class EmployeePersonalDetailsDB
    {
        public static EmployeePersonalDetails GetEmployeePersonalDetails(ulong customerNumber)
        {
            EmployeePersonalDetails employeePersonalDetails = new EmployeePersonalDetails();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT  CASE WHEN filial_code <> 22000 THEN  Name_Fill 
                                        ELSE d.Description END Description, c.Position, DATEDIFF(YEAR,  V.date_order_beginning, getDate()) as Years
                                        FROM V_employee_list V
                                        INNER JOIN [v_public_full_cashers_list] c on c.emp_number = V.employee_number
                                        INNER JOIN v_department_list d on c.Id_Department =d.id
                                        LEFT JOIN Name_Branches f on c.filial_code=f.ID_Fill 
                                        WHERE V.customer_number = @customerNumber";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;


                    using SqlDataReader rd = cmd.ExecuteReader();

                    if (rd.Read())
                    {
                        employeePersonalDetails.DepDescription = Utility.ConvertAnsiToUnicode(rd["Description"].ToString());
                        employeePersonalDetails.Position = Utility.ConvertAnsiToUnicode(rd["Position"].ToString());
                        employeePersonalDetails.WorkingYears = int.Parse(rd["Years"].ToString());
                    }
                }
            }

            return employeePersonalDetails;
        }
    }
}
