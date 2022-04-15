using ExternalBanking.DBManager.SberTransfers;
using ExternalBanking.SberTransfers.Models;

namespace ExternalBanking.SberTransfers
{
    public static class SberTransfers
    {
        public static SberPreTransferRequisites GetDataForSberTransfer(ulong customerNumber)
        {
            return SberTransfersDB.GetDataForSberTransfer(customerNumber);
        }
    }
}
