using ExternalBankingRESTService.XBS;


namespace ExternalBankingRESTService
{
    public class MembershipRewardsRequestResponse
    {
        public MembershipRewards MembershipRewards { get; set; }
        public Result Result { get; set; }

        public MembershipRewardsRequestResponse()
        {
            Result = new Result();
        }
    }
}