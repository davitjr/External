using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.XBManagement
{
    public class LogonInfo
    {
        /// <summary>
        /// Հաջողված մուտքերի քանակ
        /// </summary>
        public int GoodLogons { get; set; }
        /// <summary>
        /// Չհաջողված մուտքերի քանակ
        /// </summary>
        public int BadLogons { get; set; }
        /// <summary>
        /// Վերջին հաջողված մուտքի ամսաթիվ
        /// </summary>
        public DateTime? LastGoodLogonDate { get; set; }
        /// <summary>
        /// Վերջին չհաջողված մուտքի փորձի ամսաթիվ
        /// </summary>
        public DateTime? LastBadLogonDate { get; set; }
        /// <summary>
        /// Սխալ մուտքերի հաշվիչ
        /// </summary>
        public short BadResetCounter { get; set; }
    }
}
