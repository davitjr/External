using ExternalBanking.XBManagement;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    class ApprovementSchemaDB
    {
        internal static ActionResult Save(ApprovementSchema schema, ulong customerNumber, Action action, long orderId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "pr_add_approvement_schema";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;

                    cmd.Parameters.Add("@schema_description", SqlDbType.NVarChar, 255).Value = schema.Description;

                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = action;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;


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

        internal static ApprovementSchema GetApprovementSchema(ulong customerNumber)
        {
            ApprovementSchema schema = new ApprovementSchema();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();

                string script = @"SELECT Id,schema_description FROM tbl_approvement_schema WHERE customer_number = @customerNumber";

                using (SqlCommand cmd = new SqlCommand(script, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        schema.Id = Convert.ToInt32(dt.Rows[0]["Id"]);
                        schema.Description = dt.Rows[0]["schema_description"].ToString();
                    }
                }
            }

            return schema;
        }

        internal static bool ExistsApprovementSchema(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT id FROM tbl_approvement_schema WHERE customer_number = @customer_number", conn))
                {
                    cmd.Parameters.Add("@customer_number", SqlDbType.BigInt).Value = customerNumber;
                    using SqlDataReader dr = cmd.ExecuteReader();
                    return dr.HasRows;
                }
            }

        }

        internal static ActionResult RemoveApprovementSchemaDetails(ApprovementSchemaDetails schemaDetails, long orderId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Remove_Approvement_Schema_Details";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@schema_details_id", SqlDbType.Int).Value = schemaDetails.Id;
                    cmd.Parameters.Add("@action", SqlDbType.TinyInt).Value = Action.Delete;
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = orderId;
                    cmd.Parameters.Add(new SqlParameter("@result", SqlDbType.SmallInt) { Direction = ParameterDirection.Output });


                    cmd.ExecuteNonQuery();

                    byte actionResult = Convert.ToByte(cmd.Parameters["@result"].Value);

                    if (actionResult == 1)
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    else if (actionResult == 0)
                    {
                        result.ResultCode = ResultCode.Failed;
                    }
                }
            }


            return result;
        }
    }
}
