using ExternalBanking.XBManagement;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class HBApplicationQualityChangeDB
    {
        internal static ActionResult Save(HBApplicationQualityChangeOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_HBApplication_Quality_Change_Order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)order.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@global_id", SqlDbType.Int).Value = order.HBApplication.ID;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Draft;
                }
            }
            return result;
        }

        internal static HBApplicationQualityChangeOrder GetHBApplicationQualityChangeOrder(HBApplicationQualityChangeOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
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

                                                         det.contract_number,   
                                                         det.contract_date                                                           
                                                         FROM Tbl_HB_documents as doc

                                                         INNER JOIN Tbl_HBApplication_Order_Details det
                                                         on doc.doc_ID=det.doc_ID
                                                         WHERE doc.customer_number=case when @customer_number = 0 then doc.customer_number else @customer_number end AND doc.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.HBApplication = new HBApplication();
                    order.HBApplication.ContractNumber = dt.Rows[0]["contract_number"].ToString();
                    order.HBApplication.ContractDate = dt.Rows[0]["contract_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["contract_date"]) : default(DateTime?);

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

                }
            }
            return order;
        }
    }
}
