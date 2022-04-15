using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class Message
    {
        /// <summary>
        /// Հաղորդագրության ունիկալ համար (Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Հաղորդագրության վերնագիր
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Հաղորդագրության բովանդակություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ուղարկման ամսաթիվ
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Կարգավիճակ, (կարդացված, հեռացված...)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Տեսակ (Ուղղարկված, ստացված)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Կից ֆայլեր
        /// </summary>
        public List<OrderAttachment> Attachments { get; set; }

        /// <summary>
        /// Պատասխան էջի համար ունիկալ կոդ
        /// </summary>
        public double ReplyId { get; set; }


        /// <summary>
        /// Վերադարձնում է հաղորդագրությունների 
        /// </summary>
        /// <param name="customerNumber">հաճախորդի համար</param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="type">հաղորդագրության տեսակ (ստացված,ուղարկված)</param>
        /// <returns></returns>
        public static List<Message> GetMessages(ulong customerNumber, DateTime dateFrom, DateTime dateTo, short type)
        {
            List<Message> messages = MessageDB.GetMessages(customerNumber, dateFrom, dateTo, type);
            return messages;
        }
        /// <summary>
        /// Վերադարձնում է հաղորդագրությունների ցանկ, կախված տեսակից և քանակից
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="messagesCount">Հաղորդագրությունների քանակ</param>
        /// <param name="type">Հաղորդագրությունների տեսակ</param>
        /// <returns></returns>
        public static List<Message> GetMessages(ulong customerNumber, short messagesCount, MessageType type)
        {
            return MessageDB.GetMessages(customerNumber, messagesCount, type);
        }



        public void Add(ulong customerNumber)
        {
            MessageDB.Add(this, customerNumber);
        }

        public void Delete(ulong customerNumber)
        {
            MessageDB.Delete(this, customerNumber);
        }

        /// <summary>
        /// Նշում է հաղորդագրությունը որպես կարդացված
        /// </summary>
        /// <param name="customerNumber"></param>
        public void MarkAsReaded(ulong customerNumber)
        {
            MessageDB.MarkAsReaded(this, customerNumber);
        }


        /// <summary>
        /// Վերադարձնում է չկարդացված հաղորդագրությունների քանակը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static int GetUnreadedMessagesCount(ulong customerNumber)
        {
            return MessageDB.GetUnreadedMessagesCount(customerNumber);
        }


        /// <summary>
        /// Վերադարձնում է նշված տեսակի չկարդացված հաղորդագրությունների քանակը:
        /// </summary>
        /// <param name="customerNumber">Հաճախորդի համար</param>
        /// <param name="type">Հաղորդագրության տեսակ</param>
        /// <returns></returns>
        public static int GetUnreadMessagesCount(ulong customerNumber, MessageType type)
        {
            return MessageDB.GetUnreadMessagesCount(customerNumber, type);
        }

        /// <summary>
        /// Օգտագործողի մուտքի ժամանակ հիշեցումների գեներացման ապահովում
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static void SendReminderNote(ulong customerNumber)
        {
            MessageDB.SendReminderNote(customerNumber);
        }

        public static OrderAttachment GetMessageAttachmentById(int Id)
        {
            return MessageDB.GetMessageAttachmentById(Id);
        }
    }
}
