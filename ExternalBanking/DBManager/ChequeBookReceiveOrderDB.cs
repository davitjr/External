using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    public class ChequeBookReceiveOrderDB
    {

        /// <summary>
        /// Չեկային գրքույքի ստացման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(ChequeBookReceiveOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Cheque_Book_Receive_Order";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@debet_account", SqlDbType.Float).Value = Convert.ToDouble(order.ChequeBookAccount.AccountNumber);
                    //cmd.Parameters.Add("@deb_for_transfer_payment", SqlDbType.Float).Value =order.FeeAccount.AccountNumber;
                    //cmd.Parameters.Add("@service_fee", SqlDbType.Float).Value = order.FeeAmount;
                    cmd.Parameters.Add("@cost_price", SqlDbType.Float).Value = order.CostPrice;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@page_number_start", SqlDbType.NVarChar, 50).Value = order.PageNumberStart;
                    cmd.Parameters.Add("@page_number_end", SqlDbType.NVarChar, 50).Value = order.PageNumberEnd;
                    cmd.Parameters.Add("@source_type", SqlDbType.TinyInt).Value = (byte)source;
                    cmd.Parameters.Add("@doc_type", SqlDbType.SmallInt).Value = (Int16)order.Type;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@receiverName", SqlDbType.NVarChar, 50).Value = order.PersonFullName;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Draft;
                }
                return result;
            }
        }


        /// <summary>
        /// Վերադարձնում է չեկային գրքույքի ստացման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>

        internal static ChequeBookReceiveOrder Get(ChequeBookReceiveOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT registration_date,
                                                         doc.document_number,
                                                         doc.document_type,
                                                         doc.debet_account,
                                                         doc.deb_for_transfer_payment,
                                                         doc.source_type,
                                                         doc.quality,
                                                         doc.document_subtype ,
                                                         doc.registration_date,
                                                         det.page_number_start,
                                                         det.page_number_end,
                                                         doc.operation_date   
                                                         
                                                         FROM Tbl_HB_documents as doc
                                                         INNER JOIN Tbl_Cheque_Book_Receive_Order_Details as det
                                                         on doc.doc_ID=det.doc_ID
                                                         WHERE doc.customer_number=case when @customer_number = 0 then doc.customer_number else @customer_number end AND doc.doc_ID=@DocID", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                order.FeeAccount = new Account();
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.PageNumberStart = dt.Rows[0]["page_number_start"].ToString();
                    order.PageNumberEnd = dt.Rows[0]["page_number_end"].ToString();
                    order.ChequeBookAccount = Account.GetAccount(dt.Rows[0]["debet_account"].ToString());
                    order.FeeAccount.AccountNumber = dt.Rows[0]["deb_for_transfer_payment"].ToString();
                    order.ChequeBookAccount.AccountTypeDescription = Utility.ConvertAnsiToUnicode(order.ChequeBookAccount.AccountTypeDescription);

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.Type = (OrderType)dt.Rows[0]["document_type"];
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

                }


            }
            return order;

        }

        /// <summary>
        /// Ստուգում է չեկային գրքույկի պատվիրման հայտ առկա է թե ոչ
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static bool HasChequeBookOrder(ulong customerNumber, string accountNumber)
        {
            string result = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"if ( select COUNT(customer_number) from [Tbl_HB_documents]  where customer_number=@customer_number and debet_account=@accountNumber and document_type=22 and quality=30)!=0
                                        select 1;
                                        else
                                        select 0";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    result = cmd.ExecuteScalar().ToString();
                }
            }
            if (result == "1")
                return true;
            else
                return false;
        }

        internal static double GetOrderServiceFee(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT TOP 1 amount_for_payment                                                       
                                        FROM Tbl_HB_documents                                                        
                                        WHERE customer_number=@customer_number AND document_type=22 AND quality=30
                                        ORDER BY doc_ID DESC";
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    return Convert.ToDouble(cmd.ExecuteScalar());
                }
            }

        }



    }
}
