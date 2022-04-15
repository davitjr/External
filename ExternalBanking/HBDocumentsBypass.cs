namespace ExternalBanking
{
    /// <summary>
    /// Գործարքի շրջանցման տեսակ
    /// </summary>
    public class HBDocumentsBypass
    {
        /// <summary>
        /// շրջանցման համար
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// շրջանցման նկարագրություն
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// true =շրջանցել
        /// </summary>
        public bool IsChecked { get; set; } = false;
    }
}
