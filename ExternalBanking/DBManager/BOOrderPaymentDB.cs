using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class BOOrderPaymentDB
    {
        internal static ulong Save(BOOrderPayment paymentOrderDetails, short userId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_BO_order_payments";
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = paymentOrderDetails.OrderId;
                    cmd.Parameters.Add("@type", SqlDbType.Float).Value = paymentOrderDetails.Type;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = paymentOrderDetails.Amount;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = paymentOrderDetails.Currency;
                    cmd.Parameters.Add("@mainDebetAccount", SqlDbType.NVarChar,50).Value = paymentOrderDetails.MainDebitAccount;
                    cmd.Parameters.Add("@mainCreditAccount", SqlDbType.NVarChar, 50).Value = paymentOrderDetails.MainCreditAccount;
                    cmd.Parameters.Add("@exchangeRate", SqlDbType.Float).Value = paymentOrderDetails.ExchangeRate;
                    cmd.Parameters.Add("@exchangeRate1", SqlDbType.Float).Value = paymentOrderDetails.ExchangeRate1;                    
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar, 250).Value = (object)paymentOrderDetails.Description ?? DBNull.Value;
                    cmd.Parameters.Add("@feeType", SqlDbType.SmallInt).Value = (object)paymentOrderDetails.FeeType ?? DBNull.Value;
                    cmd.Parameters.Add("@action", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@actionSetNumber", SqlDbType.SmallInt).Value = userId;


                    //SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, 4000);
                    //param.Direction = ParameterDirection.Output;
                    //cmd.Parameters.Add(param);

                    SqlParameter param = new SqlParameter("@Id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    //order.Id = Convert.ToInt64(cmd.Parameters["@orderId"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        //result.Id = paymentOrderDetails.Id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                    return ulong.Parse(param.Value.ToString());
                }
            }
        }
    }
}
