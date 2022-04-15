using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    internal class AccreditiveDB
    {
        internal static List<Accreditive> GetAccreditives(ulong customerNumber)
        {
            List<Accreditive> accreditives = new List<Accreditive>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(S.App_Id,0) App_Id,S.connect_account_full_number,S.benefeciar,S.percent_cummulation,s.overdue_capital_nused,S.filialcode,
                                S.start_capital,S.currency,S.date_of_beginning,S.date_of_normal_end ,S.interest_rate_nused,
                                ABS(S.current_capital_nused) as current_capital,ABS(S.current_rate_value_nused) as current_rate_value,
                                S.quality, S.security_code_2,
                                S.loan_type,Abs(S.inpaied_rest_of_rate_nused) as inpaied_rest_of_rate,Abs(S.penalty_rate_nused) as penalty_rate,
                                Abs(S.penalty_add_nused) as penalty_add,Abs(S.total_rate_value_nused) as total_rate_value,S.[last_day_of rate calculation] as day_of_rate_calculation
                                FROM  Tbl_given_guarantee S
                                where s.customer_number=@customerNumber and s.quality not in(40) and loan_type=41
                                ORDER BY date_of_normal_end";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Accreditive accreditive = SetAccreditive(row);

                        accreditives.Add(accreditive);
                    }
                }
            }
            return accreditives;
        }

        internal static List<Accreditive> GetClosedAccreditives(ulong customerNumber)
        {
            List<Accreditive> accreditives = new List<Accreditive>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(S.App_Id,0) App_Id,S.connect_account_full_number,S.benefeciar,S.percent_cummulation,s.overdue_capital_nused,S.filialcode,
                                S.start_capital,S.currency,S.date_of_beginning,S.date_of_normal_end ,S.interest_rate_nused,
                                ABS(ISNULL(S.current_capital_nused,0)) as current_capital,ABS(S.current_rate_value_nused) as current_rate_value,
                                S.quality,S.security_code_2,
                                S.loan_type,Abs(S.inpaied_rest_of_rate_nused) as inpaied_rest_of_rate,Abs(S.penalty_rate_nused) as penalty_rate,
                                Abs(S.penalty_add_nused) as penalty_add,Abs(S.total_rate_value_nused) as total_rate_value,S.[last_day_of rate calculation] as day_of_rate_calculation
                                FROM  Tbl_given_guarantee S
                                where s.customer_number=@customerNumber and s.quality=40 and loan_type=41
                                ORDER BY date_of_normal_end";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Accreditive accreditive = SetAccreditive(row);

                        accreditives.Add(accreditive);
                    }
                }
            }
            return accreditives;
        }

        internal static Accreditive GetAccreditive(ulong customerNumber, ulong productId)
        {
            Accreditive accreditive = new Accreditive();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(S.App_Id,0) App_Id,S.connect_account_full_number,S.benefeciar,S.percent_cummulation,s.overdue_capital_nused,S.filialcode,
                                S.start_capital,S.currency,S.date_of_beginning,S.date_of_normal_end ,S.interest_rate_nused,
                                ABS(S.current_capital_nused) as current_capital,ABS(S.current_rate_value_nused) as current_rate_value,
                                S.quality,S.security_code_2,
                                S.loan_type,Abs(S.inpaied_rest_of_rate_nused) as inpaied_rest_of_rate,Abs(S.penalty_rate_nused) as penalty_rate,
                                Abs(S.penalty_add_nused) as penalty_add,Abs(S.total_rate_value_nused) as total_rate_value,S.[last_day_of rate calculation] as day_of_rate_calculation
                                FROM  Tbl_given_guarantee S
                                where s.customer_number=@customerNumber and s.App_Id=@appId and loan_type=41
                                ORDER BY date_of_normal_end";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        accreditive = SetAccreditive(row);
                    }
                    else
                    {
                        accreditive = null;
                    }

                }

            }
            return accreditive;
        }

        private static Accreditive SetAccreditive(DataRow row)
        {
            Accreditive accreditive = new Accreditive();

            if (row != null)
            {
                accreditive.FillialCode = Convert.ToInt32(row["filialcode"].ToString());
                accreditive.ProductId = long.Parse(row["app_id"].ToString());
                accreditive.ConnectAccount = Account.GetAccount(ulong.Parse(row["connect_account_full_number"].ToString()));
                accreditive.LoanType = short.Parse(row["loan_type"].ToString());
                accreditive.Quality = short.Parse(row["quality"].ToString());
                accreditive.Currency = row["currency"].ToString();
                accreditive.InterestRate = float.Parse(row["interest_rate_nused"].ToString());
                if (row.Table.Columns["overdue_capital_nused"] != null)
                    accreditive.OverdueCapital = double.Parse(row["overdue_capital_nused"].ToString());
                accreditive.StartCapital = double.Parse(row["start_capital"].ToString());
                accreditive.CurrentCapital = double.Parse(row["current_capital"].ToString());
                accreditive.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
                accreditive.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
                accreditive.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
                accreditive.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
                accreditive.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
                if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
                {
                    accreditive.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
                }
                accreditive.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                accreditive.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                accreditive.ContractNumber = ulong.Parse(row["security_code_2"].ToString());
                accreditive.Benefeciar = Utility.ConvertAnsiToUnicode(row["benefeciar"].ToString());
                accreditive.PercentCummulation = ushort.Parse(row["percent_cummulation"].ToString());
                accreditive.ProductType = Utility.GetProductTypeFromLoanType(accreditive.LoanType);
                accreditive.CreditCode = LoanProduct.GetCreditCode(accreditive.ProductId, accreditive.ProductType);
            }
            return accreditive;
        }


        internal static bool CheckAccreditiveProvisionCurrency(long productId, string currency)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select LP.app_id From Tbl_Link_application_Provision LP 
				                                  Inner join Tbl_provision_of_clients P 
				                                  ON LP.IdPro=P.IdPro 
				                                  Inner join Tbl_type_of_all_provision 
				                                  ON P.[type]=Tbl_type_of_all_provision.type_id
                                                  Where  LP.app_id=@productId and matured_date is null and p.type=13 and p.currency<> @currency", conn))
                {
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.HasRows)
                        {
                            check = true;
                        }
                    }

                }

            }
            return check;


        }


        internal static bool HasTransportProvison(long productId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT pr.idpro FROM Tbl_provision_of_clients pr INNER JOIN  Tbl_Link_application_Provision link on pr.IdPro =  link.IdPro 
                                                 WHERE Type in(5,6) AND matured_date is null AND pr.idpro not in (SELECT idpro FROM Tbl_Link_application_Provision WHERE activated_date is not null AND app_id <>@productId) AND app_id = @productId", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    check = true;
                }
                return check;

            }
        }



    }
}
