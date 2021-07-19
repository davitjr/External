using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Ելքային գործարքների տարանցիկ հաշիվ
    /// </summary>
    public class TransitAccountForDebitTransactions
    {
        /// <summary>
        /// Տարանցիկ հաշվի ունիկալ համար
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Տարանցիկ հաշիվ
        /// </summary>
        public Account TransitAccount { get; set; }

        /// <summary>
        /// Հաշվի նկարագրություն
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Բացման ա/թ
        /// </summary>
        public DateTime OpenDate { get; set; }

        /// <summary>
        /// Փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Մասնաճյուղ
        /// </summary>
        public ushort FilialCode { get; set; } 

        /// <summary>
        /// Պահանջում է հաստատում հաշիվը թե ոչ
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Միջնորդավճարի տոկոսադրույք
        /// </summary>
        public double FeeRate { get; set; }

        /// <summary>
        /// Միջնորդավճարի նվազագույն գումար
        /// </summary>
        public double MinFeeAmount { get; set; }

        /// <summary>
        /// Հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Հաճախորդի տարանցիկ հաշիվ է թե ոչ
        /// </summary>
        public bool IsCustomerTransitAccount { get; set; }

        /// <summary>
        /// Մուտքագրողի ՊԿ
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Բոլոր Մ/Ճ-ների համար
        /// </summary>
        public bool ForAllBranches { get; set; }


        private void Complete()
        {
            if (this.IsCustomerTransitAccount)
            {
                if(this.TransitAccount!=null && !string.IsNullOrEmpty(this.TransitAccount.AccountNumber))
                {
                    Account account = Account.GetSystemAccount(this.TransitAccount.AccountNumber);
                    this.FilialCode = (ushort)account.FilialCode;
                }

            }
        }

        /// <summary>
        /// Ելքային գործարքների տարանցիկ հաշիվ պահպանում
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveTransitAccountForDebitTransactions(ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate(user);

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                TransitAccountForDebitTransactionsDB.SaveTransitAccountForDebitTransactions(this,user.userID);
                scope.Complete();
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }

        /// <summary>
        /// Ելքային գործարքների տարանցիկ հաշվի խմբագրում 
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateTransitAccountForDebitTransactions()
        {
            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                TransitAccountForDebitTransactionsDB.UpdateTransitAccountForDebitTransactions(this);
                scope.Complete();
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }


        public ActionResult CloseTransitAccountForDebitTransactions()
        {
            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                TransitAccountForDebitTransactionsDB.CloseTransitAccountForDebitTransactions(this);
                scope.Complete();
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }

        public ActionResult ReopenTransitAccountForDebitTransactions()
        {
            ActionResult result = new ActionResult();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                TransitAccountForDebitTransactionsDB.ReopenTransitAccountForDebitTransactions(this);
                scope.Complete();
                result.ResultCode = ResultCode.Normal;
            }
            return result;
        }

        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateTransitAccountForDebitTransactions(this, user));
            return result;
        }

        /// <summary>
        /// Վերադարձնում է տարանցիկ հաշիվը կախված մ/ճ ից և հաշվեհամարից
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="filialCode"></param>
        /// <returns></returns>
        public static TransitAccountForDebitTransactions GetTransitAccountsForDebitTransaction(string accountNumber, ushort filialCode)
        {
            return TransitAccountForDebitTransactionsDB.GetTransitAccountsForDebitTransaction(accountNumber, filialCode);
        }

        /// <summary>
        /// Վերադարձնում է տարամցիկ հաշիվները
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static List<TransitAccountForDebitTransactions> GetTransitAccountsForDebitTransactions(ProductQualityFilter quality)
        {
            List<TransitAccountForDebitTransactions> accounts = new List<TransitAccountForDebitTransactions>();

            if (quality == ProductQualityFilter.Opened || quality == ProductQualityFilter.NotSet)
            {
                accounts.AddRange(TransitAccountForDebitTransactionsDB.GetTransitAccountsForDebitTransactions());
            }
            if (quality == ProductQualityFilter.Closed)
            {
                accounts.AddRange(TransitAccountForDebitTransactionsDB.GetClosedTransitAccountsForDebitTransactions());
            }
            if (quality == ProductQualityFilter.All)
            {
                accounts.AddRange(TransitAccountForDebitTransactionsDB.GetTransitAccountsForDebitTransactions());
                accounts.AddRange(TransitAccountForDebitTransactionsDB.GetClosedTransitAccountsForDebitTransactions());
            }
            return accounts;
        }

    }
}
