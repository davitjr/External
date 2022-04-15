using ExternalBanking.DBManager;

namespace ExternalBanking
{

    //Հայտին կցված փաստաթուղթ
    public class OrderAttachment
    {
        /// <summary>
        /// Ունիկալ համար (տվյալ դեպքում GUID)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Կցված ֆայլ
        /// </summary>
        public byte[] Attachment { get; set; }

        /// <summary>
        /// Կցված ֆայլ-ը base64 ֆորմատով
        /// </summary>
        public string AttachmentInBase64 { get; set; }

        /// <summary>
        /// Կցված ֆայլի անվանում
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Կցված  ֆայլի տեսակ
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Վերադարձնում է՝ արդյոք տվյալ հայտի համար կա տվյալ տեսակի պահպանված ֆայլ, թե ոչ
        /// </summary>
        /// <param name="orderId">Գործարքի կոդ</param>
        /// <param name="type">Կցված ֆայլի տեսակ</param>
        /// <returns></returns>
        public static bool HasAttachedFile(long orderId, int type)
        {
            return OrderDB.HasUploadedLoanContract(orderId, type);
        }
    }
}
