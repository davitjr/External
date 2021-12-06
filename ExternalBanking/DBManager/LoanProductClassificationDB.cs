using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ExternalBanking.DBManager
{
    class LoanProductClassificationDB
    {
        internal static List<LoanProductClassification> GetLoanProductClassifications(ulong productId, DateTime dateFrom)
        {
            List<LoanProductClassification> classifications = new List<LoanProductClassification>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT date_of_calculation,sum_of_capital,[days],individ,store,
                                                CASE [type] WHEN 1 THEN CASE WHEN isnull(C.loan_type,0)=9 THEN N'Օգտ․ մաս․ գեր․' ELSE N'Օգտ․ մաս․' END WHEN 5 THEN N'Չօգտ․ մաս․' END  as [type] 
                                                FROM Tbl_store_new s
                                                LEFT JOIN (select app_id,loan_type from tbl_credit_lines where app_id=@app_id) c
                                                on s.app_id = c.app_id 
                                                WHERE s.app_id=@app_id AND date_of_calculation >=@date_from Order by date_of_calculation desc", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@app_Id", SqlDbType.Float).Value = productId;
                cmd.Parameters.Add("@date_from", SqlDbType.SmallDateTime).Value = dateFrom;

                using DataTable dt = new DataTable();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {

                    dt.Load(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    DataRow row = dt.Rows[i];

                    LoanProductClassification classification = new LoanProductClassification();
                    classification.ClassificationDate = DateTime.Parse(row["date_of_calculation"].ToString());
                    classification.StoreAmount = double.Parse(row["sum_of_capital"].ToString());
                    classification.DaysCount = int.Parse(row["days"].ToString());
                    classification.StoreType = bool.Parse(row["individ"].ToString()) ? (byte)1 : (byte)0;
                    classification.StorePercent = float.Parse(row["store"].ToString());
                    classification.StoreAmountTypeDescription = row["type"].ToString();
                    classifications.Add(classification);
                }
            }
            return classifications;

        }
    }
}
