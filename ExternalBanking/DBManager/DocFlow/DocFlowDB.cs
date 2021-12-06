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
    public class DocFlowDB
    {
        internal static ActionResult SendHBApplicationOrderToConfirm(long hbId, long memoId)
        {
            ActionResult result = new ActionResult();
            
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_link_hb_docflow (hb_doc_id,memo_id)    VALUES(@hbId,@memoId) ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@hbId", SqlDbType.BigInt).Value = hbId;
                cmd.Parameters.Add("@memoId", SqlDbType.BigInt).Value = memoId;
                cmd.ExecuteReader();
                result.ResultCode = ResultCode.SaveAndSendToConfirm;
                return result;
            }
        }

        internal static ActionResult SendToConfirm(long Id, long memoId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_link_to_docflow (doc_id,memo_id)    VALUES(@Id,@memoId) ", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = Id;
                cmd.Parameters.Add("@memoId", SqlDbType.BigInt).Value = memoId;
                cmd.ExecuteReader();
                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }

        internal static ActionResult SaveUploadedFiles(OrderAttachment order,long memoId)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DocFlowConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"INSERT INTO Tbl_scaned_documents(image_content,image_date,registration_date,unic_number,document_type,[File_Name],set_number)
                                                  VALUES(@upload_content,GETDATE(),GETDATE(),@memo_id,2,@UploadFileName, @registration_set_number)", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@upload_content", SqlDbType.VarBinary).Value = Convert.FromBase64String( order.AttachmentInBase64);
                cmd.Parameters.Add("@memo_id", SqlDbType.BigInt).Value = memoId;
                cmd.Parameters.Add("@UploadFileName", SqlDbType.NVarChar).Value = order.FileName + order.FileExtension;
                cmd.Parameters.Add("@registration_set_number", SqlDbType.Int).Value = 88;
                cmd.ExecuteReader();
                result.ResultCode = ResultCode.Normal;
                return result;
            }
        }
    }
}
