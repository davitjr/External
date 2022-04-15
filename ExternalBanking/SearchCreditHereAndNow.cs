namespace ExternalBanking
{
    public class SearchCreditHereAndNow : SearchParams
    {
        /// <summary>
        /// Խանութի մասնաճյուղ
        /// </summary>
        public int ShopFilial { get; set; }
        /// <summary>
        /// Պրոդուկտի ֆիլտր
        /// </summary>
        public ProductQualityFilter QualityFilter { get; set; }

    }
}