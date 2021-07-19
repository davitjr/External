using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    class ProblemLoanTaxDB
    {
        internal static ProblemLoanTax GetProblemLoanTaxDetails(long ClaimNumber)
        {
           
            ProblemLoanTax problemLoanTax = new ProblemLoanTax();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "pr_get_problem_loan_tax_list";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@claimNumber", SqlDbType.Float).Value = ClaimNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    problemLoanTax.FilialCode = Convert.ToInt16(dt.Rows[0]["filialcode"]);
                    
                    problemLoanTax.CustomerNumber = Convert.ToUInt64(dt.Rows[0]["customer_number"]);
                    problemLoanTax.FullName = Utility.ConvertAnsiToUnicode(dt.Rows[0]["NameFull"].ToString());
                    problemLoanTax.LoanFullNumber = Convert.ToUInt64(dt.Rows[0]["loan_full_number"]);
                    problemLoanTax.AppId = Convert.ToUInt64(dt.Rows[0]["app_id"]);
                    problemLoanTax.LoanType = Convert.ToInt16(dt.Rows[0]["loan_type"]);
                    problemLoanTax.TaxRegistrationDate = Convert.ToDateTime(dt.Rows[0]["tax_registration_date"]);
                    problemLoanTax.TaxRegistrationTime = Convert.ToDateTime(dt.Rows[0]["tax_registration_time"]);
                    problemLoanTax.TaxAmount = Convert.ToDecimal(dt.Rows[0]["tax_amount"]);
                    problemLoanTax.TransferRegistrationDate = dt.Rows[0]["transfer_registration_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["transfer_registration_date"]) : default(DateTime); 
                    problemLoanTax.TransferUnicNumber = dt.Rows[0]["transfer_unic_number"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["transfer_unic_number"]) : default(Int32); 
                    problemLoanTax.RegistrationSetNumber = dt.Rows[0]["registration_set_number"] != DBNull.Value ? Convert.ToInt16(dt.Rows[0]["registration_set_number"]) : default(Int16); 
                    problemLoanTax.ConfirmationDate = dt.Rows[0]["confirmation_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["confirmation_date"]) : default(DateTime); 
                    problemLoanTax.ConfirmationSetNumber = dt.Rows[0]["confirmation_set_number"] != DBNull.Value ? Convert.ToInt16(dt.Rows[0]["confirmation_set_number"]) : default(Int16); 
                    problemLoanTax.TaxQuality = (TaxQuality)Convert.ToInt16(dt.Rows[0]["tax_quality"]);
                    problemLoanTax.TaxCourtDecision = dt.Rows[0]["tax_court_decision"] != DBNull.Value ? (TaxCourtDecision)Convert.ToInt16(dt.Rows[0]["tax_court_decision"]) : (TaxCourtDecision)(-1);
                    problemLoanTax.TaxQualityDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["quality"].ToString());
                    problemLoanTax.LoanTypeDescription = Utility.ConvertAnsiToUnicode(dt.Rows[0]["description"].ToString());
                }
            }
            return problemLoanTax;
        }
    }
}
