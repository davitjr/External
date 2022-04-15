using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    internal class SSTerminalDB
    {

        internal static Account GetOperationSystemAccount(string terminalNumber, string currency)
        {
            Account account = new Account();
            account.Currency = currency;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select dbo.fnc_get_ATM_CashIn_Account(@merchant_ID,@cur)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@merchant_ID", SqlDbType.NVarChar, 10).Value = terminalNumber;
                    cmd.Parameters.Add("@cur", SqlDbType.NVarChar, 3).Value = currency;
                    account.AccountNumber = cmd.ExecuteScalar().ToString();
                }
            }
            return account;
        }

        internal static short CheckTerminalAuthorization(string terminalID, string ipAddress, string password)
        {
            short result;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["PermissionsBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = "pr_terminal_password_check_int";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@username ", SqlDbType.NVarChar, 50).Value = terminalID;
                    cmd.Parameters.Add("@password ", SqlDbType.NVarChar, 50).Value = password;
                    cmd.Parameters.Add("@ip", SqlDbType.NVarChar, 50).Value = ipAddress;
                    cmd.Parameters.Add(new SqlParameter("@res", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();
                    result = Convert.ToInt16(cmd.Parameters["@res"].Value);
                }
            }
            return result;
        }

        internal static ushort GetTerminalFilial(string terminalID)
        {
            ushort filial = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT filial FROM tbl_arca_points_list WHERE merchant_id = @merchant_ID";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@merchant_ID", SqlDbType.NVarChar, 10).Value = terminalID;
                    filial = ushort.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            return filial;
        }

        internal static Account GetOperationSystemTransitAccount(string terminalNumber, string currency)
        {
            Account account = new Account();
            account.Currency = currency;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select dbo.fnc_get_SSTerminal_Transit_Account(@merchant_ID,@cur)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@merchant_ID", SqlDbType.NVarChar, 10).Value = terminalNumber;
                    cmd.Parameters.Add("@cur", SqlDbType.NVarChar, 3).Value = currency;
                    account.AccountNumber = cmd.ExecuteScalar().ToString();
                }
            }
            return account;
        }
    }
}
