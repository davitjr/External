using ExternalBanking.QrTransfers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.QrTransfers
{
    public class QrTransfersDB
    {

        internal static string GetAccountQrCodeGuid(string accountNumber)
        {
            string guid = string.Empty;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand("pr_Get_Account_Qr_Code_Guid", conn);
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@account_Number", SqlDbType.NVarChar).Value = accountNumber;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    guid = Convert.ToString(dr["guid"]);
                }
            }
            return guid;
        }

        internal static QrTransfer SearchAccountByQrCode(string guid)
        {
            QrTransfer qrTransfer = new QrTransfer();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using SqlCommand cmd = new SqlCommand("pr_Search_Account_By_Qr_Code", conn);
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@guid", SqlDbType.NVarChar).Value = guid;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    qrTransfer.AccountNumber = Convert.ToString(dr["accountNumber"]);
                    qrTransfer.CustomerNumber = ulong.Parse(dr["customerNumber"].ToString());
                }
            }
            return qrTransfer;
        }

        internal static ActionResult SaveAccountQrCode(string accountNumber, string guid, ulong customerNumber)
        {
            ActionResult actionResult = new ActionResult();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("pr_Save_Account_Qr_Code", conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@account_Number", SqlDbType.NVarChar).Value = accountNumber;
                    cmd.Parameters.Add("@guid", SqlDbType.NVarChar).Value = guid;
                    cmd.Parameters.Add("@customer_Number", SqlDbType.Float).Value = customerNumber;

                    cmd.ExecuteNonQuery();
                }
                actionResult.ResultCode = ResultCode.Normal;
            }
            return actionResult;
        }

    }
}
