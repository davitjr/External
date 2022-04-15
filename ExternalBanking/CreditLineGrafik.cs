using System;


namespace ExternalBanking
{
    /// <summary>
    /// Վարկային գծի մարման մանրամասն
    /// </summary>
    public class CreditLineGrafik
    {
        /// <summary>
        /// Մարման ժամանակահատվածի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Մարման ժամանակահատվածի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Մարման ենթակա գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Մարված գումար
        /// </summary>
        public double MaturedAmount { get; set; }
        /// <summary>
        /// Ժամկետանցի նշում
        /// </summary>
        public byte OverdueSign { get; set; }
        /// <summary>
        /// Պլանային մարման ենթակա գումար
        /// </summary>
        public double PlannedAmount { get; set; }
    }
}
