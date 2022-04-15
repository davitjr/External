using ExternalBanking.DBManager;
using System.Collections.Generic;

namespace ExternalBanking
{
    /// <summary>
    /// Դատական գործընթաց
    /// </summary>
    public class CorrespondentBankAccount
    {
        /// <summary>
        /// Կոդ
        /// </summary>
        public string CodeAccount { get; set; }
        /// <summary>
        /// Բանկի անուն
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// Արժույթ
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Հաշիվ
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// Համակարգ
        /// </summary>
        public short TransferSystem { get; set; }
        /// <summary>
        /// Կարգավիճակի նկարագրություն
        /// </summary>
        public string SwiftCode { get; set; }
        /// <summary>
        /// ԱԿԲԱ-ի փոխանցում
        /// </summary>
        public byte AcbaTransfer { get; set; }




        public static List<CorrespondentBankAccount> GetCorrespondentBankAccounts(CorrespondentBankAccount filter)
        {
            return CorrespondentBankAccountDB.GetCorrespondentBankAccounts(filter);
        }
    }
}
