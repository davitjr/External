using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
	class VirtualCardStatusChangeOrderDB
	{
		internal static ActionResult SaveVirtualCardStatusChangeOrder(VirtualCardStatusChangeOrder order, string userName)
		{
			ActionResult result = new ActionResult();
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
			{
				using (SqlCommand cmd = new SqlCommand())
				{

					conn.Open();
					cmd.Connection = conn;
					cmd.CommandText = "pr_virtual_card_status_change_order";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
					cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
					cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
					cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
					cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
					cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
					cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
					cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
					cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
					cmd.Parameters.Add("@productId", SqlDbType.Float).Value = order.ProductId;
					cmd.Parameters.Add("@reason", SqlDbType.TinyInt).Value = order.ChangeReason;
					cmd.Parameters.Add("@reasonAdd", SqlDbType.NVarChar,100).Value = order.ChangeReasonAdd;
					cmd.Parameters.Add("@cardStatus", SqlDbType.NVarChar,50).Value = order.Status;
					cmd.Parameters.Add("@virtualCardID", SqlDbType.NVarChar, 50).Value = order.VirtualCardId;

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


		internal static VirtualCardStatusChangeOrder GetVirtualCardStatusChangeOrder(VirtualCardStatusChangeOrder order)
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
                                            dt.*
                                            from Tbl_HB_documents hb inner join Tbl_virtual_card_status_change_order_details dt
                                            on dt.doc_id=hb.doc_ID
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
					order.ProductId = Convert.ToUInt64(dr["product_Id"].ToString());
					order.ChangeReason = Convert.ToInt16(dr["change_reason"]);
					if (dr["change_reason_add"] != DBNull.Value)
						order.ChangeReasonAdd = dr["change_reason_add"].ToString();
					order.Status = dr["virtual_card_status"].ToString();
					order.VirtualCardId = dr["virtual_card_id"].ToString();
					
				}


			}
			return order;
		}
	}
}
