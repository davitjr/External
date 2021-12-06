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
    internal class ConsumeLoanApplicationOrderDB
    {
        internal static ActionResult SaveConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_consume_loan_application_order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;



                    cmd.Parameters.Add("@Amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@Duration", SqlDbType.Int).Value = order.Duration;

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (short)source;
                    cmd.Parameters.Add("@operation_filial_code", SqlDbType.SmallInt).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ushort actionResult = Convert.ToUInt16(cmd.Parameters["@result"].Value);

                    if (actionResult == 1 || actionResult == 9 || actionResult == 10)
                    {
                        result.ResultCode = ResultCode.Normal;
                        order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                        order.Quality = OrderQuality.Draft;
                        result.Id = order.Id;

                    }
                    else if (actionResult == 0 || actionResult == 8)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                        result.Errors.Add(new ActionError((short)actionResult));
                    }

                    return result;
                }

            }
        }


        internal static ConsumeLoanApplicationOrder GetConsumeLoanApplicationOrder(ConsumeLoanApplicationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT d.amount,d.source_type,d.registration_date,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,n.*,d.operation_date,d.order_group_id,d.confirmation_date                                             
		                                           FROM Tbl_HB_documents as d left join Tbl_Consume_Loan_Application_Order_Details as n on  d.doc_ID=n.Doc_ID                                                 
                                                   WHERE d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);

                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());

                order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                order.Currency = dt.Rows[0]["currency"].ToString();
                order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                order.Duration = Convert.ToInt32(dt.Rows[0]["duration"]);

                order.ProductType = int.Parse(dt.Rows[0]["loan_type"].ToString());

                order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());

                order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);


            }
            return order;
        }


        internal static (long, DateTime) ExistsConsumeLoanApplicationOrder(ulong customerNumber, List<OrderQuality> qualities)
        {
            string qualitiesCondition = "";
            (long, DateTime) result = (0, DateTime.MinValue);
            if (qualities.Count > 0)
            {
                qualitiesCondition = " and quality in (";


                foreach (OrderQuality quality in qualities)
                {
                    qualitiesCondition += (short)quality + ",";
                }

                qualitiesCondition = qualitiesCondition.Substring(0, qualitiesCondition.Length - 1);
                qualitiesCondition += ") ";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@" SELECT doc_id, registration_date
		                                           FROM Tbl_HB_documents                                            
                                                   WHERE customer_number=case when @customer_number = 0 then customer_number else @customer_number end and document_type = 255 " + qualitiesCondition, conn);
                {

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result.Item1 = Convert.ToInt64(dr["doc_id"].ToString());
                            result.Item2 = Convert.ToDateTime(dr["registration_date"]);
                        }
                    }
                }
            }
            return result;
        }

    }
}
