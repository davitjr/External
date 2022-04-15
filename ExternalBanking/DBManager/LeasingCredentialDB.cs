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
    public class LeasingCredentialDB
    {
        internal static ActionResult SaveAndEditLeasingCredential(LeasingCredential credential, short userID)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "pr_Save_And_Edit_Leasing_Credential";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ActionType", SqlDbType.SmallInt).Value = credential.ActionType;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = credential.Id;
                        cmd.Parameters.Add("@CredentialNumber", SqlDbType.NVarChar).Value = credential.CredentialNumber;
                        cmd.Parameters.Add("@CustomerNumber", SqlDbType.Int).Value = credential.CustomerNumber;
                        cmd.Parameters.Add("@OwnerCustomerNumber", SqlDbType.Int).Value = credential.OwnerCustomerNumber;
                        cmd.Parameters.Add("@CredentialValidationDate", SqlDbType.SmallDateTime).Value = credential.CredentialValidationDate;
                        cmd.Parameters.Add("@CredentialStartDate", SqlDbType.SmallDateTime).Value = credential.StartDate;
                        cmd.Parameters.Add("@CredentialEndDate", SqlDbType.SmallDateTime).Value = credential.EndDate;
                        cmd.Parameters.Add("@HasNotaryAuthorization", SqlDbType.Bit).Value = credential.HasNotarAuthorization;
                        cmd.Parameters.Add("@NotaryTerritory", SqlDbType.NVarChar).Value = credential.NotaryTerritory;
                        cmd.Parameters.Add("@Notary", SqlDbType.NVarChar).Value = credential.Notary;
                        cmd.Parameters.Add("@LedgerNumber", SqlDbType.NVarChar).Value = credential.LedgerNumber;
                        cmd.Parameters.Add("@TranslationValidationDate", SqlDbType.SmallDateTime).Value = credential.TranslationValidationDate;
                        cmd.Parameters.Add("@TranslationOfNotaryTerritory", SqlDbType.NVarChar).Value = credential.TranslationOfNotaryTerritory;
                        cmd.Parameters.Add("@TranslationOfNotary", SqlDbType.NVarChar).Value = credential.TranslationOfNotary;
                        cmd.Parameters.Add("@TranslationValidationLedgerNumber", SqlDbType.NVarChar).Value = credential.TranslationValidationLedgerNumber;
                        cmd.Parameters.Add("@RegistrationDate", SqlDbType.SmallDateTime).Value = credential.RegistrationDate;
                        cmd.Parameters.Add("@SetNumber", SqlDbType.Int).Value = userID;

                        cmd.ExecuteNonQuery();
                        result.ResultCode = ResultCode.Normal;

                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(ex.Message));
            }

            return result;
        }


        internal static List<LeasingCredential> GetLeasingCredentials(int customerNumber, ProductQualityFilter filter)
        {
            List<LeasingCredential> credentials = new List<LeasingCredential>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"SELECT C.*, CR.description FROM [dbo].[Tbl_Credentials] C
                                        LEFT JOIN Tbl_type_of_credential_closing_reason CR ON C.ClosingReasonType = CR.Id WHERE [CustomerNumber] = @number";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = customerNumber;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LeasingCredential credential = new LeasingCredential();
                            credential.Id = Convert.ToInt32(reader["Id"]);
                            credential.CredentialNumber = Convert.ToString(reader["CredentialNumber"]);
                            credential.OwnerCustomerNumber = Convert.ToInt32(reader["OwnerCustomerNumber"]);
                            credential.CredentialValidationDate = Convert.ToDateTime(reader["CredentialValidationDate"]);
                            credential.StartDate = Convert.ToDateTime(reader["CredentialStartDate"]);
                            credential.EndDate = Convert.ToDateTime(reader["CredentialEndDate"]);
                            credential.Status = Convert.ToUInt16(reader["Status"]);

                            if (reader["HasNotaryAuthorization"] != DBNull.Value)
                                credential.HasNotarAuthorization = Convert.ToBoolean(reader["HasNotaryAuthorization"]);

                            credential.NotaryTerritory = Convert.ToString(reader["NotaryTerritory"]);
                            credential.Notary = Convert.ToString(reader["Notary"]);
                            credential.LedgerNumber = Convert.ToString(reader["LedgerNumber"]);
                            credential.TranslationValidationDate = Convert.ToDateTime(reader["TranslationValidationDate"]);
                            credential.TranslationOfNotaryTerritory = Convert.ToString(reader["TranslationOfNotaryTerritory"]);
                            credential.TranslationOfNotary = Convert.ToString(reader["TranslationOfNotary"]);
                            credential.TranslationValidationLedgerNumber = Convert.ToString(reader["TranslationValidationLedgerNumber"]);
                            credential.ClosingReason = Convert.ToString(reader["ClosingReason"]);

                            if (reader["ClosingReasonType"] != DBNull.Value)
                                credential.ClosingReasonType = Convert.ToInt16(reader["ClosingReasonType"]);

                            credential.ClosingReasonTypeDescription = Convert.ToString(reader["description"]);

                            if (reader["ClosingSetNumber"] != DBNull.Value)
                                credential.ClosingSetNumber = Convert.ToInt32(reader["ClosingSetNumber"]);

                            credential.SetNumber = Convert.ToInt32(reader["SetNumber"]);
                            credential.RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]);

                            credentials.Add(credential);
                        }
                    }


                }
            }

            return credentials;
        }

        internal static ActionResult SaveRemovedLeasingCredential(int credentialId, short userID)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "pr_Remove_Or_Close_Leasing_Credential";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ActionType", SqlDbType.SmallInt).Value = 1;
                        cmd.Parameters.Add("@CredentialId", SqlDbType.Int).Value = credentialId;
                        cmd.Parameters.Add("@SetNumber", SqlDbType.Int).Value = userID;

                        cmd.ExecuteNonQuery();
                        result.ResultCode = ResultCode.Normal;

                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(ex.Message));
            }

            return result;
        }

        internal static ActionResult SaveClosedLeasingCredential(LeasingCredential credential, short userID)
        {
            ActionResult result = new ActionResult();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.CommandText = "pr_Remove_Or_Close_Leasing_Credential";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ActionType", SqlDbType.SmallInt).Value = 2;
                        cmd.Parameters.Add("@CredentialId", SqlDbType.Int).Value = credential.Id;
                        cmd.Parameters.Add("@ClosingReason", SqlDbType.NVarChar).Value = credential.ClosingReason;
                        cmd.Parameters.Add("@ClosingReasonType", SqlDbType.Int).Value = credential.ClosingReasonType;
                        cmd.Parameters.Add("@SetNumber", SqlDbType.Int).Value = userID;

                        cmd.ExecuteNonQuery();
                        result.ResultCode = ResultCode.Normal;

                    }
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Failed;
                result.Errors.Add(new ActionError(ex.Message));
            }

            return result;
        }

        internal static Dictionary<string, string> GetLeasingCustomerCredentials(int customerNumber)
        {
            Dictionary<string, string> credential = new Dictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["LeasingAccOperConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT [Id],[CredentialNumber] FROM [dbo].[Tbl_Credentials] WHERE CustomerNumber = @customerNumber";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.SmallInt).Value = customerNumber;

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            credential.Add(Convert.ToString(reader["Id"]), Convert.ToString(reader["CredentialNumber"]));
                        }
                    }
                }
            }

            return credential;
        }


    }
}
