using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class CardlessCashoutCancellationOrder : Order
    {
        /// <summary>
        /// Բանկոմատին փոխանցման հայտի չեղարկման ունիկալ համար
        /// </summary>
        public long CancellationDocId { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public double TransferFee { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public double OldAmount { get; set; }

        /// <summary>
        /// Փոխանցման միջնորդավճար
        /// </summary>
        public double OldTransferFee { get; set; }

        /// <summary>
        /// Credit հաշիվ
        /// </summary>
        public Account CreditAccount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CardlessCashoutCode { set; get; }

        /// <summary>
        /// Բանկոմատին փոխանցման չեղարկման հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove()
        {
            this.Complete();
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = CardlessCashoutCancellationOrderDB.Save(this);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                scope.Complete();
            }



            if (result.ResultCode != ResultCode.Normal)
            {
                return result;
            }

            result = base.Approve(3, this.user.userName);

            if (result.ResultCode == ResultCode.Normal)
            {
                warnings.AddRange(base.GetActionResultWarnings(result));

                this.Quality = OrderQuality.Sent3;
                base.SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                base.SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);


                LogOrderChange(user, Action.Update);
                result.Id = this.Id;
            }
            else
            {
                result.ResultCode = ResultCode.Failed;
                return result;
            }


            return result;
        }

        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.Type = OrderType.CardlessCashoutCancellationOrder;
            this.Quality = OrderQuality.Draft;

            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = "0";

            if (this.Source != SourceType.Bank)
            {
                GetCardlessCashOutOrderFromCodeOrDocId();
                this.Description = "ArCa-ից ստացված հետվճար";
            }
            else
            {
                this.Description = "Հետվերադարձ` " + this.Description;
            }

            if (!string.IsNullOrEmpty(this.CardlessCashoutCode))
            {
                this.Source = SourceType.AcbaOnline;
                this.user = new ACBAServiceReference.User() { userID = 88, userName = "Auto" };
                this.OperationDate = Utility.GetNextOperDay();
            }
            if (this.Currency == "AMD")
                this.SubType = 1;
            else
                this.SubType = 2;
        }

        private void GetCardlessCashOutOrderFromCodeOrDocId() =>
            CardlessCashoutCancellationOrderDB.GetCardlessCashOutOrderFromCodeOrDocId(this);


        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            List<ActionError> validationList = new List<ActionError>();

            if (Amount > OldAmount)
            {
                //Տվյալ ԱՄՏԾ-ով գործող պարտատոմսի թողարկում գոյություն չունի։
                validationList.Add(new ActionError(1445));
                return result;
            }

            if (TransferFee > OldTransferFee)
            {
                //Տվյալ ԱՄՏԾ-ով գործող պարտատոմսի թողարկում գոյություն չունի։
                validationList.Add(new ActionError(1445));
                return result;
            }

            result.Errors.AddRange(validationList);

            return result;
        }


    }
}
