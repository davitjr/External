using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;

namespace ExternalBanking
{
    public class DepositaryAccount
    {

        /// <summary>
        /// Արժեթղթերի հաշվի ունիկալ համար
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Արժեթղթերի հաշիվ
        /// </summary>
        public double AccountNumber { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշիվը հաշվառող անձի կոդը
        /// </summary>
        public int BankCode { get; set; }

        /// <summary>
        /// Արժեթղթերի հաշիվը հաշվառող անձի անվանում
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Գրանցման օր
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Գրանցողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Եթե դրական ապա դիմում ենք BondManagementApi
        /// </summary>
        public bool IsOpeningAccInDepo { get; set; }

        /// <summary>
        /// "N" - Normal , "Y" - Suspended
        /// </summary>
        public string Status { get; set; }

        public string StockIncomeAccountNumber { get; set; }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ Բանկի բազայում առկա արժեթղթերի հաշիվը
        /// </summary>
        /// <param name="customerNumber"></param>
        public static DepositaryAccount GetCustomerDepositaryAccount(ulong customerNumber, ref bool hasAccount)
        {
            DepositaryAccount account = DepositaryAccountDB.GetCustomerDepositaryAccount(customerNumber, ref hasAccount);
            if (account.ID == 0)
            {
                account = null;
            }
            return account;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ Բանկի բազայում առկա արժեթղթերի հաշիվը
        /// </summary>
        /// <param name="customerNumber"></param>
        public static List<DepositaryAccount> GetCustomerDepositaryAccounts(ulong customerNumber)
        {
            List<DepositaryAccount> account = new List<DepositaryAccount>();
            account = DepositaryAccountDB.GetCustomerDepositaryAccounts(customerNumber);
            return account;
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի՝ Բանկի բազայում առկա արժեթղթերի հաշիվը
        /// </summary>
        /// <param name="customerNumber"></param>
        public static bool HasCustomerDepositaryAccount(ulong customerNumber)
        {
            bool hasAccount = false;
            DepositaryAccount account = GetCustomerDepositaryAccount(customerNumber, ref hasAccount);
            return hasAccount;

        }


        public static DepositaryAccount GetDepositaryAccountById(int id)
        {
            return DepositaryAccountDB.GetDepositaryAccountById(id);
        }


        public static void DeleteDepoAccounts(ulong customerNumber)
        {
            DepositaryAccountDB.DeleteDepoAccounts(customerNumber);
        }

        public static double GetDepositaryAccount(ulong customerNumber)
        {
            return DepositaryAccountDB.GetDepositaryAccount(customerNumber);
        }
    }
}
