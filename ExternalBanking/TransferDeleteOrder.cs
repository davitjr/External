using System;
using System.Collections.Generic;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;

namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class TransferDeleteOrder : Order
    {

        /// <summary>
        /// Ձևակերպվող փոխանցում
        /// </summary>
        public Transfer Transfer { get; set; }

        public new ActionResult Delete(string filialCode, DateTime setDate, SourceType source, short schemaType )
        {
            this.Complete();

            string isCallCenter = null;
            user.AdvancedOptions.TryGetValue("isCallCenter", out isCallCenter);
            ActionResult result = new ActionResult();

            List<short> errors = new List<short>();
            errors = TransferDB.CheckForDelete(this.Transfer.Id, filialCode,  setDate, user, Convert.ToUInt16(isCallCenter));
    
            if (errors.Count != 0)
            {
                //for (int i = 0; i < errors.Count; i++)
                //{
                result.Errors.Add(new ActionError(errors[0]));
                //}
            }


            if (result.Errors.Count != 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }



            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TransferDB.TransferDeleteOrder(this, user.userName, source , Convert.ToUInt16(isCallCenter));

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (this.OPPerson != null)
                {
                    result = base.SaveOrderOPPerson();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
                }

                result = base.SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                //result = base.SaveOrderFee();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                result = base.Approve(schemaType, user.userName );

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
        /// Լրացնում է վճարման հանձնարարականի ավտոմատ լրացվող դաշտերը
        /// </summary>
        protected void Complete()
        {
            this.Transfer.Get();
            this.Type = OrderType.TransferDeleteOrder;
            this.SubType = 1;
            this.RegistrationDate = Convert.ToDateTime(this.OperationDate);
            this.OrderNumber = "1";
            this.Quality = OrderQuality.Draft;

        }


 
    }

}
