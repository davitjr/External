using ExternalBanking;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using infsec = ExternalBankingService.InfSecServiceReference;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IXBSForAPS" in both code and config file together.
    [ServiceContract]
    public interface IXBAutomatedProcessesService
    {
        [OperationContract]
        ActionResult GenerateAndMakeSwiftMessagesByPeriodicTransfer(DateTime statementDate, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        void Init(string clientIp, ExternalBanking.ACBAServiceReference.User user);

        [OperationContract]
        infsec.AuthorizedUser AuthorizeUserBySessionToken(string authorizedUserSessionToken);

        [OperationContract]
        ACBALibrary.User InitUser(infsec.AuthorizedUser authUser);

        [OperationContract]
        infsec.UserAccessForCustomer GetUserAccessForCustomer(string userSessiobToken, string customerSessionToken);

        [OperationContract]
        List<ActionResult> SaveAndApproveClassifiedLoanActionOrders(SearchClassifiedLoan searchParameters);



    }
}
