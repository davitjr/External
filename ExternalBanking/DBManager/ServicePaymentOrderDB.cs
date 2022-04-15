using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public static class ServicePaymentOrderDB
    {

        internal static ActionResult Save(ServicePaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_ServiceFeePayment";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;


                    if (order.Type == OrderType.AccountServicePaymentXnd || order.Type == OrderType.HBServicePaymentXnd)
                        cmd.Parameters.Add("@accountNumberForPayment", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;

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
        public static double ServicePaymentPreparation(ulong customerNumber, double debt, DateTime setDate, string credit_account, bool provisionSign)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("[dbo].[sp_service_payment_preparation]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@customer_debt", SqlDbType.Float).Value = debt;
                cmd.Parameters.Add("@oper_date", SqlDbType.SmallDateTime).Value = setDate;
                cmd.Parameters.Add("@cur_acc_amd", SqlDbType.BigInt).Value = credit_account;
                cmd.Parameters.Add("@provision_sign", SqlDbType.Bit).Value = provisionSign;
                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                    double sum = 0;
                    foreach (DataRow row in dt.Rows)
                        sum += (double)row["Amount"] * (double)row["Kurs"];
                    return sum;
                }
            }
        }

    }
}


