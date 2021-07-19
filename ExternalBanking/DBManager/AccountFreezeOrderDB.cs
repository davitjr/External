using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class AccountFreezeOrderDB
    {
        /// <summary>
        /// Հաշվի սառեցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondFreeze(ulong customerNumber, string accountNumber)
        {
            bool secondFreeze;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select doc_ID from Tbl_HB_documents where quality in (1,2,3,5) and document_type=66 and document_subtype=1 and
                                                debet_account=@accountNumber and customer_number=@customerNumber", conn);
                cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                if (cmd.ExecuteReader().Read())
                {
                    secondFreeze = true;
                }
                else
                {
                    secondFreeze = false;
                }
            }
            return secondFreeze;
        }

        
        /// <summary>
        /// Հաշվի սառեցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(AccountFreezeOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[dbo].[pr_account_freeze_order]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@account_number", SqlDbType.VarChar).Value = order.FreezeAccount.AccountNumber;
                    cmd.Parameters.Add("@freeze_amount", SqlDbType.Float).Value = (order.FreezeAmount);
                    cmd.Parameters.Add("@freeze_amount_date", SqlDbType.SmallDateTime).Value = order.AmountFreezeDate;
                    cmd.Parameters.Add("@freeze_date", SqlDbType.SmallDateTime).Value = order.FreezeDate;
                    cmd.Parameters.Add("@freeze_reason", SqlDbType.Int).Value = order.FreezeReason;
                    cmd.Parameters.Add("@freeze_reason_add", SqlDbType.NVarChar, 200).Value = !string.IsNullOrEmpty(order.FreezeReasonAddInf)? (object)order.FreezeReasonAddInf : DBNull.Value;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;

                    cmd.Parameters.Add(new SqlParameter("@Doc_ID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    order.Id = Convert.ToInt64(cmd.Parameters["@Doc_ID"].Value);
                    result.ResultCode = ResultCode.Normal;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }
        }

        /// <summary>
        /// Հաշվի սառեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static AccountFreezeOrder Get(AccountFreezeOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string sqlString = @"SELECT registration_date,document_number,document_subtype,document_type,quality,source_type,debet_account,
				                        freeze_amount,freeze_amount_date,freeze_date,freeze_reason,T.Description as freeze_reason_description,freeze_reason_add,operation_date
                                        FROM Tbl_HB_documents HB
                                        INNER JOIN tbl_account_freeze_order_details TD on HB.doc_ID=TD.doc_ID
                                        LEFT JOIN Tbl_type_of_freeze_reason T on TD.freeze_reason = T.Id   
                                        WHERE customer_number=case when @customer_number = 0 then customer_number else @customer_number end AND HB.doc_ID=@docID ";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                    order.OrderNumber = dr["document_number"].ToString();
                    order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                    order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                    order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                    order.FreezeAccount = Account.GetAccount(Convert.ToUInt64(dr["debet_account"].ToString()));
                    order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                    order.FreezeReason = ushort.Parse(dr["freeze_reason"].ToString());
                    order.FreezeReasonDescription = Utility.ConvertAnsiToUnicode(dr["freeze_reason_description"].ToString());
                    if (!String.IsNullOrEmpty(dr["freeze_date"].ToString()))
                    {
                        order.FreezeDate = (DateTime)dr["freeze_date"];
                    }
                    order.FreezeAmount = (dr["freeze_amount"] != DBNull.Value) ? Convert.ToDouble(dr["freeze_amount"]) : default(double);
                    if (!String.IsNullOrEmpty(dr["freeze_amount_date"].ToString()))
                    {
                        order.AmountFreezeDate = (DateTime)dr["freeze_amount_date"];
                    }
                    order.FreezeReasonAddInf = Utility.ConvertAnsiToUnicode(dr["freeze_reason_add"].ToString());
                                        order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                }
            }
            return order;
        }

        internal static ActionResult SaveAccountFreezeOrderDetails(AccountFreezeOrder accountFreezeOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_account_freeze_order_details(order_id, freeze_account, freeze_reason, freeze_reason_add, freeze_date, freeze_amount_date, freeze_amount) 
                                        VALUES(@orderId, @freezeAccount, @freezeReason, @freezeReasonAdd, @freezeDate, @freezeAmountDate, @freezeAmount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@freezeAccount", SqlDbType.BigInt).Value = accountFreezeOrder.FreezeAccount.AccountNumber;
                    cmd.Parameters.Add("@freezeReason", SqlDbType.Int).Value = (object)accountFreezeOrder.FreezeReason ?? DBNull.Value;
                    cmd.Parameters.Add("@freezeReasonAdd", SqlDbType.NVarChar, 250).Value = (object)accountFreezeOrder.FreezeReasonAddInf ?? DBNull.Value;
                    cmd.Parameters.Add("@freezeDate", SqlDbType.DateTime).Value = (object)accountFreezeOrder.FreezeDate ?? DBNull.Value;
                    cmd.Parameters.Add("@freezeAmountDate", SqlDbType.DateTime).Value = (object)accountFreezeOrder.AmountFreezeDate ?? DBNull.Value;
                    cmd.Parameters.Add("@freezeAmount", SqlDbType.Float).Value = (object)accountFreezeOrder.FreezeAmount ?? DBNull.Value;
                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        public static bool ValidateFreezing(string accountNumber)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT dbo.fn_validate_freezing(@account_number)";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account_number", SqlDbType.Float).Value = accountNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return Convert.ToBoolean(dr[0]);
                        else return false;
                    }
                }
            }
        }

    }
}
