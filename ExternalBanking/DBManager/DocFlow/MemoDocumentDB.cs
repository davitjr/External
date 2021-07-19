using ExternalBanking.DocFlowManagement;
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
    public class MemoDocumentDB
    {
        internal static int Save(MemoDocument memoDocument, ACBAServiceReference.User user, int memoType, int filialCode)
        {

            //ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocFlowConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Sp_Insert_MemoDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@memo_type", SqlDbType.Int).Value = memoType;
                    cmd.Parameters.Add("@filialcode", SqlDbType.Int).Value = filialCode;
                    cmd.Parameters.Add("@registration_set_number", SqlDbType.Int).Value = user.userID;
                    cmd.Parameters.Add("@memo_items", SqlDbType.Structured).Value = DocFlowManagement.Utility.ConvertMemoFieldsToDataTable(memoDocument.MemoFields);
                    cmd.Parameters.Add("@memo_department_owner", SqlDbType.Int).Value = user.DepartmentId;
                    cmd.Parameters.Add(new SqlParameter("@memo_id_from_memo_doc", SqlDbType.Int) { Direction = ParameterDirection.Output });
                                                                                          
                    cmd.ExecuteNonQuery();
                    //result.ResultCode = ResultCode.Normal;
                    int memoId = Convert.ToInt32(cmd.Parameters["@memo_id_from_memo_doc"].Value);
                    return memoId;
                }
            }
        }
        public static ActionResult Send(int memoId,int userId)
        {
            ActionResult result = new ActionResult();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocFlowConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Sp_Send_MemoDocument";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@memo_id", SqlDbType.Int).Value = memoId;
                    cmd.Parameters.Add("@sender_set_number", SqlDbType.Int).Value = userId;
               
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;
                    return result;
                }
            }
        }
    }
}
