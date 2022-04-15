using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class CashPosPaymentOrderDB
    {
        internal static ActionResult Save(CashPosPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_cash_out_by_POS";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@docType", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@docNumber", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@regDate", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar, 4000).Value = order.Description;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@sourceType", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = order.CardNumber;
                    cmd.Parameters.Add("@authorizationCode", SqlDbType.NVarChar, 20).Value = order.AuthorizationCode;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@posAccount", SqlDbType.Float).Value = order.PosAccount.AccountNumber;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.VarChar, 50).Value = order.CreditAccount.AccountNumber;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.ResultCode = ResultCode.Normal;
                }
            }

            return result;
        }

        internal static ActionResult SaveCashPosPaymentOrderDetails(CashPosPaymentOrder cashPosPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_cash_pos_payment_order_details(order_id, card_number, authorization_code) VALUES(@orderId, @cardNumber, @authorizationCode)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 20).Value = cashPosPaymentOrder.CardNumber;
                    cmd.Parameters.Add("@authorizationCode", SqlDbType.NVarChar, 20).Value = cashPosPaymentOrder.AuthorizationCode;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static CashPosPaymentOrder GetCashPosPaymentOrder(CashPosPaymentOrder order)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select * from Tbl_Hb_Documents D inner join tbl_cash_pos_order_details C on D.doc_ID=C.doc_ID where C.doc_id=@id and d.customer_number=case when @customerNumber = 0 then d.customer_number else @customerNumber end ";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            order.Id = long.Parse(dr["doc_id"].ToString());

                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"]);

                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);

                            order.Type = (OrderType)Convert.ToInt16((dr["document_type"]));

                            order.SubType = Convert.ToByte((dr["document_subtype"]));

                            if (dr["amount"] != DBNull.Value)
                                order.Amount = Convert.ToDouble(dr["amount"]);

                            if (dr["currency"] != DBNull.Value)
                                order.Currency = dr["currency"].ToString();


                            order.SubType = Convert.ToByte(dr["document_subtype"]);

                            order.OrderNumber = dr["document_number"].ToString();

                            if (dr["description"] != DBNull.Value)
                                order.Description = Utility.ConvertAnsiToUnicode(dr["description"].ToString());

                            if (dr["card_number"] != DBNull.Value)
                                order.CardNumber = dr["card_number"].ToString();

                            if (dr["authorization_code"] != DBNull.Value)
                                order.AuthorizationCode = dr["authorization_code"].ToString();

                            if (dr["debet_account"] != DBNull.Value)
                                order.PosAccount = Account.GetSystemAccount(dr["debet_account"].ToString());

                            if (dr["credit_account"] != DBNull.Value)
                                order.CreditAccount = Account.GetSystemAccount(dr["credit_account"].ToString());

                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);


                        }
                        else
                        {
                            order = null;
                        }
                    }
                }
            }
            return order;
        }
    }
}
