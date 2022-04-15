using ExternalBanking.ACBAServiceReference;
using ExternalBanking.XBManagement;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class PhoneBankingContractDB
    {
        /// <summary>
        /// Վերադարձնում է տրված հաճախորդի Հեռախոսային Բանկինգի պայմանագիրը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        internal static PhoneBankingContract Get(ulong customerNumber)
        {
            PhoneBankingContract phoneBankingContract = new PhoneBankingContract();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT  p.*,s.description as quality_description  FROM Tbl_PhoneBanking_Contracts p
                                       INNER JOIN  [dbo].[Tbl_application_statuses] s
                                       ON p.application_status=s.app_status						
                                       WHERE p.customer_number = @customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.NVarChar, 50).Value = customerNumber.ToString();

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    phoneBankingContract = SetPhoneBankingContract(row);
                }
                else
                {
                    phoneBankingContract = null;
                }

            }
            return phoneBankingContract;
        }

        /// <summary>
        /// Հեռախոսային Բանկինգի պայմանագրի ինիցիալիզացում
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static PhoneBankingContract SetPhoneBankingContract(DataRow row)
        {
            PhoneBankingContract phoneBankingContract = new PhoneBankingContract();

            if (row != null)
            {
                phoneBankingContract.Id = Convert.ToInt32(row["ID"].ToString());
                phoneBankingContract.CustomerNumber = Convert.ToUInt64(row["customer_number"].ToString());
                phoneBankingContract.ContractNumber = row["contract_number"].ToString();
                phoneBankingContract.FilialCode = Convert.ToInt32(row["filial"].ToString());
                phoneBankingContract.ApplicationStatus = Convert.ToByte(row["application_status"].ToString());
                phoneBankingContract.StatusDescription = Utility.ConvertAnsiToUnicode(row["quality_description"].ToString());
                phoneBankingContract.SetID = Convert.ToInt32(row["set_id"].ToString());

                phoneBankingContract.SetName = Utility.ConvertAnsiToUnicode(Utility.GetUserFullName(phoneBankingContract.SetID));

                phoneBankingContract.StatusChangeSetID = row["status_change_set_id"] != DBNull.Value ? Convert.ToInt32(row["status_change_set_id"].ToString()) : default(int);
                phoneBankingContract.ContractDate = row["contract_date"] != DBNull.Value ? Convert.ToDateTime(row["contract_date"]) : default(DateTime?);
                phoneBankingContract.ApplicationDate = row["application_date"] != DBNull.Value ? Convert.ToDateTime(row["application_date"]) : default(DateTime?);
                phoneBankingContract.StatusChangeDate = row["status_change_date"] != DBNull.Value ? Convert.ToDateTime(row["status_change_date"]) : default(DateTime?);
                phoneBankingContract.DayLimitToOwnAccount = !row.IsNull("limit_of_day_own_account") ? Convert.ToDouble(row["limit_of_day_own_account"].ToString()) : 0;
                phoneBankingContract.DayLimitToAnothersAccount = !row.IsNull("limit_of_day_other_account") ? Convert.ToDouble(row["limit_of_day_other_account"].ToString()) : 0;
                phoneBankingContract.OneTransactionLimitToOwnAccount = !row.IsNull("limit_of_one_transaction_own_account") ? Convert.ToDouble(row["limit_of_one_transaction_own_account"].ToString()) : 0;
                phoneBankingContract.OneTransactionLimitToAnothersAccount = !row.IsNull("limit_of_one_transaction_other_account") ? Convert.ToDouble(row["limit_of_one_transaction_other_account"].ToString()) : 0;

            }
            return phoneBankingContract;
        }

        internal static DataTable GetQuestionAnswers(int phoneBankingContractId)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                string sql = @"SELECT A.Question_ID, A.Question_Answer, Q.Question
                               FROM Tbl_pBanking_Sec_Answers A
                               INNER JOIN Tbl_pBanking_Sec_Questions Q ON A.Question_ID = Q.ID
                               WHERE Client_ID = @contractId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@contractId", SqlDbType.Int).Value = phoneBankingContractId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                }

            }
            return dt;
        }

        internal static CustomerEmail GetPhoneBankingContractEmail(ulong customerNumber)
        {

            DataTable dt = new DataTable();
            CustomerEmail email = new CustomerEmail();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT    CE.emailAddress, CE.emailId, CE.id
                               FROM      Tbl_Customers C INNER JOIN TBL_Communications_by_email AS E ON C.identityId = E.identityId  LEFT JOIN                         
                                         V_CustomerEmails AS CE ON E.CustomerEmailId = CE.id
							   WHERE     E.CommunicationTypeId = 1 AND C.customer_number = @customerNumber";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                        if (dt.Rows.Count > 0)
                        {
                            email.email = new Email();
                            email.email.emailAddress = dt.Rows[0]["emailAddress"].ToString();
                            email.id = Convert.ToUInt32(dt.Rows[0]["id"].ToString());
                            email.email.id = Convert.ToUInt32(dt.Rows[0]["emailId"].ToString());
                        }
                    }
                }

            }
            return email;

        }

        internal static CustomerPhone GetPhoneBankingContractPhone(ulong customerNumber)
        {

            DataTable dt = new DataTable();
            CustomerPhone phone = new CustomerPhone();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT CP.id as id, CP.phoneId as phoneId, countryCode,areaCode,phoneNumber 
                              FROM Tbl_Communications_By_Phone CBP inner join Tbl_Customer_Phones CP on CBP.CustomerPhoneId = CP.id INNER JOIN TBl_Customers C ON CBP.identityId = C.identityId INNER JOIN Tbl_Phones P ON CP.phoneId = P.id
                              WHERE C.customer_number =  @customerNumber and CBP.CommunicationTypeId = 2";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                        if (dt.Rows.Count > 0)
                        {
                            phone.phone = new Phone();
                            phone.phone.countryCode = dt.Rows[0]["countryCode"].ToString();
                            phone.phone.areaCode = dt.Rows[0]["areaCode"].ToString();
                            phone.phone.phoneNumber = dt.Rows[0]["phoneNumber"].ToString();
                            phone.id = Convert.ToUInt32(dt.Rows[0]["id"].ToString());
                            phone.phone.id = Convert.ToUInt32(dt.Rows[0]["phoneId"].ToString());

                        }
                    }
                }

            }
            return phone;

        }

        internal static double GetPBServiceFee(ulong customerNumber, DateTime date, HBServiceFeeRequestTypes requestType)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = "SELECT dbo.[fn_get_ACBA_OnLine_service_Fee](@customerNumber,@date,@requestType,default,default) as fee";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@date", SqlDbType.SmallDateTime).Value = date;
                    cmd.Parameters.Add("@requestType", SqlDbType.SmallInt).Value = requestType;

                    return Convert.ToDouble(cmd.ExecuteScalar());
                }

            }

        }

        /// <summary>
        /// Ստուգվում է գոյություն ունի դեռրևս չակտիվացված հեռախոսային բանկինգի ծառայություն 
        /// </summary>
        internal static bool isExistsNotConfirmedPBOrder(ulong customerNumber)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"IF EXISTS(SELECT 1 FROM tbl_hb_documents WHERE document_Type = 166 and quality = 3 AND customer_number = @customer_number ) 
                                        SELECT 1 result ELSE SELECT 0 result";

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;

                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }
    }
}
