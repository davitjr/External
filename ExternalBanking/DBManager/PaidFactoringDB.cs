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
    class PaidFactoringDB
    {
        internal static List<PaidFactoring> GetPaidFactorings(ulong customerNumber)
        {
            List<PaidFactoring> paidFactorings = new List<PaidFactoring>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.filialcode,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.quality,
                                          S.security_code_2,
                                          S.loan_type,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          p.Account_number,
                                          dbo.fnc_get_product_account_from_group(S.App_Id, 18, 24) AS connect_account_full_number,
                                          (S.current_capital + S.out_capital) - S.rebate_current_capital AS amount_not_paid,
                                          S.matured_current_rate_value,
                                          S.matured_judgment_penalty_rate,
                                          S.matured_penalty,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_paid_factoring S
                                    INNER JOIN Tbl_Products_Accounts_Groups g
                                          ON s.App_Id = g.App_ID
                                    INNER JOIN Tbl_Products_Accounts p
                                          ON p.Group_ID = g.Group_ID
                                    WHERE customer_number = @customerNumber
                                    AND S.quality NOT IN (40)
                                    AND (s.loan_type = 33
                                    OR s.loan_type = 55)
                                    AND p.Type_of_account = 1
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

                        PaidFactoring paidFactoring = SetPaidFactoring(row);

                        paidFactorings.Add(paidFactoring);
                    }
                }

            }
            return paidFactorings;
        }

        internal static List<PaidFactoring> GetClosedPaidFactorings(ulong customerNumber)
        {
            List<PaidFactoring> paidFactorings = new List<PaidFactoring>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.filialcode,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.quality,
                                          S.security_code_2,
                                          S.loan_type,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          p.Account_number,
                                          dbo.fnc_get_product_account_from_group(S.App_Id, 18, 24) AS connect_account_full_number,
                                          (s.current_capital + s.out_capital) - s.rebate_current_capital AS amount_not_paid,
                                          s.matured_current_rate_value,
                                          s.matured_judgment_penalty_rate,
                                          s.matured_penalty,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_closed_paid_factoring S
                                    INNER JOIN Tbl_Products_Accounts_Groups g
                                          ON s.App_Id = g.App_ID
                                    INNER JOIN Tbl_Products_Accounts p
                                          ON p.Group_ID = g.Group_ID
                                    WHERE customer_number = @customerNumber
                                    AND S.quality = 40
                                    AND (s.loan_type = 33
                                    OR s.loan_type = 55)
                                    AND p.Type_of_account = 1
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

                        PaidFactoring paidFactoring = SetPaidFactoring(row);

                        paidFactorings.Add(paidFactoring);
                    }
                }

            }
            return paidFactorings;
        }

        internal static PaidFactoring GetPaidFactoring(ulong customerNumber, ulong productId)
        {
            PaidFactoring paidFactoring = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"	SELECT   
                                            F2.customer_number as FactoringCustomerNumber,
                                             S.App_Id,
                                             S.filialcode,
                                             S.start_capital,
                                             S.currency,
                                             S.date_of_beginning,
                                             S.date_of_normal_end,
                                             S.interest_rate,
                                             ABS(S.current_capital) AS current_capital,
                                             ABS(S.current_rate_value) AS current_rate_value,
                                             S.quality,
                                             S.security_code_2,
                                             S.loan_type,
                                             ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                             ABS(S.penalty_rate) AS penalty_rate,
                                             ABS(S.penalty_add) AS penalty_add,
                                             ABS(S.total_fee) AS total_fee,
                                             ABS(S.total_rate_value) AS total_rate_value,
                                             S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                             p.Account_number,
                                             dbo.fnc_get_product_account_from_group(S.App_Id, 18, 24) AS connect_account_full_number,
                                             (S.current_capital + S.out_capital) - S.rebate_current_capital AS amount_not_paid,
                                             S.matured_current_rate_value,
                                             S.matured_judgment_penalty_rate,
                                             S.matured_penalty,
                                             S.Overdue_loan_date_for_classification
                                             FROM Tbl_paid_factoring S
                                             INNER JOIN tbl_factoring F1 on s.main_app_id = F1.App_Id
	                                         INNER JOIN tbl_factoring F2 on F1.main_app_id = F2.app_id
	                                         INNER JOIN Tbl_Products_Accounts_Groups g
	                                         		ON s.App_Id = g.App_ID
	                                         INNER JOIN Tbl_Products_Accounts p
	                                         		ON p.Group_ID = g.Group_ID
	                                         WHERE s.customer_number = @customerNumber
	                                         AND S.quality NOT IN (40)
	                                         AND (s.loan_type = 33
	                                         OR s.loan_type = 55)
	                                         AND s.app_id =  @appId
	                                         AND p.Type_of_account = 1
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

                        paidFactoring = SetPaidFactoring(row);
                        paidFactoring.FactoringCustomerNumber = long.Parse(row["FactoringCustomerNumber"].ToString());

                    }
                }
            }
            return paidFactoring;
        }


        internal static PaidFactoring GetClosedPaidFactoring(ulong customerNumber, ulong productId)
        {
            PaidFactoring paidFactoring = new PaidFactoring();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"
                               SELECT
                                          S.App_Id,
                                          S.filialcode,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.quality,
                                          S.security_code_2,
                                          S.loan_type,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          p.Account_number,
                                          dbo.fnc_get_product_account_from_group(S.App_Id, 18, 24) AS connect_account_full_number,
                                          (S.current_capital + S.out_capital) - S.rebate_current_capital AS amount_not_paid,
                                          S.matured_current_rate_value,
                                          S.matured_judgment_penalty_rate,
                                          S.matured_penalty,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_closed_paid_factoring S
                                    INNER JOIN Tbl_Products_Accounts_Groups g
                                          ON s.App_Id = g.App_ID
                                    INNER JOIN Tbl_Products_Accounts p
                                          ON p.Group_ID = g.Group_ID
                                    WHERE customer_number = @customerNumber
                                    AND S.quality = 40
                                    AND (s.loan_type = 33
                                    OR s.loan_type = 55)
                                    AND s.app_id = @appId
                                    AND p.Type_of_account = 1
                                    ORDER BY Date_of_normal_end ";
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

                        paidFactoring = SetPaidFactoring(row);
                    }
                }
            }
            return paidFactoring;
        }


        private static PaidFactoring SetPaidFactoring(DataRow row)
        {
            PaidFactoring paidFactoring = new PaidFactoring();

            if (row != null)
            {
                paidFactoring.ProductId = long.Parse(row["app_id"].ToString());
                paidFactoring.LoanType = short.Parse(row["loan_type"].ToString());
                paidFactoring.Quality = short.Parse(row["quality"].ToString());
                paidFactoring.Currency = row["currency"].ToString();
                paidFactoring.InterestRate = float.Parse(row["interest_rate"].ToString());
                if (row.Table.Columns["out_capital"] != null)
                    paidFactoring.OutCapital = double.Parse(row["out_capital"].ToString());
                if (row.Table.Columns["judgment_penalty_rate"] != null)
                    paidFactoring.JudgmentRate = double.Parse(row["judgment_penalty_rate"].ToString());
                if (row.Table.Columns["overdue_capital"] != null)
                    paidFactoring.OverdueCapital = double.Parse(row["overdue_capital"].ToString());
                if (row.Table.Columns["Subsidia_Current_Rate_Value"] != null)
                    paidFactoring.SubsidiaCurrentRateValue = double.Parse(row["Subsidia_Current_Rate_Value"].ToString());
                if (row.Table.Columns["current_fee"] != null)
                    paidFactoring.CurrentFee = double.Parse(row["current_fee"].ToString());
                paidFactoring.StartCapital = double.Parse(row["start_capital"].ToString());
                paidFactoring.CurrentCapital = double.Parse(row["current_capital"].ToString());
                paidFactoring.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
                paidFactoring.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
                paidFactoring.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
                paidFactoring.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
                paidFactoring.TotalFee = double.Parse(row["total_fee"].ToString());
                paidFactoring.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
                if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
                {
                    paidFactoring.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
                }
                paidFactoring.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                paidFactoring.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                paidFactoring.ContractNumber = ulong.Parse(row["security_code_2"].ToString());

                if (row["Account_number"] != DBNull.Value)
                {
                    paidFactoring.LoanAccount = Account.GetAccount(row["Account_number"].ToString());
                }

                paidFactoring.ConnectAccount = Account.GetAccount(ulong.Parse(row["connect_account_full_number"].ToString()));
                paidFactoring.AmountNotPaid = double.Parse(row["amount_not_paid"].ToString());


                if (row["matured_current_rate_value"] != DBNull.Value)
                {
                    paidFactoring.MaturedCurrentRateValue = double.Parse(row["matured_current_rate_value"].ToString());
                }

                if (row["matured_judgment_penalty_rate"] != DBNull.Value)
                {
                    paidFactoring.MaturedJudgmentPenaltyRate = double.Parse(row["matured_judgment_penalty_rate"].ToString());
                }

                if (row["matured_penalty"] != DBNull.Value)
                {
                    paidFactoring.MaturedPenaltyRate = double.Parse(row["matured_penalty"].ToString());
                }

                paidFactoring.ProductType = Utility.GetProductTypeFromLoanType(paidFactoring.LoanType);
                paidFactoring.CreditCode = LoanProduct.GetCreditCode(paidFactoring.ProductId, paidFactoring.ProductType);
                paidFactoring.FillialCode = int.Parse(row["filialcode"].ToString());
                paidFactoring.NextRepayment = new LoanRepaymentGrafik();
                if (row["Overdue_loan_date_for_classification"] != DBNull.Value)
                {
                    paidFactoring.OverdueLoanDateForClassification = Convert.ToDateTime(row["Overdue_loan_date_for_classification"].ToString());
                }

            }
            return paidFactoring;
        }
    }
}
