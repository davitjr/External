using ExternalBanking.XBManagement;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class ApprovementSchemaStepDB
    {
        internal static ActionResult Save(ApprovementSchemaStep step, int schemaId, ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_submit_approvement_step";
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.Add("@schema_id", SqlDbType.Int).Value = schemaId;
                    cmd.Parameters.Add("@step_description", SqlDbType.NVarChar, 255).Value = step.Description;
                    cmd.Parameters.Add("@actionSetNumber", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@step_id", SqlDbType.Int).Value = step.Id;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = step.Id == 0 ? Action.Add : Action.Update;

                    SqlParameter param = new SqlParameter("@id", SqlDbType.Int);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    int id = Convert.ToInt32(cmd.Parameters["@id"].Value);

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

        internal static ApprovementSchemaStep GetApprovementSchemaStep(int id)
        {
            ApprovementSchemaStep step = new ApprovementSchemaStep();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                string script = @"SELECT id,step_description FROM Tbl_Steps WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        step.Id = Convert.ToInt32(row["id"]);
                        step.Description = row["step_description"].ToString();

                    }
                }
            }

            return step;
        }
    }
}
