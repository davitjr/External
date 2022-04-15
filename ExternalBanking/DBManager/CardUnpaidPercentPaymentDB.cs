using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class CardUnpaidPercentPaymentOrderDB
    {
        internal static ActionResult Save(CardUnpaidPercentPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_add_unpaid_percent_payment_order";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@customer_number", (double)order.CustomerNumber);//
                    cmd.Parameters.AddWithValue("@registration_date", order.RegistrationDate);//
                    cmd.Parameters.AddWithValue("@document_type", OrderType.CardUnpayedPercentPayment);//
                    cmd.Parameters.AddWithValue("@operation_filial_code", (short)order.FilialCode);//
                    cmd.Parameters.AddWithValue("@document_number", order.OrderNumber);//
                    if (order.Card.Currency.Equals("AMD"))
                    {
                        cmd.Parameters.AddWithValue("@amount", Math.Round(order.Card.PositiveRate, 1));
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@amount", Math.Round(order.Card.PositiveRate, 2));
                    }
                    cmd.Parameters.AddWithValue("@currency", order.Card.Currency);
                    cmd.Parameters.AddWithValue("@app_ID", (double)order.Card.ProductId);
                    cmd.Parameters.AddWithValue("@credit_account", order.Account.AccountNumber);
                    cmd.Parameters.AddWithValue("@source_type", (short)source);//
                    cmd.Parameters.AddWithValue("@document_subtype", order.SubType);//
                    cmd.Parameters.AddWithValue("@username", userName);//
                    cmd.Parameters.AddWithValue("@oper_day", order.OperationDate);
                    cmd.Parameters.Add("@ID", SqlDbType.BigInt);
                    cmd.Parameters["@ID"].Direction = ParameterDirection.Output;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@ID"].Value);
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;
                    result.ResultCode = ResultCode.Normal;
                }
            }
            return result;
        }

        internal static CardUnpaidPercentPaymentOrder getCardUnpaidPercentPaymentOrder(CardUnpaidPercentPaymentOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand("pr_get_unpaid_percent_payment_order", conn);
                cmd.Parameters.AddWithValue("@ID", order.Id);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    order.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                    order.Card = Card.GetCard(Convert.ToUInt64(dr["App_ID"]), order.CustomerNumber);
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"]);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"]);
                    order.OrderNumber = dr["document_number"].ToString();
                    order.Amount = Convert.ToDouble(dr["amount"]);
                    order.Currency = dr["currency"].ToString();
                    order.Account = Account.GetAccount(dr["credit_account"].ToString(), order.CustomerNumber);
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"]);
                    order.Quality = (OrderQuality)dr["quality"];
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                }
            }
            return order;
        }
    }
}
