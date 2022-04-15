using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// ՀԲ գործարքի ստուգման շրջանցում
    /// </summary>
    public class HBDocumentBypassTransaction
    {
        /// <summary>
        /// Գործարքի կոդ
        /// </summary>
        public long DocID { get; set; }

        /// <summary>
        /// Շրջանցման տեսակներ
        /// </summary>
        public List<HBDocumentsBypass> HBBypass { get; set; }

        /// <summary>
        /// ՊԿ
        /// </summary>
        public short SetNumber { get; set; }

        /// <summary>
        /// Գործարքի կատարման թույլտվություն՝ եթե true => շրջանցման աղյուսակում(Tbl_HB_bypass_history) առկա է տող տվյալ գործարքի համար
        /// </summary>
        public bool AllowApprove { get; set; }


    }
}
