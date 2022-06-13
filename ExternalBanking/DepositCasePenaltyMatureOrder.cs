using ExternalBanking.DBManager;
using System;
using System.Transactions;


namespace ExternalBanking
{
    /// <summary>
    /// Պահատուփի տուժանքի մարման հայտ
    /// </summary>
    public class DepositCasePenaltyMatureOrder : Order
    {
        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Ելքագրվող (դեբետ) հաշիվ
        /// </summary>
       // public override Account DebitAccount { get; set; }

        /// <summary>
        /// Մուտքագրվող (կրեդիտ) հաշվի
        /// </summary>
        public Account ReceiverAccount { get; set; }


        private void Complete()
        {
            this.RegistrationDate = DateTime.Now.Date;
            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                this.FilialCode = (ushort)DepositCase.GetDepositCase(this.ProductId, this.CustomerNumber).FilialCode;
                this.Description = "Անհատական պահատուփերի գծով մարում";


            }
            
            //Հայտի համար
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            DepositCase depositCase = DepositCase.GetDepositCase(this.ProductId, this.CustomerNumber);
            this.ReceiverAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.CreditAccount), "AMD", (ushort)depositCase.FilialCode, 0, "0", "", this.CustomerNumber);

            if (this.Type == OrderType.CashDepositCasePenaltyMatureOrder)
            {
                this.DebitAccount = Account.GetOperationSystemAccount(Utility.GetOperationSystemAccountType(this, OrderAccountType.DebitAccount), "AMD", user.filialCode);
            }

        }


        /// <summary>
        /// Հայտի պահպանման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateDepositCasePenaltyMatureOrder(this, this.user));
            return result;
        }


        /// <summary>
        /// Հայտի ուղարկման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {

            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.SetAmountsForCheckBalance(this));

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
        /// Վերադարձնում է հայտի տվյալները
        /// </summary>
        public void Get()
        {

            Fees = GetOrderFees(Id);

            if (Fees.Exists(m => m.Type == 7))
            {
               
                Currency = this.Fees.Find(m => m.Type == 7).Currency;
                Fees.Find(m => m.Type == 7).Account = this.DebitAccount;
            }
            DepositCasePenaltyMatureOrderControllerDB.GetDepositCasePenaltyMatureOrder(this);
        }


        /// <summary>
        /// Մարման հայտի պահպանում և ուղարկում
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

            if (this.Type == OrderType.DepositCasePenaltyMatureOrder)
            {
                result = this.ValidateForSend();
                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

            Action action = this.Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = DepositCasePenaltyMatureOrderControllerDB.SaveDepositCasePenaltyMatureOrder(this, userName);

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


        public ActionResult Save(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
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
                result = DepositCasePenaltyMatureOrderControllerDB.SaveDepositCasePenaltyMatureOrder(this, userName);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    
                        ActionResult resultOrderFee = SaveOrderFee();
                 
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);

                }

                result = base.SaveOrderOPPerson();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);
             scope.Complete();

             }
            result.Id = this.Id;
            return result;
        }


        public ActionResult Approve(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {

            ActionResult result = new ActionResult();

            if (this.Type == OrderType.DepositCasePenaltyMatureOrder)
            {
                result = this.ValidateForSend();
                if (result.Errors.Count > 0)
                {
                    result.ResultCode = ResultCode.ValidationError;
                    return result;
                }
            }

              result = base.Approve(schemaType, userName);
              result.Id = this.Id;

            return result;
        }

    }
}
