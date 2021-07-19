

namespace ExternalBanking
{
    /// <summary>
    /// Ավանդի օպցիաներ
    /// </summary>
    public class DepositOption
    {
        /// <summary>
        /// Օպցիաի տեսակ
        /// </summary>
        public ushort Type { get; set; }

        /// <summary>
        /// Օպցիայի տեսակի նկարագրություն
        /// </summary>
        public string TypeDescription { get; set; }

        /// <summary>
        /// Տոկոսադրույք
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// Օպցիաների գրուպան
        /// </summary>
        public ushort OptionGroup { get; set; }
    }
}
