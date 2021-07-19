using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ArcaDataServiceReference;

namespace ExternalBanking
{
    public class C2CTransferResult
    {
        //public C2CTransferResponse transferOrderResultt { get; set; }

        /// <summary>
        /// Վճարման գրանցման արդյունքի կարգավիճակ
        /// </summary>
        public byte ResultCode { get; set; }
        /// <summary>
        /// Վճարման գրանցման արդյունքի նկարագրություն
        /// </summary>
        public string ResultCodeDescription { get; set; }
        
        public string ResponseCode { get; set; }
        
        public string ResponseCodeDescription { get; set; }

        /// <summary>
        /// Տվյալների բազայում փոխանցման հերթական համար
        /// </summary>
        public ulong TransferID { get; set; }

        public string ProcessingCode { get; set; }

        public int SystemTraceAuditNumber { get; set; }

        public DateTime LocalTransactionDate { get; set; }

        public string rrn { get; set; }

        public string AuthorizationIdResponse { get; set; }

       


    }
}
