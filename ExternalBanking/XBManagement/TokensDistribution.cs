using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking.XBManagement
{
    public class TokensDistribution
    {
        /// <summary>
        /// Վերադարձնում է ընտրված միջակայքի մասնաճյուղի ազատ տոկեները
        /// </summary>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public static List<string> GetTokenNumbersByRangeAndFilial(string from, string to, int filial)
        {
            return TokensDistributionDB.GetUnusedTokensByFilialAndRange(from, to, filial);
        }

        public static void MoveUnusedTokens(int filialToMove, List<string> unusedTokens)
        {
            TokensDistributionDB.MoveUnusedTokens(filialToMove, unusedTokens);
        }
    }
}
