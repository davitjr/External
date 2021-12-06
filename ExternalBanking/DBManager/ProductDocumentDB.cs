using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.Transactions;
using System.IO;
using ExternalBanking.ACBAServiceReference;

namespace ExternalBanking.DBManager
{
    class ProductDocumentDB
    {
        public static List<ProductDocument> GetProductDocuments(ulong productId)
        {
             
            List<ProductDocument> productDocumentList=new List<ProductDocument>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                string sqlString = @"SELECT 
						              p.document_id,
						              d.document_type ,
						              s.Type_of_passport 
				            FROM Tbl_link_product_to_document p
				            INNER JOIN tbl_type_of_Products t ON p.product_type = t.code
				            inner join Tbl_Customer_Documents d on d.document_id =p.document_id 
				            inner join Tbl_passport s on s.id=d.document_type 
				            where product_id=@productId";


                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ProductDocument productDocument = new ProductDocument();
                    productDocument.Id = ulong.Parse(dr["document_id"].ToString());
                    productDocument.DocumentType = int.Parse(dr["document_type"].ToString());
                    productDocument.DocumentTypeDescription =Utility.ConvertAnsiToUnicode(dr["Type_of_passport"].ToString());
                    productDocument.Source = 1;
                    productDocumentList.Add(productDocument);
                }
               
            } 
            
            return productDocumentList;
        }

        public static List<ProductDocument> GetHbProductDocuments(ulong productId)
        {
            List<ProductDocument> productDocumentList=new List<ProductDocument>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {

                string sqlString = "";

                conn.Open();
                SqlCommand cmd;
                int docId = GetHbDocId(productId);
                if (docId == 0)
                {
                    sqlString = @"SELECT id,attachment_type,type_description FROM tbl_HB_Attached_Documents D
                                     INNER JOIN Tbl_types_of_HB_Attached_Documents T on D.attachment_type=T.[type_id] 
									 inner join
									 (SELECT isNull(doc_ID,0) as doc_ID from tbl_hb_documents where debet_account=@productId and document_type in(7,12,13,14,17,28,29)) A on A.doc_ID=D.doc_ID";
                    cmd = new SqlCommand(sqlString, conn); 
                    cmd.Parameters.Add("@productId", SqlDbType.Float).Value = productId;
                }
                else
                {
                     sqlString = @"SELECT id,attachment_type,type_description FROM tbl_HB_Attached_Documents D
                                     INNER JOIN Tbl_types_of_HB_Attached_Documents T on D.attachment_type=T.[type_id] 
                                     WHERE doc_ID=@docId";
                     cmd = new SqlCommand(sqlString, conn);
                     cmd.Parameters.Add("@docId", SqlDbType.Int).Value = docId;
                }

                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ProductDocument productDocument = new ProductDocument();
                    productDocument.Id = ulong.Parse(dr["id"].ToString());
                    productDocument.DocumentType = int.Parse(dr["attachment_type"].ToString());
                    productDocument.DocumentTypeDescription =Utility.ConvertAnsiToUnicode(dr["type_description"].ToString());
                    productDocument.Source = 2;
                    productDocumentList.Add(productDocument);

                }
            }
            return productDocumentList;
                
        }
        public static List<AttachmentDocument> GetHBAttachmentsInfo(ulong documentId)
        {
            List<AttachmentDocument> attDocumentList = new List<AttachmentDocument>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sqlString = @"select id,attachment_date,extension from tbl_HB_Attached_Documents D
                                     inner join Tbl_types_of_HB_Attached_Documents T on D.attachment_type=T.[type_id] 
                                     where id=@docId";


                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@docId", SqlDbType.Int).Value = documentId;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    AttachmentDocument attDocument = new AttachmentDocument();
                    attDocument.id = ulong.Parse(dr["id"].ToString());
                    attDocument.AttachmentDate = DateTime.Parse(dr["attachment_date"].ToString());
                    attDocument.PageNumber = 1;
                    if (dr["extension"].ToString() == ".pdf")
                    {
                        attDocument.FileExtension = 2;
                    }
                    else if (dr["extension"].ToString() == ".jpg")
                    {
                        attDocument.FileExtension = 1;
                    }
                    else if (dr["extension"].ToString() == ".png")
                    {
                        attDocument.FileExtension = 3;
                    }
                    attDocumentList.Add(attDocument);
                }


            }
            return attDocumentList;
        }
        public static byte[] GetOneHBAttachment(ulong id)
        {
            byte[] attachment=null;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["HbBaseConn"].ToString()))
            {
                string sqlString = @"select attachment from tbl_HB_Attached_Documents where id=@Id";


                conn.Open();
                using SqlCommand cmd = new SqlCommand(sqlString, conn);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    attachment = (byte[])dr["attachment"];
                }
            }
            return attachment;
        }
        public static int GetHbDocId(ulong productId)
        {
            int docId = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AccOperBaseConnRO"].ToString()))
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(@"SELECT isNull(HB_doc_ID,0) as HB_doc_ID FROM [tbl_deposits;] WHERE App_Id=@appId
                                                    Union ALL SELECT isNull(HB_doc_ID,0) as HB_doc_ID FROM [tbl_short_time_loans;] WHERE App_Id=@appId
                                                    Union ALL SELECT isNull(HB_doc_ID,0) as HB_doc_ID FROM Tbl_credit_lines WHERE App_Id=@appId
                                                    Union ALL SELECT isNull(doc_ID,0) as HB_doc_ID FROM Tbl_bonds WHERE App_Id=@appId", conn);
                 cmd.Parameters.Add("@appId", SqlDbType.Float).Value = productId;

                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    docId = int.Parse(dr["HB_doc_ID"].ToString());
                }


            }
            return docId;
        }
    }
}
