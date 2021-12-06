using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    class ClaimDB
    {
        internal static List<Claim> GetProductClaims(ulong productId)
        {
            DataTable dt = new DataTable();
            List<Claim> claims = new List<Claim>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select * from Tbl_problem_loan_claims  Where app_id=@productId", conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Claim claim = new Claim();
                        claim.ClaimNumber = int.Parse(row["claim_number"].ToString());
                        claim.ProductId = ulong.Parse(row["app_id"].ToString());
                        claim.SetNumber = int.Parse(row["claim_set_number"].ToString());
                        claim.ClaimDate = DateTime.Parse(row["claim_date"].ToString());
                        claim.Quality = short.Parse(row["claim_quality"].ToString());
                        claim.Purpose = short.Parse(row["claim_purpose"].ToString());
                        claim.Events = ClaimEvent.GetClaimEvents(claim.ClaimNumber);

                        claims.Add(claim);
                    }
                }

            }
            return claims;
        }



        internal static bool CheckProductHasClaim(ulong loanProductId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"SELECT C.app_id  FROM Tbl_problem_loan_claims C
						                               INNER JOIN Tbl_problem_loan_taxes T ON c.claim_number=t.claim_number 
                                                      WHERE C.app_id=@productApp_ID AND tax_quality in (0,11,12)", conn);
                cmd.Parameters.Add("@productApp_ID", SqlDbType.Float).Value = loanProductId;
                if (cmd.ExecuteReader().Read())
                {
                    check = true;
                }
                

            }
            return check;
        }


        internal static bool IsAparikTexumClaim(ulong loanProductId)
        {
            bool check = false;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
               using SqlCommand cmd = new SqlCommand(@"
													  SELECT p.App_Id FROM Tbl_problem_loan_claims C
						                              INNER JOIN Tbl_problem_loan_taxes T 
													  ON c.claim_number=t.claim_number 
													  INNER JOIN Tbl_paid_factoring p
													  ON p.App_Id=c.app_id
                                                      WHERE t.app_id=@productApp_ID AND p.loan_type=38
                                                ", conn);
                cmd.Parameters.Add("@productApp_ID", SqlDbType.Float).Value = loanProductId;
                if (cmd.ExecuteReader().Read())
                {
                    check = true;
                }


            }
            return check;
        }

        internal static ActionResult ChangeProblemLoanTaxQuality(ulong taxAppId, int setNumber)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_change_problem_loan_tax_quality";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@taxAppID", SqlDbType.BigInt).Value = taxAppId;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;
                    SqlParameter param = new SqlParameter("@errorTermid", SqlDbType.SmallInt)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                    ushort errorTermId = Convert.ToUInt16(cmd.Parameters["@errorTermid"].Value);
                    if (errorTermId != 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Errors = new List<ActionError>
                        {
                            new ActionError((short)errorTermId)
                        };
                    }
                    else
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                }
                return result;
            }
        }


        }
}
