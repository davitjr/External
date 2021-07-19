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
    public class DepositaryAccountDB
    {
        internal static DepositaryAccount GetCustomerDepositaryAccount(ulong customerNumber, ref bool hasAccount)
        {
            hasAccount = false;
            DepositaryAccount account = new DepositaryAccount();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT * from Tbl_depo_accounts where customer_number = @customer_number";
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    SqlDataReader dr = cmd.ExecuteReader();
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
                        hasAccount = true;

                    }
                }

            }
            return account;
        }

        internal static DepositaryAccount GetDepositaryAccountById(int id)
        {

            DepositaryAccount depoAccount = new DepositaryAccount();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = @"select id,bank_code,[description],depo_account,registration_date,set_number from tbl_depo_accounts  where id= @id";
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

                }
            }

            return depoAccount;
        }
    }
}
