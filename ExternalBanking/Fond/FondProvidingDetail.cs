using System;

namespace ExternalBanking
{ /// <summary>
  /// Ֆոնդի միջոցների տրամադրման պայման
  /// </summary>
    public class FondProvidingDetail
    {
        /// <summary>
        /// Ֆոնդի միջոցների տրամադրման պայմանի ունիկալ համար
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Ֆոնդի համար
        /// </summary>
        public int FondID { get; set; }

        /// <summary>
        /// Տրմադրման արժույթ
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Տրմադրման տոկոսադրույք
        /// </summary>
        public float InterestRate { get; set; }

        /// <summary>
        /// Տրմադրման դադարեցում
        /// </summary>
        public DateTime? TerminationDate { get; set; }

        public FondProvidingDetail()
        {

        }


    }
}
