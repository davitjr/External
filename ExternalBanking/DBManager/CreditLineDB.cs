using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    static class CreditLineDB
    {
        internal static CreditLine GetCardCreditLine(string cardNumber)
        {
            CreditLine creditLine = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT CR.app_id,
									  la.interest_rate_effective_with_only_bank_profit,
                                      security_code_2,
                                      start_capital,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                      abs(overdue_capital) as overdue_capital,
                                      abs(penalty_rate) as penalty_rate,
								      abs(out_capital) as out,
                                      abs(out_penalty) as out_penalty,
									  abs(current_rate_value) as current_rate_value,
									  abs(judgment_penalty_rate) as judgment_penalty_rate,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      Abs(total_rate_value) as total_rate_value,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,
                                      current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      on CR.app_id = LA.app_id
                                      WHERE visa_number=@cardNumber and loan_type=8";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        creditLine = SetCreditLine(row);
                    }
                }
            }

            return creditLine;
        }

        internal static async Task<CreditLine> GetCardCreditLineAsync(string cardNumber)
        {
            CreditLine creditLine = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"  SELECT CR .app_id,
                                      security_code_2,
                                      LA.interest_rate_effective_with_only_bank_profit,
                                      start_capital,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      CR.loan_full_number,
                                      interest_rate,
                                      abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                      abs(overdue_capital) as overdue_capital,
                                      abs(penalty_rate) as penalty_rate,
								      abs(out_capital) as out,
                                      abs(out_penalty) as out_penalty,
									  abs(current_rate_value) as current_rate_value,
									  abs(judgment_penalty_rate) as judgment_penalty_rate,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      Abs(total_rate_value) as total_rate_value,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,current_rate_value_nused,
                                      matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE visa_number=@cardNumber and loan_type=8 ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        creditLine = SetCreditLine(row);
                    }
                }
            }

            return creditLine;
        }

        internal static List<CreditLine> GetCreditLines(ulong customerNumber)
        {
            List<CreditLine> creditLines = new List<CreditLine>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT CR.app_id,
                                      la.interest_rate_effective_with_only_bank_profit,
                                      security_code_2,
                                      start_capital,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      out_capital as out,
                                      abs(out_penalty) as out_penalty,
                                      overdue_capital,
                                      penalty_rate,
                                      judgment_penalty_rate ,
                                      current_rate_value,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,
                                      current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE cr.customer_number=@customerNumber and loan_type<>9 ";

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

                    if (dt.Rows.Count > 0)
                        creditLines = new List<CreditLine>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        CreditLine creditLine = SetCreditLine(row);

                        creditLines.Add(creditLine);
                    }

                }
            }

            return creditLines;
        }

        internal static List<CreditLine> GetClosedCreditLines(ulong customerNumber)
        {
            List<CreditLine> creditLines = new List<CreditLine>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(cr.app_id,0) as app_id,
                                      la.interest_rate_effective_with_only_bank_profit,
                                      start_capital,
                                      security_code_2,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      out_capital as out,
                                      abs(out_penalty) as out_penalty,
                                      overdue_capital,
                                      penalty_rate,
                                      judgment_penalty_rate ,
                                      current_rate_value,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,current_rate_value_nused,
                                      matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Closed_Credit_Lines CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE cr.customer_number=@customerNumber and loan_type<>9 ";

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

                    if (dt.Rows.Count > 0)
                        creditLines = new List<CreditLine>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        CreditLine creditLine = SetCreditLine(row);

                        creditLines.Add(creditLine);
                    }

                }
            }

            return creditLines;
        }


        internal static CreditLine GetClosedCreditLine(ulong productId, ulong customerNumber)
        {
            CreditLine creditLine = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(cr.app_id,0) as app_id,
                                     la.interest_rate_effective_with_only_bank_profit,
                                      start_capital,
                                      security_code_2,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      out_capital as out,
                                      abs(out_penalty) as out_penalty,
                                      overdue_capital,
                                      penalty_rate,
                                      judgment_penalty_rate ,
                                      current_rate_value,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent, 
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,
                                      current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused ,date_of_credit_line_stopping ,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Closed_Credit_Lines CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE cr.customer_number=@customerNumber and CR.App_Id=@app_id and loan_type<>9 ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            creditLine = SetCreditLine(row);

                        }

                }
            }


            return creditLine;
        }



        /// <summary>
        /// Վերադարձնում է վարկային գծի  դադարեցման հայտի տվյալները
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        internal static CreditLineTerminationOrder GetCreditLineTerminationOrder(CreditLineTerminationOrder order)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT  d.document_number,d.currency,d.debet_account,d.quality,d.description,
                                                                                                                 d.registration_date,d.document_type, d.document_subtype,d.source_type,d.operation_date,d.order_group_id, d.confirmation_date
                                                                                              FROM Tbl_HB_documents as d 
                                                                                              WHERE d.Doc_ID=@DocID and d.customer_number=@customer_number", conn))
                {
                    cmd.Parameters.Add("@DocID", SqlDbType.Int).Value = order.Id;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    order.CreditLineAccount = new Account();
                    dt.Load(cmd.ExecuteReader());

                    order.ProductId = ulong.Parse(dt.Rows[0]["document_number"].ToString());
                    //order.FilialCode = ushort.Parse(dt.Rows[0]["operationFilialCode"].ToString());
                    order.Currency = dt.Rows[0]["currency"].ToString();
                    order.CreditLineAccount.AccountNumber = dt.Rows[0]["debet_account"].ToString();
                    order.CreditLineAccount.Currency = string.Empty;
                    order.Quality = (OrderQuality)(dt.Rows[0]["quality"]);
                    order.Description = dt.Rows[0]["description"].ToString();
                    order.RegistrationDate = Convert.ToDateTime(dt.Rows[0]["registration_date"]);
                    order.Type = (OrderType)Convert.ToInt16(dt.Rows[0]["document_type"]);
                    order.SubType = Convert.ToByte(dt.Rows[0]["document_subtype"]);
                    order.Source = (SourceType)int.Parse(dt.Rows[0]["source_type"].ToString());
                    order.OperationDate = dt.Rows[0]["operation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["operation_date"]) : default(DateTime?);
                    order.GroupId = dt.Rows[0]["order_group_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["order_group_id"]) : 0;
                    order.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime?);
                    return order;
                }


            }

        }



        private static CreditLine SetCreditLine(DataRow row)
        {
            CreditLine creditLine = new CreditLine();

            if (row != null)
            {
                if (row["interest_rate_effective_with_only_bank_profit"] != DBNull.Value)
                {
                    if (double.Parse(row["interest_rate_effective_with_only_bank_profit"].ToString()) * 100 != 0)
                    {
                        creditLine.InterestRateEffectiveWithOnlyBankProfit = double.Parse(row["interest_rate_effective_with_only_bank_profit"].ToString());
                    }
                }

                creditLine.ProductId = long.Parse(row["app_id"].ToString());
                creditLine.LoanAccount = Account.GetAccount(ulong.Parse(row["loan_full_number"].ToString()));
                if (creditLine.LoanAccount == null)
                {
                    creditLine.LoanAccount = Account.GetSystemAccount(row["loan_full_number"].ToString());
                }

                creditLine.Type = byte.Parse(row["credit_line_type"].ToString());
                creditLine.Quality = byte.Parse(row["quality"].ToString());
                creditLine.Currency = row["Currency"].ToString();
                creditLine.InterestRate = float.Parse(row["interest_rate"].ToString());
                if (row.Table.Columns["out"] != null)
                    creditLine.OutCapital = double.Parse(row["out"].ToString());
                if (row.Table.Columns["judgment_penalty_rate"] != null)
                    creditLine.JudgmentRate = double.Parse(row["judgment_penalty_rate"].ToString());
                if (row.Table.Columns["overdue_capital"] != null)
                    creditLine.OverdueCapital = double.Parse(row["overdue_capital"].ToString());
                if (row.Table.Columns["penalty_rate"] != null)
                    creditLine.PenaltyRate = double.Parse(row["penalty_rate"].ToString());
                if (row.Table.Columns["current_rate_value"] != null)
                    creditLine.CurrentRateValue = double.Parse(row["current_rate_value"].ToString());
                if (row.Table.Columns["inpaied_rest_of_rate"] != null)
                    creditLine.InpaiedRestOfRate = double.Parse(row["inpaied_rest_of_rate"].ToString());
                if (row.Table.Columns["out_penalty"] != null)
                    creditLine.OutPenalty = double.Parse(row["out_penalty"].ToString());
                creditLine.StartCapital = double.Parse(row["start_capital"].ToString());
                creditLine.CurrentCapital = double.Parse(row["current_capital"].ToString());
                creditLine.PenaltyAdd = double.Parse(row["penalty_add"].ToString());
                creditLine.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                creditLine.EndDate = (DateTime)row["date_of_normal_end"];
                creditLine.TotalRateValue = double.Parse(row["total_rate_value"].ToString());
                creditLine.LastDateOfRateRepair = row["last_day_of_rate_repair"] != DBNull.Value ? DateTime.Parse(row["last_day_of_rate_repair"].ToString()) : default(DateTime?);
                creditLine.OverdueLoanDate = row["overdue_loan_date"] != DBNull.Value ? DateTime.Parse(row["overdue_loan_date"].ToString()) : default(DateTime?);
                if (row["day_of_rate_calculation"] != DBNull.Value && row["day_of_rate_calculation"].ToString() != "")
                {
                    creditLine.DayOfRateCalculation = DateTime.Parse(row["day_of_rate_calculation"].ToString());
                }
                creditLine.ProductType = 2;
                creditLine.ConnectAccount = Account.GetAccount(ulong.Parse(row["connect_account_full_number"].ToString()));
                creditLine.JudgmentStartDate = row["date_of_judgment_begin"] != DBNull.Value ? DateTime.Parse(row["date_of_judgment_begin"].ToString()) : default(DateTime?);
                creditLine.JudgmentEndDate = row["date_of_judgment_end"] != DBNull.Value ? DateTime.Parse(row["date_of_judgment_end"].ToString()) : default(DateTime?);
                creditLine.JudgmentPenaltyPercent = float.Parse(row["judgment_penalty_percent"].ToString());
                creditLine.InterestRateEffective = float.Parse(row["interest_rate_effective"].ToString());
                creditLine.InterestRateFull = float.Parse(row["interest_rate_full"].ToString());
                creditLine.DailyPenaltyInterestRate = float.Parse(row["penalty_add_percent"].ToString());

                if (creditLine.Quality == 40)
                {
                    creditLine.ClosingDate = row["change_date"] != DBNull.Value ? DateTime.Parse(row["change_date"].ToString()) : default(DateTime?);
                }
                creditLine.FillialCode = int.Parse(row["filialcode"].ToString());
                creditLine.HasClaim = LoanProduct.CheckLoanProductClaimAvailability(creditLine.ProductId);
                if (row.Table.Columns.Contains("security_code_2"))
                    creditLine.ContractNumber = ulong.Parse(row["security_code_2"].ToString());


                if (row["matured_current_rate_value"] != DBNull.Value)
                {
                    creditLine.MaturedCurrentRateValue = double.Parse(row["matured_current_rate_value"].ToString());
                }

                if (row["matured_judgment_penalty_rate"] != DBNull.Value)
                {
                    creditLine.MaturedJudgmentPenaltyRate = double.Parse(row["matured_judgment_penalty_rate"].ToString());
                }

                if (row["matured_penalty"] != DBNull.Value)
                {
                    creditLine.MaturedPenaltyRate = double.Parse(row["matured_penalty"].ToString());
                }

                creditLine.CreditCode = LoanProduct.GetCreditCode(creditLine.ProductId, creditLine.ProductType);
                creditLine.CurrentRateValueUnused = Math.Abs(double.Parse(row["current_rate_value_nused"].ToString()));
                creditLine.MaturedCurrentRateValueUnused = Math.Abs(double.Parse(row["matured_current_rate_value_nused"].ToString()));

                if (row.Table.Columns["interest_rate_nused"] != null)
                {
                    creditLine.InterestRateNused = double.Parse(row["interest_rate_nused"].ToString());
                }


                creditLine.DateOfCreditLineStopping = row["date_of_credit_line_stopping"] != DBNull.Value ? DateTime.Parse(row["date_of_credit_line_stopping"].ToString())
                    : default(DateTime?);

                if (row["date_of_stopping_calculation"] != DBNull.Value)
                {
                    creditLine.DateOfStoppingCalculation = Convert.ToDateTime(row["date_of_stopping_calculation"].ToString());
                }

                if (row["date_of_stopping_penalty_calculation"] != DBNull.Value)
                {
                    creditLine.DateOfStoppingPenaltyCalculation = Convert.ToDateTime(row["date_of_stopping_penalty_calculation"].ToString());
                }

                if (row["visa_number"] != DBNull.Value)
                {
                    creditLine.CardNumber = row["visa_number"].ToString();
                }

            }
            return creditLine;
        }
        /// <summary>
        /// Քարտի վարկային գծի գրաֆիկ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static List<CreditLineGrafik> GetCreditLineGrafik(ulong productId)
        {
            List<CreditLineGrafik> creditLineGrafik = new List<CreditLineGrafik>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"Select 
                                        beg_period_of_repayment,
                                        date_of_repayment,
                                        capital_repayment,
                                        matured_amount,
                                        for_overdue,
                                        planned_repayment 
                                        from 
                                        Tbl_repayments_of_credit_lines 
                                        where app_id=@productId";

                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        creditLineGrafik = new List<CreditLineGrafik>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        CreditLineGrafik creditLineRepayment = SetCreditLineGrafikDetail(row);

                        creditLineGrafik.Add(creditLineRepayment);
                    }


                }
            }
            creditLineGrafik = creditLineGrafik.OrderBy(o => o.StartDate).ToList();
            return creditLineGrafik;
        }
        private static CreditLineGrafik SetCreditLineGrafikDetail(DataRow row)
        {
            CreditLineGrafik creditLineRepayment = new CreditLineGrafik();

            if (row != null)
            {
                creditLineRepayment.StartDate = DateTime.Parse(row["beg_period_of_repayment"].ToString());
                creditLineRepayment.EndDate = DateTime.Parse(row["date_of_repayment"].ToString());
                creditLineRepayment.Amount = double.Parse(row["capital_repayment"].ToString());
                creditLineRepayment.MaturedAmount = double.Parse(row["matured_amount"].ToString());
                creditLineRepayment.OverdueSign = byte.Parse(row["for_overdue"].ToString());
                creditLineRepayment.PlannedAmount = double.Parse(row["planned_repayment"].ToString());
            }
            return creditLineRepayment;
        }
        internal static CreditLine GetCreditLine(ulong productId, ulong customerNumber)
        {
            CreditLine creditLine = new CreditLine();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT   cr.app_id,
                                        la.interest_rate_effective_with_only_bank_profit,
                                        security_code_2,
                                        start_capital,
                                        CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                        date_of_normal_end,
                                        current_capital,
                                        Currency,
                                        loan_type,
                                        credit_line_type,     
                                        quality,
                                        cr.loan_full_number,
                                        interest_rate,
                                        abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                        abs(overdue_capital) as overdue_capital,
                                        abs(penalty_rate) as penalty_rate,
								        abs(out_capital) as out,
                                        abs(out_penalty) as out_penalty,
									    abs(current_rate_value) as current_rate_value,
									    abs(judgment_penalty_rate) as judgment_penalty_rate,
                                        [last_day_of rate calculation] as day_of_rate_calculation,
                                        penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                        Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                        interest_rate_effective,interest_rate_full,Change_date,filialcode,filialcode,penalty_add_percent ,
                                        matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty ,current_rate_value_nused,
                                        matured_current_rate_value_nused,interest_rate_nused , date_of_credit_line_stopping,
                                        date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                        FROM Tbl_Credit_Lines  CR 
                                        LEFT JOIN Tbl_liability_add  LA
                                        on CR.app_id = LA.app_id
                                        WHERE cr.App_id=@product_id and cr.customer_number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@product_id", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        creditLine = SetCreditLine(dt.Rows[0]);


                }
            }

            return creditLine;
        }

        internal static CreditLine GetCardOverDraft(string cardNumber)
        {
            CreditLine overdraft = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT cr.app_id,
                                      la.interest_rate_effective_with_only_bank_profit,
                                      security_code_2,
                                      start_capital,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                      abs(overdue_capital) as overdue_capital,
                                      abs(penalty_rate) as penalty_rate,
								      abs(out_capital) as out,
                                      abs(out_penalty) as out_penalty,
									  abs(current_rate_value) as current_rate_value,
									  abs(judgment_penalty_rate) as judgment_penalty_rate,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,current_rate_value_nused,
                                      matured_current_rate_value_nused ,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE visa_number=@cardNumber and loan_type=9 ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        overdraft = SetCreditLine(row);
                    }
                }
            }

            return overdraft;
        }

        internal static async Task<CreditLine> GetCardOverDraftAsync(string cardNumber)
        {
            CreditLine overdraft = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT CR.app_id,
                                      la.interest_rate_effective_with_only_bank_profit,
                                      security_code_2,
                                      start_capital,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      CR.loan_full_number,
                                      interest_rate,
                                      abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                      abs(overdue_capital) as overdue_capital,
                                      abs(penalty_rate) as penalty_rate,
								      abs(out_capital) as out,
                                      abs(out_penalty) as out_penalty,
									  abs(current_rate_value) as current_rate_value,
									  abs(judgment_penalty_rate) as judgment_penalty_rate,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty ,
                                      current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE visa_number=@cardNumber and loan_type=9 ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        overdraft = SetCreditLine(row);
                    }
                }
            }

            return overdraft;
        }

        /// <summary>
        /// Վարկային գծի դադարեցման հայտ
        /// </summary>
        /// <param name="order"></param>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static ActionResult SaveCreditLineTerminationOrder(CreditLineTerminationOrder order, string userName, SourceType source, int filialCode)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_terminate_CreditLine";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = order.ProductId;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = order.CustomerNumber;
                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 3).Value = order.Currency;
                    cmd.Parameters.Add("@loan_full_number", SqlDbType.Float).Value = order.CreditLineAccount.AccountNumber;
                    cmd.Parameters.Add("@reg_date", SqlDbType.SmallDateTime).Value = DateTime.Now.Date;
                    cmd.Parameters.Add("@quality", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 20).Value = userName;
                    cmd.Parameters.Add("@source_type", SqlDbType.Int).Value = (short)source;
                    cmd.Parameters.Add("@operationFilialCode", SqlDbType.Int).Value = order.FilialCode;
                    cmd.Parameters.Add("@oper_day", SqlDbType.SmallDateTime).Value = order.OperationDate;
                    if (order.GroupId != 0)
                        cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = order.GroupId;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = order.Id;


                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    order.Id = Convert.ToInt64(cmd.Parameters["@id"].Value);
                    result.Id = order.Id;
                    order.Quality = OrderQuality.Draft;
                    return result;
                }
            }

        }

        /// <summary>
        /// Վարկային գծի դադարեցման հայտի համար
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        internal static bool IsSecondTermination(ulong customerNumber, string orderNumber)
        {
            bool secondTermination;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"Select doc_ID from Tbl_HB_documents where quality in (2,3,5) and document_type=21 and document_subtype=1 and
                                                document_number=@ordernumber and  customer_number=@customer_number", conn))
                {
                    cmd.Parameters.Add("@ordernumber", SqlDbType.Float).Value = orderNumber;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    if (cmd.ExecuteReader().Read())
                    {
                        secondTermination = true;
                    }
                    else
                        secondTermination = false;
                }

            }
            return secondTermination;
        }

        internal static ActionResult SaveCreditLineTerminationOrderDetails(CreditLineTerminationOrder creditLineTerminationOrder, ulong orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Tbl_BO_credit_line_termination_details(order_id, currency, credit_line_account) VALUES(@orderId, @currency, @creditLineAccount)";

                    cmd.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@currency", SqlDbType.VarChar, 3).Value = creditLineTerminationOrder.Currency;
                    cmd.Parameters.Add("@creditLineAccount", SqlDbType.Float).Value = creditLineTerminationOrder.CreditLineAccount.AccountNumber;

                    cmd.ExecuteNonQuery();

                    result.ResultCode = ResultCode.Normal;

                    return result;
                }
            }
        }

        internal static List<CreditLine> GetCardClosedCreditLines(ulong customerNumber, string cardNumber)
        {

            List<CreditLine> creditLines = new List<CreditLine>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT ISNULL(cr.app_id,0) AS app_id,
                                      la.interest_rate_effective_with_only_bank_profit,
                                      start_capital,
                                      security_code_2,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                      abs(overdue_capital) as overdue_capital,
                                      abs(penalty_rate) as penalty_rate,
								      abs(out_capital) as out,
                                      abs(out_penalty) as out_penalty,
									  abs(current_rate_value) as current_rate_value,
									  abs(judgment_penalty_rate) as judgment_penalty_rate,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      Abs(total_rate_value) as total_rate_value,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,
                                      current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Closed_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id

                                      WHERE visa_number=@cardNumber and loan_type=8 AND cr.customer_number=@customerNumber";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar, 16).Value = cardNumber;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        creditLines = new List<CreditLine>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        DataRow row = dt.Rows[i];

                        CreditLine creditLine = SetCreditLine(row);

                        creditLines.Add(creditLine);
                    }
                }
            }

            return creditLines;
        }

        internal static List<LoanMainContract> GetCreditLineMainContract(ulong customerNumber)
        {
            List<LoanMainContract> contracts = new List<LoanMainContract>();


            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT general_number, 
                                      start_capital,
                                      currency,
                                      date_of_beginning_contract,
                                      date_of_normal_end_contract
                                      FROM Tbl_main_loans_contracts 
                                      WHERE customer_number=@customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            LoanMainContract contract = new LoanMainContract();
                            contract.Amount = Convert.ToDouble(dt.Rows[i]["start_capital"]);
                            contract.Currency = dt.Rows[i]["currency"].ToString();
                            if (dt.Rows[i]["date_of_beginning_contract"] != DBNull.Value)
                            {
                                contract.StartDate = Convert.ToDateTime(dt.Rows[i]["date_of_beginning_contract"]);
                            }
                            if (dt.Rows[i]["date_of_normal_end_contract"] != DBNull.Value)
                            {
                                contract.EndDate = Convert.ToDateTime(dt.Rows[i]["date_of_normal_end_contract"]);
                            }
                            contract.GeneralNumber = dt.Rows[i]["general_number"].ToString();
                            contracts.Add(contract);
                        }
                    }


                }
            }


            return contracts;

        }
        internal static CreditLine GetCreditLine(string loanFullNumber)
        {
            CreditLine creditLine = new CreditLine();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT   cr.app_id,
                                        la.interest_rate_effective_with_only_bank_profit,
                                        security_code_2,
                                        start_capital,
                                        CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                        date_of_normal_end,
                                        current_capital,
                                        Currency,
                                        loan_type,
                                        credit_line_type,     
                                        quality,
                                        cr.loan_full_number,
                                        interest_rate,
                                        abs(inpaied_rest_of_rate) as inpaied_rest_of_rate,
                                        abs(overdue_capital) as overdue_capital,
                                        abs(penalty_rate) as penalty_rate,
								        abs(out_capital) as out,
                                        abs(out_penalty) as out_penalty,
									    abs(current_rate_value) as current_rate_value,
									    abs(judgment_penalty_rate) as judgment_penalty_rate,
                                        [last_day_of rate calculation] as day_of_rate_calculation,
                                        penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                        Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                        interest_rate_effective,interest_rate_full,Change_date,filialcode,filialcode,penalty_add_percent ,
                                        matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,
                                        current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                        date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      

                                        FROM Tbl_Credit_Lines  CR 
                                         LEFT JOIN Tbl_liability_add  LA
                                         ON CR.app_id = LA.app_id
                                        WHERE cr.loan_full_number=@loanFullNumber";

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
                        creditLine = SetCreditLine(dt.Rows[0]);
                    else
                        return null;
                }
            }

            return creditLine;
        }
        public static List<LoanRepaymentGrafik> GetDecreaseLoanGrafik(long productId)
        {
            List<LoanRepaymentGrafik> list = new List<LoanRepaymentGrafik>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT date_of_decrease, amount, rest 
                                        FROM [Tbl_credit_lines_decrease_grafik]
                                        WHERE app_id = @app_id
                                        ORDER BY date_of_decrease";
                    cmd.Parameters.AddWithValue("@app_id", productId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);

                        if (dt.Rows.Count != 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                DataRow row = dt.Rows[i];
                                LoanRepaymentGrafik loanRepayment = SetDecreaseLoanGrafikDetails(row);
                                list.Add(loanRepayment);
                            }

                            list = list.OrderBy(o => o.RepaymentDate).ToList();
                        }
                        else
                            list = null;
                    }
                }
                return list;
            }
        }

        private static LoanRepaymentGrafik SetDecreaseLoanGrafikDetails(DataRow row)
        {
            LoanRepaymentGrafik grafik = new LoanRepaymentGrafik();
            grafik.RepaymentDate = DateTime.Parse(row["date_of_decrease"].ToString());
            grafik.RestCapital = double.Parse(row["rest"].ToString());
            grafik.TotalRepayment = double.Parse(row["amount"].ToString());
            return grafik;
        }


        internal static string GetCreditLineCardNumber(ulong productId)
        {
            string cardnumber = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT visa_number FROM tbl_credit_lines WHERE app_id=@productId", conn))
                {
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                    cardnumber = cmd.ExecuteScalar().ToString();
                }
            }
            return cardnumber;
        }




        internal static DataTable GetCreditLinePrecontractData(DateTime startDate, DateTime endDate, double interestRate, double repaymentPercent, string cardNumber, string currency, double amount, int loanType)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("effRate");
            dt.Columns.Add("effRateWithoutAccountServiceFee");
            dt.Columns.Add("repaymentKurs");

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("pr_Get_Precontract_HB_Deposit_Credit_Line_Data", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@start_date", SqlDbType.SmallDateTime).Value = startDate;
                    cmd.Parameters.Add("@end_date", SqlDbType.SmallDateTime).Value = endDate;
                    cmd.Parameters.Add("@interest_rate", SqlDbType.Float).Value = interestRate;
                    cmd.Parameters.Add("@repayment_percent", SqlDbType.Float).Value = repaymentPercent;
                    cmd.Parameters.Add("@card_number", SqlDbType.NVarChar).Value = cardNumber;
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar).Value = currency;
                    cmd.Parameters.Add("@amount", SqlDbType.Float).Value = amount;
                    cmd.Parameters.Add("@loan_type", SqlDbType.Int).Value = loanType;


                    SqlParameter param = new SqlParameter("@TotalRep", SqlDbType.Money);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@TotalPERCENT", SqlDbType.Money);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@TotalFeeByGRAFIK", SqlDbType.Money);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@TotalFee", SqlDbType.Money);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@effRate", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@penaltyRate", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@effRateNoFull", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@cash_rate_repayment", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@effRateWithoutAccountServiceFee", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@accountServiceFee", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@repaymentKurs", SqlDbType.Float);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);


                    cmd.ExecuteReader();


                    dt.Rows.Add(Convert.ToDouble(cmd.Parameters["@effRate"].Value != DBNull.Value ? cmd.Parameters["@effRate"].Value : 0), Convert.ToDouble(cmd.Parameters["@effRateWithoutAccountServiceFee"].Value != DBNull.Value ? cmd.Parameters["@effRateWithoutAccountServiceFee"].Value : 0), Convert.ToDouble(cmd.Parameters["@repaymentKurs"].Value != DBNull.Value ? cmd.Parameters["@repaymentKurs"].Value : 0));
                }
            }
            return dt;
        }


        internal static string GetCardTypeName(string cardNumber)
        {
            string cardTypeName = "";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT  ApplicationsCardType FROM Tbl_Visa_Numbers_Accounts  va 
										                         INNER JOIN tbl_type_of_Card C
										                         ON va.card_type = c.id
                                                                 WHERE visa_number=@cardNumber", conn))
                {
                    cmd.Parameters.Add("@cardNumber", SqlDbType.NVarChar).Value = cardNumber;
                    cardTypeName = cmd.ExecuteScalar().ToString();
                }
            }
            return cardTypeName;
        }


        internal static byte[] GetLoansDramContract(long docId, int productType, bool fromApprove, ulong customerNumber)
        {
            byte[] result;
            LoanProductOrder order = new LoanProductOrder();
            Dictionary<string, string> parameters = Provision.ProvisionContract(docId).ToDictionary(x => x.Key, x => x.Value);
            short filialCode = Customer.GetCustomerFilial(customerNumber).key;
            string contractName = "ProvisionContract";
            Customer customer = new Customer();
            DateTime contractDate = new DateTime();

            if (productType == 1)
            {
                order = customer.GetLoanOrder(docId);
            }
            else if (productType == 2)
            {
                order = customer.GetCreditLineOrder(docId);
                contractDate = GetCreditLineContractDate(docId);
            }


            parameters.Add(key: "customerNumberHB", value: customerNumber.ToString());
            ContractServiceRef.Contract contract = null;
            if (fromApprove == true)
            {
                contract = new ContractServiceRef.Contract();

                parameters.Add(key: "attachFile", value: "1");
                contract.AttachDocType = 10;
                contract.AttachFile = 1;
                contract.ContractName = contractName;
                contract.DocID = (int)docId;
                contract.ParametersList = new List<ContractServiceRef.StringKeyValue>();
            }

            parameters.Add(key: "IDContract", value: "0");
            parameters.Add(key: "provisionNumber", value: "0");
            parameters.Add(key: "contractType", value: "7");
            parameters.Add(key: "HbDocID", value: docId.ToString());
            parameters.Add(key: "startCapitalHB", value: order.ProvisionAmount.ToString());
            parameters.Add(key: "amountHB", value: order.Amount.ToString());
            if (contractDate != default(DateTime))
            {
                parameters.Add(key: "dateOfBeginningHB", value: contractDate.ToString());
                parameters.Add(key: "dateOfBeginningContractHB", value: contractDate.ToString());
            }
            else
            {
                parameters.Add(key: "dateOfBeginningHB", value: order.StartDate.ToString());
                parameters.Add(key: "dateOfBeginningContractHB", value: order.StartDate.ToString());
            }

            if (order.Source == SourceType.AcbaOnline || order.Source == SourceType.MobileBanking)
            {
                parameters.Add(key: "currencyProvisionHB", value: order.PledgeCurrency.ToString());
            }
            else
            {
                parameters.Add(key: "currencyProvisionHB", value: order.ProvisionCurrency.ToString());
            }
            //provision currency i poxaren pledgecurrency
            parameters.Add(key: "currencyHB", value: order.Currency.ToString());
            parameters.Add(key: "provisionNumberHB", value: "/01");
            parameters.Add(key: "filialCodeHB", value: filialCode.ToString());
            parameters.Add(key: "productTypeHB", value: productType.ToString());


            result = Contracts.RenderContract(contractName, parameters, "ProvisionContract.pdf", contract);
            return result;
        }
        /// <summary>
        /// Վարկային գծի պայմանագրի առկայության ստուգում
        /// </summary>
        /// <param name="loanAccountNumber"></param>
        /// <returns></returns>
        internal static bool HasUploadedCreditLineContract(string loanAccountNumber)
        {
            int docId = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT hb_doc_id 
                               FROM [Tbl_credit_lines]
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
            using (SqlConnection conn2 = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
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
        internal static bool HasActiveCreditLineForCardAccount(string cardAccount)
        {
            bool hasActiveCreditLine = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 1 loan_number FROM [Tbl_credit_lines] WHERE connect_account_full_number=@cardAccount AND loan_type<>9", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardAccount", SqlDbType.Float).Value = cardAccount;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            hasActiveCreditLine = true;
                        }
                    }

                }

            }
            return hasActiveCreditLine;
        }

        internal static async Task<List<CreditLine>> GetCreditLinesAsync(ulong customerNumber)
        {
            List<CreditLine> creditLines = new List<CreditLine>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT CR.app_id,
                                      la.interest_rate_effective_with_only_bank_profit,
                                      security_code_2,
                                      start_capital,
                                      CASE WHEN CR.contract_date IS NULL THEN cr.date_of_beginning ELSE CR.contract_date END AS date_of_beginning,
                                      date_of_normal_end,
                                      current_capital,
                                      Currency,
                                      loan_type,
                                      credit_line_type,
                                      quality,
                                      cr.loan_full_number,
                                      interest_rate,
                                      out_capital as out,
                                      abs(out_penalty) as out_penalty,
                                      overdue_capital,
                                      penalty_rate,
                                      judgment_penalty_rate ,
                                      current_rate_value,
                                      [last_day_of rate calculation] as day_of_rate_calculation,
                                      penalty_add,overdue_loan_date,last_day_of_rate_repair,connect_account_full_number,
                                      Abs(total_rate_value) as total_rate_value,date_of_judgment_begin,date_of_judgment_end,judgment_penalty_percent,
                                      interest_rate_effective,interest_rate_full,Change_date,filialcode,penalty_add_percent ,
                                      matured_current_rate_value,matured_judgment_penalty_rate,matured_penalty,
                                      current_rate_value_nused,matured_current_rate_value_nused,interest_rate_nused,date_of_credit_line_stopping,
                                      date_of_stopping_calculation,date_of_stopping_penalty_calculation,visa_number
                                      FROM Tbl_Credit_Lines  CR 
                                      LEFT JOIN Tbl_liability_add  LA
                                      ON CR.app_id = LA.app_id
                                      WHERE cr.customer_number=@customerNumber and loan_type<>9 ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                        creditLines = new List<CreditLine>();

                    Parallel.For(0, dt.Rows.Count, (i) =>
                    {

                        DataRow row = dt.Rows[i];

                        CreditLine creditLine = SetCreditLine(row);

                        creditLines.Add(creditLine);
                    });

                }
            }

            return creditLines;
        }

        internal static List<CreditLine> GetCreditLinesForTotalBalance(ulong customerNumber)
        {
            List<CreditLine> creditLines = new List<CreditLine>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT 
                                start_capital,
                                current_capital,
                                out_capital as out,
							    quality,
                                currency
                                FROM Tbl_Credit_Lines  CR 
                                LEFT JOIN Tbl_liability_add  LA
                                ON CR.app_id = LA.app_id
                                WHERE cr.customer_number=@customerNumber and loan_type<>9";

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

                        CreditLine creditLine = new CreditLine
                        {
                            StartCapital = Convert.ToDouble(row["start_capital"].ToString()),
                            CurrentCapital = Convert.ToDouble(row["current_capital"].ToString()),
                            OutCapital = Convert.ToDouble(row["out"].ToString()),
                            Quality = Convert.ToInt16(row["quality"].ToString()),
                            Currency = row["currency"].ToString()
                        };

                        creditLines.Add(creditLine);
                    }

                }
            }

            return creditLines;
        }
        internal static void UpdateCreditLinesFnameOnline(ulong productId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"UPDATE tbl_credit_lines
                                        SET F_name = 'ONLINE'
                                        WHERE App_Id = @productId";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static void UpdateCloseCreditLinesFnameOnline(ulong productId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"UPDATE Tbl_closed_credit_lines
                                        SET F_name = 'ONLINE'
                                        WHERE App_Id = @productId";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static double GetCreditLineballance(ulong productId)
        {
            double ballance;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT start_capital FROM Tbl_credit_lines 
                                        WHERE app_id = @productId";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    ballance = Convert.ToDouble(cmd.ExecuteScalar().ToString());
                }
            }
            return ballance;
        }
        internal static void SaveCreditLineByApiGate(long docId,double productId,ulong orderId )
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO tbl_Credit_lines_Activate_By_ApiGate
                                        VALUES(@docId,GETDATE(),@appId,@orderId)";
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                    cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@orderId", SqlDbType.Float).Value = orderId;

                    cmd.ExecuteNonQuery();
                }
            }

        }
        internal static bool IsCreditLineActivateOnApiGate(long docId)
        {
            bool isApiGate = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT * FROM tbl_Credit_lines_Activate_By_ApiGate
                                        WHERE doc_ID = @docId";
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            isApiGate = true;
                        }
                    }
                }
            }
            return isApiGate;
        }
        internal static void UpdateCreditLinesFnameNull(ulong productId)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"UPDATE tbl_credit_lines
                                        SET F_name = 'NULL'
                                        WHERE App_Id = @productId";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static bool IsProlongApiGate(ulong productId)
        {
            var isApiGate = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT f_name
                                        FROM tbl_credit_lines 
                                        tbl_credit_lines  WHERE  App_Id = @productId";
                    
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    var result = cmd.ExecuteScalar();
                    if (result == DBNull.Value)
                        isApiGate = true;
                }
            }
            return isApiGate;
        }
        internal static double GetArcaLimitBallance(ulong productId)
        {
            double ballance;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT arca_limit FROM Tbl_credit_lines 
                                        WHERE app_id = @productId";
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;

                    ballance = Convert.ToDouble(cmd.ExecuteScalar().ToString());
                }
            }
            return ballance;
        }

        public static DateTime GetCreditLineContractDate(long docID)
        {
            var date = new DateTime();

            using (SqlConnection DbConn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                DbConn.Open();

                SqlCommand cmd = new SqlCommand(@"
                                                  SELECT  contract_date FROM Tbl_New_Credit_Line_Documents 
                                                  WHERE Doc_ID = @docID", DbConn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@docID", SqlDbType.Int).Value = docID;
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                    date = dr["contract_date"] != DBNull.Value ? Convert.ToDateTime(dr["contract_date"]) : default;
            }
            return date;
        }


    }
}