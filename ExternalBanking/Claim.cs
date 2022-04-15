using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Դատական գործընթաց
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Գործընթացի ունիկալ համար
        /// </summary>
        public int ClaimNumber { get; set; }
        /// <summary>
        /// Վարկային պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }
        /// <summary>
        /// Դատական գործընթացը մուտքագրողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }
        /// <summary>
        /// Դատական գործի սկիզբ
        /// </summary>
        public DateTime ClaimDate { get; set; }
        /// <summary>
        /// Կարգավիճակ
        /// </summary>
        public short Quality { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string QualityDescription { get; set; }
        /// <summary>
        /// Դատական գործի նպատակ
        /// </summary>
        public short Purpose { get; set; }
        /// <summary>
        /// Նպատակի նկարագրություն
        /// </summary>
        public string PurposeDescription { get; set; }

        public List<ClaimEvent> Events { get; set; }


        public static List<Claim> GetProductClaims(ulong productId)
        {
            return ClaimDB.GetProductClaims(productId);
        }

        /// <summary>
        /// Ստուգոջմ է առկա է դատական հայցադիմում վարկի համար
        /// </summary>
        /// <param name="loanProductId"></param>
        /// <returns></returns>
        public static bool CheckProductHasClaim(ulong loanProductId)
        {
            return ClaimDB.CheckProductHasClaim(loanProductId);
        }

        public static bool IsAparikTexumClaim(ulong loanProductId)
        {
            return ClaimDB.IsAparikTexumClaim(loanProductId);
        }

        public static ActionResult ChangeProblemLoanTaxQuality(ulong taxAppId, User user)
        {
            ActionResult result = new ActionResult();
            if (user.userPermissionId != 505)
            {
                result.Errors.Add(new ActionError(1501, new string[] { "Դուք կարգավիճակը փոփոխելու հասանելիություն չունեք։" }));
                result.ResultCode = ResultCode.Failed;
                return result;
            }
            return ClaimDB.ChangeProblemLoanTaxQuality(taxAppId, user.userID);
        }
    }
}
