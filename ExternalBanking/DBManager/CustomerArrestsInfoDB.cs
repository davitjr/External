using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Configuration;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.DBManager
{
    class CustomerArrestsInfoDB
    {
        internal static string GetArrestTypesList()
        {
            List<ArrestTypes> arrestsList = new List<ArrestTypes>();

            //Dictionary<string, string> arrestsList = new Dictionary<string, string>();

            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                _con.Open();

                string sql = "SELECT Id,[dbo].[fnc_convertAnsiToUnicode](description) FROM [tbl_types_of_arrests]";

                using (SqlCommand cmd = new SqlCommand(sql, _con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                ArrestTypes arrest = new ArrestTypes();
                                arrest.ID = Convert.ToInt32(dr[0]);
                                arrest.Description = Convert.ToString(dr[1]);

                                arrestsList.Add(arrest);

                            }
                        }
                    }
                }
                _con.Close();
            }
            string json = new JavaScriptSerializer().Serialize(arrestsList);
            return json;
        }


        internal static string GetArrestsReasonTypesList()
        {
            List<ArrestsReasonTypes> reasonsList = new List<ArrestsReasonTypes>();

            //Dictionary<string, string> reasonsList = new Dictionary<string, string>();

            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                _con.Open();

                string sql = "SELECT id,[dbo].[fnc_convertAnsiToUnicode](description) FROM [Tbl_type_of_customer_arrest_reason]";

                using (SqlCommand cmd = new SqlCommand(sql, _con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                ArrestsReasonTypes reason = new ArrestsReasonTypes();
                                reason.ID = Convert.ToInt32(dr[0]);
                                reason.Description = Convert.ToString(dr[1]);

                                reasonsList.Add(reason);

                            }
                        }
                    }
                }
                _con.Close();
            }

            string json = new JavaScriptSerializer().Serialize(reasonsList);
            return json;
        }

        internal static string PostNewAddedCustomerArrestInfo(CustomerArrestInfo obj)
        {
            string json = "";

            if (obj != null)
            {

                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    _con.Open();

                    string prName = "pr_Insert_Customer_Arrests_Info";

                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(prName, _con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@region", SqlDbType.TinyInt).Value = obj.Region;
                            cmd.Parameters.Add("@branch", SqlDbType.TinyInt).Value = obj.Branch;
                            cmd.Parameters.Add("@village", SqlDbType.TinyInt).Value = obj.Village;
                            cmd.Parameters.Add("@number", SqlDbType.SmallInt).Value = obj.Number;
                            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = obj.CustomerNumber;
                            cmd.Parameters.Add("@info", SqlDbType.NVarChar).Value = obj.Description;
                            cmd.Parameters.Add("@type_Id", SqlDbType.TinyInt).Value = obj.TypeID;
                            cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = obj.SetNumber;
                            cmd.Parameters.Add("@arrest_reason_Id", SqlDbType.SmallInt).Value = obj.ArrestReasonID;

                            cmd.ExecuteNonQuery();
                        }
                        _con.Close();
                    }
                    catch (Exception ex)
                    {
                        json = ex.Message;
                        _con.Close();
                    }
                }

            }

            return json;
        }

        internal static string RemoveCustomerArrestInfo(CustomerArrestInfo obj)
        {
            string json = "";
            if (obj != null)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    _con.Open();

                    string prName = "pr_Remove_Customer_Arrests_Info";

                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(prName, _con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = obj.ID;
                            cmd.Parameters.Add("@region", SqlDbType.TinyInt).Value = obj.Region;
                            cmd.Parameters.Add("@branch", SqlDbType.TinyInt).Value = obj.Branch;
                            cmd.Parameters.Add("@village", SqlDbType.TinyInt).Value = obj.Village;
                            cmd.Parameters.Add("@number", SqlDbType.SmallInt).Value = obj.Number;
                            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = obj.CustomerNumber;
                            cmd.Parameters.Add("@info", SqlDbType.NVarChar).Value = obj.Description;
                            cmd.Parameters.Add("@type_Id", SqlDbType.TinyInt).Value = obj.TypeID;
                            cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = obj.SetNumber;
                            cmd.Parameters.Add("@arrest_reason_Id", SqlDbType.SmallInt).Value = obj.ArrestReasonID;

                            cmd.ExecuteNonQuery();
                        }
                        _con.Close();
                    }
                    catch (Exception ex)
                    {
                        json = ex.Message;
                    }

                }

            }

            return json;
        }

        internal static short GetSetNumberInfo(UserInfoForArrests obj)
        {
            short groupId = 0;
            using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                _con.Open();

                string sql = "SELECT group_id FROM [dbo].[v_cashers_list] WHERE  new_id = @id";

                using (SqlCommand cmd = new SqlCommand(sql, _con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = obj.SetNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                groupId = Convert.ToInt16(dr["group_id"]);
                            }
                        }
                    }
                }
                _con.Close();
            }


            return groupId;
        }


        internal static string GetCustomerArrestsInfo(ulong customerNumber)
        {
            string json = "";
            List<CustomerArrestInfo> customerArrests = new List<CustomerArrestInfo>();
            List<ArrestTypes> arrestsTypes = JsonConvert.DeserializeObject<List<ArrestTypes>>(GetArrestTypesList());
            List<ArrestsReasonTypes> arrestsReasonTypes = JsonConvert.DeserializeObject<List<ArrestsReasonTypes>>(GetArrestsReasonTypesList());

            if (customerNumber != 0)
            {
                try
                {
                    using SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString());
                    _con.Open();

                    string query = "select ID,region,branch,village,number,case when (SELECT dbo.IsTextUnicode(Info)) = 1 then Info else [dbo].[fnc_convertAnsiToUnicode](Info) end as Info," +
                        "customer_number,[Type],SetDate,Set_Number,arrest_reason from tbl_Info Where customer_number = @customerNumber " +
                        "Order By Isnull(SetDate,'01/jan/1900'),ID";

                    using (SqlCommand cmd = new SqlCommand(query, _con))
                    {
                        cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                        SqlDataReader reader = cmd.ExecuteReader();


                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                CustomerArrestInfo ca = new CustomerArrestInfo();
                                ca.ID = Convert.ToInt32(reader["ID"]);
                                ca.Region = Convert.ToInt32(reader["region"]);
                                ca.Branch = Convert.ToInt32(reader["branch"]);
                                ca.Village = Convert.ToInt32(reader["village"]);
                                ca.Number = Convert.ToInt32(reader["number"]);
                                ca.CustomerNumber = Convert.ToInt64(reader["customer_number"]);
                                ca.Description = Convert.ToString(reader["Info"]);
                                if (Convert.ToString(reader["SetDate"]) != "")
                                {
                                    ca.RegistrationDate = Convert.ToDateTime(reader["SetDate"]).ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    ca.RegistrationDate = "";
                                }

                                if (Convert.ToString(reader["Set_Number"]) != "")
                                {
                                    ca.SetNumber = Convert.ToInt32(reader["Set_Number"]);
                                }
                                if (Convert.ToString(reader["arrest_reason"]) != "")
                                {
                                    ca.ArrestReasonID = Convert.ToInt32(reader["arrest_reason"]);

                                    //List<string> keyList = new List<string>(arrestsReasonTypes.Keys);
                                    //List<string> valueList = new List<string>(arrestsReasonTypes.Values);


                                    for (int i = 0; i < arrestsReasonTypes.Count; i++)
                                    {
                                        if (arrestsReasonTypes[i].ID == ca.ArrestReasonID)
                                        {
                                            ca.ArrestReasonDescription = arrestsReasonTypes[i].Description;
                                        }
                                    }


                                }
                                else
                                {
                                    ca.ArrestReasonDescription = "";
                                }


                                if (Convert.ToString(reader["Type"]) != "")
                                {

                                    ca.TypeID = Convert.ToInt32(reader["Type"]);

                                    //List<string> keyList = new List<string>(arrestsTypes.Keys);
                                    //List<string> valueList = new List<string>(arrestsTypes.Values);

                                    for (int i = 0; i < arrestsTypes.Count; i++)
                                    {
                                        if (arrestsTypes[i].ID == ca.TypeID)
                                        {
                                            ca.TypeDescription = arrestsTypes[i].Description;
                                        }
                                    }

                                }
                                else
                                {
                                    ca.TypeDescription = "";
                                }

                                if (ca.SetNumber != 0)
                                {

                                    string sql = @"SELECT * FROM [dbo].[v_cashers_list] WHERE new_id = @id";

                                    using SqlCommand cmd1 = new SqlCommand(sql, _con);
                                    cmd1.CommandType = CommandType.Text;
                                    cmd1.Parameters.Add("@id", SqlDbType.Int).Value = ca.SetNumber;
                                    using SqlDataReader dr = cmd1.ExecuteReader();
                                    if (dr.HasRows)
                                    {
                                        while (dr.Read())
                                        {
                                            ca.SetPerson = Convert.ToString(dr["First_Name"]) + ' ' + Convert.ToString(dr["Last_Name"]);
                                        }
                                    }
                                }

                                customerArrests.Add(ca);
                            }
                        }

                        reader.Close();

                        cmd.CommandText = "SELECT arrest FROM tbl_customers WHERE customer_number= @customerNumber";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;

                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Convert.ToInt32(reader["arrest"]) == 1 || Convert.ToInt32(reader["arrest"]) == 2)
                                {
                                    customerArrests[0].HasArrests = true;
                                }
                            }
                        }



                    }
                    _con.Close();
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }
            }
            json = new JavaScriptSerializer().Serialize(customerArrests);
            return json;
        }

        internal static CheckCustomerArrests GetCustomerHasArrests(ulong customerNumber)
        {
            CheckCustomerArrests customer = new CheckCustomerArrests();

            if (customerNumber != 0)
            {
                using (SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    _con.Open();

                    string query = "SELECT arrest FROM tbl_customers WHERE customer_number= @customerNumber";

                    using (SqlCommand cmd = new SqlCommand(query, _con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (int.Parse(reader["arrest"].ToString()) == 2 || int.Parse(reader["arrest"].ToString()) == 1)
                                {
                                    customer.HasArrests = true;
                                }
                                else
                                {
                                    customer.HasArrests = false;
                                }
                            }
                        }

                        reader.Close();

                        query = "select * from Tbl_Info where Type = 3 and customer_number = @customerNumber";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;

                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            customer.HasInfo = true;
                        }
                    }
                    _con.Close();

                }

            }

            return customer;
        }
    }
}
