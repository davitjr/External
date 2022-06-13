using ExternalBanking;
using ExternalBanking.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ExternalBankingService.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IEventService" in both code and config file together.
    [ServiceContract]
    public interface IEventService
    {

        [OperationContract]
        void SetUser(AuthorizedCustomer authorizedCustomer, byte language, string ClientIp, ExternalBanking.ACBAServiceReference.User user, SourceType source);

        [OperationContract]
        ActionResult SaveEventTicketOrder(EventTicketOrder order);

        [OperationContract]
        List<Event> GetEventSubTypes(EventTypes eventTypes);

        [OperationContract]
        EventTicketOrder GetEventTicketOrder(long ID);

        [OperationContract]
        ActionResult ApproveEventTicketOrder(EventTicketOrder order);
    }
}
