using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace ExternalBanking.DBManager
{
    internal class SMSMessageDB
    {
        /// <summary>
        /// Ստեղծում ենք խմբագրվող ստատուսով  SMS
        /// </summary>
        internal static uint CreateSMSMessage(SMSMessage sms)
        {
            uint messageID;
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SMSBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand ("pr_create_one_message",conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@identity_ID", SqlDbType.Int).Value = sms.IdentityID;
                    cmd.Parameters.Add("@phone_number", SqlDbType.NVarChar).Value = sms.PhoneNumber;
                    cmd.Parameters.Add("@message_text", SqlDbType.NVarChar).Value = sms.MessageText;
                    cmd.Parameters.Add("@meesage_type_ID", SqlDbType.Int).Value = sms.MessageType;
                    cmd.Parameters.Add("@sesionDescription", SqlDbType.NVarChar).Value = sms.SessionDescription;
                    cmd.Parameters.Add("@registrationSetNumber", SqlDbType.Int).Value = sms.UserID;
                    cmd.Parameters.Add(new SqlParameter("@messageID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    messageID = uint.Parse(cmd.Parameters["@messageID"].Value.ToString());
                }
            }
            return messageID;
        }

        /// <summary>
        /// Ուղարկելու համար տվյալ ID-ով SMS -ի ստատուսը դարձնում ենք 2 (Service-ը ուղարկում է 2 ստատուսով (ուղարկման ենթակա) SMS-ները) 
        /// </summary>
        internal static void SendSMSMessage(uint messageID)
        { 
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SMSBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"UPDATE tbl_messages_to_send SET message_status_ID = 2 WHERE ID= @Id AND message_status_ID <> 2";
                    cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = messageID;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդին ուղարկված SMS-ների ցանկ ժամանակահատվածի համար
        /// </summary>
        internal static List<SMSMessage> GetSentSMSMessages(ulong customerNumber,DateTime dateFrom,DateTime dateTo)
        {
            List<SMSMessage> smsMessages = new List<SMSMessage>();
            ulong identityID;
            Customer customer = new Customer();
            identityID = customer.GetIdentityId(customerNumber);

            using (SqlConnection conn = new SqlConnection (WebConfigurationManager.ConnectionStrings["SMSBaseConn"].ToString()))
            {
                using(SqlCommand cmd = new SqlCommand ())
                {
                    cmd.Connection=conn ;
                    cmd.CommandType=CommandType.Text ;
                    cmd.CommandText = @"SELECT * FROM tbl_sent_messages 
                                        WHERE identity_ID=@identityID AND registration_date >=@dateFrom AND registration_date <=@dateTo
                                        ORDER BY registration_date desc";
                    cmd.Parameters.Add("@identityID", SqlDbType.Int).Value = identityID;
                    cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read ())
                    {
                        SMSMessage smsMessage = new SMSMessage();
                        smsMessage.ID = uint.Parse(dr["ID"].ToString());
                        smsMessage.PhoneNumber = dr["phone_number"].ToString();
                        smsMessage.MessageText = dr["message_text"].ToString();
                        smsMessage.MessageType = Convert.ToInt32(dr["message_type_ID"]);
                        smsMessage.Status = Convert.ToInt32(dr["message_status_ID"]);
                        smsMessage.SessionID = Convert.ToInt32(dr["messaging_sesion_ID"]);
                        
                        if (dr["identity_id"]!=DBNull.Value)
                        {
                            smsMessage.IdentityID = Convert.ToUInt64(dr["identity_ID"]);
                        }
                        
                        smsMessage.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                        smsMessages.Add(smsMessage);
                    }
                    return smsMessages;
                }
            }
        }


        /// <summary>
        /// Վերադարձնում է հաճախորդին ուղարկված SMS-ների ցանկ կոնկրետ տեսակի ու ժամանակահատվածի համար
        /// </summary>
        internal static List<SMSMessage> GetSentSMSMessagesByType(ulong customerNumber, DateTime dateFrom, DateTime dateTo, int messageType)
        {
            List<SMSMessage> smsMessages = new List<SMSMessage>();

            ulong identityID;
            Customer customer = new Customer();
            identityID = customer.GetIdentityId(customerNumber);

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["SMSBaseConn"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT * FROM tbl_sent_messages 
                                        WHERE identity_ID=@identityID AND message_type_ID = @messageType 
                                              AND registration_date >=@dateFrom AND registration_date <=@dateTo
                                        ORDER BY registration_date desc";
                    cmd.Parameters.Add("@identityID", SqlDbType.Int).Value = identityID;
                    cmd.Parameters.Add("@dateFrom", SqlDbType.SmallDateTime).Value = dateFrom;
                    cmd.Parameters.Add("@dateTo", SqlDbType.SmallDateTime).Value = dateTo;
                    cmd.Parameters.Add("@messageType", SqlDbType.Int).Value = messageType;
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        SMSMessage smsMessage = new SMSMessage();
                        smsMessage.ID = uint.Parse(dr["ID"].ToString());
                        smsMessage.PhoneNumber = dr["phone_number"].ToString();
                        smsMessage.MessageText = dr["message_text"].ToString();
                        smsMessage.MessageType = Convert.ToInt32(dr["message_type_ID"]);
                        smsMessage.Status = Convert.ToInt32(dr["message_status_ID"]);
                        smsMessage.SessionID = Convert.ToInt32(dr["messaging_sesion_ID"]);

                        if (dr["identity_id"] != DBNull.Value)
                        {
                            smsMessage.IdentityID = Convert.ToUInt64(dr["identity_ID"]);
                        }

                        smsMessage.RegistrationDate = Convert.ToDateTime(dr["registration_date"]);
                        smsMessages.Add(smsMessage);
                    }
                    return smsMessages;
                }
            }
        }

        /// <summary>
        /// Ստեղծում ենք խմբագրվող ստատուսով  SMS, ուղարկում MessageType = 1-ով (PhoneBanking-ի ավտորիզացիայի համար)
        /// </summary>
        internal static ActionResult SendPhoneBankingAuthorizationSMSMessage(ulong customerNumber,string message, int user)
        {
            ActionResult result = new ActionResult();
            try
            {
                ulong identityID;
                string phoneNumber;
                uint messageID;
                Customer customer = new Customer();
                phoneNumber = customer.GetPhoneBankingAuthorizationPhoneNumber(customerNumber);
                identityID = customer.GetIdentityId(customerNumber);
                SMSMessage smsMessage = new SMSMessage();
                smsMessage.IdentityID = identityID;
                smsMessage.MessageText = message;
                smsMessage.MessageType = 1;
                smsMessage.PhoneNumber = phoneNumber;
                smsMessage.SessionDescription = "";
                smsMessage.UserID = user;
                messageID = CreateSMSMessage(smsMessage);
                SendSMSMessage(messageID);
                result.ResultCode = ResultCode.Normal;
                result.Id = 1;
            }
            catch
            {
                result.ResultCode = ResultCode.Failed;
                result.Id = -1;
            }
            return result; 
            
        }
        

    }


}
