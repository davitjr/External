using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Linq;
using System.Data.SqlTypes;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    internal class LoanDB
    {

        internal static List<Loan> GetLoans(ulong customerNumber)
        {
            List<Loan> loanList = new List<Loan>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.loan_full_number,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.contract_date,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          S.sum_of_given_money,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          ABS(current_fee) AS current_fee,
                                          S.quality,
                                          CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN N'Տրամադրված է 24/7 եղանակով' else LQ.quality end AS quality_description_arm,
                                          S.security_code_2,
                                          ABS(out_capital) AS out_capital,
                                          ABS(overdue_capital) AS overdue_capital,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(total_judgment_penalty_rate) AS total_judgment_penalty_rate,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          overdue_loan_date,
                                          last_day_of_rate_repair,
                                          connect_account_full_number,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          subsidia_interest_rate,
                                          changed_rate,
                                          advanced_repayment_rate,
                                          first_start_capital,
                                          date_of_beginning_second_period,
                                          S.filialcode,
                                          S.kind_of_money,
                                          S.loan_program,
                                          S.action,
                                          S.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          S.matured_current_rate_value,
                                          S.matured_penalty,
                                          S.matured_judgment_penalty_rate,
                                          S.matured_current_fee,
                                          S.date_of_stopping_calculation,
                                          S.date_of_stopping_penalty_calculation,
                                          S.Overdue_loan_date_for_classification,
                                          S.out_loan_date,
									      S.out_from_outbal_date,
                                          t.Description_Engl,
                                          CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN 1 else 0 end as is_24_7
                                          FROM [tbl_short_time_loans;] S
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON s.loan_type = T.code
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON S.Quality = LQ.number
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = s.App_Id
                                          LEFT JOIN (SELECT doc_id,app_id  FROM (SELECT doc_id FROM  dbo.Tbl_HB_documents  WHERE  
                                        customer_number = @customerNumber AND quality = 20 ) HB
                                        INNER JOIN dbo.Tbl_HB_Products_Identity IDENT ON
                                        HB.doc_id = IDENT.HB_doc_id) CONTRACTS24_7 
										ON s.app_id = CONTRACTS24_7.app_id
                                    WHERE s.Customer_Number = @customerNumber
                                    ORDER BY Date_of_normal_end";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();


                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);

                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        Loan loan = SetLoan(row);

                        loanList.Add(loan);
                    }

                }
            }

            return loanList;
        }

        internal static List<Loan> GetClosedLoans(ulong customerNumber)
        {
            List<Loan> loanList = new List<Loan>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          ISNULL(S.App_Id, 0) AS App_id,
                                          S.loan_full_number,
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
                                          ABS(out_capital) AS out_capital,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(total_judgment_penalty_rate) AS total_judgment_penalty_rate,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          overdue_loan_date,
                                          last_day_of_rate_repair,
                                          connect_account_full_number,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          subsidia_interest_rate,
                                          changed_rate,
                                          advanced_repayment_rate,
                                          first_start_capital,
                                          date_of_beginning_second_period,
                                          S.filialcode,
                                          S.kind_of_money,
                                          S.loan_program,
                                          S.action,
                                          S.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          S.matured_current_rate_value,
                                          S.matured_penalty,
                                          S.matured_judgment_penalty_rate,
                                          S.matured_current_fee,
                                          S.date_of_stopping_calculation,
                                          S.date_of_stopping_penalty_calculation,
                                          S.Overdue_loan_date_for_classification,
                                          S.out_loan_date,
									      S.out_from_outbal_date
                                          FROM Tbl_closed_short_loans S
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON s.loan_type = T.code
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON S.Quality = LQ.number
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = s.App_Id
                                    WHERE s.Customer_Number = @customerNumber
                                    AND S.quality = 40
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

                        Loan loan = SetLoan(row);

                        loanList.Add(loan);
                    }

                }
            }

            return loanList;
        }

        internal static List<Loan> GetAparikTexumLoans(ulong customerNumber)
        {
            List<Loan> loanList = new List<Loan>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          f.App_Id,
                                          dbo.fnc_get_product_account_from_group(F.app_id, CASE
                                                WHEN F.Loan_type = 38 THEN 58
                                                ELSE 111
                                          END, 1) AS loan_full_number,
                                          start_capital,
                                          currency,
                                          f.date_of_beginning,
                                          date_of_normal_end,
                                          interest_rate,
                                          ABS(current_capital) AS current_capital,
                                          ABS(current_rate_value) AS current_rate_value,
                                          f.quality,
                                          LQ.quality AS quality_description_arm,
                                          security_code_2,
                                          ABS(out_capital) AS out_capital,
                                          ABS(current_fee) AS current_fee,
                                          loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(F.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(F.penalty_rate) AS penalty_rate,
                                          ABS(F.penalty_add) AS penalty_add,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(F.total_fee) AS total_fee,
                                          ABS(F.total_rate_value) AS total_rate_value,
                                          F.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          last_day_of_rate_repair,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          overdue_loan_date,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          dbo.fnc_get_product_account_from_group(f.App_Id, CASE
                                                WHEN F.Loan_type = 38 THEN 18
                                                ELSE 10
                                          END, 24) AS connect_account_full_number,
                                          NULL AS subsidia_interest_rate,
                                          changed_rate,
                                          0 AS advanced_repayment_rate,
                                          0 AS first_start_capital,
                                          NULL AS date_of_beginning_second_period,
                                          F.filialcode,
                                          F.kind_of_money,
                                          0 AS loan_program,
                                          0 AS action,
                                          F.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          f.matured_current_fee,
                                          f.matured_current_rate_value,
                                          f.matured_judgment_penalty_rate,
                                          f.matured_penalty,
                                          F.date_of_stopping_calculation,
                                          F.date_of_stopping_penalty_calculation,
                                          F.Overdue_loan_date_for_classification,
                                          F.out_loan_date,
									      F.out_from_outbal_date,
                                          ABS(overdue_capital) AS overdue_capital
                                          FROM [Tbl_Paid_factoring] F
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON F.Quality = LQ.number
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON F.loan_type = T.code
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = f.App_Id
                                    WHERE (F.Loan_type = 38
                                    OR F.Loan_type = 49
                                    OR F.Loan_type = 49)
                                    AND F.quality <> 40
                                    AND f.Customer_Number = @customerNumber
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

                        Loan loan = SetLoan(row);

                        loanList.Add(loan);
                    }
                }
            }

            return loanList;
        }

        internal static byte[] LoansDramContract(string accountNumber)
        {
            int docid = 0;

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT HB_doc_Id 
														 FROM 
                                                            (
															    SELECT HB_doc_Id, loan_full_number
														        FROM [Tbl_short_time_loans;]
														        WHERE ISNULL(HB_doc_ID,0) <> 0
														        UNION ALL
														        SELECT HB_doc_Id, loan_full_number
														        FROM tbl_credit_lines
														        WHERE ISNULL(HB_doc_ID,0) <> 0
														    ) AS L
														 WHERE loan_full_number  = @accountNumber", conn))
                {
                    cmd.Parameters.Add("@accountNumber", SqlDbType.Float).Value = accountNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        docid = Convert.ToInt32(dt.Rows[0]["hb_doc_id"].ToString());
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return UploadedFile.GetAttachedFile(docid, 10);

        }

        internal static List<Loan> GetAparikTexumClosedLoans(ulong customerNumber)
        {
            List<Loan> loanList = new List<Loan>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"     SELECT
                                              f.App_Id,
                                              pr.Account_number AS loan_full_number,
                                              start_capital,
                                              currency,
                                              f.date_of_beginning,
                                              date_of_normal_end,
                                              interest_rate,
                                              ABS(current_capital) AS current_capital,
                                              ABS(current_rate_value) AS current_rate_value,
                                              f.quality,
                                              LQ.quality AS quality_description_arm,
                                              security_code_2,
                                              ABS(out_capital) AS out_capital,
                                              loan_type,
                                              T.description AS loan_type_desription_arm,
                                              ABS(F.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                              ABS(F.penalty_rate) AS penalty_rate,
                                              ABS(F.penalty_add) AS penalty_add,
                                              ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                              ABS(F.total_fee) AS total_fee,
                                              ABS(F.total_rate_value) AS total_rate_value,
                                              F.[last_day_of rate calculation] AS day_of_rate_calculation,
                                              overdue_loan_date,
                                              last_day_of_rate_repair,
                                              date_of_judgment_begin,
                                              date_of_judgment_end,
                                              judgment_penalty_percent,
                                              interest_rate_effective,
                                              interest_rate_full,
                                              cn.Account_number AS connect_account_full_number,
                                              NULL AS subsidia_interest_rate,
                                              changed_rate,
                                              0 AS advanced_repayment_rate,
                                              0 AS first_start_capital,
                                              NULL AS date_of_beginning_second_period,
                                              F.filialcode,
                                              F.kind_of_money,
                                              0 AS loan_program,
                                              0 AS action,
                                              F.penalty_add_percent,
                                              ad.interest_rate_effective_with_only_bank_profit,
                                              ad.interest_rate_effective_without_account_service_fee,
                                              f.matured_current_fee,
                                              f.matured_current_rate_value,
                                              f.matured_judgment_penalty_rate,
                                              f.matured_penalty,
                                              F.date_of_stopping_calculation,
                                              F.date_of_stopping_penalty_calculation,
                                              F.Overdue_loan_date_for_classification,
                                              F.out_loan_date,
									          F.out_from_outbal_date
                                              FROM [Tbl_closed_paid_factoring] F
                                              INNER JOIN [Tbl_loan_list_quality;]
                                              LQ
                                                    ON F.Quality = LQ.number
                                              INNER JOIN [tbl_type_of_loans;] T
                                                    ON F.loan_type = T.code
                                              LEFT JOIN Tbl_liability_add ad
                                                    ON ad.app_id = f.App_Id

                                              CROSS APPLY (SELECT
                                                    Account_number
                                              FROM Tbl_Products_Accounts a
                                              INNER JOIN Tbl_Products_Accounts_Groups g
                                                    ON a.Group_Id = g.Group_ID
                                              WHERE g.group_status = 0
                                              AND g.App_ID = f.App_Id
                                              AND a.Type_of_product =
                                                                     CASE
                                                                           WHEN F.Loan_type = 38 THEN 58
                                                                           ELSE 111
                                                                     END
                                              AND a.Type_of_account = 1) pr

                                              CROSS APPLY (SELECT
                                                    Account_number
                                              FROM Tbl_Products_Accounts a
                                              INNER JOIN Tbl_Products_Accounts_Groups g
                                                    ON a.Group_Id = g.Group_ID
                                              WHERE g.group_status = 0
                                              AND g.App_ID = f.App_Id
                                              AND a.Type_of_product =
                                                                     CASE
                                                                           WHEN F.Loan_type = 38 THEN 18
                                                                           ELSE 10
                                                                     END
                                              AND a.Type_of_account = 24) cn

                                        WHERE f.Customer_Number = @customerNumber
                                        AND (F.Loan_type = 38
                                        OR F.Loan_type = 49)
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

                        Loan loan = SetLoan(row);

                        loanList.Add(loan);
                    }
                }
            }

            return loanList;
        }


        internal static Loan GetLoan(ulong productId, ulong customerNumber)
        {
            Loan loan = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.loan_full_number,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.contract_date,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          S.sum_of_given_money,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.security_code_2,
                                          S.quality,
                                          CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN N'Տրամադրված է 24/7 եղանակով' else LQ.quality end AS quality_description_arm,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(out_capital) AS out_capital,
                                          ABS(current_fee) AS current_fee,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(Subsidia_Current_Rate_Value) AS Subsidia_Current_Rate_Value,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(total_judgment_penalty_rate) AS total_judgment_penalty_rate,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          ABS(overdue_capital) AS overdue_capital,
                                          ABS(S.total_fee) AS total_fee,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          overdue_loan_date,
                                          last_day_of_rate_repair,
                                          connect_account_full_number,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          subsidia_interest_rate,
                                          changed_rate,
                                          advanced_repayment_rate,
                                          first_start_capital,
                                          date_of_beginning_second_period,
                                          S.filialcode,
                                          S.kind_of_money,
                                          S.loan_program,
                                          S.action,
                                          S.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          s.matured_current_rate_value,
                                          s.matured_penalty,
                                          s.matured_judgment_penalty_rate,
                                          s.matured_current_fee,
                                          s.date_of_stopping_calculation,
                                          s.date_of_stopping_penalty_calculation,
                                          S.Overdue_loan_date_for_classification,
                                          s.out_loan_date,
									      s.out_from_outbal_date,
                                           CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN 1 else 0 end as is_24_7
                                          FROM [Tbl_short_time_loans;] S
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON s.loan_type = T.code
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON S.Quality = LQ.number
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = s.App_Id
                                            LEFT JOIN (SELECT doc_id,app_id  FROM (SELECT doc_id FROM  dbo.Tbl_HB_documents  WHERE  
                                        customer_number = @customerNumber AND quality = 20 ) HB
                                        INNER JOIN dbo.Tbl_HB_Products_Identity IDENT ON
                                        HB.doc_id = IDENT.HB_doc_id) CONTRACTS24_7 
										ON s.app_id = CONTRACTS24_7.app_id	
                                    WHERE s.app_id = @appId
                                    AND s.customer_number = @customerNumber
                                    ORDER BY Date_of_normal_end";
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
                        loan = SetLoan(row);
                    }
                    else
                    {
                        loan = null;
                    }

                }
            }

            return loan;
        }

        internal static Loan GetAparikTexumLoan(ulong productId, ulong customerNumber)
        {
            Loan loan = null;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          f.App_Id,
                                          dbo.fnc_get_product_account_from_group(F.app_id, CASE
                                                WHEN F.Loan_type = 38 THEN 58
                                                ELSE CASE
                                                            WHEN F.Loan_type = 33 OR
                                                                  F.Loan_type = 55 THEN 54
                                                            ELSE 111
                                                      END
                                          END, 1) AS loan_full_number,
                                          start_capital,
                                          currency,
                                          f.date_of_beginning,
                                          date_of_normal_end,
                                          interest_rate,
                                          ABS(current_capital) AS current_capital,
                                          ABS(current_rate_value) AS current_rate_value,
                                          f.quality,
                                          LQ.quality AS quality_description_arm,
                                          security_code_2,
                                          loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(F.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(F.penalty_rate) AS penalty_rate,
                                          ABS(out_capital) AS out_capital,
                                          ABS(current_fee) AS current_fee,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(overdue_capital) AS overdue_capital,
                                          ABS(F.penalty_add) AS penalty_add,
                                          ABS(F.total_rate_value) AS total_rate_value,
                                          ABS(F.total_fee) AS total_fee,
                                          F.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          overdue_loan_date,
                                          last_day_of_rate_repair,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          dbo.fnc_get_product_account_from_group(f.App_Id, CASE
                                                WHEN F.Loan_type = 38 OR
                                                      F.Loan_type = 33 OR
                                                      F.Loan_type = 55 THEN 18
                                                ELSE 10
                                          END, 24) AS connect_account_full_number,
                                          NULL AS subsidia_interest_rate,
                                          changed_rate,
                                          0 AS advanced_repayment_rate,
                                          0 AS first_start_capital,
                                          NULL AS date_of_beginning_second_period,
                                          F.filialcode,
                                          F.kind_of_money,
                                          0 AS loan_program,
                                          0 AS action,
                                          F.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          f.matured_current_fee,
                                          f.matured_current_rate_value,
                                          f.matured_judgment_penalty_rate,
                                          f.matured_penalty,
                                          f.date_of_stopping_calculation,
                                          F.date_of_stopping_penalty_calculation,
                                          F.Overdue_loan_date_for_classification, 
                                          F.out_loan_date,
									      F.out_from_outbal_date
                                          FROM [Tbl_Paid_factoring] F
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON F.Quality = LQ.number
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON F.loan_type = T.code
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = f.App_Id
                                    WHERE (F.Loan_type = 38
                                    OR F.Loan_type = 49
                                    OR F.Loan_type = 33
                                    OR F.Loan_type = 55)
                                    AND F.quality <> 40
                                    AND f.Customer_Number = @customerNumber
                                    AND f.app_id = @appId
                                    ORDER BY Date_of_normal_end";
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
                        loan = SetLoan(row);
                    }
                }
            }

            return loan;
        }

        private static Loan SetLoan(DataRow row)
        {
            Loan loan = new Loan();

            if (row != null)
            {

                loan.ProductId = long.Parse(row["app_id"].ToString());
                loan.LoanAccount = Account.GetAccount(ulong.Parse(row["loan_full_number"].ToString()));
                loan.LoanType = short.Parse(row["loan_type"].ToString());
                loan.Quality = short.Parse(row["quality"].ToString());
                loan.Currency = row["currency"].ToString();
                loan.InterestRate = float.Parse(row["interest_rate"].ToString());
                loan.LoanTypeDescription = Utility.ConvertAnsiToUnicode(row["loan_type_desription_arm"].ToString());
                loan.QualityDescription = Utility.ConvertAnsiToUnicode(row["quality_description_arm"].ToString());
                if (row.Table.Columns["out_capital"] != null)
                    loan.OutCapital = double.Parse(row["out_capital"].ToString());
                if (row.Table.Columns["judgment_penalty_rate"] != null)
                    loan.JudgmentRate = double.Parse(row["judgment_penalty_rate"].ToString());
                if (row.Table.Columns["total_judgment_penalty_rate"] != null)
                    loan.TotalJudgmentPenaltyRate = double.Parse(row["total_judgment_penalty_rate"].ToString());
                if (row.Table.Columns["overdue_capital"] != null)
                    loan.OverdueCapital = double.Parse(row["overdue_capital"].ToString());
                if (row.Table.Columns["Subsidia_Current_Rate_Value"] != null)
                    loan.SubsidiaCurrentRateValue = double.Parse(row["Subsidia_Current_Rate_Value"].ToString());
                if (row.Table.Columns["current_fee"] != null)
                    loan.CurrentFee = double.Parse(row["current_fee"].ToString());
                loan.StartCapital = double.Parse(row["start_capital"].ToString());
                loan.CurrentCapital = double.Parse(row["current_capital"].ToString());
                loan.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
                loan.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
                loan.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
                loan.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
                loan.TotalFee = double.Parse(row["total_fee"].ToString());
                loan.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
                if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
                {
                    loan.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
                }
                loan.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                loan.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
                loan.ContractNumber = ulong.Parse(row["security_code_2"].ToString());
                loan.LastDateOfRateRepair = row["last_day_of_rate_repair"] != DBNull.Value ? DateTime.Parse(row["last_day_of_rate_repair"].ToString()) : default(DateTime?);
                loan.OverdueLoanDate = row["overdue_loan_date"] != DBNull.Value ? DateTime.Parse(row["overdue_loan_date"].ToString()) : default(DateTime?);
                loan.JudgmentStartDate = row["date_of_judgment_begin"] != DBNull.Value ? DateTime.Parse(row["date_of_judgment_begin"].ToString()) : default(DateTime?);
                loan.JudgmentEndDate = row["date_of_judgment_end"] != DBNull.Value ? DateTime.Parse(row["date_of_judgment_end"].ToString()) : default(DateTime?);
                loan.JudgmentPenaltyPercent = float.Parse(row["judgment_penalty_percent"].ToString());
                loan.InterestRateEffective = float.Parse(row["interest_rate_effective"].ToString());
                loan.InterestRateFull = row["interest_rate_full"] != DBNull.Value ? float.Parse(row["interest_rate_full"].ToString()) : 0;
                loan.ConnectAccount = Account.GetAccount(ulong.Parse(row["connect_account_full_number"].ToString()));
                loan.DailyPenaltyInterestRate = float.Parse(row["penalty_add_percent"].ToString());

                if (row["subsidia_interest_rate"] != DBNull.Value)
                {
                    loan.SubsidiaInterestRate = double.Parse(row["subsidia_interest_rate"].ToString());
                }
                if (row["changed_rate"] != DBNull.Value)
                {
                    loan.ChangeRate = short.Parse(row["changed_rate"].ToString());
                }
                if (row["advanced_repayment_rate"] != DBNull.Value)
                {
                    loan.AdvancedRepaymentRate = double.Parse(row["advanced_repayment_rate"].ToString());
                }
                if (row["first_start_capital"] != DBNull.Value)
                {
                    loan.FirstStartCapital = double.Parse(row["first_start_capital"].ToString());
                }
                if (row["date_of_beginning_second_period"] != DBNull.Value)
                {
                    loan.DateOfBeginningSecondPeriod = DateTime.Parse(row["date_of_beginning_second_period"].ToString());
                }

                loan.Fond = short.Parse(row["kind_of_money"].ToString());
                if (row["action"] != DBNull.Value)
                {
                    loan.Sale = short.Parse(row["action"].ToString());
                }

                loan.LoanProgram = short.Parse(row["loan_program"].ToString());

                loan.FillialCode = int.Parse(row["filialcode"].ToString());
                loan.ProductType = 1;
                loan.HasClaim = LoanProduct.CheckLoanProductClaimAvailability(loan.ProductId);

                if (row["interest_rate_effective_with_only_bank_profit"] != DBNull.Value)
                {
                    loan.InterestRateEffectiveWithOnlyBankProfit = double.Parse(row["interest_rate_effective_with_only_bank_profit"].ToString());
                }
                if (row["interest_rate_effective_without_account_service_fee"] != DBNull.Value)
                {
                    loan.InterestRateEffectiveWithoutAccountServiceFee = double.Parse(row["interest_rate_effective_without_account_service_fee"].ToString());
                }

                if (row["matured_current_fee"] != DBNull.Value)
                {
                    loan.MaturedCurrentFee = double.Parse(row["matured_current_fee"].ToString());
                }

                if (row["matured_current_rate_value"] != DBNull.Value)
                {
                    loan.MaturedCurrentRateValue = double.Parse(row["matured_current_rate_value"].ToString());
                }

                if (row["matured_judgment_penalty_rate"] != DBNull.Value)
                {
                    loan.MaturedJudgmentPenaltyRate = double.Parse(row["matured_judgment_penalty_rate"].ToString());
                }

                if (row["matured_penalty"] != DBNull.Value)
                {
                    loan.MaturedPenaltyRate = double.Parse(row["matured_penalty"].ToString());
                }

                loan.CreditCode = LoanProduct.GetCreditCode(loan.ProductId, loan.ProductType);
                loan.CheckAdvancedRepaymentRate = CheckAdvancedRepaymentRate(loan.ProductId);

                if (row["date_of_stopping_calculation"] != DBNull.Value)
                {
                    loan.DateOfStoppingCalculation = Convert.ToDateTime(row["date_of_stopping_calculation"].ToString());
                }

                if (row["date_of_stopping_penalty_calculation"] != DBNull.Value)
                {
                    loan.DateOfStoppingPenaltyCalculation = Convert.ToDateTime(row["date_of_stopping_penalty_calculation"].ToString());
                }
                if (row["Overdue_loan_date_for_classification"] != DBNull.Value)
                {
                    loan.OverdueLoanDateForClassification = Convert.ToDateTime(row["Overdue_loan_date_for_classification"].ToString());
                }
                if (row["out_loan_date"] != DBNull.Value)
                {
                    loan.OutLoanDate = Convert.ToDateTime(row["out_loan_date"].ToString());
                }
                if (row["out_from_outbal_date"] != DBNull.Value)
                {
                    loan.OutFromOutbalDate = Convert.ToDateTime(row["out_from_outbal_date"].ToString());
                }

                if (row.Table.Columns.Contains("Description_Engl"))
                {
                    loan.LoanTypeDescriptionEng = row["Description_Engl"].ToString();
                }
                else
                    loan.LoanTypeDescriptionEng = "";

                if (row.Table.Columns.Contains("is_24_7"))
                {
                    loan.Is_24_7 = Convert.ToBoolean(row["is_24_7"]);
                }

                if (row.Table.Columns.Contains("contract_date") && row["contract_date"] != DBNull.Value)
                {
                    loan.ContractDate = Convert.ToDateTime(row["contract_date"]);
                }
                if (row.Table.Columns.Contains("sum_of_given_money") && row["sum_of_given_money"] != DBNull.Value)
                {
                    loan.ContractAmount = Convert.ToDouble(row["sum_of_given_money"]);
                }


            }
            return loan;
        }
        /// <summary>
        /// Վարկի գրաֆիկ
        /// </summary>
        /// <returns></returns>
        public static List<LoanRepaymentGrafik> GetLoanGrafik(Loan loan)
        {
            List<LoanRepaymentGrafik> list = new List<LoanRepaymentGrafik>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "select * from [dbo].[fn_get_loan_repayment_new_schedule_by_app_id](@appId,@set_date,@Tot)";
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = loan.ProductId;
                    cmd.Parameters.Add("@set_date", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();
                    cmd.Parameters.Add("@Tot", SqlDbType.Int).Value = 0;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    if (dt.Rows.Count != 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            LoanRepaymentGrafik loanRepayment = SetLoanGrafikDetail(row);
                            list.Add(loanRepayment);
                        }

                        list = list.OrderBy(o => o.RepaymentDate).ToList();
                    }
                    else
                        list = null;

                }
                return list;
            }
        }

        /// <summary>
        /// Վարկի սկզբնական գրաֆիկ
        /// </summary>
        /// <returns></returns>
        public static List<LoanRepaymentGrafik> GetLoanInceptiveGrafik(Loan loan, ulong customerNumber)
        {
            List<LoanRepaymentGrafik> list = new List<LoanRepaymentGrafik>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT
                                                  date_of_repayment AS repayment_date,
                                                  capital_repayment,
                                                  rate_repayment AS interest_repayment,
                                                  interest_rate,
                                                  fee_repayment,
                                                  Subsidia_Rate_Repayment AS rest_of_capital,
                                                  capital_repayment + rate_repayment + Fee_repayment - Subsidia_rate_repayment AS total_repayment,
                                                  DATEDIFF(DAY, date_of_beginning, date_of_repayment) AS days,
                                                  rescheduled_amount  
                                                  FROM [Tbl_repayments_of_bl;]
                                            WHERE customer_number = @Cust_Num
                                            AND loan_full_number = @lfn
                                            AND date_of_beginning = @dtb
                                            ORDER BY date_of_repayment";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Cust_Num", SqlDbType.NVarChar, 12).Value = customerNumber;
                    cmd.Parameters.Add("@lfn", SqlDbType.NVarChar, 15).Value = loan.LoanAccount.AccountNumber;
                    cmd.Parameters.Add("@dtb", SqlDbType.NVarChar, 15).Value = loan.StartDate.ToString("dd/MMM/yy");

                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count != 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            LoanRepaymentGrafik loanRepayment = SetLoanGrafikDetail(row);
                            list.Add(loanRepayment);
                        }
                        list = list.OrderBy(o => o.RepaymentDate).ToList();
                    }
                    else
                        list = null;

                }
                return list;
            }
        }

        private static LoanRepaymentGrafik SetLoanGrafikDetail(DataRow row)
        {
            LoanRepaymentGrafik grafik = new LoanRepaymentGrafik();
            grafik.CapitalRepayment = double.Parse(row["capital_repayment"].ToString());
            if (row.Table.Columns.Contains("fee_repayment"))
            {
                grafik.FeeRepayment = double.Parse(row["fee_repayment"].ToString());
            }
            else
            {
                grafik.FeeRepayment = 0;
            }

            grafik.RateRepayment = double.Parse(row["interest_repayment"].ToString());
            grafik.InterestRate = double.Parse(row["interest_rate"].ToString());
            grafik.RepaymentDate = DateTime.Parse(row["repayment_date"].ToString());
            grafik.RestCapital = double.Parse(row["rest_of_capital"].ToString());
            grafik.TotalRepayment = double.Parse(row["total_repayment"].ToString());

            if (row.Table.Columns.Contains("Subsidia_rate_repayment"))
            {
                grafik.SubsidiaRateRepayment = double.Parse(row["Subsidia_rate_repayment"].ToString());
            }

            if (row.Table.Columns.Contains("non_subsidia_rate_repayment"))
            {
                grafik.NonSubsidiaRateRepayment = double.Parse(row["non_subsidia_rate_repayment"].ToString());
            }


            grafik.RescheduledAmount = double.Parse(row["rescheduled_amount"].ToString());


            return grafik;
        }


        internal static List<LoanRepaymentGrafik> GetLoanRepayments(Loan loan, double customerNumber)
        {
            DataTable dt = new DataTable();
            List<LoanRepaymentGrafik> list = new List<LoanRepaymentGrafik>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                      capital_repayment,
                                      fee_repayment,
                                      rate_repayment,
                                      interest_rate,
                                      date_of_repayment
                                      FROM [Tbl_repayments_of_bl;]
                                WHERE customer_number = @customerNumber
                                AND loan_full_number = @loanFullNumber
                                ORDER BY date_of_repayment";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@loanFullNumber", SqlDbType.Float).Value = loan.LoanAccount.AccountNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count != 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            LoanRepaymentGrafik loanRepayment = SetLoanRepaymentDetail(row);
                            list.Add(loanRepayment);
                        }
                    }
                    else
                        list = null;
                }
            }

            return list;
        }

        private static LoanRepaymentGrafik SetLoanRepaymentDetail(DataRow row)
        {
            LoanRepaymentGrafik graphic = new LoanRepaymentGrafik();
            graphic.CapitalRepayment = double.Parse(row["capital_repayment"].ToString());
            if (row.Table.Columns.Contains("fee_repayment"))
            {
                graphic.FeeRepayment = double.Parse(row["fee_repayment"].ToString());
            }
            else
                graphic.FeeRepayment = 0;
            graphic.RateRepayment = double.Parse(row["rate_repayment"].ToString());
            graphic.InterestRate = double.Parse(row["interest_rate"].ToString());
            graphic.RepaymentDate = DateTime.Parse(row["date_of_repayment"].ToString());
            return graphic;
        }


        /// <summary>
        /// Վերադարձնում է վարկի գլխավոր պայմանգրի տվյալները
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static LoanMainContract GetLoanMainContract(ulong productId, ulong customerNumber)
        {
            LoanMainContract contract = new LoanMainContract();

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                      cn.general_number,
                                      cn.currency,
                                      cn.start_capital,
                                      cn.date_of_beginning_contract,
                                      cn.date_of_normal_end_contract
                                      FROM [Tbl_short_time_loans;] sh
                                      INNER JOIN Tbl_main_loans_contracts cn
                                            ON sh.ID_Contract = cn.ID_Contract
                                WHERE sh.App_Id = @appId
                                AND sh.customer_number = @customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        contract.Amount = Convert.ToDouble(dt.Rows[0]["start_capital"]);
                        contract.Currency = dt.Rows[0]["currency"].ToString();
                        if (dt.Rows[0]["date_of_beginning_contract"] != DBNull.Value)
                        {
                            contract.StartDate = Convert.ToDateTime(dt.Rows[0]["date_of_beginning_contract"]);
                        }
                        if (dt.Rows[0]["date_of_normal_end_contract"] != DBNull.Value)
                        {
                            contract.EndDate = Convert.ToDateTime(dt.Rows[0]["date_of_normal_end_contract"]);
                        }
                        contract.GeneralNumber = dt.Rows[0]["general_number"].ToString();

                    }


                }
            }


            return contract;

        }


        /// <summary>
        /// Վերադարձնում է վարկի միջնորդավճար և այլ մուծումների ցանկը
        /// </summary>
        /// <param name="prodictId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<ProductOtherFee> GetProductOtherFees(ulong prodictId, ulong customerNumber)
        {

            List<ProductOtherFee> otherFees = new List<ProductOtherFee>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                      repayment_date,
                                      repayment_type,
                                      repayment_description,
                                      repayment_amount,
                                      repayment_currency
                                      FROM Tbl_repayments_add R
                                      INNER JOIN (SELECT
                                            code,
                                            repayment_description
                                      FROM Tbl_type_of_repayments_add
                                      GROUP BY code,
                                               repayment_description) T
                                            ON R.repayment_type = T.code
                                WHERE app_id = @productId
                                AND customer_number = @customerNumber
                                ORDER BY repayment_date;";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.NVarChar, 16).Value = prodictId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        otherFees = new List<ProductOtherFee>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        ProductOtherFee otherFee = SetProductOtherFee(row);
                        otherFees.Add(otherFee);
                    }
                }
            }

            return otherFees;
        }

        private static ProductOtherFee SetProductOtherFee(DataRow row)
        {
            ProductOtherFee otherFee = new ProductOtherFee();
            if (row != null)
            {
                otherFee.PaymentDate = (DateTime)row["repayment_date"];
                otherFee.Type = int.Parse(row["repayment_type"].ToString());
                otherFee.TypeDescription = Utility.ConvertAnsiToUnicode(row["repayment_description"].ToString());
                otherFee.Amount = float.Parse(row["repayment_amount"].ToString());
                otherFee.Currency = row["repayment_currency"].ToString();
            }
            return otherFee;
        }

        internal static ulong GetPaidFactoringMainProductId(long productId)
        {
            ulong mainProductId = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"select main_app_id from Tbl_paid_factoring where app_id=@productId ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            mainProductId = Convert.ToUInt64(dr["main_app_id"].ToString());
                        }
                    }
                }
            }
            return mainProductId;
        }

        /// <summary>
        /// Ապառիկի տվյալներ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static List<GoodsDetails> GetGoodsDetails(ulong productId, ulong customerNumber)
        {
            List<GoodsDetails> goodsDetails = new List<GoodsDetails>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @" SELECT
                                          GoodName,
                                          GoodCount,
                                          GoodCost,
                                          GoodPrepaid,
                                          CashPrice,
                                          GoodAddress,
                                          CASE
                                                WHEN loan_type = 38 THEN (SELECT
                                                            Tbl_aparik_shops.Shop_Name
                                                      FROM Tbl_aparik_shops
                                                      WHERE Shop_ID = sh.Shop_ID)
                                                ELSE (SELECT
                                                            Shop_Name
                                                      FROM Tbl_Contract_Aparik
                                                      WHERE new_id = sh.Keeper_Change
                                                      AND Deleted = 0)
                                          END Shop_name,
                                          apr.GoodCostSum

                                          FROM V_ShortLoans AS sh
                                          INNER JOIN Tbl_Aparik_Info ap
                                                ON ap.loan_full_number = sh.loan_full_number
                                                AND ap.date_of_beginning = sh.date_of_beginning
                                          OUTER APPLY (SELECT
                                                SUM(GoodCount * GoodCost) AS GoodCostSum
                                          FROM Tbl_Aparik_Info
                                          WHERE loan_full_number = sh.loan_full_number
                                          AND date_of_beginning = sh.date_of_beginning
                                          GROUP BY loan_full_number,
                                                   date_of_beginning) apr
                                    WHERE customer_number = @customerNumber
                                    AND sh.App_Id = @appId";
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
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            GoodsDetails goodDetails = SetGoodsDetails(row);
                            goodsDetails.Add(goodDetails);
                        }

                    }
                }
            }

            return goodsDetails;
        }




        private static GoodsDetails SetGoodsDetails(DataRow row)
        {
            GoodsDetails goodsDetails = new GoodsDetails();
            goodsDetails.TotalGoodAmount = double.Parse(row["GoodCostSum"].ToString());
            goodsDetails.GoodCount = int.Parse(row["goodcount"].ToString());
            goodsDetails.GoodAmount = Convert.ToDouble(row["GoodCost"]);
            goodsDetails.GoodName = Utility.ConvertAnsiToUnicode(row["GoodName"].ToString());
            goodsDetails.GoodPrepaid = Convert.ToDouble(row["GoodPrepaid"]);
            goodsDetails.CashPrice = double.Parse(row["CashPrice"].ToString());
            goodsDetails.GoodAddress = Utility.ConvertAnsiToUnicode(row["GoodAddress"].ToString());
            goodsDetails.ShopName = Utility.ConvertAnsiToUnicode(row["Shop_Name"].ToString());

            return goodsDetails;

        }

        internal static Loan GetLoan(string loanFullNumber)
        {
            Loan loan = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                      S.App_Id,
                                      S.loan_full_number,
                                      S.start_capital,
                                      S.currency,
                                      S.date_of_beginning,
                                      S.contract_date,
                                      S.date_of_normal_end,
                                      S.interest_rate,
                                      ABS(S.current_capital) AS current_capital,
                                      S.sum_of_given_money,
                                      ABS(S.current_rate_value) AS current_rate_value,
                                      ABS(current_fee) AS current_fee,
                                      S.quality,
                                      CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN N'Տրամադրված է 24/7 եղանակով' else LQ.quality end AS quality_description_arm,
                                      S.security_code_2,
                                      ABS(out_capital) AS out_capital,
                                      ABS(overdue_capital) AS overdue_capital,
                                      S.loan_type,
                                      T.description AS loan_type_desription_arm,
                                      ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                      ABS(S.penalty_rate) AS penalty_rate,
                                      ABS(S.penalty_add) AS penalty_add,
                                      ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                      ABS(total_judgment_penalty_rate) AS total_judgment_penalty_rate,
                                      ABS(S.total_fee) AS total_fee,
                                      ABS(S.total_rate_value) AS total_rate_value,
                                      S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                      overdue_loan_date,
                                      last_day_of_rate_repair,
                                      connect_account_full_number,
                                      date_of_judgment_begin,
                                      date_of_judgment_end,
                                      judgment_penalty_percent,
                                      interest_rate_effective,
                                      interest_rate_full,
                                      subsidia_interest_rate,
                                      changed_rate,
                                      advanced_repayment_rate,
                                      first_start_capital,
                                      date_of_beginning_second_period,
                                      S.filialcode,
                                      S.kind_of_money,
                                      S.loan_program,
                                      S.action,
                                      S.penalty_add_percent,
                                      ad.interest_rate_effective_with_only_bank_profit,
                                      ad.interest_rate_effective_without_account_service_fee,
                                      s.matured_current_rate_value,
                                      s.matured_penalty,
                                      s.matured_judgment_penalty_rate,
                                      s.matured_current_fee,
                                      s.Overdue_loan_date_for_classification,
                                      s.date_of_stopping_calculation,
                                      s.date_of_stopping_penalty_calculation,
                                      s.out_loan_date,
									  s.out_from_outbal_date,
                                      t.Description_Engl,
                                      CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN 1 else 0 end as is_24_7
                                      FROM V_ShortLoans S
                                      INNER JOIN [tbl_type_of_loans;] T
                                            ON s.loan_type = T.code
                                      INNER JOIN [Tbl_loan_list_quality;] LQ
                                            ON S.Quality = LQ.number
                                      LEFT JOIN Tbl_liability_add ad
                                            ON ad.app_id = s.App_Id
                                             LEFT JOIN (SELECT doc_id,app_id  FROM (SELECT doc_id FROM  dbo.Tbl_HB_documents  WHERE  
                                         quality = 20 ) HB
                                        INNER JOIN dbo.Tbl_HB_Products_Identity IDENT ON
                                        HB.doc_id = IDENT.HB_doc_id) CONTRACTS24_7 
										ON s.app_id = CONTRACTS24_7.app_id
                                WHERE S.loan_full_number = @loanFullNumber
                                AND S.quality NOT IN (40)
                                ORDER BY Date_of_normal_end";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@loanFullNumber", SqlDbType.Float).Value = loanFullNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        loan = SetLoan(row);
                    }
                    else
                    {
                        loan = null;
                    }

                }
            }

            return loan;
        }


        internal static Loan GetLoan(ulong productId)
        {
            Loan loan = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.loan_full_number,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.contract_date,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          S.sum_of_given_money,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          ABS(current_fee) AS current_fee,
                                          S.quality,
                                          S.date_of_stopping_penalty_calculation,
                                          CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN N'Տրամադրված է 24/7 եղանակով' else LQ.quality end AS quality_description_arm,
                                          S.security_code_2,
                                          ABS(out_capital) AS out_capital,
                                          ABS(overdue_capital) AS overdue_capital,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(total_judgment_penalty_rate) AS total_judgment_penalty_rate,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          overdue_loan_date,
                                          last_day_of_rate_repair,
                                          connect_account_full_number,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          subsidia_interest_rate,
                                          changed_rate,
                                          advanced_repayment_rate,
                                          first_start_capital,
                                          date_of_beginning_second_period,
                                          S.filialcode,
                                          S.kind_of_money,
                                          S.loan_program,
                                          S.action,
                                          S.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          s.matured_current_rate_value,
                                          s.matured_penalty,
                                          s.matured_judgment_penalty_rate,
                                          s.matured_current_fee,
                                          s.Overdue_loan_date_for_classification,
                                          s.out_loan_date,
									      s.out_from_outbal_date,
                                          CASE WHEN ISNULL(CONTRACTS24_7.doc_Id, 0) <> 0 THEN 1 else 0 end as is_24_7
                                          FROM V_ShortLoans S
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON s.loan_type = T.code
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON S.Quality = LQ.number
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = s.App_Id
                                         LEFT JOIN (SELECT doc_id,app_id  FROM (SELECT doc_id FROM  dbo.Tbl_HB_documents  WHERE  
                                         quality = 20 ) HB
                                        INNER JOIN dbo.Tbl_HB_Products_Identity IDENT ON
                                        HB.doc_id = IDENT.HB_doc_id) CONTRACTS24_7 
										ON s.app_id = CONTRACTS24_7.app_id
                                    WHERE S.App_Id = @productId
                                    AND S.quality NOT IN (40)
                                    ORDER BY Date_of_normal_end";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        loan = SetLoan(row);
                    }
                    else
                    {
                        loan = null;
                    }

                }
            }

            return loan;
        }
        internal static bool CheckAdvancedRepaymentRate(long productId)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT dbo.fnc_check_advanced_repayment_rate(@appId) result";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            result = (bool)dr["result"];
                        }

                    }

                }
            }

            return result;
        }

        internal static ulong GetSurchargeAppId(ulong productId)
        {
            ulong appID = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT
                                          app_id
                                          FROM tbl_credit_lines
                                    WHERE app_id <> @appId
                                    AND visa_number IN (SELECT
                                          visa_number
                                    FROM tbl_credit_lines
                                    WHERE app_id = @appId
                                    AND loan_type NOT IN (18, 25,60))";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            appID = ulong.Parse(dr["app_id"].ToString());
                        }
                    }
                }
            }

            return appID;
        }

        internal static LoanStatement GetStatement(string account, DateTime dateFrom, DateTime dateTo, double minAmount = -1, double maxAmount = -1, string debCred = null, int transactionsCount = 0, short orderByAscDesc = 0, byte langId = 1)
        {
            LoanStatement loanStatement = new LoanStatement();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_loan_statment", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = dateTo;
                    cmd.Parameters.Add("@account", SqlDbType.NVarChar, 32).Value = account;
                    cmd.Parameters.Add("@filial_code", SqlDbType.Int, 2).Value = 22000;
                    cmd.Parameters.Add("@min_amount", SqlDbType.Float).Value = minAmount;
                    cmd.Parameters.Add("@max_amount", SqlDbType.Float).Value = maxAmount;
                    cmd.Parameters.Add("@debCred", SqlDbType.NChar, 1).Value = debCred == null ? debCred : debCred == "d" ? "c" : "d";
                    cmd.Parameters.Add("@transactionsCount", SqlDbType.Int).Value = transactionsCount;
                    cmd.Parameters.Add("@orderByAmountAscDesc", SqlDbType.TinyInt).Value = orderByAscDesc;
                    cmd.Parameters.Add("@lang_id", SqlDbType.Int).Value = langId;

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        if (dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                LoanStatementDetail statementDetails = new LoanStatementDetail();
                                if (dr["date_of_accounting"] != DBNull.Value)
                                {
                                    statementDetails.OperationDate = DateTime.Parse(dr["date_of_accounting"].ToString());
                                }
                                else
                                {
                                    statementDetails.OperationDate = new DateTime(2001, 1, 1);
                                }

                                statementDetails.OperationCurrency = dr["currency"].ToString();
                                statementDetails.OperationAmount = statementDetails.OperationCurrency == "AMD" ? (dr["amount"] == DBNull.Value ? (double)0 : double.Parse(dr["amount"].ToString())) : (dr["amount_currency"] == DBNull.Value ? (double)0 : double.Parse(dr["amount_currency"].ToString()));
                                statementDetails.Description = dr["wording"].ToString();
                                statementDetails.DebetCredit = dr["debit_credit"].ToString();
                                statementDetails.TransactionsGroupNumber = dr["transactions_group_number"] != DBNull.Value ? Convert.ToUInt64(dr["transactions_group_number"]) : 0;

                                loanStatement.Transactions.Add(statementDetails);
                            }
                        }

                        if (dr.NextResult())
                        {
                            if (dr.Read())
                            {
                                loanStatement.TotalDebitAmountAMD = double.Parse(dr["debet_all_amd"].ToString());
                                loanStatement.TotalCreditAmountAMD = double.Parse(dr["credit_all_amd"].ToString());

                                loanStatement.TotalDebitAmountInCurrency = double.Parse(dr["debet_all_currency"].ToString());
                                loanStatement.TotalCreditAmountInCurrency = double.Parse(dr["credit_all_currency"].ToString());
                            }
                        }
                    }
                }
            }

            return loanStatement;
        }

        internal static async Task<List<Loan>> GetLoansAsync(ulong customerNumber)
        {
            List<Loan> loanList = new List<Loan>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT
                                          S.App_Id,
                                          S.loan_full_number,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          ABS(current_fee) AS current_fee,
                                          S.quality,
                                          LQ.quality AS quality_description_arm,
                                          S.security_code_2,
                                          ABS(out_capital) AS out_capital,
                                          ABS(overdue_capital) AS overdue_capital,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(judgment_penalty_rate) AS judgment_penalty_rate,
                                          ABS(total_judgment_penalty_rate) AS total_judgment_penalty_rate,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          overdue_loan_date,
                                          last_day_of_rate_repair,
                                          connect_account_full_number,
                                          date_of_judgment_begin,
                                          date_of_judgment_end,
                                          judgment_penalty_percent,
                                          interest_rate_effective,
                                          interest_rate_full,
                                          subsidia_interest_rate,
                                          changed_rate,
                                          advanced_repayment_rate,
                                          first_start_capital,
                                          date_of_beginning_second_period,
                                          S.filialcode,
                                          S.kind_of_money,
                                          S.loan_program,
                                          S.action,
                                          S.penalty_add_percent,
                                          ad.interest_rate_effective_with_only_bank_profit,
                                          ad.interest_rate_effective_without_account_service_fee,
                                          S.matured_current_rate_value,
                                          S.matured_penalty,
                                          S.matured_judgment_penalty_rate,
                                          S.matured_current_fee,
                                          S.date_of_stopping_calculation,
                                          S.date_of_stopping_penalty_calculation,
                                          S.Overdue_loan_date_for_classification,
                                          S.out_loan_date,
									      S.out_from_outbal_date
                                          FROM [tbl_short_time_loans;] S
                                          INNER JOIN [tbl_type_of_loans;] T
                                                ON s.loan_type = T.code
                                          INNER JOIN [Tbl_loan_list_quality;] LQ
                                                ON S.Quality = LQ.number
                                          LEFT JOIN Tbl_liability_add ad
                                                ON ad.app_id = s.App_Id
                                    WHERE s.Customer_Number = @customerNumber
                                    ORDER BY Date_of_normal_end";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@operDay", SqlDbType.SmallDateTime).Value = Utility.GetCurrentOperDay();

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    var result = Parallel.For(0, dt.Rows.Count, (i) =>
                    {
                        DataRow row = dt.Rows[i];
                        Loan loan = SetLoan(row);
                        loanList.Add(loan);
                    });

                }
            }

            return loanList;
        }

        //private static async Task<Loan> SetLoanAsync(DataRow row)
        //{
        //    Loan loan = new Loan();

        //    if (row != null)
        //    {
        //        lock{

        //        }
        //        loan.ProductId = long.Parse(row["app_id"].ToString());
        //        loan.LoanAccount = Account.GetAccount(ulong.Parse(row["loan_full_number"].ToString()));
        //        loan.LoanType = short.Parse(row["loan_type"].ToString());
        //        loan.Quality = short.Parse(row["quality"].ToString());
        //        loan.Currency = row["currency"].ToString();
        //        loan.InterestRate = float.Parse(row["interest_rate"].ToString());
        //        loan.LoanTypeDescription = Utility.ConvertAnsiToUnicode(row["loan_type_desription_arm"].ToString());
        //        loan.QualityDescription = Utility.ConvertAnsiToUnicode(row["quality_description_arm"].ToString());
        //        if (row.Table.Columns["out_capital"] != null)
        //            loan.OutCapital = double.Parse(row["out_capital"].ToString());
        //        if (row.Table.Columns["judgment_penalty_rate"] != null)
        //            loan.JudgmentRate = double.Parse(row["judgment_penalty_rate"].ToString());
        //        if (row.Table.Columns["total_judgment_penalty_rate"] != null)
        //            loan.TotalJudgmentPenaltyRate = double.Parse(row["total_judgment_penalty_rate"].ToString());
        //        if (row.Table.Columns["overdue_capital"] != null)
        //            loan.OverdueCapital = double.Parse(row["overdue_capital"].ToString());
        //        if (row.Table.Columns["Subsidia_Current_Rate_Value"] != null)
        //            loan.SubsidiaCurrentRateValue = double.Parse(row["Subsidia_Current_Rate_Value"].ToString());
        //        if (row.Table.Columns["current_fee"] != null)
        //            loan.CurrentFee = double.Parse(row["current_fee"].ToString());
        //        loan.StartCapital = double.Parse(row["start_capital"].ToString());
        //        loan.CurrentCapital = double.Parse(row["current_capital"].ToString());
        //        loan.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
        //        loan.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
        //        loan.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
        //        loan.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
        //        loan.TotalFee = double.Parse(row["total_fee"].ToString());
        //        loan.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
        //        if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
        //        {
        //            loan.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
        //        }
        //        loan.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
        //        loan.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
        //        loan.ContractNumber = ulong.Parse(row["security_code_2"].ToString());
        //        loan.LastDateOfRateRepair = row["last_day_of_rate_repair"] != DBNull.Value ? DateTime.Parse(row["last_day_of_rate_repair"].ToString()) : default(DateTime?);
        //        loan.OverdueLoanDate = row["overdue_loan_date"] != DBNull.Value ? DateTime.Parse(row["overdue_loan_date"].ToString()) : default(DateTime?);
        //        loan.JudgmentStartDate = row["date_of_judgment_begin"] != DBNull.Value ? DateTime.Parse(row["date_of_judgment_begin"].ToString()) : default(DateTime?);
        //        loan.JudgmentEndDate = row["date_of_judgment_end"] != DBNull.Value ? DateTime.Parse(row["date_of_judgment_end"].ToString()) : default(DateTime?);
        //        loan.JudgmentPenaltyPercent = float.Parse(row["judgment_penalty_percent"].ToString());
        //        loan.InterestRateEffective = float.Parse(row["interest_rate_effective"].ToString());
        //        loan.InterestRateFull = row["interest_rate_full"] != DBNull.Value ? float.Parse(row["interest_rate_full"].ToString()) : 0;
        //        loan.ConnectAccount = await Account.GetAccountAsync(row["connect_account_full_number"].ToString());
        //        loan.DailyPenaltyInterestRate = float.Parse(row["penalty_add_percent"].ToString());

        //        if (row["subsidia_interest_rate"] != DBNull.Value)
        //        {
        //            loan.SubsidiaInterestRate = double.Parse(row["subsidia_interest_rate"].ToString());
        //        }
        //        if (row["changed_rate"] != DBNull.Value)
        //        {
        //            loan.ChangeRate = short.Parse(row["changed_rate"].ToString());
        //        }
        //        if (row["advanced_repayment_rate"] != DBNull.Value)
        //        {
        //            loan.AdvancedRepaymentRate = double.Parse(row["advanced_repayment_rate"].ToString());
        //        }
        //        if (row["first_start_capital"] != DBNull.Value)
        //        {
        //            loan.FirstStartCapital = double.Parse(row["first_start_capital"].ToString());
        //        }
        //        if (row["date_of_beginning_second_period"] != DBNull.Value)
        //        {
        //            loan.DateOfBeginningSecondPeriod = DateTime.Parse(row["date_of_beginning_second_period"].ToString());
        //        }

        //        loan.Fond = short.Parse(row["kind_of_money"].ToString());
        //        if (row["action"] != DBNull.Value)
        //        {
        //            loan.Sale = short.Parse(row["action"].ToString());
        //        }

        //        loan.LoanProgram = short.Parse(row["loan_program"].ToString());

        //        loan.FillialCode = int.Parse(row["filialcode"].ToString());
        //        loan.ProductType = 1;
        //        loan.HasClaim = LoanProduct.CheckLoanProductClaimAvailability(loan.ProductId);

        //        if (row["interest_rate_effective_with_only_bank_profit"] != DBNull.Value)
        //        {
        //            loan.InterestRateEffectiveWithOnlyBankProfit = double.Parse(row["interest_rate_effective_with_only_bank_profit"].ToString());
        //        }
        //        if (row["interest_rate_effective_without_account_service_fee"] != DBNull.Value)
        //        {
        //            loan.InterestRateEffectiveWithoutAccountServiceFee = double.Parse(row["interest_rate_effective_without_account_service_fee"].ToString());
        //        }

        //        if (row["matured_current_fee"] != DBNull.Value)
        //        {
        //            loan.MaturedCurrentFee = double.Parse(row["matured_current_fee"].ToString());
        //        }

        //        if (row["matured_current_rate_value"] != DBNull.Value)
        //        {
        //            loan.MaturedCurrentRateValue = double.Parse(row["matured_current_rate_value"].ToString());
        //        }

        //        if (row["matured_judgment_penalty_rate"] != DBNull.Value)
        //        {
        //            loan.MaturedJudgmentPenaltyRate = double.Parse(row["matured_judgment_penalty_rate"].ToString());
        //        }

        //        if (row["matured_penalty"] != DBNull.Value)
        //        {
        //            loan.MaturedPenaltyRate = double.Parse(row["matured_penalty"].ToString());
        //        }

        //        loan.CreditCode = LoanProduct.GetCreditCode(loan.ProductId, loan.ProductType);
        //        loan.CheckAdvancedRepaymentRate = CheckAdvancedRepaymentRate(loan.ProductId);

        //        if (row["date_of_stopping_calculation"] != DBNull.Value)
        //        {
        //            loan.DateOfStoppingCalculation = Convert.ToDateTime(row["date_of_stopping_calculation"].ToString());
        //        }

        //        if (row["date_of_stopping_penalty_calculation"] != DBNull.Value)
        //        {
        //            loan.DateOfStoppingPenaltyCalculation = Convert.ToDateTime(row["date_of_stopping_penalty_calculation"].ToString());
        //        }
        //        if (row["Overdue_loan_date_for_classification"] != DBNull.Value)
        //        {
        //            loan.OverdueLoanDateForClassification = Convert.ToDateTime(row["Overdue_loan_date_for_classification"].ToString());
        //        }
        //        if (row["out_loan_date"] != DBNull.Value)
        //        {
        //            loan.OutLoanDate = Convert.ToDateTime(row["out_loan_date"].ToString());
        //        }
        //        if (row["out_from_outbal_date"] != DBNull.Value)
        //        {
        //            loan.OutFromOutbalDate = Convert.ToDateTime(row["out_from_outbal_date"].ToString());
        //        }

        //    }
        //    return loan;
        //}

        /// <summary>
        /// Վարկի պայմանագրի առկայության ստուգում
        /// </summary>
        /// <param name="loanAccountNumber"></param>
        /// <returns></returns>
        internal static bool HasUploadedLoanContract(string loanAccountNumber)
        {
            int docId = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT hb_doc_id 
                               FROM [Tbl_short_time_loans;]
                               WHERE loan_full_number = @loanAccountNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@loanAccountNumber", SqlDbType.Float).Value = loanAccountNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (int.Parse(dr["hb_doc_id"].ToString()) == 0)
                                return false;
                            docId = int.Parse(dr["hb_doc_id"].ToString());
                        }
                    }
                }
            }
            using (SqlConnection conn2 = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sql2 = @"SELECT doc_id 
                                            FROM tbl_HB_Attached_Documents
                                            WHERE doc_id = " + docId;
                conn2.Open();
                using (SqlCommand cmd2 = new SqlCommand(sql2, conn2))
                {
                    using (SqlDataReader dr2 = cmd2.ExecuteReader())
                    {
                        if (dr2.HasRows)
                            return true;
                        else
                            return false;
                    }
                }
            }
        }
        internal static string GetLoanTypeDescriptionEng(string loanFullNumber)
        {
            string loanTypeDescriptionEng = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"SELECT t.Description_Engl AS loan_type_desription_eng
                                                        FROM V_ShortLoans_small S
                                                        INNER JOIN [tbl_type_of_loans;] T ON s.loan_type = T.code
                                                        WHERE S.loan_full_number = @loanFullNumber AND S.quality NOT IN (40)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@loanFullNumber", SqlDbType.Float).Value = loanFullNumber;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                if (dr["loan_type_desription_eng"] != DBNull.Value)
                                {
                                    loanTypeDescriptionEng = dr["loan_type_desription_eng"].ToString();
                                }
                            }
                        }
                    }
                }

                return loanTypeDescriptionEng;
            }
        }


        internal static bool CheckCutomerHasPaidInsurance(ulong customerNumber, ulong productId)
        {
            bool hasPaidInsurance = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"SELECT
					                                                            app_id 
                                                            FROM Tbl_paid_factoring
					                                                            WHERE customer_number = @customerNumber
					                                                            AND quality <> 10
					                                                            AND loan_type = 49", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                hasPaidInsurance = true;
                                if (Convert.ToUInt64(dr["app_id"]) == productId)
                                {
                                    hasPaidInsurance = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                return hasPaidInsurance;
            }
        }


        internal static short? GetLoanType(ulong productId)
        {
            short? loanType = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"SELECT loan_type
                                                        FROM V_ShortLoans_small 
                                                        WHERE app_id=@productId", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                if (dr["loan_type"] != DBNull.Value)
                                {
                                    loanType = Convert.ToInt16(dr["loan_type"]);
                                }
                            }
                        }
                    }
                }

                return loanType;
            }
        }

        internal static bool CheckLoanExists(ulong customerNumber, ulong productId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 1
                                                        FROM V_ShortLoans_small 
                                                        WHERE app_id=@productId and customer_number=@customerNumber", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                        cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            return dr.Read();
                        }
                    }
                }

            }
        }


        internal static string PostResetEarlyRepaymentFee(ulong productId, string description, bool recovery, short setNumber)
        {
            string result = string.Empty;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Reset_Early_Repayment_Fee";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@App_ID", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@description", SqlDbType.NVarChar).Value = description;
                    cmd.Parameters.Add("@set_number", SqlDbType.Int).Value = setNumber;
                    cmd.Parameters.Add("@recovery", SqlDbType.Bit).Value = recovery;

                    cmd.ExecuteNonQuery();

                }

            }

            return result;
        }

        internal static bool GetResetEarlyRepaymentFeePermission(ulong productId)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT 1 FROM tbl_advance_repayment_rate_for_recovery WHERE App_Id = @App_ID";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@App_ID", SqlDbType.BigInt).Value = productId;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        result = true;
                    }

                }

            }

            return result;
        }

        internal static bool IsLoan_24_7(ulong productId)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT 1 FROM  [tbl_short_time_loans;] S INNER JOIN dbo.Tbl_HB_Products_Identity IDENT " +
                                       "ON S.APP_ID = IDENT.App_ID INNER JOIN Tbl_HB_documents HB ON IDENT.HB_Doc_ID =HB.DOC_Id AND HB.quality = 20" +
                                       "  WHERE S.App_Id = @App_ID";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@App_ID", SqlDbType.BigInt).Value = productId;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
        internal static string GetLiabilitiesAccountNumberByAppId(ulong appId)
        {
            string Account_number = string.Empty;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandText = "SELECT p.Account_number from [dbo].[Tbl_Products_Accounts] p JOIN [dbo].[Tbl_Products_Accounts_Groups] b ON p.group_id = b.group_id " +
                        "WHERE App_Id = @app_id AND Type_of_account = 224";
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = appId;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        Account_number = dr["Account_number"].ToString();
                    }
                }
            }
            return Account_number;
        }

        internal static double GetMaxAvailableAmountForNewLoan(string provisionCurrency, ulong customerNumber)
        {
            double amount = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                string sqltext = "SELECT dbo.fn_GetMaxAvailableAmountForNewLoan(@provisionCurrency, @customerNumber)";

                using (SqlCommand cmd = new SqlCommand(sqltext, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@provisionCurrency", SqlDbType.NVarChar).Value = provisionCurrency;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            amount = double.Parse(dr[0].ToString());
                    }
                }
            }
            return amount;
        }

        internal static LoanRepaymentDelayDetails GetLoanRepaymentDelayDetails(ulong productId)
        {
            var delayDetails = new LoanRepaymentDelayDetails();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select * from tbl_loan_repayments_delays WHERE App_Id = @App_ID";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@App_ID", SqlDbType.BigInt).Value = productId;
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        delayDetails.Id = ulong.Parse(dr["Id"].ToString());
                        delayDetails.ProductAppId = long.Parse(dr["app_id"].ToString());
                        delayDetails.DelayReason = dr["delay_reason"].ToString();
                        delayDetails.DelayDate = DateTime.Parse(dr["delay_date"].ToString());
                        delayDetails.RegistrationDate = DateTime.Parse(dr["registration_date"].ToString());
                        delayDetails.SetNumber = int.Parse(dr["registration_set_number"].ToString());
                    }
                }

            }

            return delayDetails;
        }

        internal static string GetLoanAccountNumber(ulong productId, ulong customerNumber)
        {
            string loan = null;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT loan_full_number
                                FROM [Tbl_short_time_loans;]
                                WHERE app_id = @appId
                                AND customer_number = @customerNumber
                                ORDER BY Date_of_normal_end";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            loan = dr["loan_full_number"].ToString();
                        }
                    }
                }
            }
            //Ապառիկ վարկեր
            if (String.IsNullOrEmpty(loan))
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
                {
                    string sql = @" SELECT  dbo.fnc_get_product_account_from_group(app_id, CASE
                                                WHEN Loan_type = 38 THEN 58
                                                ELSE CASE
                                                            WHEN Loan_type = 33 OR
                                                                  Loan_type = 55 THEN 54
                                                            ELSE 111
                                                      END
                                          END, 1) AS loan_full_number
FROM [Tbl_Paid_factoring]  WHERE (Loan_type = 38
                                    OR Loan_type = 49
                                    OR Loan_type = 33
                                    OR Loan_type = 55)
                                    AND quality <> 40
                                    AND Customer_Number = @customerNumber
                                    AND app_id = @appId
                                ORDER BY Date_of_normal_end";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@appId", SqlDbType.BigInt).Value = productId;
                        cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                        conn.Open();
                        DataTable dt = new DataTable();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                loan = dr["loan_full_number"].ToString();
                            }
                        }
                    }
                }
            }
            return loan;
        }

        internal static double GetLoanOrderAcknowledgement(long docId)
        {
            double result = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string query = @"SELECT interest_rate_effective 
                                    FROM Tbl_HB_loan_precontract_data 
                                 WHERE doc_id=@docId ORDER BY registration_date DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            result = Convert.ToDouble(dr["interest_rate_effective"]);
                    }
                }
            }

            return result;
        }

        internal static LoanRepaymentFromCardDataChange GetLoanRepaymentFromCardDataChangeHistory(ulong appId)
        {
            LoanRepaymentFromCardDataChange loanRepaymentFromCardDataChange = new LoanRepaymentFromCardDataChange();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select TOP 1 AppId , StartDate , EndDate , dbo.fnc_convertAnsiToUnicode(ChangeDescription) ChangeDescription, SetNumber " +
                                      " from Tbl_Loan_Repayment_From_Card_Data_Change WHERE AppId = @App_ID ORDER BY Id DESC";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@App_ID", SqlDbType.BigInt).Value = appId;
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        loanRepaymentFromCardDataChange.AppId = ulong.Parse(dr["appid"].ToString());
                        loanRepaymentFromCardDataChange.EndDate = !string.IsNullOrEmpty(dr["EndDate"].ToString()) ? Convert.ToDateTime(dr["EndDate"].ToString()) : (DateTime?)null;
                        loanRepaymentFromCardDataChange.StartDate = Convert.ToDateTime(dr["StartDate"].ToString());
                        loanRepaymentFromCardDataChange.SetNumber = Convert.ToInt32(dr["SetNumber"].ToString());
                        loanRepaymentFromCardDataChange.Description = dr["ChangeDescription"].ToString();
                    }
                }
            }
            return loanRepaymentFromCardDataChange;
        }

        internal static LoanRepaymentFromCardDataChange SaveLoanRepaymentFromCardDataChange(LoanRepaymentFromCardDataChange loanRepaymentFromCardDataChange)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Loan_Repayment_From_Card_Data_Change ";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@AppId", SqlDbType.BigInt).Value = loanRepaymentFromCardDataChange.AppId;
                    cmd.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = loanRepaymentFromCardDataChange.StartDate;
                    cmd.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = loanRepaymentFromCardDataChange.EndDate;
                    cmd.Parameters.Add("@ChangeDescription", SqlDbType.NVarChar).Value = loanRepaymentFromCardDataChange.Description;
                    cmd.Parameters.Add("@SetNumber", SqlDbType.Int).Value = loanRepaymentFromCardDataChange.SetNumber;
                    cmd.Parameters.Add("@ChangeAction", SqlDbType.SmallInt).Value = loanRepaymentFromCardDataChange.Action;

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        loanRepaymentFromCardDataChange.AppId = ulong.Parse(dr["appid"].ToString());
                        loanRepaymentFromCardDataChange.EndDate = !string.IsNullOrEmpty(dr["EndDate"].ToString()) ? Convert.ToDateTime(dr["EndDate"].ToString()) : (DateTime?)null;
                        loanRepaymentFromCardDataChange.StartDate = Convert.ToDateTime(dr["StartDate"].ToString());
                        loanRepaymentFromCardDataChange.SetNumber = Convert.ToInt32(dr["SetNumber"].ToString());
                        loanRepaymentFromCardDataChange.Description = dr["ChangeDescription"].ToString();
                    }
                }
            }
            return loanRepaymentFromCardDataChange;
        }

    }
}
