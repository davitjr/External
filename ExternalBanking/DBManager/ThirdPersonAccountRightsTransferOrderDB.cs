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
    public class ThirdPersonAccountRightsTransferOrderDB
    {
      
        internal static bool CheckRightsWereTransferred(string accountNumber)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT third_person_Customer_number, arm_number, closing_date FROM tbl_co_accounts_main m 
                                INNER JOIN tbl_co_accounts a ON m.id = a.co_main_id WHERE arm_number = @arm_number";
                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@arm_number", SqlDbType.Float).Value = accountNumber;
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            result = reader["closing_date"] != DBNull.Value ? true : false;
                        }
                    }
                }
            }

            return result;
        }

        internal static bool CheckAccountHasArrest(ulong customerNumber)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"SELECT dbo.Fnc_Get_customer_acc_argelanq(@customer_number) AS arrest_result";
                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            result = Convert.ToInt32(reader["arrest_result"]) == 0 ? false : true;
                        }
                    }
                }
            }


            return result;
        }

        internal static bool CheckThirdPersonIsCustomer(ulong customerNumber)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string script = @"Select quality from tbl_customers where customer_number = @customer_number";
                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            result = Convert.ToInt32(reader["quality"]) != 1 ? false : true;
                        }
                    }
                }
            }

            return result;
        }

        /// Իրավունքի փոխանցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondClosing(ulong customerNumber, string accountNumber, SourceType sourceType)
        {
            bool secondClosing;
            string sourceTypeCond = "";
            if (sourceType == SourceType.Bank)
            {
                sourceTypeCond = " and source_type not in (1,5)";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=29 and document_subtype=1 and
                                                debet_account=@accountNumber and customer_number=@customerNumber" + sourceTypeCond, conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        secondClosing = true;
                    }
                    else
                        secondClosing = false;
                }

            }
            return secondClosing;
        }

        /// <summary>
        /// Երրորդ անձի իրավունքի փոխանցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult RightsTransferOrder(ThirdPersonAccountRightsTransferOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_transfer_rights_to_third_person";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.JointAccount.Currency;
                    cmd.Parameters.Add("@joint_account", SqlDbType.VarChar, 50).Value = order.JointAccount.AccountNumber;
                    cmd.Parameters.Add("@third_person_customer_number", SqlDbType.Float).Value = order.ThirdPersonCustomerNumber;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;

                    return result;
                }
            }

        }
    }
}
