using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ACBAServiceReference;
using ExternalBanking.Interfaces;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    public class ClassifiedLoanActionOrders : PreOrder
    {
        /// <summary>
        /// Դասակարգված վարկերի դուրսգրման կամ հետ դասակարգման հայտերի նախնական հայտի մանրամասներ
        /// </summary>
        public List<PreOrderDetails> ClassifiedLoanActionDetails { get; set; }
        /// <summary>
        /// Գործողության տեսակ
        /// </summary>
        public ClassifiedLoanActionType ActionType { get; set; }
        /// <summary>
        /// Ստուգում 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateClassifiedLoanActionOrders(this));
            return result;
        }
        /// <summary>
        /// Հայտի պահպանում և կատարում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="user"></param>
        /// <param name="source"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, ACBAServiceReference.User user, SourceType source, short schemaType)
        {
            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = ClassifiedLoanDB.SaveClassifiedLoanActionPreOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    scope.Complete();
                }
            }

            return result;
        }


    }
}
