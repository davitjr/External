using System;
using System.Collections.Generic;
using System.Transactions;
using ExternalBanking.DBManager;

namespace ExternalBanking
{
    /// <summary>
    /// Վաղաժամկետ մարման վճարման փոփոխման հայտ
    /// </summary>
    public class LoanProductDataChangeOrder : Order
    {

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public long ProductAppId { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(տեսակները նկարագրված են HBBase ի Tbl_loan_product_data_change_field_types -ում)
        /// </summary>
        public short FieldType { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի տեսակի նկարագրություն
        /// </summary>
        public string FieldTypeDescription { get; set; }

        /// <summary>
        /// Փոփոխվող դաշտի արժեք
        /// </summary>
        public string FieldValue { get; set; }

        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete();
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }


            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = LoanProductDataChangeOrderDB.SaveLoanProductDataChangeOrder(this, userName, source);
               
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

        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
        }


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateLoanProductDataChangeOrder(this));
            return result;
        }



        public void Get()
        {
            LoanProductDataChangeOrderDB.GetLoanProductDataChangeOrder(this);
        }

        public static bool ExistsLoanProductDataChange(ulong appid)
        {
            return LoanProductDataChangeOrderDB.ExistsLoanProductDataChange(appid);
        }

    }
}
