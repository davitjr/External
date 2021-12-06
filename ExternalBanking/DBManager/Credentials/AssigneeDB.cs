using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class AssigneeDB
    {       

        /// <summary>
        /// GET CREDENTIAL ASIGNEE LIST
        /// </summary>
        internal static List<Assignee> GetCredentialAssigneeList(ulong credentialId)
        {
            List<Assignee> assigneeList = new List<Assignee>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT A.id, A.assignee_CustNumber, A.signature_type, A.isemployee, isnull(F.unicode_name,'') as Name, isnull(F.unicode_Lastname,'') as LastName, 
                                                isnull(F.unicode_middleName,'') as MiddleName, D.document_number, D.document_type
                                                FROM Tbl_assignees A
                                                LEFT JOIN tbl_Customers C ON A.assignee_CustNumber = C.Customer_number
												LEFT JOIN Tbl_Persons P On C.IdentityId = P.identityId 
                                                LEFT JOIN V_FullNames F On F.id = p.fullNameId
                                                LEFT JOIN tbl_Customer_documents_Current D ON D.is_default = 1 AND C.identityid = D.identityId
                                                WHERE A.assign_id = @credentialId", conn);

                cmd.Parameters.Add("@credentialId", SqlDbType.Float).Value = credentialId;

                DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                    assigneeList = new List<Assignee>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    Assignee oneAssignee = SetAssignee(row);

                    assigneeList.Add(oneAssignee);
                }

            }                        

            return assigneeList;
        }

        private static Assignee SetAssignee(DataRow row)
        {
            Assignee oneAssignee = new Assignee();

            if (row != null)
            {
                oneAssignee.Id = uint.Parse(row["id"].ToString());
                oneAssignee.CustomerNumber = row["assignee_CustNumber"] != null ? ulong.Parse(row["assignee_CustNumber"].ToString()) : 0;
                oneAssignee.SignatureType = ushort.Parse(row["signature_type"].ToString());
                oneAssignee.IsEmployee = ushort.Parse(row["isemployee"].ToString());
                oneAssignee.AssigneeFirstName =  row["Name"].ToString() ;
                oneAssignee.AssigneeLastName = row["LastName"].ToString() ;
                oneAssignee.AssigneeMiddleName = row["MiddleName"].ToString();                   
                oneAssignee.AssigneeDocumentNumber = row["document_number"].ToString();
                
                oneAssignee.AssigneeDocumentType = (row["document_type"] != null && row["document_type"].ToString() != String.Empty) ? int.Parse(row["document_type"].ToString()) : 0;
                oneAssignee.OperationList = AssigneeOperationDB.GetAssigneeOperationsList(oneAssignee.Id);
            }
            return oneAssignee;
        }

        
    }
}
