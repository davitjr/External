using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using ExternalBanking.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DocFlowManagement
{
    public class MemoDocument
    {

        public int MemoId { get; set; }

        public int MemoType { get; set; }

        public List<MemoField> MemoFields { get; set; }

        public ActionResult SaveAndSend(User user, int memoType,int filialCode)
        {
            int memoId = MemoDocumentDB.Save(this, user, memoType,filialCode);
            var result = MemoDocumentDB.Send(Convert.ToInt32(memoId), user.userID);
            result.Id = memoId;
            return result;
        }


        public ActionResult Send(int memoId, int hbDocId)
        {
            return MemoDocumentDB.Send(memoId,hbDocId);
        }

        public ActionResult LinkHBToDocFlow(int memoId, int hbDocId)
        {
            return new ActionResult();
        }

        public static List<MemoField> GetMemoTemplate(int memoType)
        {
            List<MemoField> memoFields = new List<MemoField>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocFlowConn"].ToString()))
            {
               using SqlCommand cmd = new SqlCommand("Sp_Get_MemoDocument_Template", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter param = cmd.Parameters.Add(new SqlParameter("@memo_type", SqlDbType.Int));
                param.Direction = ParameterDirection.Input;
                param.Value = memoType;
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    dt.Load(dr);
                }
                foreach (DataRow row in dt.Rows)
                {
                    MemoField memoField = new MemoField
                    {
                        PositionId = Convert.ToInt32(row["position_id"].ToString()),
                        InputType = Convert.ToInt32(row["input_type"].ToString()),
                        ControlType = Convert.ToInt32(row["control_type"].ToString()),
                        ControlName = row["control_name"].ToString(),
                        FieldValue = row["field_value"].ToString(),
                        //FieldValueType = Convert.ToInt32(row["field_value_type"].ToString()),
                        ParametrName = row["parameter_name"].ToString()
                    };            
                    memoFields.Add(memoField);
                }
            }
            return memoFields;
        }
    }
}
