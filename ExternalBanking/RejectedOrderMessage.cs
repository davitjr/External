using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class RejectedOrderMessage : UserMessage
    {
        /// <summary>
        /// Հաղորդագրության ունիկալ համար
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Հայտ
        /// </summary>
        public Order Order { get; set; }
        /// <summary>
        ///  Վերադարձնում է մերժված հաղորդագրությունները
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<RejectedOrderMessage> GetRejectedMessages(int userId, int filter, int start, int end)
        {

            return RejectedOrderMessageDB.GetRejectedMessages(userId, filter, start, end);
        }

        /// <summary>
        /// Փակում է նշված հաղորդագրությունը
        /// </summary>
        /// <param name="messageId"></param>
        public static void CloseRejectedMessage(int messageId)
        {

            RejectedOrderMessageDB.CloseRejectedMessage(messageId);
        }


        /// <summary>
        /// Վերադարձնում է հաղորդագրությունների ընդհանուր քանակը
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetTotalRejectedUserMessages(int userId)
        {

            return RejectedOrderMessageDB.GetTotalRejectedUserMessages(userId);
        }
    }
}
