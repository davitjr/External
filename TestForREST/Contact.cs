using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace TestForREST
{
	public class Contact
	{
		public ulong Id { get; set; }
		public string Description { get; set; }
		public List<ContactAccount> ContactAccountList { get; set; }

		public Contact()
		{
			ContactAccountList = new List<ContactAccount>();
		}
	}
}
