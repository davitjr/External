using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class CTPaymentDB
    {

        internal static PaymentStatus GetPaymentStatus(long paymentID)
        {
            PaymentStatus status = new PaymentStatus();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT hb.quality,hq.change_date FROM Tbl_HB_documents hb
													INNER JOIN Tbl_HB_quality_history hq
													on hq.quality=hb.quality and hq.Doc_ID=hb.doc_ID
                                                    WHERE hb.doc_ID=@docId", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@docId", SqlDbType.Float).Value = paymentID;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    ushort quality = Convert.ToUInt16(dr["quality"]);
                    status.StatusDateTime = Convert.ToDateTime(dr["change_date"]);
                    if (quality == 30)
                    {
                        status.StatusCode = 1;
                    }
                    else
                    {
                        status.StatusCode = 2;
                    }
                }
                else
                {
                    status.StatusCode = 6;
                }

            }

            return status;
        }



        internal static CashTerminal CheckTerminalPassword(string userName, string password)
        {

            CashTerminal cashTerminal = null;
            short result = 1;
            ulong customerNumber = 0;
            int terminalId = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PermissionsBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "pr_terminal_password_check";

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = userName;
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar, 50).Value = password;
                    cmd.Parameters.Add(new SqlParameter("@res", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@customerNumber", SqlDbType.Float) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@terminalId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result = Convert.ToInt16(cmd.Parameters["@res"].Value);
                    if (cmd.Parameters["@customerNumber"].Value != null && result == 0)
                    {
                        customerNumber = Convert.ToUInt64(cmd.Parameters["@customerNumber"].Value);
                        terminalId = Convert.ToInt32(cmd.Parameters["@terminalId"].Value);
                        if (customerNumber != 0)
                        {
                            cashTerminal = new CashTerminal();
                            cashTerminal.CustomerNumber = customerNumber;
                            cashTerminal.UserName = userName;
                            cashTerminal.AccessibleActions = GetCTAccessibleActions(terminalId);
                        }
                    }

                }
            }
            return cashTerminal;

        }


        internal static PaymentStatus GetPaymentStatusByOrderID(long orderID)
        {
            PaymentStatus status = new PaymentStatus();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 hb.quality,hq.change_date ,hb.Doc_ID FROM Tbl_HB_documents hb
                                                    INNER JOIN tbl_payment_registration_request_details dt
                                                    ON hb.doc_ID=dt.doc_ID
													INNER JOIN Tbl_HB_quality_history hq
													ON hq.quality=hb.quality and hq.Doc_ID=hb.doc_ID
                                                    WHERE dt.order_id=@orderId
                                                    ORDER BY doc_id DESC", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = orderID;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    ushort quality = Convert.ToUInt16(dr["quality"]);
                    status.StatusDateTime = Convert.ToDateTime(dr["change_date"]);
                    status.PaymentID = Convert.ToInt32(dr["Doc_ID"]);

                    if (quality == 30)
                    {
                        status.StatusCode = 1;
                    }
                    else
                    {
                        status.StatusCode = 2;
                    }
                }
                else
                {
                    status.StatusCode = 6;
                    status.PaymentID = 0;
                    status.StatusDateTime = DateTime.Now;
                }

            }

            return status;
        }

        internal static Account GetCTDebitAccount(ulong customerNumber)
        {
            Account account = new Account();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT account_number FROM tbl_external_payments_system_configurations
                                                    WHERE customer_number=@customerNumber
                                                    ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string accountNumber = dr["account_number"].ToString();
                    account = Account.GetAccount(accountNumber);
                }
                else
                {
                    account = null;
                }

            }

            return account;
        }


        internal static List<CTAccessibleAction> GetCTAccessibleActions(int terminalId)
        {
            List<CTAccessibleAction> actions = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PermissionsBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT acc.actionID,acc.access FROM Tbl_terminal_users us
                                                    INNER JOIN Tbl_terminal_action_access acc
                                                    ON acc.term_user_id=us.id
                                                    WHERE us.ID=@terminalId
                                                    ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@terminalId", SqlDbType.Int).Value = terminalId;

                using SqlDataReader dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                if (dt.Rows.Count > 0)
                {
                    actions = new List<CTAccessibleAction>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        CTAccessibleAction action = new CTAccessibleAction();
                        action.Access = Convert.ToUInt32(dt.Rows[i]["access"]);
                        action.Action = (CTAction)Convert.ToUInt32(dt.Rows[i]["actionID"]);
                        actions.Add(action);

                    }
                }



            }

            return actions;
        }

        internal static int GetTerminalID(ulong customerNumber)
        {
            int ID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT ID FROM tbl_external_payments_system_configurations 
                                                                            WHERE  customer_number = @customerNumber", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    ID = Convert.ToUInt16(dr["ID"]);
                }

            }

            return ID;
        }

        internal static CashTerminal GetTerminalByID(int teminalID)
        {
            CashTerminal terminal = new CashTerminal();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PermissionsBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT customer_number,username 
                                                                            FROM [dbo].[Tbl_terminal_users] 
                                                                            WHERE  ID = @ID", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ID", SqlDbType.Float).Value = teminalID;

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    terminal.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                    terminal.UserName = dr["username"].ToString();
                    terminal.ID = teminalID;
                }

            }

            return terminal;
        }
    }
}
