
namespace ExternalBanking
{
    public class ArcaBalanceResponseData
    {
        /// <summary>
        /// Մնացորդ
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Վարկային գծի հասանելի մնացորդ
        /// </summary>
        public double AvailableExceedLimit { get; set; }

        /// <summary>
        /// Վարկային գծի հասանելի մնացորդի արժույթ
        /// </summary>
        public string AvailableExceedLimitCurrency { get; set; }

        /// <summary>
        /// Սեփական միջոցներ
        /// </summary>
        public double OwnFunds { get; set; }

        /// <summary>
        /// Սեփական միջոցների մնացորդ
        /// </summary>
        public string OwnFundsCurrency { get; set; }

        /// <summary>
        /// Պատասխանի կոդ
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// Պատասխանի կոդի նկարագրություն
        /// </summary>
        public string ResponseDescription { get; set; }
    }
}
