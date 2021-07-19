using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    static class CardAdditionalDataDB
    {
        internal static List<CardAdditionalData> GetCardAdditionalDatas(string cardnumber, string expirydate)
        {
            List<CardAdditionalData> cardAdditionalDatas = new List<CardAdditionalData>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["AccOperBaseConn"].ToString()))
            {
                string sql = @" SELECT AdditionDescription, AdditionValue,Number_Of_Set, Cardnumber,Key_num,Cardnumber, ExpiryDate,TVA.AdditionID
                                 FROM Tbl_VisaAppAdditions VAA
                                 INNER JOIN Tbl_Type_of_VisaApp_Add TVA ON VAA.AdditionID = TVA.AdditionID 
                                WHERE Cardnumber = @cardnumber
                                 AND ExpiryDate = @expirydate
                                 AND VAA.AdditionID  IN (3,4,9,10,14)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@cardnumber", SqlDbType.VarChar).Value = cardnumber;
                    cmd.Parameters.Add("@expirydate", SqlDbType.VarChar).Value = expirydate;

                    conn.Open();

                    DataTable dt = new DataTable();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {

                        dt.Load(dr);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            DataRow row = dt.Rows[i];

                            CardAdditionalData cardAdditionalData = new CardAdditionalData();
                            cardAdditionalData.CardAdditionalDataID = int.Parse(row["Key_num"].ToString());
                            cardAdditionalData.AdditionID = int.Parse(row["AdditionID"].ToString());
                            cardAdditionalData.AdditionValue = row["AdditionValue"].ToString();
                            cardAdditionalData.AdditionDescription = Utility.ConvertAnsiToUnicode(row["AdditionDescription"].ToString());
                            cardAdditionalData.SetNumber = int.Parse(row["Number_Of_Set"].ToString());

                            cardAdditionalDatas.Add(cardAdditionalData);
                        }
                    }

                    return cardAdditionalDatas;
                }
            }
        }
    }
}
