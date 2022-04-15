using ExternalBanking.ACBAServiceReference;
using ExternalBanking.XBManagement;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class HBServletRequestOrderDB
    {

        /// <summary>
        /// Տոկենի ակտիվացման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ActionResult SaveHBServletRequestOrder(HBServletRequestOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HBServletRequest_Order_Details";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)order.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    if (order.ServletAction != HBServletAction.UnlockUser)
                        cmd.Parameters.Add("@hb_token_id", SqlDbType.Int).Value = order.HBtoken.ID;
                    cmd.Parameters.Add("@servletRequestActionType", SqlDbType.TinyInt).Value = order.ServletAction;

                    if (order.ServletAction == HBServletAction.UnlockToken)
                        cmd.Parameters.Add("@numeric_code_one", SqlDbType.Int).Value = order.ServletRequest.OTP;

                    if (order.GroupId != 0)
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);

                    result.ResultCode = ResultCode.Normal;
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                }
            }
            return result;
        }

        internal static HBServletRequestOrder GetHBServletRequestOrder(HBServletRequestOrder order)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"SELECT registration_date,
                                                         doc.document_number,
                                                         doc.document_type,
                                                         doc.debet_account,
                                                         doc.deb_for_transfer_payment,
                                                         doc.source_type,
                                                         doc.quality,
                                                         doc.document_subtype ,
                                                         doc.registration_date,
                                                         doc.operation_date,
                                                         doc.confirmation_date,
                                                         det.hb_token_id                                                            
                                                         FROM Tbl_HB_documents as doc

                                                         INNER JOIN Tbl_HBServletRequest_Order_Details det
                                                         on doc.doc_ID=det.doc_ID
                                                         WHERE doc.customer_number=case when @customer_number = 0 then doc.customer_number else @customer_number end AND doc.doc_ID=@DocID", conn);
            cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        order.HBtoken = new HBToken
                        {
                            TokenNumber = dr["hb_token_id"].ToString()
                        };
                        order.OrderNumber = dr["document_number"].ToString();
                        order.Type = (OrderType)dr["document_type"];
                        order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                        order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);
                        order.SubType = Convert.ToByte(dr["document_subtype"]);
                        order.Source = (SourceType)int.Parse(dr["source_type"].ToString());
                        order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                        order.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime?);
                    }
                }
            }
            return order;
        }
        internal static ActionResult UpdateHBdocumentQuality(long docID, User user)
        {

            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "HB_OK_confirm";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docID;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@Uniq_Item_number", SqlDbType.BigInt).Value = DBNull.Value;
                    cmd.Parameters.Add("@transactions_Group_number", SqlDbType.BigInt).Value = DBNull.Value;
                    cmd.Parameters.Add("@set_number", SqlDbType.SmallInt).Value = user.userID;

                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;

        }



        internal static HBServletRequestOrder GetHBServletRequestOrder(ulong customerNumber, OrderType documentType)
        {
            HBServletRequestOrder order = null;
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"
											SELECT TOP 1 registration_date,
                                                         doc.doc_id,
                                                         doc.document_number,
                                                         doc.document_type,
                                                         doc.debet_account,
                                                         doc.deb_for_transfer_payment,
                                                         doc.source_type,
                                                         doc.quality,
                                                         doc.document_subtype ,
                                                         doc.registration_date,
                                                         doc.operation_date,
                                                         det.hb_token_id,
                                                        tt.token_serial,
                                                        tt.GID,
                                                        tt.id as tokenId
                                                         FROM Tbl_HB_documents as doc
                                                         INNER JOIN Tbl_HBServletRequest_Order_Details det
                                                         on doc.doc_ID=det.doc_ID
                                                          inner join dbo.tbl_tokens TT 
                                                        on det.hb_token_id = TT.id 
                                                         WHERE doc.customer_number=case when @customer_number = 0 then doc.customer_number else @customer_number end AND doc.document_type=@documentType order by doc_ID desc", conn);
            cmd.Parameters.Add("@documentType", SqlDbType.Int).Value = (int)documentType;
            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.HasRows)
                {
                    if (dr.Read())
                    {
                        order = new HBServletRequestOrder
                        {
                            HBtoken = new HBToken(),

                            Id = int.Parse(dr["doc_id"].ToString()),
                            OrderNumber = dr["document_number"].ToString(),
                            Type = (OrderType)dr["document_type"],
                            RegistrationDate = Convert.ToDateTime(dr["registration_date"]),
                            Quality = (OrderQuality)Convert.ToInt16(dr["quality"]),
                            SubType = Convert.ToByte(dr["document_subtype"]),
                            Source = (SourceType)int.Parse(dr["source_type"].ToString()),
                            OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?)
                        };
                        order.HBtoken.TokenNumber = dr["token_serial"].ToString();
                        order.HBtoken.GID = dr["gid"].ToString();
                        order.HBtoken.ID = int.Parse(dr["tokenId"].ToString());
                    }
                }
            }
            return order;

        }
    }
}
