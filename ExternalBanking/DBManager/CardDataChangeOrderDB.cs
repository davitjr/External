using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    static class CardDataChangeOrderDB
    {
        internal static ActionResult SaveCardDataChangeOrder(CardDataChangeOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_card_data_change_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.ProductAppId;

                    cmd.Parameters.Add("@field_type", SqlDbType.SmallInt).Value = order.FieldType;
                    cmd.Parameters.Add("@field_value", SqlDbType.NVarChar, 50).Value = order.FieldValue;
                    cmd.Parameters.Add("@document_number", SqlDbType.NVarChar, 100).Value = order.DocumentNumber;
                    cmd.Parameters.Add("@document_date", SqlDbType.SmallDateTime).Value = order.DocumentDate;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);

                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;

                    return result;
                }
            }

        }


        internal static CardDataChangeOrder GetCardServiceFeeDataChangeOrder(CardDataChangeOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sqlString = @"SELECT   
                                            hb.filial,
                                            hb.customer_number,
                                            hb.registration_date,
                                            hb.document_type,
                                            hb.document_number as hb_document_number,
                                            hb.document_subtype,
                                            hb.quality,
                                            hb.source_type,
                                            hb.operationFilialCode,
                                            hb.operation_date,
                                            ch.*
                                            from Tbl_HB_documents hb inner join tbl_card_data_change_order_details ch
                                            on ch.doc_id=hb.doc_ID
                                            WHERE hb.doc_ID=@docID AND hb.customer_number=case WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";
                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();


                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["hb_document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);
                    order.FilialCode = Convert.ToUInt16(dr["operationFilialCode"].ToString());
                    order.FieldType = Convert.ToInt16(dr["field_type"].ToString());
                    order.ProductAppId = Convert.ToInt64(dr["product_app_Id"].ToString());
                    order.FieldValue = dr["field_value"] != DBNull.Value ? dr["field_value"].ToString() : null;
                    order.DocumentDate = dr["document_date"] != DBNull.Value ? Convert.ToDateTime(dr["document_date"]) : default(DateTime?);
                    order.DocumentNumber = dr["document_number"] != DBNull.Value ? dr["document_number"].ToString() : null;
                    order.FieldTypeDescription = Info.GetCardDataChangeFieldTypeDescription((ushort)order.FieldType);
                }


            }
            return order;
        }


        public static bool CheckFieldTypeIsRequaried(short fieldType)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT required FROM Tbl_card_data_change_field_types
                                                  WHERE  field_type=@fieldType", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@fieldType", SqlDbType.Float).Value = fieldType;

                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        internal static List<CardDataChangeOrder> GetCardDataChangesByProduct(long ProductAppId, short FieldType)
        {
            List<CardDataChangeOrder> cardDataChangeOrders = new List<CardDataChangeOrder>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sqlString = @"select *
                                        from (
			                                        select a.Change_Id, a.New_value field_value, a.Change_date registration_date, a.Set_Id change_set_number, b.document_number document_number
			                                        from Tbl_short_time_loans_changes_details a 
			                                        left join Tbl_short_time_loans_changes_reasons b on a.Change_Id=b.change_id
			                                        where Field_name='related_office_number' and a.app_id= @productAppId 

			                                        UNION ALL

			                                        select 0 Change_Id, M.Old_value field_value, NULL registration_date, NULL change_set_number, '' document_number
			                                        from Tbl_short_time_loans_changes_details M
			                                        left join Tbl_short_time_loans_changes_reasons b on M.Change_Id=b.change_id
			                                        inner join (
								                                        select MIN(a.Change_Id) Change_Id
								                                        from Tbl_short_time_loans_changes_details a 
								                                        left join Tbl_short_time_loans_changes_reasons b on a.Change_Id=b.change_id
								                                        where Field_name='related_office_number' and a.app_id= @productAppId
								                                        group by a.app_id ) MC on MC.Change_Id = M.Change_Id ) A
                                        order by A.Change_Id desc";

                using SqlCommand cmd = new SqlCommand(sqlString, conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@productAppId", SqlDbType.Float).Value = ProductAppId;

                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    CardDataChangeOrder cardDataChangeOrder = new CardDataChangeOrder();
                    cardDataChangeOrder.user = new ACBAServiceReference.User();
                    cardDataChangeOrder.FieldValue = dr["field_value"].ToString();
                    cardDataChangeOrder.RegistrationDate = dr["registration_date"] != DBNull.Value ? Convert.ToDateTime(dr["registration_date"].ToString()) : DateTime.MinValue;
                    cardDataChangeOrder.user.userID = dr["change_set_number"] != DBNull.Value ? short.Parse(dr["change_set_number"].ToString()) : (short)0;
                    cardDataChangeOrder.DocumentNumber = dr["document_number"] != DBNull.Value ? dr["document_number"].ToString() : null;

                    cardDataChangeOrders.Add(cardDataChangeOrder);
                }
            }

            return cardDataChangeOrders;
        }


    }
}
