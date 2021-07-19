using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
   internal class OrderForCashRegisterDB
    {
        internal static List<OrderForCashRegister> GetOrdersForCashRegister(SearchOrders searchOrders)
        {
            List<OrderForCashRegister> orders = new List<OrderForCashRegister>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = "pr_GetOrdersForCashRegister";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FilialCode", SqlDbType.Int).Value = searchOrders.OperationFilialCode;
                    cmd.Parameters.Add("@CustomerNumber", SqlDbType.Float).Value = searchOrders.CustomerNumber == 0 ? DBNull.Value : (object)searchOrders.CustomerNumber;
                    cmd.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = searchOrders.LastName;
                    cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar).Value = searchOrders.FirstName;
                    cmd.Parameters.Add("@MiddleName", SqlDbType.NVarChar).Value = searchOrders.MiddleName;
                    cmd.Parameters.Add("@OrgName", SqlDbType.NVarChar).Value = searchOrders.OrgName;
                    cmd.Parameters.Add("@DocumentNumber", SqlDbType.NVarChar).Value = searchOrders.DocumentNumber;
                    cmd.Parameters.Add("@AccountNumber", SqlDbType.NVarChar).Value = searchOrders.AccountNumber;
                    cmd.Parameters.Add("@CardNumber", SqlDbType.NVarChar).Value = searchOrders.CardNumber;
                    cmd.Parameters.Add("@LoanNumber", SqlDbType.NVarChar).Value = searchOrders.LoanNumber;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = searchOrders.Id == 0 ? DBNull.Value : (object)searchOrders.Id;
                    cmd.Parameters.Add("@CurrentState", SqlDbType.Int).Value = searchOrders.CurrentState == 0 ? DBNull.Value : (object)searchOrders.CurrentState;
                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = searchOrders.Amount == 0 ? DBNull.Value : (object)searchOrders.Amount;
                    cmd.Parameters.Add("@DateFrom", SqlDbType.DateTime).Value = searchOrders.DateFrom;
                    cmd.Parameters.Add("@DateTo", SqlDbType.DateTime).Value = searchOrders.DateTo;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            OrderForCashRegister CRorder = new OrderForCashRegister();
                            CRorder.Order.Fees = new List<OrderFee>();

                            CRorder.Order.Id = long.Parse(dr["doc_ID"].ToString());
                            CRorder.Order.Quality = (OrderQuality)short.Parse(dr["Quality"].ToString());
                            CRorder.Order.CustomerNumber = ulong.Parse(dr["customerNumber"].ToString());
                            CRorder.Order.Currency = dr["currency"].ToString();
                            CRorder.Order.RegistrationDate = DateTime.Parse(dr["RegistrationDate"].ToString());
                            CRorder.UserID = short.Parse(dr["UserID"].ToString());
                            CRorder.Order.OrderNumber = dr["OrderNumber"].ToString();
                            CRorder.Order.Amount = double.Parse(dr["Amount"].ToString());
                            CRorder.TypeDescription = dr["TypeDescription"].ToString();
                            CRorder.AccountNumber = dr["AccountNumber"].ToString();
                            CRorder.AccountNumberDescription = dr["AccountNumberDescription"].ToString();
                            CRorder.Order.Description = dr["description"].ToString();
                            CRorder.FullName = dr["FullName"].ToString();
                            CRorder.DocumentNumber = dr["DocumentNumber"].ToString();
                            CRorder.Order.Fees.Add(new OrderFee() { Amount = double.Parse(dr["FeeAmount"].ToString()), Currency = dr["FeeCurrency"].ToString() });

                            orders.Add(CRorder);
                        }
                    }
                }
            }

            return orders;
        }
    }
}
