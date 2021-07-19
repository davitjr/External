using System.Collections.Generic;
using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
	public class AccountsRequestResponse
	{

	  public List<Account> Accounts { get; set;}
      public Result Result { get; set; }

      public AccountsRequestResponse()
      {
          Result = new Result();
      }

	}
}