using ExternalBanking.PreferredAccounts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager.PreferredAccounts
{
    public class PreferredAccountDB
    {
        internal static PreferredAccount GetSelectedOrDefaultPreferredAccountNumber(PreferredAccountServiceTypes serviceType, ulong customerNumber)
        {
            PreferredAccount preferredAccount = new PreferredAccount();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "get_selected_preferred_account";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@service_type", SqlDbType.Int).Value = (int)serviceType;
                cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    preferredAccount.AccountNumber = Convert.ToString(dr["accountNumber"]);
                    preferredAccount.IsActive = Convert.ToBoolean(dr["isActive"]);
                }
            }
            return preferredAccount;
        }

        internal static bool IsDisabledPreferredAccountService(ulong customerNumber, PreferredAccountServiceTypes preferredAccountServiceType)
        {
            bool isDisabled;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "Select dbo.[IsDisabledPreferredAccountService](@customerNumber, @serviceType)";
                    cmd.Parameters.Add("@serviceType", SqlDbType.Int).Value = (int)preferredAccountServiceType;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    isDisabled = (bool)cmd.ExecuteScalar();
                }
            }
            return isDisabled;
        }
    }
}
