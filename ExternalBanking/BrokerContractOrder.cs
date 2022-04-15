using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace ExternalBanking
{
    public class BrokerContractOrder : Order
    {
        public string ContractId { get; set; }
        public int EducationId { get; set; }
        public string Profession { get; set; }
        public int ActivitySphereId { get; set; }
        public int StockKnowledgeId { get; set; }
        public int RiskLeaningId { get; set; }
        public int FinancialExperienceId { get; set; }
        public int? FinancialExperienceDuration { get; set; }
        public string FinancialExperienceDurationType { get; set; }     
        public List<int> StockToolIds { get; set; }
        public int? StockToolsExpDurationId { get; set; }
        public int LastYearStockOrderCount { get; set; }
        public decimal OneOrderAvg { get; set; }
        public int StockPortfolio { get; set; }
        public int FinancialSituationId { get; set; }
        public decimal LastYearProfit { get; set; }
        public int InvestmentPurposeId { get; set; }
        public int? BookValueOfPreviousYearOfAssetId { get; set; }
        public int? LastYearSalesTurnoverId { get; set; }
        public int? LastYearCapitalId { get; set; }

        private void Complete()
        {
            //Հայտի համար   
            if (string.IsNullOrEmpty(OrderNumber) && Id == 0)
            {
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            }

            OPPerson = SetOrderOPPerson(CustomerNumber);
        }

        public ActionResult SaveAndApprove(short schemaType, User user)
        {
            Complete();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                //Հայտի ձևակերպում
                ActionResult result = BrokerContractOrderDB.SaveBrokerContractOrder(this, user.userName);

                ActionResult resultOpPerson = SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                Action action = Id == 0 ? Action.Add : Action.Update;

                LogOrderChange(user, action);

                result = Approve(schemaType, user.userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
                    LogOrderChange(user, Action.Update);
                }
                else
                {
                    return result;
                }

                scope.Complete();
            }

            ActionResult resultConfirm = base.Confirm(user);
            return resultConfirm;
        }

        public ActionResult Save(User user, string userName)
        {
            Complete();
            ActionResult result = new ActionResult();

            Action action = Id == 0 ? Action.Add : Action.Update;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                //Հայտի ձևակերպում
                result = BrokerContractOrderDB.SaveBrokerContractOrder(this, userName);

                ActionResult resultOpPerson = SaveOrderOPPerson();
                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }

                LogOrderChange(user, action);

                scope.Complete();
            }

            return result;
        }

        public ActionResult Approve(string userName, User user, short schemaType)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                ActionResult result = Approve(schemaType, userName);

                if (result.ResultCode == ResultCode.Normal)
                {
                    Quality = OrderQuality.Sent3;
                    SetQualityHistoryUserId(OrderQuality.Sent, user.userID);
                    SetQualityHistoryUserId(OrderQuality.Sent3, user.userID);
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

        public void GetBrokerContractOrder()
        {
            BrokerContractOrderDB.GetBrokerContractOrder(this);
        }

        public static async Task<BrokerContractSurvey> GetBrokerContractSurvey(Languages language)
        {
            return await BrokerContractOrderDB.GetBrokerContractSurvey(language);
        }

        public static async Task<bool> HasBrokerContract(ulong customerNumber)
        {
            return await BrokerContractOrderDB.HasBrokerContract(customerNumber);
        }

        /// <summary>
        /// Վերադարձնում է հաճախորդի բրոքերային պայմանագրի համարը
        /// </summary>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        public static async Task<string> GetBrokerContractId(ulong customerNumber) => await BrokerContractOrderDB.GetBrokerContractId(customerNumber);

    }


    public class BrokerContract
    {
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }

        public static async Task<BrokerContract> GetBrokerContractProduct(ulong customerNumber)
        {
            return await BrokerContractOrderDB.GetBrokerContractProduct(customerNumber);
        }

    }



}
