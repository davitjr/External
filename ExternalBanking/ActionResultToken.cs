using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
	class ActionResultToken:ActionResult
	{
		public int ErrorResponsecode { get; set; }
		public string Description { get; set; }
	}
}
