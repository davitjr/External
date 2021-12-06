using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal class DepositDataChangeOrderDB
    {

        internal static ActionResult SaveDepositDataChangeOrder(DepositDataChangeOrder order, string userName)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_deposit_data_change_order";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_sub_type", SqlDbType.Int).Value = order.SubType;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)order.Source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@field_value", SqlDbType.VarChar, 50).Value = order.FieldValue;
                    cmd.Parameters.Add("@field_type", SqlDbType.SmallInt).Value = order.FieldType;
                    cmd.Parameters.Add("@product_app_Id", SqlDbType.Float).Value = order.Deposit.ProductId;

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


        internal static DepositDataChangeOrder GetDepositDataChangeOrder(DepositDataChangeOrder order)
        {
            DataTable dt = new DataTable();
            order.Deposit = new Deposit();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"
                                                SELECT                                   
                                                d.registration_date,
                                                d.document_number,
                                                d.customer_number,
                                                d.document_type,
                                                d.document_subtype,
                                                d.quality,
                                                d.operation_date ,
                                                n.product_app_id,
                                                n.field_type,
                                                n.field_value
                                                FROM Tbl_deposit_data_change_order_details  n
                                                inner join Tbl_HB_documents  d
                                                on  d.doc_ID=n.Doc_ID
                                                where n.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);


                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.FieldType= Convert.ToInt16(dt.Rows[0]["field_type"]);
                order.FieldValue = dt.Rows[0]["field_value"].ToString();
                order.FieldTypeDescription = Info.GetDepositDataChangeFieldTypeDescription((ushort)order.FieldType);
                order.Deposit.ProductId = Convert.ToInt64(dt.Rows[0]["product_app_id"]);
                order.CustomerNumber= Convert.ToUInt64(dt.Rows[0]["customer_number"]);


            }
            return order;
        }



    }
}
