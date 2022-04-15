using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class InterestMarginOrderDB
    {
        internal static InterestMarginOrder GetInterestMarginOrder(InterestMarginOrder order)
        {
            order.InterestMargin = new InterestMargin();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT   hb.customer_number,
                                                    hb.registration_date,
                                                    hb.document_type,
                                                    hb.document_number,
                                                    hb.document_subtype,
                                                    hb.quality,
                                                    hb.currency, 
                                                    hb.source_type,
                                                    hb.operationFilialCode,
                                                    hb.operation_date,
                                                    p.*

			                        FROM Tbl_HB_documents hb

                                                        INNER JOIN Tbl_Interest_Margin_order_details p ON hb.doc_ID = p.doc_id
                                    WHERE hb.Doc_ID = @DocID ";
                    cmd.Parameters.Add("@DocID", SqlDbType.Float).Value = order.Id;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                        order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                        order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                        order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                        order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                        order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                        order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);

                        order.InterestMargin.marginType = (InterestMarginType)byte.Parse(dt.Rows[0]["margin_type"].ToString());
                        order.InterestMargin.marginDate = Convert.ToDateTime(dt.Rows[0]["margin_date"]);

                        order.InterestMargin.marginDetails = GetInterestMarginOrderDetails(order.Id);
                    }
                }
            }
            return order;
        }

        private static List<InterestMarginDetail> GetInterestMarginOrderDetails(long orderID)
        {
            List<InterestMarginDetail> marginDetails = new List<InterestMarginDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM Tbl_Interest_Margin_Order_Details WHERE Doc_ID = @orderID";
                    cmd.Parameters.Add("@orderID", SqlDbType.Float).Value = orderID;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        InterestMarginDetail marginDetail = SetInterestMarginOrderDetail(row);

                        marginDetails.Add(marginDetail);
                    }

                }

            }

            return marginDetails;
        }

        private static InterestMarginDetail SetInterestMarginOrderDetail(DataRow row)
        {
            InterestMarginDetail marginDetail = new InterestMarginDetail();

            if (row != null)
            {
                marginDetail.Currency = row["currency"].ToString();
                if (row["interest_rate"] != DBNull.Value)
                    marginDetail.InterestRate = float.Parse(row["interest_rate"].ToString());
            }

            return marginDetail;
        }
    }
}
