using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;

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
        /// Վերադարձնում է հաճախորդի՝ Բանկի բազայում առկա արժեթղթերի հաշիվը
        /// </summary>
        /// <param name="customerNumber"></param>
        public static DepositaryAccount GetCustomerDepositaryAccount(ulong customerNumber, ref bool hasAccount)
        {
            DepositaryAccount account = DepositaryAccountDB.GetCustomerDepositaryAccount(customerNumber, ref hasAccount);
            if(account.ID == 0)
            {
                account = null;
            }
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
    }
}
