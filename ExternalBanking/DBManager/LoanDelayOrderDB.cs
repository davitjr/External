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
    static class LoanDelayOrderDB
    {
        internal static ActionResult SaveLoanDelayOrder(LoanDelayOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            using SqlCommand cmd = new SqlCommand();

            conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = "pr_loan_delay_order";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
            cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
            cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
            cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductAppId;

            cmd.Parameters.Add("@delay_date", SqlDbType.SmallDateTime).Value = order.DelayDate;
            cmd.Parameters.Add("@delay_reason", SqlDbType.NVarChar, 500).Value = order.DelayReason;


            SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);
            cmd.ExecuteNonQuery();
            result.ResultCode = ResultCode.Normal;
            order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
            result.Id = order.Id;

            return result;

        }
        internal static LoanDelayOrder GetLoanDelayOrderData(LoanDelayOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT   
                                            hb.filial,
                                            hb.customer_number,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number as hb_document_number,
                                            hb.document_subtype,
                                            hb.quality,
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
                                            ld.*
                                            from Tbl_HB_documents hb inner join Tbl_loan_delay_order_details ld
                                            on ld.doc_id=hb.doc_ID
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();


                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["hb_document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.ProductAppId = Convert.ToInt64(dr["product_app_Id"].ToString());
                    order.DelayDate = dr["delay_date"] != DBNull.Value ? Convert.ToDateTime(dr["delay_date"]) : default(DateTime);
                    order.DelayReason = dr["delay_reason"] != DBNull.Value ? dr["delay_reason"].ToString() : null;

                }
            }
            return order;
        }
        internal static ActionResult SaveCancelLoanDelayOrder(CancelDelayOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_loan_cancel_delay_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductAppId;

                    cmd.Parameters.Add("@delay_date", SqlDbType.SmallDateTime).Value = order.CancelDelayDate;
                    cmd.Parameters.Add("@delay_reason", SqlDbType.Int).Value = Convert.ToInt32(order.DelayReason);


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;

                    return result;
                }
            }
        }
        internal static CancelDelayOrder GetCancelLoanDelayOrderData(CancelDelayOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT   
                                            hb.filial,
                                            hb.customer_number,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number as hb_document_number,
                                            hb.document_subtype,
                                            hb.quality,
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
											rt.description,
                                            ld.*
                                            from Tbl_HB_documents hb inner join Tbl_loan_cancel_delay_order_details ld
                                            on ld.doc_id=hb.doc_ID
											inner join tbl_cancel_delay_reason rt on ld.delay_reason = rt.reason_type
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();


                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["hb_document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.ProductAppId = Convert.ToInt64(dr["product_app_Id"].ToString());
                    order.CancelDelayDate = dr["delay_date"] != DBNull.Value ? Convert.ToDateTime(dr["delay_date"]) : default(DateTime);
                    order.DelayReason = dr["delay_reason"] != DBNull.Value ? dr["description"].ToString() : null;

                }
            }
            return order;
        }


        internal static bool CheckValidLoanDelayCancelOrder(long producId)
        {
            bool isValid = true;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT  1 FROM [Tbl_repayments_of_bl;] bl
                                        INNER JOIN tbl_loan_repayments_delays dl
                                        on dl.app_id= bl.app_id
                                        WHERE dl.app_id=@appId AND bl.date_of_repayment>=dl.delay_date AND bl.is_rescheduled=1";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = producId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                    isValid = false;

                return isValid;

            }
        }
    }
}
