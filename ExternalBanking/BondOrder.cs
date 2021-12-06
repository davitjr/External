using ExternalBanking.ACBAServiceReference;
using ExternalBanking.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ExternalBanking
{
    public class BondOrder : Order
    {
        /// <summary>
        /// Վաճառված պարտատոմս
        /// </summary>
        public Bond Bond { get; set; }

        /// <summary>
        /// Հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult SaveAndApprove(string userName, SourceType source, ACBAServiceReference.User user, short schemaType)
        {
            ActionResult result = CheckBondIssueReplacementDate(this.Bond.BondIssueId);
            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Complete();

            if ((GetCustomerTypeAndResidence(CustomerNumber).Item1 == false
                && Bond.CustomerDepositaryAccount.AccountNumber == 0 && Bond.DepositaryAccountExistenceType == DepositaryAccountExistence.Exists) == false)
            {
                if (Bond.DepositaryAccountExistenceType != DepositaryAccountExistence.ExistsInOtherOperator)
                    SetDepositaryInfo();
            }

            result = Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Action.Add;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = BondOrderDB.SaveBondOrder(this, userName);

                result = SaveOrderOPPerson();
                SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                LogOrderChange(user, action);

                result = Approve(schemaType, userName);

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

        /// <summary>
        /// Հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="source"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
        public ActionResult Save(string userName)
        {
            Complete();

            ActionResult result = Validate();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Action.Add;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                result = BondOrderDB.SaveBondOrder(this, userName);

                ActionResult resultOpPerson = SaveOrderOPPerson();

                if (resultOpPerson.Errors.Count > 0)
                {
                    resultOpPerson.ResultCode = ResultCode.Failed;
                    return resultOpPerson;
                }

                SaveOrderAttachments();

                if (result.ResultCode != ResultCode.Normal)
                {
                    return result;
                }
                else
                {
                    SetQualityHistoryUserId(OrderQuality.Draft, user.userID);
                }

                LogOrderChange(user, action);

                scope.Complete();
            }

            return result;
        }

        /// <summary>
        /// Հայտի պահպանում և ուղարկում
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="user"></param>
        /// <param name="schemaType"></param>
        /// <returns></returns>
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

        private void Complete()
        {
            RegistrationDate = DateTime.Now.Date;
            if ((OrderNumber == null || OrderNumber == "") && Id == 0)
            {
                OrderNumber = GenerateNextOrderNumber(CustomerNumber);
            }

            BondIssueFilter bondIssueFilter = new BondIssueFilter
            {
                BondIssueId = Bond.BondIssueId
            };
            List<BondIssue> bondIssues = BondIssueFilter.SearchBondIssues(bondIssueFilter);

            if (bondIssues != null && bondIssues.Count > 0)
            {
                BondIssue bondIssue = bondIssues.First();
                Bond.BondIssueId = bondIssue.ID;
                Bond.InterestRate = bondIssue.InterestRate;
                Bond.Currency = bondIssue.Currency;
                Bond.ISIN = bondIssue.ISIN;

                if (Bond.ShareType == SharesTypes.Stocks)
                {
                    Bond.UnitPrice = (double)bondIssue.PlacementPrice;
                }
            }


            if (Bond.ShareType == SharesTypes.Bonds)
            {
                Bond.UnitPrice = Bond.GetBondPrice(Bond.BondIssueId);


                if (Bond.DepositaryAccountExistenceType == DepositaryAccountExistence.NonExisted)
                {
                    if (Bond.CustomerDepositaryAccount != null)
                    {
                        Bond.CustomerDepositaryAccount = null;
                    }
                }
                else
                {

                    if (DepositaryAccount.HasCustomerDepositaryAccount(CustomerNumber))
                    {
                        bool hasAccount = false;
                        Bond.CustomerDepositaryAccount = DepositaryAccount.GetCustomerDepositaryAccount(CustomerNumber, ref hasAccount);
                        Bond.DepositaryAccountExistenceType = DepositaryAccountExistence.ExistsInBank;
                    }
                    else
                    {
                        if (Bond.DepositaryAccountExistenceType == DepositaryAccountExistence.Exists)
                        {
                            if (Bond.CustomerDepositaryAccount != null && Bond.CustomerDepositaryAccount.BankCode != 0)
                            {
                                Bond.CustomerDepositaryAccount.Description = Info.GetBank(Bond.CustomerDepositaryAccount.BankCode, Languages.hy);
                            }
                        }
                        else
                        {
                            Bond.CustomerDepositaryAccount = null;
                        }
                    }
                }
            }

            Bond.TotalPrice = Math.Round(Bond.BondCount * Bond.UnitPrice, 4);

            Amount = Bond.TotalPrice;
            OPPerson = SetOrderOPPerson(CustomerNumber);

        }

        public ActionResult Validate()
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateBondOrder(this));
            return result;
        }

        public void Get()
        {
            BondOrderDB.GetBondOrder(this);
        }

        public static int GetBondOrderIssueSeria(int bondIssueId)
        {
            return BondOrderDB.GetBondOrderIssueSeria(bondIssueId);
        }

        public void SetDepositaryInfo()
        {

            DepositaryAccount account = DepositaryAccountDB.GetDepositaryAccountForStock(this.CustomerNumber);

            this.Bond.CustomerDepositaryAccount = account;

        }

        public static ActionResult ConfirmStockOrder(int bondId, int userID)
        {
            return BondOrderDB.ConfirmStockOrder(bondId, userID);
        }


        public static Tuple<bool, bool> GetCustomerTypeAndResidence(ulong customerNumber)
        {
            return BondOrderDB.GetCustomerTypeAndResidence(customerNumber);
        }

        public ActionResult CheckBondIssueReplacementDate(int bondIssueId)
        {
            ActionResult result = new ActionResult();
            result.Errors.AddRange(Validation.ValidateBondIssueReplacementDate(bondIssueId));
            return result;
        }
    }
}
