using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Լիազորված անձի նույնականացման հայտ
    /// </summary>
    public class AssigneeIdentificationOrder : Order
    {
        /// <summary>
        /// Լիազորագիր
        /// </summary>
        public Credential Credential { get; set; }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            return result;
        }

        private void Complete()
        {
            if (String.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            this.SubType = 1;

        }


        /// <summary>
        /// Լիազորված անձի նույնականացման հայտի պահպանում և ուղարկում 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                result = CredentialOrderDB.SaveAssigneeIdentificationOrder(this, userName);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                result = base.SaveOrderOPPerson();
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

        /// <summary>
        /// Վերադարձնում է լիազորված անձի նույնականացման հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            List<Credential> credentials = Credential.GetCustomerCredentialsList(this.CustomerNumber, ProductQualityFilter.Opened);

            DataTable dt = Credential.GetAssigneeIdentificationOrderByDocId(this.Id);

            ulong credentialId = 0;
            ulong assigneeCustomerNumber = 0;

            if (dt.Rows.Count > 0)
            {
                credentialId = Convert.ToUInt64(dt.Rows[0]["assign_id"].ToString());
                assigneeCustomerNumber = Convert.ToUInt64(dt.Rows[0]["assignee_customer_number"].ToString());
            }


            this.Credential = credentials.Find(m => m.Id == credentialId);

            this.Credential.AssigneeList[0].CustomerNumber = assigneeCustomerNumber;
        }

    }


}
