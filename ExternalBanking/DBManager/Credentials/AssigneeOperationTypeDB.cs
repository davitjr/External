using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ExternalBanking.DBManager
{
    internal static class AssigneeOperationTypeDB
    {
        internal static AssigneeOperationType GetAssigneeOperationType(uint operationTypeId)
        {
                      
            AssigneeOperationType oneAssigneeOperationType = new AssigneeOperationType();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT * FROM [dbo].[Tbl_type_of_assign_operation] 
                                                                            WHERE id=@operationTypeId", conn);

                cmd.Parameters.Add("@operationTypeId", SqlDbType.Int).Value = operationTypeId;

                DataTable dt = new DataTable();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    oneAssigneeOperationType.Id = uint.Parse(dr["id"].ToString());
                    oneAssigneeOperationType.Description = dr["description"].ToString();
                    oneAssigneeOperationType.GroupId = uint.Parse(dr["groupId"].ToString());
                    oneAssigneeOperationType.ForNaturalPerson = bool.Parse(dr["forNaturalPerson"].ToString());
                    oneAssigneeOperationType.ForPrivateEntrepreneur = bool.Parse(dr["forPrivateEntrepreneur"].ToString());
                    oneAssigneeOperationType.ForLegalEntity = bool.Parse(dr["forLegalEntity"].ToString());

                }

            }                               

            return oneAssigneeOperationType;
        }        
    }
}
