using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class BondQualityUpdateOrderDB
    {
        /// <summary>
        /// Պարտատոմսի հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal static ActionResult SaveBondQualityUpdateOrder(BondQualityUpdateOrder order, string userName)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_bond_quality_update_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (int)order.Type;
                    cmd.Parameters.Add("@document_subtype", SqlDbType.SmallInt).Value = order.SubType;


                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = Convert.ToInt32(order.Source);
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@bond_id", SqlDbType.Int).Value = order.BondId;

                    if(order.SubType == 3)
                    {
                        cmd.Parameters.Add("@reason_id", SqlDbType.SmallInt).Value = order.ReasonId;
                        cmd.Parameters.Add("@reason_description", SqlDbType.NVarChar).Value = order.ReasonDescription;
                    }


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);
                    int id = Convert.ToInt32(cmd.Parameters["@id"].Value);


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


        internal static void GetBondQualityUpdateOrder(BondQualityUpdateOrder order)
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
                                        db.bond_id,
                                        db.reason_id,
                                        db.reason_description
			                            FROM Tbl_HB_documents hb
                                        INNER JOIN Tbl_bond_quality_update_order_details DB  ON hb.doc_ID = DB.doc_id 
                            WHERE hb.Doc_ID = @DocID AND hb.customer_number = CASE WHEN @customer_number = 0 THEN hb.customer_number ELSE @customer_number END";


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
                    order.BondId = Convert.ToInt32(dt.Rows[0]["bond_ID"]);
                }
            }
        }

        internal static bool ExistsNotConfirmedBondQualityUpdateOrder(ulong customerNumber, byte subType, int bondId)
        { 
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS(SELECT 1 FROM tbl_hb_documents D inner join Tbl_bond_quality_update_order_details B on D.doc_id = B.doc_id
                                        WHERE document_type = @document_type AND quality = 3 AND customer_number = @customer_number 
                                        and document_type = @document_type and document_subtype = @document_sub_type and B.bond_id = @bond_id) 
                                        SELECT 1 result ELSE SELECT 0 result";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@document_type", SqlDbType.SmallInt).Value = OrderType.BondQualityUpdateOrder;
                    cmd.Parameters.Add("@document_sub_type", SqlDbType.SmallInt).Value = subType;
                    cmd.Parameters.Add("@bond_id", SqlDbType.Int).Value = bondId;


                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

    }
}
