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
    internal static class OrderRejectionDB
    {
        internal static ActionResult Save(OrderRejection rejection, Languages language)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_Reject_Document"; 
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = rejection.CustomerNumber; 
                    cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = rejection.OrderId;
                    cmd.Parameters.Add("@reject_reason", SqlDbType.NVarChar, 255).Value = rejection.RejectReason;
                    cmd.Parameters.Add("@user_name", SqlDbType.NVarChar, 30).Value = rejection.UserName;
                    cmd.Parameters.Add("@lang_id", SqlDbType.SmallInt).Value = language == Languages.hy ? 0 : 1;
                  

                    SqlParameter param = new SqlParameter("@msg", SqlDbType.NVarChar, int.MaxValue);
                    param.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    SqlParameter param1 = new SqlParameter("@result", SqlDbType.Int);
                    param1.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(param1);

                    cmd.ExecuteNonQuery();
                    int resultValue = Convert.ToInt32(cmd.Parameters["@result"].Value);

                    if (resultValue != 0)
                    {
                        result.ResultCode = ResultCode.Normal;
                    }
                    else
                    {
                        result.ResultCode = ResultCode.ValidationError;
                        result.Errors = new List<ActionError>();
                        ActionError actionError = new ActionError();
                        actionError.Description = cmd.Parameters["@msg"].Value.ToString();
                        result.Errors.Add(actionError);
                    }


                }
            }

            return result;
        }
    }
}
