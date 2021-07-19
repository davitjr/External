using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class BOOrderCustomerDB
    {
        internal static ActionResult Save(BOOrderCustomer orderCustomer, short userId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Save_BO_order_customers";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = orderCustomer.OrderId;
                    cmd.Parameters.Add("@type", SqlDbType.Float).Value = orderCustomer.Type;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = orderCustomer.CustomerNumber;
                    cmd.Parameters.Add("@customerName", SqlDbType.NVarChar, 500).Value = orderCustomer.CustomerName;
                    cmd.Parameters.Add("@documentNumber", SqlDbType.NVarChar,250).Value = orderCustomer.DocumentNumber;
                    cmd.Parameters.Add("@documentType", SqlDbType.SmallInt).Value = orderCustomer.documentType;                    
                    cmd.Parameters.Add("@action", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@actionSetNumber", SqlDbType.SmallInt).Value = userId;

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.TinyInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

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

                    return result;
                }
            }
        }

        internal static List<BOOrderCustomer> GetAccountCustomers(string accountNumber, ulong orderId, OrderCustomerType orderCustomerType)
        {
            List<BOOrderCustomer> paymentCustomers = new List<BOOrderCustomer>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                string sql = "";


                sql = @"SELECT @OrderId orderId, " + (short)orderCustomerType + @" type, A.Customer_number,dbo.fnc_convertAnsiToUnicode(CASE WHEN D.type_of_client = 6 THEN D.lastName + ' ' + D.name ELSE D.Description END) CustomerName,CASE WHEN D.type_of_client = 6 THEN d.Passport_Number ELSE D.code_of_tax END documentNumber, D.passport_type documentType FROM V_All_Accounts A
	                    INNER JOIN V_CustomerDesriptionDocs D ON A.customer_number = D.Customer_number WHERE arm_number = @accountNumber";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        BOOrderCustomer oneOrderCustomer = new BOOrderCustomer();
                        oneOrderCustomer.OrderId = ulong.Parse(row["orderId"].ToString());
                        oneOrderCustomer.Type = short.Parse(row["type"].ToString());
                        oneOrderCustomer.CustomerNumber = ulong.Parse(row["Customer_number"].ToString());
                        oneOrderCustomer.CustomerName = row["CustomerName"].ToString();
                        oneOrderCustomer.DocumentNumber = row["documentNumber"] == DBNull.Value ? "" : row["documentNumber"].ToString();
                        oneOrderCustomer.documentType = row["documentType"] == DBNull.Value ? 0 : int.Parse(row["documentType"].ToString());

                        paymentCustomers.Add(oneOrderCustomer);
                    }
                }

            }

            return paymentCustomers;
        }
    }
}
