using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExternalBanking.DBManager
{
    class ClaimEventDB
    {
        internal static List<ClaimEvent> GetClaimEvents(int claimNumber)
        {
            DataTable dt = new DataTable();
            List<ClaimEvent> events = null;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"Select * From Tbl_problem_loan_events  Where Claim_Number =@claimNumber  Order By event_date", conn);
                cmd.Parameters.Add("@claimNumber", SqlDbType.Int).Value = claimNumber;
                dt.Load(cmd.ExecuteReader());
                if (dt.Rows.Count > 0)
                {
                    events = new List<ClaimEvent>();
                    foreach (DataRow row in dt.Rows)
                    {
                        ClaimEvent claimEvent = new ClaimEvent();
                        claimEvent.claimNumber = int.Parse(row["claim_number"].ToString());
                        claimEvent.EventNumber = int.Parse(row["event_number"].ToString());
                        claimEvent.SetNumber = int.Parse(row["event_set_number"].ToString());
                        claimEvent.EventDate = DateTime.Parse(row["event_date"].ToString());
                        claimEvent.Type = short.Parse(row["event_type"].ToString());
                        claimEvent.CourtType = row["court_type"] != DBNull.Value ? short.Parse(row["court_type"].ToString()) : (short)0;
                        claimEvent.Purpose = row["event_purpose"] != DBNull.Value ? short.Parse(row["event_purpose"].ToString()) : (short)0;
                        claimEvent.ClaimAmount = row["claim_amount"] != DBNull.Value ? double.Parse(row["claim_amount"].ToString()) : 0;
                        claimEvent.Description = row["event_add_inf"] != DBNull.Value ? row["event_add_inf"].ToString() : null;
                        claimEvent.EventTax = new List<Tax>();
                        Tax tax = Tax.GetTax(claimEvent.claimNumber, claimEvent.EventNumber);
                        if (tax != null)
                        {
                            claimEvent.EventTax.Add(tax);
                        }


                        events.Add(claimEvent);
                    }
                }

            }
            return events;
        }
    }
}
