using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalBanking.ACBAServiceReference;
using System.Transactions;
using ExternalBanking.DBManager;

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

            this.Complete();

            ActionResult result = this.Validate();
            List<ActionError> warnings = new List<ActionError>();

            if (result.Errors.Count > 0)
            {
                result.ResultCode = ResultCode.ValidationError;
                return result;
            }

            Action action = Action.Add;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                result = BondOrderDB.SaveBondOrder(this, userName);
               
                result = base.SaveOrderOPPerson();
                this.SaveOrderAttachments();

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
            this.RegistrationDate = DateTime.Now.Date;
            if ((this.OrderNumber == null || this.OrderNumber == "") && this.Id == 0)
                this.OrderNumber = Order.GenerateNextOrderNumber(this.CustomerNumber);

            BondIssueFilter bondIssueFilter = new BondIssueFilter();
            bondIssueFilter.BondIssueId = this.Bond.BondIssueId;
            List<BondIssue> bondIssues = BondIssueFilter.SearchBondIssues(bondIssueFilter);
           

            if(bondIssues != null && bondIssues.Count > 0)
            {
                BondIssue bondIssue = bondIssues.First();
                this.Bond.BondIssueId = bondIssue.ID;
                this.Bond.InterestRate = bondIssue.InterestRate;
                this.Bond.Currency = bondIssue.Currency;
                this.Bond.ISIN = bondIssue.ISIN;
            }

 

            this.Bond.UnitPrice = Bond.GetBondPrice(this.Bond.BondIssueId);
            this.Bond.TotalPrice = Math.Round(this.Bond.BondCount * this.Bond.UnitPrice, 4);

            this.Amount = this.Bond.TotalPrice;
            this.OPPerson = Order.SetOrderOPPerson(this.CustomerNumber);

            if(this.Bond.DepositaryAccountExistenceType  == DepositaryAccountExistence.NonExisted)
            {
                if (this.Bond.CustomerDepositaryAccount != null)
                {
                    this.Bond.CustomerDepositaryAccount = null;
                }
            }
            else
            {
                
                if (DepositaryAccount.HasCustomerDepositaryAccount(this.CustomerNumber))
                {
                    bool hasAccount = false;
                    this.Bond.CustomerDepositaryAccount = DepositaryAccount.GetCustomerDepositaryAccount(this.CustomerNumber, ref hasAccount);
                    this.Bond.DepositaryAccountExistenceType = DepositaryAccountExistence.ExistsInBank;
                }
                else
                {
                    if(this.Bond.DepositaryAccountExistenceType == DepositaryAccountExistence.Exists)
                    {
                        if (this.Bond.CustomerDepositaryAccount != null && this.Bond.CustomerDepositaryAccount.BankCode != 0)
                        {
                            this.Bond.CustomerDepositaryAccount.Description = Info.GetBank(this.Bond.CustomerDepositaryAccount.BankCode, Languages.hy);
                        }
                    }
                    else
                    {
                        this.Bond.CustomerDepositaryAccount = null;
                    }
                    
                }
            }
           
               
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

       
    }
}
