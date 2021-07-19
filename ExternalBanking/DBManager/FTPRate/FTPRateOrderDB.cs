using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    static class FTPRateOrderDB
    {
        /// <summary>
        /// FTP տոկոսադրույքի հայտի  Get
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static FTPRateOrder GetFTPRateOrder(FTPRateOrder order)
        {
            order.FTPRate = new FTPRate();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
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

                                                        INNER JOIN Tbl_FTP_rate_order_details p ON hb.doc_ID = p.doc_id
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

                        order.FTPRate.rateType = (FTPRateType)byte.Parse(dt.Rows[0]["rate_type"].ToString());
                        //order.Fond.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["fond_description"].ToString());
                        //order.Fond.IsActive = byte.Parse(dt.Rows[0]["is_active"].ToString());
                        //order.Fond.IsSubsidia = byte.Parse(dt.Rows[0]["is_subsidia"].ToString());
                        order.FTPRate.FTPRateDetails = GetFTPRateOrderDetails(order.Id);
                    }
                }

            }
            return order;
        }

        private static List<FTPRateDetail> GetFTPRateOrderDetails(long orderID)
        {
            List<FTPRateDetail> rateDetails = new List<FTPRateDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM Tbl_FTP_rate_order_details WHERE Doc_ID = @orderID";
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

                        FTPRateDetail rateDetail = SetFTPRateOrderDetail(row);

                        rateDetails.Add(rateDetail);
                    }

                }

            }

            return rateDetails;
        }

        private static FTPRateDetail SetFTPRateOrderDetail(DataRow row)
        {
            FTPRateDetail rateDetail = new FTPRateDetail();

            if (row != null)
            {
                rateDetail.Currency = row["currency"].ToString();
                if (row["interest_rate"] != DBNull.Value)
                    rateDetail.InterestRate = float.Parse(row["interest_rate"].ToString());

            }

            return rateDetail;
        }
    }
}
