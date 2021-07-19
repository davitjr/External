using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class BondQualityUpdateOrder : Order
    {
        /// <summary>
        /// Հեռացվող պարտատոմսի Id
        /// </summary>
        public int BondId { get; set;}

        /// <summary>
        /// Մերժման պատճառի կոդ
        /// </summary>
        public BondRejectReason ReasonId { get; set; }

        /// <summary>
        /// Մերժման պատճառի նկարագրություն
        /// </summary>
        public string ReasonDescription { get; set; }


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
                result = BondQualityUpdateOrderDB.SaveBondQualityUpdateOrder(this, userName);
               
                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
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
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            if(this.SubType == 3)
            {
                if(this.ReasonId != BondRejectReason.Other)
                {
                    this.ReasonDescription = null;
                }
            }
        }


        public ActionResult Validate()
        {

            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateBondQualityUpdateOrder(this));


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }
            else
            {
                result.ResultCode = ResultCode.ValidationError;
            }

            return result;
        }


        public void GetBondQualityUpdateOrder()
        {
            BondQualityUpdateOrderDB.GetBondQualityUpdateOrder(this);
        }

        public static bool ExistsNotConfirmedBondQualityUpdateOrder(ulong customerNumber, byte subType, int bondId)
        {
            return BondQualityUpdateOrderDB.ExistsNotConfirmedBondQualityUpdateOrder(customerNumber, subType, bondId);
        }

    }
}
