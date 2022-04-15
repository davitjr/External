using ExternalBanking.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class Borrower
    {
        public ulong CustomerNumber { get; set; }
        public string FullName { get; set; }
        public ulong AgreementId { get; set; } = 0;
        public bool AgreementExistence { get; set; } = false;

        public static List<Borrower> GetLoanBorrowers(ulong productId) => BorrowerDB.GetLoanBorrowers(productId);

        public static ActionResult SaveTaxRefundAgreementDetails(ulong customerNumber, ulong productId, byte agreementExistence, int setNumber)
            => BorrowerDB.SaveTaxRefundAgreementDetails(customerNumber, productId, agreementExistence, setNumber);

        public static List<ChangeDetails> GetTaxRefundAgreementHistory(int agreementId) => BorrowerDB.GetTaxRefundAgreementHistory(agreementId);

    }
}
