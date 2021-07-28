using ExternalBanking.DBManager.QrTransfers;

namespace ExternalBanking.QrTransfers
{
    public class QrTransfer
    {
        public string AccountNumber { get; set; }

        public string Guid { get; set; }

        public ulong CustomerNumber { get; set; }

        public QrTransfer() { }

        public QrTransfer(string accountNumber, string guid, ulong customerNumber)
        {
            AccountNumber = accountNumber;
            Guid = guid;
            CustomerNumber = customerNumber;
        }

        public ActionResult SaveAccountQrCode()
        {
            return QrTransfersDB.SaveAccountQrCode(AccountNumber, Guid, CustomerNumber);
        }

        public QrTransfer SearchAccountByQrCode()
        {
            return QrTransfersDB.SearchAccountByQrCode(Guid);
        }

        public string GetAccountQrCodeGuid()
        {
            return QrTransfersDB.GetAccountQrCodeGuid(AccountNumber);
        }
    }
}
