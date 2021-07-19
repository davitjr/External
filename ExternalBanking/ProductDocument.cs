using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference; 

namespace ExternalBanking
{
   public class ProductDocument
    {
        /// <summary>
        /// Փաստաթղթի ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }
        /// <summary>
        /// Փաստաթղթի տեսակ 
        /// </summary>
        public int DocumentType { get; set; }
        /// <summary>
        /// Փաստաթղթի տեսակի նկարագրություն
        /// </summary>
        public string DocumentTypeDescription { get; set; }
        /// <summary>
        /// Փաստաթղթի 
        /// </summary>
        public int Source { get; set; }

        public static List<ProductDocument> GetProductDocuments(ulong productId)
        {
            List<ProductDocument> productDocumentsList = new List<ProductDocument>();
            productDocumentsList.AddRange(ProductDocumentDB.GetProductDocuments(productId));
            productDocumentsList.AddRange(GetHbProductDocuments(productId));
            return productDocumentsList;
        }

        public static List<ProductDocument> GetHbProductDocuments(ulong productId)
        {
            return ProductDocumentDB.GetHbProductDocuments(productId);
        }
        public static List<AttachmentDocument> GetHBAttachmentsInfo(ulong documentId)
        {
            return ProductDocumentDB.GetHBAttachmentsInfo(documentId);
        }
        public static byte[] GetOneHBAttachment(ulong id)
        {
            return ProductDocumentDB.GetOneHBAttachment(id);
        }
        public static int GetHbDocId(ulong productId)
        {
            return ProductDocumentDB.GetHbDocId(productId);
        }
    }
}
