using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Linq;


namespace ExternalBanking.DBManager
{
    class LoanProductDB
    {

        internal static double GetPenaltyRateForDate(DateTime date)
        {
            double penaltyRate = 0;

            using(SqlConnection conn=new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT penalty_rate FROM [tbl_penalty] WHERE with_date<=@date ORDER BY with_date DESC", conn);
                cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = date;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    penaltyRate = double.Parse(dr["penalty_rate"].ToString());
                }
                

            }
            return penaltyRate;
        }


        internal static List<LoanProductProlongation> GetLoanProductProlongations(ulong productId)
        {

            List<LoanProductProlongation> loanProductProlongations = new List<LoanProductProlongation>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT   date_of_normal_end,
                                        interest_rate,
                                        interest_rate_effective,
                                        interest_rate_full, 
                                        penalty_add_percent,
                                        date_of_beginning, 
                                        reg_date, reg_set_number,
                                        confirmation_date, 
                                        confirmation_set_number,
                                        activation_date,
                                        activation_set_number
                                        FROM Tbl_prolonged_products WHERE app_id =@app_id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count != 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            LoanProductProlongation loanProductProlongation = SetLoanProductProlongationDetail(row);
                            loanProductProlongations.Add(loanProductProlongation);
                        }
                    }
                    else
                        loanProductProlongations = null;

                }
            }



            return loanProductProlongations;
            
        }


        private static LoanProductProlongation SetLoanProductProlongationDetail(DataRow row)
        {
            LoanProductProlongation loanProductProlongation = new LoanProductProlongation();
            loanProductProlongation.EndDate = Convert.ToDateTime(row["date_of_normal_end"].ToString());
            loanProductProlongation.InterestRate = Convert.ToDecimal(row["interest_rate"].ToString());
            loanProductProlongation.InterestRateEffective = Convert.ToDecimal(row["interest_rate_effective"].ToString());
            loanProductProlongation.InterestRateFull = Convert.ToDecimal(row["interest_rate_full"].ToString());
            if (row["penalty_add_percent"] != DBNull.Value)
                loanProductProlongation.PenaltyAddPercent = Convert.ToDecimal(row["penalty_add_percent"].ToString());
            if (row["date_of_beginning"] != DBNull.Value)
                loanProductProlongation.StartDate = Convert.ToDateTime(row["date_of_beginning"].ToString());
            loanProductProlongation.RegistrationDate = Convert.ToDateTime(row["reg_date"].ToString());
            loanProductProlongation.RegistrationSetNumber = Convert.ToInt32(row["reg_set_number"].ToString());
            if (row["confirmation_date"] != DBNull.Value)
                loanProductProlongation.ConfirmationDate = Convert.ToDateTime(row["confirmation_date"].ToString());
            if (row["confirmation_set_number"] != DBNull.Value)
                loanProductProlongation.ConfirmationSetNumber = Convert.ToInt32(row["confirmation_set_number"].ToString());
            if (row["activation_date"] != DBNull.Value)
                loanProductProlongation.ActivationDate = Convert.ToDateTime(row["activation_date"].ToString());
            if (row["activation_set_number"] != DBNull.Value)
                loanProductProlongation.ActivationSetNumber = Convert.ToInt32(row["activation_set_number"].ToString());



            return loanProductProlongation;
        }


        internal static bool CheckLoanProductClaimAvailability(long productId)
        {
            bool claimAvailability = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dbo.fn_hasclaim(@productId) result";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        claimAvailability = Convert.ToBoolean(Convert.ToInt16(dr["result"].ToString()));
                    }


                }
            }



            return claimAvailability;

        }

        internal static string GetCreditCode(long productId,short productType)
        {
            string creditCode = "";
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dbo.fn_get_credit_code_by_application_id(@productId,@productType) result";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@productType", SqlDbType.SmallInt).Value = productType;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        creditCode = dr["result"].ToString();
                    }


                }
            }



            return creditCode;

        }


        internal static string GetApplicationIdByCreditCode(string creditCode)
        {
            ulong  productId = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dbo.fn_get_application_id_by_credit_code(@creditCode) result";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@creditCode", SqlDbType.NVarChar,16).Value = creditCode;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        productId = Convert.ToUInt64(dr["result"]);
                    }


                }
            }



            return creditCode;

        }

        internal static ulong GetCustomerNumberByLoanApp(ulong productId)
        {
            ulong customerNumber=0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"IF exists(SELECT 1 FROM V_ShortLoans_small WHERE app_id = @productId)
							    BEGIN
							        SELECT customer_number as result FROM V_ShortLoans_small  WHERE app_id = @productId
							    END
							  ELSE IF exists(SELECT 1 FROM Tbl_credit_lines WHERE app_id = @productId)
							    BEGIN
							        SELECT customer_number as result FROM Tbl_credit_lines  WHERE app_id = @productId
							    END
							  ELSE IF exists(SELECT 1 FROM Tbl_given_guarantee WHERE app_id = @productId)
							    BEGIN
							        SELECT customer_number as result FROM Tbl_given_guarantee WHERE app_id = @productId
							    END ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.BigInt).Value = productId;
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                customerNumber = Convert.ToUInt64(dr["result"]);
                            }
                        }
                    }
                }
            }            
            return customerNumber;
        }

        internal static short CheckForEffectiveSign(int loanType)
        {
            short sign = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT  [dbo].[fn_check_calculating_interest_rate_effective_with_only_bank_profit](0,@loanType,0)", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@loanType", SqlDbType.Float).Value = loanType;

                    sign = Convert.ToInt16(cmd.ExecuteScalar());
                }


            }
            return sign;
        }

        internal static string GetNextPeriodRateAccount(long productId)
        {
            string NextPeriodRateAccount = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT Account_number FROM Tbl_Products_Accounts p INNER JOIN 
                                Tbl_Products_Accounts_Groups pg ON p.Group_Id = pg.Group_ID
	                            WHERE Type_of_account = 282 AND  App_ID = @app_id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        NextPeriodRateAccount = dr["Account_number"].ToString();
                    }


                }
            }

            return NextPeriodRateAccount;

        }

        internal static double GetNextPeriodRateAccountBalance(string NextPeriodRateAccount)
        {
            double NextPeriodRateAccountBalance = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT balance FROM [tbl_all_accounts;] WHERE Arm_number = @account_number";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@account_number", SqlDbType.NVarChar,20).Value = NextPeriodRateAccount;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        NextPeriodRateAccountBalance = Convert.ToDouble(dr["balance"]);
                    }


                }
            }

            return NextPeriodRateAccountBalance;

        }


    }
}
