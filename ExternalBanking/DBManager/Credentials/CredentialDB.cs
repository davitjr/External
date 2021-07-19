using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class CredentialDB
    {

        /// <summary>
        /// GET CUSTOMER CREDENTIALS LIST
        /// </summary>
        internal static List<Credential> GetCustomerCredentialsList(ulong customerNumber)
        {
            List<Credential> credentialList = new List<Credential>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT A.assign_Id, A.assign_number, A.beginning_date, A.end_date, A.assign_type, A.[status],
                                                                                                      T.[description], A.given_by_bank, CRD.credentialGivenDate, CRD.notary, CRD.notaryTerritory,
			                                                                                          CRD.ledgerNumber, CRD.translationValidationDate, CRD.translationOfNotary, CRD.translationOfNotaryTerritory,
			                                                                                          CRD.translationValidationLedgerNumber
                                                                                      FROM Tbl_assigns A 
                                                                                      INNER JOIN Tbl_type_of_assigns T ON A.Assign_type = T.id
                                                                                      LEFT JOIN [HBBase].[dbo].Tbl_credential_order_details CRD ON a.order_id = crd.order_id
                                                                                      WHERE A.Customer_Number = @customerNumber Order By Beginning_Date Desc", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    credentialList = new List<Credential>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Credential oneCredential = SetCredential(row);

                    credentialList.Add(oneCredential);
                }

            }

            return credentialList;
        }

        private static Credential SetCredential(DataRow row)
        {
            Credential oneCredential = new Credential();

            if (row != null)
            {
                oneCredential.Id = ulong.Parse(row["assign_Id"].ToString());
                oneCredential.CredentialNumber = row["assign_number"].ToString();
                oneCredential.StartDate = DateTime.Parse(row["beginning_date"].ToString());
                oneCredential.EndDate = row["end_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["end_date"].ToString());
                oneCredential.CredentialType = ushort.Parse(row["assign_type"].ToString());
                oneCredential.Status = ushort.Parse(row["status"].ToString());
                oneCredential.CredentialTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                oneCredential.AssigneeList = AssigneeDB.GetCredentialAssigneeList(oneCredential.Id);
                if (row["given_by_bank"] != DBNull.Value)
                    oneCredential.GivenByBank = bool.Parse(row["given_by_bank"].ToString());
                if (row["credentialGivenDate"] != DBNull.Value)
                    oneCredential.CredentialGivenDate = Convert.ToDateTime(row["credentialGivenDate"].ToString());
                if (row["notary"] != DBNull.Value)
                    oneCredential.Notary = Utility.ConvertAnsiToUnicode(row["notary"].ToString());
                if (row["notaryTerritory"] != DBNull.Value)
                    oneCredential.NotaryTerritory = Utility.ConvertAnsiToUnicode(row["notaryTerritory"].ToString());
                if (row["ledgerNumber"] != DBNull.Value)
                    oneCredential.LedgerNumber = Utility.ConvertAnsiToUnicode(row["ledgerNumber"].ToString());
                if (row["translationValidationDate"] != DBNull.Value)
                    oneCredential.TranslationValidationDate = Convert.ToDateTime(row["translationValidationDate"].ToString());
                if (row["translationOfNotary"] != DBNull.Value)
                    oneCredential.TranslationOfNotary = Utility.ConvertAnsiToUnicode(row["translationOfNotary"].ToString());
                if (row["translationOfNotaryTerritory"] != DBNull.Value)
                    oneCredential.TranslationOfNotaryTerritory = Utility.ConvertAnsiToUnicode(row["translationOfNotaryTerritory"].ToString());
                if (row["translationValidationLedgerNumber"] != DBNull.Value)
                    oneCredential.TranslationValidationLedgerNumber = Utility.ConvertAnsiToUnicode(row["translationValidationLedgerNumber"].ToString());
            }
            return oneCredential;
        }

        /// <summary>
        /// GET CUSTOMER CLOSED CREDENTIALS LIST
        /// </summary>
        internal static List<Credential> GetCustomerClosedCredentialsList(ulong customerNumber)
        {
            List<Credential> credentialList = new List<Credential>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT A.assign_Id, A.assign_number, A.beginning_date, A.end_date, A.assign_type, A.closing_date, A.closing_reason, 
                                                A.closing_set_number, T.description,A.given_by_bank
                                                FROM Tbl_assigns_history A 
                                                INNER JOIN Tbl_type_of_assigns T ON A.Assign_type = T.id
                                                WHERE isnull(closing_reason,0)<>0 AND Customer_Number = @customerNumber Order By Beginning_Date Desc", conn);

                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    credentialList = new List<Credential>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Credential oneCredential = new Credential();

                    if (row != null)
                    {
                        oneCredential.Id = ulong.Parse(row["assign_Id"].ToString());
                        oneCredential.CredentialNumber = row["assign_number"].ToString();
                        oneCredential.StartDate = row["beginning_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["beginning_date"].ToString());
                        oneCredential.EndDate = row["end_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["end_date"].ToString());
                        oneCredential.CredentialType = ushort.Parse(row["assign_type"].ToString());
                        oneCredential.CredentialTypeDescription = Utility.ConvertAnsiToUnicode(row["description"].ToString());
                        oneCredential.ClosingDate = row["closing_date"] == DBNull.Value ? default(DateTime?) : DateTime.Parse(row["closing_date"].ToString());
                        oneCredential.ClosingReason = Utility.ConvertAnsiToUnicode(row["closing_reason"].ToString());
                        oneCredential.ClosingSetNumber = int.Parse(row["closing_set_number"].ToString());
                        oneCredential.AssigneeList = AssigneeDB.GetCredentialAssigneeList(oneCredential.Id);
                    }

                    credentialList.Add(oneCredential);
                }

            }

            return credentialList;
        }

        internal static int GetCredentialDocId(ulong credentialId)
        {
            int docID = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select BO.document_id from Tbl_assigns A inner join [HBBase].[dbo].Tbl_Link_HB_document_order BO
                                                  on A.order_id = BO.order_id where A.assign_id = @assignId", conn);

                cmd.Parameters.Add("@assignId", SqlDbType.Float).Value = credentialId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    docID = Convert.ToInt32(dt.Rows[0]["document_id"].ToString());

            }
            return docID;
        }


        internal static DataTable GetAssigneeIdentificationOrderByDocId(long docId)
        {
            //ulong credentialId = 0;
            DataTable dt = new DataTable();


            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select A.assign_id, D.* from Tbl_assigns A inner join TBL_Assignees Ass on A.assign_id = Ass.assign_id
                                                  inner join dbo.TBl_Assignee_Identification D on Ass.id = D.assignee_id 
                                                  where D.doc_id = @docId ", conn);

                cmd.Parameters.Add("@docId", SqlDbType.Float).Value = docId;


                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }

                //if (dt.Rows.Count > 0)
                //    credentialId = Convert.ToUInt64(dt.Rows[0]["assign_id"].ToString());

            }
            return dt;
        }
    }
}
