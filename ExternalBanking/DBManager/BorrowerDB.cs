using ExternalBanking.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking
{
    public class BorrowerDB
    {
        public static List<Borrower> GetLoanBorrowers(ulong productId)
        {
            var borrowers = new List<Borrower>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using SqlCommand cmd = new SqlCommand(@"SELECT customer_number,fullName,Agreement.Id,AgreementExistence FROM 
                                                        (SELECT C.customer_number,[name] + ' ' + lastName AS fullName
                                                        FROM V_U_CustomerDescription C WHERE C.customer_number IN (SELECT customer_number FROM tbl_loan_applications WHERE app_id = @App_id 
                                                                                                                   UNION
																												   SELECT link_customer_number FROM Tbl_loan_joint_debitors WHERE loan_app_id = @App_id)
                                                        ) Borrowers
                                                        LEFT JOIN Tbl_Tax_Refund_Agreements  Agreement 
                                                        ON Agreement.LoanAppId = @App_id AND Agreement.CustomerNumber = Borrowers.customer_number", conn);
                conn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@App_id", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    borrowers.Add(new Borrower
                    {
                        CustomerNumber = Convert.ToUInt64(dr["customer_number"]),
                        FullName = dr["fullName"].ToString(),
                        AgreementId = dr["Id"] != DBNull.Value ? Convert.ToUInt64(dr["Id"]) : default,
                        AgreementExistence = dr["AgreementExistence"] != DBNull.Value && Convert.ToBoolean(dr["AgreementExistence"])
                    });
                }
            }
            return borrowers;
        }

        public static ActionResult SaveTaxRefundAgreementDetails(ulong customerNumber, ulong productId, byte agreementExistence, int setNumber)
        {
            var result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand();
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText = "pr_Borrower_Agreement_Registration";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@CustomerNumber", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@LoanAppId", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@Action", SqlDbType.Bit).Value = Convert.ToBoolean(agreementExistence);
                cmd.Parameters.Add("@SetNumber", SqlDbType.SmallInt).Value = setNumber;

                cmd.ExecuteNonQuery();

                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }
        public static List<ChangeDetails> GetTaxRefundAgreementHistory(int agreementId)
        {
            var history = new List<ChangeDetails>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using SqlCommand cmd = new SqlCommand(@"SELECT ChangeDate,ChangeSetId,CASE WHEN [Action] = 1 THEN N'Նշումը կատարված է' ELSE N'Նշումը հանված է' END AS ActionDescription 
                                                        FROM Tbl_Tax_Refund_Agreements_History
                                                        WHERE AgreementId = @AgreementId
                                                        ORDER BY ID DESC", conn);
                conn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@AgreementId", SqlDbType.Float).Value = agreementId;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    history.Add(new ChangeDetails
                    {
                        Date = Convert.ToDateTime(dr["ChangeDate"].ToString()),
                        SetNumber = Convert.ToInt32(dr["ChangeSetId"].ToString()),
                        Description = dr["ActionDescription"].ToString()
                    });
                }
            }
            return history;
        }
    }
}