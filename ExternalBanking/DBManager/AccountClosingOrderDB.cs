using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class AccountClosingOrderDB
    {
        /// <summary>
        /// Հաշվի փակման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult CloseAccountOrder(AccountClosingOrder order, string userName, SourceType source)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_close_account";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.ClosingAccounts[0].Currency;
                    cmd.Parameters.Add("@closing_account", SqlDbType.VarChar, 50).Value = order.ClosingAccounts[0].AccountNumber;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@closing_reason_type", SqlDbType.Int).Value = order.ClosingReasonType;
                    if(source == SourceType.AcbaOnline || source == SourceType.MobileBanking)
                    {
                        if (order.ClosingReasonDescription == null) order.ClosingReasonDescription = "";
                        cmd.Parameters.Add("@closing_reason_description", SqlDbType.NVarChar, 200).Value = order.ClosingReasonDescription;
                    }
                    else
                    {
                        cmd.Parameters.Add("@closing_reason_description", SqlDbType.NVarChar, 200).Value = order.ClosingReasonDescription;
                    }
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

                    string accountsInsertString = @"INSERT INTO Tbl_account_order_selected_accounts(Doc_ID, account_number) VALUES(@Doc_ID, @account_number)";

                    for (int i = 1; i < order.ClosingAccounts.Count; i++)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = accountsInsertString;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;
                        cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = order.ClosingAccounts[i].AccountNumber;
                        cmd.ExecuteNonQuery();
                    }

                    return result;
                }
            }

        }

        /// <summary>
        /// Հաշվի փակման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondClosing(ulong customerNumber, string accountNumber, SourceType sourceType)
        {
            bool secondClosing;
            string sourceTypeCond = "";
            if(sourceType == SourceType.Bank)
            {
                sourceTypeCond = " and source_type not in (1,5)";
            }
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=29 and document_subtype=1 and
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
        /// Վերադարձնում է հաշվի փակման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        internal static AccountClosingOrder GetAccountClosingOrder(AccountClosingOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"select d.registration_date,d.debet_account,d.document_number,d.customer_number,d.document_type,d.document_subtype,d.quality,                                                
		                                                 c.closing_reason_description,c.closing_reason_type,d.operation_date,d.confirmation_date, d.order_group_id
                                                  from Tbl_HB_documents as d inner join Tbl_account_closing_order_details as c on d.doc_ID=c.Doc_ID
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
                    order.ClosingReasonType = ushort.Parse((dt.Rows[0]["closing_reason_type"]).ToString());
                    order.ClosingReasonDescription = dt.Rows[0]["closing_reason_description"].ToString();
                    order.ClosingReasonTypeDescription = Info.GetAccountClosingReasonTypeDescription(order.ClosingReasonType);
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;



                    cmd.Parameters.Clear();
                    cmd.CommandText = @"SELECT account_number FROM Tbl_account_order_selected_accounts WHERE Doc_ID=@Doc_ID";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Doc_ID", SqlDbType.Int).Value = order.Id;

                    order.ClosingAccounts = new List<Account>();
                    dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        order.ClosingAccounts = new List<Account>();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            Account currentAccount = AccountDB.GetAccount(row["account_number"].ToString());
                            order.ClosingAccounts.Add(currentAccount);
                        }
                    }

                }


            }
            return order;
        }

        internal static ActionResult SaveAccountClosingOrderDetails(AccountClosingOrder accountClosingOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    string boAccountsInsertString = @"INSERT INTO Tbl_BO_account_order_details(order_id, currency, account_type, statement_type, account_number, reopen_reason) 
                                                    VALUES(@orderId, @currency, @accountType, @statementType, @accountNumber, @reopenReason)";

                    accountClosingOrder.ClosingAccounts.ForEach(m =>
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = boAccountsInsertString;
                        cmd.CommandType = CommandType.Text;

                        cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                        cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = DBNull.Value;
                        cmd.Parameters.Add("@accountType", SqlDbType.SmallInt).Value = DBNull.Value;
                        cmd.Parameters.Add("@statementType", SqlDbType.SmallInt).Value = DBNull.Value;

                        cmd.Parameters.Add("@accountNumber", SqlDbType.BigInt).Value = m.AccountNumber;
                        cmd.Parameters.Add("@reopenReason", SqlDbType.NVarChar, 255).Value = accountClosingOrder.ClosingReasonDescription;
                        cmd.ExecuteNonQuery();

                    });
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
        }
    }

}
