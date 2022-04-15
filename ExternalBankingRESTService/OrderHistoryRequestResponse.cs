using ExternalBankingRESTService.XBS;
using System.Collections.Generic;


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