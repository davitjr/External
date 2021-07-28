
namespace ExternalBanking
{
    /// <summary>
    /// Լրացուցիչ տվյալներ
    /// </summary>
    public class AdditionalDetails
    {
        
        /// <summary>
        /// Ունիկալ համար (Id)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Տեսակ
        /// </summary>
        public ushort AdditionType { get; set; }

        /// <summary>
        /// Տեսակի նկարագրություն
        /// </summary>
        public string AdditionTypeDescription { get; set; }

        /// <summary>
        /// Արժեք
        /// </summary>
        public string AdditionValue { get; set; }

        /// <summary>
        /// Արժեքի տեսակ
        /// </summary>
        public AdditionalValueType AdditionalValueType { get; set; }
        
    }
}
