using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public class ConsumeLoanSettlementOrder : Order
    {
        /// <summary>
        /// Վարկային պրոդուկտի տեսակ
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// Վարկային պրոդուկտի տեսակի նկարագրություն
        /// </summary>
        public string ProductTypeDescription { get; set; }

        /// <summary>
        /// Պրոդուկտի ունիկալ համար
        /// </summary>
        public ulong ProductId { get; set; }

        /// <summary>
        /// Վարկի սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վարկի վերջ
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Վարկի տոկոսադրույք
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Ամսական մարման չափ
        /// </summary>
        public double MonthlyRepaymentAmount { get; set; }

        /// <summary>
        /// Սպառողական վարկին կցված ընթացիկ հաշիվ
        /// </summary>
        public Account CurrentAccount { get; set; }

        /// <summary>
        /// Առաջին վճարման օր
        /// </summary>
        public DateTime FirstRepaymentDate { get; set; }

        /// <summary>
        /// Տևողություն
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// true` եթե հաճախորդը նշել է checkbox-ը, false հակառակ դեպքում
        /// </summary>
        public bool AcknowledgedByCheckBox { get; set; }

        /// <summary>
        /// Եթե checkbox-ը նշված է ապա checkbox-ի դիմաց ցուցադրված տեքստը, հակառակ դեպքում՝ հաճախորդի կողմից մուտքագրված տեքստը 
        /// </summary>
        public string AcknowledgementText { get; set; }

        /// <summary>
        /// Վերլուծության մերժման հիմքեր
        /// </summary>
        public byte RefuseReasonType { get; set; }

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

                result = ConsumeLoanApplicationOrderDB.SaveConsumeLoanSettlementOrder(this, userName, source);

                //**********
                LogOrderChange(user, action);
                scope.Complete();
            }

            return result;
        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();

            (long, DateTime) sentOrder = ConsumeLoanApplicationOrderDB.ExistsConsumeLoanSettlementOrder(this.CustomerNumber, new List<OrderQuality> { OrderQuality.Sent3, OrderQuality.Draft, OrderQuality.Completed, OrderQuality.SBQprocessed }, this.Id, this.ProductId);
            if (sentOrder.Item1 > 0)
            {
                //{0} թ.-ին Դուք մուտքագրել եք վարկի ձևակերպման հայտ։ Խնդրում ենք շարունակել այն։
                result.Errors.Add(new ActionError(2055, new string[] { sentOrder.Item2.ToString("dd/MM/yy") }));
            }

            if ((this.GroupId != 0) ? !OrderGroup.CheckGroupId(this.GroupId) : false)
            {
                //Նշված խումբը գոյություն չունի։
                result.Errors.Add(new ActionError(1628));
            }


            return result;
        }

        private void Complete(SourceType source)
        {
            this.RegistrationDate = DateTime.Now.Date;
            this.SubType = 1;
            this.Type = OrderType.ConsumeLoanSettlementOrder;
            //Հայտի համար   
            if (string.IsNullOrEmpty(this.OrderNumber) && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            this.ProductType = 5;

            this.StartDate = Utility.GetNextOperDay();
            this.EndDate = StartDate.AddMonths(Duration);

            bool isNotWorkingDay = true;
            while (isNotWorkingDay)
            {
                if (Utility.IsWorkingDay(this.EndDate))
                {
                    isNotWorkingDay = false;
                    break;
                }
                else
                {
                    this.EndDate = this.EndDate.AddDays(1);
                }
            }


            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);
            this.CurrentAccount = new Account();
            this.CurrentAccount.AccountNumber = Account.GetAllCurrentAccounts(this.CustomerNumber)
                .Where(item => item.Currency == "AMD")
                .OrderBy(x => x.OpenDate)
                .ToList()[0].AccountNumber;


        }

        public void GetConsumeLoanSettlementOrder()
        {
            ConsumeLoanApplicationOrderDB.GetConsumeLoanSettlementOrder(this);
            List<OrderHistory> orderHistory = Order.GetOrderHistory(this.Id);
            this.SentDate = orderHistory.Where(m => m.Quality == OrderQuality.Sent3)?.FirstOrDefault()?.ChangeDate;
            this.CancellationDate = orderHistory.Where(m => m.Quality == OrderQuality.Canceled)?.FirstOrDefault()?.ChangeDate;
        }

        public static byte[] PrintConsumeLoanSettlement(long docId, ulong customerNumber, bool fromApprove = false) => ConsumeLoanApplicationOrderDB.PrintConsumeLoanSettlement(docId, customerNumber, fromApprove);

        public ActionResult Approve(string userName, short schemaType, ACBAServiceReference.User user)
        {

            ActionResult result = ValidateForSend();

            if (result.ResultCode == ResultCode.Normal)
            {
                if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                    UpdateConsumeLoanSettlementScheduleContractDate(this.Id);

                PrintConsumeLoanSettlement(this.Id, this.CustomerNumber, true);

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    if (this.Source == SourceType.AcbaOnline || this.Source == SourceType.MobileBanking)
                        SaveLoanAcknowledgementText(this.AcknowledgedByCheckBox, this.AcknowledgementText, this.Id);

                    result = base.Approve(schemaType, userName);

                    if (result.ResultCode == ResultCode.Normal)
                    {
                        Quality = OrderQuality.Sent3;
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
            }



            return result;
        }

        public static Dictionary<string, string> GetConsumeLoanSettlementSchedult(long docId) => ConsumeLoanApplicationOrderDB.GetConsumeLoanSettlementSchedult(docId);

        public static void SaveLoanAcknowledgementText(bool acknowledgedByCheckBox, string acknowledgementText, long id) => ConsumeLoanApplicationOrderDB.SaveLoanAcknowledgementText(acknowledgedByCheckBox, acknowledgementText, id);


        public static void UpdateConsumeLoanSettlementScheduleContractDate(long orderId) => ConsumeLoanApplicationOrderDB.UpdateConsumeLoanSettlementScheduleContractDate(orderId);

        /// <summary>
        /// Հայտի ուղարկման ստուգումներ
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateForSend()
        {
            ActionResult result = new ActionResult();

            if (ConsumeLoanApplicationOrder.ExistsRefusedConsumeLoanApplication(this.ProductId))
            {
                result.ResultCode = ResultCode.ValidationError;
                //Դուք հրաժարվել եք վարկից, հայտի կատարումը հնարավոր չէ։
                result.Errors.Add(new ActionError(2056));
            }


            if (result.Errors.Count == 0)
            {
                result.ResultCode = ResultCode.Normal;
            }

            return result;
        }

    }
}

