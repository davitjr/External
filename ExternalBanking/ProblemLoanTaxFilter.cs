using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class ProblemLoanTaxFilter
    {
        /// <summary>
        /// Պետ. տուրքի շարք
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Պետ. տուրք ունեցող խնդրահարույց վարկի մասնաճյուղ
        /// </summary>
        public short FilialCode { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի անվանում
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Վարկային հաշիվ
        /// </summary>
        public ulong LoanFullNumber { get; set; }

        /// <summary>
        /// Պետ. տուրքի գրանցման ամսաթիվ սկիզբ
        /// </summary>
        public DateTime TaxRegistrationStartDate { get; set; }

        /// <summary>
        /// Պետ. տուրքի գրանցման ամսաթիվ ավարտ
        /// </summary>
        public DateTime TaxRegistrationEndDate { get; set; }

        /// <summary>
        ///  Պետ. տուրքի գումար
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Պետ. տուրքի կարգավիճակ
        /// </summary>
        public TaxQuality TaxQuality { get; set; }

        /// <summary>
        /// Համաձայն դատարանի որոշման ում կողմից է մարվում պետ. տուրքը
        /// </summary>
        public TaxCourtDecision TaxCourtDecision { get; set; }

        /// <summary>
        /// Պետ. տուրքի փոխանցման ամսաթիվ կա թե ոչ
        /// </summary>
        public IsTransferRegistratoinDateExists IsTransferRegistratoinDateExists { get; set; }

        public static Dictionary<int, List<ProblemLoanTax>> SearchProblemLoanTax(ProblemLoanTaxFilter problemLoanTaxFilter, bool Cache)
        {
            Dictionary<int, List<ProblemLoanTax>> problemLoanTax = ProblemLoanTaxFilterDB.SearchProblemLoanTax(problemLoanTaxFilter, Cache);
            return problemLoanTax;
        }
    }
}
