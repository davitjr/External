using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;

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