using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;

namespace ExternalBanking.DBManager
{
    internal class CardClosingOrderDB
    {

        internal static ActionResult Save(CardClosingOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_AddNewCardClosingDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@closing_reason", SqlDbType.Int).Value = order.ClosingReason;
                    cmd.Parameters.Add("@closing_reason_add", SqlDbType.NVarChar).Value = order.ClosingReasonAdd;
                    cmd.Parameters.Add("@App_ID", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@close_card_account", SqlDbType.Bit).Value = order.CloseCardAccount;

                    if (order.GroupId != 0)
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;
                    return result;
                }

            }
        }

        internal static CardClosingOrder GetCardClosingOrder(CardClosingOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select  d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		                                                c.closing_reason,c.app_id,c.closing_reason_add,d.operation_date, d.order_group_id,d.confirmation_date, d.source_type
                                                        from Tbl_HB_documents as d left join Tbl_card_closing_order_details as c on  d.doc_ID=c.Doc_ID
                                                        where d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn))
                {

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.ProductId = ulong.Parse(dt.Rows[0]["app_id"].ToString());
                    order.ClosingReason = short.Parse(dt.Rows[0]["closing_reason"].ToString());
                    order.ClosingReasonAdd = dt.Rows[0]["closing_reason_add"] != DBNull.Value ? dt.Rows[0]["closing_reason_add"].ToString() : null;
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = int.Parse(dt.Rows[0]["order_group_id"].ToString());
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.Source = dt.Rows[0]["source_type"] != DBNull.Value ? (SourceType)Convert.ToInt32(dt.Rows[0]["source_type"]) : SourceType.NotSpecified;
                }
            }
            return order;
        }

        public static List<ActionError> CheckCardClosingReason(ulong productId, int reason)
        {
            List<ActionError> result = new List<ActionError>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"select 1 from Tbl_CardChanges where app_id=@app_ID", conn);

                cmd.Parameters.Add("@app_ID", SqlDbType.Float).Value = productId;


                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if ((reason > 38 && reason < 48) || (reason > 56 && reason < 65) || reason == 75)
                    {
                        //Քարտը վերաթողարկված է: Ընտրեք համապատասխան պատճառը
                        result.Add(new ActionError(522));
                    }

                }
                else
                {
                    if ((reason > 47 && reason < 57) || reason == 74)
                    {
                        //Քարտը վերաթողարկված չէ: Ընտրեք համապատասխան պատճառը
                        result.Add(new ActionError(523));
                    }
                }

            }
            return result;
        }

        public static List<ActionError> CheckCardPeriodicTransfer(string accountNumber)
        {
            List<ActionError> result = new List<ActionError>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 nn FROM [Tbl_Operations_By_Period] 
                                                 WHERE (quality = 1 And (Debet_Account =@cardAccount OR Credit_Account =@cardAccount))", conn);

                cmd.Parameters.Add("@cardAccount", SqlDbType.Float).Value = accountNumber;


                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    result.Add(new ActionError(524, new string[] { dr["nn"].ToString() }));
                }

            }
            return result;
        }

        public static List<ActionError> CheckCardTransactions(string cardNumber, short cardType)
        {
            List<ActionError> result = new List<ActionError>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                //CashBack NFC  (50)
                using SqlCommand cmd = new SqlCommand(@"select 1 
                    FROM Tbl_Visa_Clearing
                    where  ((TransOK=0  and  ( @cardType NOT IN (34, 50) or ( device_type in (6011,6010,2222,742))))
                    or (  @cardType IN (34, 50)  and not (device_type in (6011,6010,2222,742) or Transaction_type in(12,17)) and confirmation_date is null))
                    and card_number=@cardNumber", conn);

                cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;
                cmd.Parameters.Add("@cardType", SqlDbType.TinyInt).Value = cardType;

                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    result.Add(new ActionError(525));
                }

            }
            return result;
        }

        internal static bool IsSecondClosing(ulong productId)
        {
            bool secondClosing = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"select  d.doc_ID                                            
                                                  from Tbl_HB_documents as d left join Tbl_card_closing_order_details as c on  d.doc_ID=c.Doc_ID

                                                  where c.app_id=@appID and d.quality in(1,2,3,5)", conn);

                cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;

                if (cmd.ExecuteReader().Read())
                {
                    secondClosing = true;
                }



            }
            return secondClosing;
        }

        internal static ActionResult SaveCardClosingOrderDetails(CardClosingOrder cardClosingOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_card_closing_order_details(order_id, closing_reason, closing_reason_add) VALUES(@orderId, @closingReason, @closingReasonAdd)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@closingReason", SqlDbType.Int).Value = cardClosingOrder.ClosingReason;
                    cmd.Parameters.Add("@closingReasonAdd", SqlDbType.NVarChar, 255).Value = (object)cardClosingOrder.ClosingReasonAdd ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }


        internal static bool CheckCardPensionApplication(string cardNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT card_number FROM Tbl_pension_application WHERE  quality = 0 and deleted = 0 and closing_date is null and card_number =@card_number", conn);

                cmd.Parameters.Add("@card_number", SqlDbType.Float).Value = cardNumber;

                if (cmd.ExecuteReader().Read())
                {
                    return true;
                }
                else
                    return false;


            }

        }
    }
}
