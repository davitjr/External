using System;

namespace ExternalBanking
{
    public class GroupTemplateShrotInfo
    {
        /// <summary>
        /// Ստացողի հաշվեհամար
        /// </summary>
        public string ReceiverAccount { get; set; }
        /// <summary>
        /// Ստացողի Անուն ազգանուն / անվանում
        /// </summary>
        public string ReceiverName { get; set; }
        /// <summary>
        /// Ելքագրվող հաշվեհամար
        /// </summary>
        public string DebitAccount { get; set; }
        /// <summary>
        /// միջնորդավճարի հաշվեհամար
        /// </summary>
        public string FeeAccount { get; set; }
        /// <summary>
        /// Վճարման ենթակա գումար
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// բաժանորդի համար / պայմանագրի համար
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// Պարտք/Գերավճար
        /// </summary>
        public double Debt { get; set; }
        /// <summary>
        /// Սպասարկման գումար
        /// </summary>
        public double FeeDebt { get; set; }
        /// <summary>
        /// Վարկի տեսակ
        /// </summary>
        public short LoanType { get; set; }
        /// <summary>
        /// Սկզբնական գումար
        /// </summary>
        public ulong LoanAppId { get; set; }
        /// <summary>
        /// փոխարկված գումար
        /// </summary>
        public double ConvertationAmount { get; set; }
        /// <summary>
        /// փոխարկման կուրս
        /// </summary>
        public double Rate { get; set; }
        /// <summary>
        /// վարկի նախնական գումար
        /// </summary>
        public double LoanInitialAmount { get; set; }
        /// <summary>
        /// վարկի հերթական վճարման օր
        /// </summary>
        public DateTime LoanNextRepayment { get; set; }
        /// <summary>
        /// Խմբի համար
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// Արտարժույթային վարկի տոկոսագումարի հաշիվ
        /// </summary>
        public string PercentAccount { get; set; }
        /// <summary>
        /// Արտարժութային վարկի տոկոսագումար
        /// </summary>
        public double RateAmount { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// կոմունալի տեսակ
        /// </summary>
        public CommunalTypes CommunalType { get; set; } = CommunalTypes.None;

    }
}
