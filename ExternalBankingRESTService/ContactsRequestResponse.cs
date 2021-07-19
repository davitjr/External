using ExternalBankingRESTService.XBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExternalBankingRESTService
{
	public class ContactsRequestResponse
	{
		public List<Contact> Contacts { get; set; }
		public Result Result { get; set; }

		public ContactsRequestResponse()
		{
			Result = new Result();
		}
	}
}