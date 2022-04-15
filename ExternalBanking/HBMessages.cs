using System.Collections.Generic;

namespace ExternalBanking
{
    public class HBMessages
    {
        public long ID { get; set; }
        public string FullName { get; set; }
        public ulong CustomerNumber { get; set; }
        public int? CustomerType { get; set; }

        public string SendDate { get; set; }

        public int SetNumber { get; set; }

        public int SentRecieve { get; set; }

        public string CustomerSubject { get; set; }

        public string CustomerMessage { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public List<HBMessageFiles> File { get; set; }
        public int? OperationType { get; set; }

        public int MessagesCount { get; set; } = 0;

        public int MessageStatus { get; set; }

    }
}
