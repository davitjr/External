﻿using ExternalBanking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ExternalBankingService.Interfaces
{
    
    [ServiceContract]
    public interface ILoanXBService
    {
    

        [OperationContract]
        bool Init(string authorizedCustomerSessionID, byte language, string clientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        AuthorizedCustomer AuthorizeCustomer(ulong customerNumber);

        [OperationContract]
        ActionResult SaveAndApproveLoanProductActivationOrder(LoanProductActivationOrder order);
    }
}
