using ExternalBanking.DBManager.SberTransfers;
using ExternalBanking.SberTransfers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
