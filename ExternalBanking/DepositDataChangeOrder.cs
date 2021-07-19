using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Ավանդային տվյալների փոփոխման հայտ
    /// </summary>
    public class DepositDataChangeOrder:Order
    {
        /// <summary>
        /// Ավանդ
        /// </summary>
        public Deposit Deposit { get; set; }


        /// <summary>
        /// Փոփոխվող դաշտի տեսակ(տեսակները նկարագրված են HBBase ի Tbl_deposit_data_change_field_types -ում)
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


        public void Get()
        {
            DepositDataChangeOrderDB.GetDepositDataChangeOrder(this);
            this.OPPerson = OrderDB.GetOrderOPPerson(this.Id);
            this.Deposit = Deposit.GetDeposit(this.Deposit.ProductId, this.CustomerNumber);
        }



        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            switch (this.FieldType)
            {
                case 1:
                    this.FieldValue = this.Deposit.ConnectAccount.AccountNumber;
                    break;
                case 2:
                    this.FieldValue = this.Deposit.ConnectAccountForPercent.AccountNumber;
                    break;
                case 3:
                    this.FieldValue = this.Deposit.InvolvingSetNumber.ToString();
                    break;
                case 4:
                    this.FieldValue = this.Deposit.ServicingSetNumber.ToString();
                    break;
                default:
                    break;
                       
            }

        }


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateDepositDataChangeOrder(this));
            return result;
        }


        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete();
            ActionResult result = this.Validate();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {

                result = DepositDataChangeOrderDB.SaveDepositDataChangeOrder(this, userName);


                ActionResult resultOpPerson = base.SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                LogOrderChange(user, action);
                scope.Complete();
            }
            return result;
        }


        public new ActionResult Approve(short schemaType, string userName)
        {
            ActionResult result = new ActionResult();

            if (result.ResultCode == ResultCode.Normal)
            {
                Action action = this.Id == 0 ? Action.Add : Action.Update;

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
                {
                    OrderQuality nextQuality = GetNextQuality(schemaType);

                    result = base.Approve(schemaType, userName);
                                       
                    if (result.ResultCode == ResultCode.Normal)
                    {
                        LogOrderChange(user, Action.Update);
                        scope.Complete();
                    }
                }
            }

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
            }
            else
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

        public ActionResult SaveAndApprove(string userName, short schemaType)
        {
          
            this.Complete();
            ActionResult result = this.Validate();

            if(result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = DepositDataChangeOrderDB.SaveDepositDataChangeOrder(this, userName);
               
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }
                result = base.SaveOrderOPPerson();
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
    }
}
