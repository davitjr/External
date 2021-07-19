using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class CashierCashLimitDB
    {
        public static List<CashierCashLimit> GetCashierLimits(int setNumber)
        {
            List<CashierCashLimit> listCashierCashLimits = new List<CashierCashLimit>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = @"SELECT  currency,amount,id 
                                FROM Tbl_cashiers_cash_limits L 
		                        WHERE set_number = @setNumber and date_of_beginning = 
                                (SELECT max(date_of_beginning)    
                                FROM Tbl_cashiers_cash_limits 
                                WHERE set_number = L.set_number and currency = L.currency)
		                        ORDER BY currency";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            CashierCashLimit cashierCashLimit = new CashierCashLimit();
                            cashierCashLimit.Currency = dr["currency"].ToString();
                            cashierCashLimit.Amount = Convert.ToDouble(dr["amount"]);
                            cashierCashLimit.Id = Convert.ToInt32(dr["id"]);
                            listCashierCashLimits.Add(cashierCashLimit);
                        }

                    }

                }
                return listCashierCashLimits;

            }

        }

        private static CashierCashLimit SetCashierCashLimit(DataRow dataRow)
        {
            CashierCashLimit cashierCashLimit = new CashierCashLimit();
            if (dataRow != null)
            {
                cashierCashLimit.SetNumber = Convert.ToInt32(dataRow["set_number"]);
                cashierCashLimit.Currency = dataRow["currency"].ToString();
                cashierCashLimit.Amount = Convert.ToDouble(dataRow["amount"]);
                cashierCashLimit.StartDate = Convert.ToDateTime(dataRow["date_of_beginning"]);
                cashierCashLimit.ChangeBySetNumber = Convert.ToInt32(dataRow["set_by"]);
                cashierCashLimit.Id = Convert.ToInt32(dataRow["Id"]);
            }
            return cashierCashLimit;

        }

        public static void SaveCashierCashLimits(CashierCashLimit listLimit)
        {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = @"INSERT INTO Tbl_cashiers_cash_limits(set_number,currency,amount,date_of_beginning,set_by) 
                                                VALUES(@setNumber,@currency,@amount,getdate(),@changeSetNumber)";

                        cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = listLimit.SetNumber;
                        cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = listLimit.Currency;
                        cmd.Parameters.Add("@amount", SqlDbType.Money).Value = listLimit.Amount;
                        cmd.Parameters.Add("@changeSetNumber", SqlDbType.Int).Value = listLimit.ChangeBySetNumber;
                        cmd.ExecuteNonQuery();
                    }
                }
        }

        public static ActionResult GenerateCashierCashDefaultLimits(int setNumber, int changeSetNumber)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_add_limits_for_cashier";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;
                    cmd.Parameters.Add("@set_by", SqlDbType.Int).Value = changeSetNumber;
                    cmd.Parameters.Add("@mode", SqlDbType.SmallInt).Value = 0;
                    cmd.ExecuteNonQuery();

                }
                result.ResultCode = ResultCode.Normal;
                return result;

            }

        }

        public static int GetCashierFilialCode(int setNumber)
        {
            int filial_code = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT filial_code FROM V_cashers_list where new_id=@setNumber",
                    conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;


                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            filial_code = Convert.ToInt32(dr["filial_code"]);

                        }
                    }
                        

                    return filial_code;
                }
                    
            }


        }

    }
}



