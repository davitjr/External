using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class UploadedFile
    {
        public string FileID { get; set; }

        public string  FileName { get; set; }
        public string FileType { get; set; }
        public byte[] File { get; set; }
        public ulong FileSize { get; set; }

        public string FileInBase64 { get; set; }
        public ulong CustomerNumber { get; set; }

        public static string SaveUploadedFile(UploadedFile uploadedFile)
        {
            return UploadedFileDB.SaveUploadedFile(uploadedFile);
        }

        public  static ReadXmlFileAndLog ReadXmlFile(string fileId, short filial, ulong customerNumber, string userName)
        {
            return UploadedFileDB.ReadXmlFile(fileId,filial,customerNumber,userName);
        }

        public static byte[] GetAttachedFile(long docID, int type)
        {
            return UploadedFileDB.GetAttachedFile(docID, type);
        }


    }
}
