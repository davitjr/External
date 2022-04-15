using ExternalBankingRESTService.XBS;
using System.Collections.Generic;

namespace ExternalBankingRESTService
{
    public class OrdersRequestResponse
    {
        public List<Order> Orders { get; set; }
        public Result Result { get; set; }

        public OrdersRequestResponse()
        {
            Result = new Result();
        }

    }
}