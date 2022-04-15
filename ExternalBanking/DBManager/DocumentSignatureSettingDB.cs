using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class DocumentSignatureSettingDB
    {

        internal static ActionResult SaveBranchDocumentSignatureSetting(DocumentSignatureSetting setting)
        {
            ActionResult result = new ActionResult();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"INSERT INTO tbl_document_signature
                                       (registration_date,filial_code,signature_type,change_set_number)
                                        VALUES
                                       (@registration_date,@filial_code,@signature_type,@change_set_number)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@registration_date", SqlDbType.SmallDateTime).Value = setting.RegistartionDate;
                    cmd.Parameters.Add("@signature_type", SqlDbType.SmallInt).Value = setting.SignatureType;
                    cmd.Parameters.Add("@change_set_number", SqlDbType.Int).Value = setting.User.userID;
                    cmd.Parameters.Add("@filial_code", SqlDbType.Int).Value = setting.User.filialCode;
                    cmd.ExecuteNonQuery();
                    result.ResultCode = ResultCode.Normal;

                    return result;


                }
            }

        }


        internal static DocumentSignatureSetting GetBranchDocumentSignatureSetting(ushort filialCode)
        {
            DocumentSignatureSetting result = new DocumentSignatureSetting();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = @"select top 1 registration_date,filial_code,signature_type,change_set_number from tbl_document_signature
                                        where filial_code=@filialCode order by ID desc";
                    cmd.Parameters.Add("@filialCode", SqlDbType.Int).Value = filialCode;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            result.RegistartionDate = Convert.ToDateTime(dt.Rows[i]["registration_date"]);
                            result.SignatureType = Convert.ToUInt16(dt.Rows[i]["signature_type"]);
                        }
                    }
                    else
                    {
                        result.SignatureType = 2;
                    }
                }
            }

            return result;
        }

    }
}
