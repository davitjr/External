using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class MessageDB
    {

        /// <summary>
        /// Վերադարձնում է հաղորդագրությունների ցանկ, կախված տեսակից (type=1 Ուղարկված,type=2 Ստացված)
        /// </summary>
        internal static List<Message> GetMessages(ulong customerNumber, DateTime dateFrom, DateTime dateTo, short type)
        {
            List<Message> messages = new List<Message>();


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT id,SentRecieve,status,Subject,Description,sent_date, reply_id FROM [Tbl_messages_with_bank] 
                                WHERE customer_number=@customerNumber and cast(sent_date as date)>=@dateFrom 
                                AND cast(sent_date as date)<=@dateTo and status<>-1 AND [SentRecieve]=@type ";

                sql = sql + " ORDER BY id desc";

                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;
                cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = type;

                conn.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Message message = new Message
                    {
                        Id = int.Parse(dr["id"].ToString()),
                        Type = int.Parse(dr["SentRecieve"].ToString()),
                        Status = int.Parse(dr["status"].ToString()),
                        Subject = Utility.ConvertAnsiToUnicode(dr["Subject"].ToString()),
                        Description = Utility.ConvertAnsiToUnicode(dr["Description"].ToString()),
                        SentDate = DateTime.Parse(dr["sent_date"].ToString()),
                        ReplyId = (dr["reply_id"] != DBNull.Value) ? Convert.ToDouble(dr["reply_id"].ToString()) : 0,
                    };
                    message.Attachments = GetMessageAttachmentDetails(message.Id);
                    messages.Add(message);
                }
            }

            return messages;
        }


        internal static List<Message> GetMessages(ulong customerNumber, short messagesCount, MessageType type)
        {
            List<Message> messages = new List<Message>();


            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT id,SentRecieve,status,Subject,Description,sent_date,  reply_id FROM [Tbl_messages_with_bank] 
                         WHERE customer_number=@customerNumber 
                         and [SentRecieve]=@type
                         and status<>-1 ORDER BY Sent_Date Desc ";



                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = type;
                conn.Open();

                using SqlDataReader dr = cmd.ExecuteReader();
                short i = 0;
                while (dr.Read() && i < messagesCount)
                {
                    Message message = new Message();
                    message.Id = int.Parse(dr["id"].ToString());
                    message.Type = int.Parse(dr["SentRecieve"].ToString());
                    message.Status = int.Parse(dr["status"].ToString());
                    message.Subject = Utility.ConvertAnsiToUnicode(dr["Subject"].ToString());
                    message.Description = Utility.ConvertAnsiToUnicode(dr["Description"].ToString());
                    message.SentDate = DateTime.Parse(dr["sent_date"].ToString());
                    message.ReplyId = (dr["reply_id"] != DBNull.Value) ? Convert.ToDouble(dr["reply_id"].ToString()) : 0;


                    messages.Add(message);
                    i++;
                }
            }

            return messages;
        }
        internal static int GetUnreadedMessagesCount(ulong customerNumber)
        {

            int messagesCount = 0;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT count(1) as cnt FROM [Tbl_messages_with_bank] WHERE customer_number=@customerNumber and status=1 and SentRecieve<>1 ";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                conn.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                    messagesCount = int.Parse(dr["cnt"].ToString());

            }

            return messagesCount;
        }

        internal static void Add(Message message, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand("sp_insertMsg", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@customer_Number", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@reply_id", SqlDbType.Float).Value = 0;
                    cmd.Parameters.Add("@insert_msg", SqlDbType.NVarChar).Value = message.Description;
                    cmd.Parameters.Add("@sentrecieve", SqlDbType.Int).Value = 1;
                    cmd.Parameters.Add("@subject", SqlDbType.NVarChar).Value = message.Subject;

                    if (message.Attachments != null)
                    {
                        DataTable table = new DataTable();
                        table.Columns.Add("UploadData", typeof(byte[]));
                        table.Columns.Add("UploadFileName", typeof(string));
                        table.Columns.Add("UploadFileExt", (typeof(string)));
                        foreach (var item in message.Attachments)
                        {
                            table.Rows.Add(item.AttachmentInBase64 == null ? item.Attachment : Convert.FromBase64String(item.AttachmentInBase64), item.FileName, item.FileExtension);
                        }

                        cmd.Parameters.Add("@msg_upload_data", SqlDbType.Structured).Value = table;
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }

        internal static void Delete(Message message, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand("Update Tbl_messages_with_bank set status=-1 where id=@id and customer_number=@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = message.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }

        internal static void MarkAsReaded(Message message, ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {

                using (SqlCommand cmd = new SqlCommand("Update Tbl_messages_with_bank set status=0 where id=@id and customer_number=@customerNumber", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = message.Id;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }

        internal static int GetUnreadMessagesCount(ulong customerNumber, MessageType type)
        {
            int result = 0;
            string sql = @"SELECT count(1) as Cnt FROM [Tbl_messages_with_bank] 
                         WHERE customer_number=@customerNumber 
                         and [SentRecieve]=@type
                         and status=1";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@customerNumber", SqlDbType.Float).Value = customerNumber;
                    cmd.Parameters.Add("@type", SqlDbType.TinyInt).Value = type;
                    result = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return result;
        }


        internal static void SendReminderNote(ulong customerNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"If not exists (Select id from Tbl_messages_with_bank where sentRecieve=4 and cast(sent_date as date) = cast(getdate() as date) and customer_number =  @customerNumber ) 
                                                           BEGIN exec sp_reminder  @customerNumber  END";
                    cmd.Connection = conn;

                    cmd.Parameters.Add("customerNumber", SqlDbType.Float).Value = customerNumber;

                    cmd.ExecuteNonQuery();
                }
            }
        }


        internal static OrderAttachment GetMessageAttachmentById(int Id)
        {
            OrderAttachment attachement = new OrderAttachment();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT FileContent,FileType,FileName FROM Tbl_Messages_Uploaded_files where Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        attachement.AttachmentInBase64 = Convert.ToBase64String((byte[])dt.Rows[i]["FileContent"]);
                        attachement.FileExtension = dt.Rows[i]["FileType"].ToString();
                        attachement.FileName = dt.Rows[i]["Filename"].ToString();
                    }
                }
            }
            return attachement;
        }

        private static List<OrderAttachment> GetMessageAttachmentDetails(int msgId)
        {
            List<OrderAttachment> attachements = new List<OrderAttachment>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["HBBaseConn"].ToString()))
            {
                string sql = @"SELECT ID, [FileName] , FileType FROM Tbl_Messages_Uploaded_files where msg_Id = @msgId";


                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@msgId", SqlDbType.Int).Value = msgId;

                    conn.Open();
                    DataTable dt = new DataTable();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dt.Load(dr);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        OrderAttachment message = new OrderAttachment();
                        message.FileName = row["FileName"].ToString();
                        message.Id = row["ID"].ToString();
                        message.FileExtension = row["FileType"].ToString();
                        attachements.Add(message);
                    }
                }
            }
            return attachements;
        }
    }
}
