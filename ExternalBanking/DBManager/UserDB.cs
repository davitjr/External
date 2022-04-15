using ExternalBanking.ACBAServiceReference;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public static class UserDB
    {
        public static decimal CashLimit(User user, string currency)
        {
            decimal result = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT TOP 1 amount 
                                      FROM Tbl_Cashiers_cash_limits 
                                      WHERE set_number=@setNumber 
                                      AND currency=@currency 
                                      AND  dbo.get_oper_day()>=CAST(date_of_beginning as date)
                                      ORDER BY date_of_beginning DESC";
                    cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = user.userID;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = currency;

                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        result = Convert.ToDecimal(obj);
                    }
                }
            }

            return result;
        }

        public static decimal CashRest(User user, string currency)
        {
            decimal result = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select dbo.Fnc_Return_cashier_cash_sum(@setNumber,@currency,dbo.get_oper_day()) ";
                    cmd.Parameters.Add("@setNumber", SqlDbType.SmallInt).Value = user.userID;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = currency;
                    result = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return result;
        }
        public static bool IsActiveUser(int new_id)
        {
            byte result = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT active_user
                                        FROM  v_cashers_list 
                                        WHERE new_id =@new_id";

                    cmd.Parameters.Add("@new_id", SqlDbType.SmallInt).Value = new_id;

                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        result = Convert.ToByte(obj);
                    }
                }
            }
            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
