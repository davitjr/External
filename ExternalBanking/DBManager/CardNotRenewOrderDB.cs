using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class CardNotRenewOrderDB
    {

        /// <summary>
        ///  Չվերաթողարկել քարտը հայտի տվյալների պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(CardNotRenewOrder order, SourceType source, string userName)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_not_renew_order_document";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@reason", SqlDbType.Int).Value = order.Reason;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@order_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Card.Currency;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = order.Card.ProductId;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 9)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;
                    }
                    else if (actionResult == 8 || actionResult == 885)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Վերադարձնում է Չվերաթողարկել քարտը հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static CardNotRenewOrder GetCardNotRenewOrder(CardNotRenewOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

               using SqlCommand cmd = new SqlCommand(@" SELECT D.app_id,                                                          
                                                          H.customer_number,
                                                          H.document_number,
                                                          H.currency,
                                                          H.document_type,
                                                          H.quality,
                                                          H.operation_date,
                                                          H.registration_date,
                                                          H.description AS descriptionH,
                                                          R.description AS descriptionR
                                                  FROM dbo.Tbl_HB_documents H 
                                                  INNER JOIN dbo.tbl_card_not_renew_order_details D 
                                                  ON H.doc_ID = D.doc_ID  
                                                  INNER JOIN (SELECT id,REPLACE(description,'âí»ñ³ÃáÕ³ñÏí³Í ù³ñï:','') AS description 
                                                                     FROM Tbl_Type_Of_CardClosingReasons WHERE forNotRnew = 1) R
                                                  ON R.id = D.reason
                                                  WHERE H.doc_ID = @DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;

                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    ulong productId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
                    order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Card = Card.GetCard(productId, order.CustomerNumber);
                    order.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["descriptionH"].ToString());
                    order.ReasonDesc = Utility.ConvertAnsiToUnicode(dt.Rows[0]["descriptionR"].ToString());
                }
            }
            return order;
        }

        /// <summary>
        /// Քարտի գարգավիճակը NORM է թե ոչ
        /// </summary>
        internal static bool IsNormCardStatus(string cardNumber, long productId)
        {
            bool isNorm = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1 FROM tbl_visa_applications WHERE cardstatus='NORM' AND cardnumber= @cardNumber AND app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.VarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            isNorm = true;
                    }
                }
            }
            return isNorm;
        }

        /// <summary>
        /// Քարտն արդեն չվերաթողարկվել է։
        /// </summary>
        internal static bool IsCardAlreadyNotRenewed(long productId)
        {
            bool isCardAlreadyNotRenewed = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 1  FROM Tbl_VisaAppAdditions WHERE  AdditionID = 11 AND app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            isCardAlreadyNotRenewed = true;
                    }
                }
            }
            return isCardAlreadyNotRenewed;
        }
    }
}
