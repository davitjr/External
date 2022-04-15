using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class ProblemLoanTaxFilterDB
    {
        public static Dictionary<int, List<ProblemLoanTax>> SearchProblemLoanTax(ProblemLoanTaxFilter problemLoanTaxFilter, bool Cache)
        {
            Dictionary<int, List<ProblemLoanTax>> searchResults = new Dictionary<int, List<ProblemLoanTax>>();
            List<ProblemLoanTax> problemLoanTaxesList = new List<ProblemLoanTax>();
            ProblemLoanTax problemLoanTaxes;
            int totalCustomersCount = 0;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_get_problem_loan_tax_list";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@filialCode", SqlDbType.SmallInt).Value = problemLoanTaxFilter.FilialCode;
                    cmd.Parameters.Add("@endRow", SqlDbType.SmallInt).Value = problemLoanTaxFilter.Row * 500;
                    cmd.Parameters.Add("@startRow", SqlDbType.SmallInt).Value = (problemLoanTaxFilter.Row - 1) * 500;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = problemLoanTaxFilter.CustomerNumber;
                    cmd.Parameters.Add("@nameFull", SqlDbType.NVarChar).Value = problemLoanTaxFilter.FullName;
                    cmd.Parameters.Add("@loanFullNumber", SqlDbType.Float).Value = problemLoanTaxFilter.LoanFullNumber;
                    if (problemLoanTaxFilter.TaxRegistrationStartDate != default(DateTime))
                    {
                        cmd.Parameters.Add("@taxRegistrationStartDate", SqlDbType.SmallDateTime).Value = problemLoanTaxFilter.TaxRegistrationStartDate;
                    }
                    if (problemLoanTaxFilter.TaxRegistrationEndDate != default(DateTime))
                    {
                        cmd.Parameters.Add("@taxRegistrationEndDate", SqlDbType.SmallDateTime).Value = problemLoanTaxFilter.TaxRegistrationEndDate;
                    }
                    cmd.Parameters.Add("@taxAmount", SqlDbType.Float).Value = problemLoanTaxFilter.TaxAmount;
                    cmd.Parameters.Add("@isTransferRegistratoinDateExists", SqlDbType.SmallInt).Value = problemLoanTaxFilter.IsTransferRegistratoinDateExists;
                    cmd.Parameters.Add("@taxQuality", SqlDbType.SmallInt).Value = problemLoanTaxFilter.TaxQuality;
                    cmd.Parameters.Add("@taxCourtDecision", SqlDbType.SmallInt).Value = problemLoanTaxFilter.TaxCourtDecision;
                    cmd.Parameters.Add("@cache", SqlDbType.SmallInt).Value = Cache;

                    using SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        totalCustomersCount = dr["RowCnt"] != DBNull.Value ? Convert.ToInt32(dr["RowCnt"].ToString()) : default(int);
                    }

                    dr.NextResult();

                    while (dr.Read())
                    {
                        problemLoanTaxes = new ProblemLoanTax();

                        problemLoanTaxes.Row = Convert.ToInt32(dr["Row"]);
                        problemLoanTaxes.FilialCode = Convert.ToInt16(dr["filialcode"]);
                        problemLoanTaxes.CustomerNumber = Convert.ToUInt64(dr["customer_number"]);
                        problemLoanTaxes.FullName = Utility.ConvertAnsiToUnicode(dr["NameFull"].ToString());
                        problemLoanTaxes.LoanFullNumber = Convert.ToUInt64(dr["loan_full_number"]);
                        problemLoanTaxes.AppId = Convert.ToUInt64(dr["app_id"]);
                        problemLoanTaxes.LoanType = Convert.ToInt16(dr["loan_type"]);
                        problemLoanTaxes.TaxRegistrationDate = Convert.ToDateTime(dr["tax_registration_date"]);
                        problemLoanTaxes.TaxRegistrationTime = dr["tax_registration_time"] != DBNull.Value ? Convert.ToDateTime(dr["tax_registration_time"]) : default(DateTime);
                        problemLoanTaxes.TaxAmount = Convert.ToDecimal(dr["tax_amount"]);
                        problemLoanTaxes.TransferRegistrationDate = dr["transfer_registration_date"] != DBNull.Value ? Convert.ToDateTime(dr["transfer_registration_date"]) : default(DateTime); //Convert.ToDateTime(dr["transfer_registration_date"]);
                        problemLoanTaxes.TransferUnicNumber = dr["transfer_unic_number"] != DBNull.Value ? Convert.ToInt32(dr["transfer_unic_number"]) : default(Int32); //Convert.ToInt32(dr["transfer_unic_number"]);
                        problemLoanTaxes.RegistrationSetNumber = dr["registration_set_number"] != DBNull.Value ? Convert.ToInt16(dr["registration_set_number"]) : default(Int16); //Convert.ToInt16(dr["registration_set_number"]);
                        problemLoanTaxes.ConfirmationDate = dr["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dr["confirmation_date"]) : default(DateTime); //Convert.ToDateTime(dr["confirmation_date"]);
                        problemLoanTaxes.ConfirmationSetNumber = dr["confirmation_set_number"] != DBNull.Value ? Convert.ToInt16(dr["confirmation_set_number"]) : default(Int16); //Convert.ToInt16(dr["confirmation_set_number"]);
                        problemLoanTaxes.TaxQuality = (TaxQuality)Convert.ToInt16(dr["tax_quality"]);
                        problemLoanTaxes.TaxCourtDecision = dr["tax_court_decision"] != DBNull.Value ? (TaxCourtDecision)Convert.ToInt16(dr["tax_court_decision"]) : (TaxCourtDecision)(-1);
                        problemLoanTaxes.TaxQualityDescription = Utility.ConvertAnsiToUnicode(dr["quality"].ToString());
                        problemLoanTaxes.ClaimNumber = Convert.ToInt64(dr["claim_number"]);

                        problemLoanTaxesList.Add(problemLoanTaxes);
                    }

                    searchResults.Add(totalCustomersCount, problemLoanTaxesList);
                }

                return searchResults;
            }
        }
    }
}
