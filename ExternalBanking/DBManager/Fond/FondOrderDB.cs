using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;


namespace ExternalBanking.DBManager
{
    static class FondOrderDB
    {

       internal static int GenerateNextFondID()
        {
            int newID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT MAX(code) + 1 newID 
                                                                          FROM [tbl_fonds;]", conn);
                cmd.CommandType = CommandType.Text;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    newID = Convert.ToUInt16(dr["newID"]);
                }

            }

            return newID;
        }

        /// <summary>
        /// Պարտատոմսի հայտի  Get
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static FondOrder GetFondOrder(FondOrder order)
        {
            order.Fond = new Fond();
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
                                                    f.*,
                                                    p.*

			                        FROM Tbl_HB_documents hb
                                                        INNER JOIN tbl_fond_order_details f ON hb.doc_ID = f.doc_id
                                                        INNER JOIN Tbl_fond_providing_order_details p ON hb.doc_ID = p.doc_id
                                    WHERE hb.Doc_ID = @DocID ";
                    cmd.Parameters.Add("@DocID", SqlDbType.Float).Value = order.Id ;
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

                        order.Fond.ID = int.Parse(dt.Rows[0]["fond_ID"].ToString());
                        order.Fond.Description = Utility.ConvertAnsiToUnicode(dt.Rows[0]["fond_description"].ToString());
                        //order.Fond.IsActive = byte.Parse(dt.Rows[0]["is_active"].ToString());
                        //order.Fond.IsSubsidia = byte.Parse(dt.Rows[0]["is_subsidia"].ToString());
                        order.Fond.ProvidingDetails = GetFondProvidingOrderDetails(order.Id);
                    }
                }

            }
            return order;
        }

        private static List<FondProvidingDetail> GetFondProvidingOrderDetails(long orderID)
        {
            List<FondProvidingDetail> providingDetails = new List<FondProvidingDetail>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM Tbl_fond_providing_order_details WHERE Doc_ID = @orderID";
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

                        FondProvidingDetail providingDetail = SetFondProvidingOrderDetail(row);

                        providingDetails.Add(providingDetail);
                    }

                }

            }

            return providingDetails;
        }

        private static FondProvidingDetail SetFondProvidingOrderDetail(DataRow row)
        {
            FondProvidingDetail providingDetail = new FondProvidingDetail();

            if (row != null)
            {
                providingDetail.Id = ulong.Parse(row["ID"].ToString());
                providingDetail.FondID = int.Parse(row["fond_ID"].ToString());
                providingDetail.Currency = row["currency"].ToString();
                if (row["filial_interest_rate"] != DBNull.Value)
                    providingDetail.InterestRate = float.Parse(row["filial_interest_rate"].ToString());
                if (row["providing_termination_date"] != DBNull.Value)
                    providingDetail.TerminationDate = Convert.ToDateTime(row["providing_termination_date"]);
            }

            return providingDetail;
        }
    }
}
