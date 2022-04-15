using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class TaxDB
    {
        internal static Tax GetTax(int claimNumber, int eventNumber)
        {
            DataTable dt = new DataTable();
            Tax tax = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select * From Tbl_problem_loan_taxes  Where Claim_Number=@claimNumber And Event_Number=@eventNumber", conn))
                {
                    cmd.Parameters.Add("@claimNumber", SqlDbType.Int).Value = claimNumber;
                    cmd.Parameters.Add("@eventNumber", SqlDbType.Int).Value = eventNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        tax = new Tax();
                        DataRow row = dt.Rows[0];
                        tax.ClaimNumber = int.Parse(row["claim_number"].ToString());
                        tax.EventNumber = int.Parse(row["event_number"].ToString());
                        tax.SetNumber = int.Parse(row["tax_set_number"].ToString());
                        tax.RegistrationDate = DateTime.Parse(row["tax_registration_date"].ToString());
                        tax.Type = short.Parse(row["tax_type"].ToString());
                        tax.Amount = double.Parse(row["tax_amount"].ToString());
                        tax.Purpose = row["tax_purpose"] != DBNull.Value ? Utility.ConvertAnsiToUnicode(row["tax_purpose"].ToString()) : null;
                        tax.MaturedAmount = row["tax_matured_amount"] != DBNull.Value ? double.Parse(row["tax_matured_amount"].ToString()) : 0;
                        tax.Quality = short.Parse(row["tax_quality"].ToString());
                        tax.Percent = row["tax_percent"] != DBNull.Value ? double.Parse(row["tax_percent"].ToString()) : 0;
                        tax.ProductId = ulong.Parse(row["app_id"].ToString());
                        tax.ConcedeAmount = double.Parse(row["tax_concede_amount"].ToString());
                        tax.TransferRegistrationDate = row["transfer_registration_date"] != DBNull.Value ? DateTime.Parse(row["transfer_registration_date"].ToString()) : default(DateTime);

                        if (row["transfer_unic_number"] != DBNull.Value)
                            tax.TransferUnicnumber = Convert.ToInt32(row["transfer_unic_number"]);

                        if (row["tax_concede_add_inf"] != DBNull.Value)
                            tax.ConcedeAddInf = row["tax_concede_add_inf"].ToString();

                        if (row["tax_court_decision"] != DBNull.Value)
                            tax.CourtDecision = Convert.ToInt16(row["tax_court_decision"]);

                        if (row["Tax_Court_decision_date"] != DBNull.Value)
                            tax.CourtDecisionDate = Convert.ToDateTime(row["Tax_Court_decision_date"]);

                        if (row["out_loan_date"] != DBNull.Value)
                            tax.OutLoanDate = Convert.ToDateTime(row["out_loan_date"]);
                    }
                }


            }
            return tax;
        }

        /// <summary>
        /// Պետ. տուրքի հաշվարկ
        /// </summary>
        /// <param name="claimNumber"></param>
        /// <param name="eventNumber"></param>
        /// <returns></returns>
        internal static ProblemLoanCalculationsDetail GetProblemLoanCalculationsDetail(int claimNumber, int eventNumber)
        {
            ProblemLoanCalculationsDetail problemLoanCalculationsDetail = new ProblemLoanCalculationsDetail();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"Select * from tbl_Problem_loan_calculations Where Claim_number = @claimNumber And Event_number =@eventNumber", conn))
                {
                    cmd.Parameters.Add("@claimNumber", SqlDbType.Int).Value = claimNumber;
                    cmd.Parameters.Add("@eventNumber", SqlDbType.Int).Value = eventNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        problemLoanCalculationsDetail.ClaimCalculationDate = Convert.ToDateTime(row["claim_calculation_date"]);
                        problemLoanCalculationsDetail.ClaimCalculationNumber = Convert.ToUInt64(row["claim_calculation_number"]);
                        problemLoanCalculationsDetail.ClaimNumber = Convert.ToInt32(row["claim_number"]);
                        if (row["course"] != DBNull.Value)
                            problemLoanCalculationsDetail.Course = Convert.ToDouble(row["course"]);
                        problemLoanCalculationsDetail.CurrentCapital = Math.Abs(Convert.ToDouble(row["current_capital"]));
                        problemLoanCalculationsDetail.CurrentFee = Convert.ToDouble(row["current_fee"]);
                        problemLoanCalculationsDetail.CurrentRateValue = Math.Abs(Convert.ToDouble(row["current_rate_value"]));
                        if (row["debt_amount"] != DBNull.Value)
                            problemLoanCalculationsDetail.DebtAmount = Convert.ToDouble(row["debt_amount"]);
                        problemLoanCalculationsDetail.EventNumber = Convert.ToInt32(row["event_number"]);
                        problemLoanCalculationsDetail.InpaiedRestOfRate = Convert.ToDouble(row["inpaied_rest_of_rate"]);
                        if (row["last_day_of rate calculation"] != DBNull.Value)
                            problemLoanCalculationsDetail.LastDateOfRateCalculation = Convert.ToDateTime(row["last_day_of rate calculation"]);
                        problemLoanCalculationsDetail.OverdueCapital = Convert.ToDouble(row["overdue_capital"]);
                        problemLoanCalculationsDetail.PenaltyAdd = Convert.ToDouble(row["penalty_add"]);
                        problemLoanCalculationsDetail.PenaltyRate = Convert.ToDouble(row["penalty_rate"]);
                    }
                }

            }

            return problemLoanCalculationsDetail;

        }



        internal static double GetPetTurk(long productId)
        {
            double fee = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT isnull(sum(tax_amount - isnull(tax_matured_amount,0) - ISNULL(tax_concede_amount,0) ),-1) as pet_turq
				                            FROM  V_ProblemLoanTaxes   
                                where app_id = @app_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                    fee = Convert.ToDouble(cmd.ExecuteScalar());
                    return fee;
                }
            }
        }
    }
}
