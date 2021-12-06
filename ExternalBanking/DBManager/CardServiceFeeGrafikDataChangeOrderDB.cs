using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class CardServiceFeeGrafikDataChangeOrderDB
    {
        internal static ActionResult SaveCardServiceFeeGrafikDataChangeOrder(CardServiceFeeGrafikDataChangeOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString());
            conn.Open();

            using SqlCommand cmd = new SqlCommand(@"
                                                    declare @filial as int
                                                    SELECT @filial=filialcode FROM Tbl_customers WHERE customer_number=@customer_number   
                                                    INSERT INTO Tbl_HB_documents
                                                    (filial,customer_number,registration_date,document_type,
                                                    document_number,document_subtype,quality, 
                                                    source_type,operationFilialCode,operation_date)
                                                    values
                                                    (@filial,@customer_number,@reg_date,@doc_type,@doc_number,@doc_sub_type,
                                                    1,@source_type,@operation_filial_code,@oper_day)
                                                    Select Scope_identity() as ID
                                                     ", conn);
            cmd.CommandType = CommandType.Text;


            cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
            cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
            cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
            cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
            cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
            cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
            cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
            cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
            cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

            order.Id = Convert.ToInt64(cmd.ExecuteScalar());
            result.ResultCode = ResultCode.Normal;
            return result;

        }

        internal static void SaveCardServiceFeeGrafikDataChangeOrderNewGrafik(CardServiceFeeGrafikDataChangeOrder order)
        {
            order.CardServiceFeeGrafik.ForEach(m =>
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_card_service_fee_grafik_data_change_order_details
                                                            (Doc_ID, product_app_Id, card_number, Service_Fee,currency, start_date,end_date)
                                                            VALUES 
                                                            (@Doc_ID, @product_app_Id, @card_number, @Service_Fee,@currency, @start_date,@end_date)", conn))
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Float).Value = order.Id;
                        cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductAppId;
                        cmd.Parameters.Add("@card_number", SqlDbType.NVarChar, 16).Value = order.CardNumber;
                        cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = m.Currency;
                        cmd.Parameters.Add("@Service_Fee", SqlDbType.Float).Value = m.ServiceFee;
                        cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = m.PeriodStart;
                        cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = m.PeriodEnd;
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        internal static CardServiceFeeGrafikDataChangeOrder GetCardServiceFeeDataChangeOrder(CardServiceFeeGrafikDataChangeOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT   
                                            filial,
                                            customer_number,
                                            registration_date,
                                            document_type,
                                            document_number,
                                            document_subtype,
                                            quality, 
                                            source_type,
                                            operationFilialCode,
                                            operation_date
                                            from Tbl_HB_documents hb 
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
                conn.Open();
                 SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                }
                sqlString = @"SELECT * FROM Tbl_card_service_fee_grafik_data_change_order_details WHERE Doc_id=@docID";
                cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                order.CardServiceFeeGrafik = new List<CardServiceFeeGrafik>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    CardServiceFeeGrafik cardServiceFeeGrafik = CardDB.SetCardServiceFeeGrafik(row);
                    order.CardServiceFeeGrafik.Add(cardServiceFeeGrafik);
                }
            }
            return order;
        }


    }
}
