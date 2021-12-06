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
    internal class OtherInsuranceOrderDB
    {

        internal static Dictionary<string, string> GetInsuranceContractTypes(bool isLoanProduct, bool isSeparatelyProduct, bool isProvision)
        {
            Dictionary<string, string> contrtype = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT ID,[dbo].[fnc_convertAnsiToUnicode]([Description]) as [Description] FROM [dbo].[Tbl_type_of_insurance_contracts]";

                if (isLoanProduct)
                {
                    cmd.CommandText += " WHERE ID = 1";
                }
                else if (isProvision)
                {
                    cmd.CommandText += " WHERE ID = 2";
                }
                else
                {
                    cmd.CommandText += " WHERE ID in (2,3)";
                }

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        contrtype.Add(Convert.ToString(reader["ID"]), Convert.ToString(reader["Description"]));
                    }
                }
            }

            return contrtype;

        }

        internal static Dictionary<string, string> GetInsuranceTypesByContractType(int insuranceContractType, bool isLoanProduct, bool isSeparatelyProduct, bool isProvision)
        {
            Dictionary<string, string> instype = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqlstring = "pr_get_insurance_types_by_contract_type";
                using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = insuranceContractType;


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (isSeparatelyProduct && !isProvision && Convert.ToInt32(dr["insurance_type"]) != 2)
                            {
                                instype.Add(Convert.ToString(dr["insurance_type"]), Utility.ConvertAnsiToUnicode(Convert.ToString(dr["description"])));
                            }

                            else if (isLoanProduct)
                            {
                                instype.Add(Convert.ToString(dr["insurance_type"]), Utility.ConvertAnsiToUnicode(Convert.ToString(dr["description"])));
                            }
                            else if (isProvision && Convert.ToInt32(dr["insurance_type"]) != 1)
                            {
                                instype.Add(Convert.ToString(dr["insurance_type"]), Utility.ConvertAnsiToUnicode(Convert.ToString(dr["description"])));
                            }
                        }
                    }
                }

                conn.Close();
            }

            return instype;

        }

        internal static void DeleteInsurance(long insuranceId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqlstring = "Update tbl_insurance_contracts set quality = 40 where App_Id = @insuranceId";
                using (SqlCommand cmd = new SqlCommand(sqlstring, conn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add("@insuranceId", SqlDbType.BigInt).Value = insuranceId;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        internal static bool HasPermissionForDelete(short setNumber)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqlstring = "select customer_number from v_cashers_list where (group_id = 11 or (group_id = 74 and position_id = 6)) and new_id = " + setNumber.ToString();

                using SqlCommand cmd = new SqlCommand(sqlstring, conn);
                cmd.CommandType = System.Data.CommandType.Text;

                using SqlDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    result = true;
                }


            }

            return result;
        }


    }
}
