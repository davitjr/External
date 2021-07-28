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
    internal class LinkPaymentOrderDB
    {

        public static ActionResult Save(LinkPaymentOrder order)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "pr_save_link_payment_order";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                cmd.Parameters.Add("@fee_amount", SqlDbType.Float).Value = order.FeeAmount;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 5).Value = order.Currency;

                cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                cmd.Parameters.Add("@registration_date", SqlDbType.DateTime).Value = order.RegistrationDate;
                cmd.Parameters.Add("@credit_account", SqlDbType.NVarChar, 50).Value = order.CreditAccount.AccountNumber;
                cmd.Parameters.Add("@document_type", SqlDbType.TinyInt).Value = order.Type;
                cmd.Parameters.Add("@ducument_sub_type", SqlDbType.TinyInt).Value = order.SubType;
                cmd.Parameters.Add("@description", SqlDbType.NVarChar, 100).Value = order.Description;
                cmd.Parameters.Add("@link_payment_description", SqlDbType.NVarChar, 200).Value = order.LinkPaymentDescription;
                cmd.Parameters.Add("@document_number", SqlDbType.Int).Value = order.OrderNumber;
                cmd.Parameters.Add("@payment_source_type", SqlDbType.Int).Value = order.PaymentSourceType;
                cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = order.Source;
                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = order.user.userName;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = order.UserId;

                cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


                cmd.ExecuteNonQuery();

                byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

                order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                order.Quality = OrderQuality.Draft;

                if (actionResult == 1)
                {
                    result.ResultCode = ResultCode.Normal;
                    result.Id = id;
                }
                else if (actionResult == 0)
                {
                    result.ResultCode = ResultCode.Failed;
                    result.Id = -1;
                }
                return result;
            }
        }

        public static LinkPaymentOrder GetDetails(long docId)
        {
            LinkPaymentOrder paymentOrder = new LinkPaymentOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT * FROM Tbl_HB_documents hb
                                    INNER JOIN Tbl_link_payment_order_details lp on hb.doc_id = lp.doc_id WHERE hb.doc_id = @doc_id ";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                        paymentOrder = SetOrderProprty(dr);

                return paymentOrder;
            }
        }

        internal static bool CheckShortId(string shortId)
        {
            bool HasRow = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = @"SELECT 1 FROM Tbl_link_payment_order_details where short_id = @short_id  ";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@short_id", SqlDbType.NVarChar, 200).Value = shortId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                        HasRow = true;

                return HasRow;
            }
        }

        internal static void SaveLink(LinkPaymentOrder Order)
        {
            string query = @"UPDATE Tbl_link_payment_order_details 
                                    SET link_url = @linkUrl,
                                        short_id = @short_id
                                    WHERE doc_id = @doc_id

                                     INSERT INTO Tbl_link_payment(short_id, status)
                                     VALUES(@short_id, 0) ";

            if (Order.PaymentSourceType == LinkPaymentSourceType.FromLinkPayment)
                query += Environment.NewLine + @" UPDATE Tbl_BILl_split_senders
                               SET link_url = @linkUrl
                           WHERE link_payment_doc_id = @doc_id";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = Order.Id;
                cmd.Parameters.Add("@linkUrl", SqlDbType.NVarChar, 20).Value = Order.LinkURL;
                cmd.Parameters.Add("@short_id", SqlDbType.NVarChar, 20).Value = Order.ShortId;

                cmd.ExecuteNonQuery();
            }
        }

        internal static ActionResult SaveLinkPaymentPayer(LinkPaymentPayer linkPayment, short userId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_save_link_payment_payer", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@short_id", SqlDbType.NVarChar, 20).Value = linkPayment.ShortId;
                    cmd.Parameters.Add("@payer_name", SqlDbType.NVarChar, 20).Value = linkPayment.PayerName;
                    cmd.Parameters.Add("@payer_card", SqlDbType.NVarChar, 20).Value = linkPayment.PayerCard;
                    cmd.Parameters.Add("@check_box", SqlDbType.Bit).Value = linkPayment.CheckBox;
                    cmd.Parameters.Add("@set_Date", SqlDbType.Int).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = userId;


                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                }
            }

            return result;
        }

        public static LinkPaymentOrder GetLinkPaymentOrderWithShortId(string shortId)
        {
            LinkPaymentOrder paymentOrder = new LinkPaymentOrder();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                //doc_id, registration_date, amount, credit_account, currency, description, link_url, link_payment_description, payment_source_type, payment_source_type
                cmd.CommandText = @"SELECT * FROM Tbl_HB_documents hb
                                    INNER JOIN Tbl_link_payment_order_details lp on hb.doc_id = lp.doc_id 
                                    WHERE hb.quality = 30 AND short_id = @short_id ";
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@short_id", SqlDbType.NVarChar, 20).Value = shortId;

                using (SqlDataReader dr = cmd.ExecuteReader())
                    if (dr.Read())
                        paymentOrder = SetOrderProprty(dr);

                return paymentOrder;
            }
        }

        private static LinkPaymentOrder SetOrderProprty(SqlDataReader dr)
        {
            LinkPaymentOrder paymentOrder = new LinkPaymentOrder();

            paymentOrder.Id = Convert.ToInt64(dr["doc_id"]);
            paymentOrder.Quality = (OrderQuality)Convert.ToInt32(dr["quality"]);
            paymentOrder.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
            paymentOrder.Amount = Convert.ToDouble(dr["amount"]);
            paymentOrder.FeeAmount = Convert.ToDouble(dr["fee_amount"]);
            paymentOrder.CreditAccount = Account.GetAccount(dr["credit_account"].ToString());
            paymentOrder.Currency = dr["currency"].ToString();
            paymentOrder.Description = dr["description"].ToString();
            paymentOrder.LinkURL = dr["link_url"] != DBNull.Value ? dr["link_url"].ToString() : string.Empty;
            paymentOrder.LinkPaymentDescription = dr["link_payment_description"] != DBNull.Value ? dr["link_payment_description"].ToString() : string.Empty;
            paymentOrder.PaymentSourceType = dr["payment_source_type"] != DBNull.Value ? (LinkPaymentSourceType)Convert.ToByte(dr["payment_source_type"]) : LinkPaymentSourceType.None;
            paymentOrder.Source = (SourceType)Convert.ToInt16(dr["payment_source_type"]);
            paymentOrder.UserId = Convert.ToDouble(dr["user_id"]);
            paymentOrder.Activ = Convert.ToBoolean(dr["isActive"]);
            paymentOrder.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

            return paymentOrder;
        }
    
    }
}
