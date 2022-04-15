using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace ExternalBanking.DBManager
{
    internal class BondAmountChargeOrderDB
    {
        internal static ActionResult SaveBondAmountChargeOrder(BondAmountChargeOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_save_bond_amount_charge_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    //cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@amount", SqlDbType.Money).Value = order.Amount;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@bond_id", SqlDbType.Float).Value = order.Bond.ID;
                    cmd.Parameters.Add("@charge_date", SqlDbType.SmallDateTime).Value = order.Bond.AmountChargeDate;
                    cmd.Parameters.Add("@charge_time", SqlDbType.Time, 7).Value = order.Bond.AmountChargeTime;
                    cmd.Parameters.Add("@isCashInTransit", SqlDbType.Bit).Value = Convert.ToBoolean(order.IsCashInTransit);


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();


                    int id = Convert.ToInt32(cmd.Parameters["@id"].Value);
                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);


                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = id;
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

        internal static bool ExistsNotConfirmedBondAmountChargeOrder(ulong customerNumber, byte subType, int bondId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @" IF EXISTS(SELECT 1
					                                                            FROM tbl_hb_documents D inner join tbl_bond_amount_charge_order_details B on D.doc_id = B.doc_id
                                                                                WHERE document_type = @document_type AND quality = 3 AND customer_number = @customer_number 
                                                                                                     and document_subtype = @document_sub_type and B.bond_id = @bond_id) 
		                                                            BEGIN
					                                                            SELECT 1 result 
		                                                            END
                                                            ELSE 
		                                                            SELECT 0 resul ";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@document_type", SqlDbType.SmallInt).Value = OrderType.BondQualityUpdateOrder;
                    cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = subType;
                    cmd.Parameters.Add("@bond_id", SqlDbType.Int).Value = bondId;


                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        internal static void GetBondAmountChargeOrder(BondAmountChargeOrder order)
        {
            DataTable dt = new DataTable();
            string sql = @"SELECT 
                                                hb.customer_number,
                                                hb.registration_date,
                                                hb.document_type,
                                                hb.document_number,
                                                hb.document_subtype,
                                                hb.quality,
                                                hb.currency, 
                                                hb.source_type,
                                                hb.operationFilialCode,
                                                hb.operation_date,
                                                od.bond_id, 
			                                    od.charge_date,
			                                    od.charge_time,
                                                od.is_Cash_Transit
                                    FROM Tbl_HB_documents hb
                                                INNER JOIN tbl_bond_amount_charge_order_details od  ON hb.doc_ID = od.doc_id 
                                    WHERE hb.Doc_ID = @DocID AND hb.customer_number = CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END ";


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();

                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.Source = (SourceType)Convert.ToInt16(dt.Rows[0]["source_type"]);
                    order.Bond = new Bond();
                    order.Bond.ID = Convert.ToInt32(dt.Rows[0]["bond_id"]);
                    order.Bond.AmountChargeDate = dt.Rows[0]["charge_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["charge_date"]) : default(DateTime);
                    order.Bond.AmountChargeTime = dt.Rows[0]["charge_time"] != DBNull.Value ? (TimeSpan)dt.Rows[0]["charge_time"] : default(TimeSpan);
                    order.IsCashInTransit = dt.Rows[0]["is_Cash_Transit"] != DBNull.Value ? Convert.ToInt16(dt.Rows[0]["is_Cash_Transit"]) : default(short?);
                }
            }
        }


    }
}