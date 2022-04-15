using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class UtilityOptionsFilterDB
    {
        internal static List<UtilityOptions> SearchUtilityOptions(UtilityOptionsFilter searchParams)
        {
            List<UtilityOptions> utilityOptions = new List<UtilityOptions>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                string sql = "";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    sql = @" SELECT  * from tbl_utility_service_configurations_history SH
                             INNER JOIN  tbl_type_of_utility_services TU 
                             ON SH.utility_type_id = TU.ID";


                    if (searchParams.NumberOfSet != 0)
                    {
                        sql = sql + " and SH.set_number  = @setNumber";
                        cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = searchParams.NumberOfSet;
                    }

                    if (searchParams.StartDate != default(DateTime))
                    {
                        sql = sql + " and CONVERT(NVARCHAR(10),registration_date, 10) >=  @StartDate ";
                        cmd.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = searchParams.StartDate.Date;
                    }

                    if (searchParams.EndDate != default(DateTime))
                    {
                        sql = sql + " and CONVERT(NVARCHAR(10),registration_date, 10) <= @EndDate ";
                        cmd.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = searchParams.EndDate.Date;
                    }


                    if (searchParams.CommunalOptions != CommunalTypes.None)
                    {
                        sql = sql + " and utility_type_id = @UtilityType";
                        cmd.Parameters.Add("@UtilityType", SqlDbType.Int).Value = searchParams.CommunalOptions;
                    }

                    sql = sql + " ORDER BY SH.id desc";

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            utilityOptions = new List<UtilityOptions>();
                        }
                        while (dr.Read())
                        {
                            UtilityOptions utilityOption = new UtilityOptions();
                            utilityOption.Type = (CommunalTypes)Convert.ToInt32(dr["utility_type_id"].ToString());
                            utilityOption.Description = Utility.ConvertAnsiToUnicode(dr["description_arm"].ToString());
                            utilityOption.NumberOfSet = Convert.ToInt32(dr["set_number"].ToString());
                            utilityOption.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                            utilityOption.IsEnabled = Convert.ToBoolean(dr["is_enabled"].ToString());
                            utilityOptions.Add(utilityOption);
                        }
                    }
                }

                return utilityOptions;
            }
        }

    }
}
