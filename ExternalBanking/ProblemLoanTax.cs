using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class ProblemLoanTax
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
        /// Պետ. տուրք ունեցող խնդրահարույց վարկի մասնաճյուղի անվանում
        /// </summary>
        public string FilialCodeName { get; set; }

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
        /// Վարկի ունիկալ համար
        /// </summary>
        public ulong AppId { get; set; }

        /// <summary>
        /// Վարկի տեսակ
        /// </summary>
        public short LoanType { get; set; }

        /// <summary>
        /// Վարկի տեսակ Նկարագրություն
        /// </summary>
        public string LoanTypeDescription { get; set; }

        /// <summary>
        /// Պետ. տուրքի գրանցման ամսաթիվ
        /// </summary>
        public DateTime TaxRegistrationDate { get; set; }

        /// <summary>
        ///  Պետ. տուրքի գրանցման ժամ
        /// </summary>
        public DateTime TaxRegistrationTime { get; set; }

        /// <summary>
        ///  Պետ. տուրքի գումար
        /// </summary>
        public decimal TaxAmount { get; set; }      

        /// <summary>
        /// Պետ. տուրքի փոխանցման ամսաթիվ
        /// </summary>
        public DateTime? TransferRegistrationDate { get; set; }    

        /// <summary>
        /// Պետ. տուրքի փոխանցման համար
        /// </summary>
        public int? TransferUnicNumber { get; set; }

        /// <summary>
        /// Պետ. տուրքի փոխանցողի ՊԿ
        /// </summary>
        public short? RegistrationSetNumber { get; set; }

        /// <summary>
        /// Պետ. տուրքի փոխանցման հաստատման ամսաթիվ
        /// </summary>
        public DateTime? ConfirmationDate { get; set; }

        /// <summary>
        /// Պետ. տուրքի փոխանցումը հաստատողի ՊԿ
        /// </summary>
        public short? ConfirmationSetNumber { get; set; }

        /// <summary>
        /// Պետ. տուրքի կարգավիճակ
        /// </summary>
        public TaxQuality TaxQuality { get; set; }

        /// <summary>
        /// Պետ. տուրքի կարգավիճակի նկարագրություն
        /// </summary>
        public string TaxQualityDescription { get; set; }

        /// <summary>
        /// Համաձայն դատարանի որոշման ում կողմից է մարվում պետ. տուրքը
        /// </summary>
        public TaxCourtDecision TaxCourtDecision { get; set; }
        
        /// <summary>
        /// Պետ. տուրքի վարկի ունիկալ համար
        /// </summary>
        public long ClaimNumber { get; set; }

        /// <summary>
        /// Խնդրահարույց վարկի պետ. տուրքի տվյալներ
        /// </summary>
        /// <param name="ClaimNumber">Պետ.տուրքի ունիկալ համար</param>
        /// <returns></returns>
        public static ProblemLoanTax GetProblemLoanTaxDetails(long ClaimNumber)
        {
            ProblemLoanTax problemLoanTax = ProblemLoanTaxDB.GetProblemLoanTaxDetails(ClaimNumber);
            Dictionary<string, string> FilialList = Info.GetFilialList(Languages.hy);
            problemLoanTax.FilialCodeName = FilialList[problemLoanTax.FilialCode.ToString()];
            return problemLoanTax;
        }
        
    }
}
