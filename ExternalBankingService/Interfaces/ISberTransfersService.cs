using ExternalBanking;
using ExternalBanking.SberTransfers.Models;
using ExternalBanking.SberTransfers.Order;
using System;
using System.ServiceModel;

namespace ExternalBankingService.Interfaces
{
    [ServiceContract]
    public interface ISberTransfersService
    {
        [OperationContract]
        SberPreTransferRequisites GetDataForSberTransfer(ulong customerNumber, bool onlyAMD);

        [OperationContract]
        (ActionResult, DateTime?) SaveAndApproveSberIncomingTransferOrder(SberIncomingTransferOrder order, SourceType sourceType);
    }
}