using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;


namespace ExternalBanking.DBManager
{
    static class MembershipRewardsOrderDB
    {
        internal static ActionResult SaveCardMembershipRewardsOrder(MembershipRewardsOrder order, string userName)      // SaveCardMembershipRewardsRegistrationOrder
        {
            ActionResult result = new ActionResult();
            //string procedureName = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_membership_rewards_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductId;

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

        internal static MembershipRewardsOrder GetCardMembershipRewardsOrder(MembershipRewardsOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                                                hb.filial,
                                                                                                hb.customer_number,
                                                                                                hb.registration_date,
                                                                                                hb.document_type,
                                                                                                hb.document_number,
                                                                                                hb.document_subtype,
                                                                                                hb.quality, 
                                                                                                hb.source_type,
                                                                                                hb.operationFilialCode,
                                                                                                hb.operation_date ,
                                                                                                p.App_ID
			                                                                                    FROM dbo.Tbl_HB_documents hb
                                                                                                INNER JOIN dbo.Tbl_HB_Products_Identity p
                                                                                                ON hb.doc_ID = p.hb_doc_id
                                                                                    WHERE hb.Doc_ID=@DocID and hb.customer_number=CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                order.FilialCode = dt.Rows[0]["operationFilialCode"] != DBNull.Value ? Convert.ToUInt16(dt.Rows[0]["operationFilialCode"]) : (ushort)0;
                
            }
            return order;
        }
    }
}
