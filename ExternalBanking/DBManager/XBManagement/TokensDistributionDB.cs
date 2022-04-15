using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    public class TokensDistributionDB
    {
        /// <summary>
        ///Վերադարձնում է ընտրված միջակայքի մասնաճյուղի ազատ տոկեները 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="userFilial"></param>
        /// <returns></returns>
        internal static List<string> GetUnusedTokensByFilialAndRange(string from, string to, int filial)
        {
            List<string> unusedTokensByFilialAndRange = new List<string>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    if (string.IsNullOrEmpty(from) && string.IsNullOrEmpty(to))
                    {
                        cmd.CommandText = @"SELECT L.token_serial FROM [Tbl_Token_List] L
                                                                    LEFT JOIN tbl_Tokens T ON L.token_serial=T.token_serial
                                                                    LEFT JOIN (SELECT TD.token_serial FROM Tbl_HB_Token_Order_Details TD 
                                                                    INNER JOIN Tbl_HB_documents D ON TD.doc_Id = D.doc_ID WHERE  D.quality in (3,50)) H ON H.token_serial=L.token_serial AND H.token_serial IS NULL
                                                                    WHERE L.[used] = 0 AND L.filial_code = ISNULL(@filial,22000)  and T.token_serial IS NULL 
                                                                    ORDER BY L.[token_serial] DESC";
                    }
                    else
                    {

                        cmd.CommandText = @"SELECT L.token_serial FROM [Tbl_Token_List] L
                                                                    LEFT JOIN tbl_Tokens T ON L.token_serial=T.token_serial
                                                                    LEFT JOIN (SELECT TD.token_serial FROM Tbl_HB_Token_Order_Details TD 
                                                                    INNER JOIN Tbl_HB_documents D ON TD.doc_Id = D.doc_ID WHERE  D.quality in (3,50)) H ON H.token_serial=L.token_serial AND H.token_serial IS NULL
                                                                    WHERE L.[used] = 0 AND L.filial_code = ISNULL(@filial,22000)  and T.token_serial IS NULL  AND L. token_serial BETWEEN  @from AND @to
                                                                    ORDER BY L.[token_serial] DESC";
                        cmd.Parameters.Add("@from", SqlDbType.NVarChar).Value = from;
                        cmd.Parameters.Add("@to", SqlDbType.NVarChar).Value = to;
                    }

                    cmd.Parameters.Add("@filial", SqlDbType.Int).Value = filial;


                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                }
                foreach (DataRow row in dt.Rows)
                {
                    unusedTokensByFilialAndRange.Add(row["token_serial"].ToString());
                }
            }
            return unusedTokensByFilialAndRange;
        }

        internal static void MoveUnusedTokens(int filialToMove, List<string> unusedTokens)
        {
            string unusedTokensString = "(";
            unusedTokens.ForEach(t =>
            {
                unusedTokensString = unusedTokensString + "'" + t + "',";
            });
            unusedTokensString = unusedTokensString.Remove(unusedTokensString.Length - 1);
            unusedTokensString = unusedTokensString + ")";
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBLoginsConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {

                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"update[Tbl_Token_List] " +
                                      "SET filial_code = @filialToMove WHERE token_serial IN " + unusedTokensString;
                    cmd.Parameters.Add("@filialToMove", SqlDbType.NVarChar).Value = filialToMove;
                    //cmd.Parameters.Add("@unUsedTokens", SqlDbType.NVarChar).Value = unusedTokensString;
                    cmd.ExecuteNonQuery();

                }

            }

        }
    }

}
