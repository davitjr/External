using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    public static class DeleteLoanOrderDB
    {
        public static ActionResult LoanDeleteOrder(DeleteLoanOrder order, short userId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_loan_delete_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = order.AppId;
                    cmd.Parameters.Add("@source_type", SqlDbType.SmallInt).Value = order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@delete_reason_type", SqlDbType.TinyInt).Value = order.DeleteReasonType;
                    cmd.Parameters.Add("@confirmation_set_number", SqlDbType.SmallInt).Value = userId;

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
                }
            }
            return result;
        }

        public static ulong GetCustomerNumber(ulong appId)
        {
            ulong result = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT customer_number FROM [Tbl_short_time_loans;] WHERE app_id = @appId", conn))
                {
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = appId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = (ulong)(dr["result"]);
                        }
                    }
                }
                return result;
            }
        }

        public static DeleteLoanOrderDetails GetLoanDeleteOrderDetails(uint orderId)
        {
            DataTable dt = new DataTable();
            DeleteLoanOrderDetails order = new DeleteLoanOrderDetails();
            
            string sql = @"SELECT H.doc_ID, registration_date, operation_date, D.general_number, 
	                            dbo.fnc_convertAnsiToUnicode(T.description) delete_reason,
	                            dbo.fnc_convertAnsiToUnicode(Q.description_arm) quality_description,
                                H.quality
                            FROM Tbl_HB_documents H
	                            INNER JOIN tbl_delete_loan_order_datails D ON H.doc_ID = D.doc_ID
	                            INNER JOIN tbl_types_of_loan_delete T ON D.delete_reason_type = T.ID
	                            INNER JOIN Tbl_types_of_HB_quality Q ON H.quality = Q.quality
                            WHERE H.doc_ID = @DocID";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = orderId;
                    
                    dt.Load(cmd.ExecuteReader());

                    order.OrderId = dt.Rows[0]["doc_ID"] != DBNull.Value ? Convert.ToUInt32(dt.Rows[0]["doc_ID"]) : 0;
                    order.RegistrationDate = dt.Rows[0]["registration_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["registration_date"]) : Convert.ToDateTime("01/JAN/1990");
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : Convert.ToDateTime("01/JAN/1990");
                    order.GeneralNumber = dt.Rows[0]["general_number"].ToString();
                    order.DeleteReason = dt.Rows[0]["delete_reason"].ToString();
                    order.DeleteReason = dt.Rows[0]["delete_reason"].ToString();
                    order.Quality = Convert.ToByte(dt.Rows[0]["quality"]);
                    order.QualityDescription = dt.Rows[0]["quality_description"].ToString();                    
                }
            }
            return order;
        }
    }
}
