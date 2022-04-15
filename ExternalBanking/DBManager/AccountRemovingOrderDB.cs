using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class AccountRemovingOrderDB
    {
        /// <summary>
        /// Հաշվի հեռացման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult RemoveAccountOrder(AccountRemovingOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_remove_account";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.RemovingAccount.Currency;
                    cmd.Parameters.Add("@removing_account", SqlDbType.VarChar, 50).Value = order.RemovingAccount.AccountNumber;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                    {
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    }

                    if (order.Id != 0)
                    {
                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                    }

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;

                    return result;
                }
            }

        }

        /// <summary>
        /// Հաշվի հեռացման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondClosing(ulong customerNumber, string accountNumber)
        {
            bool secondClosing;
            string sourceTypeCond = "";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=247  and
                                                debet_account=@accountNumber and customer_number=@customerNumber" + sourceTypeCond, conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        secondClosing = true;
                    }
                    else
                        secondClosing = false;
                }

            }
            return secondClosing;
        }

        /// <summary>
        /// Վերադարձնում է հաշվի հեռացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static AccountRemovingOrder GetAccountRemovingOrder(AccountRemovingOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select d.registration_date,d.debet_account,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		                                                d.operation_date,d.confirmation_date, d.order_group_id
                                                  from Tbl_HB_documents as d 
                                                  where d.Doc_ID=@DocID and d.customer_number=case when @customer_number = 0 then d.customer_number else @customer_number end", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    dt.Load(cmd.ExecuteReader());

                    order.OrderNumber = dt.Rows[0]["document_number"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;

                    dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            Account currentAccount = AccountDB.GetAccount(row["account_number"].ToString());
                            order.RemovingAccount = currentAccount;
                        }
                    }

                }

            }
            return order;
        }



    }
}
