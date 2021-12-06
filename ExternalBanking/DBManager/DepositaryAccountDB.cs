using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class DepositaryAccountDB
    {
        internal static List<DepositaryAccount> GetCustomerDepositaryAccounts(ulong customerNumber)
        {
            List<DepositaryAccount> listaccount = new List<DepositaryAccount>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT * from Tbl_depo_accounts where customer_number = @customer_number";
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    using SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        listaccount.Add(new DepositaryAccount
                        {
                            AccountNumber = Convert.ToDouble(dr["Depo_account"]),
                            BankCode = dr["Bank_code"] != DBNull.Value ? Convert.ToInt32(dr["Bank_code"]) : 0,
                            Description = Utility.ConvertAnsiToUnicode(dr["Description"].ToString()),
                            ID = Convert.ToInt32(dr["id"]),
                            RegistrationDate = Convert.ToDateTime(dr["registration_date"]),
                            SetNumber = Convert.ToInt32(dr["set_number"].ToString()),
                            Status = SetDepoAccountStatus(dr["status"].ToString()),
                            StockIncomeAccountNumber = dr["stock_income_account"].ToString()
                        });
                    }
                }
            }
            return listaccount;
        }
        internal static DepositaryAccount GetCustomerDepositaryAccount(ulong customerNumber, ref bool hasAccount)
        {
            hasAccount = false;
            DepositaryAccount account = new DepositaryAccount();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using SqlCommand cmd = new SqlCommand();

                cmd.Connection = conn;
                cmd.CommandText = @"SELECT * from Tbl_depo_accounts where customer_number = @customer_number";
                cmd.CommandType = CommandType.Text;
                conn.Open();
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    account.AccountNumber = Convert.ToDouble(dr["Depo_account"]);
                    if (dr["Bank_code"] != DBNull.Value)
                    {
                        account.BankCode = Convert.ToInt32(dr["Bank_code"]);
                    }

                    account.Description = Utility.ConvertAnsiToUnicode(dr["Description"].ToString());
                    account.ID = Convert.ToInt32(dr["id"]);
                    account.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                    account.SetNumber = Convert.ToInt32(dr["set_number"].ToString());
                    account.Status = SetDepoAccountStatus(dr["status"].ToString());


                    hasAccount = true;

                }
            }
            return account;
        }

        internal static double GetDepositaryAccount(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"select  top 1  depo_account from  Tbl_depo_accounts  where status = 'N' and customer_number = @customerNumber";
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.CommandType = CommandType.Text;
                    return Convert.ToDouble(cmd.ExecuteScalar());
                }
            }
        }

        private static string SetDepoAccountStatus(string status)
        {
            return status == "N" ? "Գործող" : (status == "Y" ? "Կասեցված" : "-");
        }

        internal static DepositaryAccount GetDepositaryAccountById(int id)
        {

            DepositaryAccount depoAccount = new DepositaryAccount();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = @"select id,bank_code,[description],depo_account,registration_date,set_number,status,stock_income_account from tbl_depo_accounts  where id= @id";
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.CommandType = CommandType.Text;



                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    DataRow row = dt.Rows[0];


                    depoAccount.ID = int.Parse(row["id"].ToString());
                    depoAccount.AccountNumber = Convert.ToDouble(row["Depo_account"]);
                    if (row["Bank_code"] != DBNull.Value)
                    {
                        depoAccount.BankCode = Convert.ToInt32(row["Bank_code"]);
                    }

                    depoAccount.Description = Utility.ConvertAnsiToUnicode(row["Description"].ToString());
                    depoAccount.RegistrationDate = Convert.ToDateTime(row["registration_date"]);
                    depoAccount.SetNumber = Convert.ToInt32(row["set_number"].ToString());
                    depoAccount.Status = SetDepoAccountStatus(row["status"].ToString());
                    depoAccount.StockIncomeAccountNumber = row["stock_income_account"].ToString();
                }
            }

            return depoAccount;
        }

        public static void DeleteDepoAccounts(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM [Tbl_depo_accounts] WHERE ISNULL(customer_number, 0) = @customer_number", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static DepositaryAccount GetDepositaryAccountForStock(ulong customerNumber)
        {

            DepositaryAccount depoAccount = new DepositaryAccount();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using SqlCommand cmd = new SqlCommand();

                cmd.Connection = conn;
                cmd.CommandText = @"select id,bank_code,[description],depo_account,registration_date,set_number,status from tbl_depo_accounts where status = 'N' and customer_number = @customerNumber";
                cmd.CommandType = CommandType.Text;
                conn.Open();
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                cmd.CommandType = CommandType.Text;

                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        depoAccount.ID = int.Parse(reader["id"].ToString());
                        depoAccount.AccountNumber = Convert.ToDouble(reader["Depo_account"]);
                        if (reader["Bank_code"] != DBNull.Value)
                        {
                            depoAccount.BankCode = Convert.ToInt32(reader["Bank_code"]);
                        }

                        depoAccount.Description = Utility.ConvertAnsiToUnicode(reader["Description"].ToString());
                        depoAccount.RegistrationDate = Convert.ToDateTime(reader["registration_date"]);
                        depoAccount.SetNumber = Convert.ToInt32(reader["set_number"].ToString());
                        depoAccount.Status = SetDepoAccountStatus(reader["status"].ToString());
                    }
                }
            }

            return depoAccount;
        }
    }
}
