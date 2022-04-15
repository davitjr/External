using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{

    /// <summary>
    /// Միջազգային վճարման հանձնարարական
    /// </summary>
    public class TransferByCallChangeOrder : Order
    {

        public string ReasonTypeDescription { get; set; }
        public short ReasonType { get; set; }

        /// <summary>
        /// Ձևակերպվող փոխանցում
        /// </summary>
        public ReceivedFastTransferPaymentOrder ReceivedFastTransfer { get; set; }

        public new ActionResult SaveAndApprove(SourceType source, short schemaType)
        {
            this.Complete();


            ActionResult result = new ActionResult();

            List<short> errors = new List<short>();
            string isCallCenter = null;
            user.AdvancedOptions.TryGetValue("isCallCenter", out isCallCenter);
            if (this.SubType == 5 || this.SubType == 1)
            {
                this.ReceivedFastTransfer.user = this.user;
                result.Errors = this.ReceivedFastTransfer.ValidateData(2);
                result.Errors.AddRange(Validation.ValidateCustomerSignature(this.ReceivedFastTransfer.CustomerNumber));
            }
            errors = TransferByCallDB.CheckForChange(this, Convert.ToUInt16(isCallCenter));

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
                Localization.SetCulture(result, new Culture(Languages.hy));

                return result;
            }



            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TransferByCallDB.ChangeSave(this, user.userName, source, Convert.ToUInt16(isCallCenter));

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

                result = base.Approve(schemaType, user.userName);

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
        protected new void Complete()
        {
            this.CustomerNumber = this.ReceivedFastTransfer.CustomerNumber;
            if (this.ReceivedFastTransfer.ReceiverAccount == null && this.SubType == 5)
            {
                this.ReceivedFastTransfer.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), this.ReceivedFastTransfer.Currency, user.filialCode);
            }
            this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            if (this.Source == SourceType.MobileBanking || (this.Source == SourceType.Bank && this.OPPerson == null))
            {
                this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            }
            this.ReceivedFastTransfer.Fee = Utility.RoundAmount(this.ReceivedFastTransfer.Fee, this.ReceivedFastTransfer.Currency);
            this.ReceivedFastTransfer.FeeAcba = Utility.RoundAmount(this.ReceivedFastTransfer.FeeAcba, this.ReceivedFastTransfer.Currency);
            if (this.SubType == 1 || this.SubType == 5)
            {
                this.Description = this.SubType == 5 ? "Հեռախոսազանգով փոխանցման հաստատում" : "Փոխանցման խմբագրում";
            }
        }

        public static Dictionary<string, string> GetCallTransferRejectionReasons()
        {

            Dictionary<string, string> reasons = TransferByCallDB.GetCallTransferRejectionReasons();
            return reasons;
        }


    }

}
