using ExternalBanking.XBManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class ApprovementSchemaDetailsDB
    {
        internal static ActionResult Save(ApprovementSchemaDetails schemaDetails, int schemaId, long orderId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "pr_submit_approvement_schema_details";
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@group_id", SqlDbType.Int).Value = schemaDetails.Group.Id;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add("@step_description", SqlDbType.NVarChar, 255).Value = schemaDetails.Step.Description;
                    cmd.Parameters.Add("@step_order", SqlDbType.TinyInt).Value = schemaDetails.Order;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = Action.Add;

                    SqlParameter param = new SqlParameter("@Id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    int id = int.Parse(cmd.Parameters["@Id"].Value.ToString());

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                        result.Id = id;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                        result.Id = -1;
                    }

                }
            }

            return result;
        }

        internal static List<ApprovementSchemaDetails> GetApprovementSchemaDetails(int schemaId)
        {
            List<ApprovementSchemaDetails> schemaDetailsList = new List<ApprovementSchemaDetails>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                string script = @"SELECT * FROM tbl_approvement_schema_details WHERE schema_id = @schemaId Order by step_order asc";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@schemaId", SqlDbType.Int).Value = schemaId;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];

                        ApprovementSchemaDetails schemaDetails = new ApprovementSchemaDetails();
                        schemaDetails.Id = Convert.ToInt32(row["id"]);
                        schemaDetails.Order = Convert.ToByte(row["step_order"]);
                        schemaDetails.Group = new XBUserGroup();
                        schemaDetails.Step = new ApprovementSchemaStep();
                        schemaDetails.Group.Id = Convert.ToInt32(row["group_id"]);
                        schemaDetails.Step.Id = Convert.ToInt32(row["step_id"]);

                        schemaDetailsList.Add(schemaDetails);
                    }
                }
            }

            return schemaDetailsList;
        }

        internal static ApprovementSchemaDetails GetApprovementSchemaDetailsById(int schemaDetailsId)
        {
            ApprovementSchemaDetails schemaDetails = new ApprovementSchemaDetails();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                string script = @"SELECT id,step_order,group_id,step_id FROM tbl_approvement_schema_details WHERE id = @id ";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = schemaDetailsId;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];


                        schemaDetails.Id = Convert.ToInt32(row["id"]);
                        schemaDetails.Order = Convert.ToByte(row["step_order"]);
                        schemaDetails.Group = new XBUserGroup();
                        schemaDetails.Step = new ApprovementSchemaStep();
                        schemaDetails.Group.Id = Convert.ToInt32(row["group_id"]);
                        schemaDetails.Step.Id = Convert.ToInt32(row["step_id"]);


                    }
                }
            }

            return schemaDetails;
        }
    }
}
