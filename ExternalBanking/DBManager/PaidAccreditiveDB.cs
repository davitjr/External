using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
   internal class PaidAccreditiveDB
    {
        internal static List<PaidAccreditive> GetPaidAccreditives(ulong customerNumber)
        {
            List<PaidAccreditive> paidAccreditives = new List<PaidAccreditive>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                              S.App_Id,
                                              S.start_capital,
                                              S.currency,
                                              S.date_of_beginning,
                                              S.date_of_normal_end,
                                              S.interest_rate,
                                              ABS(S.current_capital) AS current_capital,
                                              ABS(S.current_rate_value) AS current_rate_value,
                                              S.quality,
                                              LQ.quality AS quality_description_arm,
                                              S.security_code_2,
                                              S.loan_type,
                                              T.description AS loan_type_desription_arm,
                                              ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                              ABS(S.penalty_rate) AS penalty_rate,
                                              ABS(S.penalty_add) AS penalty_add,
                                              ABS(S.total_fee) AS total_fee,
                                              ABS(S.total_rate_value) AS total_rate_value,
                                              S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                              S.Overdue_loan_date_for_classification
                                        FROM Tbl_paid_factoring S
                                        INNER JOIN [tbl_type_of_loans;] T
                                              ON s.loan_type = T.code
                                        INNER JOIN [Tbl_loan_list_quality;] LQ
                                              ON S.Quality = LQ.number
                                        WHERE customer_number = @customerNumber
                                        AND S.quality NOT IN (10, 40)
                                        AND s.loan_type = 35
                                        ORDER BY Date_of_normal_end";
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

                        PaidAccreditive paidAccreditive = SetPaidAccreditive(row);

                        paidAccreditives.Add(paidAccreditive);
                    }
                }

            }
            return paidAccreditives;
        }

        internal static List<PaidAccreditive> GetClosedPaidAccreditives(ulong customerNumber)
        {
            List<PaidAccreditive> paidAccreditives = new List<PaidAccreditive>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.quality,
                                          LQ.quality AS quality_description_arm,
                                          S.security_code_2,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_closed_paid_factoring S
                                    INNER JOIN [tbl_type_of_loans;] T
                                          ON s.loan_type = T.code
                                    INNER JOIN [Tbl_loan_list_quality;] LQ
                                          ON S.Quality = LQ.number
                                    WHERE customer_number = @customerNumber
                                    AND S.quality = 40
                                    AND s.loan_type = 35
                                    ORDER BY Date_of_normal_end";
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

                        PaidAccreditive paidAccreditive = SetPaidAccreditive(row);

                        paidAccreditives.Add(paidAccreditive);
                    }
                }

            }
            return paidAccreditives;
        }

        internal static PaidAccreditive GetPaidAccreditive(ulong customerNumber, ulong productId)
        {
            PaidAccreditive paidAccreditive = new PaidAccreditive();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.quality,
                                          LQ.quality AS quality_description_arm,
                                          S.security_code_2,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_paid_factoring S
                                    INNER JOIN [tbl_type_of_loans;] T
                                          ON s.loan_type = T.code
                                    INNER JOIN [Tbl_loan_list_quality;] LQ
                                          ON S.Quality = LQ.number
                                    WHERE customer_number = @customerNumber
                                    AND S.quality NOT IN (10, 40)
                                    AND s.loan_type = 35
                                    AND app_id = @appId
                                    ORDER BY Date_of_normal_end";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        paidAccreditive = SetPaidAccreditive(row);
                    }
                }
            }
            return paidAccreditive;
        }
        private static PaidAccreditive SetPaidAccreditive(DataRow row)
        {
            PaidAccreditive paidAccreditive = new PaidAccreditive();

            if (row != null)
            {
                paidAccreditive.ProductId = long.Parse(row["app_id"].ToString());
                paidAccreditive.LoanType = short.Parse(row["loan_type"].ToString());
                paidAccreditive.Quality = short.Parse(row["quality"].ToString());
                paidAccreditive.Currency = row["currency"].ToString();
                paidAccreditive.InterestRate = float.Parse(row["interest_rate"].ToString());
                if (row.Table.Columns["out_capital"] != null)
                    paidAccreditive.OutCapital = double.Parse(row["out_capital"].ToString());
                if (row.Table.Columns["judgment_penalty_rate"] != null)
                    paidAccreditive.JudgmentRate = double.Parse(row["judgment_penalty_rate"].ToString());
                if (row.Table.Columns["overdue_capital"] != null)
                    paidAccreditive.OverdueCapital = double.Parse(row["overdue_capital"].ToString());
                if (row.Table.Columns["Subsidia_Current_Rate_Value"] != null)
                    paidAccreditive.SubsidiaCurrentRateValue = double.Parse(row["Subsidia_Current_Rate_Value"].ToString());
                if (row.Table.Columns["current_fee"] != null)
                    paidAccreditive.CurrentFee = double.Parse(row["current_fee"].ToString());
                paidAccreditive.StartCapital = double.Parse(row["start_capital"].ToString());
                paidAccreditive.CurrentCapital = double.Parse(row["current_capital"].ToString());
                paidAccreditive.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
                paidAccreditive.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
                paidAccreditive.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
                paidAccreditive.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
                paidAccreditive.TotalFee = double.Parse(row["total_fee"].ToString());
                paidAccreditive.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
                if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
                {
                    paidAccreditive.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
                }
                paidAccreditive.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                paidAccreditive.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                paidAccreditive.ContractNumber = ulong.Parse(row["security_code_2"].ToString());

                paidAccreditive.ProductType = Utility.GetProductTypeFromLoanType(paidAccreditive.LoanType);
                paidAccreditive.CreditCode = LoanProduct.GetCreditCode(paidAccreditive.ProductId,paidAccreditive.ProductType);
                if (row["Overdue_loan_date_for_classification"] != DBNull.Value)
                {
                    paidAccreditive.OverdueLoanDateForClassification = Convert.ToDateTime(row["Overdue_loan_date_for_classification"].ToString());
                }

            }
            return paidAccreditive;
        }
    }
}
