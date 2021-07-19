using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ExternalBanking;
using ExternalBankingService.Interfaces;

namespace ExternalBankingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "LeasingXBService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select LeasingXBService.svc or LeasingXBService.svc.cs at the Solution Explorer and start debugging.
    public class LeasingXBService : ILeasingXBService
    {

        public string GetAccountsForLeasing(ulong CustomerNumber)
        {
            try
            {
                Customer customer = new Customer();
                return customer.GetAccountsForLeasing(CustomerNumber);
            }
            catch (Exception ex)
            {
                //WriteLog(ex);
                throw new FaultException(Resourse.InternalError);
            }
        }
    }
}
