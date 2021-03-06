using ExternalBanking.SberTransfers.Models;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager.SberTransfers
{
    public static class SberTransfersDB
    {
        internal static SberPreTransferRequisites GetDataForSberTransfer(ulong customerNumber, bool onlyAMD)
        {
            SberPreTransferRequisites result = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "[pr_get_card_data_for_sber_transfer]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@customerNumber", SqlDbType.BigInt).Value = customerNumber;
                    cmd.Parameters.Add("@onlyAMD", SqlDbType.Bit).Value = onlyAMD;

                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            result = new SberPreTransferRequisites
                            {
                                CreditCard = reader["visaNumber"].ToString(),
                                FirstName = reader["firstName"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                CreditCurrency = reader["currency"].ToString(),
                                ReceiverAccount = reader["card_account"].ToString()
                            };
                        }
                    }
                    reader.Close();
                }
                return result;
            }
        }
    }
}
