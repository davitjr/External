using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExternalBankingRESTService.XBS;


namespace ExternalBankingRESTService
{
    public class OrderHistoryRequestResponse
    {
        public List<OrderHistory> OrderHistory { get; set; }

        public Result Result { get; set; }

        public OrderHistoryRequestResponse()
        {
            Result = new Result();
        }
    }
}