using ExternalBanking.PreferredAccounts;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.PreferredAccounts
{
    public class PreferredAccountOrderDB
    {
        internal static ActionResult SavePreferredAccountOrder(PreferredAccountOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_preferred_account_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@service_type", SqlDbType.SmallInt).Value = order.PreferredAccountServiceType;
                    cmd.Parameters.Add("@account_number", SqlDbType.NVarChar, 20).Value = order.AccountNumber;
                    cmd.Parameters.Add("@is_active", SqlDbType.Bit).Value = order.IsActive;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = order.Source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                }
            }
            return result;
        }

        internal static ActionResult ApprovePreferredAccountOrder(long id)
        {
            ActionResult result = new ActionResult
            {
                ResultCode = ResultCode.Normal
            };
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_confirm_preferred_account_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = id;

                    cmd.ExecuteNonQuery();

                }
            }
            return result;
        }

        internal static PreferredAccountOrder Get(PreferredAccountOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                                        SELECT D.doc_id,                                       
                                               D.confirmation_date                                        
                                        FROM   tbl_hb_documents D                                           
                                        WHERE  D.doc_id = @id 
                                               AND customer_number = @customerNumber";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());
                            order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                        }
                    }
                }
            }
            return order;
        }
    }
}
