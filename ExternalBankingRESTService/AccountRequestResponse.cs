using ExternalBankingRESTService.XBS;

namespace ExternalBankingRESTService
{
	public class AccountRequestResponse
	{
		public Account Account { get; set; }
		public Result Result { get; set; }

		public AccountRequestResponse()
		{
			Result = new Result();
		}
	}
}