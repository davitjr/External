
namespace ExternalBanking
{
    /// <summary>
    /// Հաշվի շարժի տվյալներ
    /// </summary>
    public class AccountFlowDetails
    {
        /// <summary>
        /// Հաշվի մնացորդը դրամով
        /// </summary>
        public double BallanceAMD { get; set; }

        /// <summary>
        /// Հաշվի մնացորդը արտարժույթով
        /// </summary>
        public double BallanceInCurrency { get; set; }

        /// <summary>
        /// Դեբետ դրամով
        /// </summary>
        public double DebitAmountAMD { get; set; }

        /// <summary>
        /// Դեբետ արտարժուկթով
        /// </summary>
        public double DebitInCurrency { get; set; }

        /// <summary>
        /// Կրեդիտ դրամով
        /// </summary>
        public double CreditAmountAMD { get; set; }

        /// <summary>
        /// Կրեդիտ արտարժույթով
        /// </summary>
        public double CreditInCurrency { get; set; }

        /// <summary>
        /// Սկզբնական մնացորդ դրամով
        /// </summary>
        public double InitiativeBallanceAMD { get; set; }

        /// <summary>
        /// Սկզբնական մնացորդ արտարժույթով
        /// </summary>
        public double InitiativeBallanceCurrency { get; set; }


    }
}
