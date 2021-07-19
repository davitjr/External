using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class LoanProductTerminationOrderDB
    {
        /// <summary>
        /// Վարկային պրոդուկտի դադարեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>

        internal static ActionResult SaveLoanProductTerminationOrder(LoanProductTerminationOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_addNewGuaranteeTerminationDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@product_Id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
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

        internal static LoanProductTerminationOrder GetLoanProductTerminationOrder(LoanProductTerminationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT  d.document_number,d.currency,d.debet_account,d.quality,d.description,d.document_type,
                                                  d.registration_date,d.document_subtype,d.source_type,d.operation_date,I.app_id
                                                  FROM Tbl_HB_documents as d
                                                  INNER JOIN Tbl_HB_Products_Identity I ON d.doc_id=I.hb_doc_id 
                                                  WHERE d.doc_ID=@docID AND d.customer_number=case WHEN @customer_number = 0 THEN d.customer_number ELSE @customer_number END", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.ProductId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.Type= (OrderType)(dt.Rows[0]["document_type"]);
                order.Description = dt.Rows[0]["description"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                return order;
            }

        }

        internal static bool IsSecondTermination(LoanProductTerminationOrder order)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select doc_id from Tbl_HB_documents D INNER JOIN Tbl_HB_Products_Identity  I ON D.doc_ID=I.HB_Doc_ID
                                                WHERE quality in (1,2,3,5) and document_type=143 and document_subtype=1 and
                                                customer_number=@customerNumber and I.App_ID=@productId", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.ProductId;

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    check = true;
                }
            }
            return check;
        }
    }
}
