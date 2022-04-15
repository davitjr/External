using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;


namespace ExternalBanking.DBManager
{
    internal class LoanInterestRateChangeHistoryDB
    {

        internal static List<LoanInterestRateChangeHistory> GetLoanInterestRateChangeHistoryDetails(ulong productID)
        {

            List<LoanInterestRateChangeHistory> historyDetails = new List<LoanInterestRateChangeHistory>();

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @"SELECT Change_date, Old_value, New_value 
                                FROM Tbl_short_time_loans_changes_details 
                                WHERE Field_name = 'interest_rate' AND app_id = @appID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productID;
                    conn.Open();

                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count != 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            LoanInterestRateChangeHistory d = new LoanInterestRateChangeHistory();

                            if (row["Change_date"] != DBNull.Value)
                            {
                                d.ChangeDate = Convert.ToDateTime(row["Change_date"].ToString());
                            }
                            if (row["New_value"] != DBNull.Value)
                            {
                                d.InterestRateNew = float.Parse(row["New_value"].ToString());
                            }
                            if (row["Old_value"] != DBNull.Value)
                            {
                                d.InterestRateOld = float.Parse(row["Old_value"].ToString());
                            }

                            historyDetails.Add(d);
                        }
                    }
                    else
                        historyDetails = null;
                }
            }

            return historyDetails;
        }


    }
}
