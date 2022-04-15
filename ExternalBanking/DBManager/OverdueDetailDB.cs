using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace ExternalBanking.DBManager
{
    static class OverdueDetailDB
    {
        /// <summary>
        /// Հաճախորդի ամբողջ ժամկետանցի պատմություն
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static List<OverdueDetail> GetOverdueDetails(ulong customerNumber)
        {
            List<OverdueDetail> details = new List<OverdueDetail>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_get_overdue_data";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_number", SqlDbType.Float).Value = customerNumber;
                    dt.Load(cmd.ExecuteReader());
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            OverdueDetail detail = SetOverdueDetail(row);

                            details.Add(detail);
                        }
                    }
                }
            }
            return details;

        }
        private static OverdueDetail SetOverdueDetail(DataRow row)
        {
            OverdueDetail detail = new OverdueDetail();
            if (row != null)
            {
                if (!String.IsNullOrEmpty(row["app_id"].ToString()))
                {
                    detail.Productid = long.Parse(row["app_id"].ToString());
                }
                if (!String.IsNullOrEmpty(row["loan_start_date"].ToString()))
                {
                    detail.ProductStartDate = DateTime.Parse(row["loan_start_date"].ToString());
                }
                if (!String.IsNullOrEmpty(row["loan_end_date"].ToString()))
                {
                    detail.ProductEndDate = DateTime.Parse(row["loan_end_date"].ToString());
                }
                if (!String.IsNullOrEmpty(row["start_capital"].ToString()))
                {
                    detail.StartCapital = double.Parse(row["start_capital"].ToString());
                }
                if (!String.IsNullOrEmpty(row["product_type"].ToString()))
                {
                    detail.ProductType = short.Parse(row["product_type"].ToString());
                }
                if (!String.IsNullOrEmpty(row["checked"].ToString()))
                {
                    detail.Checked = bool.Parse(row["checked"].ToString());
                }
                if (!String.IsNullOrEmpty(row["quality"].ToString()))
                {
                    detail.Quality = short.Parse(row["quality"].ToString());
                }
                if (!String.IsNullOrEmpty(row["rate_amount"].ToString()))
                {
                    detail.RateAmount = double.Parse(row["rate_amount"].ToString());
                }
                if (!String.IsNullOrEmpty(row["amount"].ToString()))
                {
                    detail.Amount = double.Parse(row["amount"].ToString());
                }
                if (!String.IsNullOrEmpty(row["currency"].ToString()))
                {
                    detail.Currency = row["currency"].ToString();
                }
                if (!String.IsNullOrEmpty(row["start_date"].ToString()))
                {
                    detail.StartDate = DateTime.Parse(row["start_date"].ToString());
                }

                if (!String.IsNullOrEmpty(row["end_date"].ToString()))
                {
                    detail.EndDate = DateTime.Parse(row["end_date"].ToString());
                }
            }

            return detail;

        }

        /// <summary>
        /// Հաճախորդի մեկ ժամկետանց պատմության ուղղում
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="updateReason"></param>
        public static void GenerateLoanOverdueUpdate(long productId, DateTime startDate, DateTime? endDate, string updateReason, short setNumber)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "pr_update_overdue_history";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@appID", SqlDbType.Float).Value = productId;
                    cmd.Parameters.Add("@dateOfOverBeg", SqlDbType.SmallDateTime).Value = startDate;
                    if (endDate != null)
                        cmd.Parameters.Add("@dateOfOverEnd", SqlDbType.SmallDateTime).Value = endDate;
                    cmd.Parameters.Add("@changeReason", SqlDbType.NVarChar).Value = updateReason;
                    cmd.Parameters.Add("@setNumber", SqlDbType.Int).Value = setNumber;

                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
