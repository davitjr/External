using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class GuaranteeDB
    {
        internal static List<Guarantee> GetGuarantees(ulong customerNumber)
        {
            List<Guarantee> guarantees = new List<Guarantee>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"	SELECT ISNULL(S.App_Id,0) App_Id,S.connect_account_full_number,S.benefeciar,S.percent_cummulation,s.overdue_capital_nused,
                                S.filialcode,S.date_of_contract_end,cn.contract_number,cn.general_number,
                                S.start_capital,S.currency,S.date_of_beginning,S.date_of_normal_end ,S.interest_rate_nused,
                                ABS(S.current_capital_nused) as current_capital,ABS(S.current_rate_value_nused) as current_rate_value,
                                S.quality, S.security_code_2,
                                S.loan_type,Abs(S.inpaied_rest_of_rate_nused) as inpaied_rest_of_rate,Abs(S.penalty_rate_nused) as penalty_rate,
                                Abs(S.penalty_add_nused) as penalty_add,Abs(S.total_rate_value_nused) as total_rate_value,S.[last_day_of rate calculation] as day_of_rate_calculation,
                                S.confirmator_bank,S.confirmator_bank_end,S.prolonged_dates,S.Keeper_open,S.out_loan_date
                                FROM  Tbl_given_guarantee S
								LEFT JOIN
								Tbl_main_loans_contracts cn
								ON s.ID_Contract = cn.ID_Contract
                                WHERE s.customer_number=@customerNumber and s.quality not in(40) and S.loan_type=40
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

                        Guarantee guarantee = SetGuarantee(row);

                        guarantees.Add(guarantee);
                    }
                }
            }
            return guarantees;
        }

        internal static List<Guarantee> GetClosedGuarantees(ulong customerNumber)
        {
            List<Guarantee> guarantees = new List<Guarantee>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(S.App_Id,0) App_Id,S.connect_account_full_number,S.benefeciar,S.percent_cummulation,s.overdue_capital_nused,
                                S.filialcode,S.date_of_contract_end,cn.contract_number,cn.general_number,
                                S.start_capital,S.currency,S.date_of_beginning,S.date_of_normal_end ,S.interest_rate_nused,
                                ABS(S.current_capital_nused) as current_capital,ABS(S.current_rate_value_nused) as current_rate_value,
                                S.quality, S.security_code_2,
                                S.loan_type,Abs(S.inpaied_rest_of_rate_nused) as inpaied_rest_of_rate,Abs(S.penalty_rate_nused) as penalty_rate,
                                Abs(S.penalty_add_nused) as penalty_add,Abs(S.total_rate_value_nused) as total_rate_value,S.[last_day_of rate calculation] as day_of_rate_calculation,
                                S.confirmator_bank,S.confirmator_bank_end,S.prolonged_dates,S.Keeper_open,S.out_loan_date
                                FROM  Tbl_given_guarantee S
								LEFT JOIN
								Tbl_main_loans_contracts cn
								ON s.ID_Contract = cn.ID_Contract
                                WHERE s.customer_number=@customerNumber and s.quality=40 and S.loan_type=40
                                ORDER BY date_of_normal_end
                                ";
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

                        Guarantee guarantee = SetGuarantee(row);

                        guarantees.Add(guarantee);
                    }
                }
            }
            return guarantees;
        }

        internal static Guarantee GetGuarantee(ulong customerNumber, ulong productId)
        {
            Guarantee guarantee = new Guarantee();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(S.App_Id,0) App_Id,S.connect_account_full_number,S.benefeciar,S.percent_cummulation,s.overdue_capital_nused,
                                S.filialcode,S.date_of_contract_end,cn.contract_number,cn.general_number,
                                S.start_capital,S.currency,S.date_of_beginning,S.date_of_normal_end ,S.interest_rate_nused,
                                ABS(S.current_capital_nused) as current_capital,ABS(S.current_rate_value_nused) as current_rate_value,
                                S.quality, S.security_code_2,
                                S.loan_type,Abs(S.inpaied_rest_of_rate_nused) as inpaied_rest_of_rate,Abs(S.penalty_rate_nused) as penalty_rate,
                                Abs(S.penalty_add_nused) as penalty_add,Abs(S.total_rate_value_nused) as total_rate_value,S.[last_day_of rate calculation] as day_of_rate_calculation,
                                S.confirmator_bank,S.confirmator_bank_end,S.prolonged_dates,S.Keeper_open,S.out_loan_date
                                FROM  Tbl_given_guarantee S
								LEFT JOIN
								Tbl_main_loans_contracts cn
								ON s.ID_Contract = cn.ID_Contract
                                WHERE s.customer_number=@customerNumber and s.App_Id=@appId  and S.loan_type=40
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
                        guarantee = SetGuarantee(row);
                    }
                    else
                    {
                        guarantee = null;
                    }

                }

            }
            return guarantee;
        }

        private static Guarantee SetGuarantee(DataRow row)
        {
            Guarantee guarantee = new Guarantee();

            if (row != null)
            {
                guarantee.FillialCode = Convert.ToInt32(row["filialcode"].ToString());
                guarantee.ProductId = long.Parse(row["app_id"].ToString());
                guarantee.ConnectAccount = Account.GetAccount(ulong.Parse(row["connect_account_full_number"].ToString()));
                guarantee.LoanType = short.Parse(row["loan_type"].ToString());
                guarantee.Quality = short.Parse(row["quality"].ToString());
                guarantee.Currency = row["currency"].ToString();
                guarantee.InterestRate = float.Parse(row["interest_rate_nused"].ToString());
                if (row.Table.Columns["overdue_capital_nused"] != null)
                    guarantee.OverdueCapital = double.Parse(row["overdue_capital_nused"].ToString());
                guarantee.StartCapital = double.Parse(row["start_capital"].ToString());
                guarantee.CurrentCapital = double.Parse(row["current_capital"].ToString());
                guarantee.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
                guarantee.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
                guarantee.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
                guarantee.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
                guarantee.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
                if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
                {
                    guarantee.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
                }
                guarantee.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                guarantee.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                guarantee.ContractNumber = ulong.Parse(row["security_code_2"].ToString());
                guarantee.Benefeciar = Utility.ConvertAnsiToUnicode(row["benefeciar"].ToString());
                guarantee.PercentCummulation = ushort.Parse(row["percent_cummulation"].ToString());

                if (row["date_of_contract_end"] != DBNull.Value)
                    guarantee.ContractEndDate = DateTime.Parse(row["date_of_contract_end"].ToString());

                guarantee.MainContractNumber = row["contract_number"].ToString() == "A" ? row["general_number"].ToString().Substring(0, 9) : row["general_number"] == DBNull.Value ? " " : row["general_number"].ToString();

                if (row["prolonged_dates"] != DBNull.Value)
                {
                    guarantee.ProlongationDates = new List<DateTime>();
                    string[] dates = row["prolonged_dates"].ToString().Split(new[] { ";" }, StringSplitOptions.None);
                    for (int i = 0; i < dates.Length; i++)
                    {
                        try
                        {
                            DateTime date = Convert.ToDateTime(dates[i]);
                            guarantee.ProlongationDates.Add(date);
                        }
                        catch
                        {
                            continue;
                        }

                    }
                }
                if (row["confirmator_bank"] != DBNull.Value)
                    guarantee.ConfirmatorBank = row["confirmator_bank"].ToString();
                if (row["confirmator_bank_end"] != DBNull.Value)
                    guarantee.ConfirmatorBankEndDate = DateTime.Parse(row["confirmator_bank_end"].ToString());
                if (row["Keeper_open"] != DBNull.Value)
                    guarantee.SetNumber = uint.Parse(row["Keeper_open"].ToString());
                guarantee.ProductType = Utility.GetProductTypeFromLoanType(guarantee.LoanType);
                guarantee.CreditCode = LoanProduct.GetCreditCode(guarantee.ProductId, guarantee.ProductType);
                if (row["out_loan_date"] != DBNull.Value)
                {
                    guarantee.OutLoanDate = Convert.ToDateTime(row["out_loan_date"].ToString());
                }


            }
            return guarantee;
        }
        internal static bool CheckFGuaranteeProvisionCurrency(long productId, string currency)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"Select LP.IdPro From Tbl_Link_application_Provision LP 
				                                  Inner join Tbl_provision_of_clients P 
				                                  ON LP.IdPro=P.IdPro 
				                                  Inner join Tbl_type_of_all_provision 
				                                  ON P.[type]=Tbl_type_of_all_provision.type_id
                                                  Where  LP.app_id=@productId and matured_date is null and p.type=13 and p.currency<> @currency", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                SqlDataReader dr = cmd.ExecuteReader();
                if (!dr.HasRows)
                {
                    check = true;
                }
            }
            return check;


        }


        internal static bool HasTransportProvison(long productId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT pr.IdPro FROM Tbl_provision_of_clients pr INNER JOIN  Tbl_Link_application_Provision link on pr.IdPro =  link.IdPro 
                                                 WHERE Type in(5,6) AND matured_date is null AND pr.idpro not in (SELECT idpro FROM Tbl_Link_application_Provision WHERE activated_date is not null AND app_id <>@productId) AND app_id = @productId", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    check = true;
                }
                return check;
            }
        }




        internal static List<GivenGuaranteeReduction> GetGivenGuaranteeReductions(ulong productId)
        {
            List<GivenGuaranteeReduction> givenGuaranteeReductions = new List<GivenGuaranteeReduction>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT * FROM tbl_given_guarantee_reductions where app_id=@appId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;


                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];

                            GivenGuaranteeReduction givenGuaranteeReduction = new GivenGuaranteeReduction();
                            givenGuaranteeReduction.ProductAppId = Convert.ToInt64(row["app_id"]);
                            givenGuaranteeReduction.Quality = Convert.ToUInt16(row["status"]);
                            givenGuaranteeReduction.QualityDescription = givenGuaranteeReduction.Quality == 0 ? "Պայմանագիր" : "Կատարված";
                            givenGuaranteeReduction.ReasonNumber = row["reason_number"].ToString();
                            givenGuaranteeReduction.ReductionAmount = Convert.ToDouble(row["reduction_amount"]);
                            givenGuaranteeReduction.ReductionDate = Convert.ToDateTime(row["reduction_date"]);
                            givenGuaranteeReduction.SetNumber = Convert.ToUInt32(row["set_number"]);
                            givenGuaranteeReduction.ReasonDate = Convert.ToDateTime(row["reason_date"]);
                            givenGuaranteeReductions.Add(givenGuaranteeReduction);
                        }

                    }
                    else
                    {
                        givenGuaranteeReductions = null;
                    }

                }

            }
            return givenGuaranteeReductions;
        }



    }
}
