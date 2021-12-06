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
    internal static class RejectedOrderMessageDB
    {
        internal static List<RejectedOrderMessage> GetRejectedMessages(int userId,int filter,int start=0,int end=0)
        {
            var initialCount = 3;
            List<RejectedOrderMessage> userMessages = new List<RejectedOrderMessage>();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = String.Empty;
                if (start == 0 && end == 0)
                {
                    string limitedRows = "";
                    if (filter == 2)
                    {
                        limitedRows = " TOP(@initialCount) ";
                    }                   
                    sql = @"SELECT " + limitedRows + @" m.Id,m.subject,m.message,m.message_type,m.userId,m.read_date,m.registration_date,d.doc_Id,t.description 
                                     FROM Tbl_user_messages AS m
                                     INNER JOIN Tbl_user_message_document AS d
                                     on m.Id=d.message_Id
                                     INNER JOIN Tbl_message_types AS t
                                     on m.message_type=t.Id
                                     WHERE m.userId=@userId";
                    if (filter == 1)
                    {
                        sql += " AND m.read_date IS  NULL";
                    }
                    else if (filter == 2)
                    {
                        sql += " AND m.read_date IS NOT NULL";
                    }
                    sql = sql + " ORDER BY Id DESC";

                }
                else if((start != 0 || end != 0) && filter==2)
                {
                    string whereCondition = "";  
                    whereCondition = " AND m.read_date IS NOT  NULL";
                    sql = @"SELECT * FROM(SELECT ROW_NUMBER() OVER (ORDER BY m.Id desc) as row , m.Id,m.subject,m.message,m.message_type,m.userId,m.read_date,m.registration_date,d.doc_Id,t.description
                                                  FROM Tbl_user_messages AS m
                                                  INNER JOIN Tbl_user_message_document AS d
                                                  on m.Id=d.message_Id
                                                  INNER JOIN Tbl_message_types AS t
                                                  on m.message_type=t.Id
                                                  WHERE m.userId=@userId"+ whereCondition+ ")AS result " +
                                                 " WHERE result.row >  (@initialCount + @start)   and result.row <= (@initialCount + @end)";
                }

                using SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@initialCount", SqlDbType.Int).Value = initialCount;
                cmd.Parameters.Add("@start", SqlDbType.Int).Value = start;
                cmd.Parameters.Add("@end", SqlDbType.Int).Value = end;
                conn.Open();
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    RejectedOrderMessage message = new RejectedOrderMessage();
                    message.Id = int.Parse(dr["Id"].ToString());
                    message.UserId = int.Parse(dr["userId"].ToString());
                    message.Order = Order.GetOrder(long.Parse(dr["doc_Id"].ToString()));
                    message.Message = dr["message"].ToString();
                    message.Subject = dr["subject"].ToString();
                    message.MessageType = int.Parse(dr["message_type"].ToString());
                    message.MessageTypeDescription = dr["description"].ToString();
                    if (dr["read_date"] != DBNull.Value)
                    {
                        message.ReadDate = DateTime.Parse(dr["read_date"].ToString());
                    }
                    else
                    {
                        message.ReadDate = null;
                    }
                    message.RegistrationDate = DateTime.Parse(dr["registration_date"].ToString());
                    userMessages.Add(message);
                }
            }
            return userMessages;
        }
        internal static void CloseRejectedMessage(int messageId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"UPDATE Tbl_user_messages 
                               SET read_date=GETDATE()
                               WHERE Id=@Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = messageId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
             
            }
        }

        internal static int GetTotalRejectedUserMessages(int userId)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    string sql = @"SELECT COUNT(1) 
                                     FROM Tbl_user_messages AS m 
                                     WHERE m.userId=@userId AND m.read_date IS NULL";

                    cmd.CommandText = sql;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    return Convert.ToInt32(cmd.ExecuteScalar().ToString());
                }
            }

        }
    }
}
