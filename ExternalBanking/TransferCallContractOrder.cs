using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ExternalBanking
{
    public class TransferCallContractOrder : Order
    {

        /// <summary>
        /// Համաձայնագրի տվյալներ
        /// </summary>
        public TransferCallContractDetails TransferCallContractDetails { get; set; }


        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user)
        {
            this.Complete(source);
            ActionResult result = this.Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TransferCallContractOrderDB.Save(this, userName, source);
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            result.Errors.AddRange(Validation.ValidateTransferCallContractOrder(this));
            //
            return result;
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public void Get()
        {
            TransferCallContractOrderDB.Get(this);
        }

        /// <summary>
        /// Վերադարձնում է ավանդի գրավով վարկի հայտի տվյալները
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>

        /// <summary>
        /// Հայտի հաստատաման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            DateTime nextOperDay = Utility.GetNextOperDay().Date;
            if (RegistrationDate.AddDays(30).Date < DateTime.Now.Date || this.RegistrationDate.Date > DateTime.Now.Date)
            {
                //Փաստաթղթի ամսաթիվը տարբերվում է այսօրվա ամսաթվից 30-ից ավելի օրով
                result.Errors.Add(new ActionError(451));
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
        /// <summary>
        /// Լրացնում է հայտի ավտոմատ լրացվող դաշտերը
        /// </summary>
        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);



        }

        /// <summary>
        ///Հեռախոսազանգով փոխանցման համաձայնագրի հայտի պահպանում և ուղղարկում
        /// </summary>
        /// <param name="userName">Օգտագործողի անուն (Հաճախորդ)</param>
        /// <param name="source">Տվյալների աղբյուր(HB, Հայկական Ծրագրեր, Մոբայլ Բանկ)</param>
        /// <param name="user">Օգտագործող</param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            this.Complete(source);
            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            result = this.ValidateForSend();
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = TransferCallContractOrderDB.Save(this, userName, source);

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

                if (this.Fees != null)
                {
                    result = base.SaveOrderFee();

                    if (result.ResultCode != ResultCode.Normal)
                    {
                        return result;
                    }
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
            }

            result = base.Confirm(user);

            return result;
        }

        public bool IsSecondActivation()
        {
            return TransferCallContractOrderDB.IsSecondActivation(this);
        }

        public bool IsSameAccountContract()
        {
            return TransferCallContractOrderDB.IsSameAccountContract(this);
        }
        public bool IsSameNumberContract()
        {
            return TransferCallContractOrderDB.IsSameNumberContract(this);
        }


    }
}
