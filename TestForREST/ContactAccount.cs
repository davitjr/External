using System.Collections.Generic;
using System.Data.SqlClient;

namespace TestForREST
{
	public class ContactAccount
	{
		public ulong Id { get; set;}
		public string Description { get; set;}
		public string AccountNumber { get; set; }
	}
}