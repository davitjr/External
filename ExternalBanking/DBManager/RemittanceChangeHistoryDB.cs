using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DBManager
{
    internal static class RemittanceChangeHistoryDB
    {
        internal static List<RemittanceChangeHistoryItem> GetRemittanceChangeHistory(ulong transferId)
        {
            List<RemittanceChangeHistoryItem> history = new List<RemittanceChangeHistoryItem>();

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT H.change_date, H.change_set_number, case D.document_type when 202 then N'Փոփոխված' else
                                                  (case document_subtype when 1 then N'Ուղարկված/վերադարձված' else N'Վճարված/չեղարկված' end) end as quality_name
                                                  FROM Tbl_HB_Documents D INNER JOIN TBL_HB_Quality_History H on D.doc_id = H.doc_id
                                                  WHERE D.transfer_id = @transfer_id and D.quality = 30 and document_type in (198,202)", conn);

                cmd.Parameters.Add("@transfer_id", SqlDbType.Int).Value = transferId;
                dt.Load(cmd.ExecuteReader());

                foreach (DataRow row in dt.Rows)
                {
                    RemittanceChangeHistoryItem historyItem = new RemittanceChangeHistoryItem();

                    historyItem.User = new ACBAServiceReference.User();
                    historyItem.User.userID = Convert.ToInt16(row[("change_set_number")]);
                    historyItem.QualityName = row["quality_name"].ToString();
                    historyItem.ChangeDate = Convert.ToDateTime(row["change_date"]);

                    history.Add(historyItem);
                }
            }

            if(history.Count == 0)
            {
                history = null;
            }

            return history;
        }

    }
}
