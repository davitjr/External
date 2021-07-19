using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class BOOrderProductDB
    {
        /// <summary>
        /// Հանձնարարականին կցված պրոդուկտի պահպանում:
        /// </summary>                        
        /// <returns></returns>
        internal static ActionResult Save(BOOrderProduct orderProduct)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_order_products(order_id, product_id, product_type) VALUES(@orderId, @productId, @productType)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = orderProduct.OrderId;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = orderProduct.ProductId;
                    cmd.Parameters.Add("@productType", SqlDbType.SmallInt).Value = orderProduct.ProductType;
                    
                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }
    }
}
