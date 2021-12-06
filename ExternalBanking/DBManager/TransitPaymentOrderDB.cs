using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace ExternalBanking.DBManager
{
    class TransitPaymentOrderDB
    {
        /// <summary>
        /// Տարանցիկ հաշվին փոխանցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(TransitPaymentOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_AddNewTransitPaymentDocument";
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    cmd.Parameters.Add("@debit_account", SqlDbType.Float).Value = order.DebitAccount.AccountNumber;
                    cmd.Parameters.Add("@transit_account", SqlDbType.VarChar,20).Value = order.TransitAccount.AccountNumber;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.DateTime).Value = order.RegistrationDate.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (int)source;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = order.Amount;
                    cmd.Parameters.Add("@doc_type", SqlDbType.TinyInt).Value = order.Type;
                    cmd.Parameters.Add("@transit_account_type", SqlDbType.TinyInt).Value = order.TransitAccountType;
                    cmd.Parameters.Add("@descr", SqlDbType.NVarChar).Value = order.Description;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    order.Quality = OrderQuality.Draft;
                    result.Id = order.Id;
                   
                }
                             
                return result;
            }
        }
        /// <summary>
        /// Վերադարձնում է տարանցիկ հաշվին փոխանցման հայտի տվյալները:
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static TransitPaymentOrder Get(TransitPaymentOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT Doc_id,amount,transit_account_type,description,document_number,document_subtype,currency,registration_date,quality,document_subtype,debet_account,credit_account,d.operation_date   FROM  Tbl_HB_documents d  WHERE Doc_id=@Docid and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn);
                cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    order.Amount = Convert.ToDouble(dt.Rows[0]["amount"]);
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.Type = OrderType.TransitPaymentOrder;
                    order.Description = dt.Rows[0]["description"].ToString();
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Quality = (OrderQuality)Convert.ToInt16(dt.Rows[0]["quality"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.TransitAccountType =(TransitAccountTypes)int.Parse(dt.Rows[0]["transit_account_type"].ToString());
                    order.DebitAccount = Account.GetSystemAccount(dt.Rows[0]["debet_account"].ToString());
                    order.TransitAccount = Account.GetSystemAccount(dt.Rows[0]["credit_account"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);

                }

            }
            return order;
        }

        internal static ushort GetTransitPaymentOrderSystemAccountType(TransitAccountTypes transitAccountType)
        {
            ushort accountType = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT [order_account_type] from [tbl_type_of_transit_accounts] where id=@id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@id", SqlDbType.Float).Value = transitAccountType;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    accountType =ushort.Parse(dr["order_account_type"].ToString());
                 }

            }
            return accountType;
        }

        internal static ActionResult SaveTranisPaymentOrderDetails(TransitPaymentOrder transitPaymentOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_transit_payment_order_details(order_id, transit_account_type) VALUES(@orderId, @transitAccountType)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@transitAccountType", SqlDbType.Int).Value = transitPaymentOrder.TransitAccountType;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }
    }
}
