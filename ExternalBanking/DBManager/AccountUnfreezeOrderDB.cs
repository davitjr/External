using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;

namespace ExternalBanking.DBManager
{
    static class AccountUnfreezeOrderDB
    {
        /// <summary>
        /// Հաշվի ապասառեցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondUnfreeze(long freezeId, string accountNumber)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT 1 FROM Tbl_HB_documents HB
                                                INNER JOIN tbl_account_unfreeze_order_details CD on HB.doc_ID = CD.doc_ID
                                                WHERE quality in (1,2,3,5) and freeze_ID= @freezeID and document_type=67 and document_subtype=1 and
                                                debet_account=@accountNumber", conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    cmd.Parameters.Add("@freezeID", SqlDbType.Float).Value = freezeId;
                    return cmd.ExecuteReader().Read();

                }
            }

        }

        /// <summary>
        /// Ապասառեցման հնարավորություն միայն 12,15 տեսակի սառեցումների համար
        /// </summary>
        /// <param name="freezeId"></param>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        internal static bool IsInaccessibleUnfreeze(long freezeId, string accountNumber, bool isOnlineAcc, bool isBranchDiv)
        {
            bool isInaccessible = false;
            AccountFreezeDetails freezeDetails = AccountFreezeDetails.GetAccountFreezeDetails(freezeId.ToString());

            if ((isOnlineAcc && freezeDetails.ReasonId == 9) ||
                (!isOnlineAcc && !isBranchDiv && freezeDetails.ReasonId != 12 && freezeDetails.ReasonId != 15))
            {

                isInaccessible = true;
                return isInaccessible;
            }

            if (freezeDetails.ReasonId == 1)
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    conn.Open();


                    using (SqlCommand cmd = new SqlCommand(@"SELECT count(1) FROM [Tbl_short_time_loans;] where loan_type = 7 and loan_full_number = @accountNumber and date_of_beginning = @freezeDate", conn))
                    {
                        cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = freezeDetails.FreezeDescription;
                        cmd.Parameters.Add("@freezeDate", SqlDbType.SmallDateTime).Value = freezeDetails.AmountFreezeDate;
                        return cmd.ExecuteReader().Read();

                    }
                }
            }

            return isInaccessible;
        }


        /// <summary>
        /// Հաշվի ապասառեցման հայտի պահպանում
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult Save(AccountUnfreezeOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[dbo].[pr_account_unfreeze_order]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@doc_type", SqlDbType.Int).Value = (int)order.Type;
                    cmd.Parameters.Add("@doc_number", SqlDbType.NVarChar, 20).Value = order.OrderNumber;
                    cmd.Parameters.Add("@account_number", SqlDbType.VarChar).Value = order.FreezedAccount.AccountNumber;
                    cmd.Parameters.Add("@freeze_ID", SqlDbType.Float).Value = order.FreezeId;
                    cmd.Parameters.Add("@unfreeze_reason", SqlDbType.Int).Value = order.UnfreezeReason;
                    cmd.Parameters.Add("@unfreeze_reason_add", SqlDbType.NVarChar, 200).Value = !string.IsNullOrEmpty(order.UnfreezeReasonAddInf) ? (object)order.UnfreezeReasonAddInf : DBNull.Value;
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
        /// Հաշվի ապասառեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static AccountUnfreezeOrder Get(AccountUnfreezeOrder order)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                string sqlString = @"SELECT registration_date,document_number,document_subtype,document_type,quality,source_type,debet_account,
				                        freeze_ID,unfreeze_reason,unfreeze_reason_add,T.Description as unfreeze_description,operation_date
                                        FROM Tbl_HB_documents HB
                                        INNER JOIN tbl_account_unfreeze_order_details CD on HB.doc_ID=CD.doc_ID
                                        LEFT JOIN Tbl_type_of_freeze_reason T on CD.unfreeze_reason = T.Id   
                                        WHERE customer_number=case when @customer_number = 0 then customer_number else @customer_number end AND HB.doc_ID=@docID ";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    cmd.Parameters.Add("@docID", SqlDbType.Float).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        if (dr.Read())
                        {
                            order.RegistrationDate = Convert.ToDateTime(dr["registration_date"].ToString());
                            order.OrderNumber = dr["document_number"].ToString();
                            order.SubType = Convert.ToByte(dr["document_subtype"].ToString());
                            order.Type = (OrderType)Convert.ToInt16(dr["document_type"].ToString());
                            order.Source = (SourceType)Convert.ToInt16(dr["source_type"].ToString());
                            order.FreezedAccount = Account.GetAccount(Convert.ToUInt64(dr["debet_account"].ToString()));
                            order.Quality = (OrderQuality)Convert.ToInt16(dr["quality"].ToString());
                            order.FreezeId = long.Parse(dr["freeze_ID"].ToString());
                            order.UnfreezeReason = ushort.Parse(dr["unfreeze_reason"].ToString());
                            order.UnfreezeReasonDescription = Utility.ConvertAnsiToUnicode(dr["unfreeze_description"].ToString());
                            order.UnfreezeReasonAddInf = Utility.ConvertAnsiToUnicode(dr["unfreeze_reason_add"].ToString());
                            order.OperationDate = dr["operation_date"] != DBNull.Value ? Convert.ToDateTime(dr["operation_date"]) : default(DateTime?);

                        }
                    }
                }
            }
            return order;
        }

        internal static ActionResult SaveAccountUnfreezeOrderDetails(AccountUnfreezeOrder accountUnfreezeOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO Tbl_BO_account_unfreeze_order_details(order_id, freeze_id, freezed_account, unfreeze_reason, unfreeze_reason_add) 
                                        VALUES(@orderId, @freezeId, @freezedAccount, @unfreezeReason, @unfreezeReasonAdd)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@freezeId", SqlDbType.Float).Value = accountUnfreezeOrder.FreezeId;
                    cmd.Parameters.Add("@freezedAccount", SqlDbType.BigInt).Value = accountUnfreezeOrder.FreezedAccount.AccountNumber;
                    cmd.Parameters.Add("@unfreezeReason", SqlDbType.Int).Value = (object)accountUnfreezeOrder.UnfreezeReason ?? DBNull.Value;
                    cmd.Parameters.Add("@unfreezeReasonAdd", SqlDbType.NVarChar, 250).Value = (object)accountUnfreezeOrder.UnfreezeReasonAddInf ?? DBNull.Value;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }
    }
}
