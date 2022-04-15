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
        SberPreTransferRequisites GetDataForSberTransfer(ulong customerNumber);

        [OperationContract]
        (ActionResult, DateTime?) SaveAndApproveSberIncomingTransferOrder(SberIncomingTransferOrder order);
    }
}