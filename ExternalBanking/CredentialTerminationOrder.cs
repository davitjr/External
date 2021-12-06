using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.DBManager;
using System.Transactions;

namespace ExternalBanking
{
    public class CredentialTerminationOrder : Order
    {
        /// <summary>
        /// Լիազորագրի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Լիազորագրի վերջ
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Լիազորագրի տեսակ
        /// </summary>
        public ushort CredentialType { get; set; }
                
        /// <summary>
        ///  Փակման պատճառ
        /// </summary>
        public ushort ClosingReasonType { get; set; }

        /// <summary>
        ///  Փակման ա/թ
        /// </summary>
        public DateTime? ClosingDate { get; set; }

        /// <summary>
        /// Լիազորագիրը փակողի ՊԿ
        /// </summary>
        public int ClosingSetNumber { get; set; }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            this.Complete();
            ActionResult result = this.Validate(user);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }            

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CredentialOrderDB.SaveCredentialTerminationOrder(this, userName, source);
                //**********                
                //ulong orderId = base.Save(this, source, user);
                ulong orderId = 0;
                result = CredentialOrderDB.SaveCredentialTerminationOrderDetails(this, this.Id);
                //Order.SaveLinkHBDocumentOrder(this.Id, orderId);
                //BOOrderProduct.Save(this, (ulong)this.Id);
                //ActionResult res = BOOrderCustomer.Save(this, (ulong)this.Id, user);
                //**********
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                result = base.SaveOrderOPPerson();                

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);


                LogOrderChange(user, action);

                result = base.Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    this.Quality = OrderQuality.Sent3;
                    base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                    scope.Complete();
                }
            }
            result = base.Confirm(user);
            return result;
        }
        
        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.SubType = 1;

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        /// <summary>
        /// Լիազորագրի դադարեցման հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateCredentialTerminationOrder(this, user));
            return result;
        }

        /// <summary>
        /// Լիազորագրի փակման վերաբերյալ զգուշացումների վերադարձ
        /// </summary>
        /// <param name="assignId"></param>
        /// <returns></returns>
        public static List<string> GetCredentialClosingWarnings(ulong assignId, Culture culture)
        {
            Int16 res = CredentialOrderDB.CheckCredentialTodos(assignId);
            List<string> warnings = new List<string>();

            if (res == 1)
            {
                warnings.Add(Info.GetTerm(1015, new string[] { }, culture.Language));
            }           
            
            return warnings;
        }
    }
}
