using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.Acbamat
{
    public abstract class AcbamatSharedDB
    {
        internal static string GetAcbamatAccountNumber(string atmId, string currency)
        {
            using SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString());
            using SqlCommand cmd = new SqlCommand(@"pr_find_acbamat_account_number ", conn);
            conn.Open();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@atmId", atmId);
            cmd.Parameters.AddWithValue("@currency", currency);
            SqlParameter param = new SqlParameter("@account_number", SqlDbType.Float)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(param);

            cmd.ExecuteNonQuery();

            return cmd.Parameters["@account_number"].Value.ToString();
        }
    }
}
