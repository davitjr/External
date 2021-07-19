using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    class CardUSSDServiceOrderDB
    {
        internal static ActionResult SaveUSSDServiceGenerationOrder(CardUSSDServiceOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_USSD_service_generation_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductID;
                    cmd.Parameters.Add("@mobilePhone", SqlDbType.NVarChar,15).Value = order.MobilePhone;
                    cmd.Parameters.Add("@actionType", SqlDbType.TinyInt).Value = order.ActionType;

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

        internal static string GetCardMobilePhone(ulong productId)
        {
            string mobilePhone = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT fax_home FROM Tbl_VISA_applications WHERE app_id =@productID ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productID", SqlDbType.Float).Value = productId;
                    conn.Open();

                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["fax_home"] != DBNull.Value)
                        {
                            mobilePhone = row["fax_home"].ToString();
                        }
                    }

                }
            }
            return mobilePhone;
        }

        internal static bool IsSecondUSSDServiceOrder(ulong productId)
        {
            bool secondService = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select  d.doc_ID                                            
                                                  from Tbl_HB_documents as d left join Tbl_card_USSD_serivce_order_details as c on  d.doc_ID=c.Doc_ID

                                                  where c.product_app_id=@appID and d.quality in(1,2,3,5)", conn);

                cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;

                if (cmd.ExecuteReader().Read())
                {
                    secondService = true;
                }



            }
            return secondService;
        }

        internal static CardUSSDServiceOrder GetCardUSSDServiceOrder(CardUSSDServiceOrder order)
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
                                            dt.product_app_Id,
                                            dt.mobile_phone,
                                            dt.action_type
                                            from Tbl_HB_documents hb inner join Tbl_card_USSD_serivce_order_details dt
                                            on dt.doc_id=hb.doc_ID
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                SqlDataReader dr = cmd.ExecuteReader();


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
                    order.ProductID = Convert.ToUInt64(dr["product_app_Id"].ToString());
                    order.MobilePhone = dr["mobile_phone"].ToString();
                    order.ActionType = short.Parse(dr["action_type"].ToString());
                }

                
            }
            return order;
        }

        public static List<CardUSSDServiceHistory> GetCardUSSDServiceHistory(ulong productID)
        {

            List<CardUSSDServiceHistory> listCardUSSDServiceHistory = new List<CardUSSDServiceHistory>();
            CardUSSDServiceHistory service;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" select t.description actionDescription,s.registration_date,r.requestFileName,s.arca_response,s.set_number, s.action
                                       FROM  V_ArcaRequestHeaders H
                                       INNER JOIN Tbl_cards_USSD_history s ON H.id = s.header_id
									   INNER join Tbl_Type_Of_ArcaCardSmsServiceActions t on t.id=S.action
									   INNER JOIN V_arcaResponse R on R.arcaAppId= H.arcaAppId
                                       WHERE commandType = 24 AND appid = @productId
                                       AND ISNULL(arca_response,-1)<>-1
                                       ORDER BY s.id DESC";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productID;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        if (row != null)
                        {
                            service = new CardUSSDServiceHistory();
                            service.ActionType = row["action"] != DBNull.Value ? short.Parse(row["action"].ToString()) : (short)-1;
                            service.ActionTypeDescription = Utility.ConvertAnsiToUnicode(row["actionDescription"].ToString());
                            service.SetNumber = Convert.ToInt32(row["set_number"].ToString());
                            service.RegistrationDate = Convert.ToDateTime(row["registration_date"].ToString());
                            service.ArcaResponse = row["arca_response"] != DBNull.Value ? Convert.ToInt16(row["arca_response"].ToString()) : (short)-1;
                            service.RequestFileName = row["requestFileName"].ToString();
                            listCardUSSDServiceHistory.Add(service);
                        }
                        
                    }
                }
            }
            return listCardUSSDServiceHistory;
        }

    }
}
