namespace ExternalBanking
{
    public class GoodsDetails
    {

        /// <summary>
        /// Ապրանքի գումար
        /// </summary>
        public double GoodAmount { get; set; }

        /// <summary>
        /// Կանխավճար(գումար)
        /// </summary>
        public double GoodPrepaid { get; set; }

        /// <summary>
        /// Կանխավճար(%)
        /// </summary>
        public float PrepaidPercent { get; set; }
        /// <summary>
        /// Ապրանքի անվանում
        /// </summary>
        public string GoodName { get; set; }

        /// <summary>
        /// Խանութի անվանում
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// Ապրանքների ընդհանուր գումար
        /// </summary>
        public double TotalGoodAmount { get; set; }

        /// <summary>
        /// Ապրանքի քանակ
        /// </summary>
        public int GoodCount { get; set; }

        /// <summary>
        /// Առանց վարկավորման գին
        /// </summary>
        public double CashPrice { get; set; }

        /// <summary>
        /// Ապրանքի գտնվելու վայր
        /// </summary>
        public string GoodAddress { get; set; }


    }
}
