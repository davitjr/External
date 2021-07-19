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
    class CardReReleaseDB
    {
        internal static CardReReleaseOrder GetCardReReleaseOrder(CardReReleaseOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select  d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		                                        d.operation_date,V.visa_number,d.order_group_id
                                                  from Tbl_HB_documents d left join Tbl_Visa_Numbers_Accounts V on d.document_number = V.app_id

                                                  where d.Doc_ID=@DocID ", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                dt.Load(cmd.ExecuteReader());
                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);           
                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.Card = Card.GetCard(dt.Rows[0]["visa_number"].ToString(), Convert.ToUInt64(dt.Rows[0]["customer_number"].ToString()));
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

            }
            return order;
        }
    }
}
