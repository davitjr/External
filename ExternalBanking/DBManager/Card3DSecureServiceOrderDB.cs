using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    static class Card3DSecureServiceOrderDB
    {


        internal static RemovalOrder Get(RemovalOrder order)
        {
            return order;
        }

        internal static ActionResult Save(Card3DSecureServiceOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"DECLARE @filial AS int                                                    
                                                 SELECT @filial=filialcode FROM Tbl_customers WHERE customer_number=@customerNumber   
                                                 INSERT INTO Tbl_HB_documents
                                                 (filial,customer_number,registration_date,document_type,
                                                 document_number,document_subtype,quality, 
                                                 source_type,operationFilialCode,operation_date)
                                                 VALUES
                                                 (@filial,@customerNumber,@regDate,@docType,@docNumber,@docSubType,
                                                 1,@sourceType,@operationFilialCode,@operDay)
                                                 UPDATE Tbl_HB_quality_history SET change_user_name = @userName 
                                                 WHERE Doc_ID = Scope_identity() and quality = 1
                                                 SELECT Scope_identity() as ID ", conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@regDate", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
            cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (int)order.Type;
            cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@docSubType", SqlDbType.Int).Value = order.SubType;
            cmd.Parameters.Add("@userName", SqlDbType.NVarChar, 20).Value = userName;
            cmd.Parameters.Add("@sourceType", SqlDbType.Int).Value = (short)order.Source;
            cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = order.OperationDate;

            order.Id = Convert.ToInt64(cmd.ExecuteScalar());
            SaveOrderDetails(order);
            result.ResultCode = ResultCode.Normal;
            order.Quality = OrderQuality.Draft;
            return result;

        }
        internal static void SaveOrderDetails(Card3DSecureServiceOrder order)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();
            using SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_card_3dsecure_serivce_order_details
                                                  (Doc_ID,product_app_Id,card_number,customer_number,email,action_type)
                                                   VALUES(@docID,@productAppId,@cardNumber,@customerNumber,@email,@actionType)", conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = order.Card3DSecureService.CardNumber;
            cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.Card3DSecureService.CustomerNumber;
            cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
            cmd.Parameters.Add("@actionType", SqlDbType.TinyInt).Value = order.Card3DSecureService.ActionType;
            cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = order.Card3DSecureService.Email;
            cmd.Parameters.Add("@productAppId", SqlDbType.BigInt).Value = order.Card3DSecureService.ProductID;
            cmd.ExecuteNonQuery();
        }

        internal static bool IsSecondCard3DServiceOrder(ulong productID)
        {

            bool secondService = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"select  d.doc_ID                                            
                                                  from Tbl_HB_documents as d left 
                                                  join tbl_card_3dsecure_serivce_order_details as c 
                                                  on  d.doc_ID=c.Doc_ID
                                                  where c.product_app_id=@appID and d.quality in(1,2,3,5)", conn);
                cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productID;
                if (cmd.ExecuteReader().Read())
                {
                    secondService = true;
                }
            }
            return secondService;

        }

        public static List<Card3DSecureService> GetCard3DSecureServiceHistory(ulong productID)
        {

            List<Card3DSecureService> card3DSecureServiceHistory = new List<Card3DSecureService>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT t.description actionDescription,s.registration_date,r.requestFileName,s.arca_response,s.set_number, s.action,s.email
                                       FROM  V_ArcaRequestHeaders H
                                       INNER JOIN Tbl_cards_3DSecure_history s ON H.id = s.header_id
									   INNER JOIN Tbl_Type_Of_ArcaCardSmsServiceActions t on t.id=S.action
									   INNER JOIN V_arcaResponse R on R.arcaAppId= H.arcaAppId
                                       WHERE commandType = 25 AND appid = @productId
                                       AND s.arca_response IS NOT NULL
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
                        Card3DSecureService service = setService(row);
                        card3DSecureServiceHistory.Add(service);
                    }
                }
            }
            return card3DSecureServiceHistory;
        }
        private static Card3DSecureService setService(DataRow row)
        {
            Card3DSecureService service = new Card3DSecureService();
            if (row != null)
            {
                //service.CardNumber = row["card_number"].ToString();
                //service.ProductID =Convert.ToUInt64( row["app_ID"].ToString());
                service.ActionType = row["action"] != DBNull.Value ? short.Parse(row["action"].ToString()) : (short)-1;
                service.ActionTypeDescription = Utility.ConvertAnsiToUnicode(row["actionDescription"].ToString());
                service.SetNumber = Convert.ToInt32(row["set_number"].ToString());
                service.Email = row["email"].ToString();
                service.RegistrationDate = Convert.ToDateTime(row["registration_date"].ToString());
                service.ArcaResponse = row["arca_response"] != DBNull.Value ? Convert.ToInt16(row["arca_response"].ToString()) : (short)-1;
                service.RequestFileName = row["requestFileName"].ToString();
            }
            return service;
        }
    }
}
