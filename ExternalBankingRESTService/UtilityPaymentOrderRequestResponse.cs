using ExternalBankingRESTService.XBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalBankingRESTService
{
	public class UtilityPaymentOrderRequestResponse
	{
		public UtilityPaymentOrder PaymentOrder { get; set; }
		public Result Result { get; set; }

		public UtilityPaymentOrderRequestResponse()
		{
			Result = new Result();
		}

	}
}