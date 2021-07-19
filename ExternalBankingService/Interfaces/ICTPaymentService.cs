using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ExternalBanking;
using ExternalBanking.ArcaDataServiceReference;

namespace ExternalBankingService.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICTPaymentService" in both code and config file together.
    [ServiceContract]
    public interface ICTPaymentService
    {
        [OperationContract]
        PaymentRegistrationResult SaveCTPaymentOrder(CTPaymentOrder order);

        [OperationContract]
        PaymentRegistrationResult SaveCTLoanMatureOrder(CTLoanMatureOrder order);


        [OperationContract]
        PaymentStatus GetPaymentStatus(long paymentID);

        [OperationContract]
        PaymentStatus GetPaymentStatusByOrderID(long orderID);

        [OperationContract]
        void Init(CashTerminal cashTerminal);

        [OperationContract]
        CashTerminal CheckTerminalPassword(string userName, string password);

        [OperationContract]
        Deposit GetActiveDeposit(string depositFullNumber);

        [OperationContract]
        C2CTransferResult SaveC2CTransferOrder(C2CTransferOrder request);

        [OperationContract]
        CashTerminal GetTerminal(int terminalID);

        [OperationContract]
        int GetC2CTransferCurrencyCode(string currency);

        [OperationContract]
        C2CTransferStatusResponse GetC2CTransferStatus(long transferID);


        [OperationContract]
        C2CTransferStatusResponse GetC2CTransferStatusByOrderID(long orderID);

        [OperationContract]
        EOGetClientResponse GetClient(EOGetClientRequest request);


        [OperationContract]
        EOTransferResponse MakeTransfer(EOTransferRequest request);


        [OperationContract]
        int GetLastKeyNumber(int keyID);
    }
}
