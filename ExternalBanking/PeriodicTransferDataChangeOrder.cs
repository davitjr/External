using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalBanking.DBManager;
using ExternalBanking.ACBAServiceReference;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    /// <summary>
    /// Պարբերական փոխանցման փոփոխման հայտ
    /// </summary>
    public class PeriodicTransferDataChangeOrder : Order
    {
        /// <summary>
        /// Պարբերական փոխանցման ունիկալ համարը 
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման տեսակ 
        /// </summary>
        public int PeriodicType { get; set; }

        /// <summary>
        ///  Պարբերական փոխանցման գումարի գանձման եղանակ`
        ///  անբողջ պարտք
        ///  անբողջ մնացորդ
        ///  ֆիքսված գումար
        /// </summary>
        public int ChargeType { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման հնարավոր մաքսիմալ գումարը 
        /// (նշվում է հաճախորդի կողմից,ամբողջ մնացորդով փոխանցում ընտրելու դեպքում )
        /// եթե պարտքը մեծ է նշված գումարից, փոխանցումը չի կատարվում 
        /// </summary>
        public double MaxAmountLevel { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման հնարավոր մինիմալ գումարը 
        /// (նշվում է հաճախորդի կողմից կամ մատակարարի պահանջը, ամբողջ մնացորդով փոխանցում ընտրելու դեպքում )
        /// եթե պարտքը փոքր է նշված գումարից, փոխանցումը չի կատարվում 
        /// </summary>
        public double MinAmountLevel { get; set; }

        /// <summary>
        /// Կոմունալ վճարման դեպքում մասնակի վճարման հնարավորություն
        /// </summary>
        public byte PartialPayment { get; set; }

        /// <summary>
        /// Պարբերական փոխանցում կատարելուց հետո դեբետագրվող հաշվի պարտադիր մինիմալ մնացորդը,
        /// որը պետք է մնա հաշվի վրա (նշվում է հաճախորդի կողմից), 
        /// եթե մնացորդը հնարաոր չէ ապահովել, փոխանցումը չի կատարվում 
        /// </summary>
        public double MinDebetAccountRest { get; set; }

        /// <summary>
        /// Կոմունալ վճարման դեպքում վճարման տեսակը
        /// </summary>
        public byte PayIfNoDebt { get; set; }

        /// <summary>
        /// Հաշվի վրա գումարը չլինելու պատճառով պարբերական փոխանցման կատարման համար
        /// հաճախորդի կողմից նախատեսված օրերի քանակը 
        /// </summary>
        public ushort CheckDaysCount { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման պարբերականությունը` քանի օր մեկ է կատարվում
        /// </summary>
        public int Periodicity { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման պլանային առաջին փոխանցման օր
        /// </summary>
        public DateTime FirstTransferDate { get; set; }

        /// <summary>
        /// Պարբերական փոխանցման վեջին կատարման ամսաթիվը
        /// </summary>
        public DateTime? LastOperationDate { get; set; }

        private void Complete()
        {
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
            {
                PeriodicTransfer editablePeriodic = PeriodicTransferDB.GetPeriodicTransfer(this.ProductId, this.CustomerNumber);
                this.FirstTransferDate = editablePeriodic.FirstTransferDate;
                this.Type = OrderType.PeriodicTransferDataChangeOrder;
                this.SubType = 1;
                this.Currency = editablePeriodic.Currency;
                this.RegistrationDate = DateTime.Now.Date;
                if(editablePeriodic.Type >= 3 && editablePeriodic.Type <= 12)
                {
                    this.Description = editablePeriodic.Description;
                    this.Periodicity = editablePeriodic.Periodicity;
                }
                if (editablePeriodic.Type >= 3 && editablePeriodic.Type <= 17 )
                {
                    this.PayIfNoDebt = 1;
                }
            }
        }
        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidatePeriodicDataChangeOrder(this));
            return result;
        }
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
                result = PeriodicTransferDataChangeOrderDB.SavePeriodicTransferDataChangeOrder(this);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                SaveOrderProductId(this.ProductId, this.Id);

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
            result = base.Confirm(user);

            return result;
        }
        /// <summary>
        /// Պարբերական փոխանցման հայտի տվյալներ
        /// </summary>
        public void Get()
        {
            PeriodicTransferDataChangeOrderDB.GetPeriodicDataChangeOrder(this);
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
                result = PeriodicTransferDataChangeOrderDB.SavePeriodicTransferDataChangeOrder(this);
                //**********                
                ulong orderId = base.Save(this, source, user);
                Order.SaveLinkHBDocumentOrder(this.Id, orderId);
                ActionResult res = BOOrderCustomer.Save(this, orderId, user);
                //**********
                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    base.SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                if (action == Action.Add)
                  SaveOrderProductId(this.ProductId, this.Id);

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                
                LogOrderChange(user, action);
                scope.Complete();

            }

            return result;
        }

        public ActionResult Approve(User user, SourceType source, string userName, short schemaType)
        {
            ActionResult result = new ActionResult();

            Action action = this.Id == 0 ? Action.Add : Action.Update;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
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

            return result;
        }
    }
}



