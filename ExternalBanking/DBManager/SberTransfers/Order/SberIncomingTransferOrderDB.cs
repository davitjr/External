using ExternalBanking.SberTransfers.Order;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.SberTransfers.Order
{
    internal static class SberIncomingTransferOrderDB
    {
        internal static ActionResult SaveSberIncomingTransferOrder(SberIncomingTransferOrder order)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_sberbank_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@receiverFIO", SqlDbType.NVarChar, 250).Value = order.ReceiverFIO;
                    cmd.Parameters.Add("@senderFIO", SqlDbType.NVarChar, 250).Value = order.SenderFIO;
                    cmd.Parameters.Add("@creditAccount", SqlDbType.BigInt).Value = order.CreditAccount.AccountNumber;
                    cmd.Parameters.Add("@currencyRateCrossBuy", SqlDbType.Float).Value = order.CurrencyRateCrossBuy;
                    cmd.Parameters.Add("@currencyRateCrossSell", SqlDbType.Float).Value = order.CurrencyRateCrossSell;
                    cmd.Parameters.Add("@currencyCrossRateFull", SqlDbType.Float).Value = order.CurrencyCrossRateFull;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@creditCurrency", SqlDbType.NVarChar, 3).Value = order.CreditCurrency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@creditAmount", SqlDbType.Float).Value = order.CreditAmount;
                    cmd.Parameters.Add("@transferID", SqlDbType.BigInt).Value = order.TransferId;
                    cmd.Parameters.Add("@payID", SqlDbType.BigInt).Value = order.PayId;
                    cmd.Parameters.Add("@payDate", SqlDbType.DateTime).Value = order.PaymentDateTime;

                    SqlParameter docId = new SqlParameter("@doc_ID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    SqlParameter regDate = new SqlParameter("@regDate", SqlDbType.DateTime)
                    {
                        Value = order.RegistrationDate,
                        Direction = ParameterDirection.InputOutput
                    };
                    cmd.Parameters.Add(docId);
                    cmd.Parameters.Add(regDate);

                    cmd.ExecuteNonQuery();

                    order.Id = Convert.ToInt64(cmd.Parameters["@doc_ID"].Value);
                    order.RegistrationDate = Convert.ToDateTime(cmd.Parameters["@regDate"].Value);
                    order.Quality = OrderQuality.Draft;

                    result.ResultCode = ResultCode.Normal;
                    result.Id = order.Id;
                }
            }
            return result;
        }
    }
}
