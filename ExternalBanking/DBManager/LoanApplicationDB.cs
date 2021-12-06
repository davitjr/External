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
    internal static class LoanApplicationDB
    {
        private static string loanApplicationSelectScript = @"SELECT
	                                                                 [application_number]
                                                                    ,[application_date]
                                                                    ,[loan_type]
                                                                    ,[amount_requested]
                                                                    ,[currency_requested]
                                                                    ,[period_requested]
                                                                    ,[date_of_beginning]
                                                                    ,[status]
                                                                    ,[CardNumber]
                                                                    ,A.app_id
                                                                    ,[customer_number]
                                                                    ,[percent_offered]
                                                                    ,A.[filialcode]
                                                                    ,[use_of_country]
                                                                    ,[use_of_arm_place]
	                                                                ,R.region 
	                                                                ,i.communication_type
                                                                    ,S.Offered_amount
                                                                    ,Case when status = 6 then analyse_rejection_reason  else '' end as rejectionReason
                                                                    ,refuse_reason
                                                                FROM [dbo].[Tbl_loan_applications] A
                                                                LEFT JOIN Tbl_link_application_resume LR on A.app_id = LR.loan_app_id 
                                                                LEFT JOIN Tbl_Loan_SCORE S on LR.resume_app_id = S.resume_app_id 
                                                                LEFT JOIN dbo.tbl_armenian_places r ON A.use_of_arm_place=R.number
                                                                LEFT JOIN [dbo].[Tbl_loan_applications_add_inf] i on i.app_id = A.app_id";

        public static LoanApplication GetLoanApplication(ulong productId, ulong customerNumber)
        {

            LoanApplication loanApplication = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(loanApplicationSelectScript + " WHERE A.app_id=@app_id and A.customer_number=@customer_number", conn);

                cmd.Parameters.Add("@app_id", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                using DataTable dt = new DataTable();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    loanApplication = SetApplication(row);
                }

            }

            return loanApplication;
        }

        public static List<LoanApplication> GetLoanApplications(ulong customerNumber)
        {
            List<LoanApplication> applications = new List<LoanApplication>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();

                string sql = loanApplicationSelectScript + @" WHERE A.customer_number=@customerNumber AND loan_type=54 ORDER BY A.application_date desc ";
                
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                using DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    applications = new List<LoanApplication>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    LoanApplication application = SetApplication(row);

                    applications.Add(application);
                }

            }


            return applications;
        }

        private static LoanApplication SetApplication(DataRow row)
        {
            LoanApplication loanApplication = new LoanApplication();

            if (row != null)
            {
                loanApplication.ProductId = long.Parse(row["app_id"].ToString());
                loanApplication.Quality = short.Parse(row["status"].ToString());
                loanApplication.LoanApplicationNumber = long.Parse(row["application_number"].ToString());
                loanApplication.ApplicationDate = DateTime.Parse(row["application_date"].ToString());
                loanApplication.ProductType = short.Parse(row["loan_type"].ToString());
                loanApplication.Amount = double.Parse(row["amount_requested"].ToString());
                loanApplication.Currency = row["currency_requested"].ToString();

                loanApplication.Duration = int.Parse(row["period_requested"].ToString());

                if (row["date_of_beginning"] != DBNull.Value)
                {
                    loanApplication.StartDate = DateTime.Parse(row["date_of_beginning"].ToString());
                    
                }
                else
                {
                    loanApplication.StartDate = DateTime.Parse(row["application_date"].ToString());
                }

                loanApplication.EndDate = loanApplication.StartDate.AddMonths(loanApplication.Duration);

                if (row["percent_offered"] != DBNull.Value)
                    loanApplication.InterestRateOffered = float.Parse(row["percent_offered"].ToString());

                if (row["communication_type"] != DBNull.Value)
                    loanApplication.CommunicationType = short.Parse(row["communication_type"].ToString());

                if (row["use_of_country"] != DBNull.Value)
                    loanApplication.LoanUseCountry = int.Parse(row["use_of_country"].ToString());

                if (row["region"] != DBNull.Value)
                    loanApplication.LoanUseRegion = int.Parse(row["region"].ToString());

                if (row["use_of_arm_place"] != DBNull.Value)
                    loanApplication.LoanUseLocality = int.Parse(row["use_of_arm_place"].ToString());

                if (row["CardNumber"] != DBNull.Value)
                    loanApplication.CardNumber = row["CardNumber"].ToString();

                if (row["Offered_amount"] != DBNull.Value)
                    loanApplication.OfferedAmount = double.Parse(row["Offered_amount"].ToString());

                if (row["rejectionReason"] != DBNull.Value)
                    loanApplication.RejectReason = Utility.ConvertAnsiToUnicode( row["rejectionReason"].ToString());


                if (row["refuse_Reason"] != DBNull.Value)
                    loanApplication.RefuseReason = Utility.ConvertAnsiToUnicode(row["refuse_Reason"].ToString());

                loanApplication.FilialCode = int.Parse(row["filialcode"].ToString());

            }


            return loanApplication;
        }


        public static List<FicoScoreResult> GetLoanApplicationFicoScoreResults(ulong customerNumber,ulong productId,DateTime requestDate)
        {
            List<FicoScoreResult> results = new List<FicoScoreResult>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();

                using SqlCommand cmd = new SqlCommand("pr_get_customer_fico_score", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@mainCustomer", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@applicationID", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@calcDate", SqlDbType.DateTime).Value = requestDate;

                using DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }
                               
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    FicoScoreResult result = new FicoScoreResult
                    {
                        CustomerNumber = ulong.Parse(row["customerNumber"].ToString()),
                        FicoScore = double.Parse(row["ficoscore"].ToString()),
                        MidFicoScore = double.Parse(row["meanficoscore"].ToString()),
                        RequestDate = DateTime.Parse(row["requestDate"].ToString()),

                        Reasons = new List<string>
                        {
                            Utility.ConvertAnsiToUnicode(row["reasoncode1"].ToString()),
                            Utility.ConvertAnsiToUnicode(row["reasoncode2"].ToString()),
                            Utility.ConvertAnsiToUnicode(row["reasoncode3"].ToString()),
                            Utility.ConvertAnsiToUnicode(row["reasoncode4"].ToString())
                        }
                    };

                    results.Add(result);
                }

            }


            return results;
        }



        public static ulong GetLoanApplicationByDocId(long docId)
        {
            ulong productId = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sql = @"SELECT loan_app_id FROM Tbl_Linked_to_loan_applications
                               WHERE type=5 AND link_app_id=@docid";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@docid", SqlDbType.Float).Value = docId;
                    conn.Open();
                    var temp = cmd.ExecuteScalar();
                    if (temp != null)
                        productId = Convert.ToUInt64(temp);
                }
            }
            return productId;
        }
    }
}
