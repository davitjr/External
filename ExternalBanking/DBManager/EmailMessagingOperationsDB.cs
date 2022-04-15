using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    internal class EmailMessagingOperationsDB
    {
        /// <summary>
        /// Հաճախորդի տվյալների ռիսկային փոփոխությունների առկայության դեպքում մոբայլ տոկենի ակտիվացման մասին ծանուցման բովանդակության ստացում
        /// </summary>
        /// <param name="TokenSerial"></param>
        public static string GetRiskyChangesAlertMailContent(string TokenSerial)
        {
            string mailContent = String.Empty;
            using (SqlConnection dbConn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    dbConn.Open();

                    cmd.Connection = dbConn;
                    cmd.CommandText = "pr_get_mobile_banking_activation_alert_content_for_risky_changes";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@token_serial", SqlDbType.NVarChar, 16).Value = TokenSerial;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            mailContent = dr["mailContent"].ToString();
                        }
                    }
                }
            }

            return mailContent;
        }

    }


}
