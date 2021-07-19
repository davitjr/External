using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ExternalBanking.DBManager
{
    class DigitalAccountRestConfigurationsDB
    {
        public static ActionResult UpdateCustomerAccountRestConfig(List<DigitalAccountRestConfigurationItem> ConfigurationItems)
        {

            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    foreach (var Configuration in ConfigurationItems)
                    {
                        cmd.CommandText = @"SELECT * FROM [tbl_digital_account_rest_configuration] 
                                            WHERE digital_user_ID = @digital_user_ID 
                                                AND account_rest_type_ID = @account_rest_type_ID";

                        cmd.Parameters.Add("@digital_user_ID", SqlDbType.Float).Value = Configuration.DigitalUserId;
                        cmd.Parameters.Add("@account_rest_type_ID", SqlDbType.Int).Value = Configuration.AccountRestTypeId;

                        SqlDataReader dr = cmd.ExecuteReader();
                        cmd.Parameters.Clear();

                        if (dr.Read())
                        {
                            conn.Close();
                            conn.Open();
                            cmd.CommandText = @"UPDATE [tbl_digital_account_rest_configuration]
                                                    SET [configuration_type_ID] = @configuration_type_ID,
                                                        [account_rest_type_ID] = account_rest_type_ID,
                                                        [account_rest_attribute_value] = @account_rest_attribute_value,
                                                        [registration_date] = @registration_date
                                                WHERE digital_user_ID = @digital_user_ID AND account_rest_type_ID = @account_rest_type_ID";


                            cmd.Parameters.Add("@digital_user_ID", SqlDbType.Float).Value = Configuration.DigitalUserId;
                            cmd.Parameters.Add("@configuration_type_ID", SqlDbType.Int).Value = Configuration.ConfigurationTypeId;
                            cmd.Parameters.Add("@account_rest_type_ID", SqlDbType.Int).Value = Configuration.AccountRestTypeId;
                            cmd.Parameters.Add("@account_rest_attribute_value", SqlDbType.Int).Value = Configuration.AccountRestAttributeValue;
                            cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = Configuration.RegistrationDate;

                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        else
                        {
                            conn.Close();
                            conn.Open();

                            cmd.CommandText = @"INSERT INTO [tbl_digital_account_rest_configuration] 
                                                    ( [customer_number], [digital_user_ID], [configuration_type_ID], [account_rest_type_ID], [account_rest_attribute_value], [registration_date])
                                                VALUES(@customer_number, @digital_user_ID, @configuration_type_ID, @account_rest_type_ID, @account_rest_attribute_value, @registration_date)";

                            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = Configuration.CustomerNumber;
                            cmd.Parameters.Add("@digital_user_ID", SqlDbType.Int).Value = Configuration.DigitalUserId;
                            cmd.Parameters.Add("@configuration_type_ID", SqlDbType.Int).Value = Configuration.ConfigurationTypeId;
                            cmd.Parameters.Add("@account_rest_type_ID", SqlDbType.Int).Value = Configuration.AccountRestTypeId;
                            cmd.Parameters.Add("@account_rest_attribute_value", SqlDbType.Bit).Value = Configuration.AccountRestAttributeValue;
                            cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = Configuration.RegistrationDate;

                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }

                    result.ResultCode = ResultCode.Normal;
                }
            }

            return result;
        }

        public static ActionResult ResetCustomerAccountRestConfig(int DigitalUserId, ulong customerNumber)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"DELETE FROM [tbl_digital_account_rest_configuration] WHERE [digital_user_ID] = @digital_user_ID";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@digital_user_id", SqlDbType.Int).Value = DigitalUserId;

                    cmd.ExecuteNonQuery();

                    conn.Close();
                    conn.Open();
                    result = SaveDefaultAccountRestConfigurations(DigitalUserId, customerNumber);
                }
            }

            return result;
        }

        public static DigitalAccountRestConfigurations GetCustomerAccountRestConfig(int DigitalUserId, ulong customerNumber, int lang)
        {
            DigitalAccountRestConfigurations restConfigurations = new DigitalAccountRestConfigurations();
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT * FROM [tbl_digital_account_rest_configuration] c
                                        INNER JOIN [tbl_types_of_digital_account_rest] t ON c.[account_rest_type_ID] = t.ID
                                        INNER JOIN [tbl_types_of_digital_account_rest_configuration] tc ON tc.id = c.[configuration_type_ID]
										Inner JOIN [tbl_digital_account_rest_configuration_template] CT ON  t.ID = CT.account_rest_type_ID
                                        WHERE digital_user_id = @digital_user_id and ct.template_ID = 1"; 
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@digital_user_id", SqlDbType.Int).Value = DigitalUserId;


                    SqlDataReader dr = cmd.ExecuteReader();
                    cmd.Parameters.Clear();

                    while (dr.Read())
                    {
                        DigitalAccountRestConfigurationItem configurationsSelectedItem = new DigitalAccountRestConfigurationItem();

                        configurationsSelectedItem.Id = Convert.ToInt32(dr["id"]);
                        configurationsSelectedItem.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                        configurationsSelectedItem.DigitalUserId = Convert.ToInt32(dr["digital_user_id"]);
                        configurationsSelectedItem.ConfigurationTypeId = Convert.ToInt32(dr["configuration_type_id"]);
                        configurationsSelectedItem.AccountRestAttributeValue = Convert.ToBoolean(dr["account_rest_attribute_value"]);
                        configurationsSelectedItem.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                        configurationsSelectedItem.AccountRestTypeDescription = lang == 1 ? dr["description_arm"].ToString() : dr["description_eng"].ToString();
                        configurationsSelectedItem.AccountRestTypeId = Convert.ToInt32(dr["account_rest_type_ID"].ToString());

                        restConfigurations.Configurations.Add(configurationsSelectedItem);
                    }

                    if (restConfigurations.Configurations.Count == 0)
                    {
                        conn.Close();
                        conn.Open();
                        if(!HasConfigInHistory(DigitalUserId))
                            result = SaveDefaultAccountRestConfigurations(DigitalUserId, customerNumber);
                    }

                    if (result.ResultCode == ResultCode.Normal)
                       return GetCustomerAccountRestConfig(DigitalUserId, customerNumber, lang);


                    if (restConfigurations.Configurations.Exists(x => x.ConfigurationTypeId == 2))
                    {
                        restConfigurations.ConfigurationType = DigitalAccountRestConfigurationType.Custom;
                        restConfigurations.ConfigurationTypeDescription = "Custom";
                    }
                    else
                    {
                        restConfigurations.ConfigurationType = DigitalAccountRestConfigurationType.Defalut;
                        restConfigurations.ConfigurationTypeDescription = "Defalut";
                    }

                }
            }

            return restConfigurations;
        }


        public static ActionResult SaveDefaultAccountRestConfigurations(int DigitalUserId, ulong customerNumber)
        {
            ActionResult result = new ActionResult();

            DataTable configs = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand selectCommand = new SqlCommand())
                {
                    selectCommand.CommandType = CommandType.Text;
                    selectCommand.Connection = conn;
                    selectCommand.CommandText = @"SELECT account_rest_type_ID
                                                    FROM tbl_digital_account_rest_configuration_template T
                                                    INNER JOIN  tbl_types_of_digital_account_rest C
                                                    ON C.ID = T.account_rest_type_ID
                                                    WHERE template_ID = 1";
                    conn.Open();
                    SqlDataReader dr = selectCommand.ExecuteReader();
                    if (dr.HasRows)
                    {
                        configs.Load(dr);
                    }
                    conn.Close();
                }

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    for (int i = 0; i < configs.Rows.Count; i++)
                    {
                        cmd.CommandText = @"INSERT INTO [tbl_digital_account_rest_configuration] ( [customer_number], [digital_user_ID], [configuration_type_ID], [account_rest_type_ID], [account_rest_attribute_value], [registration_date])
                                                                                             VALUES(@customer_number, @digital_user_ID, @configuration_type_ID, @account_rest_type_ID, @account_rest_attribute_value, @registration_date)";

                        cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                        cmd.Parameters.Add("@digital_user_ID", SqlDbType.Int).Value = DigitalUserId;
                        cmd.Parameters.Add("@configuration_type_ID", SqlDbType.Int).Value = DigitalAccountRestConfigurationType.Defalut;
                        cmd.Parameters.Add("@account_rest_type_ID", SqlDbType.Int).Value = Convert.ToInt32(configs.Rows[i]["account_rest_type_ID"].ToString());
                        cmd.Parameters.Add("@account_rest_attribute_value", SqlDbType.Bit).Value = true;
                        cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = DateTime.Now;

                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    result.ResultCode = ResultCode.Normal;
                }
            }

            return result;
        }

        private static bool HasConfigInHistory(int digitalUserId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = @"SELECT top 1 digital_user_id FROM [tbl_digital_account_rest_configuration_History] 
                                        WHERE digital_user_id = @digital_User_ID";

                    cmd.Parameters.Add("@digital_user_ID", SqlDbType.Int).Value = digitalUserId;
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
