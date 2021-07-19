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
   internal class PaidGuaranteeDB
    {
       internal static List<PaidGuarantee> GetPaidGuarantees(ulong customerNumber)
       {
           List<PaidGuarantee> paidGuarantees = new List<PaidGuarantee>();
           using(SqlConnection conn=new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
           {
               string sql = @"SELECT
                                          S.App_Id,
                                          dbo.fnc_get_product_account_from_group(S.app_id, 51, 1) AS loan_full_number,
                                          a2.account_number AS [connect_account_full_number],
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
                                          S.filialcode,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_paid_factoring S
                                    INNER JOIN Tbl_Products_Accounts_Groups gr
                                          ON S.App_Id = gr.App_ID
                                    INNER JOIN Tbl_Products_Accounts a2
                                          ON gr.Group_ID = a2.Group_Id
                                          AND a2.Type_of_product = 10
                                          AND a2.Type_of_account = 24
                                    INNER JOIN [tbl_type_of_loans;] T
                                          ON s.loan_type = T.code
                                    INNER JOIN [Tbl_loan_list_quality;] LQ
                                          ON S.Quality = LQ.number
                                    WHERE customer_number = @customerNumber
                                    AND S.quality NOT IN (40)
                                    AND s.loan_type = 34
                                    ORDER BY Date_of_normal_end ";
               using(SqlCommand cmd=new SqlCommand(sql,conn))
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

                       PaidGuarantee paidGuarantee = SetPaidGuarantee(row);

                       paidGuarantees.Add(paidGuarantee);
                   }
               }

           }
           return paidGuarantees;
       }

       internal static List<PaidGuarantee> GetClosedPaidGuarantees(ulong customerNumber)
       {
           List<PaidGuarantee> paidGuarantees = new List<PaidGuarantee>();
           using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
           {
               string sql = @"SELECT
                                          S.App_Id,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          dbo.fnc_get_product_account_from_group(S.app_id, 51, 1) AS loan_full_number,
                                          a2.account_number AS [connect_account_full_number],
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
                                          S.filialcode,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_closed_paid_factoring S
                                    INNER JOIN [tbl_type_of_loans;] T
                                          ON s.loan_type = T.code
                                    INNER JOIN Tbl_Products_Accounts_Groups gr
                                          ON S.App_Id = gr.App_ID
                                    INNER JOIN Tbl_Products_Accounts a2
                                          ON gr.Group_ID = a2.Group_Id
                                          AND a2.Type_of_product = 10
                                          AND a2.Type_of_account = 24
                                    INNER JOIN [Tbl_loan_list_quality;] LQ
                                          ON S.Quality = LQ.number
                                    WHERE customer_number = @customerNumber
                                    AND S.quality = 40
                                    AND s.loan_type = 34
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

                       PaidGuarantee paidGuarantee = SetPaidGuarantee(row);

                       paidGuarantees.Add(paidGuarantee);
                   }
               }

           }
           return paidGuarantees;
       }

       internal static PaidGuarantee GetPaidGuarantee(ulong customerNumber,ulong productId)
       {
           PaidGuarantee paidGuarantee = new PaidGuarantee();
           using(SqlConnection conn=new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
           {
               string sql = @"SELECT
                                          S.App_Id,
                                          S.start_capital,
                                          S.currency,
                                          S.date_of_beginning,
                                          S.date_of_normal_end,
                                          S.interest_rate,
                                          dbo.fnc_get_product_account_from_group(S.app_id, 51, 1) AS loan_full_number,
                                          a2.account_number AS [connect_account_full_number],
                                          ABS(S.current_capital) AS current_capital,
                                          ABS(S.current_rate_value) AS current_rate_value,
                                          S.quality,
                                          LQ.quality AS quality_description_arm,
                                          S.security_code_2,
                                          S.loan_type,
                                          T.description AS loan_type_desription_arm,
                                          ABS(S.inpaied_rest_of_rate) AS inpaied_rest_of_rate,
                                          ABS(S.penalty_rate) AS penalty_rate,
                                          S.request_status,
                                          ABS(S.penalty_add) AS penalty_add,
                                          ABS(S.total_fee) AS total_fee,
                                          ABS(S.total_rate_value) AS total_rate_value,
                                          S.[last_day_of rate calculation] AS day_of_rate_calculation,
                                          S.filialcode,
                                          S.Overdue_loan_date_for_classification
                                    FROM Tbl_paid_factoring S
                                    INNER JOIN [tbl_type_of_loans;] T
                                          ON s.loan_type = T.code
                                    INNER JOIN Tbl_Products_Accounts_Groups gr
                                          ON S.App_Id = gr.App_ID
                                    INNER JOIN Tbl_Products_Accounts a2
                                          ON gr.Group_ID = a2.Group_Id
                                          AND a2.Type_of_product = 10
                                          AND a2.Type_of_account = 24
                                    INNER JOIN [Tbl_loan_list_quality;] LQ
                                          ON S.Quality = LQ.number
                                    WHERE customer_number = @customerNumber
                                    AND S.quality NOT IN (40)
                                    AND s.loan_type = 34
                                    AND S.app_id = @appId
                                    ORDER BY Date_of_normal_end";
               using(SqlCommand cmd=new SqlCommand(sql,conn))
               {
                   cmd.CommandType = CommandType.Text;
                   cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                   cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                   conn.Open();

                   DataTable dt = new DataTable();
                   using(SqlDataReader dr=cmd.ExecuteReader())
                   {
                       dt.Load(dr);
                   }
                   if (dt.Rows.Count>0)
                   {
                       DataRow row = dt.Rows[0];
                       paidGuarantee = SetPaidGuarantee(row);
                   }
               }
           }
           return paidGuarantee;
       }
       private static PaidGuarantee SetPaidGuarantee(DataRow row)
       {
           PaidGuarantee paidGuarantee = new PaidGuarantee();

           if (row != null)
           {
               paidGuarantee.ProductId = long.Parse(row["app_id"].ToString());
               paidGuarantee.LoanType = short.Parse(row["loan_type"].ToString());
               paidGuarantee.Quality = short.Parse(row["quality"].ToString());
               paidGuarantee.Currency = row["currency"].ToString();
               paidGuarantee.InterestRate = float.Parse(row["interest_rate"].ToString());
                paidGuarantee.FillialCode = int.Parse(row["filialcode"].ToString());
               if (row.Table.Columns["out_capital"] != null)
                   paidGuarantee.OutCapital = double.Parse(row["out_capital"].ToString());
               if (row.Table.Columns["judgment_penalty_rate"] != null)
                   paidGuarantee.JudgmentRate = double.Parse(row["judgment_penalty_rate"].ToString());
               if (row.Table.Columns["overdue_capital"] != null)
                   paidGuarantee.OverdueCapital = double.Parse(row["overdue_capital"].ToString());
               if (row.Table.Columns["Subsidia_Current_Rate_Value"] != null)
                   paidGuarantee.SubsidiaCurrentRateValue = double.Parse(row["Subsidia_Current_Rate_Value"].ToString());
               if (row.Table.Columns["current_fee"] != null)
                   paidGuarantee.CurrentFee = double.Parse(row["current_fee"].ToString());
               paidGuarantee.StartCapital = double.Parse(row["start_capital"].ToString());
               paidGuarantee.CurrentCapital = double.Parse(row["current_capital"].ToString());
               paidGuarantee.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
               paidGuarantee.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
               paidGuarantee.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
               paidGuarantee.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
               paidGuarantee.TotalFee = double.Parse(row["total_fee"].ToString());
               paidGuarantee.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
               if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
               {
                   paidGuarantee.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
               }
               paidGuarantee.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
               paidGuarantee.EndDate = DateTime.Parse(row["date_of_normal_end"].ToString());
               paidGuarantee.ContractNumber = ulong.Parse(row["security_code_2"].ToString());
               paidGuarantee.LoanAccount = Account.GetAccount(ulong.Parse(row["loan_full_number"].ToString()));
               paidGuarantee.ConnectAccount= Account.GetAccount(ulong.Parse(row["connect_account_full_number"].ToString()));
               if(row.Table.Columns["request_status"]!=null )
                   paidGuarantee.RequestStatus = int.Parse(row["request_status"].ToString());

                paidGuarantee.ProductType = Utility.GetProductTypeFromLoanType(paidGuarantee.LoanType);
                paidGuarantee.CreditCode = LoanProduct.GetCreditCode(paidGuarantee.ProductId, paidGuarantee.ProductType);

                if (row["Overdue_loan_date_for_classification"] != DBNull.Value)
                {
                    paidGuarantee.OverdueLoanDateForClassification = Convert.ToDateTime(row["Overdue_loan_date_for_classification"].ToString());
                }

            }
           return paidGuarantee;
       }
    }
}
