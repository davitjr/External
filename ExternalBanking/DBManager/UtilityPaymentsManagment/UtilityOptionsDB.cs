using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class UtilityOptionsDB
    {
        public static List<UtilityOptions> GetUtiltyForChange()
        {

            List<UtilityOptions> utilityOptions = new List<UtilityOptions>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                string sqlString = @"SELECT us.id,  description_arm,is_active,is_enabled,sc.id as config_id  from tbl_type_of_utility_services  US
                                       INNER JOIN 
                                       tbl_utility_service_configurations SC
                                       ON US.id = sc.utility_type_id 
                                       WHERE is_active = 1 and closing_date is null ";

                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    UtilityOptions utilityOption = new UtilityOptions();
                    utilityOption.TypeID = Convert.ToInt32((dr["id"]).ToString());
                    utilityOption.Description = Utility.ConvertAnsiToUnicode(dr["description_arm"].ToString());
                    utilityOption.IsEnabled = Convert.ToBoolean(dr["is_enabled"].ToString());
                    utilityOption.ConfigId = Convert.ToInt32(dr["config_id"].ToString());
                    utilityOptions.Add(utilityOption);
                }
            }

            return utilityOptions;
        }



        public static void SaveUtilityConfigurationsAndHistory(UtilityOptions utilityOption)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("pr_save_utility_config_and_history", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Is_enabled", SqlDbType.Bit).Value = utilityOption.IsEnabled;
                cmd.Parameters.Add("@Type_id", SqlDbType.Int).Value = utilityOption.TypeID;
                cmd.Parameters.Add("@Set_number", SqlDbType.Int).Value = utilityOption.NumberOfSet;
                cmd.Parameters.Add("@Config_id", SqlDbType.Int).Value = utilityOption.ConfigId;

                cmd.ExecuteNonQuery();
            }

        }


        internal static ActionResult SaveAllUtilityConfigurationsAndHistory(List<UtilityOptions> utilityOptions, int a)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "update tbl_utility_service_configurations set is_enabled =@is_enabled";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@is_enabled", SqlDbType.Int).Value = a;
                    cmd.ExecuteNonQuery();
                }

            }

            utilityOptions.ForEach(x =>
            {

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = @"INSERT INTO tbl_utility_service_configurations_history 
                                        	VALUES (@Set_number,@Config_id,@utility_type_id,getdate(),@Is_enabled)";

                        cmd.Parameters.Add("@Is_enabled", SqlDbType.Bit).Value = a;
                        cmd.Parameters.Add("@utility_type_id", SqlDbType.Int).Value = x.TypeID;
                        cmd.Parameters.Add("@Set_number", SqlDbType.Int).Value = x.NumberOfSet;
                        cmd.Parameters.Add("@Config_id", SqlDbType.Int).Value = x.ConfigId;

                        cmd.ExecuteNonQuery();

                    }

                }
            });

            result.ResultCode = ResultCode.Normal;
            return result;


        }



        internal static List<string> GetExistsNotSentAndSettledRows(Dictionary<int, bool> keyValues)
        {
            List<string> list = new List<string>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["PaymentsConn"].ToString()))
            {
                string serviceName = "";
                string sqlString = @"SELECT distinct s.description as ServiceName, u.Utility_type_id from tbl_utility_payments_main U 
                                    INNER JOIN tbl_utility_service_configurations C on U.Utility_type_id = C.utility_type_Id 
                                    INNER JOIN tbl_type_of_utility_services S on S.id = C.utility_type_Id 
                                    WHERE s.is_active = 1 and Transaction_group_number  is not null and Deleted<> 1 and Status = 0  and c.is_enabled <> 0";

                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    if (keyValues.ContainsKey(Convert.ToInt32(dr["Utility_type_id"])))
                    {
                        if (keyValues[Convert.ToInt32(dr["Utility_type_id"])] == false)
                        {
                            serviceName = (dr["ServiceName"]).ToString();
                            list.Add(serviceName);
                        }
                    }
                }
            }

            return list;

        }

    }
}


