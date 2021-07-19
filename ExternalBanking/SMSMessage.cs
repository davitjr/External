using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class SMSMessage
    {
        /// <summary>
        /// Հաղորդագրության համար
        /// </summary>
        public uint ID { get; set; }

        /// <summary>
        /// Հաղորդագրություն ստացող հեռախոսահամար
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Ուղարկվող տեքստ
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Հաղորդագրության տեսակ
        /// </summary>
        public int MessageType { get; set; }

        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Սեսիայի համարը
        /// </summary>
        public int SessionID { get; set; }

        /// <summary>
        /// Սեսիայի նկարագրությունը
        /// </summary>
        public string SessionDescription { get; set; }

        /// <summary>
        /// Հաճախորդ (եթե հասցեատերը հաճախորդ է)
        /// </summary>
        public ulong IdentityID { get; set; }

        /// <summary>
        /// Գրանցման ամսաթիվ
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public int UserID { get; set; }



        public void Add(ulong customerNumber)
        {
            SMSMessageDB.CreateSMSMessage(this);
        }

        public void Send(uint messageID)
        {
            SMSMessageDB.SendSMSMessage(messageID);
        }

        /// <summary>
        /// Վերադարձնում է ուղարկված SMS-ներ ժսմանակահատվածի համար
        /// </summary>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public static List<SMSMessage> GetSMSMessages(ulong customerNumber, DateTime dateFrom, DateTime dateTo)
        {
            List<SMSMessage> smsMessages = SMSMessageDB.GetSentSMSMessages(customerNumber, dateFrom, dateTo);
            return smsMessages;
        }

        /// <summary>
        /// Վերադարձնում է ուղարկված SMS-ներ ժսմանակահատվածի ու կոնկրետ տեսակի համար 
        /// </summary>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static List<SMSMessage> GetSMSMessagesByType(ulong customerNumber, DateTime dateFrom, DateTime dateTo, int messageType)
        {
            List<SMSMessage> smsMessages = SMSMessageDB.GetSentSMSMessagesByType(customerNumber, dateFrom, dateTo,messageType);
            return smsMessages;
        }

        /// <summary>
        /// Ուղարկում է PhoneBanking-ի ավտորիզացիոն  SMS
        /// </summary>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <param name="smsText">ուղարկվող տեքստ</param>
        public static ActionResult SendPhoneBankingAuthorizationSMSMessage(ulong customerNumber, string smsText, ACBAServiceReference.User user)
        {            
            int userID = user.userID;
            ActionResult result = SMSMessageDB.SendPhoneBankingAuthorizationSMSMessage(customerNumber, smsText, userID);
            return result;
        }
    }
}
