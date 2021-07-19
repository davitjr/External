using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class BOOrderPaymentAdditionDB
    {
        internal static ActionResult Save(BOOrderPaymentAddition orderPaymentAddition)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_order_payments_additions(order_payment_id, debet_account, credit_account) VALUES(@OrderPaymentId, @DebetAccount, @CreditAccount)";
                    //cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@OrderPaymentId", SqlDbType.Float).Value = orderPaymentAddition.BOOrderPaymentId;
                    cmd.Parameters.Add("@DebetAccount", SqlDbType.BigInt).Value = orderPaymentAddition.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@CreditAccount", SqlDbType.BigInt).Value = orderPaymentAddition.CreditAccount.AccountNumber;
                    
                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }
    }
}
