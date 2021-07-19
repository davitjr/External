using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class LoanInterestRateConcessionOrder : Order
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductAppId { get; set; }

        /// <summary>
        /// Տոկոսագումարի զիջման ամսաթիվ
        /// </summary>
        public DateTime ConcessionDate { get; set; }

        /// <summary>
        /// Տոկոսագումարի զիջման ամիսների քանակ
        /// </summary>
        public int NumberOfMonths { get; set; }
        /// <summary>
        /// Վարկի տրման մասնաճյուղ
        /// </summary>
        public int LoanFilialCode { get; set; }

        /// <summary>
        /// Սպասարկման վճար
        /// </summary>
        public double CurrentFee { get; set; }

        /// <summary>
        /// Կուտակված տոկոսագումար
        /// </summary>
        public double CurrentRateValue { get; set; }

        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }

        public ActionResult Validate(ACBAServiceReference.User user)
        {
            ActionResult result = new ActionResult();
            LoanInterestRateConcessionOrder concessionOrder = LoanInterestRateConcessionOrderDB.GetLoanInterestRateConcessionDetailsDB(ProductAppId);
            if(concessionOrder.Quality == (OrderQuality)3 || concessionOrder.Quality == (OrderQuality)30)
            {
                ActionError actionError = new ActionError
                {
                    Code = 0,
                    Description = "Տվյալ վարկի համար գործողության հայտ արդեն գոյություն ունի"
                };
                result.Errors.Add(actionError);
            }

            //Ուրիշ մասնաճյուղից վարկի զիջումն արգելող ստուգում
            concessionOrder = LoanInterestRateConcessionOrderDB.GetLoanDetailsForValidation(ProductAppId);
            if (concessionOrder.LoanFilialCode != user.filialCode)
            {
                ActionError actionError = new ActionError
                {
                    Code = 0,
                    Description = "Օգտատերի և վարկի մասնաճյուղերը տարբեր են։"
                };
                result.Errors.Add(actionError);
            }
            if (concessionOrder.CurrentRateValue != 0 || concessionOrder.CurrentFee != 0)
            {
                ActionError actionError = new ActionError
                {
                    Code = 0,
                    Description = "Անտոկոս ժամանակահատվածի կիրառումը հնարավոր չէ. առկա է կուտակված տոկոսագումար կամ սպասարկման վարձ։"
                };
                result.Errors.Add(actionError);
            }
            return result;
        }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = this.Validate(user);
           // List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = LoanInterestRateConcessionOrderDB.SaveLoanConcessionOrder(this, userName, source);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                result = base.SaveOrderOPPerson();
                result = base.SaveOrderFee();
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

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
                else
                {
                    return result;
                }
            }

            ActionResult resultConfirm = base.Confirm(user);

            return resultConfirm;
        }

        public static LoanInterestRateConcessionOrder GetLoanInterestRateConcessionDetails(ulong productId)
        {
            return LoanInterestRateConcessionOrderDB.GetLoanInterestRateConcessionDetailsDB(productId);
        }
        public void GetLoanInterestRateConcessionOrder()
        {
            LoanInterestRateConcessionOrderDB.GetLoanInterestRateConcessionOrder(this);
        }
    }
}
