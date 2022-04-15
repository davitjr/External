using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class PINRegenerationOrderDB
    {
        /// <summary>
        /// "Փոխարինում` նույն համար, նույն ժամկետ" հայտի տվյալների պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SavePINRegOrderDetails(PINRegenerationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_PIN_regeneration_document";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@card_type", SqlDbType.Int).Value = order.Card.Type;
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
        /// Վերադարձնում է "Փոխարինում` նույն համար, նույն ժամկետ" հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static PINRegenerationOrder GetPINRegenerationOrder(PINRegenerationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT D.app_id AS app_id,
                                                          H.customer_number,
                                                          H.document_number,
                                                          H.currency,
                                                          H.document_type,
                                                          H.document_subtype,
                                                          H.quality,
                                                          H.operation_date,
                                                          H.registration_date,
                                                          C.armenian AS card_technology
                                                    FROM Tbl_HB_documents H 
                                                         LEFT JOIN tbl_PIN_regeneration_order_details D 
                                                         ON H.doc_ID = D.doc_ID  
                                                         INNER JOIN tbl_visa_applications VA
                                                         ON VA.app_id = D.app_id 
                                                         INNER JOIN tbl_type_of_card T
                                                         ON T.ID = VA.cardType
                                                         INNER JOIN Tbl_type_of_Card_Technology C
                                                         ON C.abbreviation = T.C_M
                                                         WHERE H.doc_ID = @DocID", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    ulong productId = dt.Rows[0]["app_id"] != DBNull.Value ? ulong.Parse(dt.Rows[0]["app_id"].ToString()) : ulong.Parse(dt.Rows[0]["appID"].ToString());
                    order.CustomerNumber = ulong.Parse(dt.Rows[0]["customer_number"].ToString());
                    order.Card = Card.GetCard(productId, order.CustomerNumber);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.CardTechnology = Utility.ConvertAnsiToUnicode(dt.Rows[0]["card_technology"].ToString());
                }
            }
            return order;
        }
    }
}
