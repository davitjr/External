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
    internal class CreditLineProlongationOrderDB
    {

        internal static ActionResult SaveCreditLineProlongationOrder(CreditLineProlongationOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_Credit_Line_Prolongation_Order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

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

        internal static CreditLineProlongationOrder GetCreditLineProlongationOrder(CreditLineProlongationOrder order)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT   hb.customer_number,
                                    hb.registration_date, 
                                    hb.document_number,
                                    hb.registration_date,
                                    hb.document_subtype,
                                    hb.quality,
                                    hb.currency, 
                                    hb.operation_date,
                                    hb.source_type,
                                    hb.document_type,
                                    hb.filial
                                        FROM Tbl_HB_documents hb
                                        WHERE hb.Doc_ID = @DocID
                                        AND hb.customer_number  = CASE WHEN @customer_number = 0 THEN hb.customer_number   ELSE @customer_number END";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                    order.FilialCode = Convert.ToUInt16(dt.Rows[0]["filial"]);
                }

                return order;
            }


        }

        

        internal static bool IsSecontCreditLineProlongationApplication(ulong productId, ushort orderType, long docId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT doc_ID FROM Tbl_HB_Products_Identity id
                                                    INNER JOIN Tbl_HB_documents hb
                                                    ON hb.doc_ID=id.HB_Doc_ID
                                                    WHERE App_ID=@AppID and hb.quality in(1,2,3,5,50,100) AND document_type=@documentType AND doc_ID<>@docId", conn);

                cmd.Parameters.Add("@AppID", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@documentType", SqlDbType.SmallInt).Value = orderType;
                cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;

                if (cmd.ExecuteReader().Read())
                {
                    return true;
                }
                else
                    return false;


            }

        }

        /// <summary>
        /// Վարկային գծի երկարաձգման տողի առկայության 
        /// </summary>
        internal static bool IsCreditLineProlongation(ulong productAppId)
        {
            bool flag = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = "Select app_id from Tbl_prolonged_products where confirmation_date is null and app_id=@productAppId";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productAppId", SqlDbType.Float).Value = productAppId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            flag = true;
                        }
                    }
                }
                return flag;
            }
        }
    }
}
    

